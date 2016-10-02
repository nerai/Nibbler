using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlog
{
	/// <summary>
	/// Null log target. Discards everything.
	/// </summary>
	public class NullLogTarget : ILogTarget
	{
		public void Write (string s)
		{
			// intentionally empty
		}

		public void SetForegroundColor (ConsoleColor c)
		{
			// intentionally empty
		}

		public void SetBackgroundColor (ConsoleColor c)
		{
			// intentionally empty
		}

		public void ResetColors ()
		{
			// intentionally empty
		}

		public void Flush ()
		{
			// intentionally empty
		}
	}
}
