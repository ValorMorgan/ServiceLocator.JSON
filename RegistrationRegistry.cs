using System;

namespace ServiceLocator.JSON
{
    internal class RegistrationRegistry : IRegistrationRegistry
    {
        public RegistrationRegistry()
        {
            // Default Values
            Interface       = null;
            Class           = null;
            Multiple        = false;
            Factory         = null;
            FactoryMethod   = null;
        }

        public string Interface { get; set; }
        public string Class { get; set; }
        public bool Multiple { get; set; }
        public string Factory { get; set; }
        public string FactoryMethod { get; set; }
    }
}
