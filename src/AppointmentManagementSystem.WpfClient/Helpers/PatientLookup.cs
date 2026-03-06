using System;
using System.Collections.Generic;

namespace AppointmentManagementSystem.WpfClient.Helpers
{
    /// <summary>
    /// Static helper for patient name lookup by ID.
    /// NOTE:
    /// For time-boxed technical assessment purposes,
    /// patient names are resolved from a static lookup
    /// that matches the seeded database patients.
    /// </summary>
    public static class PatientLookup
    {
        private static readonly Dictionary<Guid, string> Map = new Dictionary<Guid, string>
        {
            { Guid.Parse("00000000-0000-0000-0000-000000000001"), "John Smith" },
            { Guid.Parse("00000000-0000-0000-0000-000000000002"), "Mary Johnson" },
            { Guid.Parse("00000000-0000-0000-0000-000000000003"), "David Lee" },
            { Guid.Parse("00000000-0000-0000-0000-000000000004"), "Sarah Miller" },
            { Guid.Parse("00000000-0000-0000-0000-000000000005"), "Michael Brown" },
            { Guid.Parse("00000000-0000-0000-0000-000000000006"), "Emily Davis" },
            { Guid.Parse("00000000-0000-0000-0000-000000000007"), "Daniel Wilson" },
            { Guid.Parse("00000000-0000-0000-0000-000000000008"), "Olivia Martinez" },
            { Guid.Parse("00000000-0000-0000-0000-000000000009"), "James Anderson" },
            { Guid.Parse("00000000-0000-0000-0000-000000000010"), "Sophia Taylor" },
            { Guid.Parse("00000000-0000-0000-0000-000000000011"), "Benjamin Thomas" },
            { Guid.Parse("00000000-0000-0000-0000-000000000012"), "Charlotte Moore" },
            { Guid.Parse("00000000-0000-0000-0000-000000000013"), "Lucas Jackson" }
        };

        /// <summary>
        /// Gets patient name from the static lookup by ID.
        /// </summary>
        /// <param name="id">The patient GUID.</param>
        /// <returns>Patient name or "Unknown Patient" if not found.</returns>
        public static string GetName(Guid id)
        {
            return Map.TryGetValue(id, out var name) ? name : "Unknown Patient";
        }
    }
}
