#nullable enable

namespace LmpLink.MAUI.Views.Controls;

using Microsoft.Extensions.DependencyInjection;

public partial class AppTabMenu : ContentView
{
    private Services.Implementation.ScaleService? _scaleService;

    public AppTabMenu()
    {
        InitializeComponent();

        try
        {
            _scaleService = IPlatformApplication.Current?.Services?.GetService<Services.Implementation.ScaleService>();
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
            MauiProgram.Log($"[AppTabMenu] Init failed: {ex.Message}");
            MauiProgram.Log(ex.StackTrace ?? string.Empty);
        }
    }

    private static async Task NavigateAsync(string route)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(route))
            {
                return;
            }

            var shell = Shell.Current;
            if (shell == null)
            {
                return;
            }

            await shell.GoToAsync(route);
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[AppTabMenu] Navigation failed ({route}): {ex.Message}");
            MauiProgram.Log(ex.StackTrace ?? string.Empty);
        }
    }

    private async void OnTabMapClicked(object? sender, EventArgs e) => await NavigateAsync("//MapPage");
    private async void OnTabManagementClicked(object? sender, EventArgs e) => await NavigateAsync("//ManagementPage");
    private async void OnTabDataClicked(object? sender, EventArgs e) => await NavigateAsync("//DataPage");
    private async void OnTabSettingsClicked(object? sender, EventArgs e) => await NavigateAsync("//SettingsPage");

    private void OnScaleUpClicked(object? sender, EventArgs e)
    {
        _scaleService ??= IPlatformApplication.Current?.Services?.GetService<Services.Implementation.ScaleService>();
        _scaleService?.Increase();
        if (_scaleService != null) UpdateScaleLabel(_scaleService.CurrentScale);
    }

    private void OnScaleDownClicked(object? sender, EventArgs e)
    {
        _scaleService ??= IPlatformApplication.Current?.Services?.GetService<Services.Implementation.ScaleService>();
        _scaleService?.Decrease();
        if (_scaleService != null) UpdateScaleLabel(_scaleService.CurrentScale);
    }

    private void OnScaleResetClicked(object? sender, EventArgs e)
    {
        _scaleService ??= IPlatformApplication.Current?.Services?.GetService<Services.Implementation.ScaleService>();
        _scaleService?.Reset();
        if (_scaleService != null) UpdateScaleLabel(_scaleService.CurrentScale);
    }

    private void OnScaleChanged(object? sender, double scale) => UpdateScaleLabel(scale);

    private void UpdateScaleLabel(double scale)
    {
        if (ScaleLabel == null)
        {
            return;
        }

        ScaleLabel.Text = $"{scale:P0}";
    }
}



