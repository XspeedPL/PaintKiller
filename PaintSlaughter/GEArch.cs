using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKiller
{
    class GEArch : GEnemy
    {
        public GEArch(uint id) : base(id) { }

        public GEArch(Vector2 position) : base(position, 12) { }

        public override bool IsColliding() { return state < 4; }

        public override void OnDraw(SpriteBatch sb)
        {
            if (state == 4)
            {
                DrawCentered(sb, PaintKiller.GetTex("GEnemyD1"), pos, GetColor() * ((40F - frame) / 40), 0, Order.Eyecandy);
                DrawCentered(sb, PaintKiller.GetTex("GEnemyD2"), pos, Color.White * ((15F - frame) / 15), 0, Order.Eyecandy);
            }
            else DrawCentered(sb, PaintKiller.GetTex("GEnemy"), pos, GetColor(), 0, Order.Enemy);
            if (state == 1 || state == 2) DrawCentered(sb, PaintKiller.GetTex(frame < 15 ? "GPArchW1" : "GPArchW2"), pos, Color.White, dir, Order.Effect, 0.75F);
        }

        public override void Update()
        {
            if (state == 0)
            {
                GPlayer close = null;
                float dist = 0;
                foreach (GPlayer gp in PaintKiller.GetPlrs())
                    if (close == null || DistanceSq(gp) < dist)
                    {
                        close = gp;
                        dist = DistanceSq(close);
                    }
                if (close != null)
                {
                    float f2 = Radius + close.Radius + 200;
                    float f1 = Radius + close.Radius + 50;
                    Vector2 v = close.pos - pos;
                    if (dist <= f1 * f1)
                    {
                        v.Normalize();
                        spd = v * GetAcc();
                        SetState(1, false);
                        UpdateAngle(true);
                    }
                    else if (dist <= f2 * f2)
                    {
                        v += close.spd * 50;
                        v.Normalize();
                        spd = v * GetAcc();
                        SetState(1, false);
                        UpdateAngle(true);
                    }
                    else
                    {
                        v.Normalize();
                        spd += v * GetAcc();
                    }
                }
            }
            else if (state == 1 || state == 2)
            {
                if (state == 1 && frame > 15)
                {
                    PaintKiller.AddObj(new GPArrowE(pos + ang * 3, ang));
                    state = 2;
                }
                if (++frame > 35) SetState(0);
            }
            else if (state == 4 && ++frame > 40) dead = true;
        }

        public override float GetAcc() { return 0.5F; }

        public override float GetMaxSpd() { return 1.8F; }

        public override short GetMaxHP() { return 12; }

        public override short GetMaxMP() { return 0; }

        public override Color GetColor() { return Color.Orange; }

        public override float GetWeight() { return 0.85F; }

        public override void Kill() { if (state != 4) SetState(4); }
    }
}
