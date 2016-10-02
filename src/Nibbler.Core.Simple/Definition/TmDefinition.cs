using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Nibbler.Core.Simple.Definition;

namespace Nibbler.Core.Simple.Definition
{
	public class TmDefinition
	{
		public string ShortDefinitionString { get; private set; }

		public string FullDefinitionString { get; private set; }

		public readonly Dictionary<byte, string> Sigma;
		public readonly Dictionary<byte, string> Gamma;

		public readonly Dictionary<string, State> Q = new Dictionary<string, State> ();
		public readonly State Q0;
		public readonly State Qa;
		public readonly State Qr;

		public readonly int? SuggestedMacroSize;

		public TmDefinition (JsonTmDefinition def)
		{
			if (def == null)
				throw new ArgumentNullException ();

			Gamma = MapSymbols (def.Gamma, 0);
			Sigma = new Dictionary<byte, string> ();
			foreach (var sig in def.Sigma) {
				Sigma.Add (Gamma.Where (p => p.Value == sig).First ().Key, sig);
			}

			Func<string, State> createState = name => {
				var halting = false
					|| def.AcceptingState == name
					|| def.RefusingState == name;
				var ts = halting ? null : new SimpleTransition[Gamma.Count];
				var q = new State (Q.Count, name, ts);
				Q.Add (name, q);
				return q;
			};

			Q0 = createState (def.InitialState);

			foreach (var qs in def.NonfinalStates) {
				if (qs == def.InitialState) {
					continue;
				}
				createState (qs);
			}

			if (def.AcceptingState != null) {
				createState (def.AcceptingState);
			}
			if (def.RefusingState != null) {
				createState (def.RefusingState);
			}

			foreach (var t in def.Delta) {
				var qin = Q[t.From];
				byte gin = Gamma.First (p => p.Value == t.Read).Key;
				var qout = Q[t.To];
				byte gout = Gamma.First (p => p.Value == t.Write).Key;
				short dir;
				switch (t.Dir) {
					case "R":
						dir = 1;
						break;

					case "L":
						dir = -1;
						break;

					case "S":
						dir = 0;
						break;

					default:
						throw new Exception ();
				}

				var st = new SimpleTransition (qin, gin, qout, gout, dir);
				qin.Delta[gin] = st;
				qout.Sources.Add (st);
			}

			SuggestedMacroSize = def.SuggestedMacroSize;

			UpdateShortDefinitionString ();
		}

		public JsonTmDefinition ToJson (string name)
		{
			var j = new JsonTmDefinition ();

			j.OriginalDefinition = FullDefinitionString;

			j.NonfinalStates = Q
				.Where (p => Qa == null || p.Key != Qa.Name)
				.Where (p => Qr == null || p.Key != Qr.Name)
				.Select (p => p.Key)
				.ToArray ();
			if (Qa != null) {
				j.AcceptingState = Qa.Name;
			}
			if (Qr != null) {
				j.RefusingState = Qr.Name;
			}
			j.InitialState = Q0.Name;

			j.Sigma = Sigma.OrderBy (p => p.Key).Select (p => p.Value).ToArray ();
			j.Gamma = Gamma.OrderBy (p => p.Key).Select (p => p.Value).ToArray ();

			var delta = new List<JsonTmDefinition.Transition> ();
			var qs = Q.Values
				.Where (q => q.Delta != null)
				.SelectMany (q => q.Delta);
			foreach (var ts in qs) {
				if (ts == null) {
					continue;
				}
				var t = new JsonTmDefinition.Transition ();
				t.From = ts.Source.Name;
				t.Read = Gamma[ts.Read];
				t.To = ts.Next.Name;
				t.Write = Gamma[ts.Write];
				t.Dir = ts.Direction == 0 ? "S" : ts.Direction == 1 ? "R" : "L";
				delta.Add (t);
			}
			j.Delta = delta.ToArray ();

			j.SuggestedMacroSize = SuggestedMacroSize;

			j.Info_Name = name;
			j.Info_Comment = GetComments (FullDefinitionString).ToArray ();
			j.Info_Url = null;
			j.Info_ExpectedResult = null;

			return j;
		}

		private static IEnumerable<string> GetComments (string s)
		{
			var m = Regex.Match (s, @"^.*?(?:(?:/\*(?<c>.*?)\*/).*?)*$", RegexOptions.Singleline);
			var g = m.Groups["c"];

			foreach (var c in g.Captures) {
				var splits = c.ToString ().Split ('\n');
				foreach (var split in splits) {
					yield return split;
				}
			}
		}

