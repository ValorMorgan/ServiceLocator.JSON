# ServiceLocator.JSON
C# Service Locator based on a JSON configuration. 

## Features
* JSON configuration (as compared to the classic XML)
* Resolve upon Resolve upon Resolve
  * Can call for new Entities that require the Resolver to get other objects, etc. Should be able to go pretty deep.
* Singleton & multiple instances support
* Multiple source Assemblies
* Modularity of Registration by registering new IResolveModule's to the Resolver
* Delegate query support within the Resolve commands for custom behavior/selection of Modules

## Built-in Module(s)
* Factory - Factory creation from specified sources in Registration

## External Dependencies
Newtonsoft.Json : https://github.com/JamesNK/Newtonsoft.Json

## Future Improvements
The following list is a collection of improvements I want to add to the system to help it be more robust and increase both performance and community support:
* Designation of default Module operations and filters (currently need to call Resolve with the delegate each time).
* Support of Constructor Data with module calls (two are exclusive to one another).
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
  // NOTE: If the Resolver is provided a parameter collection that is not NULL or an empty collection, it assumes that is the final list of parameters
  ILogger _logger = _resolver.Resolve<ILogger>(new object[] { _resolver, "Application" });
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

## Module(s)
IResolverModule's are a powerful feature that allows you to customize the ServiceLocator to fit your needs at any given moment in the application process.  Just like the built-in Factory module, IResolverModule's allow for the injection of custom logic that is tied to a Registry's value collection.  As well, the Resolver supports delegate querying and overriding of Module calling code for every Resolve call.  This means that if one Resolve needs custom logic around the modules or an object with or without a specific module interfering, the Resolve call will support the injection of said logic as desired.  With the Module Logic delegate, you can perform surrounding code for each module that could be unique to a given instance in the source code (i.e. some form of Trace logging for each module call).  And with the Module Filtering delegate, you can filter the needed/unneeded module(s) or even re-order them as desired.

### To Create a New Module...
You will need to create a new Class that extends "BaseModule" and implements "IResolveModule".  If the module needs to act on a Registration outside the default Bin locations, you may setup the Constructor to call the equivelent "BaseModule" construction code.
``` c#
public class FactoryModule : BaseModule, IResolverModule
{
    public FactoryModule()
        : base()
    {
    }

    public FactoryModule(string registrationFileName)
        : base(registrationFileName)
    {
    }

    public FactoryModule(string registrationFileName, string binFolderLocation)
        : base(registrationFileName, binFolderLocation)
    {
    }

    public object ExecuteModule(Type interfaceType, IResolver resolver, object existingObject)
    {
      // ... Module logic ...
    }
}
```

### To Create a New Registry Value...
You will need to create a new class that extends "BaseRegistry" and implements "IResolverRegistry".  This registy will serve as the access point for the new Registry value for your Module to operate as expected.  This component is optional if you desire to have behvaior that is registered to an Entity.  The module could contain logic that does not require the Registration to operate and would operate on an already existing object as desired.
``` c#
public class FactoryRegistry : BaseRegistry, IResolverRegistry
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
```

### To Register a New Module...
You will either call the "RegisterModule" method of the source Resolver or call the Resolver constructor that takes in a list of IResolverModules at construction.
``` c#
public Main (string[] args)
{
    // First create the module
    IResolverModule module = new FactoryModule();
  
    // Either register the module at construction
    IResolver resolver = new Resolver(new List<IResolverModule>(){ module });
  
    // Or register it after creation
    resolver.RegisterModule(module);
}
```

### To Use the New Module(s)...
You will need to add any Registration changes needed for your Entities as desired (if any were setup for your module(s)) and then call for the module(s) at the Resolve call.  When calling with the modules, no constructorData should be passed in.  The two approaches are exclusive to one another.
``` c#
public Main (string[] args)
{
    IResolverModule module = new FactoryModule();
    IResolver resolver = new Resolver(new List<IResolverModule>(){ module });
    
    // Notice that the constructorData is not used for these kind of calls.
    ILogger logger = resolver.Resolve<ILogger>(
        // Surrounding module logic.  Note that there should be a call to ExecuteModule to perform the actual module logic.
        (obj, module, iType, resolver) =>
        {
            return module.ExecuteModule(iType, resolver, obj);
        },
        
        // Not needed here but proof of concept to only retrieve modules that have "factory" in the name
        (moduleList) =>
        {
            return moduleList.Where(m => m.GetType().Name.ToLower().Contains("factory")).ToList();
        }
    );
}
```
