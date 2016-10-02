using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nibbler.Core.Simple;

namespace TuringBox.TM.Run
{
	public class DetectTapeTooSmall
	{
		public void Detect (ITmRunBase tm)
		{
			if (tm.Shifts % 1000 == 0) { // TODO this does not work well with block transitions
				var proof = IsIndefinite (tm);
				if (proof != null) {
					tm.Result.SetNonhalting (proof);
				}
			}
		}

		private static long DebugStats_Indefinite_TrivialRunner = 0;
		private static long DebugStats_Indefinite_SpaceTooSmall = 0;

		private string IsIndefinite (ITmRunBase tm)
		{
			if (tm.Tape is ISimpleTape) { // todo what about MM?
				var tape = tm.Tape as ISimpleTape;

				HeadPosition atEnd = tm.Tape.IsAtEnd;
				if (atEnd != HeadPosition.Neither && tape.ReadSingle () == 0) {
					var closure = tm.Q.TransitiveClosure (
						t => t != null && t.Read == 0,
						t => t.Direction != (int) atEnd);
					if (closure != null && !closure.Contains (tm.Definition.Qr)) {
						DebugStats_Indefinite_TrivialRunner++;
						return ""
							+ "Runs indefinitely. Reason: "
							+ "We're at the end of the tape and the tape is already 0. "
							+ "All further transitions reached via input 0 only direct further away. "
							+ "But those nodes do not contain the exit. "
							+ "Thus we're certainly in an infinite run.";
					}
				}
			}

			ulong nTape = tm.Tape.TotalTapeLength + 1;
			if (nTape < 30) {
				int usedStates = tm.Definition.Q.Count (q => q.Value.Sources.Any ());
				ulong requiredSpace = (ulong) usedStates * nTape * (ulong) (2 << (int) nTape);
				if (tm.Shifts > requiredSpace) {
					DebugStats_Indefinite_SpaceTooSmall++;
					return ""
						+ "Runs indefinitely. Reason: "
						+ "The tape used is too small to hold as many different configurations as there have been shifts. "
						+ "Used states: " + usedStates + ", required space: " + requiredSpace + ", shifts: " + tm.Shifts + " "
						+ "(This can only be used for small tapes.)";
				}
			}

			return null;
		}
	}
}
