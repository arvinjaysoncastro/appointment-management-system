using System;
using System.Windows;
using System.Windows.Controls;

namespace AppointmentManagementSystem.WpfClient.Infrastructure
{
    public static class ScrollViewerOffsetBehavior
    {
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "VerticalOffset",
                typeof(double),
                typeof(ScrollViewerOffsetBehavior),
                new PropertyMetadata(0d));

        public static readonly DependencyProperty ScrollRequestIdProperty =
            DependencyProperty.RegisterAttached(
                "ScrollRequestId",
                typeof(int),
                typeof(ScrollViewerOffsetBehavior),
                new PropertyMetadata(0, OnScrollRequestIdChanged));

        public static double GetVerticalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(VerticalOffsetProperty);
        }

        public static void SetVerticalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(VerticalOffsetProperty, value);
        }

        public static int GetScrollRequestId(DependencyObject obj)
        {
            return (int)obj.GetValue(ScrollRequestIdProperty);
        }

        public static void SetScrollRequestId(DependencyObject obj, int value)
        {
            obj.SetValue(ScrollRequestIdProperty, value);
        }

        private static void OnScrollRequestIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as ScrollViewer;
            if (scrollViewer == null)
            {
                return;
            }

            var target = GetVerticalOffset(scrollViewer);
            if (target < 0d)
            {
                target = 0d;
            }

            if (target > scrollViewer.ScrollableHeight)
            {
                target = scrollViewer.ScrollableHeight;
            }

            scrollViewer.Dispatcher.BeginInvoke(new Action(() =>
            {
                var resolved = target;
                if (resolved > scrollViewer.ScrollableHeight)
                {
                    resolved = scrollViewer.ScrollableHeight;
                }

                scrollViewer.ScrollToVerticalOffset(resolved);
            }));
        }
    }
}
