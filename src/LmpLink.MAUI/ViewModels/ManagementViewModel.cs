#nullable enable

using LmpLink.MAUI.ViewModels.Base;
using LmpLink.MAUI.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace LmpLink.MAUI.ViewModels;

/// <summary>
/// ViewModel for ManagementPage (CRUD for users and assistants).
/// </summary>
public partial class ManagementViewModel : BaseViewModel
{
    private readonly IMockDataService _mockDataService;
    private readonly ISupabaseService _supabaseService;

    // --- Observable Properties ---

    [ObservableProperty]
    private ObservableCollection<SelectablePerson> _allPersons = new();

    [ObservableProperty]
    private ObservableCollection<SelectablePerson> _filteredPersons = new();

    [ObservableProperty]
    private SelectablePerson? _selectedPerson;

    [ObservableProperty]
    private ObservableCollection<SelectablePerson> _selectedPersons = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedTypeFilter = "all"; // "all", "user", "assistant" - for UI binding

    // --- Constructor ---

    public ManagementViewModel(
        IMockDataService mockDataService,
        ISupabaseService supabaseService)
    {
        _mockDataService = mockDataService;
        _supabaseService = supabaseService;
        MauiProgram.Log("[ManagementViewModel] Constructor called");

        SelectedPersons.CollectionChanged += OnSelectedPersonsChanged;
    }

    // --- Commands ---

    /// <summary>
    /// Load all persons from Supabase (or mock data).
    /// </summary>
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            MauiProgram.Log("[ManagementViewModel] LoadDataAsync START");
            List<Person> users;
            List<Person> assistants;

            try
            {
                MauiProgram.Log("[ManagementViewModel] Attempting Supabase connection...");
                await _supabaseService.InitializeAsync();
                users = await _supabaseService.GetUsersAsync();
                assistants = await _supabaseService.GetAssistantsAsync();
                MauiProgram.Log($"[ManagementViewModel] Supabase SUCCESS: {users.Count} users, {assistants.Count} assistants");
            }
            catch (Exception ex)
            {
                MauiProgram.Log($"[ManagementViewModel] Supabase FAILED: {ex.Message}. Falling back to Mock data.");
                users = await _mockDataService.GetUsersAsync();
                assistants = await _mockDataService.GetAssistantsAsync();
                MauiProgram.Log($"[ManagementViewModel] Loaded from Mock: {users.Count} users, {assistants.Count} assistants");
            }

            AllPersons.Clear();
            foreach (var user in users)
            {
                AllPersons.Add(new SelectablePerson(user));
            }
            foreach (var assistant in assistants)
            {
                AllPersons.Add(new SelectablePerson(assistant));
            }

