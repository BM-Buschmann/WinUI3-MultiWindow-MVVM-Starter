using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using App.Contracts.Services;
using App.Contracts.ViewModels;
using App.Helpers;
using App.ViewModels;
using App.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace App.Services;

/// <summary>
/// A service that manages browser-style navigation within a tabbed, multi-window application.
/// Each window's Shell gets its own instance of this service.
/// </summary>
public class NavigationService : ObservableObject, INavigationService
{
    private readonly IPageService _pageService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWindowService _windowService;
    private TabViewModel? _selectedTab;

    /// <summary>
    /// Occurs when navigation in any tab's frame has completed.
    /// </summary>
    public event NavigatedEventHandler? Navigated;

    /// <summary>
    /// Gets the collection of open tabs for the window associated with this service instance.
    /// </summary>
    public ObservableCollection<TabViewModel> Tabs { get; } = new();

    /// <summary>
    /// Gets or sets the currently selected tab.
    /// </summary>
    public TabViewModel? SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (SetProperty(ref _selectedTab, value))
            {
                // When the active tab changes, the state of the global Back/Forward buttons must update
                // to reflect the history of the newly selected tab's frame.
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(CanGoForward));
                Debug.WriteLine($"[NavigationService] Selected tab changed to: {value?.Header}");
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the currently selected tab's frame can navigate backward.
    /// </summary>
    public bool CanGoBack => SelectedTab?.NavigationFrame.CanGoBack ?? false;

    /// <summary>
    /// Gets a value indicating whether the currently selected tab's frame can navigate forward.
    /// </summary>
    public bool CanGoForward => SelectedTab?.NavigationFrame.CanGoForward ?? false;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationService"/> class.
    /// </summary>
    public NavigationService(IPageService pageService, IServiceProvider serviceProvider, IWindowService windowService)
    {
        _pageService = pageService;
        _serviceProvider = serviceProvider;
        _windowService = windowService;
        Debug.WriteLine("[NavigationService] Initialized.");
    }

    /// <summary>
    /// Navigates the frame of the currently selected tab to a new page, creating a back-stack.
    /// Use this for "drill-down" navigation.
    /// </summary>
    public bool NavigateInFrame(string pageKey, object? parameter = null)
    {
        if (SelectedTab == null)
        {
            Debug.WriteLine("[NavigationService] NavigateInFrame called, but no tab is selected.");
            return false;
        }

        var viewType = _pageService.GetPageType(pageKey);
        if (viewType == null)
        {
            Debug.WriteLine($"[NavigationService] Failed to find page with key: {pageKey}. In-frame navigation cancelled.");
            return false;
        }

        Debug.WriteLine($"[NavigationService] Navigating frame in tab '{SelectedTab.Header}' to page '{pageKey}'.");
        return SelectedTab.NavigationFrame.Navigate(viewType, parameter);
    }

    /// <summary>
    /// Creates a new tab or focuses an existing singleton tab, navigating it to the specified page.
    /// Use this for top-level navigation actions.
    /// </summary>
    public bool NavigateToTab(string pageKey, object? parameter = null)
    {
        var allowMultiple = _pageService.GetPageAllowsMultipleInstances(pageKey);
        if (!allowMultiple)
        {
            // Find an existing singleton tab across all windows.
            var existingTab = FindSingletonTab(pageKey, out var targetWindow);
            if (existingTab != null && targetWindow != null)
            {
                Debug.WriteLine($"[NavigationService] Singleton tab found. Activating window and selecting tab.");
                targetWindow.Activate();
                if (targetWindow.Content is ShellPage shellPage)
                {
                    shellPage.ViewModel.SelectedTab = existingTab;
                }
                return true;
            }
        }

        // If not a singleton, or singleton not found, create a new tab.
        return CreateNewTab(pageKey, parameter);
    }

