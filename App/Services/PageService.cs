// Services/PageService.cs
using App.Contracts.Services;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace App.Services;

/// <summary>
/// A service that manages the mapping between ViewModel keys, page types, and page titles.
/// </summary>
public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new();
    private readonly Dictionary<string, string> _pageTitles = new();
    private readonly Dictionary<string, bool> _pageAllowsMultipleInstances = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PageService"/> class.
    /// </summary>
    public PageService()
    {
        Debug.WriteLine("[PageService] Initialized.");
    }

    /// <summary>
    /// Configures and registers a new ViewModel-to-View mapping. This method is typically called
    /// during application startup (e.g., in App.xaml.cs).
    /// </summary>
    /// <typeparam name="TViewModel">The type of the ViewModel. Its full name will be used as the key.</typeparam>
    /// <typeparam name="TView">The type of the corresponding View (must be a Page).</typeparam>
    /// <param name="titleResourceKey">The resource key for the localized page title.</param>
    public void Configure<TViewModel, TView>(string titleResourceKey, bool allowMultipleInstances = true) where TView : Page
    {
        lock (_pages)
        {
            var key = typeof(TViewModel).FullName!;
            if (_pages.ContainsKey(key))
            {
                var message = $"The ViewModel key {key} is already configured.";
                Debug.WriteLine($"[PageService] ERROR: {message}");
                throw new ArgumentException(message);
            }

            _pages.Add(key, typeof(TView));
            _pageTitles.Add(key, titleResourceKey);
            _pageAllowsMultipleInstances.Add(key, allowMultipleInstances);
            Debug.WriteLine($"[PageService] Configured page for key '{key}' (AllowMultiple: {allowMultipleInstances}).");
        }
    }

    /// <summary>
    /// Retrieves the page type associated with a given ViewModel key.
    /// </summary>
    /// <param name="key">The full name of the ViewModel.</param>
    /// <returns>The <see cref="Type"/> of the corresponding page.</returns>
    /// <exception cref="ArgumentException">Thrown if the key is not registered.</exception>
    public Type GetPageType(string key)
    {
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out var pageType))
            {
                var message = $"Page not found for key: {key}. Ensure it is registered in App.xaml.cs.";
                Debug.WriteLine($"[PageService] ERROR: {message}");
                throw new ArgumentException(message);
            }

            Debug.WriteLine($"[PageService] Retrieved page type '{pageType.Name}' for key '{key}'.");
            return pageType;
        }
    }

    public bool GetPageAllowsMultipleInstances(string key)
    {
        lock (_pageAllowsMultipleInstances)
        {
            // Defaults to true if for some reason the key is not found, which maintains existing behavior.
            return _pageAllowsMultipleInstances.TryGetValue(key, out var allowMultiple) ? allowMultiple : true;
        }
    }

    /// <summary>
    /// Gets a collection of all registered ViewModel keys.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of registered keys.</returns>
    public IEnumerable<string> GetPageKeys()
    {
        lock (_pages)
        {
            return _pages.Keys.ToList();
        }
    }

    /// <summary>
    /// Retrieves the title resource key for a given ViewModel key.
    /// </summary>
    /// <param name="key">The full name of the ViewModel.</param>
    /// <returns>The title resource key, or the ViewModel key itself as a fallback.</returns>
    public string GetPageTitle(string key)
    {
        lock (_pageTitles)
        {
            if (_pageTitles.TryGetValue(key, out var titleKey))
            {
                return titleKey;
            }

            // Fallback to the ViewModel key if a title is not found.
            Debug.WriteLine($"[PageService] Title not found for key '{key}'. Falling back to key name.");
            return key;
        }
    }
}