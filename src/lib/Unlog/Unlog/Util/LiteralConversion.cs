using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Unlog.Util
{
	public static class LiteralConversion
	{
		/// <summary>
		/// Escapes a string using C# string escaping, so the result will be translated by the C# compiler back
		/// into the original string.
		/// </summary>
		public static string ToLiteral (string input)
		{
			using (var writer = new StringWriter ()) {
				using (var provider = CodeDomProvider.CreateProvider ("CSharp")) {
					provider.GenerateCodeFromExpression (new CodePrimitiveExpression (input), writer, null);
					return writer.ToString ();
				}
			}
		}
	}
}
