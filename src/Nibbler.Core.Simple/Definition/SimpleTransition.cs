using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TuringBox.TM
{
	public class SimpleTransition
	{
		public readonly State Source;
		public readonly byte Read;
		public readonly State Next;
		public readonly byte Write;
		public readonly short Direction;

		public SimpleTransition (State source, byte read, State q, byte o, short d)
		{
			if (source == null) throw new ArgumentNullException ();
			if (q == null) throw new ArgumentNullException ();

			Source = source;
			Read = read;
			Next = q;
			Write = o;
			Direction = d;
		}

		public override string ToString ()
		{
			return ToString (null);
		}

		public string ToString (TmDefinition def)
		{
			return ""
				+ Source.Name
				+ ","
				+ (def == null ? "" + Read : def.Gamma[Read])
				+ " -> "
				+ Next.Name
				+ ","
				+ (def == null ? "" + Write : def.Gamma[Write])
				+ ","
				+ (Direction > 0 ? "R" : (Direction == 0 ? "S" : "L"));
		}

		public int EquivalenceEncoding ()
		{
			int i = (Read << 24) | (Write << 16);
			if (Source == Next) {
				i |= 0xFF00;
			}
			else {
				i |= Next.Id << 8;
			}
			i |= (Direction + 1);
			return i;
		}
	}
}