            ApplyFilter();
            MauiProgram.Log("[ManagementViewModel] LoadDataAsync END");
        });
    }

    /// <summary>
    /// Apply search and type filter.
    /// </summary>
    [RelayCommand]
    private void ApplyFilter(string? typeFilter = null)
    {
        // Update type filter if provided
        if (!string.IsNullOrEmpty(typeFilter))
        {
            SelectedTypeFilter = typeFilter;
        }

        MauiProgram.Log($"[ManagementViewModel] ApplyFilter: SearchText='{SearchText}', Type='{SelectedTypeFilter}'");

        var filtered = AllPersons.AsEnumerable();

        // Filter by type
        if (SelectedTypeFilter == "user")
        {
            filtered = filtered.Where(p => p.Type == PersonType.User);
        }
        else if (SelectedTypeFilter == "assistant")
        {
            filtered = filtered.Where(p => p.Type == PersonType.Assistant);
        }
        // "all" - no filter

        // Filter by search text (name, address)
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim().ToLower();
            filtered = filtered.Where(p =>
                (p.Name != null && p.Name.ToLower().Contains(search)) ||
                (p.Address != null && p.Address.ToLower().Contains(search)));
        }

        // Reset selection when filtering to keep UX deterministic.
        foreach (var p in AllPersons)
        {
            p.IsSelected = false;
        }
        SelectedPersons.Clear();
        SelectedPerson = null;

        FilteredPersons.Clear();
        foreach (var person in filtered)
        {
            FilteredPersons.Add(person);
        }
        MauiProgram.Log($"[ManagementViewModel] FilteredPersons count: {FilteredPersons.Count}");
    }

    /// <summary>
    /// Select a single person via row tap (clears other selections).
    /// </summary>
    [RelayCommand]
    private void SelectRow(SelectablePerson? person)
    {
        if (person == null)
        {
            return;
        }

        ClearSelections();
        person.IsSelected = true;
        SelectedPersons.Add(person);
        SelectedPerson = person;
        UpdateSelectionState();
    }

    /// <summary>
    /// Toggle selection for a person (checkbox or row tap).
    /// </summary>
    [RelayCommand]
    private void ToggleSelection(SelectablePerson? person)
    {
        if (person == null)
        {
            return;
        }

        person.IsSelected = !person.IsSelected;

        var existing = SelectedPersons.FirstOrDefault(p => p.Id == person.Id);
        if (person.IsSelected)
        {
            if (existing == null)
            {
                SelectedPersons.Add(person);
            }
        }
        else
        {
            if (existing != null)
            {
                SelectedPersons.Remove(existing);
            }
        }

        UpdateSelectionState();
    }

    /// <summary>
    /// Create a new person.
    /// </summary>
    [RelayCommand]
    private async Task CreatePersonAsync()
    {
        MauiProgram.Log("[ManagementViewModel] CreatePersonAsync - Navigating to PersonEditPage");
        await Shell.Current.GoToAsync("PersonEditPage");
    }

    /// <summary>
    /// Edit the single selected person.
    /// </summary>
    [RelayCommand]
    private async Task EditPersonAsync()
    {
        var target = SelectedPersons.Count == 1 ? SelectedPersons.First() : null;
        if (target == null)
        {
            await Shell.Current.DisplayAlertAsync("오류", "편집할 대상을 하나만 선택하세요.", "확인");
            return;
        }

        MauiProgram.Log($"[ManagementViewModel] EditPersonAsync: {target.Name}");
        await Shell.Current.GoToAsync($"PersonEditPage?PersonId={target.Id}");
    }

    /// <summary>
    /// Delete selected persons (single or multiple).
    /// </summary>
    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedPersons.Count == 0)
        {
            await Shell.Current.DisplayAlertAsync("오류", "삭제할 대상을 선택하세요.", "확인");
            return;
        }

        if (_supabaseService == null)
        {
            await Shell.Current.DisplayAlertAsync("오류", "삭제 서비스를 사용할 수 없습니다.", "확인");
            return;
        }

        // 스냅샷을 만들어 중간에 컬렉션이 변경되어도 안전하게 처리
        var targets = SelectedPersons.ToList();
        var targetCount = SelectedPersons.Count;
        var title = targetCount == 1 ? "삭제 확인" : "일괄 삭제 확인";
        var message = targetCount == 1
            ? $"{targets.First().Name}님을 삭제하시겠습니까?"
            : $"{targetCount}명을 삭제하시겠습니까?";

        var confirm = await Shell.Current.DisplayAlertAsync(
            title,
            message,
            "삭제",
            "취소");

        if (!confirm)
        {
            return;
        }

        var ids = targets.Select(p => p.Id).ToList();

        await ExecuteAsync(async () =>
        {
            try
            {
                foreach (var id in ids)
                {
                    await _supabaseService.DeletePersonAsync(id);
                }

                var remaining = AllPersons.Where(p => !ids.Contains(p.Id)).ToList();
                AllPersons = new ObservableCollection<SelectablePerson>(remaining);
                SelectedPersons.Clear();
                ApplyFilter();

                var successMessage = targetCount == 1 ? "삭제되었습니다." : "일괄 삭제되었습니다.";
                await Shell.Current.DisplayAlertAsync("성공", successMessage, "확인");
                MauiProgram.Log($"[ManagementViewModel] Deleted {targetCount} person(s)");
            }
            catch (Exception ex)
            {
                MauiProgram.Log($"[ManagementViewModel] Delete FAILED: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("오류", $"삭제 실패: {ex.Message}", "확인");
            }
        });
    }

    private void OnSelectedPersonsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateSelectionState();
    }

    private void UpdateSelectionState()
    {
        SelectedPerson = SelectedPersons.Count == 1 ? SelectedPersons.First() : null;
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(HasSingleSelection));
    }

    public bool HasSelection => SelectedPersons.Count > 0;
    public bool HasSingleSelection => SelectedPersons.Count == 1;

    private void ClearSelections()
    {
        foreach (var p in AllPersons)
        {
            p.IsSelected = false;
        }
        SelectedPersons.Clear();
        SelectedPerson = null;
    }
}

