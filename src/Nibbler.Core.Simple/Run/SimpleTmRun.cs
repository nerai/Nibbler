using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Nibbler.Core.Simple;
using Nibbler.Core.Simple.Definition;
using Nibbler.Core.Simple.Run;
using Nibbler.Core.Simple.Tapes;
using Unlog;

namespace Nibbler.Core.Simple.Run
{
	public class SimpleTmRun : TmRunBase<ISimpleTape>
	{
		private readonly SimpleTmRun _LoopCheck;

		public SimpleTmRun (
			TmDefinition def,
			ISimpleTape tape,
			ISimpleTape secondary = null,
			State initialState = null,
			History<ISimpleTape> history = null)
			: base (def, tape, initialState, history)
		{
			if (secondary != null) {
				_LoopCheck = new SimpleTmRun (def, secondary, null, initialState);
				_LoopCheck.Options.AllowCreateMissingTransitionBranches = true; // Implies that this machine will follow the path taken by the primary machine.
			}
		}

		protected override bool Step ()
		{
			if (!RegularTransitionStep ()) {
				return false;
			}

			if (_LoopCheck != null && (Shifts % 2 == 0)) {
				_LoopCheck.Run (1);

				if (GetMachineConfigB ().Equals (_LoopCheck.GetMachineConfigB ())) {
					var proof = "Machine does not halt, since the secondary TM has an equal configuration.";
					Result.SetNonhalting (proof);
					return false;
				}
			}

			return true;
		}

		private bool RegularTransitionStep ()
		{
			byte read = Tape.ReadSingle ();
			var t = Q.Delta[read];

			if (t == null) {
				if (Options.AllowCreateMissingTransitionBranches) {
					// undefined transition -> split this TM up into children!
					t = CreateMissingTransitionBranches (read);
				}
				else {
					var msg = "Transition from state " + Q.Name + " with cell " + Definition.Gamma[read] + " is not defined.";
					throw new TransitionNotDefinedException (read, msg, null);
				}
			}

			if (TmPrintOptions.PrintTransitionLevel >= PrintTransitionLevel.All) {
				using (var block = Log.BeginLocal ()) {
					Log.ForegroundColor = ConsoleColor.Yellow;
					Log.Write (t.ToString (Definition));
					Log.ResetColor ();
					Log.WriteLine ();
				}
			}

			Tape.WriteSingle (t.Write, t.Direction);
			Q = t.Next;

			var tt = Tape as LimitedBasicTape; // TODO: check how this is used
			if (tt != null && tt.ExitsOnLeftSide.HasValue) {
				Result.SetRefusedCandidate ("Head is falling off the tape.");
				return false;
			}

			return true;
		}

		protected override void BeforeRunCheck ()
		{
			if (!(Tape is ISimpleTape)) {
				throw new InvalidOperationException ("Cannot run on non simple tape.");
			}
			if (Tape is LimitedBasicTape && Options.UseMacros) {
				throw new InvalidOperationException ("Limited tape does not properly store overextended writes.");
			}
		}
	}
}
