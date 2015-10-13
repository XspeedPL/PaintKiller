using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKiller
{
    public class GEC : GameObj
    {
        // 0 - scale / 2
        // 1 - scale / 3
        // 2 - scale * 2
        // 3 - scale * 3

        internal Texture2D gfx;
        public bool[] prefs = new bool[8];

        public GEC(uint id) : base(id) { }

        public GEC(Vector2 position, Texture2D tex, byte time) : base(position, 0)
        {
            state = frame = time;
            gfx = tex;
        }

        public override float GetAcc() { return 0; }

        public override Color GetColor() { return Color.White; }

        public override short GetMaxHP() { return 1; }

        public override short GetMaxMP() { return 0; }

        public override float GetMaxSpd() { return 0; }

        public override float GetWeight() { return 999999; }

        public override bool IsColliding() { return false; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            float s = 1;
            if (prefs[0]) s /= 2;
            if (prefs[1]) s /= 3;
            if (prefs[2]) s *= 2;
            if (prefs[3]) s *= 3;
            DrawCentered(sb, gfx, pos, GetColor() * ((float)frame / state), dir, Order.Effect, s);
        }

        public override void Update() { if (--frame < 1) Kill(); }

        public byte GetPrefs()
        {
            byte ret = 0;
            for (byte i = 7; i < 255; --i)
            {
                ret <<= 1;
                if (prefs[i]) ++ret;
            }
            return ret;
        }

        public void SetPrefs(byte data)
        {
            for (byte i = 0; i < 8; ++i)
            {
                prefs[i] = data % 2 == 1;
                data /= 2;
            }
        }
    }
}
