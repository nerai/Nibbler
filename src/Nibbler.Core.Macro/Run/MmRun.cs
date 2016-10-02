using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Nibbler.Core.Simple.Run;
using TuringBox.Tapes;
using TuringBox.Utils;
using Unlog;

namespace TuringBox.TM.Run
{
	public class MmRun : TmRunBase<PackedExponentTape>
	{
		public Func<MmRun, bool?> PrestepHook = null;

		public readonly MacroLibrary ML;

		public MmRun (
			TmDefinition def,
			PackedExponentTape tape,
			MacroLibrary ml,
			State initialState = null,
			History<PackedExponentTape> history = null)
			: base (def, tape, initialState, history)
		{
			ML = ml;
		}

		/// <returns>
		/// false iff (any of) NT or T
		/// </returns>
		protected override bool Step ()
		{
			if (PrestepHook != null) {
				var hookresult = PrestepHook (this);
				if (hookresult != null) {
					return hookresult.Value;
				}
			}

			MacroTransition mt;
			while (true) {
				try {
					mt = Options.UseMacros ? GetMacro () : null;
					break;
				}
				catch (TransitionNotDefinedException ex) {
					CreateMissingTransitionBranches (ex.ReadSymbol.Value);
				}
			}

			if (mt == null) {
				throw new NotImplementedException ();
			}

			if (!MacroTransitionStep (mt)) {
				return false;
			}

			return true;
		}

		private bool MacroTransitionStep (MacroTransition t)
		{
			if (t == null)
				throw new ArgumentNullException ();

			if (t.Next == null) {
				Result.SetNonhalting ("Single cell runs indefinitely: " + t.ToString (Definition));
				return false;
			}

			if (TmPrintOptions.PrintTransitionLevel >= PrintTransitionLevel.All) {
				Log.ForegroundColor = ConsoleColor.Yellow;
				Log.Write (t.ToString (Definition));
			}

			try {
				if (t.EndFacingRight == null) {
					// transition to halting state
					Tape.WriteSingleInCell (t.Write, t.Direction, t.Shifts);
				}
				else {
					bool dir;
					bool whole;

					if (true
						&& Options.UseMacroForwarding
						&& t.Source == t.Next
						&& t.StartFacingRight == t.EndFacingRight
						) {
						if (TmPrintOptions.PrintTransitionLevel >= PrintTransitionLevel.All) {
							var reps = Tape.GetEqualSymbolRepetitionCount ();
							Log.Write ("x" + reps.ToString ());
						}
						dir = t.StartFacingRight;
						whole = true;
					}
					else {
						dir = t.Direction > 0;
						whole = false;
					}
					if (!Tape.WritePacked (t.Write, dir, t.Shifts, whole)) {
						Result.SetRefusedCandidate ("A cell with a virtual exponent without direct value was accessed. This operation is invalid, as it may depend on the virtual's value. (LONELY_EXPONENT_EXCEPTION)");
						return false;
					}
				}
			}
			finally {
				if (TmPrintOptions.PrintTransitionLevel >= PrintTransitionLevel.All) {
					Log.WriteLine ();
					Log.ResetColor ();
				}
			}

			Q = t.Next;
			return true;
		}

		private MacroTransition GetMacro ()
		{
			if (Tape.IsMisaligned) {
				return null;
			}

			var read = Tape.ReadPacked ();
			var mt = ML.GetMacro (Q, read, Tape.FacingRight);
			if (mt == null) {
				mt = MacroTransition.CreateSingleMacroTransition (Definition, Q, read, Tape.FacingRight, Tape.Macro);
				if (mt != null) {
					ML.LearnMacro (mt);
				}
			}

			return mt;
		}

		protected override void BeforeRunCheck ()
		{
			if (!(Tape is PackedExponentTape)) {
				throw new InvalidOperationException ("Cannot run on non packed exponent tape.");
			}
			if (Tape is PackedExponentTape != Options.UseMacros) {
				throw new InvalidOperationException ("Packed tape and macro usage must be coherent.");
			}
		}

		protected override void MacroSizeCheck ()
		{
			if (Tape.PackedTapeLength > 10) {
				var suggest = Tape.SuggestMacroSizeEx ();
				if (suggest != null && suggest.Value != Tape.Macro.MacroSize) {
					throw new PoorChoiceOfBlockSizeException (suggest.Value);
				}
			}
		}
	}
}
