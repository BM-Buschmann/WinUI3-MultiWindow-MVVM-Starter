using App.Contracts.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace App.ViewModels;

public partial class HomeViewModel : ObservableRecipient, INavigationAware
{
    public HomeViewModel()
    {
    }

    public void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }
}
