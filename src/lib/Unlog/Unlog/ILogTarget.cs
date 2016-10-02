using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlog
{
	/// <summary>
	/// Describes a log target. It should be able to write strings and provide basic formatting.
	/// </summary>
	public interface ILogTarget
	{
		void Write (string s);

		void SetForegroundColor (ConsoleColor c);

		void SetBackgroundColor (ConsoleColor c);

		void ResetColors ();

		void Flush ();
	}
}
