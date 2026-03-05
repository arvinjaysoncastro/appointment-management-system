namespace AppointmentManagementSystem.Domain.Entities;

public sealed class Appointment
{
    public Guid Id { get; }
    public Guid PatientId { get; }
    public string Title { get; private set; } = string.Empty;
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public Appointment(
        Guid id,
        Guid patientId,
        string title,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        string? notes = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Appointment id is required.", nameof(id));
        }

        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        Id = id;
        PatientId = patientId;

        SetTitle(title);
        SetTimeRange(startTime, endTime);
        SetNotes(notes);
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        Title = title.Trim();
    }

    public void SetNotes(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void SetTimeRange(DateTimeOffset startTime, DateTimeOffset endTime)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("End time must be after start time.");
        }

        StartTime = startTime;
        EndTime = endTime;
    }

    public bool OverlapsWith(Appointment other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // Overlap rule: [Start, End) intersects [other.Start, other.End)
        return StartTime < other.EndTime && other.StartTime < EndTime;
    }
}

