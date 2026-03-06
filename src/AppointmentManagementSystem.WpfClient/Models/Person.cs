using System;

namespace AppointmentManagementSystem.WpfClient.Models
{
    /// <summary>
    /// Represents a patient in the system.
    /// NOTE:
    /// Due to time constraints for this technical assessment,
    /// full Patient CRUD was not implemented.
    /// The list reflects the seeded patients used by appointments.
    /// </summary>
    public class Person
    {
        public Guid PatientId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
