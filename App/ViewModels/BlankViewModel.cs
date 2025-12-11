using App.Contracts.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace App.ViewModels;

public partial class BlankViewModel : ObservableRecipient, INavigationAware
{
    public BlankViewModel()
    {
    }
    
    public void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }
}
