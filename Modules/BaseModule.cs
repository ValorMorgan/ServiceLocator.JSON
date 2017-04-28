using ServiceLocator.JSON.Parsers;
using System;

namespace ServiceLocator.JSON.Modules
{
    public abstract class BaseModule
    {
        #region CONSTRUCTORS
        public BaseModule()
        {
        }

        public BaseModule(string registrationFileName)
        {
            _parser = new RegistrationParser(registrationFileName);
        }

        public BaseModule(string registrationFileName, string binFolderLocation)
        {
            _parser = new RegistrationParser(registrationFileName, binFolderLocation);
        }
        #endregion

        #region VARIABLES
        private RegistrationParser _parser;
        private RegistryParser _registryParser;
        #endregion

        #region PROPERTIES
        public RegistrationParser RegistrationParser => _parser ?? (_parser = new RegistrationParser());
        public RegistryParser RegistryParser => _registryParser ?? (_registryParser = new RegistryParser(RegistrationParser));
        #endregion
    }
}
