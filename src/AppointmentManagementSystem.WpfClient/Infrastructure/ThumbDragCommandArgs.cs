using System;

namespace AppointmentManagementSystem.WpfClient.Infrastructure
{
    public sealed class ThumbDragCommandArgs
    {
        public object Parameter { get; set; }
        public double HorizontalChange { get; set; }
        public double VerticalChange { get; set; }
    }
}
