using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web;
using NetBash.UI;
using System.Reflection;
using System.Web.Compilation;
using NetBash.Helpers;

namespace NetBash
{
	public partial class NetBash
	{
		private List<Type> _commandTypes;
		private List<MethodInfo> _commandMethods;
		private static readonly Type _attributeTypeType = typeof (WebCommandTypeAttribute);
		private static readonly Type _attributeCommandType = typeof (WebCommandAttribute);

		public static void Init()
		{
			NetBashHandler.RegisterRoutes();
		}

		internal void LoadCommands()
		{
			try
			{
				var assemblies = AssemblyLocator.GetAssemblies();

				_commandTypes = assemblies.SelectMany(x => x.GetTypes().MarkedWith<WebCommandTypeAttribute>()).ToList();

				_commandMethods = _commandTypes.SelectMany(x => x.GetMethods().MarkedWith<WebCommandAttribute>())
					.Where(y=>
							(y.ReturnType == typeof(CommandResult) || y.ReturnType == typeof(string))
						)
					.ToList();
			}
			catch (ReflectionTypeLoadException ex)
			{
				var text = string.Join(", ", ex.LoaderExceptions.Select(e => e.Message));
				throw new ApplicationException(text);
			}

			//if we still cant find any throw exception
			if (_commandTypes == null || !_commandTypes.Any())
				throw new ApplicationException("No commands found");
		}

		internal CommandResult Process(string commandText)
		{
			if (_commandTypes == null || !_commandTypes.Any())
				LoadCommands();

			if (string.IsNullOrWhiteSpace(commandText))
				throw new ArgumentNullException("commandText", "Command text cannot be empty");

			var split = commandText.SplitCommandLine().ToList();
			var command = (split.FirstOrDefault() ?? commandText).ToLower();
		    var subcommand = (split.Count < 2 ? "" : split.Skip(1).Take(1).FirstOrDefault()).ToLower();
		    var args = (split.Count < 3 ? new string[0] : split.Skip(2)).ToArray();


			if (command == "help")
				return renderHelp();

			var commandType = (from c in _commandTypes
						 let attr = c.GetAttribute<WebCommandTypeAttribute>()
						 where attr != null && attr.Name.ToLower() == command
						 select c
						).FirstOrDefault();

			if(commandType == null)
			{
			    throw new ArgumentException(string.Format("Command '{0}' not found", command.ToUpper()));
			}

			if (split.Count() == 1)
			{
				return renderHelp(commandType);
			}

			var webCommand = Activator.CreateInstance(commandType);

		    MethodInfo method =
		        _commandMethods.FirstOrDefault(
		            x =>
		            (x.GetAttribute<WebCommandAttribute>() != null && (x.GetAttribute<WebCommandAttribute>().Name != null && x.GetAttribute<WebCommandAttribute>().Name.ToLower() == subcommand) || (x.GetAttribute<WebCommandAttribute>().Name == null && x.Name.ToLower() == subcommand)) &&
		            x.DeclaringType == commandType);

            object returnValue;
            if (method.GetParameters().Count() == 1 && method.GetParameters().Count(x => x.ParameterType == typeof(string[])) == 1)
            {
                returnValue = method.Invoke(webCommand, new object[] { args.ToArray() });    
            }
            else
            {
                if (method.GetParameters().Count() != args.Count())
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (var parameterInfo in method.GetParameters())
                    {
                        builder.AppendLine(parameterInfo.Name);
                    }
                    returnValue = new CommandResult()
                                      {
                                          IsError = true,
                                          Result = string.Format(
                                                  "Invalid Number of Parameters Supplied.\r\nParameters that are required are {1}:\r\n{0}",
                                                  builder, method.GetParameters().Count())
                                      };
                }
                else
                {
                    object[] objArray = new object[args.Count()];
                    args.CopyTo(objArray, 0);

                    returnValue = method.Invoke(webCommand, objArray);
                }
            }

            
		    CommandResult result = new CommandResult();

			if (returnValue is string)
			{
				result = new CommandResult { IsHtml = false, Result = returnValue.ToString() };
			}
			else if (returnValue is CommandResult)
			{
				result = returnValue as CommandResult;
			}
			
			return result;
		}

		private CommandResult renderHelp(Type commandType)
		{
			var sb = new StringBuilder();

			foreach (var t in _commandMethods.Where(x=>x.DeclaringType == commandType))
			{
				var attr = (WebCommandAttribute)t.GetCustomAttributes(_attributeCommandType, false).FirstOrDefault();
                
                if (attr == null)
                {
                    continue;
                }
			    
                string name = attr.Name ?? t.Name;
			    string description = attr.Description ?? "No Description Found";

                sb.AppendLine(string.Format("{0} - {1}", name.ToUpper().PadRight(15, ' '), description));
			}

			return new CommandResult { Result = sb.ToString(), IsHtml = false };

		}

		private CommandResult renderHelp()
		{
			var sb = new StringBuilder();

			sb.AppendLine("CLEAR           - Clears current console window");

			foreach (var t in _commandTypes)
			{
				var attr = (WebCommandTypeAttribute)t.GetCustomAttributes(_attributeTypeType, false).FirstOrDefault();

				if(attr == null)
					continue;

				sb.AppendLine(string.Format("{0} - {1}", attr.Name.ToUpper().PadRight(15, ' '), attr.Description));
			}

			return new CommandResult { Result = sb.ToString(), IsHtml = false };
		}

		public static IHtmlString RenderIncludes()
		{
			return NetBashHandler.RenderIncludes();
		}

		#region singleton
		static readonly NetBash instance= new NetBash();

		// Explicit static constructor to tell C# compiler
		// not to mark type as beforefieldinit
		static NetBash()
		{
		}

		NetBash()
		{
		}

		public static NetBash Current
		{
			get
			{
				return instance;
			}
		}
		#endregion
	}
}
