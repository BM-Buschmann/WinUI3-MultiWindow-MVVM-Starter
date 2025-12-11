using System;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using Windows.UI;

namespace App.Helpers;

/// <summary>
/// A helper class to manage the colors of the default WinUI caption buttons (min, max, close)
/// to ensure they match the application's theme in a multi-window environment.
/// </summary>
public static class TitleBarHelper
{
    // P/Invoke definitions for forcing a refresh of the non-client area.
    private const int WAINACTIVE = 0x00;
    private const int WAACTIVE = 0x01;
    private const int WMACTIVATE = 0x0006;

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

    /// <summary>
    /// Updates the title bar caption button colors for a specific window to match the given theme.
    /// </summary>
    /// <param name="window">The window whose title bar needs to be updated.</param>
    /// <param name="theme">The theme to apply to the caption buttons.</param>
    public static void UpdateTitleBar(Window window, ElementTheme theme)
    {
        if (window.ExtendsContentIntoTitleBar)
        {
            // Resolve the "Default" theme to either Light or Dark.
            if (theme == ElementTheme.Default)
            {
                // Use the theme of the window's content.
                if (window.Content is FrameworkElement rootElement)
                {
                    theme = rootElement.ActualTheme;
                }
                else
                {
                    // Fallback for safety.
                    theme = Application.Current.RequestedTheme == ApplicationTheme.Light ? ElementTheme.Light : ElementTheme.Dark;
                }
            }

            // Set the button colors based on the resolved theme.
            window.AppWindow.TitleBar.ButtonForegroundColor = theme switch
            {
                ElementTheme.Dark => Colors.White,
                ElementTheme.Light => Colors.Black,
                _ => Colors.Transparent
            };

            window.AppWindow.TitleBar.ButtonHoverForegroundColor = theme switch
            {
                ElementTheme.Dark => Colors.White,
                ElementTheme.Light => Colors.Black,
                _ => Colors.Transparent
            };

            window.AppWindow.TitleBar.ButtonHoverBackgroundColor = theme switch
            {
                ElementTheme.Dark => Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF),
                ElementTheme.Light => Color.FromArgb(0x33, 0x00, 0x00, 0x00),
                _ => Colors.Transparent
            };

            window.AppWindow.TitleBar.ButtonPressedBackgroundColor = theme switch
            {
                ElementTheme.Dark => Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF),
                ElementTheme.Light => Color.FromArgb(0x66, 0x00, 0x00, 0x00),
                _ => Colors.Transparent
            };

            window.AppWindow.TitleBar.BackgroundColor = Colors.Transparent;

            // Force the title bar to redraw to apply the new colors.
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            if (hwnd == GetActiveWindow())
            {
                SendMessage(hwnd, WMACTIVATE, WAINACTIVE, IntPtr.Zero);
                SendMessage(hwnd, WMACTIVATE, WAACTIVE, IntPtr.Zero);
            }
            else
            {
                SendMessage(hwnd, WMACTIVATE, WAACTIVE, IntPtr.Zero);
                SendMessage(hwnd, WMACTIVATE, WAINACTIVE, IntPtr.Zero);
            }
        }
    }
}