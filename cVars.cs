using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Cub
{
    public static class cVars
    {
        public const int SCREEN_WIDTH = 1280;
        public const int SCREEN_HEIGHT = 720;

        //public const int GAME_WIDTH = 360;
        //public const int GAME_HEIGHT = 640;

        public static string VERSION => Assembly.GetEntryAssembly().GetName().Version.ToString();
    }
}
