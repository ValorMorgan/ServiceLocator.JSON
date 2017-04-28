using ServiceLocator.JSON.Modules;
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
        TInterface Resolve<TInterface>();
        TInterface Resolve<TInterface>(object[] constructorData);
        TInterface Resolve<TInterface>(Func<object, IResolverModule, Type, IResolver, object> moduleLogic);
        TInterface Resolve<TInterface>(Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter);
        object Resolve(Type interfaceType);
        object Resolve(Type interfaceType, object[] constructorData);
        object Resolve(Type interfaceType, Func<object, IResolverModule, Type, IResolver, object> moduleLogic);
        object Resolve(Type interfaceType, Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter);

        TInterface ResolveNew<TInterface>();
        TInterface ResolveNew<TInterface>(object[] constructorData);
        TInterface ResolveNew<TInterface>(Func<object, IResolverModule, Type, IResolver, object> moduleLogic);
        TInterface ResolveNew<TInterface>(Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter);
        object ResolveNew(Type interfaceType);
        object ResolveNew(Type interfaceType, object[] constructorData);
        object ResolveNew(Type interfaceType, Func<object, IResolverModule, Type, IResolver, object> moduleLogic);
        object ResolveNew(Type interfaceType, Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter);

        IEnumerable<TInterface> ResolveAll<TInterface>();
        IEnumerable<object> ResolveAll(Type interfaceType);

        TInterface ResolveWithoutCaching<TInterface>();
        TInterface ResolveWithoutCaching<TInterface>(object[] constructorData);
        TInterface ResolveWithoutCaching<TInterface>(Func<object, IResolverModule, Type, IResolver, object> moduleLogic);
        TInterface ResolveWithoutCaching<TInterface>(Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter);
        object ResolveWithoutCaching(Type interfaceType);
        object ResolveWithoutCaching(Type interfaceType, object[] constructorData);
        object ResolveWithoutCaching(Type interfaceType, Func<object, IResolverModule, Type, IResolver, object> moduleLogic);
        object ResolveWithoutCaching(Type interfaceType, Func<object, IResolverModule, Type, IResolver, object> moduleLogic, Func<IList<IResolverModule>, IList<IResolverModule>> moduleFilter);

        void ClearCache();
        IList<string> ViewCache();

        void RegisterModule(IResolverModule module);
        bool RemoveModule(IResolverModule moduleToRemove);
    }
}