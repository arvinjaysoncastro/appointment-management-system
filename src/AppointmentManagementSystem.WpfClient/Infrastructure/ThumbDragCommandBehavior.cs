using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace AppointmentManagementSystem.WpfClient.Infrastructure
{
    public static class ThumbDragCommandBehavior
    {
        private const double AutoScrollThreshold = 28d;
        private const double AutoScrollMaxStep = 24d;

        public static readonly DependencyProperty DragDeltaCommandProperty =
            DependencyProperty.RegisterAttached(
                "DragDeltaCommand",
                typeof(ICommand),
                typeof(ThumbDragCommandBehavior),
                new PropertyMetadata(null, OnDragDeltaCommandChanged));

        public static readonly DependencyProperty DragDeltaCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "DragDeltaCommandParameter",
                typeof(object),
                typeof(ThumbDragCommandBehavior),
                new PropertyMetadata(null));

        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.RegisterAttached(
                "MouseDownCommand",
                typeof(ICommand),
                typeof(ThumbDragCommandBehavior),
                new PropertyMetadata(null, OnMouseDownCommandChanged));

        public static readonly DependencyProperty MouseDownCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "MouseDownCommandParameter",
                typeof(object),
                typeof(ThumbDragCommandBehavior),
                new PropertyMetadata(null));

        public static ICommand GetDragDeltaCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(DragDeltaCommandProperty);
        }

        public static void SetDragDeltaCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(DragDeltaCommandProperty, value);
        }

        public static object GetDragDeltaCommandParameter(DependencyObject obj)
        {
            return obj.GetValue(DragDeltaCommandParameterProperty);
        }

        public static void SetDragDeltaCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(DragDeltaCommandParameterProperty, value);
        }

        public static ICommand GetMouseDownCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(MouseDownCommandProperty);
        }

        public static void SetMouseDownCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(MouseDownCommandProperty, value);
        }

        public static object GetMouseDownCommandParameter(DependencyObject obj)
        {
            return obj.GetValue(MouseDownCommandParameterProperty);
        }

        public static void SetMouseDownCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(MouseDownCommandParameterProperty, value);
        }

        private static void OnDragDeltaCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thumb = d as Thumb;
            if (thumb == null)
            {
                return;
            }

            thumb.DragDelta -= OnThumbDragDelta;
            if (e.NewValue != null)
            {
                thumb.DragDelta += OnThumbDragDelta;
            }
        }

        private static void OnMouseDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thumb = d as Thumb;
            if (thumb == null)
            {
                return;
            }

            thumb.PreviewMouseLeftButtonDown -= OnThumbMouseLeftButtonDown;
            if (e.NewValue != null)
            {
                thumb.PreviewMouseLeftButtonDown += OnThumbMouseLeftButtonDown;
            }
        }

        private static void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            if (thumb == null)
            {
                return;
            }

            var command = GetDragDeltaCommand(thumb);
            if (command == null)
            {
                return;
            }

            var autoScrollDelta = ApplyAutoScroll(thumb);

            var args = new ThumbDragCommandArgs
            {
                Parameter = GetDragDeltaCommandParameter(thumb),
                HorizontalChange = e.HorizontalChange,
                VerticalChange = e.VerticalChange + autoScrollDelta
            };

            if (command.CanExecute(args))
            {
                command.Execute(args);
            }
        }

        private static double ApplyAutoScroll(Thumb thumb)
        {
            var scrollViewer = FindAncestor<ScrollViewer>(thumb);
            if (scrollViewer == null || scrollViewer.ScrollableHeight <= 0d)
            {
                return 0d;
            }

            var mousePosition = Mouse.GetPosition(scrollViewer);
            var viewportHeight = scrollViewer.ViewportHeight;
            if (viewportHeight <= 0d)
            {
                return 0d;
            }

            var oldOffset = scrollViewer.VerticalOffset;
            var requestedDelta = 0d;

            if (mousePosition.Y < AutoScrollThreshold)
            {
                requestedDelta = -Math.Min(AutoScrollMaxStep, AutoScrollThreshold - mousePosition.Y);
            }
            else if (mousePosition.Y > viewportHeight - AutoScrollThreshold)
            {
                requestedDelta = Math.Min(AutoScrollMaxStep, mousePosition.Y - (viewportHeight - AutoScrollThreshold));
            }

            if (requestedDelta == 0d)
            {
                return 0d;
            }

            var newOffset = oldOffset + requestedDelta;
            if (newOffset < 0d)
            {
                newOffset = 0d;
            }
            else if (newOffset > scrollViewer.ScrollableHeight)
            {
                newOffset = scrollViewer.ScrollableHeight;
            }

            if (Math.Abs(newOffset - oldOffset) <= double.Epsilon)
            {
                return 0d;
            }

            scrollViewer.ScrollToVerticalOffset(newOffset);
            return newOffset - oldOffset;
        }

        private static T FindAncestor<T>(DependencyObject child)
            where T : DependencyObject
        {
            var current = child;
            while (current != null)
            {
                if (current is T match)
                {
                    return match;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private static void OnThumbMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var thumb = sender as Thumb;
            if (thumb == null)
            {
                return;
            }

            var command = GetMouseDownCommand(thumb);
            if (command == null)
            {
                return;
            }

            var parameter = GetMouseDownCommandParameter(thumb);
            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }
    }
}
