

# Puresharp API .NET 4.5.2+
Puresharp is a set of features for .NET 4.5.2+ to improve productivity by producing flexible and efficient applications.

## Overview
Puresharp mainly provides architectural tools to build the basics of professional applications :
- Dependency Injection Container
- Aspect Oriented Programming
- Metadata Reflection API

This framework is divided into 2 parts :
- **IPuresharp** &nbsp;&nbsp;[![NuGet](https://img.shields.io/nuget/v/IPuresharp.svg)](https://www.nuget.org/packages/IPuresharp)

IPuresharp is a nuget package dedicated to rewrite assemblies (using **[Mono.Cecil](https://github.com/jbevain/cecil)**) to allow them to be highly customizable at runtime. IPuresharp won't add a new library reference to assmblies, but only include a post build process to automatically rewrite assemblies just after success build.

    Install-Package IPuresharp -Version 3.1.2

It can be used manually with command line to manage third party assemblies

    IPuresharp.exe "FullnameToAssembly.dll"

- **Puresharp** &nbsp;&nbsp;[![NuGet](https://img.shields.io/nuget/v/Puresharp.svg)](https://www.nuget.org/packages/Puresharp)

Puresharp is a nuget package offering various features useful for designing a healthy and productive architecture. This package also includes all the artillery to easily handle the elements that brings the IL writer IPuresharp. The nuget package add a library (Puresharp.dll) without any other depencies.

    Install-Package Puresharp -Version 3.1.2

_note : It is recommanded to install **IPuresharp** nuget package in all projects and install **Puresharp** nuget package only in project where you explicitly need it._

### Dependency Injection Container

The global workflow of the DI Container is similar to others : setup a composition, create a container and instantiate from container some components.

#### Preview

Example of interfaces to configure

    public interface IA
    {
    }

    public interface IB
    {
    }

    public interface IC
    {
    }
    
Example of implementations to bind to interfaces

    public class A : IA
    {
        public A(IB b, IC c)
        {
        }
    }

    public class B : IB
    {
        public B(IC c, int constant)
        {
        }
    }

    public class C : IC
    {
    }

Create a composition

    var _composition = new Composition();

Setup composition for IA, IB, IC whith respectivily A, B, C

    _composition.Setup<IA>(() => new A(Metadata<IB>.Value, Metadata<IC>.Value), Instantiation.Multiton);
    _composition.Setup<IB>(() => new B(Metadata<IC>.Value, 28), Instantiation.Multiton);
    _composition.Setup<IC>(() => new C(), Instantiation.Multiton);
    
Create a container from composition setup

    var _container = new Container(_composition);

Instantiate a module of IA from container

    using (var _module = _container.Module<IA>())
    {
        var _ia = _module.Value;
    }
    
_note : module is [IDisposable](https://msdn.microsoft.com/en-us/library/system.idisposable(v=vs.110).aspx) and crontrol lifecycle for all dependencies._

#### FAQ

- **How is managed lifecycle for dependencies?** 
_When a module is setup into composition, instantiation mode is required and can be **Singleton** (a single instance with a lifecycle related to container), **Multiton** (a new instance for each module with lifecycle related to module itself) or **Volatile** (always a new instance with lifecycle related to owner module). Container and Module are both IDisposable to release created components._

- **Why using lambda expression to configure components instead of classic generic parameter?** 
_Lambda expression offer a way to target constructor to use, specify when to use dependencies and capture constant._

- **How dependency is configured?** 
_Simply use Metadata&lt;T&gt;.Value into lambda expression when configuring a component._

- **Is constructor injection prevent cyclic reference betwwen component?** 
_No, cyclic references are a feature. When an instance is created, it is not really the case, a lazy proxy instance is prepared to minimize unused resources retention and allow cyclic references._

### Aspect Oriented Programming

Workflow :
- Identify group of methods by defining a **Pointcut**
- Specify an **Aspect** by defining some **Advices**
- Instantiate the **Aspect** and **Weave** it into **Pointcut**

#### Preview

Example of interface

    [AttributeUsage(AttributeTargets.Method)]
    public class Read : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class Operation : Attribute
    {
    }

    public interface IService
    {
        [Operation]
        void SaveComment(int id, string text);

        [Read]
        [Operation]
        string GetComment(int id);
    }
    
Example of implementation

    public class Service : IService
    {
        public void SaveComment(int id, string text)
        {
        }

        public string GetComment(int id)
        {
            return null;
        }
    }
    
Define a **Pointcut** that represent all methods that are readonly operation (where Read attribute and Operation attribute are placed

    public class ReadonlyOperation : Pointcut.And<Pointcut<Operation>, Pointcut<Read>>
    {
    }
    
Define an **Advice** to call Trace.WriteLine when calling methods

    public class Log : IAdvice
    {
        private MethodBase m_Method;

        public Log(MethodBase method)
        {
            this.m_Method = method;
        }

        public void Instance<T>(T instance)
        {
        }

        public void Argument<T>(ref T value)
        {
        }

        public void Begin()
        {
            Trace.WriteLine(this.m_Method);
        }

        public void Await(MethodInfo method, Task task)
        {
        }

        public void Await<T>(MethodInfo method, Task<T> task)
        {
        }
        
        public void Continue()
        {
        }
        
        public void Throw(ref Exception exception)
        {
        }

        public void Throw<T>(ref Exception exception, ref T value)
        {
        }

        public void Return()
        {
        }

        public void Return<T>(ref T value)
        {
        }
        
        public void Dispose()
        {
        }
    }
    
Define an **Aspect** that use log **Advice**

    public class Logging : Aspect
    {
        override public IEnumerable<Func<IAdvice>> Advise(MethodBase method)
        {
            yield return () => new Log(method);
        }
    }
    
Instantiate **Aspect** and weave it to our ReadonlyOperation **Pointcut**

    var _logging = new Logging();
    _logging.Weave<ReadonlyOperation>();
    
#### FAQ

- **Can I weave multiple Aspects into the same Pointcut?** 
_Yes, just be carefull about weaving order._

- **How can I remove an Aspect from a Pointcut?** 
_There is a **Release** method defined in **Aspect** to get rid of **Aspect** from **Pointcut**._

- **Are attributes required to define Pointcut?** 
_No, **Pointcut** can be defined by directly inherits from **Pointcut** and implement the abstract method **Match** that take a **MethodBase** as single argument and return a boolean to indicate if a method is in **Pointcut** scope._

- **Why I have to use IPuresharp?** 
_Interception is based on IPuresharp. Indeed IPuresharp add a build action to rewrite CIL to make assembly "Architect Friendly" by injecting transparents and hidden features to to grant full execution control at runtime._

- **Can I intercept constructor? If yes, how do I implement it?**
_Constructor interception is supported and is treated like another method with declaring type as first argument and void for return type._

- **Is generic types and methods supported?**
_Generic types and methods ares fully supported by injection._

- **Can I intercept async methods?**
_Async methods ares fully supported by injection and offer a way to intercept each asynchronous steps._

## More
- [https://en.m.wikipedia.org/wiki/Aspect-oriented_programming](https://en.m.wikipedia.org/wiki/Aspect-oriented_programming)
- [https://en.m.wikipedia.org/wiki/Dependency_injection](https://en.m.wikipedia.org/wiki/Dependency_injection)
- [https://en.m.wikipedia.org/wiki/SOLID](https://en.m.wikipedia.org/wiki/SOLID)
- [https://en.m.wikipedia.org/wiki/Domain-driven_design](https://en.m.wikipedia.org/wiki/Domain-driven_design)

## Contribution
- Feedbacks are wellcome, don't hesitate to tell me if any improvements seems to be cool
- Any pull requests for patching or correct my bad english are wellcome too
- You can also donate to help financially maintain the project : **3D8txm4gMJDtti7tpcYnndLDfxpADkLggn**




