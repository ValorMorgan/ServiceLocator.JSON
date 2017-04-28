using ServiceLocator.JSON.Registries;
using System;

namespace ServiceLocator.JSON.Parsers
{
    public class RegistryParser
    {
        #region CONSTRUCTORS
        public RegistryParser()
        {
            _parser = new RegistrationParser();
        }

        public RegistryParser(RegistrationParser parser)
        {
            _parser = parser;
        }
        #endregion

        #region VARIABLES
        RegistrationParser _parser;
        #endregion

        #region METHODS
        public Type GetRegistryClassType(IResolverRegistry registry)
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
