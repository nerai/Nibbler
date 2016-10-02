using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Unlog.Util
{
	public class Debugging
	{
		public static void LaunchDebugger ()
		{
			if (Debugger.IsAttached || Debugger.Launch ()) {
				Debugger.Break ();
			}
			else {
				throw new Exception ("ERROR - Unable to launch debugger. Will raise exception instead.");
			}
		}
	}
}
