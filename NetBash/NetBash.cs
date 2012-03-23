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
        //private static Type _attributeType = typeof(WebCommandAttribute);
    	private static Type _attributeTypeType = typeof (WebCommandTypeAttribute);
    	private static Type _attributeCommandType = typeof (WebCommandAttribute);
        private static Type _interfaceType = typeof(IWebCommand);

        public static void Init()
        {
            NetBashHandler.RegisterRoutes();
        }

        internal void LoadCommands()
        {
            try
            {
				_interfaceType = typeof(WebCommandTypeAttribute);
                var assemblies = AssemblyLocator.GetAssemblies();

                var results = from a in assemblies
                              from t in a.GetTypes()
							  where t.GetCustomAttributes(_attributeTypeType, false).Any()
                              select t;

                _commandTypes = results.ToList();

				IEnumerable<MethodInfo> enumerable = results.SelectMany
					(x => x.GetMethods().Where(y => y.GetCustomAttributes(_attributeCommandType, false).Any() ).
						Where(y=>
							y.GetParameters().Count() == 1 && 
							y.GetParameters().Count(z=>z.ParameterType == typeof(string[])) == 1 &&
							(y.ReturnType == typeof(CommandResult) || y.ReturnType == typeof(string))
							) 
						);

            	_commandMethods = enumerable.ToList();
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

            if (command == "help")
                return renderHelp();

            var commandType = (from c in _commandTypes
							   let attr = (WebCommandTypeAttribute)c.GetCustomAttributes(_attributeTypeType, false).FirstOrDefault()
                              where attr != null
                              && attr.Name.ToLower() == command
                              select c).FirstOrDefault();

            if(commandType == null)
                throw new ArgumentException(string.Format("Command '{0}' not found", command.ToUpper()));

			if (split.Count() == 1)
			{
				return renderHelp(commandType);
			}

            var webCommand = (IWebCommand)Activator.CreateInstance(commandType);
        	MethodInfo firstOrDefault = _commandMethods.FirstOrDefault(x =>
        	                                                           	{
        	                                                           		var webCommandCommandAttribute = (WebCommandAttribute) x.GetCustomAttributes(_attributeCommandType, false).FirstOrDefault();
        	                                                           		return webCommandCommandAttribute != null && (x.DeclaringType == commandType && webCommandCommandAttribute.Name == split[1]);
        	                                                           	});

        	object invokeMember = firstOrDefault.Invoke(webCommand, new object[] {split.Skip(2).ToArray()});

			CommandResult result = new CommandResult();
			if (invokeMember is string)
			{
				
				result = new CommandResult() { IsHtml = false, Result = invokeMember.ToString() };
			}
			else if (invokeMember is CommandResult)
			{
				result = invokeMember as CommandResult;
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
					continue;

				sb.AppendLine(string.Format("{0} - {1}", attr.Name.ToUpper().PadRight(15, ' '), attr.Description));
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
