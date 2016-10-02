using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nibbler.Core.Simple.Run
{
	public enum PrintTransitionLevel
	{
		None = 0,
		OnlyCTR = 50,
		All = 100,
	}

	public class TmRunOptions
	{
		public bool AllowCreateMissingTransitionBranches = true;
		public bool UseMacros = false;
		public bool UseMacroForwarding = false;
	}
}
