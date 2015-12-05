using System;

namespace IoC
{
    public interface IContainer
    {
        void Register<TInterface, TImplementation>(LifeCycle lifeCycle = LifeCycle.PerRequest)
            where TImplementation : TInterface;

        void Register<TInterface, TImplementation>(TImplementation implementation)
            where TImplementation : TInterface; 

        void Register<TImplementation>(TImplementation implementation, LifeCycle lifeCycle = LifeCycle.PerRequest)
            where TImplementation : class;

        object Resolve(Type type);

        TImplementation Resolve<TImplementation>()
            where TImplementation : class;
    }
}