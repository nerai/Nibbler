using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Unlog.Util
{
	public static class ConsoleColorRgbExtensions
	{
		private static readonly Color[] ConsoleColor_RgbMap = new Color[] {
			Color.FromArgb (000, 000, 000), // Black       #000000
			Color.FromArgb (000, 000, 128), // DarkBlue    #000080
			Color.FromArgb (000, 128, 000), // DarkGreen   #008000
			Color.FromArgb (000, 128, 128), // DarkCyan    #008080
			Color.FromArgb (128, 000, 000), // DarkRed     #800000
			Color.FromArgb (128, 000, 128), // DarkMagenta #800080
			Color.FromArgb (128, 128, 000), // DarkYellow  #808000
			Color.FromArgb (192, 192, 192), // Gray        #C0C0C0
			Color.FromArgb (128, 128, 128), // DarkGray    #808080
			Color.FromArgb (000, 000, 255), // Blue        #0000FF
			Color.FromArgb (000, 255, 000), // Green       #00FF00
			Color.FromArgb (000, 255, 255), // Cyan        #00FFFF
			Color.FromArgb (255, 000, 000), // Red         #FF0000
			Color.FromArgb (255, 000, 255), // Magenta     #FF00FF
			Color.FromArgb (255, 255, 000), // Yellow      #FFFF00
			Color.FromArgb (255, 255, 255), // White       #FFFFFF
		};

		/// <summary>
		/// Convert a ConsoleColor to RGB format.
		/// </summary>
		public static Color ToRGB (this ConsoleColor cc)
		{
			return ConsoleColor_RgbMap[(int) cc];
		}
	}
}
