using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corde.Graphics
{
    using ColorPack = (byte R, byte G, byte B);
    public struct Color
    {
        public static string Reset { get => "\u001b[0m"; }
		public static Color OnlyBg(ColorPack bg) => new(bg, (0, 0, 0));
		public static Color OnlyFg(ColorPack fg) => new((0, 0, 0), fg);

		public ColorPack Background { get; set; }
		public ColorPack Foreground { get; set; }

		public override string ToString() => 
			$"\x1b[38;2;{Foreground.R};{Foreground.G};{Foreground.B}m" +
			$"\x1b[48;2;{Background.R};{Background.G};{Background.B}m";

		public Color(ColorPack bg, ColorPack fg) 
		{
			Background = bg;
			Foreground = fg;
		}
		public Color(ColorPack col)
		{
			Background = Foreground = col;
		}

		public static ColorPack Contrast(ColorPack a)
		{
			return ((byte,byte,byte))((255 - a.R) / 2, (255 - a.G) / 2, (255 - a.B) / 2);
		}
		public static ColorPack Lighten(ColorPack c, float s)
		{
            return ((byte, byte, byte))(c.R * s, c.G * s, c.B * s);
        }
		public static ColorPack Greyscale(float s)
		{
			return ((byte,byte,byte))(255 * s, 255 * s, 255 * s);
		}

        // Static helper methods for common colors
        public static ColorPack Red => (255, 0, 0);
        public static ColorPack Green => (0, 255, 0);
        public static ColorPack Blue => (0, 0, 255);
        public static ColorPack White => (255, 255, 255);
        public static ColorPack Black => (0, 0, 0);
        public static ColorPack Yellow => (255, 255, 0);
        public static ColorPack Cyan => (0, 255, 255);
        public static ColorPack Magenta => (255, 0, 255);

    }
}
