using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AppointmentManagementSystem.WpfClient.Models
{
    public sealed class AppointmentModel : INotifyPropertyChanged
    {
        private Guid _id;
        private string _title;
        private string _description;
        private DateTime _start;
        private DateTime _end;

        public event PropertyChangedEventHandler PropertyChanged;

        public Guid Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        public string Title
        {
            get => _title;
            set => SetField(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetField(ref _description, value);
        }

        public DateTime Start
        {
            get => _start;
            set => SetField(ref _start, value);
        }

        public DateTime End
        {
            get => _end;
            set => SetField(ref _end, value);
        }

        public AppointmentModel()
        {
            _title = string.Empty;
            _description = string.Empty;
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}