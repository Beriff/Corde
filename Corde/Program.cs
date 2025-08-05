using Corde.Editor;
using Corde.Graphics;
using Corde.Input;

Editor editor = new(new() { BackgroundColor = new((31,31,31)) });
editor.LoadFile(@"C:\Users\Maxim\source\repos\Corde\Corde\Editor\Editor.cs");

while (!InputHandler.KeyPressed(Keys.Esc))
{
    editor.Render();
    editor.Update();
}