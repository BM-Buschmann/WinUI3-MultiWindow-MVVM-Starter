using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace App.Contracts.Services;

/// <summary>
/// Defines a contract for a service that manages the application's theme.
/// </summary>
public interface IThemeSelectorService
{
    /// <summary>
    /// Occurs when the application theme has been changed.
    /// The parameter is the new ElementTheme that was set.
    /// </summary>
    event Action<ElementTheme> ThemeChanged;

    /// <summary>
    /// Gets the current theme of the application.
    /// </summary>
    ElementTheme Theme
    {
        get;
    }

    /// <summary>
    /// Initializes the service by loading the saved theme from settings.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Sets the application's theme and raises the ThemeChanged event.
    /// </summary>
    /// <param name="theme">The theme to apply (Light, Dark, or Default).</param>
    Task SetThemeAsync(ElementTheme theme);
}