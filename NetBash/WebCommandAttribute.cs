using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetBash
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class WebCommandTypeAttribute : Attribute
	{
		public string Name { get; set; }
		public string Description { get; set; }

		public WebCommandTypeAttribute(string name, string description)
		{
			Name = name;
			Description = description;
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class WebCommandAttribute : Attribute
	{
	    public WebCommandAttribute()
	    {
	    }

	    public string Name { get; set; }
		public string Description { get; set; }

		public WebCommandAttribute(string name, string description)
		{
			Name = name;
			Description = description;
		}
	}

}
