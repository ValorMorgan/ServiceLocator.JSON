using System;
using System.Collections.Generic;

namespace ServiceLocator.JSON
{
    /// <summary>
    /// Dependency Resolver for providing Entities with the means
    /// to instantiate new objects or pull existing objects from
    /// the cache.
    /// </summary>
    /// <remarks>The default Entities and Interfaces Namespaces used are defined
    /// within the Application Values Registry as either the provided Application
    /// Setting or the default constant value stored within the Global Constants.</remarks>
    public interface IResolver
    {
        /// <summary>
        /// Clears the cache of instantied objects.
        /// </summary>
        void ClearCachedObjects();
        /// <summary>
        /// Retrieves the instance of the TInterface object that is either
        /// newly created and stored within the object cache or already
        /// existing in the object cache.
        /// </summary>
        /// <typeparam name="TInterface">The Interface type to retrieve.</typeparam>
        /// <exception cref="NullReferenceException">If the existing object cannot be found even after inserting a new one if needed.</exception>
        /// <returns>The instantiated object of the type of TInterface.</returns>
        TInterface Resolve<TInterface>();
        /// <summary>
        /// Retrieves the instance of the TInterface object that is either
        /// newly created and stored within the object cache or already
        /// existing in the object cache.
        /// </summary>
        /// <typeparam name="TInterface">The Interface type to retrieve.</typeparam>
        /// <param name="constructorData">Values for instantiating the object with a respective constructor.</param>
        /// <exception cref="NullReferenceException">If the existing object cannot be found even after inserting a new one if needed.</exception>
        /// <returns>The instantiated object of the type of TInterface.</returns>
        TInterface Resolve<TInterface>(object[] constructorData);
        object Resolve(Type interfaceType);
        object Resolve(Type interfaceType, object[] constructorData);
        /// <summary>
        /// Retrieves the instance of the TInterface object that is newly created.
        /// </summary>
        /// <typeparam name="TInterface">The Interface type to retrieve.</typeparam>
        /// <returns>The instantiated object of the type of TInterface.</returns>
        TInterface ResolveWithoutCaching<TInterface>();
        /// <summary>
        /// Retrieves the instance of the TInterface object that is newly created.
        /// </summary>
        /// <typeparam name="TInterface">The Interface type to retrieve.</typeparam>
        /// <param name="constructorData">Values for instantiating the object with a respective constructor.</param>
        /// <returns>The instantiated object of the type of TInterface.</returns>
        TInterface ResolveWithoutCaching<TInterface>(object[] constructorData);
        /// <summary>
        /// Retrieves a list of the instantiated objects as a
        /// list of their object cast ToString().
        /// </summary>
        /// <returns>A list of the instantiated objects as a
        /// list of their object cast ToString().</returns>
        IList<string> ViewCache();
    }
}