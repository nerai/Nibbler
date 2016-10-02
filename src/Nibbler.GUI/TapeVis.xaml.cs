using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TuringBox.Tapes;
using TuringBox.TM.Run;

namespace BeaverUI
{
	public partial class TapeVis : UserControl
	{
		public TapeVis ()
		{
			InitializeComponent ();

			/*
			for (int i = 0; i < 20; i++) {
				CreateCV ();
			}
			*/
		}

		private List<CellVis> _CellVisualization = new List<CellVis> ();

		private CellVis CreateCV ()
		{
			CellVis cv = null;
			Dispatcher.Invoke ((Action) (() => {
				cv = new CellVis ();
				stackTape.Children.Add (cv);
			}));
			_CellVisualization.Add (cv);
			return cv;
		}

		private IEnumerable<CellVis> CellVisFactory ()
		{
			while (true) {
				yield return CreateCV ();
			}
		}

		public void UpdateTapeVisualization (TmRunBase<PackedExponentTape> tm)
		{
			var t = tm.Tape;
			var cells = t.OrderedCells;
			var cviter = _CellVisualization.Concat (CellVisFactory ()).GetEnumerator ();
			Func<byte, string> decode = b => string.Concat (t.Macro.Decode (b));

			CellVis l = null;
			CellVis r = null;

			if (t.Left == null && !t.FacingRight) {
				// virtual leftmost node
				cviter.MoveNext ();
				l = cviter.Current;
				l.Head = null;
				l.Data = decode (0);
				l.Exponent = 0;
			}

			foreach (var c in cells) {
				cviter.MoveNext ();
				var cv = cviter.Current;
				cv.Head = null;
				cv.Data = decode (c.Data);
				cv.Exponent = c.Exponent;
				if (c == t.Left)
					l = cv;
				if (c == t.Right)
					r = cv;
			}

			if (t.Right == null && t.FacingRight) {
				// virtual rightmost node
				cviter.MoveNext ();
				r = cviter.Current;
				r.Head = null;
				r.Data = decode (0);
				r.Exponent = 0;
			}

			(t.FacingRight ? r : l).Head = tm.Q.ToString (); // todo direction

			Dispatcher.Invoke ((Action) (() => {
				foreach (var c in _CellVisualization) {
					c.Update ();
				}
			}));
		}

		public void Reset ()
		{
			_CellVisualization.Clear ();
			Dispatcher.Invoke ((Action) (() => {
				stackTape.Children.Clear ();
			}));
		}
	}
}
