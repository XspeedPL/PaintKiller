using System;

namespace PaintKilling.Net
{
    /// <summary>A class for storing player controls state</summary>
    public sealed class Controls
    {
        public const int KEY_RETURN = 6, KEY_ESC = 7;

        public readonly float X, Y;
        public readonly bool[] Keys;

        public Controls Prev { get; private set; }

        public bool IsFirstPress(int key) { return Keys[key] && !Prev.Keys[key]; }

        public int FirstX { get { return Math.Abs(X) >= 0.5F && Math.Abs(Prev.X) < 0.5F ? Math.Sign(X) : 0; } }

        public int FirstY { get { return Math.Abs(Y) >= 0.5F && Math.Abs(Prev.Y) < 0.5F ? Math.Sign(Y) : 0; } }

        public void SetPrev(Controls old) { Prev = old; }

        public Controls(float x, float y, bool[] keys) { X = x; Y = y; Keys = keys; }
    }
}
