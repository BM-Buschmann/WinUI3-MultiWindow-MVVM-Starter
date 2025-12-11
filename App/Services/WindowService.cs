using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using App.Contracts.Services;
using App.Helpers;
using App.ViewModels;
using App.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace App.Services;

/// <summary>
/// Implements IWindowService to manage the creation, tracking, and theming of all application windows.
/// This service listens for theme changes from IThemeSelectorService and applies them globally.
/// </summary>
public class WindowService : IWindowService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly List<Window> _activeWindows;

    public IReadOnlyList<Window> ActiveWindows => _activeWindows.AsReadOnly();
    public WindowEx? MainWindow => _activeWindows.FirstOrDefault() as WindowEx;

    public WindowService(IServiceProvider serviceProvider, IThemeSelectorService themeSelectorService)
    {
        _serviceProvider = serviceProvider;
        _themeSelectorService = themeSelectorService;
        _activeWindows = new List<Window>();
        _themeSelectorService.ThemeChanged += OnThemeChanged;
    }


    /// <summary>
    // Event handler that applies the new theme to all active windows and their title bars.
    /// </summary>
    private void OnThemeChanged(ElementTheme theme)
    {
        foreach (var window in _activeWindows)
        {
            if (window.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = theme;
            }
            // Call the helper to update the title bar for this specific window.
            TitleBarHelper.UpdateTitleBar(window, theme);
        }
        Debug.WriteLine($"[WindowService] Heard ThemeChanged event. Applied theme '{theme}' to all windows.");
    }

    public WindowEx CreateWindowWithShell()
    {
        var window = new MainWindow();
        var shellPage = _serviceProvider.GetRequiredService<ShellPage>();
        window.Content = shellPage;

        // Apply the current theme to the new window's content.
        if (window.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = _themeSelectorService.Theme;
        }


        var manager = WindowManager.Get(window);
        manager.MinWidth = 640;
        manager.MinHeight = 480;

        TrackWindow(window);
        return window;
    }

    public Window? FindWindowForShellViewModel(ShellViewModel shellViewModel)
    {
        var window = _activeWindows.FirstOrDefault(w => (w.Content as ShellPage)?.ViewModel == shellViewModel);
        if (window == null)
        {
            Debug.WriteLine("[WindowService] No window found for the specified ShellViewModel instance.");
        }
        return window;
    }

    public Window? GetWindowForElement(UIElement element)
    {
        if (element.XamlRoot == null) return null;

        foreach (var window in _activeWindows)
        {
            if (window.Content?.XamlRoot == element.XamlRoot)
            {
                return window;
            }
        }
        return null;
    }

    public void RequestCloseWindow(ShellViewModel shellViewModel)
    {
        if (_activeWindows.Count <= 1)
        {
            Debug.WriteLine("[WindowService] Close requested for the last active window. Request denied.");
            return;
        }

        var windowToClose = FindWindowForShellViewModel(shellViewModel);
        if (windowToClose != null)
        {
            Debug.WriteLine("[WindowService] Close request approved. Closing window.");
            windowToClose.Close();
        }
    }

    private void TrackWindow(Window window)
    {
        if (!_activeWindows.Contains(window))
        {
            window.Closed += OnWindowClosed;
            _activeWindows.Add(window);
            Debug.WriteLine($"[WindowService] Started tracking new window. {_activeWindows.Count} windows are now active.");
        }
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        if (sender is Window closedWindow)
        {
            closedWindow.Closed -= OnWindowClosed;
            _activeWindows.Remove(closedWindow);
            Debug.WriteLine($"[WindowService] Window closed and removed from tracking. {_activeWindows.Count} windows remaining.");
        }
    }
}