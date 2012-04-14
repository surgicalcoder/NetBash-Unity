using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetBash
{
	public class CommandResult
    {
        public string Result { get; set; }
        public bool IsHtml { get; set; }
        public bool IsError { get; set; }
    }
}
