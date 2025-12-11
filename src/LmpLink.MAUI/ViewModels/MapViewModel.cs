#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using LmpLink.MAUI.Messages;
using LmpLink.MAUI.Services.Implementation;
using LmpLink.MAUI.ViewModels.Base;

namespace LmpLink.MAUI.ViewModels;

/// <summary>
/// Center mode for filtering: User-centric or Assistant-centric.
/// </summary>
public enum CenterMode
{
    User,      // 이용자 중심 - 이용자 선택 시 활동지원사 필터링
    Assistant  // 활동지원사 중심 - 활동지원사 선택 시 이용자 필터링
}

/// <summary>
/// ViewModel for the main map page.
/// Manages users, assistants, filtering by radius.
/// 
/// Converted from Blazor Home.razor @code block:
/// - private List<Person> filteredPeople → ObservableCollection<Person> FilteredAssistants
/// - private Person? selectedUser → Person? SelectedUser (ObservableProperty)
/// - private double currentRadius → double CurrentRadius (ObservableProperty)
/// - FilterByRadius(double radiusKm) → ChangeRadiusCommand (RelayCommand)
/// - ShowAll() → ShowAllCommand (RelayCommand)
/// </summary>
public partial class MapViewModel : BaseViewModel
{
    private readonly IMockDataService _mockDataService;
    private readonly ILocationService _locationService;
    private readonly ISupabaseService _supabaseService;
    private readonly ScaleService _scaleService;

