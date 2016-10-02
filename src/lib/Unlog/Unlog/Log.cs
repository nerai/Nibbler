using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Unlog.Util;

namespace Unlog
{
	public class Log
	{
		/// <summary>
		/// List of log targets. Add new targets to have all log written to them.
		///
		/// The targets are global, not per instance.
		/// </summary>
		private static List<ILogTarget> _Targets = new List<ILogTarget> ();

		/// <summary>
		/// The target is uses copy-on-write to avoid the requirement to lock for read operations. This lock is
		/// only required for changes to the list, and the changes must be applied atomically by replacing the
		/// whole list.
		/// </summary>
		private static readonly object _TargetsLock = new object ();

		/// <summary>
		/// Out current log node. It is nested inside all its ancestors.
		/// </summary>
		private static Log _Cur = null;

		/// <summary>
		/// Static ctor, adds a console log target which acts as a replacement for Console.Write etc.
		/// </summary>
		static Log ()
		{
			_Cur = new Log (false, false);
			AddTarget (new ConsoleLogTarget ());
			AllowAsynchronousWriting = false;

			var writeThread = new Thread (WriteThread) {
				Name = "Unlog write thread",
				IsBackground = true
			};
			writeThread.Start ();
		}

		public static void AddTarget (ILogTarget t) {
			lock (_TargetsLock) {
				var list = new List<ILogTarget> (_Targets);
				list.Add (t);
				_Targets = list;
			}
		}

		public static void RemoveTarget (ILogTarget t) {
			lock (_TargetsLock) {
				var list = new List<ILogTarget> (_Targets);
				list.Remove (t);
				_Targets = list;
			}
		}

		public static void ClearTargets () {
			lock (_TargetsLock) {
				var list = new List<ILogTarget> ();
				_Targets = list;
			}
		}

		public static bool AllowAsynchronousWriting
		{
			get;
			set;
		}

		/// <summary>
		/// Adds a default file target.
		/// </summary>
		public static void AddDefaultFileTarget ()
		{
			var path = "log " + DateTime.UtcNow.ToString ("yyyy.MM.dd HH.mm.ss") + ".ql";
			AddTarget (new FileLogTarget (path));
		}

		/// <summary>
		/// The parent node of this node.
		/// </summary>
		private readonly Log _Parent;

		/// <summary>
		/// Indentation level. Usually increased by 1 per nesting level.
		/// </summary>
		private readonly int _Indent;

		/// <summary>
		/// A Stash we write to, if this is a stashed log. Stashed logs are never written directly, but the
		/// next time the ancestor history of the current node contains no stashed nodes.
		///
		/// This means that a stashed node A inside a nonstashed node B inside a stashed node C with parent D
		/// will not be written until control goes back to D (instead of B).
		/// </summary>
		private readonly StringBuilder _Stash;

		/// <summary>
		/// Virtual nodes are not indented and disallow manual enter/leave operations. They are exclusively
		/// used in disposable blocks, which forces leaving on dispose.
		///
		/// The enter/leave restriction is somewhat arbitrary and intented to reduce bugs due to mixups of node
		/// levels.
		/// </summary>
		private readonly bool _IsVirtual;

		/// <summary>
		/// Remember if we're at position 0 in a line - important for indentation.
		/// </summary>
		private bool _isAtLineStart = true;

		private Log (bool stashed, bool isVirtual)
		{
			_Parent = _Cur;
			_IsVirtual = isVirtual;

			if (_Parent == null) {
				_Indent = 0;
			}
			else {
				_Indent = _Parent._Indent;
				if (!isVirtual) _Indent++;

				_isAtLineStart = _Parent._isAtLineStart;
				stashed |= _Parent._Stash != null;
			}

			_Stash = stashed ? new StringBuilder () : null;
		}

		/// <summary>
		/// Enter a local log block. These share indentation with their parents and are stashed and kept by
		/// default. Their main purpose is to increase performance, as they allow several lines worth of log
		/// text to be written in a single command.
		///
		/// These blocks should always be used with <c>using (var log = Log.BeginLocal())</c>.
		///
		/// It is not allowed to enter or leave manually while inside a local block.
		/// </summary>
		/// <returns></returns>
		public static LogBlock BeginLocal ()
		{
			_Cur = new Log (true, true);
			return new LogBlock ();
		}

		/// <summary>
		/// Enter a regular log block. This increases indentation by 1.
		///
		/// If the block is stashed, it will not be written until there are no stashed blocks present in the hierarchy.
		/// </summary>
		public static void Enter (bool stashed)
		{
			if (_Cur._IsVirtual) {
				throw new Exception ("Manual enter/leave operations are not allowed inside virtual log blocks.");
			}
			_Cur = new Log (stashed, false);
		}

