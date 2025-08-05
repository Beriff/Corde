using Corde.Base;
using Corde.Graphics;
using Corde.Input;

using System.Data.Common;

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

        private Vec2I DocumentCursorPosition;

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

        private int CursorRawIndex
        {
            get
            {
                int index = 0;

                // Add lengths of all previous lines plus their line breaks
                for (int i = 0; i < DocumentCursorPosition.Y; i++)
                {
                    index += PreparedLines[i].Count + 2; // +2 for \r\n
                }

                // Add the column position in the current line
                // Make sure we don't go past the end of the line
                int currentLineLength = PreparedLines[DocumentCursorPosition.Y].Count;
                index += Math.Min(DocumentCursorPosition.X, currentLineLength);

                return index;
            }
            
        }

        private void ShiftCursorH(bool left = false)
        {
            if (left)
            {
                if (DocumentCursorPosition.X == 0)
                {
                    if (DocumentCursorPosition.Y != 0)
                    {
                        DocumentCursorPosition.Y--;
                        DocumentCursorPosition.X = PreparedLines[DocumentCursorPosition.Y].Count - 1;
                    }
                }
                else
                {
                    DocumentCursorPosition.X--;
                }
                PreferredCursorOffset = DocumentCursorPosition.X;
            } else
            {
                if (DocumentCursorPosition.X == PreparedLines[DocumentCursorPosition.Y].Count - 1)
                {
                    if (DocumentCursorPosition.Y != PreparedLines.Count - 1)
                    {
                        DocumentCursorPosition.X = 0;
                        DocumentCursorPosition.Y++;
                    }
                }
                else
                {
                    DocumentCursorPosition.X++;
                }
                PreferredCursorOffset = DocumentCursorPosition.X;
            }
        }
        private void ShiftCursorV(bool up = false)
        {
            if (up)
            {
                if (DocumentCursorPosition.Y != 0)
                {
                    DocumentCursorPosition.Y--;
                    if (PreparedLines[DocumentCursorPosition.Y].Count - 1 < PreferredCursorOffset)
                    {
                        DocumentCursorPosition.X = Math.Clamp(PreparedLines[DocumentCursorPosition.Y].Count - 1, 0, int.MaxValue);
                    }
                    else
                    {
                        DocumentCursorPosition.X = PreferredCursorOffset;
                    }
                }
            } else
            {
                if (DocumentCursorPosition.Y != PreparedLines.Count - 1)
                {
                    DocumentCursorPosition.Y++;
                    if (PreparedLines[DocumentCursorPosition.Y].Count - 1 < PreferredCursorOffset)
                    {
                        DocumentCursorPosition.X = Math.Clamp(PreparedLines[DocumentCursorPosition.Y].Count - 1, 0, int.MaxValue);
                    }
                    else
                    {
                        DocumentCursorPosition.X = PreferredCursorOffset;
                    }
                }
            }
        }

        private void HandleUserInsertions()
        {
            foreach(var (key, _) in InputHandler.CurrentKeysState)
            {
                if(
                    InputHandler.KeyState(key) == InputType.JustPressed && 
                    InputHandler.LetterKey(key) != null
                    )
                {
                    string inskey = InputHandler.LetterKey(key).ToString()!;
                    if(!InputHandler.KeyPressed(Keys.Shift)) { inskey = inskey.ToLower(); }

                    SourceText = SourceText.Insert(CursorRawIndex, inskey);
                    ShiftCursorH();
                }
            }
            if(InputHandler.KeyState(Keys.Backspace) == InputType.JustPressed)
            {
                SourceText = SourceText.Remove(CursorRawIndex - 1, 1);
                ShiftCursorH(true);
            }
        }

        public void Update()
        {
            InputHandler.Update();
            HandleUserInsertions();

            // handle cursor movement
            if (InputHandler.KeyState(Keys.ArrowLeft) == InputType.JustReleased)
            {
                ShiftCursorH(true);
            }
            else if (InputHandler.KeyState(Keys.ArrowRight) == InputType.JustReleased)
            {
                ShiftCursorH();
            }
            if (InputHandler.KeyState(Keys.ArrowDown) == InputType.JustReleased)
            {
                ShiftCursorV();
            }
            if (InputHandler.KeyState(Keys.ArrowUp) == InputType.JustReleased)
            {
                ShiftCursorV(true);
            }

            // adjust camera to cursor pos
            if (DocumentCursorPosition.Y < CameraOffset.Y) 
            { 
                CameraOffset.Y--;
            }
            else if(DocumentCursorPosition.Y + 1 > CameraOffset.Y + TextBuffer.Area.Height) 
            {
                CameraOffset.Y++;
            }
            if (DocumentCursorPosition.X < CameraOffset.X)
            {
                CameraOffset.X--;
            }
            else if (DocumentCursorPosition.X + 1 > CameraOffset.X + TextBuffer.Area.Width)
            {
                CameraOffset.X++;
            }
        }

        private void PrepareLines()
        {
            PreparedLines.Clear();

            // create and mark out side bar (line numbers)
            int locn_sidebar_width = SourceText.Count(c => c == '\n').ToString().Length + 1;
            SidebarBuffer = new(new(locn_sidebar_width, FrontBuffer.Area.Height));
            SidebarBuffer.Fill(new(' ', Settings.BackgroundColor));
            
            for(int i = 0; i < SidebarBuffer.Area.Height; i++)
            {
                var n = (i + CameraOffset.Y + 1).ToString();

                if (n.Length < (locn_sidebar_width - 1))
                {
                    n = n.PadLeft(locn_sidebar_width - 1);
                }

                for (int x = locn_sidebar_width - 2; x != 0; x--)
                {
                    var fg = i == DocumentCursorPosition.Y - CameraOffset.Y ? Color.White : Color.Greyscale(0.6f);
                    SidebarBuffer[x, i] = new(n[x], new(Settings.BackgroundColor.Background, fg));
                }
            }

            // create the code text buffer
            TextBuffer = new(new(FrontBuffer.Area.Width - locn_sidebar_width, FrontBuffer.Area.Height));

            // Fill PreparedLines array
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

                var background = Settings.BackgroundColor.Background;
                if (newline_count == DocumentCursorPosition.Y)
                    background = Color.Lighten(background, 1.3f);
                PreparedLines[newline_count].Add(
                    new EditorSymbol(SourceText[x], new(background, Color.White) )
                );
            }

            // Add cursor
            EditorSymbol selected;
            if(PreparedLines[DocumentCursorPosition.Y].Count > DocumentCursorPosition.X)
            {
                selected = PreparedLines[DocumentCursorPosition.Y][DocumentCursorPosition.X];
                PreparedLines[DocumentCursorPosition.Y][DocumentCursorPosition.X] = new(selected.Character, new(Color.White, Color.Black));
            } 
            else
            {
                selected = new(' ', new(Color.White));
                PreparedLines[DocumentCursorPosition.Y].Add(new(selected.Character, new(Color.White, Color.Black)));
                
            }
            
        }

        private void AssembleView()
        {
            TextBuffer.Fill(new(' ', Settings.BackgroundColor));

            for (int i = 0; i < TextBuffer.Area.Width; i++)
            {
                TextBuffer[i, DocumentCursorPosition.Y - CameraOffset.Y] = new(' ', new(Color.Lighten(Settings.BackgroundColor.Background, 1.3f)));
            }

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