    /// <summary>
    /// Navigates backwards in the active tab's frame history.
    /// </summary>
    public bool GoBack()
    {
        if (!CanGoBack) return false;
        Debug.WriteLine($"[NavigationService] Navigating back in frame for tab: {SelectedTab!.Header}.");
        SelectedTab!.NavigationFrame.GoBack();
        return true;
    }

    /// <summary>
    /// Navigates forwards in the active tab's frame history.
    /// </summary>
    public bool GoForward()
    {
        if (!CanGoForward) return false;
        Debug.WriteLine($"[NavigationService] Navigating forward in frame for tab: {SelectedTab!.Header}.");
        SelectedTab!.NavigationFrame.GoForward();
        return true;
    }

    /// <summary>
    /// Closes the specified tab.
    /// </summary>
    public void CloseTab(TabViewModel tab)
    {
        if (tab == null) return;

        Debug.WriteLine($"[NavigationService] Closing tab: {tab.Header}.");
        // Unsubscribe from the event handler to prevent memory leaks.
        tab.NavigationFrame.Navigated -= OnFrameNavigated;
        Tabs.Remove(tab);
        // As a sensible default, select the last tab in the list after closing one.
        SelectedTab = Tabs.LastOrDefault();
        Debug.WriteLine($"[NavigationService] Tab '{tab.Header}' removed.");
    }

    /// <summary>
    /// Helper method to create and configure a new tab.
    /// </summary>
    private bool CreateNewTab(string pageKey, object? parameter)
    {
        var viewType = _pageService.GetPageType(pageKey);
        if (viewType == null)
        {
            Debug.WriteLine($"[NavigationService] Failed to find page with key: {pageKey}. Tab creation cancelled.");
            return false;
        }

        var newTab = new TabViewModel
        {
            PageKey = pageKey,
            Header = _pageService.GetPageTitle(pageKey).GetLocalized()
        };

        Debug.WriteLine($"[NavigationService] Creating new tab '{newTab.Header}' for page key '{pageKey}'.");

        newTab.NavigationFrame.Navigated += OnFrameNavigated;
        var navigated = newTab.NavigationFrame.Navigate(viewType, parameter);

        if (!navigated)
        {
            Debug.WriteLine($"[NavigationService] Frame navigation failed for page key: {pageKey}. Cleaning up new tab.");
            newTab.NavigationFrame.Navigated -= OnFrameNavigated;
            return false;
        }

        Tabs.Add(newTab);
        SelectedTab = newTab;
        return true;
    }

    /// <summary>
    /// Helper method to find a singleton tab across all active windows.
    /// </summary>
    private TabViewModel? FindSingletonTab(string pageKey, out Window? targetWindow)
    {
        foreach (var window in _windowService.ActiveWindows)
        {
            if (window.Content is ShellPage shellPage && shellPage.ViewModel != null)
            {
                var tabInWindow = shellPage.ViewModel.Tabs.FirstOrDefault(t => t.PageKey == pageKey);
                if (tabInWindow != null)
                {
                    targetWindow = window;
                    return tabInWindow;
                }
            }
        }

        targetWindow = null;
        return null;
    }

    /// <summary>
    /// Handles the Navigated event for any frame within a tab. This is crucial for updating the
    /// shell's Back/Forward button states and passing parameters to ViewModels.
    /// </summary>
    private void OnFrameNavigated(object sender, NavigationEventArgs e)
    {
        // Update the shell's button states to reflect the new history of the navigated frame.
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoForward));

        // If the navigated page's ViewModel implements INavigationAware, pass the parameter.
        if (e.Content is Page page && page.DataContext is INavigationAware navigationAware)
        {
            navigationAware.OnNavigatedTo(e.Parameter);
            Debug.WriteLine($"[NavigationService] Passed navigation parameter to '{page.DataContext.GetType().Name}'.");
        }

        // Propagate the navigation event to any external subscribers.
        Navigated?.Invoke(this, e);
    }
}