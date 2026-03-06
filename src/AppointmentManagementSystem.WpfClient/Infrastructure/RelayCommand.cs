using System;
using System.Windows.Input;

namespace AppointmentManagementSystem.WpfClient.Infrastructure
{
    /// <summary>
    /// A command that relays its functionality to other objects by invoking delegates.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }

    /// <summary>
    /// A generic command that relays its functionality to other objects by invoking delegates.
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }

    /// <summary>
    /// An async command that relays its functionality to async methods.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<System.Threading.Tasks.Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isExecuting;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public AsyncRelayCommand(Func<System.Threading.Tasks.Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            try
            {
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    /// <summary>
    /// A generic async command that relays its functionality to async methods.
    /// </summary>
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T, System.Threading.Tasks.Task> _execute;
        private readonly Func<T, bool> _canExecute;
        private bool _isExecuting;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public AsyncRelayCommand(Func<T, System.Threading.Tasks.Task> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke((T)parameter) ?? true);
        }

        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            try
            {
                await _execute((T)parameter);
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
