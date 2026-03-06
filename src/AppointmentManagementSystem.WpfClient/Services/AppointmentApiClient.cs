using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AppointmentManagementSystem.WpfClient.Services
{
    /// <summary>
    /// HTTP client for communicating with the Appointment API.
    /// Abstracts API communication from ViewModels.
    /// All IO is asynchronous.
    /// </summary>
    public interface IAppointmentApiClient
    {
        Task<List<AppointmentSummaryDto>> GetAppointmentsAsync(DateTime date);
    }

    public class AppointmentApiClient : IAppointmentApiClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5000/api/appointments";

        public AppointmentApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<AppointmentSummaryDto>> GetAppointmentsAsync(DateTime date)
        {
            try
            {
                var url = $"{BaseUrl}?date={date:O}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                
                // TODO: Implement proper JSON deserialization
                // For now, return empty list. Can use DataContractJsonSerializer or HttpClient.GetFromJsonAsync
                return new List<AppointmentSummaryDto>();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to fetch appointments from API: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deserializing API response: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// DTO matching API AppointmentSummaryDto
    /// </summary>
    public class AppointmentSummaryDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
