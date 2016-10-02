using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nibbler.Core.Simple.Run;
using Nibbler.Utils.BasicDataStructures;

namespace Nibbler.Core.Macro.Tapes
{
	public class Cell : LinkedListNode2<Cell>
	{
		protected readonly PackedExponentTape Parent;

		public byte Data;
		public ulong Exponent;

		public Cell (PackedExponentTape parent, byte b, ulong exponent)
		{
			Parent = parent;
			Data = b;
			Exponent = exponent;
		}

		public virtual Cell Clone ()
		{
			return new Cell (Parent, Data, Exponent);
		}

		public override string ToString ()
		{
			var sb = new StringBuilder ();

			var howOften = TmPrintOptions.ExplodeExponents
				? Exponent
				: 1;

			for (var i = 1ul; i <= howOften; i++) {
				if (!TmPrintOptions.DecodeWords) {
					sb.Append (Data);
				}
				else {
					foreach (var bb in Parent.Macro.Decode (Data)) {
						sb.Append (Parent.Macro._Gamma[bb]);
					}
				}
			}

			if (!TmPrintOptions.ExplodeExponents && Exponent != 1) {
				sb.Append ("^");
				sb.Append (Exponent.ToString ());
			}

			return sb.ToString ();
		}

		public string ToStringDecoded (bool facingRight, string headName, int headPos)
		{
			var s = "";
			var printSeparate = Exponent > 1;

			Exponent--; // (hack)

			if (printSeparate && !facingRight) {
				s += ToString ();
				if (!TmPrintOptions.OutputForLatex) {
					s += " ";
				}
			}

			if (TmPrintOptions.OutputForLatex) {
				s += "@";
				if (!TmPrintOptions.DecodeWords) {
					s += Data;
				}
				else {
					foreach (var bb in Parent.Macro.Decode (Data)) {
						s += Parent.Macro._Gamma[bb];
					}
				}
				s += "@";
			}
			else {
				var unpacked = Parent.Macro.Decode (Data).Select (x => Parent.Macro._Gamma[x]).ToArray ();
				s += TapeUtil.WriteHeadCell (facingRight, unpacked, headPos, headName);
			}

			if (printSeparate && facingRight) {
				if (!TmPrintOptions.OutputForLatex) {
					s += " ";
				}
				s += ToString ();
			}

			Exponent++;

			return s;
		}
	}
}
