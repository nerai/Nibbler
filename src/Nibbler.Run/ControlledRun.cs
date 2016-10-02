using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ConsoleMenu;
using ExampleMenu;
using ExampleMenu.Examples;
using ExampleMenu.Examples.Recording;
using Nibbler.Core.Macro.Definition;
using Nibbler.Core.Macro.Run;
using Nibbler.Core.Macro.Tapes;
using Nibbler.Core.Simple.Definition;
using Nibbler.Core.Simple.Run;
using Unlog;

namespace Nibbler.Run
{
	public class ControlledRun
	{
		private TmDefinition _Def;
		public MmRun MM;

		public event Action<MmRun> MmCreated = delegate { };

		public void ImmediateInput (string s)
		{
			menu.CQ.ImmediateInput (s);
		}

		private readonly CMenu menu = new CMenu ();

		private class Menu_Print : CMenuItem
		{
			private readonly ControlledRun CR;

			public Menu_Print (ControlledRun cr)
				: base ("print")
			{
				CR = cr;

				HelpText = ""
					+ "print\n"
					+ "Configures printing options.\n"
					+ "print steps [show|hide]\n"
					+ "print trans [all|ctr|none]\n"
					+ "\n"
					+ "The 'print' commands changes which configurations and transitions are displayed.\n"
					+ "\n"
					+ "To display both the tape and the transitions:\n"
					+ "$ print steps show\n"
					+ "$ print trans all\n"
					+ "\n"
					+ "To hide all regular output:\n"
					+ "$ print steps hide\n"
					+ "$ print trans none\n"
					;

				Add (new CMenuItem ("steps")  {
					{"show",  s => TmPrintOptions.PrintTapeSteps = true},
					{"hide",  s => TmPrintOptions.PrintTapeSteps = false},
				});

				Add (new CMenuItem ("trans")  {
					{"all",  s => TmPrintOptions.PrintTransitionLevel = PrintTransitionLevel.All},
					{"ctr",  s => TmPrintOptions.PrintTransitionLevel = PrintTransitionLevel.OnlyCTR},
					{"none", s => TmPrintOptions.PrintTransitionLevel = PrintTransitionLevel.None},
				});

				Add ("latex", s => {
					TmPrintOptions.ExplodeExponents = true;
					TmPrintOptions.LeftAlignShifts = true;
					TmPrintOptions.OutputForLatex = true;
					TmPrintOptions.DecodeWords = false;
				});
			}

			public override void Execute (string s)
			{
				if (CR.MM == null) {
					Console.WriteLine ("Error: No MM selected.");
					return;
				}
				ExecuteChild (s);
			}
		}

		private class Menu_Lib : CMenuItem
		{
			private readonly ControlledRun CR;

			public Menu_Lib (ControlledRun cr)
				: base ("lib")
			{
				CR = cr;
				HelpText = ""
					+ "lib [name]\n"
					+ "Loads the specified TM definition from library, or\n"
					+ "Displays all available definitions\n"
					+ "\n"
					+ "Examples:\n"
					+ "\n"
					+ "$ lib\n"
					+ "BusyBeaver2         1RB 1LB, 1LA 1RH, K = 1\n"
					+ "BusyBeaver3         1RB 1RH, 0RC 1RB, 1LC 1LA, K = 1\n"
					+ "\n"
					+ "$ lib bb 2\n"
					+ "Loading BusyBeaver2\n";

				Add ("", s => {
					DefinitionLibrary.PrintContent ();
				});
				Add ("reexport", s => {
					DefinitionLibrary.ReexportJson ();
				});
				Add (null, s => {
					var sdef = DefinitionLibrary.GetDefinitionByName (s);
					if (sdef != null) {
						Log.WriteLine ("Loading " + sdef);
						CR._Def = DefinitionLibrary.Load (sdef);
					}
				});
			}
		}

		private class Menu_Def : CMenuItem
		{
			private readonly ControlledRun CR;

			public Menu_Def (ControlledRun cr)
				: base ("def")
			{
				CR = cr;

				HelpText = ""
					+ "def [k x|definition]\n"
					+ "Loads, changes or displays a TM definition\n"
					+ "\n"
					+ "'def d' loads the definition d, overwriting the currently loaded TM.\n"
					+ "Using 'def k x' changes the macro size of the loaded TM to x. x must be a positive integer between 1 and 8.\n"
					+ "The 'def' command without argument displays the definition of the currently loaded TM.\n"
					+ "\n"
					+ "Examples:\n"
					+ "\n"
					+ "$ def 1RA 1RH\n"
					+ "Loaded definition: 1RA 1RH,\n"
					+ "$ def k 2\n"
					+ "Macro sized changed: 1RA 1RH, K = 2\n"
					+ "$ def\n"
					+ "1RA 1RH, K = 2\n";

				Add ("", s => {
					// Write current definition
					Log.WriteLine (CR._Def);
				});
				Add ("k", s => {
					// Change macro size
					int k = int.Parse (s);
					var sdef = CR._Def.GetShortDefinitionString (k, null);
					CR._Def = new TmDefinition (sdef);
					Log.WriteLine ("Macro sized changed: " + CR._Def);
				});
				Add (null, s => {
					CR._Def = new TmDefinition (s);
					Log.WriteLine ("Loaded definition: " + CR._Def);
				});
			}
		}

