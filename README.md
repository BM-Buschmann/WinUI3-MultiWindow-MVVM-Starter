# WinUI3 Tabbed MVVM Template

A professional scaffolding project for building modern, multi-tab, and multi-window WinUI 3 applications. This template provides a robust foundation based on the MVVM pattern and a decoupled, service-based architecture, allowing you to focus on building features, not boilerplate.

It demonstrates best practices for dependency injection, navigation, theme management, and window handling in a complex, real-world scenario.

---

## ✨ Features

*   **Modern MVVM Architecture:** Built with the Community Toolkit MVVM library (source generators, `ObservableObject`, `RelayCommand`).
*   **Multi-Tab Interface:** A fully-featured `TabView` that serves as the core of the application shell.
*   **Multi-Window Support:**
    *   Drag tabs out to create new windows seamlessly.
    *   Drag tabs between existing windows.
    *   Logic to prevent the last application window from closing.
*   **Decoupled Navigation Service:**
    *   A powerful `INavigationService` that handles all navigation logic.
    *   Supports browser-style, **per-tab navigation history** (Back/Forward).
    *   Differentiates between creating new tabs (`NavigateToTab`) and navigating within a tab (`NavigateInFrame`).
    *   Manages "singleton" tabs that can only have one instance open across all windows (e.g., Settings).
*   **Centralized Window & Theme Management:**
    *   A singleton `IWindowService` tracks all active windows.
    *   A decoupled `IThemeSelectorService` manages Light/Dark/Default themes.
    *   Theme changes apply instantly to all open windows and their title bars.
    *   Correctly handles system theme changes while the app is running.
*   **Dependency Injection:** Fully configured with `Microsoft.Extensions.DependencyInjection` for a clean, testable, and maintainable codebase.
*   **Custom Title Bar:** A fully customized title bar with a clean, modern aesthetic that is theme-aware.

---

## 🏛️ Architectural Overview

This template is built on a foundation of decoupled services managed by a Dependency Injection (DI) container. This ensures that responsibilities are clearly separated and components are easily testable.

### Core Services

*   **`IPageService`:** Acts as the application's "phonebook" for pages. It is configured at startup in `App.xaml.cs` to map a ViewModel to its corresponding View, a title resource key, and whether it can have multiple tab instances.

*   **`INavigationService`:** This is the brain of all navigation logic. A new, `transient` instance is created for each window.
    *   `NavigateToTab(pageKey)`: Use this for top-level actions, like opening a page from a menu. It will either create a new tab or, if the page is a singleton, find and focus the existing tab across any open window.
    *   `NavigateInFrame(pageKey)`: Use this for "drill-down" actions, like clicking an item in a list to see its details. This is what creates the history that enables the **Back** and **Forward** buttons.
    *   `GoBack()` / `GoForward()`: These methods operate on the navigation history of the frame *inside the currently selected tab*.

*   **`IWindowService`:** A `singleton` service that acts as the single source of truth for all window-related operations.
    *   Tracks all active application windows.
    *   Creates new windows (and their associated shells).
    *   Enforces the rule preventing the last window from closing.
    *   Listens for theme changes and applies them to all active windows.

*   **`IThemeSelectorService`:** A `singleton` service that manages the application's theme state (Light, Dark, or Default). It is completely decoupled from the UI and announces changes via a `ThemeChanged` event, which the `IWindowService` subscribes to.

---

## 🚀 Getting Started

1.  **Clone the repository:**
    ```bash
    git clone [Your-Repository-URL]
    ```
2.  **Open the solution:** Open the `.sln` file in Visual Studio 2022 (or later).
3.  **Restore Dependencies:** Build the solution to restore all NuGet packages.
4.  **Run the application:** Set the main project as the startup project and press F5 to run.

---

## 🔧 How to Extend

Adding new pages is simple and follows a consistent pattern.

### How to Add a New Page (e.g., "Projects")

1.  **Create the View:**
    *   Create a new `Blank Page` in the `Views` folder named `ProjectsPage.xaml`.

2.  **Create the ViewModel:**
    *   Create a new class in the `ViewModels` folder named `ProjectsViewModel.cs`.
    *   Make it inherit from `ObservableObject`.

    ```csharp
    // ViewModels/ProjectsViewModel.cs
    using CommunityToolkit.Mvvm.ComponentModel;

    namespace App.ViewModels;

    public partial class ProjectsViewModel : ObservableObject
    {
        public ProjectsViewModel()
        {
            // Your logic here
        }
    }
    ```

3.  **Register for Dependency Injection:**
    *   Open `App.xaml.cs`.
    *   **Register the View and ViewModel** in the `ConfigureServices` method.
    *   **Configure the page** with the `IPageService` after the Host is built.

    ```csharp
    // App.xaml.cs
    ...
    // Inside ConfigureServices...
    services.AddTransient<ProjectsViewModel>();
    services.AddTransient<ProjectsPage>();

    ...
    // After Host.Build()...
    var pageService = GetService<IPageService>();
    pageService.Configure<HomeViewModel, HomePage>("Home_Page_Title");
    pageService.Configure<SettingsViewModel, SettingsPage>("Settings_Page_Title", allowMultipleInstances: false);
    // Add your new page here
    pageService.Configure<ProjectsViewModel, ProjectsPage>("Projects_Page_Title");
    ```

4.  **Navigate to the New Page:**
    *   From any ViewModel where you have injected `INavigationService`, you can now navigate.

    ```csharp
    // Example from another ViewModel
    public class SomeOtherViewModel
    {
        private readonly INavigationService _navigationService;

        // ...

        [RelayCommand]
        private void OpenProjects()
        {
            // This will open "Projects" in a new tab.
            _navigationService.NavigateToTab(typeof(ProjectsViewModel).FullName!);
        }
    }
    ```

---

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgements

*   [Windows App SDK](https://github.com/microsoft/windowsappsdk)
*   [Community Toolkit for MVVM](https://docs.microsoft.com/windows/communitytoolkit/mvvm/introduction)
*   [WinUIEx Library](https://github.com/dotMorten/WinUIEx)
