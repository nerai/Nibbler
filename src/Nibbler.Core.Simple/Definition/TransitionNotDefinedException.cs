using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TuringBox.TM
{
	public class TransitionNotDefinedException : Exception
	{
		public readonly byte? ReadSymbol;

		public TransitionNotDefinedException (byte readSymbol)
			: base ()
		{
			ReadSymbol = readSymbol;
		}

		public TransitionNotDefinedException (string message)
			: base (message)
		{
			ReadSymbol = null;
		}

		public TransitionNotDefinedException (byte? readSymbol, string message, Exception innerException)
			: base (message, innerException)
		{
			ReadSymbol = readSymbol;
		}
	}
}
