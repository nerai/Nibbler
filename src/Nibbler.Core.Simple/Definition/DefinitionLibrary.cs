using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Nibbler.Core.Simple.Definition;
using Unlog;

namespace TuringBox.TM
{
	public class DefinitionLibrary
	{
		public static string CreateBeaverFromNote (string s)
		{
			if (string.IsNullOrWhiteSpace (s)) {
				throw new ArgumentException ("s");
			}

			var regex = ""
				+ @"^"
				+ @"("
				+ @"(?<tuple>[A-Z][0-9][LR]|[0-9][LR][A-Z]|-+)"
				+ @"[\s,]*"
				+ @")+"
				+ @"(?<after>.*)?"
				+ @"$";
			var m = Regex.Match (s, regex, RegexOptions.Singleline);
			if (!m.Success) {
				throw new ArgumentException ();
			}
			var tuples = m
				.Groups["tuple"]
				.Captures.Cast<Capture> ()
				.Select (c => c.Value)
				.ToArray ();
			var after = m.Groups["after"].Value;

			var n = tuples.Length;
			var def = "";
			def += string.Join (", ", Enumerable.Range (0, n / 2).Select (x => (char) ('A' + x))) + ";";
			def += " H; ; 0,1; \n";

			int i = -1;
			foreach (var t in tuples) {
				i++;

				if (t.StartsWith ("-")) {
					// this transition is explicitly undefined
					continue;
				}

				def += (char) ('A' + (i / 2)) + "," + (i % 2) + " -> ";
				def += string.Join (",", (IEnumerable<char>) t);
				def += ";\n";
			}

			if (after.Length > 0) {
				def += " " + after + ";";
			}
			return def;
		}

		private static IEnumerable<JsonTmDefinition> AllDefinitionsJ ()
		{
			return AllDefinitionNames ().Select (f => LoadJ (f));
		}

		private static string FindLibraryDirectory ()
		{
			var dir = ".";
			for (;;) {
				dir = Path.GetFullPath (dir);
				var dirs = Directory.EnumerateDirectories (dir).Where (d => d.Contains ("Library"));
				foreach (var d in dirs) {
					if (Directory.EnumerateFiles (d, "*.tmj").Any ()) {
						return d;
					}
				}
				if (Path.GetPathRoot (dir).Equals (dir)) {
					return null;
				}
				dir = dir + "\\..";
			}
		}

		private static IEnumerable<string> AllDefinitionNames ()
		{
			return Directory
				.EnumerateFiles (FindLibraryDirectory (), "*.tmj")
				.Select (f => Path.GetFileNameWithoutExtension (f));
		}

		public static string GetDefinitionByName (string name)
		{
			var sel = ExampleMenu.Util.LooseSelect (
				AllDefinitionNames (),
				name,
				StringComparison.InvariantCultureIgnoreCase);
			return sel;
		}

		public static JsonTmDefinition LoadJ (string index)
		{
			var path = FindLibraryDirectory () + "\\" + index + ".tmj";
			var s = File.ReadAllText (path, UTF8Encoding.UTF8);
			var j = JsonTmDefinition.Restore (s);
			return j;
		}

		public static TmDefinition Load (string index)
		{
			var j = LoadJ (index);
			var t = new TmDefinition (j);
			return t;
		}

		public static void PrintContent ()
		{
			var defs = AllDefinitionsJ ().ToArray ();
			var max = defs.Max (d => d.Info_Name.Length);

			foreach (var json in defs) {
				var def = new TmDefinition (json);
				Log.WriteLine (json.Info_Name.PadRight (max) + " " + def.ShortDefinitionString);
			}
		}

		/// <summary>
		/// Re-Export JSON to JSON (for format updates)
		/// </summary>
		public static void ReexportJson ()
		{
			foreach (var json in AllDefinitionsJ ()) {
				var def = new TmDefinition (json.OriginalDefinition);
				File.WriteAllText (json.Info_Name + ".tmj", def.ToJson (json.Info_Name).Persist (), UTF8Encoding.UTF8);
			}
		}
	}
}
