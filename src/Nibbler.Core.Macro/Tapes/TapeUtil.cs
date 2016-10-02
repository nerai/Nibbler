using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nibbler.Core.Macro.Tapes
{
	public static class TapeUtil
	{
		public static string WriteHeadCell (
			bool facingRight,
			string[] unpackedWord,
			int headPosition,
			string stateName)
		{
			if (headPosition < -1 || headPosition > unpackedWord.Length) {
				throw new ArgumentOutOfRangeException ();
			}

			var sb = new StringBuilder ();

			if (headPosition == -1) {
				sb.Append ("<");
				sb.Append (stateName);
				sb.Append (" ");
			}
			else if (facingRight && headPosition == 0) {
				sb.Append (stateName);
				sb.Append (">");
			}

			for (int i = 0; i < unpackedWord.Length; i++) {
				if (i > 0 && i < unpackedWord.Length - 1 && headPosition == i) {
					sb.Append (stateName);
				}
				sb.Append (unpackedWord[i]);
			}

			if (!facingRight && headPosition == unpackedWord.Length - 1) {
				sb.Append ("<");
				sb.Append (stateName);
			}
			else if (headPosition == unpackedWord.Length) {
				sb.Append (" ");
				sb.Append (stateName);
				sb.Append (">");
			}

			return sb.ToString ();
		}
	}
}
