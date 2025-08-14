using System;
using System.Collections.Generic;
using Core.Repositories;
using Core.Services;
using Infrastructure.Repositories;
using Infrastructure.Services;

namespace Infrastructure.DependencyInjection
{
    /// <summary>
    /// Simple dependency injection container for managing service lifetimes.
    /// Implements Inversion of Control principle.
    /// </summary>
    public class ServiceContainer
    {
        private readonly Dictionary<Type, Func<object>> _services = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, object> _singletonInstances = new Dictionary<Type, object>();

        /// <summary>
        /// Registers a transient service (new instance created each time).
        /// </summary>
        public void RegisterTransient<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            _services[typeof(TInterface)] = () => CreateInstance(typeof(TImplementation));
        }

        /// <summary>
        /// Registers a transient service with a factory method.
        /// </summary>
        public void RegisterTransient<TInterface>(Func<TInterface> factory)
        {
            _services[typeof(TInterface)] = () => factory();
        }

        /// <summary>
        /// Registers a singleton service (same instance returned each time).
        /// </summary>
        public void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            _services[typeof(TInterface)] = () =>
            {
                if (!_singletonInstances.ContainsKey(typeof(TInterface)))
                {
                    _singletonInstances[typeof(TInterface)] = CreateInstance(typeof(TImplementation));
                }
                return _singletonInstances[typeof(TInterface)];
            };
        }

        /// <summary>
        /// Registers a singleton service with a factory method.
        /// </summary>
        public void RegisterSingleton<TInterface>(Func<TInterface> factory)
        {
            _services[typeof(TInterface)] = () =>
            {
                if (!_singletonInstances.ContainsKey(typeof(TInterface)))
                {
                    _singletonInstances[typeof(TInterface)] = factory();
                }
                return _singletonInstances[typeof(TInterface)];
            };
        }

        /// <summary>
        /// Resolves a service instance.
        /// </summary>
        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        /// <summary>
        /// Resolves a service instance by type.
        /// </summary>
        public object Resolve(Type serviceType)
        {
            if (_services.ContainsKey(serviceType))
            {
                return _services[serviceType]();
            }

            throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered");
        }

        /// <summary>
        /// Creates an instance of the specified type with constructor injection.
        /// </summary>
        private object CreateInstance(Type type)
        {
            var constructors = type.GetConstructors();
            var constructor = constructors[0]; // Take the first constructor

            var parameters = constructor.GetParameters();
            var args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                args[i] = Resolve(parameters[i].ParameterType);
            }

            return Activator.CreateInstance(type, args);
        }

        /// <summary>
        /// Configures all application services.
        /// </summary>
        public static ServiceContainer ConfigureServices()
        {
            var container = new ServiceContainer();

            // Register repositories
            container.RegisterTransient<IProductRepository, ProductRepository>();

            // Register services
            container.RegisterTransient<IProductService, ProductService>();

            return container;
        }
    }
}