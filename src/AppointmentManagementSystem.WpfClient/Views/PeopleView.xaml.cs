using System.Windows.Controls;
using AppointmentManagementSystem.WpfClient.ViewModels;

namespace AppointmentManagementSystem.WpfClient.Views
{
    /// <summary>
    /// Interaction logic for PeopleView.xaml
    /// UserControl for displaying the list of patients.
    /// Read-only view - no editing capabilities.
    /// Receives ViewModel via binding.
    /// NOTE:
    /// Due to time constraints for this technical assessment,
    /// full Patient CRUD was not implemented.
    /// This is a read-only view of seeded patients.
    /// </summary>
    public partial class PeopleView : UserControl
    {
        public PeopleView()
        {
            InitializeComponent();
        }
    }
}
