using System;

namespace AppointmentManagementSystem.WpfClient.ViewModels
{
    /// <summary>
    /// Event args for appointment created event.
    /// </summary>
    public class AppointmentCreatedEventArgs : EventArgs
    {
        public Guid AppointmentId { get; set; }
    }
}