		private Dictionary<byte, string> MapSymbols (IEnumerable<string> symbols, int counter)
		{
			var dict = new Dictionary<byte, string> ();

			foreach (var p in symbols.Select (x => x.Trim ())) {
				if (p == "") {
					continue;
				}

				if (dict.ContainsValue (p)) {
					throw new Exception ("Duplicate symbol or state.");
				}

				if (counter >= 255) {
					throw new Exception ("Too many symbols or states.");
				}

				var n = (byte) (counter);
				dict.Add (n, p);
				counter++;
			}

			return dict;
		}

		/// <summary>
		/// </summary>
		/// <param name="s">
		/// Format:
		/// q0,q1,...; Q
		/// s0,s1,...; Sigma
		/// g0,g1,...; Gamma without Sigma
		/// q,i -> q',o,d; Single Delta transition, repeatable
		/// K=n; Suggested macro width, optional
		/// /* anywhere: comments in old C style allowed */
		/// </param>
		public TmDefinition (string s)
		{
			if (s == null)
				throw new ArgumentNullException ();

			if (s.Count (c => c == ';') <= 1) { // todo: bug when ';' in comments
				s = DefinitionLibrary.CreateBeaverFromNote (s);
			}
			FullDefinitionString = s;

			s = StripComments (s);

			var split = s.Split (';').Select (x => x.Trim ()).ToList ();

			var sStates = split[0];
			split.RemoveAt (0);
			var sHalting = split[0];
			split.RemoveAt (0);
			var sSigma = split[0];
			split.RemoveAt (0);
			var sGamma = split[0];
			split.RemoveAt (0);

			/*
			 * Sigma/Gamma
			 */
			var sigmasplit = sSigma.Split (',');
			var gammasplit = sGamma.Split (',');
			Sigma = MapSymbols (sigmasplit, 1);
			Gamma = MapSymbols (gammasplit.Skip (1), 1 + Sigma.Count);
			Gamma.Add (0, gammasplit[0]);
			foreach (var sig in Sigma) {
				Gamma.Add (sig.Key, sig.Value);
			}

			/*
			 * Q
			 * (can only be constructed once Gamma is known)
			 */
			var statessplit = sStates.Split (',');
			foreach (var p in statessplit.Select (x => x.Trim ())) {
				if (p == "") {
					continue;
				}

				if (Q.Count >= 255) {
					throw new Exception ();
				}

				var q = new State ((byte) Q.Count, p, new SimpleTransition[Gamma.Count]);
				Q.Add (p, q);

				if (Q0 == null) {
					Q0 = q;
				}
			}
			foreach (var p in sHalting.Split (',').Select (x => x.Trim ())) {
				if (p == "") {
					continue;
				}

				if (Q.Count >= 255) {
					throw new Exception ();
				}

				var q = new State ((byte) Q.Count, p, null);
				Q.Add (p, q);

				if (Q0 == null) {
					Q0 = q;
				}
			}

			var haltingStates = Q.Values.Where (q => q.Delta == null).ToArray ();
			if (haltingStates.Length == 0) {
				var nonhaltingStates = Q.OrderBy (x => x.Key).Select (x => x.Value).ToArray ();
				Qa = nonhaltingStates[nonhaltingStates.Length - 2];
				Qr = nonhaltingStates[nonhaltingStates.Length - 1];
			}
			else if (haltingStates.Length == 1) {
				Qa = haltingStates[0];
				Qr = null;
			}
			else {
				Qa = haltingStates[0];
				Qr = haltingStates[1];
			}

			// Here be dragons

			/*
			 * Delta
			 */
			while (split.Any ()) {
				var d = split[0];
				d = d.TrimStart (' ', '\t');
				split.RemoveAt (0);

				if (d == "") {
					continue;
				}

				if (d.StartsWith ("K=")) {
					d = d.Substring (2);
					SuggestedMacroSize = Unlog.Util.StringCutting.CutInt (ref d);
					split.Insert (0, d);
					continue;
				}

				var parts = d.Split (new[] { "->" }, 2, StringSplitOptions.None);
				var input = parts[0].Split (',').Select (x => x.Trim ()).ToArray ();
				var output = parts[1].Split (',').Select (x => x.Trim ()).ToArray ();

				var sqin = input[0];
				var stin = input[1];
				var sqout = output[0];
				var stout = output[1];
				var sd = output[2].ToUpperInvariant ();

				// Check different notation
				if (sd != "L" && sd != "R") {
					if (stout == "L" || stout == "R") {
						var tmp = sd;
						sd = stout;
						stout = sqout;
						sqout = tmp;
					}
				}

				var qin = Q[sqin];
				byte gin = Gamma.First (p => p.Value == stin).Key;
				var qout = Q[sqout];
				byte gout = Gamma.First (p => p.Value == stout).Key;
				short dir;
				switch (sd) {
					case "R":
						dir = 1;
						break;

					case "L":
						dir = -1;
						break;

					case "S":
						dir = 0;
						break;

					default:
						throw new Exception ();
				}

				var t = new SimpleTransition (qin, gin, qout, gout, dir);
				qin.Delta[gin] = t;
				qout.Sources.Add (t);
			}

			UpdateShortDefinitionString ();
		}

