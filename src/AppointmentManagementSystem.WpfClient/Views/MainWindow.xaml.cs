using System.Windows;
using AppointmentManagementSystem.WpfClient.ViewModels;
using AppointmentManagementSystem.WpfClient.Views;

namespace AppointmentManagementSystem.WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// No business logic - view only
    /// Receives ViewModel via constructor injection
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(AppointmentListViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