		private class Menu_Create : CMenuItem
		{
			private readonly ControlledRun CR;

			public Menu_Create (ControlledRun cr)
				: base ("create")
			{
				CR = cr;

				HelpText = ""
					+ "create [nohist] [silent] [nofastmacro]\n"
					+ "Creates a new TM from the currently loaded TM definition.\n"
					+ "\n"
					+ "Options: \n"
					+ "nohist      No history is created, disabling nontermination proofs by repetition.\n"
					+ "silent      Step and transition output disabled.\n"
					+ "nofastmacro Macro forwarding disabled.\n"
					+ "\n"
					+ "Example:\n"
					+ "\n"
					+ "$ def 1RA 1RH\n"
					+ "Loaded definition: 1RA 1RH,\n"
					+ "$ create\n"
					+ "Selected machine: 1RA 1RH,\n"
					+ "";
			}

			public override void Execute (string s)
			{
				var options = s.Split (null)
					.Select (o => o.ToLowerInvariant ())
					.Where (o => !string.IsNullOrWhiteSpace (o))
					.ToArray ();

				var hist = options.Contains ("nohist")
					? null
					: new History<PackedExponentTape> (CR._Def);
				var macro = (uint) (CR._Def.SuggestedMacroSize ?? 1);
				var tape = new PackedExponentTape (macro, CR._Def.Gamma, null);
				CR.MM = new MmRun (CR._Def, tape, new MacroLibrary (), null, hist);

				var silent = options.Contains ("silent");

				TmPrintOptions.PrintTapeSteps = !silent;
				TmPrintOptions.PrintTransitionLevel = silent
					? PrintTransitionLevel.None
					: PrintTransitionLevel.All;

				var useFastMacro = !options.Contains ("nofastmacro");
				CR.MM.Options.UseMacros = true; // must be true unless tape is not exponential
				CR.MM.Options.UseMacroForwarding = useFastMacro;

				CR.MM.AfterStep += new DetectTapeTooSmall ().Detect;
				CR.MM.Result.Changed += () => {
					Console.WriteLine ("Result of " + CR._Def.ShortDefinitionString + ": " + CR.MM.Result);
				};

				using (var block = Log.BeginLocal ()) {
					Log.Write ("Selected machine: ");
					Log.BackgroundColor = ConsoleColor.Yellow;
					Log.ForegroundColor = ConsoleColor.Black;
					Log.Write (CR._Def.ShortDefinitionString);
					Log.ResetColor ();
					if (options.Any ()) {
						Log.Write ("; Options: ");
						Log.Write (s);
					}
					Log.WriteLine ();
				}

				CR.RaiseMmCreated ();
			}
		}

		private void RaiseMmCreated ()
		{
			MmCreated (MM);
		}

		private class Menu_Step : CMenuItem
		{
			private readonly ControlledRun CR;

			public Menu_Step (ControlledRun cr)
				: base ("step")
			{
				CR = cr;

				HelpText = "step n\n"
					+ "Performs n steps.\n"
					+ "\n"
					+ "'step n' lets the currently selected TM run for n steps. A step is either a basic transition or a single whole block operation, if enabled.\n"
					+ "\n"
					+ "Example:\n"
					+ "\n"
					+ "$ lib simple tree\n"
					+ "Loading SimpleChristmasTree\n"
					+ "$ create\n"
					+ "Selected machine: 1RB 1LA, 1LA 1RC, 1RH 1RB,\n"
					+ "$ print steps hide\n"
					+ "$ step 8\n"
					+ "A>0 => 1 B> (1s)\n"
					+ "B>0 => <A 1 (1s)\n"
					+ "1<A => <A 1 (1s)x1\n"
					+ "0<A => 1 B> (1s)\n"
					+ "B>1 => 1 C> (1s)\n"
					+ "C>1 => 1 B> (1s)\n"
					+ "B>0 => <A 1 (1s)\n"
					+ "1<A => <A 1 (1s)x3\n";
			}

			public override void Execute (string s)
			{
				if (CR.MM == null) {
					Log.WriteLine ("Error: No MM selected.");
					return;
				}

				ulong n = 1;
				if (!string.IsNullOrWhiteSpace (s)) {
					n = ulong.Parse (s);
				}
				CR.MM.Run (n);
			}
		}

		private class Menu_Run : CMenuItem
		{
			private readonly ControlledRun CR;

