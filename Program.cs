using System;

namespace Cub
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new CubGame())
                game.Run();
        }
    }
}
