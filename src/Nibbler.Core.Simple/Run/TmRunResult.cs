using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nibbler.Core.Simple.Definition;
using Unlog;

namespace Nibbler.Core.Simple.Run
{
	public class TmRunResult
	{
		public event Action Changed = delegate { };

		public readonly ITmRunBase Machine;

		public bool? Halted { get; private set; }

		public bool? Accepted { get; private set; }

		public bool? IsValidBBCandidate { get; private set; }

		public ulong? SymbolsOnTape { get; private set; }

		public readonly List<TmDefinition> Branches = new List<TmDefinition> ();

		public string Proof { get; private set; }

		public TmRunResult (ITmRunBase tm)
		{
			if (tm == null)
				throw new ArgumentNullException ();

			Machine = tm;
		}

		public void SetNonhalting (string proof)
		{
			Halted = false;
			Accepted = false;
			SymbolsOnTape = null;
			IsValidBBCandidate = false;

			Proof = proof;

			Changed ();
		}

		public void SetHalted (bool accepted, ulong symbolsOnTape)
		{
			if (symbolsOnTape < 0)
				throw new ArgumentOutOfRangeException ();

			Halted = true;
			Accepted = accepted;
			SymbolsOnTape = symbolsOnTape;
			IsValidBBCandidate = true;

			Changed ();
		}

		public void SetRefusedCandidate (string reason)
		{
			Halted = null;
			Accepted = null;
			SymbolsOnTape = null;
			IsValidBBCandidate = false;

			Proof = reason;

			Changed ();
		}

		public void Print ()
		{
			using (var block = Log.BeginLocal ()) {
				Log.BackgroundColor = ConsoleColor.White;
				Log.ForegroundColor = ConsoleColor.DarkBlue;
				Log.Write (ToString ());
				Log.ResetColor ();
				Log.WriteLine ();
			}
		}

		public override string ToString ()
		{
			var s = "";

			if (IsValidBBCandidate == false && Halted == null) {
				s += "REFUSED CANDIDATE";
			}
			else if (Halted == null) {
				s += "UNDECIDED";
			}
			else if (Halted == false) {
				s += "PROVEN NON-HALTING";
			}
			else {
				s += "HALTING WITH ";
				s += Accepted.Value ? "ACCEPT" : "REJECT";
			}
			s += " after " + Machine.Shifts + " shifts.";
			if (Halted == true) {
				s += " " + SymbolsOnTape + " symbols on tape.";
			}
			if (Proof != null) {
				s += " " + Proof;
			}

			s += " Tape: " + Machine.Tape.ToString (Machine.Q.ToString ());

			return s;
		}
	}
}
