using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKiller
{
    public sealed class GPArc : GProjectile
    {
        public GPArc(uint id) : base(id) { }

        public GPArc(Vector2 position, Vector2 direction, GPlayer shoot) : base(position, 5, direction, shoot) { }

        public override float GetAcc() { return 0.7F; }

        public override float GetMaxSpd() { return 5; }

        public override short GetMaxHP() { return 80; }

        public override short GetMaxMP() { return 0; }

        public override Color GetColor() { return Color.White; }

        public override float GetWeight() { return 6; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            DrawCentered(sb, PaintKiller.GetTex("Arrow"), pos, hp < 10 ? GetColor() * (hp / 10F) : GetColor(), dir, Order.Effect);
        }

        public override void Update()
        {
            base.Update();
            if (hp == 10)
                foreach (GEnemy ge in PaintKiller.GetEnes())
                    if (ge.IsColliding() && Intersects(ge))
                    {
                        PaintKiller.AddObj(new GEC((pos + ge.pos) / 2, PaintKiller.GetTex("BloodS"), 10));
                        shooter.OnStrike(ge.Hit(7), ge);
                        ge.Knockback(pos, GetWeight());
                        break;
                    }
        }

        public override bool CanBounce() { return true; }
    }
}
