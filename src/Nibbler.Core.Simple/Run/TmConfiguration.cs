using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nibbler.Core.Simple;

namespace TuringBox.TM.Run
{
	public class TmConfiguration<TTape>
		where TTape : class, ITape
	{
		public readonly State Q;
		public readonly TTape T;

		public TmConfiguration (State q, TTape t)
		{
			if (q == null)
				throw new ArgumentNullException ();
			if (t == null)
				throw new ArgumentNullException ();

			Q = q;
			T = t;
		}

		public override string ToString ()
		{
			return T.ToString (Q.ToString ());
		}

		public TmConfiguration<TTape> Clone ()
		{
			var c = T as ICloneable;
			if (c != null) {
				return new TmConfiguration<TTape> (Q, c.Clone () as TTape);
			}
			else {
				throw new InvalidOperationException ("This tape does not support cloning.");
			}
		}

		public override int GetHashCode ()
		{
			unchecked {
				return (int.MaxValue - Q.Id) ^ (int) T.TotalTapeLength;
			}
		}

		public override bool Equals (object obj)
		{
			if (obj == null || obj.GetType () != GetType ()) {
				return false;
			}
			return Equals (obj as TmConfiguration<TTape>);
		}

		public bool Equals (TmConfiguration<TTape> c)
		{
			if (c == null) {
				return false;
			}
			if (Q != c.Q) {
				return false;
			}
			return T.CompleteContentEquals (c.T);
		}
	}
}
