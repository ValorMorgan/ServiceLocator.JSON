using ServiceLocator.JSON.Registries;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;

namespace ServiceLocator.JSON.Parsers
{
    public class RegistrationParser
    {
        #region CONSTRUCTORS
        public RegistrationParser()
        {
        }

        public RegistrationParser(string registrationFileName)
        {
            if (string.IsNullOrWhiteSpace(registrationFileName))
                throw new NullReferenceException($"Registration File name cannot be NULL.");

            _registrationFileName = registrationFileName;
        }

        public RegistrationParser(string registrationFileName, string binFolderLocation)
            : this(registrationFileName)
        {
            if (string.IsNullOrWhiteSpace(binFolderLocation))
                throw new NullReferenceException($"Bin Folder location cannot be NULL.");

            _customBin = (binFolderLocation.EndsWith("\\")) ? binFolderLocation : $"{binFolderLocation}\\";
        }
        #endregion

        #region VARIABLES
        private string _registrationFileName = "registration.json";
        private string _customBin = null;
        private string _webBin = $"{HostingEnvironment.ApplicationPhysicalPath}\\bin\\";
        private string _consoleBin = Directory.GetCurrentDirectory();
        #endregion

        #region PROPERTIES
        public string BinFolder
        {
            get
            {
                // Using this approach to confirm we are getting the correct Bin folder and not a random one
                string customPath = (_customBin != null) ? Path.Combine(_customBin, _registrationFileName).Trim() : null;
                string webPath = (_webBin != null) ? Path.Combine(_webBin, _registrationFileName).Trim() : null;
                string consolePath = (_consoleBin != null) ? Path.Combine(_consoleBin, _registrationFileName).Trim() : null;

                if (File.Exists(customPath))
                    return _customBin;
                else if (File.Exists(webPath))
                    return _webBin;
                else if (File.Exists(consolePath))
                    return _consoleBin;

                throw new DirectoryNotFoundException($"Failed to locate the {nameof(BinFolder)} at \"{_customBin ?? "no Custom Bin"}\", \"{_webBin ?? "no Web Bin"}\", nor \"{_consoleBin ?? "no Console Bin"}\"");
            }
        }
        public string RegistrationFile
        {
            get
            {
                string path = Path.Combine(BinFolder, _registrationFileName).Trim();
                if (string.IsNullOrWhiteSpace(path))
                    throw new NullReferenceException($"No {nameof(BinFolder)} that would contain that {nameof(RegistrationFile)} was found or provided.");

                if (File.Exists(path))
                    return path;

                throw new FileNotFoundException($"Failed to locate the {nameof(RegistrationFile)} at \"{path}\"", _registrationFileName);
            }
        }
        private JObject RegistrationFileContents => JObject.Parse(
            File.ReadAllText(RegistrationFile)
        );

        private JToken Assemblies => RegistrationFileContents["Assemblies"];
        private JToken Interfaces => Assemblies["Interfaces"];
        private JToken Entities => Assemblies["Entities"];
        private JToken Registration => RegistrationFileContents["Registration"];
        #endregion

        #region METHODS
        public IEnumerable<string> GetInterfaceAssemblies()
        {
            foreach (string value in Interfaces.Values<string>())
                yield return (value.EndsWith(".dll")) ? value : $"{value}.dll";
        }

        public IEnumerable<string> GetEntityAssemblies()
        {
            foreach (string value in Entities.Values<string>())
                yield return (value.EndsWith(".dll")) ? value : $"{value}.dll";
        }

        public IEnumerable<JToken> GetRegistration()
        {
            foreach (JToken token in Registration.Children())
                yield return token;
        }

        public IResolverRegistry GetRegistrationRegistry(string interfaceName)
        {
            IResolverRegistry interfaceRegistry = null;
            foreach (JToken token in GetRegistration())
            {
                IResolverRegistry registry = token.ToObject<BaseRegistry>();
                if (registry.Interface.Equals(interfaceName))
                {
                    interfaceRegistry = registry;
                    break;
                }
            }

            ThrowExceptionIfRegistryNotValid(interfaceRegistry, interfaceName);

            return interfaceRegistry;
        }

        public TResolverRegistry GetRegistrationRegistry<TResolverRegistry>(string interfaceName) where TResolverRegistry : BaseRegistry
        {
            IResolverRegistry interfaceRegistry = null;
            foreach (JToken token in GetRegistration())
            {
                // NOTE: ToObject requires a Class Type. Potentially will need custom mapping logic for any custom IResolverRegistry
                IResolverRegistry registry = token.ToObject<TResolverRegistry>();
                if (registry.Interface.Equals(interfaceName))
                {
                    interfaceRegistry = registry;
                    break;
                }
            }

            ThrowExceptionIfRegistryNotValid(interfaceRegistry, interfaceName);

            return (TResolverRegistry) interfaceRegistry;
        }

        public IEnumerable<Type> GetClassTypesInAssemblies()
        {
            foreach (string classAssembly in GetEntityAssemblies())
            {
                string assemblyPath = Path.Combine(BinFolder, classAssembly);

                foreach (Type type in Assembly.LoadFile(assemblyPath).GetTypes())
                {
                    if (type == null || !type.IsClass)
                        continue;

                    yield return type;
                }
            }
        }

        public IEnumerable<Type> GetInterfaceTypesInAssemblies()
        {
            foreach (string classAssembly in GetInterfaceAssemblies())
            {
                string assemblyPath = Path.Combine(BinFolder, classAssembly);

                foreach (Type type in Assembly.LoadFile(assemblyPath).GetTypes())
                {
                    if (type == null || !type.IsInterface)
                        continue;

                    yield return type;
                }
            }
        }

        public IEnumerable<Type> GetPublicClassTypesInAssemblies()
        {
            foreach (Type type in GetClassTypesInAssemblies().Where(t => t.IsPublic))
            {
                if (type == null)
                    continue;

                yield return type;
            }
        }

        public IEnumerable<Type> GetPublicInterfaceTypesInAssemblies()
        {
            foreach (Type type in GetInterfaceTypesInAssemblies().Where(t => t.IsPublic))
            {
                if (type == null)
                    continue;

                yield return type;
            }
        }

        #region PRIVATE METHODS
        private void ThrowExceptionIfRegistryNotValid(IResolverRegistry registry, string interfaceName)
        {
            if (registry == null ||
                (string.IsNullOrWhiteSpace(registry.Interface) || !registry.Interface.Equals(interfaceName)) ||
                string.IsNullOrWhiteSpace(registry.Class))
                throw new ApplicationException($"Registration for {interfaceName} is not setup correctly.");
        }
        #endregion

        #endregion
    }
}
