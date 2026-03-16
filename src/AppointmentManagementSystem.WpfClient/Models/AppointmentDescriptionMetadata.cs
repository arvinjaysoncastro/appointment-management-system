using System;

namespace AppointmentManagementSystem.WpfClient.Models
{
    public class AppointmentDescriptionMetadata
    {
        public string DescriptionText { get; set; }

        // New optional metadata fields. Kept as plain strings for C# language compatibility.
        // Older plain-text descriptions remain compatible when these are empty.
        public string Priority { get; set; }
        public string Status { get; set; }
    }
}
