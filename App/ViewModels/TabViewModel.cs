// ViewModels/TabViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace App.ViewModels;

/// <summary>
/// Represents the data model for a single tab in the shell.
/// This class serves as the DataContext for each generated TabViewItem.
/// </summary>
public partial class TabViewModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the text displayed in the tab's header.
    /// </summary>
    [ObservableProperty]
    private string _header;

    // This Frame is what will be displayed in the ShellPage.
    // It maintains its own navigation history (back-stack).
    public Frame NavigationFrame { get; } = new();

    // This stores the ViewModel's FullName (the pageKey) that this tab represents.
    // The 'init' keyword means it can only be set when the object is created.
    public string PageKey
    {
        get; init;
    }
}