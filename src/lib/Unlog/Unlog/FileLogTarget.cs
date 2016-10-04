using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Unlog.Util;

namespace Unlog
{
	/// <summary>
	/// File Target. Logs data to a text file in a simple text format which can later be
	/// converted to human readable formats (e.g. a HTML page).
	/// </summary>
	public class FileLogTarget : ILogTarget, IDisposable
	{
		private readonly StreamWriter outF;

		public FileLogTarget (string path)
		{
			outF = new StreamWriter (File.Open (path, FileMode.CreateNew, FileAccess.Write, FileShare.Read), UTF8Encoding.UTF8);
		}

		public void Write (string s)
		{
			outF.Write (s);
		}

		public void SetForegroundColor (ConsoleColor c)
		{
			outF.Write ("\\c" + ((int) Console.ForegroundColor).ToString ("X") + ((int) Console.BackgroundColor).ToString ("X") + " ");
		}

		public void SetBackgroundColor (ConsoleColor c)
		{
			outF.Write ("\\c" + ((int) Console.ForegroundColor).ToString ("X") + ((int) Console.BackgroundColor).ToString ("X") + " ");
		}

		public void ResetColors ()
		{
			outF.Write ("\\~ ");
		}

		public void Flush ()
		{
			outF.Flush ();
		}

		public static void ConvertAllFilesToHTML (bool openFiles = false)
		{
			foreach (var f in Directory.EnumerateFiles (".", "*.ql")) {
				var dest = Path.GetFileNameWithoutExtension (f) + ".html";
				ConvertToHTML (f, dest);
				File.Delete (f);
				if (openFiles) {
					Process.Start (dest);
				}
			}
		}

		public static void ConvertToHTML (string source, string dest)
		{
			string[] lines;
			for (;;) {
				try {
					lines = File.ReadAllLines (source);
					break;
				}
				catch (IOException) {
					Thread.Sleep (100);
				}
			}

			// TODO: This is Sparta
			using (var file = File.Open (
					dest,
					FileMode.OpenOrCreate,
					FileAccess.Write,
					FileShare.Read))
			using (var sw = new StreamWriter (file, Encoding.UTF8)) {
				sw.WriteLine ("<!DOCTYPE HTML>");
				sw.WriteLine ("<HTML>");
				sw.WriteLine ();
				sw.WriteLine ("<HEAD>");
				sw.WriteLine ("<META charset='utf-8'>");
				sw.WriteLine ("</HEAD>");
				sw.WriteLine ();
				sw.WriteLine ("<BODY bgcolor=#000>");
				sw.WriteLine ();
				sw.WriteLine ("<pre>");
				sw.WriteLine ("<span style=\"color: #FFF; background-color: #000\">");
				sw.WriteLine (source);

				foreach (var line in lines) {
					var s = line;
					s = System.Security.SecurityElement.Escape (s);

					s = s.Replace ("\\~ ", "</span><span style=\"color: #FFF; background-color: #000\">");

					for (int f = 0; f < 16; f++) {
						var fc = ((ConsoleColor) f).ToRGB ();
						var fs = "#" + fc.R.ToString ("X2") + fc.G.ToString ("X2") + fc.B.ToString ("X2");
						for (int b = 0; b < 16; b++) {
							var bc = ((ConsoleColor) b).ToRGB ();
							var bs = "#" + bc.R.ToString ("X2") + bc.G.ToString ("X2") + bc.B.ToString ("X2");

							var i = f.ToString ("X") + b.ToString ("X");
							s = s.Replace (
								"\\c" + i + " ",
								"</span><span style=\"color: " + fs + "; background-color: " + bs + "\">"
								);
						}
					}

					sw.WriteLine (s);
				}

				sw.WriteLine ("</span>");
				sw.WriteLine ("</pre>");
				sw.WriteLine ();
				sw.WriteLine ("</BODY>");
				sw.WriteLine ();
				sw.WriteLine ("</HTML>");
			}
		}

		public void Dispose ()
		{
			outF.Dispose ();
		}
	}
}
