using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nibbler.Core.Simple.Run;
using TuringBox.Tapes;
using TuringBox.TM;
using TuringBox.TM.Run;

namespace TuringTest
{
	[TestClass ()]
	public class BeaverTest
	{
		[TestMethod (), Timeout (500)]
		public void TestBB2 ()
		{
			Run ("BusyBeaver2", false, 4, 6);
			Run ("BusyBeaver2", true, 4, 6);
		}

		[TestMethod (), Timeout (500)]
		public void TestBB3 ()
		{
			Run ("BusyBeaver3", false, 6, 14);
			Run ("BusyBeaver3", true, 6, 14);
		}

		[TestMethod (), Timeout (500)]
		public void TestBB4 ()
		{
			Run ("BusyBeaver4", false, 13, 107);
			Run ("BusyBeaver4", true, 13, 107);
		}

		[TestMethod (), Timeout (500)]
		public void TestBusyBeaver5_Mabu1 ()
		{
			Run ("BusyBeaver5_Mabu1", false, 4098, 47176870);
			Run ("BusyBeaver5_Mabu1", true, 4098, 47176870);
		}

		[TestMethod (), Timeout (500)]
		public void TestBusyBeaver5_Mabu2 ()
		{
			Run ("BusyBeaver5_Mabu2", false, 4098, 11798826);
			Run ("BusyBeaver5_Mabu2", true, 4098, 11798826);
		}

		[TestMethod (), Timeout (5000)]
		public void TestBB6_1_Marxen_136612 ()
		{
			Run ("BB6_1_Marxen_136612", true, 136612, 13122572797);
		}

		[TestMethod (), Timeout (500)]
		public void TestLocalInfiniteLoop ()
		{
			Run ("LocalInfiniteLoop", true, 0, 0);
		}

		static void Run (
			string sdef,
			bool useHistory,
			ulong ones,
			ulong shifts
			)
		{
			var def = DefinitionLibrary.Load (sdef);
			var macro = (uint) (def.SuggestedMacroSize ?? 1);
			var tape = new PackedExponentTape (macro, def.Gamma, null);
			var hist = useHistory ? new History<PackedExponentTape> (def) : null;
			var tm = new MmRun (def, tape, new MacroLibrary (), null, hist);

			TmPrintOptions.PrintTapeSteps = true;
			TmPrintOptions.PrintTransitionLevel = PrintTransitionLevel.OnlyCTR;

			tm.Options.UseMacros = true;
			tm.Options.UseMacroForwarding = true;

			tm.Run (long.MaxValue);
			tm.Result.Print ();

			if (ones > 0) {
				Assert.AreEqual (true, tm.Result.Halted);
				Assert.AreEqual (ones, tm.Result.SymbolsOnTape);
				Assert.AreEqual (shifts, tm.Shifts);
			}
			else {
				Assert.AreEqual (false, tm.Result.Halted);
			}
		}
	}
}
