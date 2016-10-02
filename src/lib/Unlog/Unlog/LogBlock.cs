using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlog
{
	/// <summary>
	/// A local log block. Is stashed and kept by default.
	/// </summary>
	public class LogBlock : IDisposable
	{
		/// <summary>
		/// Set to false to discard this block on dispose instead of writing it to the log output.
		/// </summary>
		private bool _WasDisposed = false;

		public bool Keep = true;

		public void Dispose ()
		{
			if (!_WasDisposed) {
				_WasDisposed = true;
				Log.DoLeave (Keep);
			}
		}
	}
}
