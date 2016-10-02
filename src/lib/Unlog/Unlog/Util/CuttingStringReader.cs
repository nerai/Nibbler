using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlog.Util
{
	/// <summary>
	/// Provides methods to cut off either an expected prefix or a single char from a string.
	/// </summary>
	public class CuttingStringReader
	{
		private readonly char[] _C;
		private int _I = 0;

		public CuttingStringReader (string initial)
		{
			_C = initial.ToCharArray ();
		}

		public int RemainingLength
		{
			get
			{
				return _C.Length - _I;
			}
		}

		/// <summary>
		/// If this starts with the specified argument, using ordinal comparison, cuts off the argument and returns true.
		/// </summary>
		public bool Eat (string expect)
		{
			if (_I + expect.Length > _C.Length) {
				return false;
			}
			for (int i = 0; i < expect.Length; i++) {
				if (_C[_I + i] != expect[i]) {
					return false;
				}
			}
			_I += expect.Length;
			return true;
		}

		/// <summary>
		/// Cut off single char.
		/// </summary>
		public char Read ()
		{
			if (_I >= _C.Length) {
				throw new InvalidOperationException ();
			}

			var c = _C[_I];
			_I++;
			return c;
		}
	}
}
