using App.Contracts.Services;
using App.ViewModels;

using Microsoft.UI.Xaml;

namespace App.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{

    public DefaultActivationHandler()
    {

    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        return true;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        await Task.CompletedTask;
    }
}
