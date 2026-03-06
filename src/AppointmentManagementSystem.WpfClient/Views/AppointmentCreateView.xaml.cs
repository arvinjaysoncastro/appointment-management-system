using System.Windows;
using AppointmentManagementSystem.WpfClient.ViewModels;

namespace AppointmentManagementSystem.WpfClient.Views
{
    /// <summary>
    /// Interaction logic for AppointmentCreateView.xaml
    /// Dialog for creating new appointments.
    /// No business logic - view only.
    /// </summary>
    public partial class AppointmentCreateView : Window
    {
        public AppointmentCreateView(AppointmentCreateViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Hook up dialog events
            viewModel.AppointmentCreated += (s, e) =>
            {
                DialogResult = true;
                Close();
            };

            viewModel.Cancelled += (s, e) =>
            {
                DialogResult = false;
                Close();
            };
        }
    }
}
