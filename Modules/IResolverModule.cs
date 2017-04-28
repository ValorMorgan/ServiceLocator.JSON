using System;

namespace ServiceLocator.JSON.Modules
{
    public interface IResolverModule
    {
        object ExecuteModule(Type interfaceType, IResolver resolver, object existingObject);
    }
}