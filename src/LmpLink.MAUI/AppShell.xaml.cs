#nullable enable

using LmpLink.MAUI.Views.Pages;

namespace LmpLink.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        MauiProgram.Log("=== AppShell Constructor START ===");
        try
        {
            InitializeComponent();
            MauiProgram.Log("[1] AppShell.InitializeComponent done");
            
            // Register routes for DI-based navigation
            Routing.RegisterRoute(nameof(MapPage), typeof(MapPage));
            Routing.RegisterRoute(nameof(ManagementPage), typeof(ManagementPage));
            Routing.RegisterRoute(nameof(DataPage), typeof(DataPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(PersonEditPage), typeof(PersonEditPage));
            Routing.RegisterRoute(nameof(TestPage), typeof(TestPage));
            MauiProgram.Log("=== AppShell Constructor SUCCESS ===");
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"!!! AppShell Constructor FAILED: {ex.Message}");
            MauiProgram.Log(ex.StackTrace ?? "");
            throw;
        }
    }
}

