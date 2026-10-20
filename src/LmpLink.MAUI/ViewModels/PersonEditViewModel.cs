#nullable enable

using LmpLink.MAUI.ViewModels.Base;
using LmpLink.MAUI.Models;
using System.Globalization;

namespace LmpLink.MAUI.ViewModels;

/// <summary>
/// ViewModel for PersonEditPage (Create/Update person).
/// </summary>
public partial class PersonEditViewModel : BaseViewModel
{
    private readonly ISupabaseService _supabaseService;
    private readonly int? _personId; // null for Create, ID for Update

    // --- Observable Properties ---

    [ObservableProperty]
    private string _pageTitle = "신규 등록";

    [ObservableProperty]
    private string _saveButtonText = "등록";

    [ObservableProperty]
    private string _selectedType = "User"; // "User" or "Assistant"

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private string _latitude = string.Empty;

    [ObservableProperty]
    private string _longitude = string.Empty;

    [ObservableProperty]
    private string? _gender; // "male", "female", or null

    [ObservableProperty]
    private bool _hasVehicle;

    [ObservableProperty]
    private string _experience = string.Empty; // Store as string for Entry binding

    public bool IsAssistant => SelectedType == "Assistant";

    // --- Constructors ---

    /// <summary>
    /// Constructor for Create mode (new person).
    /// </summary>
    public PersonEditViewModel(ISupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
        _personId = null;
        PageTitle = "신규 등록";
        SaveButtonText = "등록";
        MauiProgram.Log("[PersonEditViewModel] Create mode");
    }

    /// <summary>
    /// Constructor for Update mode (edit existing person).
    /// </summary>
    public PersonEditViewModel(ISupabaseService supabaseService, Person person)
    {
        _supabaseService = supabaseService;
        _personId = person.Id;
        PageTitle = "정보 수정";
        SaveButtonText = "저장";

        // Load person data
        SelectedType = person.Type.ToString();
        Name = person.Name;
        Phone = person.Phone ?? string.Empty;
        Address = person.Address;
        Latitude = person.Latitude.ToString(CultureInfo.InvariantCulture);
        Longitude = person.Longitude.ToString(CultureInfo.InvariantCulture);
        Gender = person.Gender;
        HasVehicle = person.HasVehicle;
        Experience = person.Experience?.ToString() ?? string.Empty;

        MauiProgram.Log($"[PersonEditViewModel] Update mode: {person.Name}");
    }

    // --- Commands ---

    /// <summary>
    /// Select person type (User or Assistant).
    /// </summary>
    [RelayCommand]
    private void SelectType(string type)
    {
        MauiProgram.Log($"[PersonEditViewModel] SelectType: {type}");
        SelectedType = type;
        OnPropertyChanged(nameof(IsAssistant));
    }

    /// <summary>
    /// Select gender.
    /// </summary>
    [RelayCommand]
    private void SelectGender(string gender)
    {
        MauiProgram.Log($"[PersonEditViewModel] SelectGender: {gender}");
        Gender = gender;
    }

    /// <summary>
    /// Search address (placeholder - would use geocoding API).
    /// </summary>
    [RelayCommand]
    private async Task SearchAddressAsync()
    {
        await Shell.Current.DisplayAlertAsync(
            "주소 검색",
            "주소 검색 기능은 추후 구현 예정입니다.\n직접 주소와 좌표를 입력해 주세요.",
            "확인");
    }

    /// <summary>
    /// Save person (Create or Update).
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        await ExecuteAsync(async () =>
        {
            MauiProgram.Log("[PersonEditViewModel] SaveAsync START");

            // Validation
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlertAsync("오류", "이름을 입력하세요.", "확인");
                return;
            }

            if (string.IsNullOrWhiteSpace(Address))
            {
                await Shell.Current.DisplayAlertAsync("오류", "주소를 입력하세요.", "확인");
                return;
            }

            if (!double.TryParse(Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat))
            {
                await Shell.Current.DisplayAlertAsync("오류", "올바른 위도를 입력하세요.", "확인");
                return;
            }

            if (!double.TryParse(Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
            {
                await Shell.Current.DisplayAlertAsync("오류", "올바른 경도를 입력하세요.", "확인");
                return;
            }

            // Parse experience (optional)
            int? experienceValue = null;
            if (!string.IsNullOrWhiteSpace(Experience))
            {
                if (int.TryParse(Experience, out int exp))
                {
                    experienceValue = exp;
                }
            }

            try
            {
                await _supabaseService.InitializeAsync();

                if (_personId == null)
                {
                    // Create new person
                    MauiProgram.Log($"[PersonEditViewModel] Creating new person: {Name}");

                    if (IsAssistant)
                    {
                        var newAssistant = BuildPerson(PersonType.Assistant, lat, lon, experienceValue);
                        await _supabaseService.CreateAssistantAsync(newAssistant);
                    }
                    else
                    {
                        var newUser = BuildPerson(PersonType.User, lat, lon, null);
                        await _supabaseService.CreateUserAsync(newUser);
                    }
                    await Shell.Current.DisplayAlertAsync("성공", $"{Name}님이 등록되었습니다.", "확인");
                }
                else
                {
                    // Update existing person
                    MauiProgram.Log($"[PersonEditViewModel] Updating person ID: {_personId}");

                    if (IsAssistant)
                    {
                        var updatedAssistant = BuildPerson(PersonType.Assistant, lat, lon, experienceValue, _personId.Value);
                        await _supabaseService.UpdateAssistantAsync(updatedAssistant);
                    }
                    else
                    {
                        var updatedUser = BuildPerson(PersonType.User, lat, lon, null, _personId.Value);
                        await _supabaseService.UpdateUserAsync(updatedUser);
                    }
                    await Shell.Current.DisplayAlertAsync("성공", $"{Name}님의 정보가 수정되었습니다.", "확인");
                }

                // Navigate back to ManagementPage
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                MauiProgram.Log($"[PersonEditViewModel] Save FAILED: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("오류", $"저장 실패:\n{ex.Message}", "확인");
            }
        });
    }

    /// <summary>
    /// Cancel and go back.
    /// </summary>
    [RelayCommand]
    private async Task CancelAsync()
    {
        var confirm = await Shell.Current.DisplayAlertAsync(
            "확인",
            "작성 중인 내용이 저장되지 않습니다. 취소하시겠습니까?",
            "예",
            "아니오");

        if (confirm)
        {
            await Shell.Current.GoToAsync("..");
        }
    }

    private Person BuildPerson(PersonType type, double lat, double lon, int? experience, int id = 0)
    {
        return new Person(
            Id: id,
            Name: Name,
            Type: type,
            Latitude: lat,
            Longitude: lon,
            Address: Address,
            Phone: string.IsNullOrWhiteSpace(Phone) ? null : Phone,
            Gender: Gender,
            HasVehicle: type == PersonType.Assistant && HasVehicle,
            AvailableTimeSlots: null,
            Experience: type == PersonType.Assistant ? experience : null
        );
    }
}

