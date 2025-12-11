using App.ViewModels;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

namespace App.Contracts.Services;

/// <summary>
/// Defines a contract for a service that manages browser-style, per-tab page navigation
/// within a tabbed, multi-window application.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Occurs when navigation within any tab's frame has successfully completed.
    /// </summary>
    event NavigatedEventHandler Navigated;

    /// <summary>
    /// Gets a value indicating whether the frame of the currently selected tab can navigate backward.
    /// </summary>
    bool CanGoBack
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether the frame of the currently selected tab can navigate forward.
    /// </summary>
    bool CanGoForward
    {
        get;
    }

    /// <summary>
    /// Gets the collection of open tabs for the window associated with this service instance.
    /// The UI binds to this collection to display the tab strip.
    /// </summary>
    ObservableCollection<TabViewModel> Tabs
    {
        get;
    }

    /// <summary>
    /// Gets or sets the currently selected tab. The UI binds to this property to display the
    /// content of the active tab.
    /// </summary>
    TabViewModel? SelectedTab
    {
        get; set;
    }

    /// <summary>
    /// Navigates the frame of the currently selected tab to a new page. This action creates a
    /// history entry in the tab's back-stack, enabling the 'GoBack' command.
    /// Use this for "drill-down" or master-detail navigation within a single context.
    /// </summary>
    /// <param name="pageKey">The key of the page to navigate to (typically the ViewModel's FullName).</param>
    /// <param name="parameter">An optional parameter to pass to the target page's ViewModel.</param>
    /// <returns><c>true</c> if the navigation was initiated successfully; otherwise, <c>false</c>.</returns>
    bool NavigateInFrame(string pageKey, object? parameter = null);

    /// <summary>
    /// Navigates to a page by creating a new tab or focusing an existing singleton tab.
    /// If the page is a singleton and already open in any window, that window and tab will be activated.
    /// Otherwise, a new tab is created in the current window.
    /// </summary>
    /// <param name="pageKey">The key of the page to navigate to (typically the ViewModel's FullName).</param>
    /// <param name="parameter">An optional parameter to pass to the target page's ViewModel.</param>
    /// <returns><c>true</c> if the navigation was successful; otherwise, <c>false</c>.</returns>
    bool NavigateToTab(string pageKey, object? parameter = null);

    /// <summary>
    /// Navigates backwards in the history of the currently selected tab's frame.
    /// </summary>
    /// <returns><c>true</c> if the backward navigation was successful; otherwise, <c>false</c>.</returns>
    bool GoBack();

    /// <summary>
    /// Navigates forwards in the history of the currently selected tab's frame.
    /// </summary>
    /// <returns><c>true</c> if the forward navigation was successful; otherwise, <c>false</c>.</returns>
    bool GoForward();

    /// <summary>
    /// Closes the specified tab and removes it from the collection.
    /// </summary>
    /// <param name="tab">The tab view model to close.</param>
    void CloseTab(TabViewModel tab);
}