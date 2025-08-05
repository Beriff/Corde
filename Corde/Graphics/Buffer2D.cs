using Corde.Base;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corde.Graphics
{
    public class Buffer2D
    {
        public Vec2I Size { get; private set; }
        public EditorSymbol[,] Grid { get; private set; }
        public Rect Area { get => new(Vec2I.Zero, Size); }
        private int LinearSize { get => Size.X * Size.Y + Size.X - 1; }

        public void Render()
        {
            StringBuilder str = new(LinearSize);
            for (int y = 0; y < Size.Y; y++)
            {
                for (int x = 0; x < Size.X; x++)
                {
                    str.Append(Grid[x, y].Transparent ? ' ' : Grid[x, y]);
                }
                str.Append('\n');
            }
            Console.Write(str.ToString());
            Console.SetCursorPosition(0, 0);
        }

        public void Blit(Buffer2D accepting, Vec2I pos)
        {
            for (int y = 0; y < Size.Y; y++)
            {
                for (int x = 0; x < Size.X; x++)
                {
                    if (
                        x + pos.X <= accepting.Size.X &&
                        y + pos.Y <= accepting.Size.Y &&
                        !this[x, y].Transparent
                        )
                    {
                        accepting[x + pos.X - 1, y + pos.Y - 1] = this[x, y];
                    }
                }
            }
        }

        public void BlitSymbString(EditorSymbol[] str, Vec2I pos)
        {
            for (int i = 0; i < str.Length; i++)
            {
                this[pos.X + i, pos.Y] = str[i];
            }
        }

        public void Fill(EditorSymbol s)
        {
            for (int x = 0; x < Size.X; x++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    this[x, y] = s;
                }
            }
        }

        public void Clear()
        {
            for (int x = 0; x < Size.X; x++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    this[x, y] = new EditorSymbol(' ', new ((0, 0, 0),(0,0,0)) );
                }
            }
        }

        public EditorSymbol this[int x, int y]
        {
            get => Grid[x, y];
            set
            {
                if (x >= 0 && x < Size.X && y >= 0 && y < Size.Y)
                {
                    Grid[x, y] = value;
                }
            }
        }

        public Buffer2D(Vec2I size)
        {
            Size = size;
            Grid = new EditorSymbol[size.X, size.Y];
            Fill(new(' ', new(Color.White, Color.White) ) { Transparent = true });
        }
    }
}
