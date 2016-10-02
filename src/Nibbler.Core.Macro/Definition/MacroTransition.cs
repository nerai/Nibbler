using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Nibbler.Core.Macro.Tapes;
using Nibbler.Core.Simple;
using Nibbler.Core.Simple.Definition;
using Nibbler.Core.Simple.Run;
using Nibbler.Core.Simple.Tapes;
using Nibbler.Utils;

namespace Nibbler.Core.Macro.Definition
{
	public class MacroTransition
	{
		public readonly State Source;
		public readonly bool StartFacingRight;
		public readonly byte Read;
		public readonly State Next;
		public readonly byte Write;
		public readonly short Direction;
		public readonly bool? EndFacingRight;
		public readonly ulong Shifts;

		private readonly byte[] _ReadUnpacked;
		private readonly byte[] _WriteUnpacked;

		public long UsageCount;

		public MacroTransition (
			State source,
			byte read,
			bool facingRight,
			State q,
			byte write,
			short d,
			ulong shifts,
			byte[] readUnpacked,
			byte[] writeUnpacked)
		{
			if (source == null)
				throw new ArgumentNullException ();
			if (readUnpacked == null)
				throw new ArgumentNullException ();
			if (writeUnpacked == null)
				throw new ArgumentNullException ();

			Source = source;
			Read = read;
			StartFacingRight = facingRight;
			Next = q;
			Write = write;
			Direction = d;
			Shifts = shifts;

			_ReadUnpacked = readUnpacked;
			_WriteUnpacked = writeUnpacked;

			if (Next != null) {
				if (false
					|| (StartFacingRight && Direction == readUnpacked.Length)
					|| (!StartFacingRight && Direction == +1)
					) {
					EndFacingRight = true;
				}
				else if (false
				       || (!StartFacingRight && Direction == -readUnpacked.Length)
				       || (StartFacingRight && Direction == -1)
					) {
					EndFacingRight = false;
				}
				else {
					EndFacingRight = null;
				}
			}
		}

		public override string ToString ()
		{
			return ToString (null);
		}

		public string ToString (TmDefinition def)
		{
			var sb = new StringBuilder ();
			Func<byte, string> convertSingle = b => def == null ? b.ToString () : def.Gamma[b];

			var headPos = StartFacingRight ? 0 : _WriteUnpacked.Length - 1;
			var unpacked = _ReadUnpacked.Select (convertSingle).ToArray ();
			sb.Append (TapeUtil.WriteHeadCell (StartFacingRight, unpacked, headPos, Source.ToString ()));

			sb.Append (" => ");

			if (Next != null) {
				headPos += Direction;
				unpacked = _WriteUnpacked.Select (convertSingle).ToArray ();
				sb.Append (TapeUtil.WriteHeadCell (EndFacingRight.Value, unpacked, headPos, Next.ToString ()));
			}
			else {
				sb.Append ("[INFTY]");
			}

			sb.Append (" (" + Shifts + "s)");

			return sb.ToString ();
		}

		public static MacroTransition CreateSingleMacroTransition (
			TmDefinition def,
			State q,
			byte readPacked,
			bool facingRight,
			MacroPacker packerInfo)
		{
			if (def == null)
				throw new ArgumentNullException ();
			if (q == null)
				throw new ArgumentNullException ();
			if (packerInfo == null)
				throw new ArgumentNullException ();

			var readUnpacked = packerInfo.Decode (readPacked);
			var left = facingRight ? new byte[0] : readUnpacked.SubArray (0, readUnpacked.Length - 1);
			var right = facingRight ? readUnpacked : readUnpacked.SubArray (readUnpacked.Length - 1, 1);
			var tape = new LimitedBasicTape (left, right, def.Gamma);
			short dir;

			var tm = new SimpleTmRun (def, tape, new LimitedBasicTape (tape, def.Gamma), q);
			tm.AfterStep += new DetectTapeTooSmall ().Detect;
			tm.Options.AllowCreateMissingTransitionBranches = false;

			try {
				TmPrintOptions.Push ();
				TmPrintOptions.PrintTapeSteps = false;
				TmPrintOptions.PrintTransitionLevel = PrintTransitionLevel.None;

				var prevPos = (short) tape.Position;
				tm.Run ();
				dir = (short) (tape.Position - prevPos);
			}
			catch (TransitionNotDefinedException) {
				// Intentional throw. Let the caller handle this.
				throw;
			}
			finally {
				TmPrintOptions.Pop ();
			}

			Debug.Assert (tm.Result.Halted.HasValue != tape.ExitsOnLeftSide.HasValue);

			if (tm.Result.Halted == false) {
				return new MacroTransition (q, readPacked, facingRight, null, readPacked, 0, tm.Shifts, readUnpacked, readUnpacked);
			}
			else {
				var writeUnpacked = tape.Tape;
				var writePacked = packerInfo.Encode (writeUnpacked);
				return new MacroTransition (q, readPacked, facingRight, tm.Q, writePacked, dir, tm.Shifts, readUnpacked, writeUnpacked);
			}
		}
	}
}
