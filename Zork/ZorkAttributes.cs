using System;
using System.Collections.Generic;
using System.Text;

namespace Zork
{
	[AttributeUsage(AttributeTargets.Class)]
	public class CommandClassAttribute : Attribute
	{
		public string CommandName { get; }
		public IEnumerable<string> Verbs { get; }
		public CommandClassAttribute(string commandNmae, string verb) :
			this(commandName, new string[] { verb })
		{
		}

		public CommandAttribute(string commandName, string[] verbs)
		{
			CommandName = commandName;
			Verbs = verbs;
		}
	}
}
