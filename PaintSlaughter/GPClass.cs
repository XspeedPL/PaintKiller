using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKiller
{
    class GPOrc : GPlayer
    {
        public GPOrc(uint id) : base(id) { }

        public GPOrc(Vector2 position) : base(position) { }

        public override float GetAcc() { return 0.75F; }

        public override float GetMaxSpd() { return 3.25F; }

        public override short GetMaxHP() { return 95; }

        public override short GetMaxMP() { return 75; }

        public override Color GetColor() { return Color.DarkGreen; }

        public override float GetWeight() { return 4; }

        public override string GetClassName() { return "Orc"; }

        public override byte GetClassID() { return 0; }

        public override bool IsColliding() { return state != 5 && state != 7; }

        public override void OnStrike(short dmg, GEnemy ge)
        {
            score += dmg;
            hp += (short)(dmg / 5);
        }

        public override void Update()
        {
            base.Update();
            if (state == 1 || state == 2)
            {
                if (state == 1 && frame > 8)
                {
                    GColli gc = new GColli(new Vector2(ang.X * 25 + pos.X, ang.Y * 25 + pos.Y), 20);
                    foreach (GameObj ge in PaintKiller.GetObjs())
                        if (ge != tag && ge is GEnemy && ge.IsColliding() && gc.Intersects(ge))
                        {
                            PaintKiller.AddObj(new GEC((gc.pos + ge.pos) / 2, PaintKiller.GetTex("BloodS"), 10));
                            OnStrike(ge.Hit(10), (GEnemy)ge);
                            ge.Knockback(gc.pos, 5);
                            if (tag != null) state = 2;
                            else tag = ge;
                            break;
                        }
                        else if (ge is GPArrowE && gc.Intersects(ge)) ge.Kill();
                }
                if (++frame > 24) SetState(0);
            }
            else if (state == 5)
            {
                if (frame > 8 && frame < 40)
                {
                    if (tag == null) tag = new List<GameObj>();
                    GColli gc = new GColli(pos, 36);
                    foreach (GameObj ge in PaintKiller.GetObjs())
                        if (ge is GEnemy && ge.IsColliding() && gc.Intersects(ge))
                        {
                            PaintKiller.AddObj(new GEC((gc.pos + ge.pos) / 2, PaintKiller.GetTex("BloodS"), 10));
                            if (!((List<GameObj>)tag).Contains(ge))
                            {
                                ((List<GameObj>)tag).Add(ge);
                                OnStrike(ge.Hit(14), (GEnemy)ge);
                            }
                            ge.Knockback(gc.pos, 12);
                            ((List<GameObj>)tag).Add(ge);
                            break;
                        }
                        else if (ge is GPArrowE && gc.Intersects(ge)) ge.Kill();
                }
                if (++frame > 48) SetState(0);
            }
            else if (state == 7)
            {
                if (tag == null) tag = new List<GameObj>();
                spd += ang * GetAcc() * 2;
                fce += ang * GetAcc();
                if (++frame % 3 == 0) PaintKiller.AddObj(new GEC(pos, PaintKiller.GetTex("GPlayer"), 15));
                GColli gc = new GColli(new Vector2(ang.X * 25 + pos.X, ang.Y * 25 + pos.Y), 20);
                foreach (GameObj ge in PaintKiller.GetObjs())
                    if (ge != tag && ge is GEnemy && ge.IsColliding() && gc.Intersects(ge))
                    {
                        PaintKiller.AddObj(new GEC((gc.pos + ge.pos) / 2, PaintKiller.GetTex("BloodS"), 10));
                        if (!((List<GameObj>)tag).Contains(ge))
                        {
                            ((List<GameObj>)tag).Add(ge);
                            OnStrike(ge.Hit(18), (GEnemy)ge);
                        }
                        ge.Knockback(gc.pos, 7);
                        break;
                    }
                    else if (ge is GPArrowE && gc.Intersects(ge)) ge.Kill();
                if (frame > 40) SetState(0);
            }
        }

        public override void OnDraw(SpriteBatch sb)
        {
            base.OnDraw(sb);
            if (state == 1 || state == 2)
            {
                if (frame < 8) DrawCentered(sb, PaintKiller.GetTex("GPOrcW1"), pos, Color.White, dir + (frame - 4) / 8F, Order.Effect);
                else if (frame < 16) DrawCentered(sb, PaintKiller.GetTex("GPOrcW2"), pos, Color.White, dir + (frame - 12) / 8F, Order.Effect);
                else DrawCentered(sb, PaintKiller.GetTex("GPOrcW3"), pos, Color.White, dir + (frame - 20) / 8F, Order.Effect);
            }
            else if (state == 5)
            {
                if (frame > 40) DrawCentered(sb, PaintKiller.GetTex("GPOrcW1"), pos, Color.White, dir + 1.8F, Order.Effect);
                else if (frame < 8) DrawCentered(sb, PaintKiller.GetTex("GPOrcW1"), pos, Color.White, dir, Order.Effect);
                else DrawCentered(sb, PaintKiller.GetTex("GPOrcS" + (frame / 4 % 2 == 0 ? "1" : "2")), pos, Color.White, (float)PaintKiller.rand.NextDouble() / 2, Order.Effect);
            }
            else if (state == 7)
            {
                Texture2D t = PaintKiller.GetTex("GPOrcW2");
                DrawCentered(sb, t, pos, Color.White, dir + (frame - 20) / 10F, Order.Effect);
                sb.Draw(t, pos, null, Color.White, dir - (frame - 20) / 10F, new Vector2(t.Width / 2, t.Height / 2), 1, SpriteEffects.FlipVertically, (float)Order.Effect / (float)Order.MAX);
            }
         }
    }

    class GPArch : GPlayer
    {
        public GPArch(uint id) : base(id) { }

        public GPArch(Vector2 position) : base(position) { }

        public override float GetAcc() { return 0.9F; }

        public override float GetMaxSpd() { return 3.75F; }

        public override short GetMaxHP() { return 80; }

        public override short GetMaxMP() { return 80; }

        public override Color GetColor() { return Color.Yellow; }

        public override float GetWeight() { return 2; }

        public override string GetClassName() { return "Archer"; }

        public override byte GetClassID() { return 2; }

        public override bool IsColliding() { return true; }

        public override void OnStrike(short dmg, GEnemy ge)
        {
            score += dmg;
        }

        public override void Update()
        {
            base.Update();
            if (state == 1 || state == 2)
            {
                if (state == 1 && frame > 5)
                {
                    PaintKiller.AddObj(new GPArrow(pos + ang * 5, ang, this));
                    state = 2;
                }
                if (++frame > 18) SetState(0);
            }
            else if (state == 5 || state == 6)
            {
                if (state == 5 && frame > 15)
                {
                    PaintKiller.AddObj(new GPPiercing(pos + ang * 5, new Vector2((float)Math.Cos(dir - 0.3F), (float)Math.Sin(dir - 0.3F)), this));
                    PaintKiller.AddObj(new GPPiercing(pos + ang * 5, ang, this));
                    PaintKiller.AddObj(new GPPiercing(pos + ang * 5, new Vector2((float)Math.Cos(dir + 0.3F), (float)Math.Sin(dir + 0.3F)), this));
                    state = 6;
                }
                if (++frame > 38) SetState(0);
            }
            else if (state == 7 || state == 8)
            {
                if (state == 7 && frame > 25)
                {
                    PaintKiller.AddObj(new GPRain(pos + ang * 5, ang, this));
                    state = 8;
                }
                if (++frame > 58) SetState(0);
            }
        }

        public override void OnDraw(SpriteBatch sb)
        {
            base.OnDraw(sb);
            if (state == 1)
            {
                DrawCentered(sb, PaintKiller.GetTex("GPArchW1"), pos, Color.White, dir, Order.Effect);
                if (frame > 3) DrawCentered(sb, PaintKiller.GetTex("Arrow"), pos + ang * 5, Color.White, dir, Order.Effect);
            }
            else if (state == 5)
            {
                DrawCentered(sb, PaintKiller.GetTex("GPArchW1"), pos, Color.White, dir, Order.Effect);
                if (frame > 5)
                {
                    DrawCentered(sb, PaintKiller.GetTex("Arrow"), pos + ang * 5, Color.White, dir, Order.Effect, 1.15F);
                    if (frame > 10)
                    {
                        DrawCentered(sb, PaintKiller.GetTex("Arrow"), pos + ang * 5, Color.White, dir - 0.3F, Order.Effect, 1.15F);
                        DrawCentered(sb, PaintKiller.GetTex("Arrow"), pos + ang * 5, Color.White, dir + 0.3F, Order.Effect, 1.15F);
                    }
                }
            }
            else if (state == 2 || state == 6 || state == 8) DrawCentered(sb, PaintKiller.GetTex("GPArchW2"), pos, Color.White, dir, Order.Effect);
            else if (state == 7)
            {
                DrawCentered(sb, PaintKiller.GetTex("GPArchW1"), pos, Color.White, dir, Order.Effect);
                if (frame > 5) DrawCentered(sb, PaintKiller.GetTex("Arrow"), pos + ang * 5, Color.White, dir, Order.Effect);
            }
        }
    }

    class GPMage : GPlayer
    {
        public GPMage(uint id) : base(id) { }

        public GPMage(Vector2 position) : base(position) { }

        public override float GetAcc() { return 0.6F; }

        public override float GetMaxSpd() { return 3F; }

        public override short GetMaxHP() { return 75; }

        public override short GetMaxMP() { return 95; }

        public override Color GetColor() { return Color.Blue; }

        public override string GetClassName() { return "Mage"; }

        public override byte GetClassID() { return 1; }

        public override float GetWeight() { return 2.5F; }

        public override bool IsColliding() { return true; }

        public override void OnStrike(short dmg, GEnemy ge)
        {
            score += dmg;
            mp += (short)(dmg / 4);
        }

        public override void OnDraw(SpriteBatch sb)
        {
            base.OnDraw(sb);
            if (state == 1 || state == 2 || state == 5 || state == 6 || state == 7 || state == 8)
                DrawCentered(sb, PaintKiller.GetTex("GPMageW"), pos, Color.White, dir + 0.2F, Order.Effect);
        }

        public override void Update()
        {
            base.Update();
            if (state == 1 || state == 2)
            {
                if (state == 1 && frame > 7)
                {
                    PaintKiller.AddObj(new GPWave(pos + ang * Radius, ang, this));
                    state = 2;
                }
                if (++frame > 30) SetState(0);
            }
            else if (state == 5 || state == 6)
            {
                if (state == 5 && frame > 10)
                {
                    Vector2 v = new Vector2(1, 0);
                    for (byte i = 0; i < 16; ++i)
                        PaintKiller.AddObj(new GPShard(pos + ang * Radius, new Vector2((float)Math.Cos(Math.PI * i / 8), (float)Math.Sin(Math.PI * i / 8)), this));
                    state = 6;
                }
                if (++frame > 30) SetState(0);
            }
            else if (state == 7 || state == 8)
            {
                if (state == 7 && frame > 10)
                {
                    GEnemy gt = null;
                    float dist = 200 * 200, d2;
                    foreach (GEnemy g in PaintKiller.GetEnes())
                        if (g.IsColliding())
                        {
                            d2 = (g.pos - pos).LengthSquared();
                            if (d2 < dist)
                            {
                                gt = g; dist = d2;
                            }
                        }
                    if (gt != null)
                    {
                        PaintKiller.AddObj(new GPChain(pos + ang * Radius, this, gt, new List<GEnemy>()));
                        state = 8;
                    }
                    else
                    {
                        mp += 55;
                        SetState(0);
                    }
                }
                if (++frame > 30) SetState(0);
            }
        }
    }
}
