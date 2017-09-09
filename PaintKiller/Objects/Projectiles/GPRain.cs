using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKilling.Objects.Projectiles
{
    public sealed class GPRain : GProjectile
    {
        public GPRain(Vector2 position, Vector2 direction, GameObj shoot) : base(position, 0, direction, shoot) { }

        public override float GetAcc() { return 999; }

        public override float GetMaxSpd() { return 5; }

        public override short GetMaxHP() { return 105; }

        public override Color GetColor() { return Color.White; }

        public override float GetWeight() { return 0; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            if (HP > 90) DrawCentered(sb, PaintKiller.Inst.GetTex("Arrow"), pos, GetColor() * (HP - 90F / 10), dir, Order.Effect);
            if (HP < 95 && HP > 80) DrawCentered(sb, PaintKiller.Inst.GetTex("ArrowU"), pos, GetColor() * ((HP - 80F) / 15), 0, Order.Midair, (HP - 80) * -0.27F + 4.5F);
        }

        public override void Update()
        {
            base.Update();
            if (HP < 95) spd /= 2;
            if (HP < 70 && HP % 9 == 0)
            {
                for (int i = -2; i < 3; ++i)
                {
                    int offset = Math.Abs(i) * 6 + (HP - 35);
                    Vector2 a = new Vector2(ang.Y * i * 32 - ang.X * offset * 3.5F, -ang.X * i * 32 - ang.Y * offset * 3.5F);
                    PaintKiller.Inst.AddObj(new GPRainA(a + pos, ang, Shooter));
                }
            }
        }

        public override void PreloadContent()
        {
            PaintKiller.Inst.GetTex("Arrow");
            PaintKiller.Inst.GetTex("ArrowU");
        }

        public sealed class GPRainA : GProjectile
        {
            public GPRainA(Vector2 position, Vector2 direction, GameObj shoot) : base(position, 12, direction, shoot)
            {
                dir = (float)(PaintKiller.Rand.NextDouble() * 2 - 1);
            }

            public override float GetAcc() { return 1; }

            public override float GetMaxSpd() { return 1; }

            public override short GetMaxHP() { return 45; }

            public override Color GetColor() { return Color.White; }

            public override float GetWeight() { return 5; }

            public override void Kill() { dead = true; }

            public override void OnDraw(SpriteBatch sb)
            {
                Texture2D arrow = PaintKiller.Inst.GetTex("ArrowD");
                if (HP > 25) DrawCentered(sb, arrow, pos, GetColor() * ((45F - HP) / 20), dir, Order.Midair, HP * 0.075F + 0.125F);
                else if (HP > 5) DrawCentered(sb, arrow, pos, GetColor(), dir, Order.Midair, HP * 0.075F + 0.125F);
                else DrawCentered(sb, arrow, pos, GetColor() * (HP / 5F), dir, Order.EyeCandy, 0.5F);
            }

            public override void Update()
            {
                base.Update();
                if ((HP == 5 || HP == 4 || HP == 6) && frame == 0)
                {
                    GameObj go = FindClosestEnemy(this);
                    if (go != null)
                    {
                        PaintKiller.Inst.AddBlood(this, go);
                        Shooter.OnStrike(go.Hit(15), go);
                        go.Knockback(pos, GetWeight());
                        frame = 1;
                    }
                }
                dir += 0.05F;
            }

            public override void PreloadContent()
            {
                PaintKiller.Inst.GetTex("ArrowD");
            }
        }
    }
}