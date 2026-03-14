using System;
using System.Collections.Generic;
using AppointmentManagementSystem.WpfClient.Infrastructure;

namespace AppointmentManagementSystem.WpfClient.Services
{
    public interface INavigationService
    {
        object CurrentViewModel { get; }
        event EventHandler CurrentViewModelChanged;
        void Register<TViewModel, TView>() where TViewModel : ViewModelBase where TView : class;
        void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    }

    /// <summary>
    /// Resolves view models via DI and tracks the active view model for navigation.
    /// View mapping is registered for validation while DataTemplates render views.
    /// </summary>
    public sealed class NavigationService : INavigationService
    {
        private readonly AppointmentManagementSystem.WpfClient.IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Type> _viewMappings = new Dictionary<Type, Type>();
        private object _currentViewModel;

        public object CurrentViewModel => _currentViewModel;

        public event EventHandler CurrentViewModelChanged;

        public NavigationService(AppointmentManagementSystem.WpfClient.IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void Register<TViewModel, TView>() where TViewModel : ViewModelBase where TView : class
        {
            _viewMappings[typeof(TViewModel)] = typeof(TView);
        }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            var viewModelType = typeof(TViewModel);
            if (!_viewMappings.ContainsKey(viewModelType))
                throw new InvalidOperationException($"No view mapping registered for '{viewModelType.Name}'.");

            _currentViewModel = _serviceProvider.GetRequiredService<TViewModel>();
            CurrentViewModelChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
