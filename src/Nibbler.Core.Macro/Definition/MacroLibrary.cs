using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace TuringBox.TM
{
	public class MacroLibrary
	{
		private readonly MultiKeyDictionary<State, byte, MacroTransition> _MacroDeltaL = new MultiKeyDictionary<State, byte, MacroTransition> ();
		private readonly MultiKeyDictionary<State, byte, MacroTransition> _MacroDeltaR = new MultiKeyDictionary<State, byte, MacroTransition> ();

		public MacroTransition GetMacro (State state, byte read, bool facingRight)
		{
			var store = facingRight ? _MacroDeltaR : _MacroDeltaL;
			MacroTransition trans;
			if (store.TryGetValue (state, read, out trans)) {
				trans.UsageCount++;
			}
			return trans;
		}

		public void LearnMacro (MacroTransition t)
		{
			if (t == null) throw new ArgumentNullException ();

			var store = t.StartFacingRight ? _MacroDeltaR : _MacroDeltaL;
			store.Add (t.Source, t.Read, t);
		}
	}
}
