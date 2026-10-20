using Supabase;
using Postgrest.Attributes;
using Postgrest.Models;
using Microsoft.Extensions.Configuration;
using LmpLink.MAUI.Models;

namespace LmpLink.MAUI.Services.Implementation;

/// <summary>
/// Supabase database service implementation
/// </summary>
public class SupabaseService : ISupabaseService
{
    private Client? _client;
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;
    private bool _isInitialized;

    public SupabaseService(IConfiguration configuration)
    {
        _supabaseUrl = configuration["SupabaseUrl"] ?? throw new InvalidOperationException("SupabaseUrl not configured. Run: dotnet user-secrets set SupabaseUrl <url>");
        _supabaseKey = configuration["SupabaseKey"] ?? throw new InvalidOperationException("SupabaseKey not configured. Run: dotnet user-secrets set SupabaseKey <key>");
        
        MauiProgram.Log($"[SupabaseService] Constructor called. URL configured: {!string.IsNullOrEmpty(_supabaseUrl)}");
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        try
        {
            MauiProgram.Log("[SupabaseService] Initializing...");

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false // Disable realtime for MVP
            };

            _client = new Client(_supabaseUrl, _supabaseKey, options);
            await _client.InitializeAsync();

            _isInitialized = true;
            MauiProgram.Log("[SupabaseService] Initialized successfully");
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] Initialization failed: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Person>> GetUsersAsync()
    {
        if (_client == null)
        {
            await InitializeAsync();
        }

        try
        {
            MauiProgram.Log("[SupabaseService] Fetching users...");

            var response = await _client!.From<PersonDto>()
                .Where(p => p.Type == "user")
                .Get();

            var users = response.Models.Select(MapPersonDtoToPerson).ToList();

            MauiProgram.Log($"[SupabaseService] Fetched {users.Count} users");
            return users;
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] GetUsersAsync failed: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Person>> GetAssistantsAsync()
    {
        if (_client == null)
        {
            await InitializeAsync();
        }

        try
        {
            MauiProgram.Log("[SupabaseService] Fetching assistants...");

            var response = await _client!.From<PersonDto>()
                .Where(p => p.Type == "assistant")
                .Get();

            var assistants = response.Models.Select(MapPersonDtoToPerson).ToList();

            MauiProgram.Log($"[SupabaseService] Fetched {assistants.Count} assistants");
            return assistants;
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] GetAssistantsAsync failed: {ex.Message}");
            throw;
        }
    }

