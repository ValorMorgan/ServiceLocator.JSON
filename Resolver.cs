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
        #endregion

        #region VARIABLES
        private static readonly InstantiatedObjectsRegistry _registry = new InstantiatedObjectsRegistry();
        private static Mapper _mapper;
        #endregion

        #region METHODS
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
            return Resolve<TInterface>(null);
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

            return (TInterface) Resolve(interfaceType, constructorData);
        }

        public object Resolve(Type interfaceType)
        {
            return Resolve(interfaceType, null);
        }

        public object Resolve(Type interfaceType, object[] constructorData)
        {
            if (!interfaceType.IsInterface)
                throw new InvalidOperationException($"Incoming type \"{interfaceType.FullName}\" is not an Interface Type. Resolve only operate with Interfaces.");

            Type classType = _mapper.MapInterfaceTypeToClassType(interfaceType);

            // If none found, create the first one
            if (!_registry.DoesObjectInterfaceAndClassExist(interfaceType, classType))
            {
                InsertNewObjectToCache(interfaceType, constructorData);
            }

            InstantiatedObject existingObject = _registry.ObjectCache.FirstOrDefault(obj => obj.ClassType == classType && obj.InterfaceType == interfaceType);
            if (existingObject == null || existingObject == default(InstantiatedObject))
                throw new NullReferenceException($"Failed to locate an object that instantiates type \"{interfaceType.FullName}\" in the {nameof(InstantiatedObjectsRegistry)}.");

            return existingObject.TheObject;
        }

        public TInterface ResolveNew<TInterface>()
        {
            return ResolveNew<TInterface>(null);
        }

        public TInterface ResolveNew<TInterface>(object[] constructorData)
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();
            Type classType = _mapper.MapInterfaceTypeToClassType(interfaceType);

            return InsertNewObjectToCache<TInterface>(constructorData);
        }

        public IEnumerable<TInterface> ResolveAll<TInterface>()
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();
            if (interfaceType == null)
                throw new NullReferenceException($"Failed to retrieve the type of \"{nameof(TInterface)}\"");

            if (!_registry.DoesObjectInterfaceExist(interfaceType))
                throw new ApplicationException($"Failed to locate any {nameof(InstantiatedObject)}'s of type \"{interfaceType.FullName}\"");

            yield return (TInterface) _registry.ObjectCache
                .Where(obj => obj.InterfaceType == interfaceType)
                .Select(obj => obj.TheObject);
        }

        /// <summary>
        /// Retrieves the instance of the TInterface object that is newly created.
        /// </summary>
        /// <typeparam name="TInterface">The Interface type to retrieve.</typeparam>
        /// <returns>The instantiated object of the type of TInterface.</returns>
        public TInterface ResolveWithoutCaching<TInterface>()
        {
            return ResolveWithoutCaching<TInterface>(null);
        }

        /// <summary>
        /// Retrieves the instance of the TInterface object that is newly created.
        /// </summary>
        /// <typeparam name="TInterface">The Interface type to retrieve.</typeparam>
        /// <param name="constructorData">Values for instantiating the object with a respective constructor.</param>
        /// <returns>The instantiated object of the type of TInterface.</returns>
        public TInterface ResolveWithoutCaching<TInterface>(object[] constructorData)
        {
            // Enforce always using a Resolver as the first parameter
            constructorData = AddResolverToConstructorData(constructorData);
            return _mapper.MapInterfaceTypeToClassInstance<TInterface>(constructorData, this);
        }

        /// <summary>
        /// Clears the cache of instantied objects.
        /// </summary>
        public void ClearCachedObjects()
        {
            _registry.ClearCachedObjects();
        }

        /// <summary>
        /// Retrieves a list of the instantiated objects as a
        /// list of their object cast ToString().
        /// </summary>
        /// <returns>A list of the instantiated objects as a
        /// list of their object cast ToString().</returns>
        public IList<string> ViewCache()
        {
            return _registry.ViewCache();
        }

        #region PRIVATE METHODS
        private object[] AddResolverToConstructorData(object[] constructorData)
        {
            if (constructorData == null)
                return new[] { this };

            // Check if an IResolver is already used in the first parameter
            if (constructorData[0].GetType() == typeof(IResolver))
                return constructorData;

            IList<object> newConstructorData = new List<object>();
            newConstructorData.Add(this);

            foreach (object dataField in constructorData)
                newConstructorData.Add(dataField);

            return newConstructorData.ToArray();
        }

        private TInterface InsertNewObjectToCache<TInterface>(object[] constructorData)
        {
            Type interfaceType = _mapper.GetInterfaceType<TInterface>();
            return (TInterface) InsertNewObjectToCache(interfaceType, constructorData);
        }

        private object InsertNewObjectToCache(Type interfaceType, object[] constructorData)
        {
            constructorData = AddResolverToConstructorData(constructorData);
            InstantiatedObject newObject = _mapper.MapInterfaceTypeToInstantiatedObject(interfaceType, constructorData, this);

            // If multiple not allowed and one already exists, throw
            if (!newObject.AllowMultiple && _registry.DoesObjectInterfaceAndClassExist(newObject.InterfaceType, newObject.ClassType))
                throw new InvalidOperationException($"Interface Type \"{newObject.InterfaceType.FullName}\" mapped to Class Type \"{newObject.ClassType.FullName}\" is not set to support multiple instances and one was already resolved before.");

            _registry.InsertObjectToCache(newObject);

            return newObject.TheObject;
        }
        #endregion
        #endregion
    }
}
