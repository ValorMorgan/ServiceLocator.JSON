using System;

namespace ServiceLocator.JSON.Registries
{
    class FactoryRegistry : BaseRegistry, IResolverRegistry
    {
        public FactoryRegistry()
            : base()
        {
            // Default Values
            Factory = null;
            FactoryMethod = null;
        }

        public string Factory { get; set; }
        public string FactoryMethod { get; set; }
    }
}
