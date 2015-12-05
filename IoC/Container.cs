using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IoC
{
    public class Container : IContainer
    {
        private readonly List<RegisteredType> _registeredTypes;

        public Container()
        {
            _registeredTypes = new List<RegisteredType>();
        }

        public void Register<TInterface, TImplementation>(LifeCycle lifeCycle = LifeCycle.PerRequest)
            where TImplementation : TInterface
        {
            CheckType(typeof(TInterface));

            var newType = new RegisteredType(typeof(TInterface), typeof(TImplementation), lifeCycle);
            _registeredTypes.Add(newType);
        }

        public void Register<TInterface, TImplementation>(TImplementation implementation)
            where TImplementation : TInterface
        {
            CheckType(typeof(TInterface));

            var newType = new RegisteredType(typeof(TInterface), typeof(TImplementation), LifeCycle.Singleton)
            {
                Instance = implementation
            };
            _registeredTypes.Add(newType);
        }

        public void Register<TImplementation>(TImplementation implementation, LifeCycle lifeCycle = LifeCycle.PerRequest)
            where TImplementation : class
        {
            CheckType(typeof(TImplementation));

            var newType = new RegisteredType(typeof(TImplementation), lifeCycle);
            if (lifeCycle == LifeCycle.Singleton)
            {
                newType.Instance = implementation;
            }

            _registeredTypes.Add(newType);
        }

        public object Resolve(Type type)
        {
            RegisteredType registeredType = _registeredTypes.FirstOrDefault(p => p.AsType == type);
            if (registeredType == null)
            {
                throw new InvalidOperationException(string.Format("Type {0} not registered.", type.Name));
            }

            if (registeredType.LifeCycle == LifeCycle.Singleton)
            {
                if (registeredType.Instance == null)
                {
                    registeredType.Instance = GetInstance(registeredType.ImplementationType);
                }

                return registeredType.Instance;
            }

            return GetInstance(registeredType.ImplementationType);
        }

        public TImplementation Resolve<TImplementation>()
            where TImplementation : class
        {
            return (TImplementation)Resolve(typeof(TImplementation));
        }

        private object GetInstance(Type type)
        {
            object[] parameters = GetConstructorParameters(type);
            object instance = Activator.CreateInstance(type, parameters);
            return instance;
        }

        private object[] GetConstructorParameters(Type type)
        {
            ConstructorInfo constructorInfo = type.GetConstructors().First();
            return constructorInfo.GetParameters().Select(parameter => Resolve(parameter.ParameterType)).ToArray();
        }

        private void CheckType(Type type)
        {
            if (_registeredTypes.Any(p => p.AsType == type))
            {
                throw new InvalidOperationException(string.Format("Type {0} already registered.", type.Name));
            }
        }
    }
}