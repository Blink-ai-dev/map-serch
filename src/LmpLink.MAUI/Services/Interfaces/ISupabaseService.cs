namespace LmpLink.MAUI.Services.Interfaces;

/// <summary>
/// Supabase database operations for users & assistants (separate tables)
/// </summary>
public interface ISupabaseService
{
    /// <summary>
    /// Initialize Supabase client
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Get users only
    /// </summary>
    Task<List<Person>> GetUsersAsync();

    /// <summary>
    /// Get assistants only
    /// </summary>
    Task<List<Person>> GetAssistantsAsync();

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<Person?> GetUserByIdAsync(int id);

    /// <summary>
    /// Get assistant by ID
    /// </summary>
    Task<Person?> GetAssistantByIdAsync(int id);

    /// <summary>
    /// Create a new user
    /// </summary>
    Task<Person> CreateUserAsync(Person person);

    /// <summary>
    /// Update an existing user
    /// </summary>
    Task<Person> UpdateUserAsync(Person person);

    /// <summary>
    /// Create a new assistant
    /// </summary>
    Task<Person> CreateAssistantAsync(Person person);

    /// <summary>
    /// Update an existing assistant
    /// </summary>
    Task<Person> UpdateAssistantAsync(Person person);

    /// <summary>
    /// Delete a user
    /// </summary>
    Task<bool> DeleteUserAsync(int id);

    /// <summary>
    /// Delete an assistant
    /// </summary>
    Task<bool> DeleteAssistantAsync(int id);
}







