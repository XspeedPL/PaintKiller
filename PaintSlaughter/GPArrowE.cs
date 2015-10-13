using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKiller
{
    public class GPArrowE : GProjectile
    {
        public GPArrowE(uint id) : base(id) { }

        public GPArrowE(Vector2 position, Vector2 direction) : base(position, 4, direction, null) { }

        public override float GetAcc() { return 999; }

        public override float GetMaxSpd() { return 6; }

        public override short GetMaxHP() { return 65; }

        public override short GetMaxMP() { return 0; }

        public override Color GetColor() { return Color.White; }

        public override float GetWeight() { return 3; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            DrawCentered(sb, PaintKiller.GetTex("Arrow"), pos, hp < 10 ? GetColor() * (hp / 10F) : GetColor(), dir, Order.Effect, 0.75F);
        }

        public override void Update()
        {
            base.Update();
            foreach (GPlayer gp in PaintKiller.GetPlrs())
                if (gp.IsColliding() && Intersects(gp))
                {
                    PaintKiller.AddObj(new GEC((pos + gp.pos) / 2, PaintKiller.GetTex("BloodS"), 10));
                    gp.Hit(4);
                    gp.Knockback(pos, GetWeight());
                    Kill();
                    break;
                }
        }

        public override bool CanBounce() { return false; }
    }
}
