using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nibbler.Core.Macro.Run
{
	public class PoorChoiceOfBlockSizeException : Exception
	{
		public readonly int Suggestion;

		public PoorChoiceOfBlockSizeException (int suggestion)
		{
			Suggestion = suggestion;
		}
	}
}
