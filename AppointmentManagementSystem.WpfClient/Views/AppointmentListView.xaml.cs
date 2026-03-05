using System.Windows.Controls;
using AppointmentManagementSystem.WpfClient.ViewModels;

namespace AppointmentManagementSystem.WpfClient.Views
{
    /// <summary>
    /// Interaction logic for AppointmentListView.xaml
    /// No business logic - view only
    /// </summary>
    public partial class AppointmentListView : UserControl
    {
        public AppointmentListView()
        {
            InitializeComponent();
            DataContext = new AppointmentListViewModel();
        }
    }
}
