using System;

namespace ProjectProphecy
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = Game1.Singleton)
                game.Run();
        }
    }
}
