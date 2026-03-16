using System;
using System.Collections.Generic;
using System.Linq;
using AppointmentManagementSystem.WpfClient.Models;
using AppointmentManagementSystem.WpfClient.ViewModels;

namespace AppointmentManagementSystem.WpfClient.Services
{
    public sealed class DraftValidationService
    {
        private const int MinimumDurationMinutes = 5;

        public Dictionary<string, List<string>> ValidateDraft(AppointmentModel draft, TimelineAppointmentBlockViewModel draftBlock, Guid? draftSourceAppointmentId, IEnumerable<AppointmentModel> existingAppointments)
        {
            var errors = new Dictionary<string, List<string>>();

            if (draft == null)
                return errors;

            // Title required
            if (string.IsNullOrWhiteSpace(draft.Title))
                AddError(errors, nameof(draft.Title), "Title is required");

            // Time range validation
            if (draft.Start >= draft.End)
            {
                AddError(errors, nameof(draft.Start), "Start must be before End");
                AddError(errors, nameof(draft.End), "End must be after Start");
                AddError(errors, nameof(draft.Start), "Start must be before End");
                AddError(errors, nameof(draft.End), "End must be after Start");
            }

            // Overlap validation under synthetic key 'TimeRange'
            if (draftBlock != null && existingAppointments != null)
            {
                var hasConflict = existingAppointments.Any(existing =>
                    existing.Start.Date == draft.Start.Date &&
                    (!draftSourceAppointmentId.HasValue || existing.Id != draftSourceAppointmentId.Value) &&
                    draft.Start < existing.End &&
                    draft.End > existing.Start);

                if (hasConflict)
                    AddError(errors, "TimeRange", "Appointment overlaps another appointment");
            }

            return errors;
        }

        private static void AddError(Dictionary<string, List<string>> map, string property, string message)
        {
            if (!map.TryGetValue(property, out var list))
            {
                list = new List<string>();
                map[property] = list;
            }

            if (!list.Contains(message))
                list.Add(message);
        }
    }
}
