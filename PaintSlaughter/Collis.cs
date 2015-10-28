using System;
using Microsoft.Xna.Framework;

namespace PaintKiller
{
    static class Collis
    {
        /// <summary>Applies force to two objects based on their weight and angle between them</summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        public static void Handle(GameObj g1, GameObj g2)
        {
            Vector2 v = g1.pos - g2.pos;
            v.Normalize();
            v *= 2;
            g1.fce += v * g2.GetWeight() / g1.GetWeight();
            g2.fce -= v * g1.GetWeight() / g2.GetWeight();
        }
    }
}
