using System.Net.Http;
using System.Windows;
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
        private ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Register HttpClient as singleton with proper HTTPS handling for development
            var httpHandler = new HttpClientHandler();
            #if DEBUG
            // Allow self-signed certificates in development only
            httpHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            #endif
            var configuredHttpClient = new HttpClient(httpHandler);
            services.AddSingleton(configuredHttpClient);

            // Register AppointmentApiClient (transient - new instance each time)
            services.AddTransient<IAppointmentApiClient>(provider =>
            {
                var httpClient = provider.GetRequiredService<HttpClient>();
                return new AppointmentApiClient(httpClient);
            });

            // Register AppointmentCreateViewModel (transient - new instance each time)
            services.AddTransient<AppointmentCreateViewModel>(provider =>
            {
                var apiClient = provider.GetRequiredService<IAppointmentApiClient>();
                return new AppointmentCreateViewModel(apiClient);
            });

            services.AddTransient<PeopleViewModel>(provider => new PeopleViewModel());
            services.AddTransient<SettingsViewModel>(provider => new SettingsViewModel());

            // Register AppointmentListViewModel (transient - new instance each time)
            services.AddTransient<AppointmentListViewModel>(provider =>
            {
                var apiClient = provider.GetRequiredService<IAppointmentApiClient>();
                var createViewModel = provider.GetRequiredService<AppointmentCreateViewModel>();
                return new AppointmentListViewModel(apiClient, createViewModel);
            });

            // Register views for IoC completeness. Rendering is done via App.xaml DataTemplates.
            services.AddTransient<AppointmentsView>(provider => new AppointmentsView());
            services.AddTransient<PeopleView>(provider => new PeopleView());
            services.AddTransient<SettingsView>(provider => new SettingsView());

            services.AddSingleton<INavigationService>(provider =>
            {
                return new NavigationService(provider);
            });

            // Register MainWindowViewModel (singleton - created once via DI)
            services.AddSingleton<MainWindowViewModel>(provider =>
            {
                var navigationService = provider.GetRequiredService<INavigationService>();
                return new MainWindowViewModel(navigationService);
            });

            // Register MainWindow as singleton (created once via DI)
            services.AddSingleton<MainWindow>(provider =>
            {
                var viewModel = provider.GetRequiredService<MainWindowViewModel>();
                return new MainWindow(viewModel);
            });
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
            navigationService.Register<AppointmentListViewModel, AppointmentsView>();
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
