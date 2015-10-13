using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKiller
{
    public sealed class GPRain : GProjectile
    {
        public GPRain(uint id) : base(id) { }

        public GPRain(Vector2 position, Vector2 direction, GPlayer shoot) : base(position, 0, direction, shoot) { }

        public override float GetAcc() { return 999; }

        public override float GetMaxSpd() { return 5; }

        public override short GetMaxHP() { return 105; }

        public override short GetMaxMP() { return 0; }

        public override Color GetColor() { return Color.White; }

        public override float GetWeight() { return 0; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            if (hp > 90) DrawCentered(sb, PaintKiller.GetTex("Arrow"), pos, GetColor() * (hp - 90F / 10), dir, Order.Effect);
            if (hp < 95 && hp > 80) DrawCentered(sb, PaintKiller.GetTex("ArrowU"), pos, GetColor() * ((hp - 80F) / 15), 0, Order.Midair, (hp - 80) * -0.27F + 4.5F);
        }

        public static Vector2 rand(Vector2 v, float radius)
        {
            float x = (float)(PaintKiller.rand.NextDouble() * radius * 2 - radius);
            float rad = radius - Math.Abs(x);
            return v + new Vector2(x, (float)(PaintKiller.rand.NextDouble() * rad * 2 - rad));
        }

        public override void Update()
        {
            base.Update();
            if (hp < 95) spd /= 2;
            if (hp < 70 && hp % 9 == 0)
            {
                for (int i = -2; i < 3; ++i)
                {
                    Vector2 a = new Vector2(ang.Y * i * 32 - ang.X * (hp - 35) * 3.5F, -ang.X * i * 32 - ang.Y * (hp - 35) * 3.5F);
                    PaintKiller.AddObj(new GPRainA(a + pos, ang, shooter));
                }
            }
        }

        public override bool CanBounce() { return false; }

        public sealed class GPRainA : GProjectile
        {
            public GPRainA(uint id) : base(id) { }

            public GPRainA(Vector2 position, Vector2 direction, GPlayer shoot) : base(position, 12, direction, shoot)
            {
                dir = (float)(PaintKiller.rand.NextDouble() * 2 - 1);
            }

            public override float GetAcc() { return 1; }

            public override float GetMaxSpd() { return 1; }

            public override short GetMaxHP() { return 45; }

            public override short GetMaxMP() { return 0; }

            public override Color GetColor() { return Color.White; }

            public override float GetWeight() { return 5; }

            public override void Kill() { dead = true; }

            public override void OnDraw(SpriteBatch sb)
            {
                if (hp > 25) DrawCentered(sb, PaintKiller.GetTex("ArrowD"), pos, GetColor() * ((45F - hp) / 20), dir, Order.Midair, hp * 0.075F + 0.125F);
                else if (hp > 5) DrawCentered(sb, PaintKiller.GetTex("ArrowD"), pos, GetColor(), dir, Order.Midair, hp * 0.075F + 0.125F);
                else DrawCentered(sb, PaintKiller.GetTex("ArrowD"), pos, GetColor() * (hp / 5F), dir, Order.Eyecandy, 0.5F);
            }

            public override void Update()
            {
                base.Update();
                if ((hp == 5 || hp == 4 || hp == 6) && frame == 0)
                    foreach (GEnemy ge in PaintKiller.GetEnes())
                        if (ge.IsColliding() && Intersects(ge))
                        {
                            PaintKiller.AddObj(new GEC((pos + ge.pos) / 2, PaintKiller.GetTex("BloodS"), 10));
                            shooter.OnStrike(ge.Hit(15), ge);
                            ge.Knockback(pos, GetWeight());
                            frame = 1;
                            break;
                        }
                dir += 0.05F;
            }

            public override bool CanBounce() { return false; }
        }
    }
}