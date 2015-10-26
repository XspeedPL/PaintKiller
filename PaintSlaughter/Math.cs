using System;

namespace PaintKiller
{
    /// <summary>Unused, but may become useful later</summary>
    internal static class MathHelp
    {
        private const short size = 2048;
        private const float DPI = (float)(Math.PI * 2);
        private const float HPI = (float)(Math.PI / 2);
        private const float step = DPI / size;
        private static float[] sin = new float[size];

        internal static void Init()
        {
            for (int i = 0; i < size; ++i)
                sin[i] = (float)Math.Sin(i * step);
        }

        public static float Sin(float x)
        {
            x %= DPI;
            if (x < 0) x += DPI;
            return sin[(int)(x / step)];
        }

        public static float Cos(float x)
        {
            return Sin(HPI - x);
        }
    }
}
