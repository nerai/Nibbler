using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TuringBox.Tapes
{
	public class MacroPacker
	{
		public readonly uint MacroSize;

		public readonly Dictionary<byte, string> _Gamma;
		private readonly Dictionary<byte, uint> _NonzerosInWord;

		public MacroPacker (uint macroSize, Dictionary<byte, string> gamma)
		{
			int nRequiredSpace = 0;
			for (int i = 0; i < macroSize; i++) {
				nRequiredSpace *= gamma.Count;
				nRequiredSpace += gamma.Count - 1;
			}
			if (nRequiredSpace >= 256) {
				throw new ArgumentOutOfRangeException ("Sorry, macro and gamma cannot be encoded into a single byte. Try using a tape with int/long instead, maybe? Required space: 0.." + nRequiredSpace);
			}

			MacroSize = macroSize;
			_Gamma = gamma;
			_NonzerosInWord = new Dictionary<byte, uint> ();
		}

		public MacroPacker (MacroPacker clone)
		{
			MacroSize = clone.MacroSize;
			_Gamma = new Dictionary<byte, string> (clone._Gamma);
			_NonzerosInWord = new Dictionary<byte, uint> (clone._NonzerosInWord);
		}

		public byte Encode (byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException ();
			if (data.Length != MacroSize)
				throw new ArgumentException ();

			return EncodeAny (data);
		}

		public byte EncodeAny (byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException ();

			int res = 0;
			foreach (byte b in data) {
				res *= _Gamma.Count;
				res += b;
			}

			Debug.Assert (res < 256);

			return (byte) res;
		}

		// todo: lookup table might be better
		public byte[] Decode (byte data)
		{
			var res = new byte[MacroSize];
			for (int i = (int) MacroSize - 1; i >= 0; --i) {
				res[i] = (byte) (data % _Gamma.Count);
				data = (byte) (data / _Gamma.Count);
			}
			return res;
		}

		public uint CountNZinWord (byte b)
		{
			uint nz;
			if (!_NonzerosInWord.TryGetValue (b, out nz)) {
				var dec = Decode (b);
				nz = (uint) dec.Count (x => x != 0);
				_NonzerosInWord.Add (b, nz);
			}
			return nz;
		}
	}
}
