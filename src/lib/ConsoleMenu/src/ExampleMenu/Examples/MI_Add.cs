﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleMenu;

namespace ExampleMenu.Examples
{
	public class MI_Add : CMenu
	{
		public MI_Add ()
			: base ("add", false)
		{
			HelpText = ""
				+ "add\n"
				+ "Adds numbers until \"=\" is entered.";
			PromptCharacter = "+";

			Add ("=", s => Quit (), "Prints the sum and quits the add submenu");
			Add (null, s => AddNumber (s));
		}

		private int _Sum = 0;

		private void AddNumber (string s)
		{
			int i;
			if (int.TryParse (s, out i)) {
				_Sum += i;
			}
			else {
				Console.WriteLine (s + " is not a valid number.");
			}
		}

		public override void Execute (string arg)
		{
			Console.WriteLine ("You're now in submenu <Add>.");
			Console.WriteLine ("Enter numbers. To print their sum and exit the submenu, enter \"=\".");
			_Sum = 0;
			Run ();
			Console.WriteLine ("Sum = " + _Sum);
		}
	}
}
