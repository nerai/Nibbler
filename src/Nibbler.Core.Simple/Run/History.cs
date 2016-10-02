using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Nibbler.Core.Simple;
using TuringBox.TM;
using TuringBox.TM.Run;

namespace TuringBox.TM.Run
{
	public class History<TTape>
		where TTape : class, ITape
	{
		public readonly TmDefinition Def;

		private readonly Dictionary<State, HashSet<TmConfiguration<TTape>>> _CfgHist = new Dictionary<State, HashSet<TmConfiguration<TTape>>> ();

		public History (TmDefinition def)
		{
			if (def == null)
				throw new ArgumentNullException ();

			Def = def;
		}

		public HashSet<TmConfiguration<TTape>> GetSubset (State q)
		{
			if (q == null)
				throw new ArgumentNullException ();

			HashSet<TmConfiguration<TTape>> sub;
			if (!_CfgHist.TryGetValue (q, out sub)) {
				sub = new HashSet<TmConfiguration<TTape>> ();
				_CfgHist.Add (q, sub);
			}
			return sub;
		}

		public string CheckAndAdd (TmConfiguration<TTape> cfg)
		{
			if (cfg == null)
				throw new ArgumentNullException ();

			var sub = GetSubset (cfg.Q);
			var dupe = sub.FirstOrDefault (x => x.Equals (cfg));
			if (dupe != null) {
				return "Equivalent configuration to " + cfg + " already present: " + dupe;
			}

			// Ok, this state is new, add it.
			sub.Add (cfg.Clone ());

			return null;
		}

		public TmConfiguration<TTape> GetPrevious (ulong shifts)
		{
			var from = _CfgHist.Values.SelectMany (h => h.Where (c => c.T.Shifts == shifts));
			var sel = from.FirstOrDefault ();
			return sel;
		}
	}
}
