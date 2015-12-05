using System;

namespace IoC
{
    internal class RegisteredType
    {
        public RegisteredType(Type asType, Type implementationType, LifeCycle lifeCycle)
        {
            AsType = asType;
            ImplementationType = implementationType;
            LifeCycle = lifeCycle;
        }

        public RegisteredType(Type implementationType, LifeCycle lifeCycle)
        {
            AsType = ImplementationType = implementationType;
            LifeCycle = lifeCycle;
        }

        public LifeCycle LifeCycle { get; private set; }

        public object Instance { get; set; }

        public Type AsType { get; private set; }

        public Type ImplementationType { get; private set; }
    }
}