			public Menu_Run (ControlledRun cr)
				: base ("run")
			{
				CR = cr;

				HelpText = "run [s]\n"
					+ "Runs the simulation.\n"
					+ "\n"
					+ "'run' simulates the machine until it halts.\n"
					+ "If a parameter s is given, the machine stops once its shift count is at least s.\n"
					+ "\n"
					+ "Example of running until a machine halts:\n"
					+ "\n"
					+ "$ lib mabu 2\n"
					+ "Loading BusyBeaver5_Mabu2\n"
					+ "$ create\n"
					+ "Selected machine: 1LB 1LA, 1RC 1RB, 1LA 1RD, 1LA 1RE, 1RH 0RC, K=3\n"
					+ "$ print steps hide\n"
					+ "$ print trans none\n"
					+ "$ run\n"
					+ "Result of 1LB 1LA, 1RC 1RB, 1LA 1RD, 1LA 1RE, 1RH 0RC, K=3: HALTING WITH REJECT after 11798826 shifts. 4098 symbols on tape. Tape: 11798826~ 011 101^2047 110\n"
					+ "\n"
					+ "Example of running up to a certain number of shifts:\n"
					+ "\n"
					+ "$ lib mabu 1\n"
					+ "Loading BusyBeaver5_Mabu1\n"
					+ "$ create\n"
					+ "Selected machine: 1LB 1RC, 1LC 1LB, 1LD 0RE, 1RA 1RD, 1LH 0RA, K=3\n"
					+ "$ print steps hide\n"
					+ "$ print trans none\n"
					+ "$ run 100000\n"
					+ "$ print steps show\n"
					+ "$ step 1\n"
					+ "100304~ 110 010^188 E> 100\n"
					+ "100307~ 110 010^188 <C 110\n";
			}

			public override void Execute (string s)
			{
				if (CR.MM == null) {
					Log.WriteLine ("Error: No MM selected.");
					return;
				}

				var restore = CR.MM.TargetShifts;
				if (!string.IsNullOrWhiteSpace (s)) {
					var to = ulong.Parse (s);
					CR.MM.TargetShifts = to;
				}
				CR.MM.Run ();
				CR.MM.TargetShifts = restore;
			}
		}

		private class Menu_Break : CMenuItem
		{
			private readonly ControlledRun CR;

			public Menu_Break (ControlledRun cr)
				: base ("break")
			{
				CR = cr;

				HelpText = "break s\n"
					+ "Inserts the breakpoint s, which must be a positive integer. "
					+ "Once the machine's shift count reaches s, execution is stopped and control is transferred to a debugger. "
					+ "If no debugger is already attached, it will be launched.";
			}

			public override void Execute (string s)
			{
				if (CR.MM == null) {
					Log.WriteLine ("Error: No MM selected.");
					return;
				}

				var at = s.Split (' ', ',', '\t').Select (x => ulong.Parse (x)).ToArray ();
				CR.MM.AddBreakpoint (at);
			}
		}

		private class Menu_Stopwatch : CMenuItem
		{
			private Stopwatch _SW;

			public Menu_Stopwatch ()
				: base ("stopwatch")
			{
				HelpText = "stopwatch start|stop\n"
					+ "Starts a stopwatch or stops it and displays the elapsed time.";

				Add ("start", s => {
					_SW = Stopwatch.StartNew ();
				});
				Add ("stop", s => {
					_SW.Stop ();
					Log.WriteLine ("Elapsed: " + _SW.ElapsedMilliseconds + "ms");
				});
			}
		}

		public ControlledRun (bool passive)
		{
			menu.CQ.PassiveMode = passive;

			menu.Add (new Menu_Print (this));
			menu.Add (new Menu_Lib (this));
			menu.Add (new Menu_Def (this));
			menu.Add (new Menu_Create (this));
			menu.Add (new Menu_Step (this));
			menu.Add (new Menu_Run (this));
			menu.Add (new Menu_Break (this));
			menu.Add (new Menu_Stopwatch ());
			//menu.Add ("log-break", s => Log.AddBreakpoint (s));

			menu.Add (new MI_Echo ());
			menu.Add (new MI_Pause ());

			var store = new FileRecordStore ();
			menu.Add (new MI_Record (store) {
				Selector = "{",
				EndRecordCommand = "}",
			});
			menu.Add (new MI_Replay (menu, store) {
				Selector = "!",
			});

			var mi_if = menu.Add (new MI_If ());
			mi_if.Conditions.Add ("hist-has", Condition_HistHas);

			_Def = DefinitionLibrary.Load ("BusyBeaver2");
		}

		public void Run ()
		{
			menu.Run ();
		}

		private bool Condition_HistHas (ref string s)
		{
			if (MM == null) {
				return false;
			}

			var sshifts = MenuUtil.SplitFirstWord (ref s);
			var shifts = ulong.Parse (sshifts);
			var prev = MM.History.GetPrevious (shifts);
			return prev != null;
		}
	}
}
