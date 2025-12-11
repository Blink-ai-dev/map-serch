using LmpLink.MAUI.Views.Pages;

namespace LmpLink.MAUI;

using Microsoft.Extensions.DependencyInjection;
using LmpLink.MAUI.Services.Implementation;

public partial class AppShell : Shell
{
    private ScaleService? _scaleService;

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

            _scaleService = IPlatformApplication.Current?.Services?.GetService<ScaleService>();
            if (_scaleService != null)
            {
                _scaleService.ScaleChanged += OnScaleChanged;
                UpdateScaleLabel(_scaleService.CurrentScale);
            }

            Loaded += (_, __) =>
            {
                if (_scaleService != null)
                {
                    UpdateScaleLabel(_scaleService.CurrentScale);
                }
            };
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"!!! AppShell Constructor FAILED: {ex.Message}");
            MauiProgram.Log(ex.StackTrace ?? "");
            throw;
        }
    }

    private void OnScaleUpClicked(object sender, EventArgs e)
    {
        _scaleService ??= IPlatformApplication.Current?.Services?.GetService<ScaleService>();
        _scaleService?.Increase();
        if (_scaleService != null) UpdateScaleLabel(_scaleService.CurrentScale);
    }

    private void OnScaleDownClicked(object sender, EventArgs e)
    {
        _scaleService ??= IPlatformApplication.Current?.Services?.GetService<ScaleService>();
        _scaleService?.Decrease();
        if (_scaleService != null) UpdateScaleLabel(_scaleService.CurrentScale);
    }

    private void OnScaleResetClicked(object sender, EventArgs e)
    {
        _scaleService ??= IPlatformApplication.Current?.Services?.GetService<ScaleService>();
        _scaleService?.Reset();
        if (_scaleService != null) UpdateScaleLabel(_scaleService.CurrentScale);
    }

    private async void OnTabMapClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MapPage");
    }

    private async void OnTabManagementClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ManagementPage");
    }

    private async void OnTabDataClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//DataPage");
    }

    private async void OnTabSettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//SettingsPage");
    }

    private void OnScaleChanged(object? sender, double scale)
    {
        UpdateScaleLabel(scale);
    }

    private void UpdateScaleLabel(double scale)
    {
        if (ScaleLabel != null)
        {
            ScaleLabel.Text = $"{scale:P0}";
        }
    }
}

