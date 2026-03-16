using System;
using System.Text.Json;
using AppointmentManagementSystem.WpfClient.Models;

namespace AppointmentManagementSystem.WpfClient.Helpers
{
    public static class DescriptionParser
    {
        public static AppointmentDescriptionMetadata Parse(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return new AppointmentDescriptionMetadata();

            try
            {
                return JsonSerializer.Deserialize<AppointmentDescriptionMetadata>(description)
                       ?? new AppointmentDescriptionMetadata();
            }
            catch
            {
                return new AppointmentDescriptionMetadata
                {
                    DescriptionText = description
                };
            }
        }

        public static string Serialize(AppointmentDescriptionMetadata metadata)
        {
            return JsonSerializer.Serialize(metadata);
        }
    }
}
