// Views/ShellPage.xaml.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using App.Contracts.Services;
using App.Helpers;
using App.Services;
using App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.ApplicationModel.DataTransfer;

namespace App.Views;

/// <summary>
/// The host page for the application's main interface, managing the TabView,
/// title bar customization, and cross-window drag-and-drop logic.
/// </summary>
public sealed partial class ShellPage : Page
{
    private readonly IWindowService _windowService;
    private readonly IThemeSelectorService _themeSelectorService;

    private readonly string DraggedTabDataFormatId = "";

    public ShellViewModel ViewModel
    {
        get;
    }

    public ShellPage(ShellViewModel viewModel, IWindowService windowService, IThemeSelectorService themeSelectorService)
    {
        _windowService = windowService;
        _themeSelectorService = themeSelectorService;

        ViewModel = viewModel;

        InitializeComponent();

        Loaded += ShellPage_Loaded;

        // Using a unique ID for drag-drop prevents interference with other apps.
        DraggedTabDataFormatId = viewModel.Title.Replace(" ", "_");
        Debug.WriteLine($"[ShellPage] Initialized. Drag ID: {DraggedTabDataFormatId}");
    }

    private void AddTabMenuFlyout_Opening(object sender, object e)
    {
        if (sender is not MenuFlyout menuFlyout) return;

        menuFlyout.Items.Clear();
        foreach (var pageInfo in ViewModel.NavigatablePages)
        {
            var menuItem = new MenuFlyoutItem
            {
                Text = pageInfo.Value,
                Command = ViewModel.AddTabCommand,
                CommandParameter = pageInfo.Key
            };
            menuFlyout.Items.Add(menuItem);
        }
        Debug.WriteLine($"[ShellPage] Populated AddTab menu with {menuFlyout.Items.Count} items.");
    }

    private void ShellPage_Loaded(object sender, RoutedEventArgs e)
    {
        var currentWindow = _windowService.GetWindowForElement(this);
        if (currentWindow != null)
        {
            currentWindow.ExtendsContentIntoTitleBar = true;
            currentWindow.SetTitleBar(ShellTitleBar);

            TitleBarHelper.UpdateTitleBar(currentWindow, _themeSelectorService.Theme);

            Debug.WriteLine("[ShellPage] Custom title bar configured for the current window.");
        }
    }

    #region Event Handlers (UI Logic)

    private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        // Delegate to the ViewModel to handle the navigation logic.
        Debug.WriteLine("[ShellPage] Settings menu item clicked.");
        ViewModel.AddTabCommand.Execute(typeof(SettingsViewModel).FullName);
    }

    private void Tabs_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is TabViewModel tabViewModel)
        {
            ViewModel.CloseTabCommand.Execute(tabViewModel);
        }
    }

    #endregion

    #region Keyboard Accelerators

    private void CloseTab_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        Debug.WriteLine("[ShellPage] Keyboard Shortcut: Close Tab.");
        ViewModel.CloseSelectedTabCommand.Execute(null);
        args.Handled = true;
    }

    private void NextTab_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        Debug.WriteLine("[ShellPage] Keyboard Shortcut: Next Tab.");
        ViewModel.NavigateToNextTabCommand.Execute(null);
        args.Handled = true;
    }

    private void PreviousTab_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        Debug.WriteLine("[ShellPage] Keyboard Shortcut: Previous Tab.");
        ViewModel.NavigateToPreviousTabCommand.Execute(null);
        args.Handled = true;
    }

    #endregion

    #region Drag and Drop Logic (Boilerplate)

    private void Tabs_TabDragStarting(TabView sender, TabViewTabDragStartingEventArgs args)
    {
        if (args.Item is not TabViewModel draggedTabViewModel)
        {
            args.Cancel = true;
            return;
        }
        var dragId = TabDragHelper.StartDrag(draggedTabViewModel);
        args.Data.SetData(DraggedTabDataFormatId, dragId);
        args.Data.RequestedOperation = DataPackageOperation.Move;
    }

    private void Tabs_TabStripDragOver(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(DraggedTabDataFormatId)) e.AcceptedOperation = DataPackageOperation.Move;
    }

    private void TabStripArea_DragOver(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(DraggedTabDataFormatId)) e.AcceptedOperation = DataPackageOperation.Move;
    }

    private async void Tabs_TabStripDrop(object sender, DragEventArgs e) => await HandleDropAsync(e);
    private async void TabStripArea_Drop(object sender, DragEventArgs e) => await HandleDropAsync(e);

    private async Task HandleDropAsync(DragEventArgs e)
    {
        var deferral = e.GetDeferral();
        try
        {
            if (e.DataView.Contains(DraggedTabDataFormatId))
            {
                var dragId = await e.DataView.GetDataAsync(DraggedTabDataFormatId) as string;
                if (string.IsNullOrEmpty(dragId)) return;
                var draggedTab = TabDragHelper.GetTabById(dragId);
                if (draggedTab == null || ViewModel.Tabs.Contains(draggedTab)) return;

                ViewModel.Tabs.Add(draggedTab);
                TabDragHelper.MarkAsExternallyHandled();
                e.Handled = true;
            }
        }
        finally { deferral.Complete(); }
    }

    private void Tabs_TabDroppedOutside(TabView sender, TabViewTabDroppedOutsideEventArgs args)
    {
        if (args.Item is not TabViewModel droppedTabViewModel) return;

        var newWindow = _windowService.CreateWindowWithShell();
        var newShellPage = (ShellPage)newWindow.Content;

        // Directly add the tab to the new window's collection.
        newShellPage.ViewModel.Tabs.Add(droppedTabViewModel);
        newWindow.Activate();

        TabDragHelper.MarkAsExternallyHandled();

        if (args.Item is TabViewModel draggedTab)
        {
            ViewModel.CloseTabCommand.Execute(draggedTab);

        }

        TabDragHelper.EndDrag();

    }

    private void Tabs_TabDragCompleted(TabView sender, TabViewTabDragCompletedEventArgs args)
    {
        // If the drag was handled by another window (move succeeded), then remove the tab from this source window.
        if (args.DropResult == DataPackageOperation.Move && TabDragHelper.WasDragHandledExternally())
        {
            if (args.Item is TabViewModel draggedTab)
            {
                ViewModel.CloseTabCommand.Execute(draggedTab);

            }
        }
        TabDragHelper.EndDrag();
    }

    #endregion
}