    // --- Observable Properties ---

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayUsers))]
    [NotifyPropertyChangedFor(nameof(DisplayAssistants))]
    private CenterMode _currentCenterMode = CenterMode.User; // Default: User-centric

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayUsers))]
    private ObservableCollection<Person> _users = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayAssistants))]
    private ObservableCollection<Person> _assistants = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayAssistants))]
    private ObservableCollection<Person> _filteredAssistants = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayUsers))]
    private ObservableCollection<Person> _filteredUsers = new(); // For Assistant-centric mode

    [ObservableProperty]
    private Person? _selectedUser;

    [ObservableProperty]
    private Person? _selectedAssistant;

    [ObservableProperty]
    private double _currentRadius = 3.0; // Default 3km

    [ObservableProperty]
    private string _filterStatusText = "전체 보기";

    [ObservableProperty]
    private int _usersInRadius; // For map overlay

    [ObservableProperty]
    private int _assistantsInRadius; // For map overlay

    [ObservableProperty]
    private Person? _selectedPerson; // For slide-out panel

    [ObservableProperty]
    private bool _isDetailPanelVisible; // Show/hide panel

    // --- Map Styling Settings (from Preferences) ---
    
    [ObservableProperty]
    private string _userMarkerColor = "#60A5FA"; // Blue

    [ObservableProperty]
    private string _assistantMarkerColor = "#FB923C"; // Orange

    [ObservableProperty]
    private string _markerSize = "Medium";

    [ObservableProperty]
    private int _userMarkerBorderWidth = 2;

    [ObservableProperty]
    private int _assistantMarkerBorderWidth = 2;

    [ObservableProperty]
    private string _userMarkerShape = "Circle";

    [ObservableProperty]
    private string _assistantMarkerShape = "Square";

    [ObservableProperty]
    private string _highlightColor = "#FFFFFF";

    [ObservableProperty]
    private int _highlightBorderWidth = 3;

    [ObservableProperty]
    private double _highlightSizeMultiplier = 2.0;

    [ObservableProperty]
    private string _circleColor = "#60A5FA"; // Blue

    [ObservableProperty]
    private double _circleOpacity = 0.3;

    [ObservableProperty]
    private int _circleBorderWidth = 2;

    // UI Scale
    [ObservableProperty]
    private double _uiScale = 1.0; // 0.8 ~ 1.5

    // --- Computed Display Properties ---
    // These switch based on current mode for proper list display

    /// <summary>
    /// Users to display in list: FilteredUsers (with distance) in Assistant mode, all Users in User mode.
    /// </summary>
    public ObservableCollection<Person> DisplayUsers => 
        CurrentCenterMode == CenterMode.Assistant ? FilteredUsers : Users;

    /// <summary>
    /// Assistants to display in list: FilteredAssistants (with distance) in User mode, all Assistants in Assistant mode.
    /// </summary>
    public ObservableCollection<Person> DisplayAssistants => 
        CurrentCenterMode == CenterMode.User ? FilteredAssistants : Assistants;

    // --- Constructor ---

    /// <summary>
    /// Constructor with DI.
    /// </summary>
    public MapViewModel(
        IMockDataService mockDataService, 
        ILocationService locationService,
        ISupabaseService supabaseService,
        ScaleService scaleService)
    {
        _mockDataService = mockDataService;
        _locationService = locationService;
        _supabaseService = supabaseService;
        _scaleService = scaleService;
        UiScale = _scaleService.CurrentScale;
        
        // Load settings from Preferences
        LoadSettings();
        
        // Subscribe to settings changes
        WeakReferenceMessenger.Default.Register<SettingsChangedMessage>(this, (r, m) =>
        {
            MauiProgram.Log("[MapViewModel] SettingsChangedMessage received, reloading settings");
            LoadSettings();
        });

        _scaleService.ScaleChanged += OnScaleChanged;
    }

    // --- Commands ---
    
    /// <summary>
    /// Load map styling settings from Preferences.
    /// </summary>
    private void LoadSettings()
    {
        MauiProgram.Log("[MapViewModel] LoadSettings START");
        
        try
        {
            UserMarkerColor = Preferences.Default.Get("UserMarkerColor", "#60A5FA");
            AssistantMarkerColor = Preferences.Default.Get("AssistantMarkerColor", "#FB923C");
            MarkerSize = Preferences.Default.Get("MarkerSize", "Medium");
            UserMarkerBorderWidth = Preferences.Default.Get("UserMarkerBorderWidth", 2);
            AssistantMarkerBorderWidth = Preferences.Default.Get("AssistantMarkerBorderWidth", 2);
            UserMarkerShape = Preferences.Default.Get("UserMarkerShape", "Circle");
            AssistantMarkerShape = Preferences.Default.Get("AssistantMarkerShape", "Square");
            
            HighlightColor = Preferences.Default.Get("HighlightColor", "#FFFFFF");
            HighlightBorderWidth = Preferences.Default.Get("HighlightBorderWidth", 3);
            HighlightSizeMultiplier = Preferences.Default.Get("HighlightSizeMultiplier", 2.0);
            
            CircleColor = Preferences.Default.Get("CircleColor", "#60A5FA");
            CircleOpacity = Preferences.Default.Get("CircleOpacity", 0.3);
            CircleBorderWidth = Preferences.Default.Get("CircleBorderWidth", 2);
            
            MauiProgram.Log($"[MapViewModel] Settings loaded: UserMarker={UserMarkerColor}, Circle={CircleColor}");
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[MapViewModel] LoadSettings FAILED: {ex.Message}");
        }
    }

    /// <summary>
    /// Load users and assistants from Supabase (with Mock fallback).
    /// </summary>
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            MauiProgram.Log("[MapViewModel] LoadDataAsync START");
            
            List<Person> users;
            List<Person> assistants;

            try
            {
                // Try loading from Supabase
                MauiProgram.Log("[MapViewModel] Attempting Supabase connection...");
                await _supabaseService.InitializeAsync();
                
                users = await _supabaseService.GetUsersAsync();
                assistants = await _supabaseService.GetAssistantsAsync();
                
                MauiProgram.Log($"[MapViewModel] Supabase SUCCESS: {users.Count} users, {assistants.Count} assistants");
            }
            catch (Exception ex)
            {
                // Fallback to Mock data
                MauiProgram.Log($"[MapViewModel] Supabase FAILED: {ex.Message}");
                MauiProgram.Log("[MapViewModel] Falling back to Mock data...");
                
                users = await _mockDataService.GetUsersAsync();
                assistants = await _mockDataService.GetAssistantsAsync();
                
                MauiProgram.Log($"[MapViewModel] Mock data loaded: {users.Count} users, {assistants.Count} assistants");
            }

            Users = new ObservableCollection<Person>(users);
            Assistants = new ObservableCollection<Person>(assistants);
            FilteredAssistants = new ObservableCollection<Person>(assistants);
            FilteredUsers = new ObservableCollection<Person>(users);

            // Initialize counts
            UsersInRadius = users.Count;
            AssistantsInRadius = assistants.Count;

            UpdateFilterStatusText();
            
            MauiProgram.Log("[MapViewModel] LoadDataAsync END");
        });
    }

    /// <summary>
    /// Toggle between User-centric and Assistant-centric mode.
    /// </summary>
    [RelayCommand]
    private void ToggleCenterMode()
    {
        CurrentCenterMode = CurrentCenterMode == CenterMode.User 
            ? CenterMode.Assistant 
            : CenterMode.User;

        // Clear selections when switching mode
        if (CurrentCenterMode == CenterMode.User)
        {
            SelectedAssistant = null;
            FilteredUsers = new ObservableCollection<Person>(Users);
        }
        else
        {
            SelectedUser = null;
            FilteredAssistants = new ObservableCollection<Person>(Assistants);
        }

        UpdateFilterStatusText();
    }

    /// <summary>
    /// Select a user (e.g., from marker click or list tap).
    /// Behavior depends on current center mode.
    /// </summary>
    [RelayCommand]
    private void SelectUser(Person? user)
    {
        SelectedUser = user;

        if (SelectedUser != null && CurrentCenterMode == CenterMode.User)
        {
            // User-centric mode: Filter assistants by radius
            FilterAssistantsByRadius();
        }
    }

    /// <summary>
    /// Select an assistant (e.g., from marker click or list tap).
    /// Behavior depends on current center mode.
    /// </summary>
    [RelayCommand]
    private void SelectAssistant(Person? assistant)
    {
        SelectedAssistant = assistant;

        if (SelectedAssistant != null && CurrentCenterMode == CenterMode.Assistant)
        {
            // Assistant-centric mode: Filter users by radius
            FilterUsersByRadius();
        }
    }

    /// <summary>
    /// Change radius (1km/3km/5km) and re-filter.
    /// </summary>
    [RelayCommand]
    private void ChangeRadius(double radiusKm)
    {
        CurrentRadius = radiusKm;

        if (CurrentCenterMode == CenterMode.User && SelectedUser != null)
        {
            FilterAssistantsByRadius();
        }
        else if (CurrentCenterMode == CenterMode.Assistant && SelectedAssistant != null)
        {
            FilterUsersByRadius();
        }
        else
        {
            // No selection, select first person automatically
            if (CurrentCenterMode == CenterMode.User)
            {
                var firstUser = Users.FirstOrDefault();
                if (firstUser != null) SelectUser(firstUser);
            }
            else
            {
                var firstAssistant = Assistants.FirstOrDefault();
                if (firstAssistant != null) SelectAssistant(firstAssistant);
            }
        }
    }

    /// <summary>
    /// Show all persons (clear filter).
    /// </summary>
    [RelayCommand]
    private void ShowAll()
    {
        CurrentRadius = 0; // 0 = show all
        SelectedUser = null;
        SelectedAssistant = null;
        FilteredAssistants = new ObservableCollection<Person>(Assistants);
        FilteredUsers = new ObservableCollection<Person>(Users);
        UsersInRadius = Users.Count;
        AssistantsInRadius = Assistants.Count;
        UpdateFilterStatusText();
    }

    // --- Private Methods ---

    /// <summary>
    /// Filter assistants by current radius from selected user.
    /// Sorts by distance (ascending).
    /// </summary>
    private void FilterAssistantsByRadius()
    {
        if (SelectedUser == null)
        {
            FilteredAssistants = new ObservableCollection<Person>(Assistants);
            AssistantsInRadius = Assistants.Count;
            UsersInRadius = Users.Count;
            UpdateFilterStatusText();
            return;
        }

        if (CurrentRadius <= 0)
        {
            // Show all
            FilteredAssistants = new ObservableCollection<Person>(Assistants);
            // Counts will be updated by JavaScript in MainPage.xaml.cs
        }
        else
        {
            // Filter by radius
            var filtered = _locationService.FilterByRadius(
                SelectedUser,
                Assistants.ToList(),
                CurrentRadius
            );

            FilteredAssistants = new ObservableCollection<Person>(filtered);
            // Counts will be updated by JavaScript in MainPage.xaml.cs
        }

        UpdateFilterStatusText();
    }

    /// <summary>
    /// Filter users by current radius from selected assistant.
    /// Sorts by distance (ascending).
    /// (Reverse filtering for Assistant-centric mode)
    /// </summary>
    private void FilterUsersByRadius()
    {
        if (SelectedAssistant == null)
        {
            FilteredUsers = new ObservableCollection<Person>(Users);
            UsersInRadius = Users.Count;
            AssistantsInRadius = Assistants.Count;
            UpdateFilterStatusText();
            return;
        }

        if (CurrentRadius <= 0)
        {
            // Show all
            FilteredUsers = new ObservableCollection<Person>(Users);
            // Counts will be updated by JavaScript in MainPage.xaml.cs
        }
        else
        {
            // Filter by radius (reverse: Assistant → Users)
            var filtered = _locationService.FilterByRadius(
                SelectedAssistant,
                Users.ToList(),
                CurrentRadius
            );

            FilteredUsers = new ObservableCollection<Person>(filtered);
            // Counts will be updated by JavaScript in MainPage.xaml.cs
        }

        UpdateFilterStatusText();
    }

    /// <summary>
    /// Update filter status text (e.g., "1km 내 12명", "전체 20명").
    /// </summary>
    private void UpdateFilterStatusText()
    {
        if (CurrentCenterMode == CenterMode.User)
        {
            // User-centric mode
            if (SelectedUser == null || CurrentRadius <= 0)
            {
                FilterStatusText = $"전체 {FilteredAssistants.Count}명";
            }
            else
            {
                FilterStatusText = $"{CurrentRadius}km 내 {FilteredAssistants.Count}명";
            }
        }
        else
        {
            // Assistant-centric mode
            if (SelectedAssistant == null || CurrentRadius <= 0)
            {
                FilterStatusText = $"전체 {FilteredUsers.Count}명";
            }
            else
            {
                FilterStatusText = $"{CurrentRadius}km 내 {FilteredUsers.Count}명";
            }
        }
    }

    // --- Detail Panel Commands ---

    /// <summary>
    /// Show detail panel for a person (from marker click or list click).
    /// Also selects the person and triggers radius filtering.
    /// </summary>
    [RelayCommand]
    private void ShowPersonDetail(Person? person)
    {
        if (person == null)
        {
            return;
        }

        MauiProgram.Log($"[MapViewModel] ShowPersonDetail: {person.Name} ({person.Type})");
        
        // Select person and trigger filtering based on type and mode
        if (person.Type == PersonType.User)
        {
            SelectedUser = person;
            
            // If in User-centric mode, filter assistants by radius
            if (CurrentCenterMode == CenterMode.User)
            {
                FilterAssistantsByRadius();
            }
        }
        else // Assistant
        {
            SelectedAssistant = person;
            
            // If in Assistant-centric mode, filter users by radius
            if (CurrentCenterMode == CenterMode.Assistant)
            {
                FilterUsersByRadius();
            }
        }
        
        SelectedPerson = person;
        IsDetailPanelVisible = true;
    }

    /// <summary>
    /// Close the detail panel.
    /// </summary>
    [RelayCommand]
    private void CloseDetailPanel()
    {
        MauiProgram.Log("[MapViewModel] CloseDetailPanel");
        IsDetailPanelVisible = false;
        SelectedPerson = null;
    }

    /// <summary>
    /// Navigate to ManagementPage to edit person details.
    /// </summary>
    [RelayCommand]
    private async Task EditPersonDetailsAsync()
    {
        if (SelectedPerson == null)
        {
            return;
        }

        MauiProgram.Log($"[MapViewModel] EditPersonDetails: {SelectedPerson.Name}");
        
        // Navigate directly to edit page with person ID
        await Shell.Current.GoToAsync($"PersonEditPage?PersonId={SelectedPerson.Id}");
        
        // Close panel after navigation
        CloseDetailPanel();
    }

    /// <summary>
    /// Increase UI scale.
    /// </summary>
    [RelayCommand]
    private void IncreaseScale()
    {
        _scaleService.Increase();
        UiScale = _scaleService.CurrentScale;
        MauiProgram.Log($"[MapViewModel] UI Scale increased to: {UiScale:F1}");
    }

    /// <summary>
    /// Decrease UI scale.
    /// </summary>
    [RelayCommand]
    private void DecreaseScale()
    {
        _scaleService.Decrease();
        UiScale = _scaleService.CurrentScale;
        MauiProgram.Log($"[MapViewModel] UI Scale decreased to: {UiScale:F1}");
    }

    /// <summary>
    /// Reset UI scale to 100%.
    /// </summary>
    [RelayCommand]
    private void ResetScale()
    {
        _scaleService.Reset();
        UiScale = _scaleService.CurrentScale;
        MauiProgram.Log("[MapViewModel] UI Scale reset to 1.0");
    }

    private void OnScaleChanged(object? sender, double scale)
    {
        UiScale = scale;
    }
}
