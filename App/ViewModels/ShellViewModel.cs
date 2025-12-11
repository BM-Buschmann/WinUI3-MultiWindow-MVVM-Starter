// ViewModels/ShellViewModel.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using App.Contracts.Services;
using App.Helpers;
using App.Services;
using App.ViewModels; // Corrected using
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.ApplicationModel;

namespace App.ViewModels;

/// <summary>
/// The ViewModel for the main application shell, which hosts the tabbed navigation structure.
/// Each application window will have its own instance of this ViewModel.
/// </summary>
public partial class ShellViewModel : ObservableObject
{
    private readonly IWindowService _windowService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private string _subtitle = "";

    /// <summary>
    /// Gets a dictionary of page keys and their localized titles, used for populating a "New Tab" menu.
    /// </summary>
    public Dictionary<string, string> NavigatablePages { get; } = new();

    /// <summary>
    /// Gets the collection of open tabs, exposed directly from the INavigationService.
    /// </summary>
    public ObservableCollection<TabViewModel> Tabs => _navigationService.Tabs;

    /// <summary>
    /// Gets or sets the currently selected tab, exposed directly from the INavigationService.
    /// </summary>
    public TabViewModel? SelectedTab
    {
        get => _navigationService.SelectedTab;
        set => _navigationService.SelectedTab = value;
    }

    // Pass-through properties for the navigation button states.
    public bool CanGoBack => _navigationService.CanGoBack;
    public bool CanGoForward => _navigationService.CanGoForward;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
    /// </summary>
    public ShellViewModel(IWindowService windowService, INavigationService navigationService, IPageService pageService)
    {
        _windowService = windowService;
        _navigationService = navigationService;

        Debug.WriteLine($"[ShellViewModel] Initialized for a new window instance.");

        Title = Package.Current.DisplayName;
        var version = Package.Current.Id.Version;
        Subtitle = $"Version {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

        // Listen for property changes on the NavigationService to update the UI.
        if (_navigationService is INotifyPropertyChanged notifier)
        {
            notifier.PropertyChanged += (s, e) =>
            {
                // This ensures the TabView's SelectedItem binding stays in sync.
                if (e.PropertyName == nameof(SelectedTab))
                {
                    OnPropertyChanged(nameof(SelectedTab));
                }
                // These are crucial for enabling/disabling the Back/Forward buttons.
                else if (e.PropertyName == nameof(CanGoBack))
                {
                    OnPropertyChanged(nameof(CanGoBack));
                    GoBackCommand.NotifyCanExecuteChanged();
                }
                else if (e.PropertyName == nameof(CanGoForward))
                {
                    OnPropertyChanged(nameof(CanGoForward));
                    GoForwardCommand.NotifyCanExecuteChanged();
                }
            };
            Debug.WriteLine("[ShellViewModel] PropertyChanged handler set up for INavigationService.");
        }

        var pageKeys = pageService.GetPageKeys();
        foreach (var key in pageKeys)
        {
            NavigatablePages.Add(key, pageService.GetPageTitle(key).GetLocalized());
        }
        Debug.WriteLine($"[ShellViewModel] Populated NavigatablePages with {NavigatablePages.Count} entries.");

        // If this ViewModel is created and has no tabs, it's a new window.
        // It is this ViewModel's responsibility to create the initial tab.
        if (_windowService.ActiveWindows.Count < 1 && Tabs.Count == 0)
        {
            Debug.WriteLine("[ShellViewModel] This is a new window with no tabs. Creating initial Home tab.");
            _navigationService.NavigateToTab(typeof(HomeViewModel).FullName!);
        }
    }

    /// <summary>
    /// Requests that a new tab be created for the specified page.
    /// </summary>
    [RelayCommand]
    private void AddTab(string? pageKey)
    {
        if (!string.IsNullOrEmpty(pageKey))
        {
            Debug.WriteLine($"[ShellViewModel] AddTab command executed for page key: '{pageKey}'.");
            _navigationService.NavigateToTab(pageKey);
        }
    }

    /// <summary>
    /// Closes the specified tab and requests window closure if it's the last tab.
    /// </summary>
    [RelayCommand]
    private void CloseTab(TabViewModel? tabToClose)
    {
        if (tabToClose != null)
        {
            Debug.WriteLine($"[ShellViewModel] CloseTab command executed for tab: '{tabToClose.Header}'.");
            _navigationService.CloseTab(tabToClose);

            if (Tabs.Count == 0)
            {
                Debug.WriteLine("[ShellViewModel] Last tab was closed. Requesting window closure from WindowService.");
                _windowService.RequestCloseWindow(this);
            }
        }
    }

    [RelayCommand]
    private void CloseSelectedTab() => CloseTab(SelectedTab);

    [RelayCommand]
    private void NavigateToNextTab()
    {
        if (Tabs.Count > 1 && SelectedTab != null)
        {
            var currentIndex = Tabs.IndexOf(SelectedTab);
            var nextIndex = (currentIndex + 1) % Tabs.Count;
            SelectedTab = Tabs[nextIndex];
        }
    }

    [RelayCommand]
    private void NavigateToPreviousTab()
    {
        if (Tabs.Count > 1 && SelectedTab != null)
        {
            var currentIndex = Tabs.IndexOf(SelectedTab);
            var previousIndex = (currentIndex - 1 + Tabs.Count) % Tabs.Count;
            SelectedTab = Tabs[previousIndex];
        }
    }

    /// <summary>
    /// Navigates backward in the currently selected tab's frame history.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack() => _navigationService.GoBack();

    /// <summary>
    /// Navigates forward in the currently selected tab's frame history.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoForward))]
    private void GoForward() => _navigationService.GoForward();
}