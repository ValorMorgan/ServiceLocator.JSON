using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;

namespace ServiceLocator.JSON
{
    internal class RegistrationParser
    {
        #region VARIABLES
        private string _registrationFileName = "registration.json";
        #endregion

        #region PROPERTIES
        public string BinFolder
        {
            get
            {
                string webBin = $"{HostingEnvironment.ApplicationPhysicalPath}\\bin\\";
                string consoleBin = Directory.GetCurrentDirectory();

                // Using this approach to confirm we are getting the correct Bin folder and not a random one
                string webPath = Path.Combine(webBin, _registrationFileName).Trim();
                string consolePath = Path.Combine(consoleBin, _registrationFileName).Trim();

                if (!string.IsNullOrWhiteSpace(webPath) && File.Exists(webPath))
                    return webBin;
                else if (!string.IsNullOrWhiteSpace(consolePath) && File.Exists(consolePath))
                    return consoleBin;

                throw new DirectoryNotFoundException($"Failed to locate the {nameof(BinFolder)} at \"{webBin ?? "NULL"}\" nor \"{consoleBin ?? "NULL"}\"");
            }
        }
        public string RegistrationFile
        {
            get
            {
                string path = Path.Combine(BinFolder, _registrationFileName).Trim();

                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    return path;

                throw new FileNotFoundException($"Failed to locate the {nameof(RegistrationFile)} at \"{path ?? "NULL"}\"", _registrationFileName);
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
        internal IEnumerable<string> GetInterfaceAssemblies()
        {
            foreach (string value in Interfaces.Values<string>())
                yield return (value.EndsWith(".dll")) ? value : $"{value}.dll";
        }

        internal IEnumerable<string> GetEntityAssemblies()
        {
            foreach (string value in Entities.Values<string>())
                yield return (value.EndsWith(".dll")) ? value : $"{value}.dll";
        }

        internal IEnumerable<JToken> GetRegistration()
        {
            foreach (JToken token in Registration.Children())
                yield return token;
        }

        internal IRegistrationRegistry GetRegistrationRegistry(string interfaceName)
        {
            IRegistrationRegistry interfaceRegistry = null;
            foreach (JToken token in GetRegistration())
            {
                // NOTE: ToObject requires a Class Type. Will need custom mapping logic for any custom IRegistrationRegistry
                IRegistrationRegistry registry = token.ToObject<RegistrationRegistry>();
                if (registry.Interface.Equals(interfaceName))
                {
                    interfaceRegistry = registry;
                    break;
                }
            }

            ThrowExceptionIfRegistryNotValid(interfaceRegistry, interfaceName);

            return interfaceRegistry;
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
        private void ThrowExceptionIfRegistryNotValid(IRegistrationRegistry registry, string interfaceName)
        {
            if (registry == null || !registry.Interface.Equals(interfaceName) || string.IsNullOrWhiteSpace(registry.Class))
                throw new ApplicationException($"Registration for {interfaceName} is not setup correctly.");
        }
        #endregion

        #endregion
    }
}
