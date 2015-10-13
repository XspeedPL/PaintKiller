using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKiller
{
    class GEKnight : GEnemy
    {
        public GEKnight(uint id) : base(id) { }

        public GEKnight(Vector2 position) : base(position, 17) { }

        public override bool IsColliding() { return state < 4; }

        public override void OnDraw(SpriteBatch sb)
        {
            if (state == 4)
            {
                DrawCentered(sb, PaintKiller.GetTex("GEKnightW"), new Vector2(pos.X, pos.Y - 35), Color.White * ((40F - frame) / 20), 2.35F, Order.Eyecandy, 0.75F);
                DrawCentered(sb, PaintKiller.GetTex("GPlayer"), pos, GetColor() * ((25F - frame) / 25), 0, Order.Eyecandy, 1.2F);
            }
            else DrawCentered(sb, PaintKiller.GetTex("GPlayer"), pos, GetColor(), 0, Order.Enemy, 1.2F);
            if ((state == 1 && frame > 10) || state == 2) DrawCentered(sb, PaintKiller.GetTex("GEKnightW"), pos, Color.White, dir + (frame - 16) / 10F, Order.Effect, 0.75F);
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
                    float f2 = Radius + close.Radius + 75;
                    float f1 = Radius + close.Radius + 50;
                    Vector2 v = close.pos - pos;
                    if (dist <= f1 * f1)
                    {
                        v += close.spd * 5;
                        v.Normalize();
                        spd += v * GetAcc();
                        SetState(1, false);
                        UpdateAngle(true);
                    }
                    else if (dist <= f2 * f2)
                    {
                        v += close.spd * 35;
                        v.Normalize();
                        spd += v * GetAcc();
                        SetState(1, false);
                        UpdateAngle(true);
                    }
                    else
                    {
                        v.Normalize();
                        spd += v * GetAcc();
                    }
                    this.ToString();
                }
            }
            else if (state == 1 && ++frame > 4)
            {
                if (frame < 30)
                {
                    spd += ang * GetAcc();
                    fce += ang * GetAcc() / 2;
                }
                if (frame > 12 && frame < 24)
                {
                    GColli gc = new GColli(new Vector2(ang.X * Radius * 1.5F + pos.X, ang.Y * Radius * 1.5F + pos.Y), 14);
                    foreach (GPlayer gp in PaintKiller.GetPlrs())
                        if (gp.IsColliding() && gc.Intersects(gp))
                        {
                            PaintKiller.AddObj(new GEC((gc.pos + gp.pos) / 2, PaintKiller.GetTex("BloodS"), 20));
                            gp.Knockback(gc.pos, 7);
                            gp.Hit(8);
                            state = 2;
                            break;
                        }
                }
                else if (frame > 36) SetState(0);
            }
            else if (state == 2 && ++frame > 36) SetState(0);
            else if (state == 4 && ++frame > 40) dead = true;
        }

        public override float GetAcc() { return 0.35F; }

        public override float GetMaxSpd() { return state == 1 ? 3.5F : 1.9F; }

        public override short GetMaxHP() { return 35; }

        public override short GetMaxMP() { return 0; }

        public override Color GetColor() { return new Color(25, 25, 25); }

        public override float GetWeight() { return 5; }

        public override void Kill() { if (state != 4) SetState(4); }
    }
}
