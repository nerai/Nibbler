using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlog
{
	/// <summary>
	/// Primitive console log target.
	/// </summary>
	public class ConsoleLogTarget : ILogTarget
	{
		public void Write (string s)
		{
			Console.Write (s);
		}

		public void SetForegroundColor (ConsoleColor c)
		{
			Console.ForegroundColor = c;
		}

		public void SetBackgroundColor (ConsoleColor c)
		{
			Console.BackgroundColor = c;
		}

		public void ResetColors ()
		{
			Console.ResetColor ();
		}

		public void Flush ()
		{
			// nothing to do here
		}
	}
}
