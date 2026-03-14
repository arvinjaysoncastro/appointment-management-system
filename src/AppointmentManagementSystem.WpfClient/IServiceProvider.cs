using System;

namespace AppointmentManagementSystem.WpfClient
{
    /// <summary>
    /// Lightweight Dependency Injection contract for .NET Framework 4.8.
    /// </summary>
    public interface IServiceProvider : IDisposable
    {
        T GetRequiredService<T>() where T : class;
    }
}
