using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuringBox.TM.Run;

namespace Nibbler.Core.Simple.Run
{
	public static class TmPrintOptions
	{
		public static bool PrintTapeSteps = false;
		public static PrintTransitionLevel PrintTransitionLevel = PrintTransitionLevel.None;
		public static bool LeftAlignShifts = false;
		public static bool OutputForLatex = false;
		public static bool ExplodeExponents = false;
		public static bool DecodeWords = true;

		private static Stack<Tuple<bool, PrintTransitionLevel, bool, bool, bool, bool>> _Stack
			= new Stack<Tuple<bool, PrintTransitionLevel, bool, bool, bool, bool>> ();

		public static void Push ()
		{
			var t = new Tuple<bool, PrintTransitionLevel, bool, bool, bool, bool> (
				PrintTapeSteps,
				PrintTransitionLevel,
				LeftAlignShifts,
				OutputForLatex,
				ExplodeExponents,
				DecodeWords);
			_Stack.Push (t);
		}

		public static void Pop ()
		{
			var t = _Stack.Pop ();
			PrintTapeSteps = t.Item1;
			PrintTransitionLevel = t.Item2;
			LeftAlignShifts = t.Item3;
			OutputForLatex = t.Item4;
			ExplodeExponents = t.Item5;
			DecodeWords = t.Item6;
		}
	}
}
