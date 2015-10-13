using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKiller
{
    public class GPArrow : GProjectile
    {
        public GPArrow(uint id) : base(id) { }

        public GPArrow(Vector2 position, Vector2 direction, GPlayer shoot) : base(position, 5, direction, shoot) { }

        public override float GetAcc() { return 999; }

        public override float GetMaxSpd() { return 9; }

        public override short GetMaxHP() { return 80; }

        public override short GetMaxMP() { return 0; }

        public override Color GetColor() { return Color.White; }

        public override float GetWeight() { return 4; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            DrawCentered(sb, PaintKiller.GetTex("Arrow"), pos, hp < 10 ? GetColor() * (hp / 10F) : GetColor(), dir, Order.Effect);
        }

        public override void Update()
        {
            base.Update();
            foreach (GEnemy ge in PaintKiller.GetEnes())
                if (ge.IsColliding() && Intersects(ge))
                {
                    PaintKiller.AddObj(new GEC((pos + ge.pos) / 2, PaintKiller.GetTex("BloodS"), 10));
                    shooter.OnStrike(ge.Hit(6), ge);
                    ge.Knockback(pos, GetWeight());
                    Kill();
                    break;
                }
        }

        public override bool CanBounce() { return true; }
    }
}
