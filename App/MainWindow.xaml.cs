using App.Contracts.Services;
using App.Helpers;
using App.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Windows.UI.ViewManagement;
using WinUIEx;

namespace App;

public sealed partial class MainWindow : WindowEx
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly UISettings _settings;

    public MainWindow()
    {
        InitializeComponent();

        // The ActivationService will navigate to the ShellPage and set it as the Content.
        Content = null;
        Title = "AppDisplayName".GetLocalized();

        // This code is essential for handling real-time changes to the Windows system theme.
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        _settings = new UISettings();
        // Subscribe to the event that fires when system colors (like theme) change.
        _settings.ColorValuesChanged += Settings_ColorValuesChanged;
    }

    /// <summary>
    /// This event handler is triggered when the user changes the Windows theme (e.g., from Light to Dark)
    /// while the application is running.
    /// </summary>
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // The event comes from a system thread. UI updates must be dispatched to the main UI thread.
        _dispatcherQueue.TryEnqueue(() =>
        {
            // First, get the ThemeSelectorService to check the app's current theme setting.
            var themeSelectorService = App.GetService<IThemeSelectorService>();


            TitleBarHelper.UpdateTitleBar(this, themeSelectorService.Theme);

        });
    }
}