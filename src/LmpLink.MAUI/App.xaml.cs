namespace LmpLink.MAUI;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using LmpLink.MAUI.Services.Implementation;

public partial class App : Application
{
    public App()
    {
        MauiProgram.Log("=== App Constructor START ===");
        try
        {
            InitializeComponent();
            
            // Load UI settings from Preferences and apply to dynamic resources
            LoadUISettings();
            
            MauiProgram.Log("=== App Constructor SUCCESS ===");
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"!!! App Constructor FAILED: {ex.Message}");
            MauiProgram.Log(ex.StackTrace ?? "");
            throw;
        }
    }

    /// <summary>
    /// Load UI settings from Preferences and apply to app resources.
    /// </summary>
    private void LoadUISettings()
    {
        try
        {
            var infoOverlaySize = Preferences.Default.Get("InfoOverlaySize", "보통");
            var tooltipSize = Preferences.Default.Get("TooltipSize", "보통");
            var detailPanelSize = Preferences.Default.Get("DetailPanelSize", "보통");
            
            ApplyInfoOverlaySize(infoOverlaySize);
            ApplyTooltipSize(tooltipSize);
            ApplyDetailPanelSize(detailPanelSize);
            
            MauiProgram.Log($"[App] UI Settings loaded: InfoOverlay={infoOverlaySize}, Tooltip={tooltipSize}, DetailPanel={detailPanelSize}");
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[App] LoadUISettings FAILED: {ex.Message}");
        }
    }

    /// <summary>
    /// Apply info overlay (인원 집계) size.
    /// </summary>
    public static void ApplyInfoOverlaySize(string size)
    {
        var scale = size switch
        {
            "작게" => 0.8,
            "보통" => 1.0,
            "크게" => 1.3,
            _ => 1.0
        };

        if (Application.Current?.Resources != null)
        {
            Application.Current.Resources["InfoOverlayScale"] = scale;
            MauiProgram.Log($"[App] InfoOverlaySize applied: {size} (scale={scale})");
        }
    }

    /// <summary>
    /// Apply tooltip (마커 툴팁) size.
    /// </summary>
    public static void ApplyTooltipSize(string size)
    {
        var scale = size switch
        {
            "작게" => 0.8,
            "보통" => 1.0,
            "크게" => 1.3,
            _ => 1.0
        };

        if (Application.Current?.Resources != null)
        {
            Application.Current.Resources["TooltipScale"] = scale;
            MauiProgram.Log($"[App] TooltipSize applied: {size} (scale={scale})");
        }
    }

    /// <summary>
    /// Apply detail panel (상세 정보 창) size.
    /// </summary>
    public static void ApplyDetailPanelSize(string size)
    {
        var scale = size switch
        {
            "작게" => 0.8,
            "보통" => 1.0,
            "크게" => 1.3,
            _ => 1.0
        };

        if (Application.Current?.Resources != null)
        {
            Application.Current.Resources["DetailPanelScale"] = scale;
            MauiProgram.Log($"[App] DetailPanelSize applied: {size} (scale={scale})");
        }
    }

    /// <summary>
    /// Apply global UI scale (전체 화면 확대/축소).
    /// Also adjusts window size proportionally.
    /// </summary>
    public static void ApplyGlobalScale(double scale)
    {
        var scaleService = IPlatformApplication.Current?.Services?.GetService<ScaleService>();
        if (scaleService == null)
        {
            MauiProgram.Log("[App] ApplyGlobalScale skipped: ScaleService not available");
            return;
        }

        scaleService.SetScale(scale);
        MauiProgram.Log($"[App] ApplyGlobalScale delegated to ScaleService: {scale:F1}");
    }

    /// <summary>
    /// Get AppWindow from MAUI Window (WinUI).
    /// </summary>
    private static AppWindow? TryGetAppWindow(Window? mauiWindow)
    {
        try
        {
            if (mauiWindow == null || mauiWindow.Handler?.PlatformView == null)
            {
                return null;
            }
            var windowHandle = WindowNative.GetWindowHandle(mauiWindow.Handler.PlatformView);
            var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            return AppWindow.GetFromWindowId(windowId);
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[App] TryGetAppWindow FAILED: {ex.Message}");
            return null;
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        MauiProgram.Log("=== App.CreateWindow START ===");
        try
        {
            MauiProgram.Log("[1] Creating AppShell...");
            var shell = new AppShell();
            MauiProgram.Log("[2] AppShell created");
            
            var window = new Window(shell)
            {
                Title = "LMP-Link MVP",
                Width = 1280,
                Height = 720
            };

            try
            {
                var scaleService = IPlatformApplication.Current?.Services?.GetService<ScaleService>();
                scaleService?.Initialize(window);
            }
            catch (Exception ex)
            {
                MauiProgram.Log($"[App] ScaleService.Initialize FAILED: {ex.Message}");
            }

            MauiProgram.Log("=== App.CreateWindow SUCCESS ===");
            return window;
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"!!! App.CreateWindow FAILED: {ex.Message}");
            MauiProgram.Log(ex.StackTrace ?? "");
            throw;
        }
    }
}
