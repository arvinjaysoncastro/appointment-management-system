using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Windows;
using AppointmentManagementSystem.WpfClient.Services;
using AppointmentManagementSystem.WpfClient.ViewModels;
using AppointmentManagementSystem.WpfClient.Views;

namespace AppointmentManagementSystem.WpfClient
{
    /// <summary>
    /// Lightweight Dependency Injection Container for .NET Framework 4.8
    /// No external dependencies required
    /// </summary>
    public interface IServiceProvider : IDisposable
    {
        T GetRequiredService<T>() where T : class;
    }

    public sealed class ServiceCollection
    {
        private readonly Dictionary<Type, ServiceDescriptor> _descriptors = new Dictionary<Type, ServiceDescriptor>();

        public void AddSingleton<TInterface>(TInterface instance) where TInterface : class
        {
            _descriptors[typeof(TInterface)] = new ServiceDescriptor(typeof(TInterface), instance);
        }

        public void AddSingleton<TInterface>(Func<IServiceProvider, TInterface> factory) where TInterface : class
        {
            _descriptors[typeof(TInterface)] = new ServiceDescriptor(typeof(TInterface), (Delegate)factory);
        }

        public void AddTransient<TInterface>(Func<IServiceProvider, TInterface> factory) where TInterface : class
        {
            _descriptors[typeof(TInterface)] = new ServiceDescriptor(typeof(TInterface), (Delegate)factory);
        }

        public ServiceProvider BuildServiceProvider()
        {
            return new ServiceProvider(_descriptors);
        }
    }

    public sealed class ServiceDescriptor
    {
        public Type ServiceType { get; }
        public object Instance { get; }
        public Delegate Factory { get; }

        public ServiceDescriptor(Type serviceType, object instance)
        {
            ServiceType = serviceType;
            Instance = instance;
        }

        public ServiceDescriptor(Type serviceType, Delegate factory)
        {
            ServiceType = serviceType;
            Factory = factory;
        }
    }

    public sealed class ServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, ServiceDescriptor> _descriptors;
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();
        private bool _disposed;

        public ServiceProvider(Dictionary<Type, ServiceDescriptor> descriptors)
        {
            _descriptors = descriptors ?? throw new ArgumentNullException(nameof(descriptors));
        }

        public T GetRequiredService<T>() where T : class
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ServiceProvider));

            var serviceType = typeof(T);

            if (!_descriptors.TryGetValue(serviceType, out var descriptor))
                throw new InvalidOperationException($"Service '{serviceType.Name}' is not registered.");

            // Return cached singleton if available
            if (descriptor.Instance != null)
                return (T)descriptor.Instance;

            // Invoke factory and cache the result
            if (descriptor.Factory != null)
            {
                lock (_singletons)
                {
                    if (_singletons.TryGetValue(serviceType, out var cached))
                        return (T)cached;

                    var result = (T)descriptor.Factory.DynamicInvoke(this);
                    _singletons[serviceType] = result;
                    return result;
                }
            }

            throw new InvalidOperationException($"Cannot resolve service '{serviceType.Name}'.");

        }
        public void Dispose()
        {
            if (_disposed) return;
            _singletons.Clear();
            _disposed = true;
        }
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// Configures Dependency Injection for the WPF application
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Register HttpClient as singleton
            services.AddSingleton<HttpClient>(new HttpClient());

            // Register AppointmentApiClient (transient - new instance each time)
            services.AddTransient<IAppointmentApiClient>(provider =>
            {
                var httpClient = provider.GetRequiredService<HttpClient>();
                return new AppointmentApiClient(httpClient);
            });

            // Register AppointmentListViewModel (transient - new instance each time)
            services.AddTransient<AppointmentListViewModel>(provider =>
            {
                var apiClient = provider.GetRequiredService<IAppointmentApiClient>();
                return new AppointmentListViewModel(apiClient);
            });

            // Register MainWindow as singleton (created once via DI)
            services.AddSingleton<MainWindow>(provider =>
            {
                var viewModel = provider.GetRequiredService<AppointmentListViewModel>();
                return new MainWindow(viewModel);
            });
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Resolve and show MainWindow via DI container
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _serviceProvider?.Dispose();
        }
    }
}
