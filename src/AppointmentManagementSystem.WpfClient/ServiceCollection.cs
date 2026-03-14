using System;
using System.Collections.Generic;

namespace AppointmentManagementSystem.WpfClient
{
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
}
