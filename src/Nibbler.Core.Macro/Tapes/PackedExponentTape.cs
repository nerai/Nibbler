// Check tape regularly for consistency
//#define CheckTapeConsistency

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Nibbler.Core.Simple;
using Nibbler.Core.Simple.Run;
using Nibbler.Core.Simple.Tapes;
using Nibbler.Utils.BasicDataStructures;
using Unlog;

namespace Nibbler.Core.Macro.Tapes
{
	public class PackedExponentTape : ITape, ICloneable
	{
		public ulong Shifts { get; protected set; }

		public readonly MacroPacker Macro;
		private int _PositionInMacro;

		public bool FacingRight
		{
			get;
			private set;
		}

		/*
		 * Left end of L and R are close to the head, right ends of L and R are far from it
		 * (this means L is 'reversed' from a tape perspective!)
		 */
		protected readonly LinkedList2<Cell> _L = new LinkedList2<Cell> ();
		protected readonly LinkedList2<Cell> _R = new LinkedList2<Cell> ();

		private IEnumerable<Cell> UnorderedCells { get { return _L.Concat (_R); } }

		public IEnumerable<Cell> OrderedCells
		{
			get {
				// todo: use reverse iterator to avoid performance loss
				return _L.Reverse ().Concat (_R);
			}
		}

		public Cell Left { get { return _L.First; } }

		public Cell Right { get { return _R.First; } }

		public bool AllowRightGrowth = true;
		public bool AllowLeftGrowth = true;

		public PackedExponentTape (
			uint macroSize,
			Dictionary<byte, string> gamma,
			IEnumerable<byte> unpackedInit)
		{
			if (macroSize <= 0)
				throw new ArgumentOutOfRangeException ();
			if (gamma == null)
				throw new ArgumentNullException ();
			Macro = new MacroPacker (macroSize, gamma);

			if (unpackedInit != null) {
				foreach (var b in unpackedInit.Reverse ()) {
					WritePacked (b, false, 0, false);
				}
				_R.RemoveLast ();
			}

			FacingRight = true;
			Shifts = 0;

			CheckConsistency ();
		}

		public PackedExponentTape (
			PackedExponentTape src,
			bool cutOffLeft = false,
			bool cutOffRight = false)
		{
			if (src == null)
				throw new ArgumentNullException ();

			Macro = new MacroPacker (src.Macro);
			_PositionInMacro = src._PositionInMacro;
			FacingRight = src.FacingRight;
			Shifts = src.Shifts;

			if (!cutOffLeft) {
				_L = new LinkedList2<Cell> (src._L.Select (c => c.Clone ()));
			}
			if (!cutOffRight) {
				_R = new LinkedList2<Cell> (src._R.Select (c => c.Clone ()));
			}

			CheckConsistency ();
		}

		public bool IsMisaligned
		{
			get {
				return (false
					|| (FacingRight && _PositionInMacro != 0)
					|| (!FacingRight && _PositionInMacro != Macro.MacroSize - 1)
					);
			}
		}

		public ulong SymbolsOnTape ()
		{
			ulong sum = 0;

			foreach (var c in UnorderedCells) {
				var nz = Macro.CountNZinWord (c.Data);
				sum += (ulong) (nz * c.Exponent);
			}

			return sum;
		}

		public ulong GetEqualSymbolRepetitionCount ()
		{
			if (FacingRight) {
				return _R.First.Exponent;
			}
			else {
				return _L.First.Exponent;
			}
		}

		protected virtual Cell CreateCell (byte b, ulong exp)
		{
			return new Cell (this, b, exp);
		}

		public virtual byte ReadPacked ()
		{
			var c = GetOrCreateCurrentCell (false);
			if (c.Exponent <= 0) {
				throw new InvalidOperationException ("Tried to read from a cell with an exponent that has a zero direct component.");
			}
			return c.Data;
		}

		protected Cell GetOrCreateCurrentCell (bool avoidChanges)
		{
			var t = FacingRight ? _R : _L;

			if (t.First == null) {
				if (avoidChanges) {
					return null;
				}
				t.AddFirst (CreateCell (0, 1));
			}

			return t.First;
		}

		public byte ReadSingle ()
		{
			var d = Macro.Decode (ReadPacked ());
			return d[_PositionInMacro];
		}

