using System.Threading.Tasks;

namespace AppointmentManagementSystem.WpfClient.Infrastructure
{
    public interface IAsyncInitializable
    {
        Task InitializeAsync();
    }
}
