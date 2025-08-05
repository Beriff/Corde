using Corde.Base;
using Corde.Graphics;
using Corde.Input;

namespace Corde.Editor
{
    public struct EditorSettings
    {
        public Color BackgroundColor;
    }

    public class Editor
    {
        private EditorSettings Settings;

        private Buffer2D FrontBuffer;
        private Buffer2D SidebarBuffer;
        private Buffer2D TextBuffer;

        private Vec2I EditorCursorPosition;
        private int PreferredCursorOffset = 0;
        public Vec2I CameraOffset;

        private string SourceText = "";
        private List<List<EditorSymbol>> PreparedLines = [];


        public Editor(EditorSettings settings)
        {
            Console.CursorVisible = false;
            Settings = settings;
            FrontBuffer = new(new(Console.WindowWidth, Console.WindowHeight));
        }

        public void LoadFile(string path)
        {
            SourceText = File.ReadAllText(path);
        }

        public void Update()
        {
            InputHandler.Update();

            // handle cursor movement
            if (InputHandler.KeyState(Keys.ArrowLeft) == InputType.JustReleased)
            {
                if(EditorCursorPosition.X == 0)
                {
                    if(EditorCursorPosition.Y != 0)
                    {
                        EditorCursorPosition.Y--;
                        EditorCursorPosition.X = PreparedLines[EditorCursorPosition.Y].Count - 1;
                    }
                } else
                {
                    EditorCursorPosition.X--;
                }
                PreferredCursorOffset = EditorCursorPosition.X;
            }
            if (InputHandler.KeyState(Keys.ArrowRight) == InputType.JustReleased)
            {
                if (EditorCursorPosition.X == PreparedLines[EditorCursorPosition.Y].Count - 1)
                {
                    if (EditorCursorPosition.Y != PreparedLines.Count - 1)
                    {
                        EditorCursorPosition.X = 0;
                        EditorCursorPosition.Y++;
                    }
                } else
                {
                    EditorCursorPosition.X++;
                }
                PreferredCursorOffset = EditorCursorPosition.X;
            }
            if (InputHandler.KeyState(Keys.ArrowDown) == InputType.JustReleased)
            {
                if(EditorCursorPosition.Y != PreparedLines.Count - 1)
                {
                    EditorCursorPosition.Y++;
                    if (PreparedLines[EditorCursorPosition.Y].Count - 1 < PreferredCursorOffset)
                    {
                        EditorCursorPosition.X = Math.Clamp(PreparedLines[EditorCursorPosition.Y].Count - 1, 0, int.MaxValue);
                    } else
                    {
                        EditorCursorPosition.X = PreferredCursorOffset;
                    }
                }
            }
            if (InputHandler.KeyState(Keys.ArrowUp) == InputType.JustReleased)
            {
                if (EditorCursorPosition.Y != 0)
                {
                    EditorCursorPosition.Y--;
                    if (PreparedLines[EditorCursorPosition.Y].Count - 1 < PreferredCursorOffset)
                    {
                        EditorCursorPosition.X = Math.Clamp(PreparedLines[EditorCursorPosition.Y].Count - 1, 0, int.MaxValue);
                    }
                    else
                    {
                        EditorCursorPosition.X = PreferredCursorOffset;
                    }
                }
            }

            // adjust camera to cursor pos
            if(EditorCursorPosition.Y < CameraOffset.Y) 
            { 
                CameraOffset.Y = EditorCursorPosition.Y; 
            }
            else if(EditorCursorPosition.Y > CameraOffset.Y + TextBuffer.Area.Height) 
            {
                CameraOffset.Y = EditorCursorPosition.Y - TextBuffer.Area.Height;
            }
            if (EditorCursorPosition.X < CameraOffset.X)
            {
                CameraOffset.X = EditorCursorPosition.X;
            }
            else if (EditorCursorPosition.X > CameraOffset.X + TextBuffer.Area.Width)
            {
                CameraOffset.X = EditorCursorPosition.X - TextBuffer.Area.Width;
            }
        }

        private void PrepareLines()
        {
            PreparedLines.Clear();

            int locn_sidebar_width = SourceText.Count(c => c == '\n').ToString().Length + 1;
            SidebarBuffer = new(new(locn_sidebar_width, FrontBuffer.Area.Height));
            SidebarBuffer.Fill(new(' ', Settings.BackgroundColor));
            TextBuffer = new(new(FrontBuffer.Area.Width - locn_sidebar_width, FrontBuffer.Area.Height));
            for(int i = 0; i < SidebarBuffer.Area.Height; i++)
            {
                var n = (i + CameraOffset.Y + 1).ToString();

                if (n.Length < (locn_sidebar_width - 1))
                {
                    n = n.PadLeft(locn_sidebar_width - 1);
                }

                for (int x = locn_sidebar_width - 2; x != 0; x--)
                {
                    SidebarBuffer[x, i] = new(n[x], new(Settings.BackgroundColor.Background, Color.Greyscale(0.6f)));
                }
            }
            
            int newline_count = 0;
            PreparedLines.Add([]);

            for (int x = 0; x < SourceText.Length; x++)
            {
                if(SourceText[x] == '\n')
                {
                    newline_count++;
                    PreparedLines.Add([]);
                    continue;
                }
                if (SourceText[x] == '\r')
                {
                    newline_count++;
                    PreparedLines.Add([]);
                    x++; continue; // skip \r\n
                }

                
                PreparedLines[newline_count].Add(
                    new EditorSymbol(SourceText[x], new(Settings.BackgroundColor.Background, Color.White) )
                );
            }

            EditorSymbol selected;
            if(PreparedLines[EditorCursorPosition.Y].Count > EditorCursorPosition.X)
            {
                selected = PreparedLines[EditorCursorPosition.Y][EditorCursorPosition.X];
                PreparedLines[EditorCursorPosition.Y][EditorCursorPosition.X] = new(selected.Character, new(Color.White, Color.Black));
            } 
            else
            {
                selected = new(' ', new(Color.White));
                PreparedLines[EditorCursorPosition.Y].Add(new(selected.Character, new(Color.White, Color.Black)));
                
            }
            
        }

        private void AssembleView()
        {
            TextBuffer.Fill(new(' ', Settings.BackgroundColor));
            var view = TextBuffer.Area;
            for (int y = CameraOffset.Y; y < CameraOffset.Y + view.Height; y++)
            {
                if (y >= PreparedLines.Count) break;
                for (int x = CameraOffset.X; x < CameraOffset.X + view.Width; x++)
                {
                    if(x >= PreparedLines[y].Count) break;
                    int norm_x = x - CameraOffset.X;
                    int norm_y = y - CameraOffset.Y;
                    TextBuffer[norm_x, norm_y] = PreparedLines[y][x];
                }
            }
        }

        public void Render()
        {
            PrepareLines();
            AssembleView();
            SidebarBuffer.Blit(FrontBuffer, new(1, 1));
            TextBuffer.Blit(FrontBuffer, new(SidebarBuffer.Area.Width + 1, 1));

            FrontBuffer.Render();
        }
    }
}
