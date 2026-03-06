using System;

namespace AppointmentManagementSystem.WpfClient.Models
{
    /// <summary>
    /// Represents a patient option for dropdown selection.
    /// </summary>
    public class PatientOption
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
