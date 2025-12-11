#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;

namespace LmpLink.MAUI.Models;

/// <summary>
/// UI wrapper for ManagementPage selection + display.
/// Keeps the domain model (Person) immutable while providing observable UI state.
/// </summary>
public sealed partial class SelectablePerson : ObservableObject
{
    public SelectablePerson(Person person)
    {
        Person = person;
    }

    public Person Person { get; }

    public int Id => Person.Id;
    public string Name => Person.Name;
    public PersonType Type => Person.Type;
    public double Latitude => Person.Latitude;
    public double Longitude => Person.Longitude;
    public string Address => Person.Address;
    public string? Phone => Person.Phone;
    public string? Gender => Person.Gender;
    public bool HasVehicle => Person.HasVehicle;
    public string? AvailableTimeSlots => Person.AvailableTimeSlots;

    // Alias used by existing XAML
    public int? ExperienceYears => Person.Experience;

    [ObservableProperty]
    private bool _isSelected;
}


