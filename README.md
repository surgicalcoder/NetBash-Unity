NetBash is a drop in (think mvc mini profiler) command line for your web app.

Credits for creating NetBash goes to [Luke Lowrey](https://github.com/lukencode/NetBash) - I merely adapted it and changed how it works and redid the reflection, and bits.


----------


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
NetBash will look for any classes that are decorated with the WebCommandType attribute. To create a command simply add the WebCommandType and WebCommand Attribtues.

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

#### Changelog

##### v 1.1 - 2011-04-15

- Reworked the reflection side of things to make things alot smoother and more reliable.
- Removed the requirement to inherit from IWebCommand - completely pointless thanks to the refactoring
- Added the ability to have named parameters, instead of only accepting a string[], ie:

```csharp
        [WebCommand("NameTest","Returns Your Name")]
        public string Test(string Name)
        {
            return string.Format("Hello, {0}", Name);
        }
```

##### Plan for vNext
- Ability to have basic value types for parametes ie. DateTime, int, etc.
- Add validation for parameters - ie. make some required, some optional
- Copy the idea of "partial" classes ie. you can have multiple classes that will join up to form a command type.