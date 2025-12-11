// Helpers/ResourceExtensions.cs
using Microsoft.Windows.ApplicationModel.Resources;
using System.Diagnostics; // Required for Debug.WriteLine

namespace App.Helpers;

/// <summary>
/// Provides robust, exception-safe extension methods for retrieving localized strings.
/// </summary>
public static class ResourceExtensions
{
    private static readonly ResourceLoader _resourceLoader = new();

    /// <summary>
    /// Retrieves a localized string for the given resource key.
    /// If the key is not found, it logs an error to the debug console
    /// and returns a formatted fallback string without crashing the application.
    /// </summary>
    /// <param name="resourceKey">The key for the resource string (e.g., "HomePage_Title").</param>
    /// <returns>The localized string, or a fallback string if the key is not found.</returns>
    public static string GetLocalized(this string resourceKey)
    {
        if (TryGetLocalized(resourceKey, out var localizedString))
        {
            return localizedString!; // The '!' is safe here because TryGetLocalized guarantees it's not null.
        }

        // If the key was not found, log it for the developer and return a helpful placeholder.
        Debug.WriteLine($"[RESOURCE ERROR] Resource key not found: '{resourceKey}'. Check your Resources.resw file.");
        return $"[MISSING: {resourceKey}]";
    }

    /// <summary>
    /// Attempts to retrieve a localized string for the given resource key in a non-throwing manner.
    /// This is the standard "Try" pattern used throughout .NET.
    /// </summary>
    /// <param name="resourceKey">The key for the resource string.</param>
    /// <param name="localizedString">When this method returns, contains the localized string if the key was found;
    /// otherwise, null.</param>
    /// <returns>true if the key was found and the string was retrieved; otherwise, false.</returns>
    public static bool TryGetLocalized(this string resourceKey, out string? localizedString)
    {
        try
        {
            // GetStringForUri is a bit more complex but is often more reliable.
            // For simplicity, we stick with GetString but handle its exception.
            var result = _resourceLoader.GetString(resourceKey);

            // A resource can exist but be empty. Treat this as a success.
            if (result != null)
            {
                localizedString = result;
                return true;
            }
        }
        catch (Exception ex)
        {
            // This catch block is our primary defense against crashes from missing keys.
            // We don't need to log here because the calling GetLocalized method will.
            Debug.WriteLine($"[RESOURCE EXCEPTION] An exception occurred while retrieving key '{resourceKey}': {ex.Message}");
        }

        localizedString = null;
        return false;
    }
}