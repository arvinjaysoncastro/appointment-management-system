using System;
using System.Collections.Generic;
using System.Linq;
using AppointmentManagementSystem.WpfClient.Models;
using AppointmentManagementSystem.WpfClient.ViewModels;

using AppointmentManagementSystem.WpfClient.Enums;

namespace AppointmentManagementSystem.WpfClient.Services
{
    public sealed class TimelineLayoutService
    {
        private const int MinimumDurationMinutes = 5;
        public enum DragMode
        {
            None,
            Move,
            ResizeStart,
            ResizeEnd
        }

        public List<TimelineHourViewModel> GenerateTimelineHours(DateTime day)
        {
            var hours = new List<TimelineHourViewModel>();
            for (var hour = 0; hour <= 23; hour++)
            {
                var start = day.Date.AddHours(hour);
                var end = start.AddHours(1).AddMinutes(-1);

                hours.Add(new TimelineHourViewModel
                {
                    Hour = hour,
                    Start = start,
                    End = end
                });
            }

            return hours;
        }

        public List<TimelineAppointmentBlockViewModel> BuildTimelineBlocks(IEnumerable<AppointmentModel> appointments, TimelineAppointmentBlockViewModel draftBlock, DateTime day, Guid? draftSourceAppointmentId, bool showAllDescriptions)
        {
            var blocks = new List<TimelineAppointmentBlockViewModel>();

            foreach (var appointment in appointments)
            {
                if (appointment.Start.Date != day.Date)
                    continue;

                if (draftSourceAppointmentId.HasValue && appointment.Id == draftSourceAppointmentId.Value)
                    continue;

                blocks.Add(new TimelineAppointmentBlockViewModel
                {
                    Appointment = appointment,
                    IsDraft = false,
                    IsConflicting = false,
                    ShowDetails = showAllDescriptions
                });
            }

            if (draftBlock != null && draftBlock.Appointment != null && draftBlock.Appointment.Start.Date == day.Date)
            {
                draftBlock.IsDraft = true;
                draftBlock.ShowDetails = showAllDescriptions;
                draftBlock.RecalculateLayout();
                blocks.Add(draftBlock);
            }

            return blocks;
        }

        public List<TimelineHourViewModel> AssignAppointmentsToTimeline(List<TimelineHourViewModel> timelineHours, IEnumerable<TimelineAppointmentBlockViewModel> timelineBlocks, DateTime day)
        {
            if (timelineHours == null)
                timelineHours = GenerateTimelineHours(day);

            foreach (var slot in timelineHours)
            {
                slot.Appointments.Clear();
            }

            foreach (var block in timelineBlocks)
            {
                var appointment = block.Appointment;
                if (appointment == null || appointment.Start.Date != day.Date)
                    continue;

                timelineHours[appointment.Start.Hour].Appointments.Add(appointment);
            }

            return timelineHours;
        }

        public AppointmentModel ApplyDragDelta(DragMode dragMode, int minuteDelta, AppointmentModel draft)
        {
            if (dragMode == DragMode.None || minuteDelta == 0 || draft == null)
                return draft;

            var dayStart = draft.Start.Date;
            var dayEnd = dayStart.AddDays(1).AddMinutes(-1);
            var duration = EnsureMinimumDuration(draft.End - draft.Start);

            var result = new AppointmentModel
            {
                Id = draft.Id,
                Title = draft.Title,
                Description = draft.Description,
                Start = draft.Start,
                End = draft.End
            };

            if (dragMode == DragMode.Move)
            {
                var proposedStart = draft.Start.AddMinutes(minuteDelta);
                if (proposedStart < dayStart)
                    proposedStart = dayStart;

                var latestStart = dayEnd.Subtract(duration);
                if (proposedStart > latestStart)
                    proposedStart = latestStart;

                result.Start = proposedStart;
                result.End = proposedStart.Add(duration);
                return result;
            }

            if (dragMode == DragMode.ResizeStart)
            {
                var maxStart = draft.End.AddMinutes(-MinimumDurationMinutes);
                var proposedStart = draft.Start.AddMinutes(minuteDelta);

                if (proposedStart < dayStart)
                    proposedStart = dayStart;

                if (proposedStart > maxStart)
                    proposedStart = maxStart;

                result.Start = proposedStart;
                return result;
            }

            if (dragMode == DragMode.ResizeEnd)
            {
                var minEnd = draft.Start.AddMinutes(MinimumDurationMinutes);
                var proposedEnd = draft.End.AddMinutes(minuteDelta);

                if (proposedEnd > dayEnd)
                    proposedEnd = dayEnd;

                if (proposedEnd < minEnd)
                    proposedEnd = minEnd;

                result.End = proposedEnd;
            }

            return result;
        }

        private static TimeSpan EnsureMinimumDuration(TimeSpan duration)
        {
            if (duration.TotalMinutes < MinimumDurationMinutes)
            {
                return TimeSpan.FromMinutes(MinimumDurationMinutes);
            }

            return duration;
        }
    }
}
