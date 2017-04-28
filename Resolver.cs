using ServiceLocator.JSON.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceLocator.JSON
{
    /// <summary>
    /// Dependency Resolver for providing Entities with the means
    /// to instantiate new objects or pull existing objects from
    /// the cache.
    /// </summary>
    /// <remarks>The default Entities and Interfaces Namespaces used are defined
    /// within the Application Values Registry as either the provided Application
    /// Setting or the default constant value stored within the Global Constants.
    /// </remarks>
    public class Resolver : IResolver
    {
        #region CONSTRUCTORS
        /// <summary>
        /// Dependency Resolver for providing Entities with the means
        /// to instantiate new objects or pull existing objects from
        /// the cache.
        /// </summary>
        public Resolver()
        {
            _mapper = new Mapper();
        }

        public Resolver(IList<IResolverModule> modules)
        {
            _mapper = new Mapper(modules);
        }

        public Resolver(IList<IResolverModule> modules, string registrationFileName)
        {
            _mapper = new Mapper(modules, registrationFileName);
        }

        public Resolver(IList<IResolverModule> modules, string registrationFileName, string binFolderLocation)
        {
            _mapper = new Mapper(modules, registrationFileName, binFolderLocation);
        }
        #endregion

        #region VARIABLES
        private static readonly InstantiatedObjectsRepository _repository = new InstantiatedObjectsRepository();
        private static Mapper _mapper;
        #endregion

        #region METHODS

        #region Resolve
        /// <summary>
        /// Retrieves the instance of the TInterface object that is either
        /// newly created and stored within the object cache or already
        /// existing in the object cache.
        /// </summary>
        /// <typeparam name="TInterface">The Interface type to retrieve.</typeparam>
        /// <exception cref="NullReferenceException">If the existing object cannot be found even after inserting a new one if needed.</exception>
        /// <returns>The instantiated object of the type of TInterface.</returns>
        public TInterface Resolve<TInterface>()
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();
            return (TInterface)Resolve(interfaceType, (object[])null);
        }

        /// <summary>
        /// Retrieves the instance of the TInterface object that is either
        /// newly created and stored within the object cache or already
        /// existing in the object cache.
        /// </summary>
        /// <typeparam name="TInterface">The Interface type to retrieve.</typeparam>
        /// <param name="constructorData">Values for instantiating the object with a respective constructor.</param>
        /// <exception cref="NullReferenceException">If the existing object cannot be found even after inserting a new one if needed.</exception>
        /// <returns>The instantiated object of the type of TInterface.</returns>
        public TInterface Resolve<TInterface>(object[] constructorData)
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();
            return (TInterface)Resolve(interfaceType, constructorData);
        }

        public TInterface Resolve<TInterface>(Func<object, IResolverModule, Type, IResolver, object> moduleLogic)
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();
            return (TInterface)Resolve(interfaceType, moduleLogic);
        }

        public TInterface Resolve<TInterface>(Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter)
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();
            return (TInterface)Resolve(interfaceType, moduleLogic, moduleFilter);
        }

        public object Resolve(Type interfaceType)
        {
            return ResolveCachedObjectWhileInsertingNewIfNotFound(interfaceType, null, null, null);
        }

        public object Resolve(Type interfaceType, object[] constructorData)
        {
            return ResolveCachedObjectWhileInsertingNewIfNotFound(interfaceType, constructorData, null, null);
        }

        public object Resolve(Type interfaceType, Func<object, IResolverModule, Type, IResolver, object> moduleLogic)
        {
            return ResolveCachedObjectWhileInsertingNewIfNotFound(interfaceType, null, moduleLogic, null);
        }

        public object Resolve(Type interfaceType, Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter)
        {
            return ResolveCachedObjectWhileInsertingNewIfNotFound(interfaceType, null, moduleLogic, moduleFilter);
        }
        #endregion

        #region ResolveNew
        public TInterface ResolveNew<TInterface>()
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();

            return (TInterface)ResolveNew(interfaceType, (object[])null);
        }

        public TInterface ResolveNew<TInterface>(object[] constructorData)
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();

            return (TInterface)ResolveNew(interfaceType, constructorData);
        }

        public TInterface ResolveNew<TInterface>(Func<object, IResolverModule, Type, IResolver, object> moduleLogic)
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();

            return (TInterface)ResolveNew(interfaceType, moduleLogic, null);
        }

        public TInterface ResolveNew<TInterface>(Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter)
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();

            return (TInterface)ResolveNew(interfaceType, moduleLogic, moduleFilter);
        }

        public object ResolveNew(Type interfaceType)
        {
            return InsertNewObjectToCache(interfaceType, null, null, null);
        }

        public object ResolveNew(Type interfaceType, object[] constructorData)
        {
            return InsertNewObjectToCache(interfaceType, constructorData, null, null);
        }

        public object ResolveNew(Type interfaceType, Func<object, IResolverModule, Type, IResolver, object> moduleLogic)
        {
            return InsertNewObjectToCache(interfaceType, null, moduleLogic, null);
        }

        public object ResolveNew(Type interfaceType, Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter)
        {
            return InsertNewObjectToCache(interfaceType, null, moduleLogic, moduleFilter);
        }
        #endregion

        #region ResolveAll
        public IEnumerable<TInterface> ResolveAll<TInterface>()
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();
            if (interfaceType == null)
                throw new NullReferenceException($"Failed to retrieve the type of \"{nameof(TInterface)}\"");

            return (IEnumerable<TInterface>)ResolveAll(interfaceType);
        }

        public IEnumerable<object> ResolveAll(Type interfaceType)
        {
            if (interfaceType == null)
                throw new ArgumentNullException("Cannot ResolveAll on a NULL Type.");

            if (!_repository.DoesObjectInterfaceExist(interfaceType))
                throw new ApplicationException($"Failed to locate any {nameof(InstantiatedObject)}'s of type \"{interfaceType.Name}\"");

            yield return _repository.ObjectCache
                .Where(obj => obj.InterfaceType == interfaceType)
                .Select(obj => obj.TheObject);
        }
        #endregion

        #region ResolveWithoutCaching
        /// <summary>
        /// Retrieves the instance of the TInterface object that is newly created.
        /// </summary>
        /// <typeparam name="TInterface">The Interface type to retrieve.</typeparam>
        /// <returns>The instantiated object of the type of TInterface.</returns>
        public TInterface ResolveWithoutCaching<TInterface>()
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();
            return (TInterface)ResolveWithoutCaching(interfaceType, (object[])null);
        }

        /// <summary>
        /// Retrieves the instance of the TInterface object that is newly created.
        /// </summary>
        /// <typeparam name="TInterface">The Interface type to retrieve.</typeparam>
        /// <param name="constructorData">Values for instantiating the object with a respective constructor.</param>
        /// <returns>The instantiated object of the type of TInterface.</returns>
        public TInterface ResolveWithoutCaching<TInterface>(object[] constructorData)
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();
            return (TInterface)ResolveWithoutCaching(interfaceType, constructorData);
        }

        public TInterface ResolveWithoutCaching<TInterface>(Func<object, IResolverModule, Type, IResolver, object> moduleLogic)
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();
            return (TInterface)ResolveWithoutCaching(interfaceType, moduleLogic, null);
        }

        public TInterface ResolveWithoutCaching<TInterface>(Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter)
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();
            return (TInterface)ResolveWithoutCaching(interfaceType, moduleLogic, moduleFilter);
        }

        /// <summary>
        /// Retrieves the instance of the TInterface object that is newly created.
        /// </summary>
        /// <param name="interfaceType">The Interface type to retrieve.</param>
        /// <returns>The instantiated object of the type of TInterface.</returns>
        public object ResolveWithoutCaching(Type interfaceType)
        {
            return ResolveWithoutCaching(interfaceType, (object[])null);
        }

        /// <summary>
        /// Retrieves the instance of the TInterface object that is newly created.
        /// </summary>
        /// <param name="interfaceType">The Interface type to retrieve.</param>
        /// <param name="constructorData">Values for instantiating the object with a respective constructor.</param>
        /// <returns>The instantiated object of the type of TInterface.</returns>
        public object ResolveWithoutCaching(Type interfaceType, object[] constructorData)
        {
            constructorData = AddResolverToConstructorDataIfNeeded(interfaceType, constructorData);
            return _mapper.MapInterfaceTypeToClassInstance(interfaceType, constructorData, this, null, null);
        }

        public object ResolveWithoutCaching(Type interfaceType, Func<object, IResolverModule, Type, IResolver, object> moduleLogic)
        {
            return ResolveWithoutCaching(interfaceType, moduleLogic, null);
        }

        public object ResolveWithoutCaching(Type interfaceType, Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter)
        {
            return _mapper.MapInterfaceTypeToClassInstance(interfaceType, null, this, moduleLogic, moduleFilter);
        }
        #endregion

        #region Object Cache
        /// <summary>
        /// Clears the cache of instantied objects.
        /// </summary>
        public void ClearCache()
        {
            _repository.ClearCache();
        }

        /// <summary>
        /// Retrieves a list of the instantiated objects as a
        /// list of their object cast ToString().
        /// </summary>
        /// <returns>A list of the instantiated objects as a
        /// list of their object cast ToString().</returns>
        public IList<string> ViewCache()
        {
            return _repository.ViewCache();
        }
        #endregion

        #region Modules
        public void RegisterModule(IResolverModule module)
        {
            _mapper.AddModule(module);
        }

        public bool RemoveModule(IResolverModule moduleToRemove)
        {
            return _mapper.RemoveModule(moduleToRemove);
        }
        #endregion

        #region PRIVATE METHODS
        private object[] AddResolverToConstructorDataIfNeeded<TInterface>(object[] constructorData)
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();
            return AddResolverToConstructorDataIfNeeded(interfaceType, constructorData);
        }

        private object[] AddResolverToConstructorDataIfNeeded(Type interfaceType, object[] constructorData)
        {
            // Check if we're provided no Constructor Data to work with
            if (constructorData == null || constructorData.Length <= 0)
            {
                // Check if no empty constructor defined (must be desiring a Resolver?)
                Type classType = _mapper.MapInterfaceTypeToClassType(interfaceType);
                if (!classType.GetConstructors().Any(c => c.GetParameters().Length <= 0))
                {
                    // Check if no constructor that only requests the IResolver is present
                    if (!classType.GetConstructors().Any(c => c.GetParameters().Length == 1 && c.GetParameters().First().ParameterType == typeof(IResolver)))
                        throw new InvalidOperationException($"The Interface \"{interfaceType.Name}\" mapping to Class \"{classType.FullName}\" requires custom constructor parameters that were not provided.");

                    return new[] { this };
                }
            }

            return constructorData;
        }

        private object InsertNewObjectToCache(Type interfaceType, object[] constructorData, Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter)
        {
            constructorData = AddResolverToConstructorDataIfNeeded(interfaceType, constructorData);
            InstantiatedObject newObject = _mapper.MapInterfaceTypeToInstantiatedObject(interfaceType, constructorData, this, moduleLogic, moduleFilter);

            // If multiple not allowed and one already exists, throw
            if (!newObject.AllowMultiple && _repository.DoesObjectInterfaceAndClassExist(newObject.InterfaceType, newObject.ClassType))
                throw new InvalidOperationException($"Interface Type \"{newObject.InterfaceType.Name}\" mapped to Class Type \"{newObject.ClassType.FullName}\" is not set to support multiple instances and one was already resolved before.");

            _repository.InsertObjectToCache(newObject);

            return newObject.TheObject;
        }

        private object ResolveCachedObjectWhileInsertingNewIfNotFound(Type interfaceType, object[] constructorData, Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter)
        {
            if (!interfaceType.IsInterface)
                throw new InvalidOperationException($"Incoming type \"{interfaceType.FullName}\" is not an Interface Type. Resolve only operates with Interfaces.");

            Type classType = _mapper.MapInterfaceTypeToClassType(interfaceType);

            // If none found, create the first one
            if (!_repository.DoesObjectInterfaceAndClassExist(interfaceType, classType))
            {
                if (constructorData != null && moduleLogic != null)
                    throw new InvalidOperationException($"Resolving should not be provided BOTH {nameof(constructorData)} and {nameof(moduleLogic)}.");

                InsertNewObjectToCache(interfaceType, constructorData, moduleLogic, moduleFilter);
            }

            InstantiatedObject existingObject = _repository.ObjectCache.FirstOrDefault(obj => obj.ClassType == classType && obj.InterfaceType == interfaceType);
            if (existingObject == null || existingObject == default(InstantiatedObject))
                throw new NullReferenceException($"Failed to locate an object that instantiates type \"{interfaceType.Name}\" in the {nameof(InstantiatedObjectsRepository)}.");

            return existingObject.TheObject;
        }
        #endregion
        #endregion
    }
}
