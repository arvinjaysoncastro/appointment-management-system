using System;
using System.Collections.ObjectModel;
using AppointmentManagementSystem.WpfClient.Infrastructure;
using AppointmentManagementSystem.WpfClient.Models;

namespace AppointmentManagementSystem.WpfClient.ViewModels
{
    /// <summary>
    /// ViewModel for displaying the list of patients/people.
    /// NOTE:
    /// Due to time constraints for this technical assessment,
    /// full Patient CRUD was not implemented.
    /// The list reflects the seeded patients used by appointments.
    /// </summary>
    public class PeopleViewModel : ViewModelBase
    {
        public ObservableCollection<Person> People { get; }

        public PeopleViewModel()
        {
            People = new ObservableCollection<Person>
            {
                new Person { PatientId = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "John Smith" },
                new Person { PatientId = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "Mary Johnson" },
                new Person { PatientId = Guid.Parse("00000000-0000-0000-0000-000000000003"), Name = "David Lee" },
                new Person { PatientId = Guid.Parse("00000000-0000-0000-0000-000000000004"), Name = "Sarah Miller" },
                new Person { PatientId = Guid.Parse("00000000-0000-0000-0000-000000000005"), Name = "Michael Brown" },
                new Person { PatientId = Guid.Parse("00000000-0000-0000-0000-000000000006"), Name = "Emily Davis" },
                new Person { PatientId = Guid.Parse("00000000-0000-0000-0000-000000000007"), Name = "Daniel Wilson" },
                new Person { PatientId = Guid.Parse("00000000-0000-0000-0000-000000000008"), Name = "Olivia Martinez" },
                new Person { PatientId = Guid.Parse("00000000-0000-0000-0000-000000000009"), Name = "James Anderson" },
                new Person { PatientId = Guid.Parse("00000000-0000-0000-0000-000000000010"), Name = "Sophia Taylor" },
                new Person { PatientId = Guid.Parse("00000000-0000-0000-0000-000000000011"), Name = "Benjamin Thomas" },
                new Person { PatientId = Guid.Parse("00000000-0000-0000-0000-000000000012"), Name = "Charlotte Moore" },
                new Person { PatientId = Guid.Parse("00000000-0000-0000-0000-000000000013"), Name = "Lucas Jackson" }
            };
        }
    }
}
