using System;
using System.Reflection;

namespace IoC
{
    public interface IContainer
    {
        void Register<TIface, TImpl>(LifeCycle lifeCycle = LifeCycle.PerRequest)
            where TImpl : TIface;

        void Register<TIface, TImpl>(TImpl implementation, LifeCycle lifeCycle = LifeCycle.Singleton)
            where TImpl : TIface;

        void Register(Type implmentationType, LifeCycle lifeCycle = LifeCycle.PerRequest);

        void Register<TIface, TImpl>(Func<TImpl> factory, LifeCycle lifeCycle = LifeCycle.PerRequest)
            where TImpl : TIface;

        void RegisterAssembly(string assemblyName);

        void RegisterAssembly(Assembly assembly);

        object Resolve(Type type);

        TImpl Resolve<TImpl>()
            where TImpl : class;
    }
}