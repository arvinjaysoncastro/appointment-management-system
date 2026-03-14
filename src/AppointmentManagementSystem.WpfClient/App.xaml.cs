using System;
using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using AppointmentManagementSystem.WpfClient.Services;
using AppointmentManagementSystem.WpfClient.ViewModels;
using AppointmentManagementSystem.WpfClient.Views;

namespace AppointmentManagementSystem.WpfClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// Configures Dependency Injection for the WPF application.
    /// Constructor injection only - no Service Locator pattern.
    /// </summary>
    public partial class App : Application
    {
        private Microsoft.Extensions.DependencyInjection.ServiceProvider _serviceProvider;

        public App()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register HttpClient as singleton with proper HTTPS handling for development.
            var httpHandler = new HttpClientHandler();
            #if DEBUG
            // Allow self-signed certificates in development only.
            httpHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            #endif

            var configuredHttpClient = new HttpClient(httpHandler)
            {
                BaseAddress = new Uri("https://localhost:7017")
            };

            services.AddSingleton(configuredHttpClient);
            services.AddSingleton<AppointmentApiService>();

            services.AddTransient<AppointmentsViewModel>();
            services.AddTransient<PeopleViewModel>();
            services.AddTransient<SettingsViewModel>();

            // Register views for IoC completeness. Rendering is done via App.xaml DataTemplates.
            services.AddTransient<AppointmentsView>();
            services.AddTransient<PeopleView>();
            services.AddTransient<SettingsView>();

            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
            navigationService.Register<AppointmentsViewModel, AppointmentsView>();
            navigationService.Register<PeopleViewModel, PeopleView>();
            navigationService.Register<SettingsViewModel, SettingsView>();
            
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
