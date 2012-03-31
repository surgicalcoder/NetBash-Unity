NetBash is a drop in (think mvc mini profiler) command line for your web app.

Credits for creating NetBash goes to [Luke Lowrey](https://github.com/lukencode/NetBash) - I merely adapted it ever so slightly.

#### Set up
On application start call NetBash.Init() to initilize the routes. You can optionally set the Authorize action, this action is run to determine whether to show the console.

```csharp
protected void Application_Start()
{
	AreaRegistration.RegisterAllAreas();

	RegisterGlobalFilters(GlobalFilters.Filters);
	RegisterRoutes(RouteTable.Routes);

	NetBash.Init();
	NetBash.Settings.Authorize = (request) =>
		{
			return request.IsLocal;
		};
}
```

You also need to add the render includes code somewhere on your page (_Layout.cshtml is proabably easiest).

```
@NetBash.RenderIncludes()
```
	
#### Usage
NetBash commands are sent using this format - "[class name] [command name] [arg1] [arg2] etc". You can see which commands are currently loaded by typing "help". There are also a few keyboard shortcuts (which can be viewed with "shortcuts" the most useful being "`" to open and focust the console. If you wish to know all the commands available in a class, simply type the class name, ie. "core".

#### Creating a Command
NetBash will look for any implementation of the interface IWebCommand with a WebCommand attribute on first request. To create a command simply implement IWebCommand and add the WebCommand Attribtue.

```csharp
[WebCommandType("core", "Core Commands")]
public class CoreCommands : IWebCommand
{

	[WebCommand("test", "Does a test")]
	public CommandResult Test(string[] args)
	{
		return new CommandResult() {IsHtml = false, Result = "This is a test!"};
	}
	
}
```

This useless example simply returns "This is a test!" when you run "core test".

#### Commands

I need to document this - but basically its the same as NetBash, but everythings in the "core" class