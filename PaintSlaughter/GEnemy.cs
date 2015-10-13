using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKiller
{
    public class GEnemy : GameObj
    {
        public GEnemy(uint id) : base(id) { }

        public GEnemy(Vector2 position, short radius) : base(position, radius) { }

        public GEnemy(Vector2 position) : base(position, 12) { }

        public override bool IsColliding() { return state < 4; }

        public override void OnDraw(SpriteBatch sb)
        {
            if (state == 4)
            {
                DrawCentered(sb, PaintKiller.GetTex("GEnemyD1"), pos, GetColor() * ((40F - frame) / 40), 0, Order.Eyecandy);
                DrawCentered(sb, PaintKiller.GetTex("GEnemyD2"), pos, Color.White * ((15F - frame) / 15), 0, Order.Eyecandy);
            }
            else DrawCentered(sb, PaintKiller.GetTex("GEnemy"), pos, GetColor(), 0, Order.Enemy);
            if ((state == 1 && frame > 10) || state == 2) DrawCentered(sb, PaintKiller.GetTex("GEnemyW"), pos, Color.White, dir + (frame - 23F) / 15, Order.Effect);
            else if (state == 5 && frame > 10) DrawCentered(sb, PaintKiller.GetTex("GEnemyW"), pos, Color.White, dir + (frame - 23F) / 15, Order.Effect);
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
                    float f = Radius + close.Radius + 9;
                    Vector2 v = close.pos - pos + close.spd * 5;
                    v.Normalize();
                    if (dist <= f * f)
                    {
                        spd += v * GetAcc() * 2.5F;
                        SetState(PaintKiller.rand.Next(5) == 0 ? (byte)5 : (byte)1, false);
                        UpdateAngle(true);
                    }
                    else spd += v * GetAcc();
                }
            }
            else if ((state == 5 || state == 1) && ++frame > 12)
            {
                if (state == 5) spd += ang * GetAcc();
                if (frame < 24)
                {
                    GColli gc = new GColli(new Vector2(ang.X * 25 + pos.X, ang.Y * 25 + pos.Y), 10);
                    foreach (GPlayer gp in PaintKiller.GetPlrs())
                        if (gp.IsColliding() && gc.Intersects(gp))
                        {
                            PaintKiller.AddObj(new GEC((gc.pos + gp.pos) / 2, PaintKiller.GetTex("BloodS"), 20));
                            gp.Knockback(gc.pos, 2);
                            gp.Hit(4);
                            state = 2;
                            break;
                        }
                }
                else if (frame > 36) SetState(0);
            }
            else if (state == 2 && ++frame > 36) SetState(0);
            else if (state == 4 && ++frame > 40) dead = true;
        }

        public override float GetAcc() { return 0.55F; }

        public override float GetMaxSpd() { return state == 5 ? 2.5F : 1.75F; }

        public override short GetMaxHP() { return 15; }

        public override short GetMaxMP() { return 0; }

        public override Color GetColor() { return Color.GreenYellow; }

        public override float GetWeight() { return 0.95F; }

        public override void Kill() { if (state != 4) SetState(4); }
    }
}
