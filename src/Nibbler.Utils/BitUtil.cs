using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nibbler.Utils
{
	public static class BitUtil
	{
		public static uint FloorLog2 (uint x)
		{
			x |= (x >> 1);
			x |= (x >> 2);
			x |= (x >> 4);
			x |= (x >> 8);
			x |= (x >> 16);

			return (uint) (NumBitsSet (x) - 1);
		}

		public static uint CeilingLog2 (uint x)
		{
			int y = (int) (x & (x - 1));
			y |= -y;
			y >>= (sizeof (uint) - 1);

			x |= (x >> 1);
			x |= (x >> 2);
			x |= (x >> 4);
			x |= (x >> 8);
			x |= (x >> 16);

			return (uint) (NumBitsSet (x) - 1 - y);
		}

		public static int NumBitsSet (uint x)
		{
			x -= ((x >> 1) & 0x55555555);
			x = (((x >> 2) & 0x33333333) + (x & 0x33333333));
			x = (((x >> 4) + x) & 0x0f0f0f0f);
			x += (x >> 8);
			x += (x >> 16);

			return (int) (x & 0x0000003f);
		}
	}
}
