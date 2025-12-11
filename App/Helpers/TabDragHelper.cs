// Helpers/TabDragHelper.cs
using App.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;

namespace App.Helpers;

/// <summary>
/// A static helper class to manage the state of a single tab drag-and-drop operation across multiple windows.
/// This lightweight approach avoids the need for a more complex, stateful service for this specific feature.
/// </summary>
public static class TabDragHelper
{
    // Holds the unique ID and the TabViewModel for the currently active drag operation.
    private static KeyValuePair<string, TabViewModel>? _activeDrag;

    // A flag to indicate if the tab drop was handled by a different window than the source.
    private static bool _wasHandledExternally;

    /// <summary>
    /// Initializes a new drag-and-drop operation for a given tab.
    /// </summary>
    /// <param name="tab">The TabViewModel being dragged.</param>
    /// <returns>A unique string ID for this drag operation.</returns>
    public static string StartDrag(TabViewModel tab)
    {
        var dragId = $"AppTabDrag_{System.Guid.NewGuid()}";
        _activeDrag = new KeyValuePair<string, TabViewModel>(dragId, tab);
        _wasHandledExternally = false;

        Debug.WriteLine($"[TabDragHelper] Started drag for tab '{tab.Header}'. Assigned ID: {dragId}");

        return dragId;
    }

    /// <summary>
    /// Retrieves the TabViewModel associated with the current drag operation using its unique ID.
    /// </summary>
    /// <param name="dragId">The unique ID of the drag operation.</param>
    /// <returns>The <see cref="TabViewModel"/> being dragged, or <c>null</c> if the ID is invalid or no drag is active.</returns>
    public static TabViewModel? GetTabById(string dragId)
    {
        if (_activeDrag.HasValue && _activeDrag.Value.Key == dragId)
        {
            return _activeDrag.Value.Value;
        }

        Debug.WriteLine($"[TabDragHelper] GetTabById failed for ID: {dragId}. Drag may have ended or ID is invalid.");
        return null;
    }

    /// <summary>
    /// Sets a flag indicating that the dragged tab has been successfully dropped and handled
    /// by a target outside of the original source window.
    /// </summary>
    public static void MarkAsExternallyHandled()
    {
        _wasHandledExternally = true;
        Debug.WriteLine("[TabDragHelper] Drag operation marked as externally handled.");
    }

    /// <summary>
    /// Checks if the current drag operation was handled by an external target.
    /// </summary>
    /// <returns><c>true</c> if the tab was dropped in a different window; otherwise, <c>false</c>.</returns>
    public static bool WasDragHandledExternally()
    {
        return _wasHandledExternally;
    }

    /// <summary>
    /// Clears all state related to the current drag-and-drop operation. This should be called
    /// when the operation is completed, cancelled, or finished.
    /// </summary>
    public static void EndDrag()
    {
        _activeDrag = null;
        _wasHandledExternally = false;
        Debug.WriteLine("[TabDragHelper] Drag operation ended and state has been reset.");
    }
}