using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKilling.Objects.Players
{
    internal sealed class GPOrc : GPlayer
    {
        public GPOrc(Vector2 position) : base(position) { }

        public override float GetAcc() { return 0.75F; }

        public override float GetMaxSpd() { return 3.25F; }

        public override short GetMaxHP() { return 95; }

        public override short GetMaxMP() { return 75; }

        public override Color GetColor() { return Color.DarkGreen; }

        public override float GetWeight() { return 4; }

        public override string GetClassName() { return "Orc"; }

        public override byte GetClassID() { return 0; }

        public override bool IsColliding() { return state != State.Sp1Atk && state != State.Sp2Atk; }

        public override void OnStrike(short dmg, GameObj go)
        {
            Score += dmg;
            HP += (short)(dmg * 2 / 9);
        }

        public override void Update()
        {
            base.Update();
            if (IsAtkState())
            {
                if (state == State.Attack && frame > 8)
                {
                    GColli gc = new GColli(new Vector2(20, 0).RotateBy(dir + (frame - 6) / 10F - MathHelper.PiOver4) + pos, 20);
                    foreach (GameObj go in PaintKiller.Inst.GetObjs())
                        if (go.IsEnemyOf(this) && gc.Intersects(go))
                            if (go.IsProjectile()) go.Kill();
                            else if (go != tag && go.IsColliding())
                            {
                                PaintKiller.Inst.AddBlood(this, go);
                                OnStrike(go.Hit(10), go);
                                go.Knockback(gc.pos, 5);
                                if (tag != null) state = State.AtkAfter;
                                else tag = go;
                            }
                }
                if (++frame > 24) SetState(0);
            }
            else if (state == State.Sp1Atk)
            {
                if (frame > 8 && frame < 40)
                {
                    List<GameObj> list = (List<GameObj>)(tag == null ? tag = new List<GameObj>() : tag);
                    float wdir = frame < 8 || frame > 40 ? 0 : (frame - 8) * MathHelper.TwoPi / 8F;
                    GColli gc = new GColli(new Vector2(20, 0).RotateBy(dir + wdir) + pos, 20);
                    foreach (GameObj go in PaintKiller.Inst.GetObjs())
                        if (go.IsEnemyOf(this) && gc.Intersects(go))
                            if (go.IsProjectile()) go.Kill();
                            else if (go.IsColliding())
                            {
                                if (!list.Contains(go))
                                {
                                    PaintKiller.Inst.AddBlood(this, go);
                                    list.Add(go);
                                    OnStrike(go.Hit(14), go);
                                }
                                go.Knockback(gc.pos, 12);
                            }
                }
                if (++frame > 48) SetState(0);
            }
            else if (state == State.Sp2Atk)
            {
                List<GameObj> list = (List<GameObj>)(tag == null ? tag = new List<GameObj>() : tag);
                spd += ang * GetAcc() * 2;
                fce += ang * GetAcc();
                if (++frame % 3 == 0) PaintKiller.Inst.AddObj(new GEC(pos, "GPlayer", 15));
                GColli gc = new GColli(new Vector2(ang.X * 25 + pos.X, ang.Y * 25 + pos.Y), 20);
                foreach (GameObj go in PaintKiller.Inst.GetObjs())
                    if (go.IsEnemyOf(this) && gc.Intersects(go))
                        if (go.IsProjectile()) go.Kill();
                        else if (go.IsColliding())
                        {
                            if (!list.Contains(go))
                            {
                                PaintKiller.Inst.AddBlood(this, go);
                                list.Add(go);
                                OnStrike(go.Hit(18), go);
                            }
                            go.Knockback(gc.pos, 7);
                        }
                if (frame > 40) SetState(0);
            }
        }

        public override void PreloadContent()
        {
            base.PreloadContent();
            PaintKiller.Inst.GetTex("GPOrcW");
        }

        public override void OnDraw(SpriteBatch sb)
        {
            base.OnDraw(sb);
            float wdir;
            if (IsAtkState()) wdir = (frame - 6) / 10F;
            else if (state == State.Sp1Atk) wdir = frame < 8 || frame > 40 ? 0 : (frame - 8) * MathHelper.TwoPi / 8F;
            else if (state == State.Sp2Atk) wdir = (frame - 10) / 16F;
            else wdir = -1;
            if (wdir != -1)
            {
                Texture2D weap = PaintKiller.Inst.GetTex("GPOrcW");
                DrawCentered(sb, weap, pos, Color.White, dir + wdir, Order.Effect);
                if (state == State.Sp2Atk)
                    sb.Draw(weap, pos, null, Color.White, dir - wdir, new Vector2(weap.Width / 2, weap.Height / 2), 1, SpriteEffects.FlipVertically, (float)Order.Effect / (float)Order.Max);
            }
        }
    }

}
