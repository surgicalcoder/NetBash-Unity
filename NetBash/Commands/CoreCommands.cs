using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;

namespace NetBash.Commands
{
	[WebCommandType("core", "Core Commands")]
	public class CoreCommands
	{

		[WebCommand("reload", "Reloads command assemblies")]
		public string Reload(string[] args)
		{
			NetBash.Current.LoadCommands();
			return "Commands loaded, type 'help' to see them";
		}

		[WebCommand("NameTest","Returns Your Name")]
		public string Test(string Name)
		{
			return string.Format("Hello, {0}", Name);
		}

		[WebCommand("server", "Displays server info")]
		public string Server(string[] args)
		{
			var sb = new StringBuilder();
			var context = HttpContext.Current;

			sb.AppendLine("Name - " + context.Server.MachineName);
			sb.AppendLine("IP - " + context.Request.ServerVariables["LOCAL_ADDR"]);
			sb.AppendLine("Domain - " + context.Request.ServerVariables["Server_Name"]);
			sb.AppendLine("Port - " + context.Request.ServerVariables["Server_Port"]);

			return sb.ToString();
		}

		[WebCommand("test", "Does a test")]
		public CommandResult Test(string[] args)
		{
			return new CommandResult() {IsHtml = false, Result = "This is a test!"};
		}

		[WebCommand("shortcuts", "Lists the shortcuts")]
		public string Shortcuts(string[] args)
		{
			var sb = new StringBuilder();

			sb.AppendFormat("{0} - {1}", "`".PadRight(7), "Opens and focuses NetBash");
			sb.AppendLine();
			sb.AppendFormat("{0} - {1}", "esc".PadRight(7), "Closes NetBash");
			sb.AppendLine();
			sb.AppendFormat("{0} - {1}", "↑".PadRight(7), "Puts last command in text box (only when focuses)");
			sb.AppendLine();
			sb.AppendFormat("{0} - {1}", "shift+`".PadRight(7), "Toggle Netbash");

			return sb.ToString();
		}

		[WebCommand("time", "Gets the server time")]
		public string Time(string[] args)
		{
			return DateTime.Now.ToString();
		}

		[WebCommand("uptime","Gets the server uptime")]
		public string Uptime(string[] args)
		{
			return UpTime.ToReadableString();
		}

		public TimeSpan UpTime
		{
			get
			{
				using (var uptime = new PerformanceCounter("System", "System Up Time"))
				{
					uptime.NextValue(); //Call this an extra time before reading its value
					return TimeSpan.FromSeconds(uptime.NextValue());
				}
			}
		}

	}

	internal static class TimeSpanExtensions
	{
		internal static string ToReadableString(this TimeSpan span)
		{
			string formatted = string.Format("{0}{1}{2}{3}",
				span.Days > 0 ? string.Format("{0:0} days, ", span.Days) : string.Empty,
				span.Hours > 0 ? string.Format("{0:0} hours, ", span.Hours) : string.Empty,
				span.Minutes > 0 ? string.Format("{0:0} minutes, ", span.Minutes) : string.Empty,
				span.Seconds > 0 ? string.Format("{0:0} seconds", span.Seconds) : string.Empty);

			if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

			return formatted;
		}
	}

	}
