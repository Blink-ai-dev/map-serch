#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using System.Linq;
using WinRT.Interop;
using LmpLink.MAUI;

namespace LmpLink.MAUI.Services.Implementation;

/// <summary>
/// Centralized UI scale service: manages scale state and window resizing.
/// </summary>
public class ScaleService
{
    private const double MinScale = 0.8;
    private const double MaxScale = 1.5;
    private const double Step = 0.1;

    // Base UI sizes
    private const double BaseTitleFontSize = 20;
    private const double BaseButtonFontSize = 16;
    private const double BaseButtonHeight = 48;
    private const double BaseTabFontSize = 15;
    private const double BaseTabHeight = 56;
    private const double BaseHeaderFontSize = 20;
    private const double BaseBodyFontSize = 14;
    private static readonly Thickness BaseCardPadding = new(12, 8);
    private const double BaseTitleBarHeight = 68;
    private static readonly Thickness BaseTitleBarPadding = new(14, 10);
    private static readonly Thickness BaseTabPadding = new(10, 6);

    private bool _baseSizeCached;
    private int _baseWidth = 1280;
    private int _baseHeight = 720;

    public double CurrentScale { get; private set; } = 1.0;

    public event EventHandler<double>? ScaleChanged;

    /// <summary>
    /// Initialize base window size from current window.
    /// </summary>
    public void Initialize(Window window)
    {
        if (window == null || window.Handler?.PlatformView == null)
        {
            return;
        }

        if (_baseSizeCached)
        {
            return;
        }

        var appWindow = TryGetAppWindow(window);
        if (appWindow != null)
        {
            _baseWidth = appWindow.Size.Width;
            _baseHeight = appWindow.Size.Height;
        }
        else
        {
            _baseWidth = (int)window.Width;
            _baseHeight = (int)window.Height;
        }

        _baseSizeCached = true;
        MauiProgram.Log($"[ScaleService] Base size cached: {_baseWidth}x{_baseHeight}");
    }

    /// <summary>
    /// Center the window on current display using base size (without changing scale).
    /// </summary>
    public void CenterWindow()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window == null)
        {
            return;
        }

        if (!_baseSizeCached)
        {
            Initialize(window);
        }

        var appWindow = TryGetAppWindow(window);
        var targetWidth = _baseWidth;
        var targetHeight = _baseHeight;

        if (appWindow != null)
        {
            try
            {
                var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
                var workArea = displayArea.WorkArea;
                var x = workArea.X + Math.Max(0, (workArea.Width - targetWidth) / 2);
                var y = workArea.Y + Math.Max(0, (workArea.Height - targetHeight) / 2);
                appWindow.MoveAndResize(new Windows.Graphics.RectInt32(x, y, targetWidth, targetHeight));
            }
            catch (Exception ex)
            {
                MauiProgram.Log($"[ScaleService] CenterWindow failed: {ex.Message}");
            }
        }
        else
        {
            window.Width = targetWidth;
            window.Height = targetHeight;
        }
    }

    public void Increase() => SetScale(CurrentScale + Step);
    public void Decrease() => SetScale(CurrentScale - Step);
    public void Reset() => SetScale(1.0);

    public void SetScale(double targetScale)
    {
        var clamped = Math.Clamp(targetScale, MinScale, MaxScale);
        if (Math.Abs(clamped - CurrentScale) < 0.0001)
        {
            return;
        }

        CurrentScale = clamped;

        ApplyWindowSize();
        UpdateScaleResources();
        ScaleChanged?.Invoke(this, CurrentScale);

        MauiProgram.Log($"[ScaleService] Scale set to {CurrentScale:F1}");
    }

    private void ApplyWindowSize()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window == null)
        {
            MauiProgram.Log("[ScaleService] ApplyWindowSize skipped: window null");
            return;
        }

        if (!_baseSizeCached)
        {
            Initialize(window);
        }

        var targetWidth = (int)(_baseWidth * CurrentScale);
        var targetHeight = (int)(_baseHeight * CurrentScale);

        var appWindow = TryGetAppWindow(window);
        if (appWindow != null)
        {
            // Keep current position; resize without re-centering.
            var pos = appWindow.Position;
            appWindow.MoveAndResize(new Windows.Graphics.RectInt32(pos.X, pos.Y, targetWidth, targetHeight));
        }
        else
        {
            window.Width = targetWidth;
            window.Height = targetHeight;
        }
    }

    private void UpdateScaleResources()
    {
        var res = Application.Current?.Resources;
        if (res == null) return;

        double scale = CurrentScale;
        res["TitleFontSize"] = BaseTitleFontSize * scale;
        res["ButtonFontSize"] = BaseButtonFontSize * scale;
        res["ButtonHeight"] = BaseButtonHeight * scale;
        res["TabFontSize"] = BaseTabFontSize * scale;
        res["TabHeight"] = BaseTabHeight * scale;
        res["HeaderFontSize"] = BaseHeaderFontSize * scale;
        res["BodyFontSize"] = BaseBodyFontSize * scale;
        res["CardPadding"] = new Thickness(BaseCardPadding.Left * scale, BaseCardPadding.Top * scale, BaseCardPadding.Right * scale, BaseCardPadding.Bottom * scale);
        res["TitleBarHeight"] = BaseTitleBarHeight * scale;
        res["TitleBarPadding"] = new Thickness(BaseTitleBarPadding.Left * scale, BaseTitleBarPadding.Top * scale, BaseTitleBarPadding.Right * scale, BaseTitleBarPadding.Bottom * scale);
        res["TabPadding"] = new Thickness(BaseTabPadding.Left * scale, BaseTabPadding.Top * scale, BaseTabPadding.Right * scale, BaseTabPadding.Bottom * scale);
    }

    private AppWindow? TryGetAppWindow(Window mauiWindow)
    {
        try
        {
            var handle = WindowNative.GetWindowHandle(mauiWindow.Handler.PlatformView);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
            return AppWindow.GetFromWindowId(windowId);
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[ScaleService] TryGetAppWindow failed: {ex.Message}");
            return null;
        }
    }
}

