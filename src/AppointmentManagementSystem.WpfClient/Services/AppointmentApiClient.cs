using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
                return SimpleJsonParser.ParseAppointmentList(content);
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
                var json = SimpleJsonParser.SerializeCreateAppointmentDto(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(BaseUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return SimpleJsonParser.ParseAppointmentDetails(responseContent);
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
    /// Simple JSON parser for .NET Framework 4.8 compatibility (no external dependencies)
    /// </summary>
    internal static class SimpleJsonParser
    {
        public static List<AppointmentSummaryDto> ParseAppointmentList(string json)
        {
            var list = new List<AppointmentSummaryDto>();
            var appointments = Regex.Matches(json, @"\{[^}]*\}");

            foreach (Match match in appointments)
            {
                var dto = ParseAppointmentSummary(match.Value);
                if (dto != null)
                    list.Add(dto);
            }

            return list;
        }

        public static AppointmentSummaryDto ParseAppointmentSummary(string json)
        {
            return new AppointmentSummaryDto
            {
                Id = ExtractGuid(json, "id"),
                PatientId = ExtractGuid(json, "patientId"),
                PatientName = ExtractString(json, "patientName"),
                Title = ExtractString(json, "title"),
                StartTime = ExtractDateTimeOffset(json, "startTime"),
                EndTime = ExtractDateTimeOffset(json, "endTime")
            };
        }

        public static AppointmentDetailsDto ParseAppointmentDetails(string json)
        {
            return new AppointmentDetailsDto
            {
                Id = ExtractGuid(json, "id"),
                PatientId = ExtractGuid(json, "patientId"),
                PatientName = ExtractString(json, "patientName"),
                Title = ExtractString(json, "title"),
                Notes = ExtractString(json, "notes"),
                StartTime = ExtractDateTimeOffset(json, "startTime"),
                EndTime = ExtractDateTimeOffset(json, "endTime")
            };
        }

        public static string SerializeCreateAppointmentDto(CreateAppointmentDto dto)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"patientId\":\"{dto.PatientId}\", ");
            sb.Append($"\"title\":\"{EscapeJsonString(dto.Title)}\", ");
            sb.Append($"\"notes\":\"{EscapeJsonString(dto.Notes)}\", ");
            sb.Append($"\"startTime\":\"{dto.StartTime:O}\", ");
            sb.Append($"\"endTime\":\"{dto.EndTime:O}\"");
            sb.Append("}");
            return sb.ToString();
        }

        private static Guid ExtractGuid(string json, string key)
        {
            var pattern = $"\"{key}\":\"([^\"]+)\"";
            var match = Regex.Match(json, pattern, RegexOptions.IgnoreCase);
            return match.Success && Guid.TryParse(match.Groups[1].Value, out var result) ? result : Guid.Empty;
        }

        private static string ExtractString(string json, string key)
        {
            var pattern = $"\"{key}\":\"([^\"]*)\"";
            var match = Regex.Match(json, pattern, RegexOptions.IgnoreCase);
            return match.Success ? UnescapeJsonString(match.Groups[1].Value) : string.Empty;
        }

        private static DateTimeOffset ExtractDateTimeOffset(string json, string key)
        {
            var pattern = $"\"{key}\":\"([^\"]+)\"";
            var match = Regex.Match(json, pattern, RegexOptions.IgnoreCase);
            if (match.Success && DateTimeOffset.TryParse(match.Groups[1].Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
            {
                return result;
            }
            return DateTimeOffset.Now;
        }

        private static string EscapeJsonString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        private static string UnescapeJsonString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t");
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
