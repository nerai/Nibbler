using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TuringBox.TM.Run;

namespace Nibbler.Core.Simple
{
	public interface ITape
	{
		ulong Shifts { get; }

		ulong SymbolsOnTape ();

		ulong TotalTapeLength { get; }

		HeadPosition IsAtEnd { get; }

		string ToString (string stateName);

		bool CompleteContentEquals (ITape t);
	}
}
