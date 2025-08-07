using ConFlag;

using Corde.Editor;
using Corde.Graphics;
using Corde.Input;

namespace Corde
{
    public class Program
    {
        static void Main(string[] args)
        {
            Arguments arguments = new(args);
            if (arguments.Commands.Count > 0)
            {
                string path;
                try
                {
                    path = Path.GetFullPath(arguments.Commands[0]);
                } catch (ArgumentException)
                {
                    Console.WriteLine($"Invalid path: {arguments.Commands[0]}");
                    path = @"C:\";
                }

                Editor.Editor editor = new(new() { BackgroundColor = new((31, 31, 31)), BottomBarColor = new((66, 66, 66)) });
                editor.LoadFile(path);

                while (!InputHandler.KeyPressed(Keys.Esc))
                {
                    editor.Render();
                    editor.Update();
                }
            }

            
        }
    }
}