    public async Task<Person?> GetUserByIdAsync(int id)
    {
        if (_client == null)
        {
            await InitializeAsync();
        }

        try
        {
            MauiProgram.Log($"[SupabaseService] Fetching user ID {id}...");

            var response = await _client!.From<PersonDto>()
                .Where(p => p.Id == id && p.Type == "user")
                .Single();

            if (response == null)
            {
                MauiProgram.Log($"[SupabaseService] User ID {id} not found");
                return null;
            }

            return MapPersonDtoToPerson(response);
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] GetUserByIdAsync failed: {ex.Message}");
            return null;
        }
    }

    public async Task<Person?> GetAssistantByIdAsync(int id)
    {
        if (_client == null)
        {
            await InitializeAsync();
        }

        try
        {
            MauiProgram.Log($"[SupabaseService] Fetching assistant ID {id}...");

            var response = await _client!.From<PersonDto>()
                .Where(p => p.Id == id && p.Type == "assistant")
                .Single();

            if (response == null)
            {
                MauiProgram.Log($"[SupabaseService] Assistant ID {id} not found");
                return null;
            }

            return MapPersonDtoToPerson(response);
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] GetAssistantByIdAsync failed: {ex.Message}");
            return null;
        }
    }

    public async Task<Person> CreateUserAsync(Person person)
    {
        if (_client == null)
        {
            await InitializeAsync();
        }

        try
        {
            MauiProgram.Log($"[SupabaseService] Creating user: {person.Name}");

            var dto = MapToPersonDto(person, "user");
            dto.Id = 0;

            var response = await _client!.From<PersonDto>().Insert(dto);

            if (response?.Models?.FirstOrDefault() == null)
            {
                throw new InvalidOperationException("Failed to create user: No response from server");
            }

            var created = MapPersonDtoToPerson(response.Models.First());
            MauiProgram.Log($"[SupabaseService] Created user ID {created.Id}");
            return created;
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] CreateUserAsync failed: {ex.Message}");
            throw;
        }
    }

    public async Task<Person> UpdateUserAsync(Person person)
    {
        if (_client == null)
        {
            await InitializeAsync();
        }

        try
        {
            MauiProgram.Log($"[SupabaseService] Updating user ID {person.Id}: {person.Name}");

            var dto = MapToPersonDto(person, "user");
            var response = await _client!.From<PersonDto>().Update(dto);

            if (response?.Models?.FirstOrDefault() == null)
            {
                throw new InvalidOperationException("Failed to update user: No response from server");
            }

            var updated = MapPersonDtoToPerson(response.Models.First());
            MauiProgram.Log($"[SupabaseService] Updated user ID {updated.Id}");
            return updated;
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] UpdateUserAsync failed: {ex.Message}");
            throw;
        }
    }

    public async Task<Person> CreateAssistantAsync(Person person)
    {
        if (_client == null)
        {
            await InitializeAsync();
        }

        try
        {
            MauiProgram.Log($"[SupabaseService] Creating assistant: {person.Name}");

            var dto = MapToPersonDto(person, "assistant");
            dto.Id = 0;

            var response = await _client!.From<PersonDto>().Insert(dto);

            if (response?.Models?.FirstOrDefault() == null)
            {
                throw new InvalidOperationException("Failed to create assistant: No response from server");
            }

            var created = MapPersonDtoToPerson(response.Models.First());
            MauiProgram.Log($"[SupabaseService] Created assistant ID {created.Id}");
            return created;
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] CreateAssistantAsync failed: {ex.Message}");
            throw;
        }
    }

    public async Task<Person> UpdateAssistantAsync(Person person)
    {
        if (_client == null)
        {
            await InitializeAsync();
        }

        try
        {
            MauiProgram.Log($"[SupabaseService] Updating assistant ID {person.Id}: {person.Name}");

            var dto = MapToPersonDto(person, "assistant");
            var response = await _client!.From<PersonDto>().Update(dto);

            if (response?.Models?.FirstOrDefault() == null)
            {
                throw new InvalidOperationException("Failed to update assistant: No response from server");
            }

            var updated = MapPersonDtoToPerson(response.Models.First());
            MauiProgram.Log($"[SupabaseService] Updated assistant ID {updated.Id}");
            return updated;
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] UpdateAssistantAsync failed: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        if (_client == null)
        {
            await InitializeAsync();
        }

        try
        {
            MauiProgram.Log($"[SupabaseService] Deleting user ID {id}...");

            await _client!.From<PersonDto>()
                .Where(p => p.Id == id && p.Type == "user")
                .Delete();

            MauiProgram.Log($"[SupabaseService] Deleted user ID {id}");
            return true;
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] DeleteUserAsync failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteAssistantAsync(int id)
    {
        if (_client == null)
        {
            await InitializeAsync();
        }

        try
        {
            MauiProgram.Log($"[SupabaseService] Deleting assistant ID {id}...");

            await _client!.From<PersonDto>()
                .Where(p => p.Id == id && p.Type == "assistant")
                .Delete();

            MauiProgram.Log($"[SupabaseService] Deleted assistant ID {id}");
            return true;
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] DeleteAssistantAsync failed: {ex.Message}");
            return false;
        }
    }

    // ========== Mapping Methods ==========

    private static Person MapPersonDtoToPerson(PersonDto dto)
    {
        var personType = dto.Type == "assistant" ? PersonType.Assistant : PersonType.User;

        return new Person(
            Id: dto.Id,
            Name: dto.Name ?? "Unknown",
            Type: personType,
            Latitude: dto.Latitude,
            Longitude: dto.Longitude,
            Address: dto.Address ?? "",
            Phone: dto.Phone,
            Gender: dto.Gender,
            HasVehicle: dto.HasVehicle,
            AvailableTimeSlots: null, // PersonDto doesn't have this field
            Experience: dto.ExperienceYears
        )
        {
            DistanceKm = null
        };
    }

    private static PersonDto MapToPersonDto(Person person, string type)
    {
        return new PersonDto
        {
            Id = person.Id,
            Name = person.Name,
            Type = type,
            Email = null, // Not available in Person model
            Phone = person.Phone,
            Address = person.Address,
            Latitude = person.Latitude,
            Longitude = person.Longitude,
            Gender = person.Gender,
            HasVehicle = person.HasVehicle,
            ExperienceYears = person.Experience
        };
    }
}

// ========== DTOs ==========

[Table("users")]
public class UserDto : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("latitude")]
    public double Latitude { get; set; }

    [Column("longitude")]
    public double Longitude { get; set; }

    [Column("gender")]
    public string? Gender { get; set; }

    [Column("preferred_time_slots")]
    public string? PreferredTimeSlots { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
}

[Table("assistants")]
public class AssistantDto : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("latitude")]
    public double Latitude { get; set; }

    [Column("longitude")]
    public double Longitude { get; set; }

    [Column("gender")]
    public string? Gender { get; set; }

    [Column("has_vehicle")]
    public bool HasVehicle { get; set; }

    [Column("experience_years")]
    public int? ExperienceYears { get; set; }

    [Column("preferred_time_slots")]
    public string? PreferredTimeSlots { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
}
/// <summary>
/// Supabase persons table DTO
/// </summary>
[Table("persons")]
public class PersonDto : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("type")]
    public string Type { get; set; } = "user";

    [Column("email")]
    public string? Email { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("latitude")]
    public double Latitude { get; set; }

    [Column("longitude")]
    public double Longitude { get; set; }

    [Column("gender")]
    public string? Gender { get; set; }

    [Column("has_vehicle")]
    public bool HasVehicle { get; set; }

    [Column("experience_years")]
    public int? ExperienceYears { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
}

