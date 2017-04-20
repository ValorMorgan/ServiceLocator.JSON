using System;

namespace ServiceLocator.JSON
{
    internal class RegistrationRegistryParser
    {
        #region CONSTRUCTORS
        public RegistrationRegistryParser()
        {
            _parser = new RegistrationParser();
        }
        #endregion

        #region VARIABLES
        RegistrationParser _parser;
        #endregion

        #region METHODS
        public Type GetRegistryClassType(IRegistrationRegistry registry)
        {
            foreach (Type classType in _parser.GetClassTypesInAssemblies())
            {
                if (classType.FullName.Equals(registry.Class))
                    return classType;
            }

            throw new TypeAccessException($"Failed to map the provided Interface Type \"{registry.Interface}\" to a respective Class Type in the Registration. Is the Registration properly setup?");
        }
        #endregion
    }
}
