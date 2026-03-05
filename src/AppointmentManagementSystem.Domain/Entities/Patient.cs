using AppointmentManagementSystem.Domain.ValueObjects;

namespace AppointmentManagementSystem.Domain.Entities;

public sealed class Patient
{
    // used to avoid accidental modification of domain state
    private readonly List<PatientContact> contacts = [];
    private readonly List<PatientNote> notes = [];
    private readonly List<Appointment> appointments = [];

    public Guid Id { get; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; }

    public IReadOnlyCollection<PatientContact> Contacts => contacts;
    public IReadOnlyCollection<PatientNote> Notes => notes;
    public IReadOnlyCollection<Appointment> Appointments => appointments;

    public Patient(Guid id, string firstName, string lastName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(id));
        }

        SetName(firstName, lastName);
        Id = id;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void SetName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name is required.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name is required.", nameof(lastName));
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public void AddContact(PatientContact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);

        if (contacts.Contains(contact))
        {
            return;
        }

        contacts.Add(contact);
    }

    public bool RemoveContact(PatientContact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);
        return contacts.Remove(contact);
    }

    public void AddNote(PatientNote note)
    {
        ArgumentNullException.ThrowIfNull(note);
        notes.Add(note);
    }

    public void AddAppointment(Appointment appointment)
    {
        ArgumentNullException.ThrowIfNull(appointment);
        appointments.Add(appointment);
    }
}

