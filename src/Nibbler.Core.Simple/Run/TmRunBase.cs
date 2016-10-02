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
	public interface ITmRunBase
	{
		State Q { get; }

		ulong Shifts { get; }

		ITape Tape { get; }

		TmRunResult Result { get; }

		void WriteCurrentState ();

		TmDefinition Definition { get; }
	}

	public abstract class TmRunBase<TTape> : ITmRunBase
		where TTape : class, ITape
	{
		public event Action<TmRunBase<TTape>> AfterStep = delegate { };

		private readonly TmDefinition _Definition;

		public TmDefinition Definition
		{
			get {
				return _Definition;
			}
		}

		public State Q { get; protected set; }

		public readonly TTape Tape;

		public ulong Shifts
		{
			get {
				return Tape.Shifts;
			}
		}

		/// <summary>
		/// Branches according to TNF rules. This is filled if unspecified transitions are
		/// encountered during execution of this machine.
		/// </summary>
		public readonly List<string> Branches = new List<string> ();

		public readonly TmRunOptions Options = new TmRunOptions ();

		public TmRunResult Result { get; private set; }

		public readonly History<TTape> History;

		public void AddBreakpoint (params ulong[] at)
		{
			_Breakpoints.AddRange (at);
			_Breakpoints.Sort ();
		}

		public ulong TargetShifts = ulong.MaxValue;

		private readonly List<ulong> _Breakpoints = new List<ulong> ();

		public bool AllowMacroSizeSuggestions = false;

		public List<string> PrintStepsOnlyInState = null;

		public TmRunBase (
			TmDefinition def,
			TTape tape,
			State initialState = null,
			History<TTape> hist = null)
		{
			if (def == null)
				throw new ArgumentNullException ();
			if (tape == null)
				throw new ArgumentNullException ();

			_Definition = def;
			Tape = tape;
			Q = initialState ?? _Definition.Q0;
			History = hist;
			Result = new TmRunResult (this);
		}

		protected abstract void BeforeRunCheck ();

		protected virtual void MacroSizeCheck ()
		{
		}

		protected abstract bool Step ();

		public void Run (ulong steps = ulong.MaxValue)
		{
			BeforeRunCheck ();

			while (true) {
				if (steps == 0) {
					break;
				}
				else {
					steps--;
				}

				if (TmPrintOptions.PrintTapeSteps) {
					if (PrintStepsOnlyInState == null || PrintStepsOnlyInState.Contains (Q.Name)) {
						WriteCurrentState ();
					}
				}

				if (AllowMacroSizeSuggestions) {
					if (steps % 100 == 0) {
						MacroSizeCheck ();
					}
				}

				if (_Breakpoints.Any () && _Breakpoints[0] <= Shifts) {
					_Breakpoints.RemoveAt (0);
					if (Debugger.IsAttached || Debugger.Launch ()) {
						Debugger.Break ();
					}
					else {
						throw new Exception ("ERROR - Unable to launch debugger on TM breakpoint. Will raise exception instead.");
					}
				}

				if (Shifts >= TargetShifts) {
					break;
				}

				if (History != null) {
					var cfg = GetMachineConfigB ();
					var proof = History.CheckAndAdd (cfg);
					if (proof != null) {
						Result.SetNonhalting (proof);
						break;
					}
				}

				if (Q.Delta == null) {
					Result.SetHalted (Q == _Definition.Qa, Tape.SymbolsOnTape ());
					break;
				}

				if (!Step ()) {
					break;
				}

				AfterStep (this);
			}

			if (TmPrintOptions.PrintTapeSteps) {
				WriteCurrentState ();
			}
		}

		public void WriteCurrentState ()
		{
			using (var block = Log.BeginLocal ()) {
				Log.ForegroundColor = ConsoleColor.Cyan;
				Log.Write (GetMachineConfigB ().ToString ());
				Log.ResetColor ();
				Log.WriteLine ();
			}
		}

		public TmConfiguration<TTape> GetMachineConfigB ()
		{
			var tmc = new TmConfiguration<TTape> (Q, Tape);
			return tmc;
		}

		public override string ToString ()
		{
			return GetMachineConfigB ().ToString ();
		}

		ITape ITmRunBase.Tape
		{
			get { return Tape; }
		}

		protected SimpleTransition CreateMissingTransitionBranches (byte read)
		{
			SimpleTransition trans = null;

			var nTransitions = _Definition.Q.Values.Sum (q => q.Delta == null ? 0 : q.Delta.Count (t => t != null));
			if (nTransitions == _Definition.Q.Count * 2 - 3) {
				// Last transition is always the halting state
				trans = new SimpleTransition (Q, read, _Definition.Qa ?? _Definition.Qr, 1, 1);
			}
			else {
				var from = _Definition.Q.Values.Where (q => q.Delta != null);
				var qs = from.Take (1).ToList ();
				qs.AddRange (from.Skip (1).TakeWhile (q => q.Sources.Any ()));
				qs.AddRange (from.Skip (1).SkipWhile (q => q.Sources.Any ()).Take (1));

				var Directions = new short[] { 1, -1 };
				foreach (var q in qs) {
					foreach (var o in _Definition.Gamma.Keys) {
						foreach (var d in Directions) {
							var t = new SimpleTransition (Q, read, q, o, d);

							if (trans == null) {
								trans = t;
							}
							else {
								var branch = _Definition.GetShortDefinitionString (null, t);
								Branches.Add (branch);
							}
						}
					}
				}
			}

			_Definition.UpdateTransition (trans);
			return trans;
		}
	}
}
