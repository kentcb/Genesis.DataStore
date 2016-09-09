![Logo](Art/Logo150x150.png "Logo")

# Genesis.DataStore

[![Build status](https://ci.appveyor.com/api/projects/status/0chenkm5csk5hg58?svg=true)](https://ci.appveyor.com/project/kentcb/genesis-datastore)

## What?

> All Genesis.* projects are formalizations of small pieces of functionality I find myself copying from project to project. Some are small to the point of triviality, but are time-savers nonetheless. They have a particular focus on performance with respect to mobile development, but are certainly applicable outside this domain.
 
**Genesis.DataStore** is a library that makes it simpler to implement a SQLite-based, versioned data store for your application. It is delivered as a PCL targeting a wide range of platforms:

* .NET 4.5
* Windows 8
* Windows Store
* Windows Phone 8
* Xamarin iOS
* Xamarin Android

## Why?

When an application includes a SQLite data store, it will often want to version that data store over time. If an upgrade to the application requires database changes, those changes should be applied without loss of existing data. **Genesis.DataStore** provides a means of versioning a data store and simplifying the process of upgrading it over time.

## Where?

The easiest way to get **Genesis.DataStore** is via [NuGet](http://www.nuget.org/packages/Genesis.DataStore/):

```PowerShell
Install-Package Genesis.DataStore
```

## How?

To use **Genesis.DataStore**, you create and use an instance of `DataStoreService`. Doing so requires that you provide it with a scheduler (generally you will want to use a dedicated `EventLoopScheduler`) as well as a means of creating an `IUpgradeHandlerProvider`.

```C#
// connection to the database
var connection = ...;
// scheduler on which to execute database operations
var scheduler = new EventLoopScheduler();
// logger to write log messages to
var logger = new Logger(typeof(DataStoreService));

var dataStoreService = new DataStoreService(
    connection,
    scheduler,
    logger,
    () => new MyUpgradeHandlerRepository());
```

Once you have a `DataStoreService`, using it is straightforward. When your application starts, you will want to call `Upgrade` so that any database upgrades are applied. Note that this method is asynchronous. Depending on your platform, you may need to block the UI thread while the upgrade takes place.

```C#
var dataStoreService = ...;

dataStoreService
    .Upgrade()
    .Do(_ => /* do other stuff as a continuation */)
    .Subscribe();
```

Perhaps the most important thing to understand is how `IUpgradeHandlerProvider` comes into play. Whenever you request an upgrade, the data store service will compare the versions in the provider with the actual version of the database. If the database is older than the versions specified in the provider, it will apply each outstanding upgrade in order. Thus, when a new release of your application needs to make changes to the database, you should define a new upgrade handler and ensure your upgrade handler provider returns an instance of it. Your provider should _always_ return the default handler (`UpgradeHandler_0_0_1`) because this handler creates the `version` table required by the data store service.

```C#
// here is our upgrade handler for V0.0.2 of our database
public class UpgradeHandler_0_0_2 : UpgradeHandler
{
    public override Version => new Version(0, 0, 2);

    protected override void ApplyCore(IDatabaseConnection connection)
    {
        // here is where you'd create any tables, modify tables, insert data etcetera
        // whatever you do here defines what it means for your database to be at version 0.0.2
    }
}

// this is the provider you pass into the data store service constructor
var upgradeHandlerProvider = new UpgradeHandlerProvider(
    new[]
    {
        // always include the 0.0.1 handler
        new UpgradeHandler_0_0_1(),
        // the initial version of our database is 0.0.2
        new UpgradeHandler_0_0_2()
        // add any future versions after this
    });

```

## Who?

**Genesis.DataStore** is created and maintained by [Kent Boogaart](http://kent-boogaart.com). Issues and pull requests are welcome.