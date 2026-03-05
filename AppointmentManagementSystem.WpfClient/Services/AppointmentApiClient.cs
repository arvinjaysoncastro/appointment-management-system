using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace AppointmentManagementSystem.WpfClient.Services
{
    /// <summary>
    /// HTTP client for communicating with the Appointment API.
    /// Abstracts API communication from ViewModels.
    /// </summary>
    public interface IAppointmentApiClient
    {
        Task<AppointmentListResponse> GetAppointmentsAsync(DateTime date);
    }

    public class AppointmentApiClient : IAppointmentApiClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5000/api/appointments";

        public AppointmentApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<AppointmentListResponse> GetAppointmentsAsync(DateTime date)
        {
            try
            {
                var url = $"{BaseUrl}?date={date:O}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // TODO: Deserialize response using System.Text.Json or Newtonsoft.Json
                // For now, scaffold only - no API calls implemented
                return new AppointmentListResponse
                {
                    Appointments = new ObservableCollection<AppointmentDto>()
                };
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Failed to fetch appointments from API", ex);
            }
        }
    }

    /// <summary>
    /// DTO for API responses
    /// </summary>
    public class AppointmentDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Title { get; set; }
    }

    public class AppointmentListResponse
    {
        public ObservableCollection<AppointmentDto> Appointments { get; set; }
            = new ObservableCollection<AppointmentDto>();
    }
}
