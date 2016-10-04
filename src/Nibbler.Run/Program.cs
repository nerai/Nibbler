using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Nibbler.Core.Simple;
using Unlog;

namespace Nibbler.Run
{
	class Program
	{
		static Program ()
		{
			/*
			 * Set up logging
			 */
			new Thread (() => Unlog.FileLogTarget.ConvertAllFilesToHTML ()) {
				Name = "Convert log files to HTML",
				IsBackground = true,
			}.Start ();
			Log.AllowAsynchronousWriting = false;
			Log.AddDefaultFileTarget ();
		}

		static void Main (string[] args)
		{
			if (!Environment.Is64BitProcess) {
				Log.WriteLine ("Running as 32 bit process.");
			}

			var cr = new ControlledRun (false);
			cr.Run ();
		}
	}
}
