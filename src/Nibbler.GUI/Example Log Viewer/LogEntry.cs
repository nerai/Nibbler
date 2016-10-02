﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace BeaverUI
{
	public class PropertyChangedBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged (string propertyName)
		{
			Application.Current.Dispatcher.BeginInvoke ((Action) (() => {
				PropertyChangedEventHandler handler = PropertyChanged;
				if (handler != null) handler (this, new PropertyChangedEventArgs (propertyName));
			}));
		}
	}

	public class LogEntry : PropertyChangedBase
	{
		public DateTime DateTime { get; set; }

		public int Index { get; set; }

		public string Message { get; set; }
	}

	public class CollapsibleLogEntry : LogEntry
	{
		public List<LogEntry> Contents { get; set; }
	}
}
