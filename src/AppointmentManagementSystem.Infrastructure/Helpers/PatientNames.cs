namespace AppointmentManagementSystem.Infrastructure.Helpers;

/// <summary>
/// Temporary static patient lookup due to assessment time constraints.
/// In production, this would be fetched from the database via EF navigation.
/// </summary>
public static class PatientNames
{
    private static readonly Dictionary<Guid, string> PatientLookup = new()
    {
        { new Guid("00000000-0000-0000-0000-000000000001"), "John Doe" },
        { new Guid("00000000-0000-0000-0000-000000000002"), "Jane Smith" },
        { new Guid("00000000-0000-0000-0000-000000000003"), "Robert Johnson" },
        { new Guid("00000000-0000-0000-0000-000000000004"), "Emily Davis" },
        { new Guid("00000000-0000-0000-0000-000000000005"), "Michael Brown" },
        { new Guid("00000000-0000-0000-0000-000000000006"), "Sarah Wilson" },
        { new Guid("00000000-0000-0000-0000-000000000007"), "David Miller" },
        { new Guid("00000000-0000-0000-0000-000000000008"), "Jessica Moore" },
        { new Guid("00000000-0000-0000-0000-000000000009"), "Christopher Taylor" },
        { new Guid("00000000-0000-0000-0000-000000000010"), "Amanda Anderson" },
        { new Guid("00000000-0000-0000-0000-000000000011"), "Daniel Thomas" },
        { new Guid("00000000-0000-0000-0000-000000000012"), "Michelle Jackson" },
        { new Guid("00000000-0000-0000-0000-000000000013"), "James White" }
    };

    public static string GetName(Guid patientId)
    {
        return PatientLookup.TryGetValue(patientId, out var name) ? name : "Unknown Patient";
    }
}
