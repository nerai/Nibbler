using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nibbler.Core.Simple.Run;
using TuringBox.TM.Run;

namespace Nibbler.Core.Simple
{
	public class BasicTape : ISimpleTape
	{
		private byte[] _Tape;
		private int _Head;
		private int _Rightmost;
		private int _Leftmost;
		private readonly Dictionary<byte, string> _Gamma;

		public ulong Shifts { get; private set; }

		public BasicTape (IEnumerable<byte> tape, Dictionary<byte, string> gamma)
		{
			if (tape == null)
				throw new ArgumentNullException ();
			if (gamma == null)
				throw new ArgumentNullException ();

			_Tape = tape.Concat (new byte[100]).ToArray ();
			_Head = 0;
			_Gamma = gamma;
			Shifts = 0;
		}

		public ulong SymbolsOnTape ()
		{
			return (ulong) _Tape.Count (x => x != 0);
		}

		public byte ReadSingle ()
		{
			return _Tape[_Head];
		}

		public bool WriteSingle (byte b, int direction)
		{
			WriteSingle (b);
			Move (direction, 1UL);
			return true;
		}

		public void WriteSingle (byte b)
		{
			_Tape[_Head] = b;
		}

		public void Move (int direction, ulong shifts)
		{
			_Head += direction;
			int increase = 100 + _Tape.Length / 2;
			if (_Head < 0) {
				_Tape = new byte[increase].Concat (_Tape).ToArray ();
				_Head += increase;
				_Leftmost += increase;
				_Rightmost += increase;
			}
			if (_Head == _Tape.Length) {
				_Tape = _Tape.Concat (new byte[increase]).ToArray ();
			}

			_Leftmost = Math.Min (_Leftmost, _Head);
			_Rightmost = Math.Max (_Rightmost, _Head);

			Shifts += shifts;
		}

		public ulong TotalTapeLength
		{
			get {
				return (ulong) (_Rightmost - _Leftmost);
			}
		}

		public HeadPosition IsAtEnd
		{
			get {
				if (_Head == _Rightmost) {
					return HeadPosition.Right;
				}
				if (_Head == _Leftmost) {
					return HeadPosition.Left;
				}
				return HeadPosition.Neither;
			}
		}

		public string ToString (string stateName)
		{
			var sb = new StringBuilder ();

			if (TmPrintOptions.LeftAlignShifts) {
				sb.Append (Shifts.ToString ().PadLeft (10));
			}
			else {
				sb.Append (Shifts);
			}
			sb.Append ("~ ");

			int start = _Leftmost;

			while (start < _Head && start < _Tape.Length && _Tape[start] == 0) {
				start++;
			}

			int end = _Rightmost;

			while (end >= 0 && _Tape[end] == 0) {
				end--;
			}

			for (int i = start; i <= _Rightmost; i++) {
				byte b = _Tape[i];

				if (i == _Head) {
					sb.Append (stateName);
					sb.Append (" ");
				}

				sb.Append (_Gamma[b]);
				sb.Append (" ");
			}

			return sb.ToString ();
		}

		public override bool Equals (object obj)
		{
			throw new InvalidOperationException ("Use explicit Equals instead");
		}

		public override int GetHashCode ()
		{
			throw new NotSupportedException ();
		}

		public bool CompleteContentEquals (ITape t)
		{
			return CompleteContentEquals (t as BasicTape);
		}

		public bool CompleteContentEquals (BasicTape t)
		{
			throw new NotImplementedException ();
		}
	}
}
