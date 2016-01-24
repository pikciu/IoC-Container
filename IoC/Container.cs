using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IoC
{
    public class Container : IContainer
    {
        private readonly bool _overridingAllowed;
        private readonly List<RegisteredType> _registeredTypes;

        public Container(bool allowOverride = false)
        {
            _registeredTypes = new List<RegisteredType>();
            _overridingAllowed = allowOverride;
        }

        public void Register<TIface, TImpl>(LifeCycle lifeCycle = LifeCycle.PerRequest)
            where TImpl : TIface
        {
            CheckType(typeof(TIface));
            var newType = new RegisteredType(typeof(TIface), typeof(TImpl), lifeCycle);
            _registeredTypes.Add(newType);
        }

        public void Register<TIface, TImpl>(TImpl implementation, LifeCycle lifeCycle = LifeCycle.Singleton)
            where TImpl : TIface
        {
            CheckType(typeof(TIface));
            var newType = new RegisteredType(typeof(TIface), typeof(TImpl), lifeCycle);
            if (newType.LifeCycle == LifeCycle.Singleton)
            {
                newType.Factory = () => implementation;
            }
            _registeredTypes.Add(newType);
        }

        public void Register(Type implmentationType, LifeCycle lifeCycle = LifeCycle.PerRequest)
        {
            CheckType(implmentationType);
            var newType = new RegisteredType(implmentationType, lifeCycle);
            _registeredTypes.Add(newType);
        }

        public void Register<TIface, TImpl>(Func<TImpl> factory, LifeCycle lifeCycle = LifeCycle.PerRequest)
            where TImpl : TIface
        {
            CheckType(typeof(TIface));
            var newType = new RegisteredType(typeof(TIface), typeof(TImpl), lifeCycle)
            {
                Factory = () => factory()
            };
            _registeredTypes.Add(newType);
        }

        public void RegisterAssembly(string assemblyName)
        {
            Assembly assembly = Assembly.Load(new AssemblyName(assemblyName));

            RegisterAssembly(assembly);
        }

        public void RegisterAssembly(Assembly assembly)
        {
            Type installerType = assembly.ExportedTypes.FirstOrDefault(x => x.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IAssemblyInstaller)));

            if (installerType == null)
            {
                throw new InvalidOperationException($"Installer not found in {assembly.FullName}");
            }
            var installer = (IAssemblyInstaller)Activator.CreateInstance(installerType);

            installer.Install(this);
        }

        public object Resolve(Type type)
        {
            RegisteredType registeredType = _registeredTypes.FirstOrDefault(p => p.AsType == type);
            if (registeredType == null)
            {
                throw new InvalidOperationException($"Type {type.Name} not registered.");
            }

            if (registeredType.LifeCycle == LifeCycle.Singleton)
            {
                object instance = registeredType.Factory == null ? GetInstance(registeredType.ImplementationType) : registeredType.Factory();
                registeredType.Factory = () => instance;
                return registeredType.Factory();
            }

            if (registeredType.Factory != null)
            {
                return registeredType.Factory();
            }

            return GetInstance(registeredType.ImplementationType);
        }

        public TImpl Resolve<TImpl>()
            where TImpl : class
        {
            return (TImpl)Resolve(typeof(TImpl));
        }

        private object GetInstance(Type type)
        {
            object[] parameters = GetConstructorParameters(type);
            object instance = Activator.CreateInstance(type, parameters);
            return instance;
        }

        private object[] GetConstructorParameters(Type type)
        {
            ConstructorInfo constructorInfo = type.GetConstructors().FirstOrDefault();
            if (constructorInfo == null)
            {
                return new object[0];
            }
            return constructorInfo.GetParameters().Select(parameter => Resolve(parameter.ParameterType)).ToArray();
        }

        private void CheckType(Type type)
        {
            RegisteredType registered = _registeredTypes.FirstOrDefault(p => p.AsType == type);
            if (registered != null)
            {
                if (_overridingAllowed)
                {
                    _registeredTypes.Remove(registered);
                }
                else
                {
                    throw new InvalidOperationException($"Type {type.Name} already registered.");
                }
            }
        }
    }
}