using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nibbler.Core.Simple.Tapes
{
	public class LimitedBasicTape : ISimpleTape
	{
		public readonly byte[] Tape;

		public int Position { get; private set; }

		public bool? ExitsOnLeftSide;

		private readonly Dictionary<byte, string> _Gamma;

		public ulong Shifts { get; private set; }

		public LimitedBasicTape (int length, Dictionary<byte, string> gamma)
			: this (new byte[length], gamma)
		{
		}

		public LimitedBasicTape (LimitedBasicTape tape, Dictionary<byte, string> gamma)
			: this (tape.Tape, gamma)
		{
			if (tape == null)
				throw new ArgumentNullException ();
			if (gamma == null)
				throw new ArgumentNullException ();

			Position = tape.Position;
			ExitsOnLeftSide = tape.ExitsOnLeftSide;
		}

		public LimitedBasicTape (byte[] cell, Dictionary<byte, string> gamma)
		{
			if (cell == null)
				throw new ArgumentNullException ();
			if (gamma == null)
				throw new ArgumentNullException ();

			Tape = cell.ToArray ();
			Position = 0;
			_Gamma = gamma;
		}

		public LimitedBasicTape (byte[] left, byte[] right, Dictionary<byte, string> gamma)
		{
			if (left == null)
				throw new ArgumentNullException ();
			if (right == null)
				throw new ArgumentNullException ();
			if (gamma == null)
				throw new ArgumentNullException ();

			Tape = left.Concat (right).ToArray ();
			Position = left.Length;
			_Gamma = gamma;
		}

		public ulong SymbolsOnTape ()
		{
			return (ulong) Tape.Count (x => x != 0);
		}

		public byte ReadSingle ()
		{
			if (ExitsOnLeftSide.HasValue) {
				throw new InvalidOperationException ();
			}

			return Tape[Position];
		}

		public bool WriteSingle (byte b, int direction)
		{
			WriteSingle (b);
			Move (direction, 1);
			return true;
		}

		public void WriteSingle (byte b)
		{
			if (ExitsOnLeftSide.HasValue) {
				throw new InvalidOperationException ();
			}

			Tape[Position] = b;
		}

		public void Move (int direction, ulong shifts)
		{
			if (ExitsOnLeftSide.HasValue) {
				throw new InvalidOperationException ();
			}

			Position += direction;
			if (Position < 0) {
				ExitsOnLeftSide = true;
			}
			else if (Position >= Tape.Length) {
				ExitsOnLeftSide = false;
			}

			Shifts += shifts;
		}

		public ulong TotalTapeLength
		{
			get {
				return (ulong) Tape.Length;
			}
		}

		public HeadPosition IsAtEnd
		{
			get {
				if (Position == 0) {
					return HeadPosition.Left;
				}
				if (Position == Tape.Length - 1) {
					return HeadPosition.Right;
				}
				return HeadPosition.Neither;
			}
		}

		public string ToString (string stateName)
		{
			// todo: use existing functions from Configuration class

			var sb = new StringBuilder ();

			if (Position == -1) {
				sb.Append (stateName);
				sb.Append (" ? ");
			}
			for (int i = 0; i < Tape.Length; i++) {
				byte b = Tape[i];

				if (i == Position) {
					sb.Append (stateName);
					sb.Append (" ");
				}

				sb.Append (_Gamma[b]);
				sb.Append (" ");
			}
			if (Position == Tape.Length) {
				sb.Append (stateName);
				sb.Append (" ?");
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
			return CompleteContentEquals (t as LimitedBasicTape);
		}

		public bool CompleteContentEquals (LimitedBasicTape t)
		{
			if (t.TotalTapeLength != TotalTapeLength)
				return false;
			if (Position != t.Position)
				return false;
			if (!Tape.SequenceEqual (t.Tape))
				return false;
			return true;
		}
	}
}
