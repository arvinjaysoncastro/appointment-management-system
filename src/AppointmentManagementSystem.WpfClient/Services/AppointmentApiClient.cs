using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
        Task<List<AppointmentSummaryDto>> GetAppointmentsAsync(DateTime? date = null);
        Task<AppointmentDetailsDto> CreateAppointmentAsync(CreateAppointmentDto dto);
    }

    public class AppointmentApiClient : IAppointmentApiClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7017/api/appointments";
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public AppointmentApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<AppointmentSummaryDto>> GetAppointmentsAsync(DateTime? date = null)
        {
            try
            {
                var url = date.HasValue ? $"{BaseUrl}?date={date:yyyy-MM-dd}" : BaseUrl;
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var appointments = JsonSerializer.Deserialize<List<AppointmentSummaryDto>>(content, JsonOptions);
                return appointments ?? new List<AppointmentSummaryDto>();
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

        public async Task<AppointmentDetailsDto> CreateAppointmentAsync(CreateAppointmentDto dto)
        {
            try
            {
                var json = JsonSerializer.Serialize(dto, JsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(BaseUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var appointment = JsonSerializer.Deserialize<AppointmentDetailsDto>(responseContent, JsonOptions);
                if (appointment == null)
                    throw new InvalidOperationException("API returned an empty response for appointment creation.");

                return appointment;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to create appointment: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating appointment: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// DTO for creating a new appointment
    /// </summary>
    public class CreateAppointmentDto
    {
        public Guid PatientId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }

    /// <summary>
    /// DTO for appointment details response
    /// </summary>
    public class AppointmentDetailsDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
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
