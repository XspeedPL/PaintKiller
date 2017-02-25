using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKilling.Objects
{
    public sealed class GColli : GameObj
    {
        public GColli(Vector2 position, short radius) : base(position, radius) { }

        public override float GetAcc() { return 0; }

        public override Color GetColor() { return Color.White; }

        public override short GetMaxHP() { return 1; }

        public override short GetMaxMP() { return 0; }

        public override float GetMaxSpd() { return 0; }

        public override float GetWeight() { return 0; }

        public override bool IsColliding() { return false; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            DrawCentered(sb, PaintKiller.Inst.GetTex("GEnemy"), pos, GetColor(), 0, Order.Effect);
        }

        public override void Update() { Kill(); }

        public override void PreloadContent()
        {
            PaintKiller.Inst.GetTex("GEnemy");
        }
    }
}
