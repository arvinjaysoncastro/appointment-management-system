using System;

namespace AppointmentManagementSystem.WpfClient
{
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
}
