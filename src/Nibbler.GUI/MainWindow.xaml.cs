using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ConsoleMenu;
using TuringRunner;
using Unlog;
using Unlog.AdditionalTargets;

namespace BeaverUI
{
	public partial class MainWindow : Window
	{
		private Thread tRun;
		private readonly ControlledRun Run;

		public MainWindow ()
		{
			InitializeComponent ();

			var t = new WpfRtfLogTarget (txtLog);
			t.DefaultBackgroundColor = Colors.Black;
			t.DefaultForegroundColor = Colors.White;
			Log.AddTarget (t);
			Log.AllowAsynchronousWriting = false;

			Run = new ControlledRun (true);
			Run.MmCreated += mm => {
				mm.AfterStep += ucTape.UpdateTapeVisualization;
				mm.AfterStep += x => Thread.Sleep (100);

				Dispatcher.Invoke ((Action) (() => {
					txtMachineInfo.Text =
						mm.Definition.GetShortDefinitionString (null, null) + "\n" +
						mm.Definition.GetLongDefinitionString (null).Replace ("\n", "").Replace ("\r", "");
				}));
			};

			tRun = new Thread (() => {
				Run.Run ();
			}) {
				IsBackground = true,
				Name = "WPF TM Run thread",
			};
			tRun.Start ();
		}

		private void txtInput_KeyDown (object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter) {
				var s = txtInput.Text;
				txtInput.Text = "";
				Run.ImmediateInput (s);
			}
		}

		/*
		private void TmRun ()
		{
			var def = DefinitionLibrary.Load ("BusyBeaver3");
			for (; ; ) {
				try {
					TmRun (def);
					break;
				}
				catch (PoorChoiceOfBlockSizeException choice) {
					ucTape.Reset ();
					def = new TmDefinition (def.GetShortDefinitionString (choice.Suggestion, null));
				}
			}
		}
		*/
	}
}
