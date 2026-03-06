# Copilot Instructions

## Project Guidelines
- WPF client uses custom lightweight DI container (ServiceProvider, ServiceCollection, IServiceProvider) integrated into App.xaml.cs for .NET Framework 4.8 compatibility. Constructor injection only - no Service Locator pattern. MainWindow and ViewModels resolved via DI in OnStartup().