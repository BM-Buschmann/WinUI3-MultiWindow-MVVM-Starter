using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using App.Activation;
using App.Contracts.Services;
using Microsoft.UI.Xaml;

namespace App.Services;

/// <summary>
/// A service responsible for orchestrating the application's startup and activation sequence.
/// It coordinates the initialization of services and the creation and activation of the first window.
/// </summary>
public class ActivationService : IActivationService
{
    private readonly IWindowService _windowService;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivationService"/> class.
    /// </summary>
    /// <param name="windowService">The service for creating and managing application windows.</param>
    /// <param name="themeSelectorService">The service for managing the application's theme.</param>
    /// <param name="defaultHandler">The default activation handler for standard launch events.</param>
    /// <param name="activationHandlers">A collection of specific activation handlers (e.g., for protocols or file associations).</param>
    public ActivationService(
        IWindowService windowService,
        IThemeSelectorService themeSelectorService,
        ActivationHandler<LaunchActivatedEventArgs> defaultHandler,
        IEnumerable<IActivationHandler> activationHandlers)
    {
        _windowService = windowService;
        _themeSelectorService = themeSelectorService;
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        Debug.WriteLine("[ActivationService] Initialized.");
    }

    /// <summary>
    /// Activates the application by initializing services, creating the main window,
    /// handling the activation event, and running startup tasks.
    /// </summary>
    /// <param name="activationArgs">The activation arguments provided by the operating system.</param>
    public async Task ActivateAsync(object activationArgs)
    {
        Debug.WriteLine("[ActivationService] Starting activation process...");

        // 1. Execute pre-activation tasks. This includes loading the saved theme
        //    *before* any UI is created, so the first window appears with the correct theme.
        await InitializeAsync();

        // 2. Create, configure, and track the main window. The WindowService will apply the theme.
        var mainWindow = _windowService.CreateWindowWithShell();
        Debug.WriteLine("[ActivationService] Main window created.");

        // 3. Handle the specific activation event (e.g., Launch, Protocol, File).
        //    The DefaultActivationHandler will run if no other handler is found.
        await HandleActivationAsync(activationArgs);

        // 4. Activate the main window, making it visible and bringing it to the foreground.
        mainWindow.Activate();
        Debug.WriteLine("[ActivationService] Main window activated.");

        // 5. Execute post-activation startup tasks.
        await StartupAsync();

        Debug.WriteLine("[ActivationService] Activation process completed.");
    }

    /// <summary>
    /// Finds and executes the appropriate activation handler for the given activation arguments.
    /// </summary>
    private async Task HandleActivationAsync(object activationArgs)
    {
        // Find a specific handler that can manage the activation type.
        var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

        if (activationHandler != null)
        {
            Debug.WriteLine($"[ActivationService] Using specific activation handler: {activationHandler.GetType().Name}");
            await activationHandler.HandleAsync(activationArgs);
        }

        // Always check if the default handler can run. This is the fallback for a normal launch.
        if (_defaultHandler.CanHandle(activationArgs))
        {
            Debug.WriteLine("[ActivationService] Using default activation handler.");
            await _defaultHandler.HandleAsync(activationArgs);
        }
    }

    /// <summary>
    /// Runs asynchronous initialization tasks required before the main window is created.
    /// </summary>
    private async Task InitializeAsync()
    {
        Debug.WriteLine("[ActivationService] Running InitializeAsync tasks...");
        // Initialize the theme service so it loads the saved theme before any windows are created.
        await _themeSelectorService.InitializeAsync().ConfigureAwait(false);
        Debug.WriteLine("[ActivationService] InitializeAsync tasks completed.");
    }

    /// <summary>
    /// Runs asynchronous startup tasks after the main window has been activated.
    /// </summary>
    private async Task StartupAsync()
    {
        Debug.WriteLine("[ActivationService] Running StartupAsync tasks...");
        // The theme is applied during window creation, so no extra call is needed here.
        // This is a good place for other startup tasks, like checking for updates,
        // syncing user data, etc., that can run after the UI is visible.
        await Task.CompletedTask;
        Debug.WriteLine("[ActivationService] StartupAsync tasks completed.");
    }
}