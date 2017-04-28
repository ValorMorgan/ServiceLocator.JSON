using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceLocator.JSON
{
    /// <summary>
    /// Houses all the Instantiated Objects within the Resolver.
    /// </summary>
    internal class InstantiatedObjectsRepository
    {
        #region VARIABLES
        private static IList<InstantiatedObject> _objectCache = new List<InstantiatedObject>();
        #endregion

        #region PROPERTIES
        /// <summary>
        /// The physical cache of Instantied Objects.
        /// </summary>
        /// <remarks>If the private cache object is null, the cache will be instantiated as new, empty list.</remarks>
        public IList<InstantiatedObject> ObjectCache
        {
            get
            {
                if (_objectCache == null)
                    _objectCache = new List<InstantiatedObject>();
                return _objectCache;
            }
        }
        #endregion

        #region METHODS
        /// <summary>
        /// Inserts the provided object to the cache of instantiated objects.
        /// </summary>
        /// <param name="objectToInsert">The object to insert into the cache.</param>
        public void InsertObjectToCache(InstantiatedObject objectToInsert)
        {
            if (objectToInsert == null)
                throw new ArgumentNullException(nameof(objectToInsert), $"Cannot add a NULL {nameof(InstantiatedObject)} to the {nameof(InstantiatedObjectsRepository)}");

            objectToInsert.Validate();

            // Replace the existing object if multiples not allowed and it exists already
            if (!objectToInsert.AllowMultiple && DoesObjectInterfaceAndClassExist(objectToInsert.InterfaceType, objectToInsert.ClassType))
            {
                InstantiatedObject objectToRemove = ObjectCache.First(obj => obj.InterfaceType == objectToInsert.InterfaceType && obj.ClassType == objectToInsert.ClassType);
                if (objectToRemove == null)
                    throw new NullReferenceException($"The object mapped to \"{objectToInsert.InterfaceType.Name}\" is marked to not allow Multiple, was found in the Repository, but could not be retrieved.");

                ObjectCache.Remove(objectToRemove);
            }

            ObjectCache.Add(objectToInsert);
        }

        /// <summary>
        /// Clears the cache of instantied objects.
        /// </summary>
        public void ClearCache()
        {
            // Dispose all objects.
            foreach (InstantiatedObject obj in ObjectCache)
                obj.Dispose();

            ObjectCache.Clear();
        }

        /// <summary>
        /// Retrieves a list of the instantiated objects as a
        /// list of their object cast ToString().
        /// </summary>
        /// <returns>A list of the instantiated objects as a
        /// list of their object cast ToString().</returns>
        public IList<string> ViewCache()
        {
            IList<string> cachedObjects = new List<string>();

            foreach (InstantiatedObject cachedObject in ObjectCache)
                cachedObjects.Add(cachedObject.ToString());

            return cachedObjects;
        }

        /// <summary>
        /// Checks if the provided Interface Type exists in the object cache.
        /// </summary>
        /// <param name="interfaceType">The Interface Type to check for.</param>
        /// <exception cref="ArgumentNullException">If the provided Type is NULL.</exception>
        /// <returns>True if the provided Interface Type exists in the cache and False otherwise.</returns>
        public bool DoesObjectInterfaceExist(Type interfaceType)
        {
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType), "The provided Interfaces Type cannot be NULL.");

            return ObjectCache.Any(obj => obj.InterfaceType == interfaceType);
        }

        /// <summary>
        /// Checks if the provided Class Type exists in the object cache.
        /// </summary>
        /// <param name="classType">The Class Type to check for.</param>
        /// <exception cref="ArgumentNullException">If the provided Type is NULL.</exception>
        /// <returns>True if the provided Class Type exists in the cache and False otherwise.</returns>
        public bool DoesObjectClassExist(Type classType)
        {
            if (classType == null)
                throw new ArgumentNullException(nameof(classType), "The provided Class Type cannot be NULL.");

            return ObjectCache.Any(obj => obj.ClassType == classType);
        }

        /// <summary>
        /// Checks if the provided Interface Type and Class Type exists in the object cache.
        /// </summary>
        /// <param name="interfaceType">The Interface Type to check for.</param>
        /// <param name="classType">The Class Type to check for.</param>
        /// <exception cref="ArgumentNullException">If either provided Type is NULL.</exception>
        /// <returns>True if the provided Interface Type and Class Type exists in the cache and False otherwise.</returns>
        public bool DoesObjectInterfaceAndClassExist(Type interfaceType, Type classType)
        {
            return DoesObjectInterfaceExist(interfaceType) && DoesObjectClassExist(classType);
        }

        /// <summary>
        /// Checks if the provided Object exists in the object cache.
        /// </summary>
        /// <param name="objectToCheck">The object to check for.</param>
        /// <returns>True if the provided object exists in the cache and False otherwise.</returns>
        public bool DoesObjectExist(dynamic objectToCheck)
        {
            return ObjectCache.Any(obj => obj.TheObject == objectToCheck);
        }

        /// <summary>
        /// Checks if the provided InstantiatedObject exists in the object cache.
        /// </summary>
        /// <param name="objectToCheck">The InstantiatedObject to check for.</param>
        /// <returns>True if the provided InstantiatedObject exists in the cache and False otherwise.</returns>
        public bool DoesInstantiatedObjectExist(InstantiatedObject objectToCheck)
        {
            return ObjectCache.Contains(objectToCheck);
        }
        #endregion
    }
}