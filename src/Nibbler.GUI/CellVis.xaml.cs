using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BeaverUI
{
	public partial class CellVis : UserControl
	{
		public CellVis ()
		{
			InitializeComponent ();
		}

		public void Update ()
		{
			lblSymbol.Text = Data + "  ";
			lblExp.Text = "E: " + Exponent;
			lblHead.Text = Head;
		}

		public string Head { get; set; }

		public string Data { get; set; }

		public ulong Exponent { get; set; }
	}
}