		[Conditional ("CheckTapeConsistency")]
		protected void CheckConsistency ()
		{
			if (FacingRight) {
				Debug.Assert (_R.First == null || _R.First.Exponent > 0);
			}
			else {
				Debug.Assert (_L.First == null || _L.First.Exponent > 0);
			}

			foreach (var c in UnorderedCells) {
				Debug.Assert (c.Next == null || c.Data != c.Next.Data);
				Debug.Assert (c.Exponent >= 0);
			}
		}

		public void WriteSingleInCell (byte b, int dir, ulong shifts)
		{
			if (_PositionInMacro + dir >= 0 && _PositionInMacro + dir < Macro.MacroSize) {
				// We stay in this cell
				var desiredFacing = FacingRight;
				WritePacked (b, !desiredFacing, shifts, false);
				WritePacked (ReadPacked (), desiredFacing, 0, false);
			}
			else {
				// We move to a different cell, i.e. this is a regular operation
				WritePacked (b, dir > 0, shifts, false);
			}

			// Adjust position
			_PositionInMacro += dir;

			if (_PositionInMacro < 0 || _PositionInMacro >= Macro.MacroSize) {
				throw new InvalidOperationException ();
			}
		}

		/// <summary>
		/// Writes to the current cell, overwriting the whole block.
		///
		/// The write will be performed as many times as there are repetitions in the current cell.
		/// </summary>
		/// <param name="v">
		/// Data to be written.
		/// </param>
		/// <param name="goRight">
		/// true if the operation direction is to the right.
		/// </param>
		/// <param name="shiftsPerWrite">
		/// Shifts required for each write.
		/// </param>
		/// <param name="wholeBlock">
		/// Should the operation be applied to a single word, or to the whole block?
		/// </param>
		/// <returns>
		/// False iff an exponent's direct value would become zero.
		/// </returns>
		public bool WritePacked (byte v, bool goRight, ulong shiftsPerWrite, bool wholeBlock)
		{
			CheckConsistency ();

			if (IsMisaligned) {
				throw new InvalidOperationException ("Tape is misaligned!");
			}

			var me = FacingRight ? _R.First : _L.First;

			// Adjust shifts
			// TODO: das nach möglichkeit erst am ENDE machen (falls operation ungültig -> tape unverändert!)
			if (wholeBlock) {
				Shifts += shiftsPerWrite * me.Exponent;
			}
			else {
				Shifts += shiftsPerWrite;
			}

			var behindTape = goRight ? _L : _R;
			var forwardTape = goRight ? _R : _L;

			/*
			 * Are we moving into the word or away from it?
			 * If we move into it, the head will end behind it with the same facing.
			 * Otherwise, facing will be negated and the word in front of the current word will be entered.
			 */
			if (FacingRight != goRight) {
				/*
				 * The operation moves away from the current block.
				 * This means we first need to write a single word to the current block, then change facing
				 */

				/*
				 * Operation on the whole block is only possible if we actually move into it and not away from it.
				 */
				if (wholeBlock) {
					throw new InvalidOperationException ("Cannot write whole block if movement direction points in opposite direction.");
				}

				/*
				 * Is the content of the current cell changed?
				 * If so, we need to split it off from the block, so the rest of the block remains unchanged.
				 */
				if (me == null || me.Data != v) {
					/*
					 * Remove 1 cell from the current block.
					 * If the block does not have enough direct exponent, fail.
					 * If the remaining block is empty, throw it away.
					 */
					if (me != null) {
						if (me.Exponent == 0) {
							return false;
						}
						me.Exponent--;
						if (me.Exponent == 0) {
							behindTape.Remove (me);
						}
					}

					/*
					 * Append a new byte to the next block.
					 * If that block has the same base value, it suffices to simply increment its exponent.
					 * Otherwise, a new cell must be created and inserted at the front.
					 */
					var prev = behindTape.First;
					if (prev != null && prev.Data == v) {
						prev.Exponent++;
					}
					else {
						var c = CreateCell (v, 1);
						behindTape.AddFirst (c);
					}
				}

				/*
				 * Facing changed. Adjust the stored position inside the word.
				 * This is in fact the only way for the facing and position within a cell to reverse.
				 */
				if (goRight) {
					_PositionInMacro = 0;
				}
				else {
					_PositionInMacro = (int) (Macro.MacroSize - 1);
				}
				FacingRight = goRight;
			}
			else {
				/*
				 * The operations works in the direction we're facing.
				 */

				/*
				 * Since the head steps over the word we are writing, we can simply write a single word behind the head for the same result.
				 * If the block behind the head has the same base value, it suffices to increment its exponent.
				 * Else, a new cell must be created and inserted at its front.
				 */
				var prev = behindTape.First;
				Cell c;
				if (prev != null && prev.Data == v) {
					c = prev;
				}
				else {
					c = CreateCell (v, 0);
					behindTape.AddFirst (c);
				}

				/*
				 * If this operation works on a single word, increment the exponent of the block behind the head.
				 * Else, we are overwriting the whole current block, so increase its exponent by the exponent of the current block.
				 */
				if (wholeBlock) {
					c.Exponent += me.Exponent;
				}
				else {
					c.Exponent++;
				}

				/*
				 * If we operated on the whole block, the current block can be removed in its entirety.
				 * Else, we must remove a single word from it by decrementing its exponent.
				 * If the block does not have enough direct exponent, fail.
				 * If the remaining block is empty, throw it away.
				 */
				if (wholeBlock) {
					forwardTape.RemoveFirst ();
				}
				else {
					if (me != null) {
						if (me.Exponent == 0) {
							return false;
						}
						me.Exponent--;
						if (me.Exponent == 0) {
							forwardTape.Remove (me);
						}
					}
				}
			}

			/*
			 * Housekeeping - if we introduced a blank at either end (directly beside an infinte stream of blanks), remove that blank.
			 */
			if (_L.Last != null && _L.Last.Data == 0) {
				_L.RemoveLast ();
			}
			if (_R.Last != null && _R.Last.Data == 0) {
				_R.RemoveLast ();
			}

			CheckConsistency ();

			return true;
		}

