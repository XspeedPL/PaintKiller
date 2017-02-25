using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaintKilling.Net;

namespace PaintKilling.Objects
{
    /// <summary>Single texture eye candy game object class</summary>
    public class GEC : GameObj
    {
        internal Texture2D gfx;
        public bool[] prefs = new bool[8];

        public GEC(Vector2 position, string tex, byte time) : base(position, 0)
        {
            MP = frame = time;
            gfx = PaintKiller.Inst.GetTex(tex);
        }

        public override float GetAcc() { return 0; }

        public override Color GetColor() { return Color.White; }

        public override short GetMaxHP() { return 1; }

        public sealed override short GetMaxMP() { return short.MaxValue; }

        public override float GetMaxSpd() { return 0; }

        public override float GetWeight() { return float.MaxValue; }

        public sealed override bool IsColliding() { return false; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            float s = 1;
            if (prefs[0]) s /= 2;
            if (prefs[1]) s /= 3;
            if (prefs[2]) s *= 2;
            if (prefs[3]) s *= 3;
            DrawCentered(sb, gfx, pos, GetColor() * (frame / (float)MP), dir, Order.Effect, s);
        }

        public override void Update() { if (--frame < 1) Kill(); }

        public override void PreloadContent() { }

        public override void CloneSpecial(GameObj src)
        {
            prefs = new bool[8];
            ((GEC)src).prefs.CopyTo(prefs, 0);
        }

        public override void ReadSpecial(BinaryReader br)
        {
            gfx = PaintKiller.Inst.GetTex(br.ReadString());
            prefs = br.ReadByte().Unpack();
        }

        public override void WriteSpecial(BinaryWriter bw)
        {
            bw.Write(gfx.Name);
            bw.Write(prefs.Pack());
        }
    }
}
