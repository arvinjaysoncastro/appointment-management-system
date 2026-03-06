using System.Windows;
using System.Windows.Input;
using AppointmentManagementSystem.WpfClient.ViewModels;
using AppointmentManagementSystem.WpfClient.Views;

namespace AppointmentManagementSystem.WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// No business logic - view only
    /// Receives ViewModel via constructor injection
    /// NOTE:
    /// The default Windows title bar was removed to allow a clean custom UI
    /// for the WPF application. Window dragging and close functionality are
    /// implemented via event handlers.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        /// <summary>
        /// Allows the window to be dragged by clicking and holding on the Grid.
        /// </summary>
        private void WindowDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        /// <summary>
        /// Closes the application when the custom close button is clicked.
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
