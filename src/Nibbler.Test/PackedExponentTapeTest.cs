using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nibbler.Core.Macro.Tapes;

namespace TuringTest
{
	[TestClass ()]
	public class PackedExponentTapeTest
	{
		private readonly Dictionary<byte, string> _DefaultGamma
			= new Dictionary<byte, string> () { { 0, "0" }, { 1, "1" } };

		[TestMethod ()]
		public void EncodeTest ()
		{
			var target = new PackedExponentTape (8, _DefaultGamma, null);
			var data = new byte[] { 1, 1, 1, 0, 0, 0, 0, 1 }; // = 128 + 64 + 32 + 1 = 225
			var actual = target.Macro.Encode (data);
			Assert.AreEqual (225, actual);
		}

		[TestMethod ()]
		public void DecodeTest ()
		{
			var target = new PackedExponentTape (8, _DefaultGamma, null);
			var data = new byte[] { 1, 1, 1, 0, 0, 0, 0, 1 }; // = 128 + 64 + 32 + 1 = 225
			var actual = target.Macro.Decode (225);
			CollectionAssert.AreEqual (data, actual);
		}
	}
}
