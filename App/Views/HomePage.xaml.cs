using App.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace App.Views;

// Implement the INavigable interface to make this page discoverable by the PageService.
public sealed partial class HomePage : Page
{

    public HomeViewModel ViewModel
    {
        get;
    }

    public HomePage()
    {
        ViewModel = App.GetService<HomeViewModel>();
        InitializeComponent();
    }
}