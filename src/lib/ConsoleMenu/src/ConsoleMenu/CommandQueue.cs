﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleMenu
{
	/// <summary>
	/// Provides global I/O functions in the context of CMenu.
	/// </summary>
	public class CommandQueue
	{
		private class Frame
		{
			public IEnumerator<string> E;

			public Frame (IEnumerable<string> source)
			{
				E = source.GetEnumerator ();
			}
		}

		private readonly Stack<Frame> _Frames = new Stack<Frame> ();

		private readonly ManualResetEvent _InputAvailable = new ManualResetEvent (false);

		/// <summary>
		/// If no input is queued already and passive mode is enabled, QueryInput will
		/// block until input is available. If no input is queued already and passive mode
		/// is disabled, QueryInput will prompt the user for input.
		///
		/// PassiveMode is disabled by default.
		/// </summary>
		public bool PassiveMode
		{
			get;
			set;
		}

		/// <summary>
		/// Returns the next available line of input.
		///
		/// Empty input (whitespace only) will always be ignored.
		///
		/// If queued input is available, its next line will be returned.
		///
		/// If no queued input is available, the call will block. Depending on PassiveMode
		/// either the user will be prompted for input, or the method will wait until input
		/// was added to the queue.
		/// </summary>
		/// <param name="prompt">
		/// Prompt string, or null if no prompt string should be displayed.
		///
		/// In passive mode, the prompt will never be displayed.
		/// </param>
		public string QueryInput (string prompt)
		{
			for (;;) {
				var input = TryGetQueuedInput ();

				if (input == null) {
					_InputAvailable.Reset ();
					if (!PassiveMode) {
						if (prompt != null) {
							Console.Write (prompt + " ");
						}
						input = Console.ReadLine ();
					}
				}

				if (input == null && PassiveMode) {
					_InputAvailable.WaitOne ();
				}

				if (!string.IsNullOrWhiteSpace (input)) {
					return input;
				}
			}
		}

		/// <summary>
		/// Returns the next queued line of input. The line is then removed from the input queue.
		///
		/// If no queued input is available, the call will not block, but return null.
		/// </summary>
		public string TryGetQueuedInput ()
		{
			lock (_Frames) {
				while (_Frames.Any ()) {
					var f = _Frames.Peek ();
					if (!f.E.MoveNext ()) {
						_Frames.Pop ();
						continue;
					}
					var input = f.E.Current;
					return input;
				}
			}
			return null;
		}

		/// <summary>
		/// Adds a new input source on top of the input stack.
		///
		/// This source will be used until it is exhausted, then the previous source will
		/// be used in the same manner.
		/// </summary>
		public void AddInput (IEnumerable<string> source)
		{
			if (source == null) {
				throw new ArgumentNullException ("source");
			}

			lock (_Frames) {
				_Frames.Push (new Frame (source));
				_InputAvailable.Set ();
			}
		}

		/// <summary>
		/// Puts a single line of input on top of the stack.
		/// </summary>
		public void ImmediateInput (string source)
		{
			if (source == null) {
				throw new ArgumentNullException ("source");
			}

			AddInput (new string[] { source });
		}

		/// <summary>
		/// Return true iff this queue contains no frames of input.
		/// </summary>
		/// <returns></returns>
		public bool IsEmpty ()
		{
			lock (_Frames) {
				return _Frames.Count == 0;
			}
		}
	}
}
