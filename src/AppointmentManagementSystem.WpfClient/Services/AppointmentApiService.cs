using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AppointmentManagementSystem.WpfClient.Models;

namespace AppointmentManagementSystem.WpfClient.Services
{
    public sealed class AppointmentApiService
    {
        private readonly HttpClient _httpClient;

        public AppointmentApiService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<AppointmentModel>> GetAppointmentsAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<List<AppointmentModel>>("appointments");
            return data ?? new List<AppointmentModel>();
        }

        public async Task<AppointmentModel> CreateAppointmentAsync(AppointmentModel appointment)
        {
            var response = await _httpClient.PostAsJsonAsync("appointments", appointment);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<AppointmentModel>();
            if (created == null)
            {
                throw new InvalidOperationException("The API returned an empty appointment payload.");
            }

            return created;
        }

        public async Task UpdateAppointmentAsync(Guid id, AppointmentModel appointment)
        {
            var response = await _httpClient.PutAsJsonAsync($"appointments/{id}", appointment);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAppointmentAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"appointments/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}