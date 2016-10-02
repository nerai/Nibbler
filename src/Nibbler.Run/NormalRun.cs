using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nibbler.Core.Simple.Run;
using TuringBox.Tapes;
using TuringBox.TM;
using TuringBox.TM.Run;
using TuringBox.Utils;
using Unlog;

namespace TuringRunner
{
	public class NormalRun
	{
		public static NormalRun CalculateBB (uint? macro = null)
		{
			var p = new NormalRun () {
				checkNT = false,
				macro = macro,
				printTape = true,
				printTransitionLevel = PrintTransitionLevel.OnlyCTR,
				proof = true,
				maxSteps = ulong.MaxValue,
			};
			return p;
		}

		public bool checkNT = true;
		public uint? macro = null;
		public bool printTape = true;
		public PrintTransitionLevel printTransitionLevel = PrintTransitionLevel.All;
		public bool proof = true;
		public ulong maxSteps = ulong.MaxValue;

		public MmRun MM { get; private set; }

		public void PrepareMM (string sdef)
		{
			var def = DefinitionLibrary.Load (sdef);

			using (var block = Log.BeginLocal ()) {
				Log.BackgroundColor = ConsoleColor.Yellow;
				Log.ForegroundColor = ConsoleColor.Black;
				Log.Write ("Simulating " + def.FullDefinitionString);
				Log.ResetColor ();
				Log.WriteLine ();
			}

			PackedExponentTape tape;
			if (macro == 0) {
				throw new NotSupportedException ();
			}
			else {
				macro = macro ?? (uint?) def.SuggestedMacroSize ?? 1u;
				tape = new PackedExponentTape (macro.Value, def.Gamma, null);
			}
			var hist = (checkNT || proof) ? new History<PackedExponentTape> (def) : null;

			MM = new MmRun (def, tape, new MacroLibrary (), null, hist);
			TmPrintOptions.PrintTapeSteps = printTape;
			TmPrintOptions.PrintTransitionLevel = printTransitionLevel;
			MM.Options.UseMacros = macro != 0;
			MM.Options.UseMacroForwarding = macro != 0;
			if (checkNT) {
				MM.AfterStep += new DetectTapeTooSmall ().Detect;
			}
		}

		public void Run ()
		{
			var time = DateTime.UtcNow;
			MM.Run (maxSteps);
			MM.Result.Print ();
			Log.WriteLine ("Calculated in " + DateTime.UtcNow.Subtract (time).TotalSeconds.ToString ("0.000") + "s");
			Log.WriteLine ();
			Console.ReadKey (true);
		}
	}
}
