using System;

namespace ServiceLocator.JSON.Registries
{
    public class BaseRegistry : IResolverRegistry
    {
        public BaseRegistry()
        {
            // Default Values
            Interface       = null;
            Class           = null;
            Multiple        = false;
        }

        public string Interface { get; set; }
        public string Class { get; set; }
        public bool Multiple { get; set; }
    }
}
