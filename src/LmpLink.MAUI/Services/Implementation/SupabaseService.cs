using Supabase;
using Postgrest.Attributes;
using Postgrest.Models;
using Microsoft.Extensions.Configuration;

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

    public async Task<List<Person>> GetPersonsAsync()
    {
        if (_client == null)
        {
            await InitializeAsync();
        }

        try
        {
            MauiProgram.Log("[SupabaseService] Fetching all persons...");

            var response = await _client!.From<PersonDto>().Get();
            var persons = response.Models.Select(MapToPerson).ToList();

            MauiProgram.Log($"[SupabaseService] Fetched {persons.Count} persons");
            return persons;
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] GetPersonsAsync failed: {ex.Message}");
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

            var users = response.Models.Select(MapToPerson).ToList();

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

            var assistants = response.Models.Select(MapToPerson).ToList();

            MauiProgram.Log($"[SupabaseService] Fetched {assistants.Count} assistants");
            return assistants;
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] GetAssistantsAsync failed: {ex.Message}");
            throw;
        }
    }

    public async Task<Person?> GetPersonByIdAsync(int id)
    {
        if (_client == null)
        {
            await InitializeAsync();
        }

        try
        {
            MauiProgram.Log($"[SupabaseService] Fetching person ID {id}...");

            var response = await _client!.From<PersonDto>()
                .Where(p => p.Id == id)
                .Single();

            if (response == null)
            {
                MauiProgram.Log($"[SupabaseService] Person ID {id} not found");
                return null;
            }

            return MapToPerson(response);
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] GetPersonByIdAsync failed: {ex.Message}");
            return null;
        }
    }

    public async Task<Person> CreatePersonAsync(Person person)
    {
        if (_client == null)
        {
            await InitializeAsync();
        }

        try
        {
            MauiProgram.Log($"[SupabaseService] Creating person: {person.Name}");

            var dto = MapToDto(person);
            dto.Id = 0; // Ensure ID is 0 for new records
            
            var response = await _client!.From<PersonDto>().Insert(dto);

            if (response?.Models?.FirstOrDefault() == null)
            {
                throw new InvalidOperationException("Failed to create person: No response from server");
            }

            var createdPerson = MapToPerson(response.Models.First());
            MauiProgram.Log($"[SupabaseService] Created person ID {createdPerson.Id}");
            return createdPerson;
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] CreatePersonAsync failed: {ex.Message}");
            throw;
        }
    }

    public async Task<Person> UpdatePersonAsync(Person person)
    {
        if (_client == null)
        {
            await InitializeAsync();
        }

        try
        {
            MauiProgram.Log($"[SupabaseService] Updating person ID {person.Id}: {person.Name}");

            var dto = MapToDto(person);
            
            var response = await _client!.From<PersonDto>().Update(dto);

            if (response?.Models?.FirstOrDefault() == null)
            {
                throw new InvalidOperationException("Failed to update person: No response from server");
            }

            var updatedPerson = MapToPerson(response.Models.First());
            MauiProgram.Log($"[SupabaseService] Updated person ID {updatedPerson.Id}");
            return updatedPerson;
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] UpdatePersonAsync failed: {ex.Message}");
            throw;
        }
    }

    public async Task<Person?> UpsertPersonAsync(Person person)
    {
        if (_client == null)
        {
            await InitializeAsync();
        }

        try
        {
            MauiProgram.Log($"[SupabaseService] Upserting person ID {person.Id}...");

            var dto = MapToDto(person);
            var response = await _client!.From<PersonDto>().Upsert(dto);

            if (response?.Models?.FirstOrDefault() == null)
            {
                MauiProgram.Log("[SupabaseService] Upsert failed");
                return null;
            }

            MauiProgram.Log($"[SupabaseService] Upserted person ID {person.Id}");
            return MapToPerson(response.Models.First());
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] UpsertPersonAsync failed: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeletePersonAsync(int id)
    {
        if (_client == null)
        {
            await InitializeAsync();
        }

        try
        {
            MauiProgram.Log($"[SupabaseService] Deleting person ID {id}...");

            await _client!.From<PersonDto>()
                .Where(p => p.Id == id)
                .Delete();

            MauiProgram.Log($"[SupabaseService] Deleted person ID {id}");
            return true;
        }
        catch (Exception ex)
        {
            MauiProgram.Log($"[SupabaseService] DeletePersonAsync failed: {ex.Message}");
            return false;
        }
    }

    // ========== Mapping Methods ==========

    private static Person MapToPerson(PersonDto dto)
    {
        return new Person(
            Id: dto.Id,
            Name: dto.Name ?? "Unknown",
            Type: Enum.TryParse<PersonType>(dto.Type, true, out var type) ? type : PersonType.User,
            Latitude: dto.Latitude,
            Longitude: dto.Longitude,
            Address: dto.Address ?? "",
            Phone: dto.Phone,
            Gender: dto.Gender,
            HasVehicle: dto.HasVehicle,
            AvailableTimeSlots: null, // TODO: Parse from JSONB
            Experience: dto.ExperienceYears
        )
        {
            DistanceKm = null
        };
    }

    private static PersonDto MapToDto(Person person)
    {
        return new PersonDto
        {
            Id = person.Id,
            Name = person.Name,
            Type = person.Type.ToString().ToLower(),
            Latitude = person.Latitude,
            Longitude = person.Longitude,
            Address = person.Address,
            Phone = person.Phone,
            Gender = person.Gender,
            HasVehicle = person.HasVehicle,
            ExperienceYears = person.Experience
        };
    }
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