		public static string StripComments (string s)
		{
			s = Regex.Replace (s, @"/\*.*\*/", "", RegexOptions.Singleline);
			return s;
		}

		public IEnumerable<byte> CreateTape (string p)
		{
			if (p == null)
				throw new ArgumentNullException ();

			return p.Select (c => Sigma.First (pair => pair.Value.Equals (c.ToString ())).Key);
		}

		public bool HasUndefinedTransitions
		{
			get {
				return Q.Values
					.Where (q => q.Delta != null)
					.SelectMany (q => q.Delta)
					.Any (t => t == null);
			}
		}

		public bool HasRedundantStates ()
		{
			var transitionHashes = new HashSet<long> ();
			foreach (var q in Q.Values) {
				var hash = q.TransitionEquivalenceHash ();
				if (hash != long.MaxValue) {
					if (!transitionHashes.Add (hash)) {
						return true;
					}
				}
			}
			return false;
		}

		public override string ToString ()
		{
			return ShortDefinitionString ?? FullDefinitionString;
		}

		public void UpdateTransition (SimpleTransition t)
		{
			if (t == null)
				throw new ArgumentNullException ();

			t.Source.Delta[t.Read] = t;
			FullDefinitionString += " " + t.ToString (this) + ";";
			UpdateShortDefinitionString ();
		}

		public string GetLongDefinitionString (int? changeSuggestedMacroSize)
		{
			var s = "";
			s += string.Join (", ", Q.Where (q => q.Value.Delta != null).Select (q => q.Key).OrderBy (x => x)) + "; ";
			s += string.Join (", ", Q.Where (q => q.Value.Delta == null).Select (q => q.Key).OrderBy (x => x)) + "; ";
			s += string.Join (", ", Sigma.Values.OrderBy (x => x)) + "; ";
			s += string.Join (", ", Gamma.Values.Except (Sigma.Values).OrderBy (x => x)) + ";";
			s += Environment.NewLine;

			foreach (var t in Q.Values.Where (q => q.Delta != null).SelectMany (q => q.Delta)) {
				if (t != null) {
					s += t.ToString (this) + ";" + Environment.NewLine;
				}
			}

			var macro = changeSuggestedMacroSize ?? SuggestedMacroSize;
			if (macro != null) {
				s += "K=" + macro + ";" + Environment.NewLine;
			}
			return s;
		}

		public string GetShortDefinitionString (int? changeSuggestedMacroSize, SimpleTransition additionalTransition)
		{
			var s = "";

			foreach (var q in Q.OrderBy (x => x.Key).Select (x => x.Value)) {
				if (s.Length > 0) {
					s += ",";
				}

				var ts = new List<SimpleTransition> ();
				if (q.Delta != null) {
					ts.AddRange (q.Delta);
				}
				if (additionalTransition != null && q == additionalTransition.Source) {
					if (ts.Any ()) {
						if (ts[additionalTransition.Read] != null) {
							throw new InvalidOperationException ("Tried to replace an existing transition in delta table.");
						}
						ts[additionalTransition.Read] = additionalTransition;
					}
					else {
						throw new Exception ("ist das so ok?");
						ts.Add (additionalTransition);
					}
				}

				foreach (var t in ts) {
					if (s.Length > 0) {
						s += " ";
					}
					if (t == null) {
						s += "---";
					}
					else {
						s += t.Write;
						s += t.Direction == 1 ? "R" : "L";
						s += t.Next.Name;
					}
				}
			}

			var k = changeSuggestedMacroSize ?? SuggestedMacroSize;
			if (k != null) {
				s += " K=" + k.Value;
			}

			return s;
		}

		private void UpdateShortDefinitionString ()
		{
			ShortDefinitionString = GetShortDefinitionString (null, null);
		}
	}
}
