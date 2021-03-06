﻿using SwinGameSDK;

namespace Arcadia
{
    /// <summary>
    /// A class that contains information that can be used throughout the application
    /// </summary>
    public static class Globals
    {
        public static Color TextColor = Color.White;
        public static Color Background = SwinGame.RGBColor(10, 10, 10);
        public static Color ScrollBarTrack = SwinGame.RGBColor(70, 70, 70);
        public static Color ScrollBarThumb = SwinGame.RGBColor(140, 140, 140);
        public static Color Marquee = SwinGame.RGBColor(20, 20, 20);
        public static int LogoMaxSize = 0;
    }
}
