// Contracts/Services/ITabInterchangeService.cs
using App.ViewModels;

namespace App.Contracts.Services;

/// <summary>
/// Defines a service that acts as a temporary broker for transferring
/// TabViewModel instances between different windows during a drag-and-drop operation.
/// </summary>
public interface ITabInterchangeService
{
    /// <summary>
    /// Stores the tab being dragged and returns a unique ID for the operation.
    /// This ID is safe to be serialized and placed in a DataPackage.
    /// </summary>
    /// <param name="tabToDrag">The TabViewModel instance being dragged.</param>
    /// <returns>A unique string identifier for the drag operation.</returns>
    string RegisterTabForDrag(TabViewModel tabToDrag);

    /// <summary>
    /// Retrieves the original TabViewModel instance using the unique ID
    /// from the drop operation. This should be called by the drop target.
    /// </summary>
    /// <param name="dragId">The unique identifier retrieved from the DataPackage.</param>
    /// <returns>The original TabViewModel instance, or null if not found.</returns>
    TabViewModel? RetrieveTabForDrop(string dragId);


    /// <summary>
    /// Marks a tab as having been successfully handled by an external target.
    /// </summary>
    void MarkAsExternallyHandled(TabViewModel tab);

    /// <summary>
    /// Checks if a tab was marked as handled by an external target.
    /// </summary>
    bool WasExternallyHandled(TabViewModel tab);

    /// <summary>
    /// Cleans up all registrations for a given tab instance after the operation is complete.
    /// </summary>
    void ClearAllRegistrations(TabViewModel tabToClear);
}