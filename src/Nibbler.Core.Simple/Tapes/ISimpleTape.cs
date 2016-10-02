using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nibbler.Core.Simple
{
	public interface ISimpleTape : ITape
	{
		byte ReadSingle ();

		bool WriteSingle (byte b, int direction);
	}
}