		/// <summary>
		/// Leaves the current log block.
		///
		/// If the block is stashed, the parameter controls if the block will be discarded.
		/// </summary>
		public static void Leave (bool keep = true)
		{
			if (_Cur._IsVirtual) {
				throw new Exception ("Manual enter/leave operations are not allowed inside virtual log blocks.");
			}
			DoLeave (keep);
		}

		internal static void DoLeave (bool keep)
		{
			if (_Cur._Stash == null && !keep) {
				throw new InvalidOperationException ("Tried to discard a block which was not stashed.");
			}

			// Tell parent to write us if we're stashed and kept
			if (_Cur._Stash != null && keep) {
				var write = _Cur._Stash.ToString ();
				_Cur._Parent.WriteOrStash (write, false);
			}

			// Transfer IsAtLineStart to parent if current content is not discarded
			if (_Cur._Stash == null || keep) {
				_Cur._Parent._isAtLineStart = _Cur._isAtLineStart;
			}

			// Switch to parent
			_Cur = _Cur._Parent;
		}

		private void WriteOrStash (string s, bool indent = true)
		{
			if (indent) {
				var rs = new CuttingStringReader (s);
				var sb = new StringBuilder ();

				while (rs.RemainingLength > 0) {
					// Detect line breaks
					if (rs.Eat ("\r\n") || rs.Eat ("\n")) {
						sb.Append ("\n");
						_isAtLineStart = true;
						continue;
					}

					// Indentation
					if (_isAtLineStart) {
						for (int j = 0; j < _Indent; j++) {
							sb.Append ('\t');
						}
						_isAtLineStart = false;
					}

					var c = rs.Read ();
					sb.Append (c);
				}
				s = sb.ToString ();
			}

			if (_Stash != null) {
				_Stash.Append (s);
			}
			else {
				var task = new WriteTask (s);
				_WriteQueue.Add (task);
				if (!AllowAsynchronousWriting) {
					task.Done.WaitOne ();
				}
			}
		}

		private class WriteTask
		{
			public readonly ManualResetEvent Done = new ManualResetEvent (false);
			public readonly string S;

			public WriteTask (string s)
			{
				S = s;
			}
		}

		private static readonly BlockingCollection<WriteTask> _WriteQueue = new BlockingCollection<WriteTask> ();

		private static void WriteThread ()
		{
			for (; ; ) {
				var task = _WriteQueue.Take ();
				var flush = !_WriteQueue.Any ();
				DoWrite (task.S, flush);
				task.Done.Set ();
			}
		}

		public static int MeasureWriteBacklog ()
		{
			return _WriteQueue.Count;
		}

		private static void DoWrite (string s, bool flush)
		{
			var targets = _Targets; // _Targets may be replaced (copy-on-write), so grab a single reference to be used throughout this method
			var rs = new CuttingStringReader (s);
			var sb = new StringBuilder ();

			while (rs.RemainingLength > 0) {
				// Case "\~ "
				if (rs.Eat ("\\~ ")) {
					foreach (var t in targets) {
						t.Write (sb.ToString ());
						t.ResetColors ();
					}
					sb.Clear ();

					continue;
				}

				// Case "\FX"
				if (rs.Eat ("\\F")) {
					var c = Int32.Parse (new string (rs.Read (), 1), NumberStyles.HexNumber);

					foreach (var t in targets) {
						t.Write (sb.ToString ());
						t.SetForegroundColor ((ConsoleColor) c);
					}
					sb.Clear ();

					continue;
				}

				// Case "\BX"
				if (rs.Eat ("\\B")) {
					var c = Int32.Parse (new string (rs.Read (), 1), NumberStyles.HexNumber);

					foreach (var t in targets) {
						t.Write (sb.ToString ());
						t.SetBackgroundColor ((ConsoleColor) c);
					}
					sb.Clear ();

					continue;
				}

				// Regular
				{
					var c = rs.Read ();
					sb.Append (c);
				}
			}

			foreach (var t in targets) {
				t.Write (sb.ToString ());
			}
			sb.Clear ();

			if (flush) {
				foreach (var t in targets)
				{
					t.Flush ();
				}
			}
		}

		public static void Write<T> (T print)
		{
			var s = print.ToString ();
			_Cur.WriteOrStash (s);
		}

		public static void WriteLine<T> (T print)
		{
			Write (print);
			WriteLine ();
		}

		public static void WriteLine ()
		{
			_Cur.WriteOrStash ("\n");
		}

		public static void ResetColor ()
		{
			_Cur.WriteOrStash ("\\~ ");
		}

		public static ConsoleColor ForegroundColor
		{
			set
			{
				_Cur.WriteOrStash ("\\F" + ((int) value).ToString ("X"));
			}
		}

		public static ConsoleColor BackgroundColor
		{
			set
			{
				_Cur.WriteOrStash ("\\B" + ((int) value).ToString ("X"));
			}
		}
	}
}
