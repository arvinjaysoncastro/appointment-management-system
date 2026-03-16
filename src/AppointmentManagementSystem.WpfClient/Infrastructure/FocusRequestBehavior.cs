using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AppointmentManagementSystem.WpfClient.Infrastructure
{
    public static class FocusRequestBehavior
    {
        public static readonly DependencyProperty FocusRequestIdProperty =
            DependencyProperty.RegisterAttached(
                "FocusRequestId",
                typeof(int),
                typeof(FocusRequestBehavior),
                new PropertyMetadata(0, OnFocusRequestIdChanged));

        public static int GetFocusRequestId(DependencyObject obj)
        {
            return (int)obj.GetValue(FocusRequestIdProperty);
        }

        public static void SetFocusRequestId(DependencyObject obj, int value)
        {
            obj.SetValue(FocusRequestIdProperty, value);
        }

        private static void OnFocusRequestIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBox textBox))
            {
                return;
            }

            if (!textBox.Dispatcher.CheckAccess())
            {
                textBox.Dispatcher.BeginInvoke(new System.Action(() => FocusTextBox(textBox)));
                return;
            }

            FocusTextBox(textBox);
        }

        private static void FocusTextBox(TextBox textBox)
        {
            if (!textBox.IsVisible || !textBox.IsEnabled)
            {
                return;
            }

            Keyboard.Focus(textBox);
            textBox.Focus();
            textBox.SelectAll();
        }
    }
}
