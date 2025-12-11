using System;
using System.Diagnostics;
using System.Threading.Tasks;
using App.Contracts.Services;
using Microsoft.UI.Xaml;

namespace App.Services;

public class ThemeSelectorService : IThemeSelectorService
{
    private const string SettingsKey = "AppBackgroundRequestedTheme";
    private readonly ILocalSettingsService _localSettingsService;

    public event Action<ElementTheme>? ThemeChanged;
    public ElementTheme Theme { get; private set; } = ElementTheme.Default;

    public ThemeSelectorService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        var themeName = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);
        if (Enum.TryParse(themeName, out ElementTheme cacheTheme))
        {
            Theme = cacheTheme;
        }
    }

    public async Task SetThemeAsync(ElementTheme theme)
    {
        if (Theme == theme) return;

        Theme = theme;
        await _localSettingsService.SaveSettingAsync(SettingsKey, theme.ToString());

        // Announce that the theme has changed.
        ThemeChanged?.Invoke(theme);
    }
}