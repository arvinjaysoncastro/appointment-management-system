using System;
using System.Collections.Generic;

namespace AppointmentManagementSystem.WpfClient
{
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

            if (descriptor.Instance != null)
                return (T)descriptor.Instance;

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
            if (_disposed)
                return;

            _singletons.Clear();
            _disposed = true;
        }
    }
}
