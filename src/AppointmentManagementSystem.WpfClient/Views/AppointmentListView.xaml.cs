using System.Windows.Controls;
using AppointmentManagementSystem.WpfClient.ViewModels;

namespace AppointmentManagementSystem.WpfClient.Views
{
    /// <summary>
    /// Interaction logic for AppointmentListView.xaml
    /// No business logic - view only
    /// DataContext will be set by parent MainWindow
    /// </summary>
    public partial class AppointmentListView : UserControl
    {
        public AppointmentListView()
        {
            InitializeComponent();
        }
    }
}
