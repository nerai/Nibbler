using System;
using System.Linq;

namespace Unlog.Util
{
    public static class StringCutting
	{
		/// <summary>
		/// Cut off a expected prefix from a string, which is passed by value.
		/// The strings are compared using ordinally.
		/// If the prefix was not found, the input will not be modified.
		/// </summary>
		/// <param name="s">
		/// Source string. This value will be modified, if the prefix matches.
		/// </param>
		/// <param name="expect">
		/// The expected prefix which should be cut off from the string.
		/// </param>
		/// <param name="ignoreCase">
		/// Compare while ignoring case.
		/// </param>
		/// <returns>
		/// True iff the prefix was found.
		/// </returns>
		public static bool Cut (ref string s, string expect, bool ignoreCase = false)
		{
			var comparison = ignoreCase
				? StringComparison.OrdinalIgnoreCase
				: StringComparison.Ordinal;
			if (!s.StartsWith (expect, comparison)) {
				return false;
			}

			s = s.Substring (expect.Length);
			return true;
		}

		/// <summary>
		/// Cut off a number of characters from a string, which is passed by value, and returns them.
		/// </summary>
		/// <param name="s">
		/// Source string. This value will be modified, if it contains enough characters to be cut off.
		/// </param>
		/// <param name="len">
		/// The number of characters which should be cut off from the string.
		/// </param>
		/// <returns>
		/// The characters which were cut off, or null if the string was too short.
		/// </returns>
		public static string Cut (ref string s, int len)
		{
			if (s.Length < len) {
				return null;
			}

			var ret = s.Substring (0, len);
			s = s.Substring (len);
			return ret;
		}

		/// <summary>
		/// Cut off an integer prefix from a string, which is passed by value, and return it.
		/// </summary>
		/// <param name="s">
		/// Source string. This value will be modified, if it starts with an integer.
		/// </param>
		/// <returns>
		/// The integer which was cut off.
		/// </returns>
		public static int CutInt (ref string s)
		{
			var si = new string (s.TakeWhile (c => char.IsDigit (c)).ToArray ());
			if (si.Length == 0) {
				throw new ArgumentException ("The specified string does not start with an integer.");
			}

			s = s.Substring (si.Length);
			return int.Parse (si);
		}
	}
}
