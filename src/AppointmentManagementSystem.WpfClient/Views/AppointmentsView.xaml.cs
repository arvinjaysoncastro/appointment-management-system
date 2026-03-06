using System.Windows.Controls;
using AppointmentManagementSystem.WpfClient.ViewModels;

namespace AppointmentManagementSystem.WpfClient.Views
{
    /// <summary>
    /// Interaction logic for AppointmentsView.xaml
    /// UserControl for displaying and managing appointments.
    /// No business logic - view only.
    /// Receives ViewModel via binding.
    /// </summary>
    public partial class AppointmentsView : UserControl
    {
        public AppointmentsView()
        {
            InitializeComponent();
        }
    }
}