		public ulong TotalTapeLength
		{
			get {
				return (ulong) UnorderedCells.Sum (c => (long) c.Exponent);
			}
		}

		public long PackedTapeLength
		{
			get {
				return _L.Count + _R.Count;
			}
		}

		public HeadPosition IsAtEnd
		{
			get {
				if (true
					&& (_L.Count == 0 || (_L.Count == 1 && _L.First.Data == 0))
					&& (FacingRight == false)
					) {
					return HeadPosition.Left;
				}
				if (true
					&& (_R.Count == 0 || (_R.Count == 1 && _R.First.Data == 0))
					&& (FacingRight == true)
					) {
					return HeadPosition.Right;
				}
				return HeadPosition.Neither;
			}
		}

		public virtual string ToString (string stateName)
		{
			var sb = new StringBuilder ();

			Action<IEnumerable<Cell>> addList = list => {
				foreach (var node in list) {
					if (!TmPrintOptions.OutputForLatex) {
						sb.Append (" ");
					}
					sb.Append (node.ToString ());
				}
			};

			var fd = Shifts.ToString ();
			if (TmPrintOptions.LeftAlignShifts) {
				fd = fd.PadLeft (10);
			}
			sb.Append (fd);
			sb.Append ("~");

			if (TmPrintOptions.OutputForLatex) {
				sb.Append (" ");
				sb.Append (stateName);
				sb.Append (" ");
			}

			if (IsMisaligned) {
				if (FacingRight) {
					addList (_L.Reverse ());

					if (!TmPrintOptions.OutputForLatex) {
						sb.Append (" ");
					}

					var c = _R.First ?? CreateCell (0, 1);
					sb.Append (c.ToStringDecoded (true, stateName, _PositionInMacro));

					addList (_R.Skip (1));
				}
				else {
					addList (_L.Skip (1).Reverse ());

					if (!TmPrintOptions.OutputForLatex) {
						sb.Append (" ");
					}

					var c = _L.First ?? CreateCell (0, 1);
					sb.Append (c.ToStringDecoded (false, stateName, _PositionInMacro));

					addList (_R);
				}
			}
			else {
				addList (_L.Reverse ());

				if (!TmPrintOptions.OutputForLatex) {
					sb.Append (" ");
				}
				if (FacingRight) {
					sb.Append (stateName + ">");
				}
				else {
					sb.Append ("<" + stateName);
				}

				addList (_R);
			}

			return sb.ToString ();
		}

		public override string ToString ()
		{
			return ToString ("Q");
		}

		public PackedExponentTape Clone ()
		{
			return new PackedExponentTape (this);
		}

		object ICloneable.Clone ()
		{
			return Clone ();
		}

