namespace LmpLink.MAUI.Views.Pages;

[QueryProperty(nameof(PersonId), "PersonId")]
public partial class PersonEditPage : ContentPage
{
    private readonly ISupabaseService _supabaseService;
    private int? _personId;

    public string? PersonId
    {
        set
        {
            if (int.TryParse(value, out int id))
            {
                _personId = id;
                LoadPersonAsync(id);
            }
        }
    }

    public PersonEditPage(ISupabaseService supabaseService)
    {
        InitializeComponent();
        _supabaseService = supabaseService;

        // If no PersonId set, create mode (default ViewModel constructor)
        if (_personId == null)
        {
            BindingContext = new PersonEditViewModel(_supabaseService);
        }
    }

    private async void LoadPersonAsync(int personId)
    {
        try
        {
            MauiProgram.Log($"[PersonEditPage] Loading person ID: {personId}");
            
            await _supabaseService.InitializeAsync();
            var person = await _supabaseService.GetUserByIdAsync(personId)
                         ?? await _supabaseService.GetAssistantByIdAsync(personId);

            if (person != null)
            {
                BindingContext = new PersonEditViewModel(_supabaseService, person);
            }
            else
            {
                await DisplayAlertAsync("오류", "해당 사용자를 찾을 수 없습니다.", "확인");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[PersonEditPage] LoadPersonAsync failed: {ex.Message}");
            await DisplayAlertAsync("오류", $"데이터 로드 실패:\n{ex.Message}", "확인");
            await Shell.Current.GoToAsync("..");
        }
    }
}

