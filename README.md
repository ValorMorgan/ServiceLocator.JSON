# ServiceLocator.JSON
C# Service Locator based on a JSON configuration.  Fairly simple and restrictive structure but gets the job done.

## Features
* JSON configuratin (as compared to the classic XML)
* Resolve upon Resolve upon Resolve
  * Can call for new Entities that require the Resolver to get other objects, etc. Should be able to go pretty deep.
* Singleton & multiple instances support
* Factory creation from specified sources in Registration
* Multiple source Assemblies

## External Dependencies
Newtonsoft.Json : https://github.com/JamesNK/Newtonsoft.Json

## Known Limitations / Restrictions
This system enforces that all Entities or Classes have a base Construction using the IResolver only (no empty Constructor supported).  As well, any other Constructors that use more parameters must start with the IResolver as the first parameter.

## Future Improvements
The following list is a collection of improvements I want to add to the system to help it be more robust and increase both performance and community support:
* Allow Assemblies to come from any location and not just the Bin folder 
* Modularity of IRegistrationRegistry to support custom Registration parameters and logic
  * Thus far, this is looking like the use IRegistryModule that will be acted on with Func<IRegistryModule, object> operations
* De-coupling of current Registration logic to moddable class and interface
* Async support
* Large amounts of Resolver calls support (i.e. system with many clients requesting objects)

## Resolver
The Resolver is the primary entry point for resolving any desired Entities.  Simply pass in a specified Interface and let the Resolving system locate singular or multiple instances of the mapped class.  All of this also abides by the Registration within the JSON file.

Example of call for an object that only uses the Resolver as the Constructor parameter:
``` c#
public void Main(object[] args)
{
  IResolver _resolver = new Resolver();
  ILogger _logger = _resolver.Resolve<ILogger>();
  _logger.LogMessage("Logger is instaniated and works as expected.");
}

public class Logger : ILogger
{
  public Logger(IResolver resolver)
  {
    // ... Resolve any internal dependencies through the provided IResolver
  }
  
  public void LogMessage(string message)
  {
    // ... Log the message
  }
}

public interface ILogger
{
  void LogMessage(string message);
}
```

Example of call for an object that uses the Resolver and other parameters in the Constructor:
``` c#
public void Main(object[] args)
{
  IResolver _resolver = new Resolver();
  ILogger _logger = _resolver.Resolve<ILogger>(new object[] { "Application" });
  _logger.LogMessage("Logger is instaniated and works as expected.");
}

public class Logger : ILogger
{
  public Logger(IResolver resolver)
  {
    // ... Resolve any internal dependencies through the provided IResolver
  }
  
  public Logger(IResolver resolver, string logSource)
    : this(resolver)
  {
    _source = logSource;
  }
  
  private string _source;
  
  public void LogMessage(string message)
  {
    // ... Log the message
  }
}

public interface ILogger
{
  void LogMessage(string message);
}
```

## Registration.json
Registration.json structure appears like the following (also found at the top of Registration.json):
```
Handles the registration setup for the Resolver to determine
the matched class for the provided Interface.
 
"Assemblies": [
  "Extension": (optional; default: "dll") "Extension of assemblies files",
  "Interfaces": [ ... Any Interface Assemblies ... ],
  "Entities": [ ... Any Entities/Classes assemblies ... ]
]
 
"Registration": [
  {
    "interface": "Name of the Interface (no namespace)"
    "class": "Full namespace and name of class",
    "multiple": (optional; default: false) true if transient (multiple instances) and false if singleton (only one instance)
    "factory": (optional; required with "factoryMethod") "Name of Factory Interface (no namespace; should match another entry in Registration)",
    "factoryMethod": (optional; required with "factory") "Name of method to generate class"
  }
]
```
Example of some basic setup could be:
``` json
"Assemblies": {
 "Interfaces": [
  "ServiceLocator.JSON.Interfaces"
 ],

 "Entities": [
  "ServiceLocator.JSON.Entities"
 ]
},

"Registration": [
 {
  "interface": "ILogger",
  "class": "ServiceLocator.JSON.Entities.Logger",
  "factory": "ILoggerFactory",
  "factoryMethod": "GenerateLogger"
 },
 {
  "interface": "ILoggerFactory",
  "class": "ServiceLocator.JSON.Entities.LoggerFactory"
 },
 {
  "interface": "IApple",
  "class": "ServiceLocator.JSON.Entities.Apple",
  "multiple": true
 }
]
```
