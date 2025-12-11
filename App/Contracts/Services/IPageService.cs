// Contracts/Services/IPageService.cs
using Microsoft.UI.Xaml.Controls;

namespace App.Contracts.Services;

public interface IPageService
{
    void Configure<TViewModel, TView>(string titleResourceKey, bool allowMultipleInstances = true) where TView : Page;
    Type GetPageType(string viewModelKey);
    IEnumerable<string> GetPageKeys();
    string GetPageTitle(string viewModelKey);

    bool GetPageAllowsMultipleInstances(string key);
}