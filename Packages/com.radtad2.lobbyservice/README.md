# Lobby Service
A small generic lobby package that decouples front and back ends and hides all bookkeeping logic. Also ships with a local lobby 
server so you can test your entire lobby flow with the real game UI all on one machine.

---

## Features
- Supports generic backends.
- Optional provider features which can be implemented or safely ignored without risk of NRE or bad result values.
- Push-only front ends. 
- Local lobby server provided for one-machine testing.
- Optionally auto buffers calls before lobby initialization.
- Steamworks.NET provider given as a sample.

---
## Disclaimer
This system was developed as tech dev for my own projects. Due to time, it has _not_ been thoroughly tested
and assuming correctness is a dangerous game to play :).
If you run into issues or have suggestions, feel free to open an issue on GitHub... no promises, but I’ll help when I can.

## Installation

In the package manager select Add by git url and paste the following:

```bash
https://github.com/TadDiDio/Lobby-Service.git?path=Packages/com.radtad2.lobbyservice#1.0.0
```

## Quick Start
### Minimal Setup
1. `Lobby.SetProvider(BaseProvider)`
2. `Lobby.ConnectView(IView)`

### Preferred Setup
1. `Lobby.SetRules(LobbyRules)` will set the rules the lobby will use to decide key settings.
2. `Lobby.SetPreInitStrategy(IPreInitStrategy)` will retroactively update how calls are handled which were called before 
the first provider was set. The default is to drop calls but, you can choose to execute them 
by calling `Lobby.SetPreInitStrategy(new ExecutePreInitStrategy)`. This is retroactive and will apply to all past and future calls
until `SetProvider` is called.
3. `Lobby.SetProvider(BaseProvider)` will set the provider and initialize the lobby system.
4. `Lobby.ConnectView(IView)` will connect a view to the lobby system. 

## Using the Local Server
To use the local server, go to Tools -> Local Lobby Server to open the window. Make sure to pull the binary which will be 
installed to the currently set 'Download Folder'. Afterwards, just press 'Start Server' and it should open a terminal window with the 
server application running which will log activity as it happens. You can see the Local Server example in Samples for an out of the box working example of how to connect to it
from client code, its very easy.

You can see the binary src files at the following repo, just note that if you wish to compile it yourself you must copy and paste the files 
from com.radtad2.lobbyservice/Runtime/Local Server/Shared from this pacakge into the repo you downloaded. Alternatively, modify the server's
.csproj file to just include that path in the compilation.
src files: https://github.com/TadDiDio/LocalLobbyServer

## API
To interact with the lobby, use the `Lobby.cs` API. All calls should be made through `Lobby.XXX`. This provides
a safe way to always access functionality regardless of whether it exists or not.

## Providers
Providers are the modules which directly call backend APIs like Steamworks.NET. To support a new platform you must implement a new
provider and optionally its extensions.

### Creating a Provider

All providers inherit from `BaseProvider`. This base class gives requires several things:
### Methods 
1. Some methods may require you to get creative like `Kick()` on Steamworks.NET. I handle it by 
    sending a target RPC using the optional lobby procedures module which in turn is built on the steam chat features
2. Should you not want your provider to detect and leave stale lobbies for some reason you can 
override the base class method `ShouldFlushStateLobbies()`.

### Events
1. Each event must be invoked when the action corresponding to its name occurs.

### Properties
1. There are several properties that are required as well. These are the optional extension
    modules which you can optionally implement by inheriting from the interface declared by the property.
    If you do not intend to use that module, leaving it null is acceptable.

### Example 
To see a complete example for Steamworks.NET, see the samples. Note this example depends on the following Steamworks.NET package:
https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net#2024.8.0

The current list of provider interfaces is this:
- `BaseProvider` (required) - Includes basic lobby services.
- `IBrowserProvider` (optional) - Gives lobby browsing functionality.
- `IBrowserFilterProvider` (optional) - Allows browsers to filter results. 
- `IChatProvider` (optional) - Gives lobby chat services.
- `IFriendsProvider` (optional) - Allows discovering and inviting friends.
- `IProcedureProvider` (optional) - Allows running procedures on all lobby members.
- `IHeartbeatProvider` (optional) - Allows faster kicking of members who timeout than waiting for TTL on backends.

### Philosophy
Providers generally prefer synchronous calls to asynchronous calls. This leads to drastically simpler workflow where the user's local intent is
captured in state and eventually the remote state is reconciled to match. Create and Join are the only main calls which are asynchronous 
because they can reasonably fail without indicating network failure. Things like leaving, sending a chat, etc. may be synchronous on a particular
platform but asynchronous failure almost certainly indicates network failure in which case there is no actionable response. This is why
this lobby system interprets user intent with eventual reconciliation rather than waiting to verify results.

If an action is asynchronous in a platform by the provider requires a synchronous hook, you should spawn an async task to handle it
and return a successful value if required. Synchronous failure modes such as attempting restricted actions without being the owner
are checked before the provider method is even called, but there is no harm in checking again if it provides you peace of mind.

### Note on Eventual Correctness
Remote state will deterministically self-heal with time. There are mechanisms in place to automatically catch stale lobby ids so that 
if you happen to still be in a lobby when starting up a new provider, you are automatically removed from it. In addition, the async 
create and join requests are captured and kept alive past the scope of Unity playmode and can automatically handle repairing remote state
should they return to see the provider that launched them has been swapped out. Between these states, you are free to assume local state
is correct and hotswap lobby providers at runtime at any point.

## Views
Views are push based and will have methods called on them as events occur. You may have any number of views
attached at a given time, and unlike the providers, there is no specific subset that must be implemented. Instead
you choose the interfaces you want to update from based on this list:

- `ICoreView`
- `IBrowserView`
- `IChatView`
- `IFriendsView`

### Creating a View
1. Implement one or more interfaces from the list above.
2. Call `Lobby.ConnectView(view)` passing in your view. This is safe to call at any point.
### Philosophy
Views are push based so they never need to wonder where to get information from. You may read basic information from
the Lobby API like Lobby.IsOwner, but call results like CreateLobbyResult will never be given after calls.
The architecture follows a request only philosophy which makes implementing views incredible easy. After makeing a 
request like `Lobby.Create(args)` you should never do anything on your display until the `DisplayCreateRequested`
and `DisplayCreateResult` are called. 

Another benefit of this request only system is that you don't need to worry about whether the backend actually
implements functionality or not. For example, if there is no lobby browsing, calling `Lobby.Browse.Filter.AddNumberFilter()`
will not do anything so you will never receive a DisplayNumberFilterAdded call. In addition, a full list of lobby capabilities
is passed in to the `ResetView` method so you can enable or disable modules based on capabilities.

Views should treat `DisplayXXX` methods as idempotent meaning that just because the method is called it doesn't mean something
actually changed on the backend. The information passed in as parameters will always be up to date though and views are only 
updated after the internal state so querying `Lobby.XXX` is always safe from a `DisplayXXX` callback.

There is a sample view in the Local Server sample which intends to show the lobby functionality but 
not how to make a view. Rather than combine everything into a one mega class like I do in the sample, I would recommend
developing a translation layer for your own game. This means inheriting and registering views that redirect specific
slices of events from one or more view interfaces. This would allow you to make prefabs that take in arbitrary slices of events 
from across all view interfaces while more logically representing a single view in your specific game.