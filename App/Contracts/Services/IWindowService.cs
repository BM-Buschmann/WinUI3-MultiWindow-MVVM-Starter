using System.Collections.Generic;
using App.ViewModels;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace App.Contracts.Services;

/// <summary>
/// Defines a contract for a service that manages the creation, tracking, and retrieval
/// of all application windows.
/// </summary>
public interface IWindowService
{
    /// <summary>
    /// Gets a read-only list of all currently active (open) windows.
    /// </summary>
    IReadOnlyList<Window> ActiveWindows
    {
        get;
    }

    /// <summary>
    /// Gets the main application window, which is conventionally the first window created.
    /// </summary>
    WindowEx? MainWindow
    {
        get;
    }

    /// <summary>
    /// Creates a new application window, hosts a ShellPage within it, applies the current theme,
    /// and begins tracking it.
    /// </summary>
    /// <returns>The newly created and configured window.</returns>
    WindowEx CreateWindowWithShell();

    /// <summary>
    /// Finds an active window by matching the exact instance of its ShellViewModel.
    /// </summary>
    /// <param name="shellViewModel">The specific ViewModel instance to find.</param>
    /// <returns>The Window that hosts the specified ViewModel, or null if not found.</returns>
    Window? FindWindowForShellViewModel(ShellViewModel shellViewModel);

    /// <summary>
    /// Retrieves the parent window for a given UIElement.
    /// </summary>
    /// <param name="element">The UI element contained within the window.</param>
    /// <returns>The parent Window, or null if the element is not yet part of the visual tree.</returns>
    Window? GetWindowForElement(UIElement element);

    /// <summary>
    /// Requests to close the window associated with the given ShellViewModel. The service will
    /// enforce application policies, such as not closing the last active window.
    /// </summary>
    /// <param name="shellViewModel">The ViewModel of the window requesting to be closed.</param>
    void RequestCloseWindow(ShellViewModel shellViewModel);
}