#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using LmpLink.MAUI.Messages;
using LmpLink.MAUI.ViewModels.Base;

namespace LmpLink.MAUI.ViewModels;

/// <summary>
/// ViewModel for SettingsPage (UI customization, theme settings).
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    // --- Observable Properties ---

    // Marker Settings
    [ObservableProperty]
    private string _userMarkerColor = "#60A5FA"; // Default: Blue

    [ObservableProperty]
    private string _assistantMarkerColor = "#FB923C"; // Default: Orange

    [ObservableProperty]
    private string _markerSize = "Medium"; // Small, Medium, Large

    [ObservableProperty]
    private int _userMarkerBorderWidth = 2; // 1-5 px

    [ObservableProperty]
    private int _assistantMarkerBorderWidth = 2; // 1-5 px

    [ObservableProperty]
    private string _userMarkerShape = "Circle"; // Circle, Square, Diamond

    [ObservableProperty]
    private string _assistantMarkerShape = "Square"; // Circle, Square, Diamond

    [ObservableProperty]
    private string _highlightColor = "#FFFFFF"; // White for highlight border

    [ObservableProperty]
    private int _highlightBorderWidth = 3; // 1-5 px

    [ObservableProperty]
    private double _highlightSizeMultiplier = 2.0; // 1.5-3.0x

    // Saved (current) colors for comparison
    [ObservableProperty]
    private string _savedUserMarkerColor = "#60A5FA";

    [ObservableProperty]
    private string _savedAssistantMarkerColor = "#FB923C";

    [ObservableProperty]
    private string _savedCircleColor = "#60A5FA";

    // Circle Settings
    [ObservableProperty]
    private string _circleColor = "#60A5FA"; // Default: Blue

    [ObservableProperty]
    private double _circleOpacity = 0.3; // 0.0 - 1.0

    [ObservableProperty]
    private int _circleBorderWidth = 2; // 1 - 5 px

    // Theme Settings
    [ObservableProperty]
    private bool _isDarkMode = true; // Default: Dark mode

    // Card Size Settings (Individual)
    [ObservableProperty]
    private string _infoOverlaySize = "보통"; // 작게, 보통, 크게

    [ObservableProperty]
    private string _tooltipSize = "보통"; // 작게, 보통, 크게

    [ObservableProperty]
    private string _detailPanelSize = "보통"; // 작게, 보통, 크게

    // --- Constructor ---

    public SettingsViewModel()
    {
        MauiProgram.Log("[SettingsViewModel] Constructor called");
        LoadSettings();
    }

    // --- Commands ---

    /// <summary>
    /// Load settings from Preferences.
    /// </summary>
    [RelayCommand]
    private void LoadSettings()
    {
        MauiProgram.Log("[SettingsViewModel] LoadSettings START");

        try
        {
            // Marker Settings
            UserMarkerColor = Preferences.Default.Get("UserMarkerColor", "#60A5FA");
            AssistantMarkerColor = Preferences.Default.Get("AssistantMarkerColor", "#FB923C");
            MarkerSize = Preferences.Default.Get("MarkerSize", "Medium");
            UserMarkerBorderWidth = Preferences.Default.Get("UserMarkerBorderWidth", 2);
            AssistantMarkerBorderWidth = Preferences.Default.Get("AssistantMarkerBorderWidth", 2);
            UserMarkerShape = Preferences.Default.Get("UserMarkerShape", "Circle");
            AssistantMarkerShape = Preferences.Default.Get("AssistantMarkerShape", "Square");
            
            // Highlight Settings
            HighlightColor = Preferences.Default.Get("HighlightColor", "#FFFFFF");
            HighlightBorderWidth = Preferences.Default.Get("HighlightBorderWidth", 3);
            HighlightSizeMultiplier = Preferences.Default.Get("HighlightSizeMultiplier", 2.0);

            // Circle Settings
            CircleColor = Preferences.Default.Get("CircleColor", "#60A5FA");
            CircleOpacity = Preferences.Default.Get("CircleOpacity", 0.3);
            CircleBorderWidth = Preferences.Default.Get("CircleBorderWidth", 2);

            // Theme Settings
            IsDarkMode = Preferences.Default.Get("IsDarkMode", true);

            // Card Size Settings
            InfoOverlaySize = Preferences.Default.Get("InfoOverlaySize", "보통");
            TooltipSize = Preferences.Default.Get("TooltipSize", "보통");
            DetailPanelSize = Preferences.Default.Get("DetailPanelSize", "보통");

            // Update saved colors (for "현재" display)
            SavedUserMarkerColor = UserMarkerColor;
            SavedAssistantMarkerColor = AssistantMarkerColor;
            SavedCircleColor = CircleColor;

            MauiProgram.Log($"[SettingsViewModel] Settings loaded: DarkMode={IsDarkMode}, MarkerSize={MarkerSize}");
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SettingsViewModel] LoadSettings FAILED: {ex.Message}");
        }
    }

    /// <summary>
    /// Save current settings to Preferences.
    /// </summary>
    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        await ExecuteAsync(async () =>
        {
            MauiProgram.Log("[SettingsViewModel] SaveSettings START");

            try
            {
                // Marker Settings
                Preferences.Default.Set("UserMarkerColor", UserMarkerColor);
                Preferences.Default.Set("AssistantMarkerColor", AssistantMarkerColor);
                Preferences.Default.Set("MarkerSize", MarkerSize);
                Preferences.Default.Set("UserMarkerBorderWidth", UserMarkerBorderWidth);
                Preferences.Default.Set("AssistantMarkerBorderWidth", AssistantMarkerBorderWidth);
                Preferences.Default.Set("UserMarkerShape", UserMarkerShape);
                Preferences.Default.Set("AssistantMarkerShape", AssistantMarkerShape);
                
                // Highlight Settings
                Preferences.Default.Set("HighlightColor", HighlightColor);
                Preferences.Default.Set("HighlightBorderWidth", HighlightBorderWidth);
                Preferences.Default.Set("HighlightSizeMultiplier", HighlightSizeMultiplier);

                // Circle Settings
                Preferences.Default.Set("CircleColor", CircleColor);
                Preferences.Default.Set("CircleOpacity", CircleOpacity);
                Preferences.Default.Set("CircleBorderWidth", CircleBorderWidth);

                // Theme Settings
                Preferences.Default.Set("IsDarkMode", IsDarkMode);

                // Card Size Settings
                Preferences.Default.Set("InfoOverlaySize", InfoOverlaySize);
                Preferences.Default.Set("TooltipSize", TooltipSize);
                Preferences.Default.Set("DetailPanelSize", DetailPanelSize);

                // Update saved colors after successful save
                SavedUserMarkerColor = UserMarkerColor;
                SavedAssistantMarkerColor = AssistantMarkerColor;
                SavedCircleColor = CircleColor;

                // Apply UI settings immediately
                App.ApplyInfoOverlaySize(InfoOverlaySize);
                App.ApplyTooltipSize(TooltipSize);
                App.ApplyDetailPanelSize(DetailPanelSize);

                MauiProgram.Log("[SettingsViewModel] Settings saved successfully");
                
                // Notify other ViewModels that settings have changed
                WeakReferenceMessenger.Default.Send(new SettingsChangedMessage());
                MauiProgram.Log("[SettingsViewModel] SettingsChangedMessage sent");
                
                await Shell.Current.DisplayAlertAsync("저장 완료", "설정이 저장되었습니다.", "확인");
            }
            catch (Exception ex)
            {
                MauiProgram.Log($"[SettingsViewModel] SaveSettings FAILED: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("오류", $"설정 저장 실패:\n{ex.Message}", "확인");
            }
        });
    }

    /// <summary>
    /// Reset settings to defaults.
    /// </summary>
    [RelayCommand]
    private async Task ResetSettingsAsync()
    {
        var confirm = await Shell.Current.DisplayAlertAsync(
            "설정 초기화",
            "모든 설정을 기본값으로 되돌리시겠습니까?",
            "초기화",
            "취소");

        if (!confirm)
        {
            return;
        }

        await ExecuteAsync(async () =>
        {
            MauiProgram.Log("[SettingsViewModel] ResetSettings START");

            try
            {
                // Clear all preferences
                Preferences.Default.Clear();

                // Reload defaults
                LoadSettings();

                MauiProgram.Log("[SettingsViewModel] Settings reset successfully");
                await Shell.Current.DisplayAlertAsync("초기화 완료", "설정이 초기화되었습니다.", "확인");
            }
            catch (Exception ex)
            {
                MauiProgram.Log($"[SettingsViewModel] ResetSettings FAILED: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("오류", $"설정 초기화 실패:\n{ex.Message}", "확인");
            }
        });
    }

    /// <summary>
    /// Update marker size.
    /// </summary>
    [RelayCommand]
    private void UpdateMarkerSize(string size)
    {
        MauiProgram.Log($"[SettingsViewModel] UpdateMarkerSize: {size}");
        MarkerSize = size;
    }

    /// <summary>
    /// Update user marker color.
    /// </summary>
    [RelayCommand]
    private void UpdateUserMarkerColor(string color)
    {
        MauiProgram.Log($"[SettingsViewModel] UpdateUserMarkerColor: {color}");
        UserMarkerColor = color;
    }

    /// <summary>
    /// Update assistant marker color.
    /// </summary>
    [RelayCommand]
    private void UpdateAssistantMarkerColor(string color)
    {
        MauiProgram.Log($"[SettingsViewModel] UpdateAssistantMarkerColor: {color}");
        AssistantMarkerColor = color;
    }

    /// <summary>
    /// Update circle color.
    /// </summary>
    [RelayCommand]
    private void UpdateCircleColor(string color)
    {
        MauiProgram.Log($"[SettingsViewModel] UpdateCircleColor: {color}");
        CircleColor = color;
    }

    /// <summary>
    /// Update user marker shape.
    /// </summary>
    [RelayCommand]
    private void UpdateUserMarkerShape(string shape)
    {
        MauiProgram.Log($"[SettingsViewModel] UpdateUserMarkerShape: {shape}");
        UserMarkerShape = shape;
    }

    /// <summary>
    /// Update assistant marker shape.
    /// </summary>
    [RelayCommand]
    private void UpdateAssistantMarkerShape(string shape)
    {
        MauiProgram.Log($"[SettingsViewModel] UpdateAssistantMarkerShape: {shape}");
        AssistantMarkerShape = shape;
    }

    /// <summary>
    /// Update highlight color.
    /// </summary>
    [RelayCommand]
    private void UpdateHighlightColor(string color)
    {
        MauiProgram.Log($"[SettingsViewModel] UpdateHighlightColor: {color}");
        HighlightColor = color;
    }

    /// <summary>
    /// Update info overlay size.
    /// </summary>
    [RelayCommand]
    private void UpdateInfoOverlaySize(string size)
    {
        MauiProgram.Log($"[SettingsViewModel] UpdateInfoOverlaySize: {size}");
        InfoOverlaySize = size;
    }

    /// <summary>
    /// Update tooltip size.
    /// </summary>
    [RelayCommand]
    private void UpdateTooltipSize(string size)
    {
        MauiProgram.Log($"[SettingsViewModel] UpdateTooltipSize: {size}");
        TooltipSize = size;
    }

    /// <summary>
    /// Update detail panel size.
    /// </summary>
    [RelayCommand]
    private void UpdateDetailPanelSize(string size)
    {
        MauiProgram.Log($"[SettingsViewModel] UpdateDetailPanelSize: {size}");
        DetailPanelSize = size;
    }

    /// <summary>
    /// Toggle dark mode.
    /// </summary>
    [RelayCommand]
    private void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        MauiProgram.Log($"[SettingsViewModel] DarkMode toggled: {IsDarkMode}");
        
        // TODO: Apply theme change to app
        // Application.Current.UserAppTheme = IsDarkMode ? AppTheme.Dark : AppTheme.Light;
    }
}