		public IEnumerable<byte[]> TapeContent (int blockSize)
		{
			var buf = new MemoryStream ();

			foreach (var c in OrderedCells) {
				var b = Macro.Decode (c.Data);
				var e = c.Exponent;

				for (var i = 0ul; i < e; i++) {
					buf.Write (b, 0, b.Length);

					while (buf.Length >= blockSize) {
						var newBuf = new MemoryStream ();
						buf.Position = blockSize;
						buf.CopyTo (newBuf);
						buf.SetLength (blockSize);
						yield return buf.ToArray ();
						buf = newBuf;
					}
				}
			}

			if (buf.Length > 0) {
				yield return buf.ToArray ();
			}
		}

		public bool CompleteContentEquals (ITape t)
		{
			return Equals (t as PackedExponentTape, ExponentComparison.TotalEquality, false, false);
		}

		public enum ExponentComparison
		{
			Ignore,
			EqualityOfDirectComponent,
			TotalEquality,
		}

		public bool Equals (
			PackedExponentTape t,
			ExponentComparison ec,
			bool ignoreTheirSuperfluousLeft,
			bool ignoreTheirSuperfluousRight)
		{
			if (t == null) {
				return false;
			}
			return true
				&& LeftSideEqualWith (t, ec, ignoreTheirSuperfluousLeft)
				&& RightSideEqualWith (t, ec, ignoreTheirSuperfluousRight);
		}

		public bool LeftSideEqualWith (PackedExponentTape t, ExponentComparison ec, bool ignoreTheirSuperfluous)
		{
			if (t == null)
				throw new ArgumentNullException ();

			if (_PositionInMacro != t._PositionInMacro) {
				return false;
			}
			if (FacingRight != t.FacingRight) {
				return false;
			}
			return ContentEqualWith (ec, _L.First, t._L.First, ignoreTheirSuperfluous);
		}

		public bool RightSideEqualWith (PackedExponentTape t, ExponentComparison ec, bool ignoreTheirSuperfluous)
		{
			if (t == null)
				throw new ArgumentNullException ();

			if (_PositionInMacro != t._PositionInMacro) {
				return false;
			}
			if (FacingRight != t.FacingRight) {
				return false;
			}
			return ContentEqualWith (ec, _R.First, t._R.First, ignoreTheirSuperfluous);
		}

		/// <summary>
		/// Compare cell contents.
		///
		/// Begin is inclusive, end is exclusive (use null to compare to the end).
		/// </summary>
		private bool ContentEqualWith (
			ExponentComparison ec,
			Cell tape1,
			Cell tape2,
			bool ignoreSuperfluousTape2)
		{
			while (tape1 != null) {
				if (tape2 == null) {
					return false;
				}

				if (tape1.Data != tape2.Data) {
					return false;
				}
				if (ec == ExponentComparison.Ignore) {
					// nothing
				}
				else if (ec == ExponentComparison.EqualityOfDirectComponent) {
					if (tape1.Exponent != tape2.Exponent) {
						return false;
					}
				}
				else if (ec == ExponentComparison.TotalEquality) {
					if (!tape1.Exponent.Equals (tape2.Exponent)) {
						return false;
					}
				}

				tape1 = tape1.Next;
				tape2 = tape2.Next;
			}
			if (!ignoreSuperfluousTape2 && tape2 != null) {
				return false;
			}
			return true;
		}

		public override bool Equals (object obj)
		{
			throw new InvalidOperationException ("Use explicit Equals instead");
		}

		public int? SuggestMacroSizeEx ()
		{
			const bool print = false;

			if (print) {
				Log.Write ("Searching best macro size for " + this);
			}
			var results = new Dictionary<int, int> ();

			for (int macro = 1; macro <= 8; macro++) { // todo check gamma, make sure its still in 1 byte
				byte previous = 0; // 0 intentional, refers to empty tape.
				int nCells = 0;

				foreach (var block in TapeContent (macro)) {
					var enc = Macro.EncodeAny (block);
					if (enc != previous) {
						nCells++;
						previous = enc;
					}
				}

				results.Add (macro, nCells + macro); // Larger macro sizes should be used more rarely
			}

			var best = results
				.OrderBy (x => x.Value)
				.ThenBy (x => x.Key)
				.First ();
			if (print) {
				Log.WriteLine (best.Key + " (" + best.Value + " cells)");
			}

			return best.Key;
		}
	}
}
