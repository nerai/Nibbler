using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nibbler.Core.Simple;
using TuringBox.Tapes;
using TuringBox.TM;
using TuringBox.TM.Run;
using Unlog;

namespace TuringRunner
{
	class Program
	{
		public static readonly string Version;

		static Program ()
		{
			/*
			 * Set up logging
			 */
			Unlog.FileLogTarget.ConvertAllFilesToHTML ();
			Log.AllowAsynchronousWriting = false;
			Log.AddDefaultFileTarget ();

			/*
			 * Find build stamp (crude hack)
			 */
			var exe = typeof (Program).Assembly.ManifestModule.FullyQualifiedName;
			var stamp = File.GetLastWriteTimeUtc (exe);
			var id = stamp.Subtract (new DateTime (2014, 10, 26)).TotalDays;
			Version = "Nibbler Build A" + id.ToString ("0");
			Version += " (" + stamp.ToString ("yyyy.MM.dd HH.mm") + ")";
			Version += " (c) Sebastian Heuchler";
		}

		static void Main (string[] args)
		{
			if (!Environment.Is64BitProcess) {
				Log.WriteLine ("WARNING: Running as 32 bit process. May crash on low memory.");
			}
			Log.WriteLine (Version);

			var cr = new ControlledRun (false);
			cr.Run ();
		}
	}
}
