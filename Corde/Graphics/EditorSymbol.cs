using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corde.Graphics
{
    public struct EditorSymbol
    {
        public char Character { get; set; }
        public Color Color { get; set; }
        public bool Transparent { get; set; } = false;

        public override string ToString() => $"{Color}{Character}{Color.Reset}";

        public EditorSymbol(char character, Color color)
        {
            Character = character;
            Color = color;
        }

        public static EditorSymbol[] Text(string text, Color? color = null)
        {
            color ??= new ((255, 255, 255));
            var symbols = new EditorSymbol[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                symbols[i] = new(text[i], (Color)color);
            }
            return symbols;
        }

        public static EditorSymbol[] Text(string text, Func<int, Color> f)
        {
            var symbols = new EditorSymbol[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                symbols[i] = new(text[i], f(i));
            }
            return symbols;
        }

        public static string StringFromText(EditorSymbol[] text)
        {
            var str = new StringBuilder();
            foreach (EditorSymbol symbol in text) { str.Append(symbol.Character); }
            return str.ToString();
        }
    }
}
