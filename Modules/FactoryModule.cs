using ServiceLocator.JSON.Registries;
using System;
using System.Reflection;

namespace ServiceLocator.JSON.Modules
{
    internal class FactoryModule : BaseModule, IResolverModule
    {
        #region CONSTRUCTORS
        public FactoryModule()
            : base()
        {
        }

        public FactoryModule(string registrationFileName)
            : base(registrationFileName)
        {
        }

        public FactoryModule(string registrationFileName, string binFolderLocation)
            : base(registrationFileName, binFolderLocation)
        {
        }
        #endregion

        public object ExecuteModule(Type interfaceType, IResolver resolver, object existingObject)
        {
            // If we already have an object, just return that object (this module is for creating a new one from the Factory)
            if (existingObject != null)
                return existingObject;

            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver), $"{nameof(FactoryModule)} requires a non-null {nameof(IResolver)}");
            else if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType), $"{nameof(FactoryModule)} requires a non-null {nameof(interfaceType)}");

            FactoryRegistry registry = RegistrationParser.GetRegistrationRegistry<FactoryRegistry>(interfaceType.Name);

            if (string.IsNullOrWhiteSpace(registry.Factory))
                throw new InvalidOperationException($"No factory setup was registered to the Interface \"{interfaceType.Name}\" within the Registration.");

            // Generate class from Factory
            IResolverRegistry factoryRegistry = RegistrationParser.GetRegistrationRegistry(registry.Factory);
            foreach (Type factoryInterfaceType in RegistrationParser.GetInterfaceTypesInAssemblies())
            {
                if (!factoryInterfaceType.Name.Equals(factoryRegistry.Interface))
                    continue;

                object factory = resolver.Resolve(factoryInterfaceType);
                MethodInfo factoryMethod = factory.GetType()
                    .GetMethod(registry.FactoryMethod);

                try
                {
                    ParameterInfo parameter = factoryMethod.GetParameters()[0];
                    if (parameter.ParameterType != typeof(IResolver))
                        throw new NotSupportedException($"{nameof(FactoryModule)} does not support calling factory methods that require unique parameters beyond just the {nameof(IResolver)}.");

                    return factoryMethod.Invoke(factory, new[] { resolver });
                }
                catch (IndexOutOfRangeException) // No parameters thus the GetParameters()[0} would throw this
                {
                    return factoryMethod.Invoke(factory, null);
                }
            }

            throw new TypeAccessException($"Interface \"{interfaceType.Name}\" was marked to use a Factory but the Factory could not be mapped to. Searched for \"{factoryRegistry.Class}\"");
        }
    }
}
