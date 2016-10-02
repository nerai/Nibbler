using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TuringBox.TM
{
	public class State
	{
		public readonly int Id;
		public readonly string Name;
		public readonly HashSet<SimpleTransition> Sources = new HashSet<SimpleTransition> ();
		public readonly SimpleTransition[] Delta;

		public State (int id, string name, SimpleTransition[] ts)
		{
			if (id < 0 || id >= 255) throw new ArgumentOutOfRangeException ();

			Id = id;
			Name = name;
			Delta = ts;
		}

		public override string ToString ()
		{
			return Name;
		}

		public IEnumerable<State> TransitiveClosure (
			Func<SimpleTransition, bool> selector,
			Func<SimpleTransition, bool> cancel)
		{
			var done = new HashSet<State> ();
			var work = new HashSet<State> ();
			work.Add (this);

			while (work.Any ()) {
				var q = work.First ();
				work.Remove (q);
				done.Add (q);

				if (q.Delta == null) {
					continue;
				}

				var ts = q.Delta
					.Where (t => selector == null || selector (t))
					.ToArray ();
				if (cancel != null && ts.Any (cancel)) {
					return null;
				}
				foreach (var qn in ts.Select (t => t.Next)) {
					if (!done.Contains (qn)) {
						work.Add (qn);
					}
				}
			}

			return done;
		}

		public IEnumerable<State> ReverseTransitiveClosure ()
		{
			var done = new HashSet<State> ();
			var work = new HashSet<State> ();
			work.Add (this);

			while (work.Any ()) {
				var q = work.First ();
				work.Remove (q);
				done.Add (q);

				foreach (var qn in q.Sources.Select (t => t.Source)) {
					if (!done.Contains (qn)) {
						work.Add (qn);
					}
				}
			}

			return done;
		}

		public long TransitionEquivalenceHash ()
		{
			if (Delta == null) {
				return long.MaxValue;
			}
			if (Delta.Length != 2) {
				throw new Exception ();
			}
			if (Delta[0] == null || Delta[1] == null) {
				return long.MaxValue;
			}

			var h0 = Delta[0].EquivalenceEncoding ();
			var h1 = Delta[1].EquivalenceEncoding ();
			if (h0 == int.MaxValue || h1 == int.MaxValue) {
				return long.MaxValue;
			}

			var hash = (h1 << 16) | h0;
			return hash;
		}
	}
}
