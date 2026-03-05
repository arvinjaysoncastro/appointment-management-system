namespace AppointmentManagementSystem.Domain.Entities;

public sealed class PatientNote
{
    public Guid Id { get; }
    public Guid PatientId { get; }
    public string Text { get; }
    public DateTimeOffset CreatedAt { get; }

    public PatientNote(Guid id, Guid patientId, string text)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Note id is required.", nameof(id));
        }

        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Note text is required.", nameof(text));
        }

        Id = id;
        PatientId = patientId;
        Text = text.Trim();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    private PatientNote() { }
}

