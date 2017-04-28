using ServiceLocator.JSON.Modules;
using ServiceLocator.JSON.Parsers;
using ServiceLocator.JSON.Registries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ServiceLocator.JSON
{
    /// <summary>
    /// Handles mapping provided Types to their respective counter-parts.
    /// </summary>
    internal class Mapper
    {
        #region CONSTRUCTORS
        /// <summary>
        /// Handles mapping provided Types to their respective counter-parts.
        /// </summary>
        /// <remarks>Uses the Global Constant values for the Entities and Interfaces namespaces.</remarks>
        internal Mapper()
        {
            _parser = new RegistrationParser();
            _registryParser = new RegistryParser(_parser);
            _modules = new List<IResolverModule>();
            RegisterPrivateModules();
        }

        internal Mapper(IList<IResolverModule> modules)
            : this()
        {
            if (_modules == null)
                _modules = new List<IResolverModule>();

            foreach (IResolverModule module in modules)
                AddModule(module);
        }

        internal Mapper(IList<IResolverModule> modules, string registrationFileName)
            : this(modules)
        {
            _parser = new RegistrationParser(registrationFileName);
            _registryParser = new RegistryParser(_parser);
        }

        internal Mapper(IList<IResolverModule> modules, string registrationFileName, string binFolderLocation)
            : this(modules)
        {
            _parser = new RegistrationParser(registrationFileName, binFolderLocation);
            _registryParser = new RegistryParser(_parser);
        }
        #endregion

        #region VARIALBES
        private RegistrationParser _parser;
        private RegistryParser _registryParser;

        private IList<IResolverModule> _modules;
        #endregion

        #region METHODS

        #region Map To Type
        public Type MapInterfaceTypeToClassType<TInterface>()
        {
            Type interfaceType = GetInterfaceType<TInterface>();
            return MapInterfaceTypeToClassType(interfaceType);
        }

        public Type MapInterfaceTypeToClassType(Type interfaceType)
        {
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType), $"Cannot map a NULL Interface Type to a Class Type.");

            if (!interfaceType.IsInterface)
                throw new InvalidOperationException($"Interface Type \"{interfaceType.FullName}\" did not resolve to an Interface.");

            IResolverRegistry registry = _parser.GetRegistrationRegistry(interfaceType.Name);
            return _registryParser.GetRegistryClassType(registry);
        }
        #endregion

        #region Map To Instance
        /// <summary>
        /// Maps the provided TInterface to a respective Class instance.
        /// </summary>
        /// <typeparam name="TInterface">The Interface Type to map.</typeparam>
        /// <param name="constructorData">Values for instantiating the object with a respective constructor.</param>
        /// <param name="resolver">The Dependency Resolver for object instantiation.</param>
        /// <returns>A Class instance of the Interface Type.</returns>
        public TInterface MapInterfaceTypeToClassInstance<TInterface>(object[] constructorData, IResolver resolver)
        {
            Type interfaceType = GetInterfaceType<TInterface>();
            return (TInterface)MapInterfaceTypeToClassInstance(interfaceType, constructorData, resolver, null, null);
        }

        /// <summary>
        /// Maps the provided TInterface to a respective Class instance.
        /// </summary>
        /// <param name="interfaceType">The Interface Type to map.</param>
        /// <param name="constructorData">Values for instantiating the object with a respective constructor.</param>
        /// <param name="resolver">The Dependency Resolver for object instantiation.</param>
        /// <returns>A Class instance of the Interface Type.</returns>
        public object MapInterfaceTypeToClassInstance(Type interfaceType, object[] constructorData, IResolver resolver)
        {
            return MapInterfaceTypeToClassInstance(interfaceType, constructorData, resolver, null, null);
        }

        public object MapInterfaceTypeToClassInstance(Type interfaceType, object[] constructorData, IResolver resolver, Func<object, IResolverModule, Type, IResolver, object> moduleLogic)
        {
            return MapInterfaceTypeToClassInstance(interfaceType, constructorData, resolver, moduleLogic, null);
        }

        public object MapInterfaceTypeToClassInstance(Type interfaceType, object[] constructorData, IResolver resolver, Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter)
        {
            // Check if we have moduler logic to act on (otherwise we are doing normal resolving operations)
            if (moduleLogic != null)
            {
                IList<IResolverModule> modules = _modules;
                if (moduleFilter != null)
                    modules = moduleFilter(modules);

                object moduleReturn = null;
                foreach (IResolverModule module in modules)
                    moduleReturn = moduleLogic(moduleReturn, module, interfaceType, resolver);

                if (moduleReturn != null)
                    return moduleReturn;

                throw new NullReferenceException($"{nameof(IResolverModule)}'s were acted upon for Interface \"{interfaceType.Name}\" but no object was returned. If modules are used, at least one {nameof(IResolverModule)} should return the finished object and no others should overwrite the return with NULL.");
            }

            Type classType = MapInterfaceTypeToClassType(interfaceType);
            return CreateClasInstance(classType, constructorData);
        }
        #endregion

        #region Map To InstantiatedObject
        public InstantiatedObject MapInterfaceTypeToInstantiatedObject<TInterface>(object[] constructorData, IResolver resolver, Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter)
        {
            Type interfaceType = GetInterfaceType<TInterface>();
            return MapInterfaceTypeToInstantiatedObject(interfaceType, constructorData, resolver, moduleLogic, moduleFilter);
        }

        public InstantiatedObject MapInterfaceTypeToInstantiatedObject(Type interfaceType, object[] constructorData, IResolver resolver, Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter)
        {
            object newInstance = MapInterfaceTypeToClassInstance(interfaceType, constructorData, resolver, moduleLogic, moduleFilter);
            IResolverRegistry registry = _parser.GetRegistrationRegistry(interfaceType.Name);

            return new InstantiatedObject()
            {
                InterfaceType = interfaceType,
                ClassType = newInstance.GetType(),
                TheObject = newInstance,
                AllowMultiple = registry.Multiple
            };
        }
        #endregion

        #region Get Type
        /// <summary>
        /// Gets the Type of the provided TClass.
        /// </summary>
        /// <typeparam name="TClass">The Class Type to retrieve.</typeparam>
        /// <exception cref="InvalidOperationException">If the provided TClass is not a Class or is not part of the Mapper's Entities namespace.</exception>
        /// <exception cref="NullReferenceException">If typeof(TClass) fails to return the TClass type.</exception>
        /// <returns>The Type of TClass.</returns>
        /// <remarks>Checks if the provided TClass is not a Class or is not part of the Mapper's Entities namespace.</remarks>
        public Type GetClassType<TClass>()
        {
            Type classType = typeof(TClass);
            if (classType == null)
                throw new NullReferenceException($"Failed to retrieve the incoming Class Type of {nameof(TClass)}.");

            if (!classType.IsClass)
                throw new InvalidOperationException($"Class Type \"{classType.FullName}\" did not resolve to a Class.");

            return classType;
        }

        /// <summary>
        /// Gets the Type of the provided TInterface.
        /// </summary>
        /// <typeparam name="TInterface">The Interface Type to retrieve.</typeparam>
        /// <exception cref="InvalidOperationException">If the provided TInterface is not a Interface or is not part of the Mapper's Interfaces namespace.</exception>
        /// <exception cref="NullReferenceException">If typeof(TInterface) fails to return the TInterface type.</exception>
        /// <returns>The Type of TInterface.</returns>
        /// <remarks>Checks if the provided TInterface is not a Interface or is not part of the Mapper's Interfaces namespace.</remarks>
        public Type GetInterfaceType<TInterface>()
        {
            Type interfaceType = typeof(TInterface);
            if (interfaceType == null)
                throw new NullReferenceException($"Failed to retrieve the incoming Interface Type of {nameof(TInterface)}.");

            if (!interfaceType.IsInterface)
                throw new InvalidOperationException($"Interface Type \"{interfaceType.FullName}\" did not resolve to an Interface.");

            return interfaceType;
        }

        /// <summary>
        /// Tries to get the provided object's Type.
        /// </summary>
        /// <param name="objectToGetType">The object to get the Type from.</param>
        /// <param name="typeOfObject">The output Type of the object.<note type="note">Is the out parameter.</note></param>
        /// <returns>True if the type could be retrieved and False otherwise.</returns>
        /// <remarks>Any exceptions will be digested.</remarks>
        public bool TryGetObjectType(dynamic objectToGetType, out Type typeOfObject)
        {
            typeOfObject = null;
            if (objectToGetType == null)
                return false;

            try
            {
                typeOfObject = objectToGetType.GetType();
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Modules
        public void AddModule(IResolverModule module)
        {
            _modules.Add(module);
        }

        public bool RemoveModule(IResolverModule moduleToRemove)
        {
            return _modules.Remove(moduleToRemove);
        }
        #endregion

        #region PRIVATE METHODS
        private object CreateClasInstance(Type classType, object[] constructorData)
        {
            if (!classType.IsClass)
                throw new InvalidOperationException($"Can only create instances from Class Types. Incoming Type: \"{classType.FullName}\"");

            Assembly assemblyToUse = Assembly.GetAssembly(classType);
            return Activator.CreateInstance(assemblyToUse.GetType(classType.FullName), constructorData);
        }

        private void RegisterPrivateModules()
        {
            AddModule(new FactoryModule());
        }
        #endregion

        #endregion
    }
}