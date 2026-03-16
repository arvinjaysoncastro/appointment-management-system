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

        private void TitleBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is AppointmentsViewModel vm)
            {
                vm.TitleTouched = true;
            }
        }

        private void TimeField_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is AppointmentsViewModel vm)
            {
                vm.TimeTouched = true;
            }
        }
    }
}
