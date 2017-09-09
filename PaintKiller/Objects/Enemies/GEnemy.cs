using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKilling.Objects.Enemies
{
    public class GEnemy : GameObj
    {
        public GEnemy(Vector2 position) : base(position, 12) { }

        public override bool IsColliding() { return (byte)state < 4; }

        public override void OnDraw(SpriteBatch sb)
        {
            if (state == State.Dying)
            {
                DrawCentered(sb, PaintKiller.Inst.GetTex("GEnemyD1"), pos, GetColor() * ((40F - frame) / 40), 0, Order.EyeCandy);
                DrawCentered(sb, PaintKiller.Inst.GetTex("GEnemyD2"), pos, Color.White * ((15F - frame) / 15), 0, Order.EyeCandy);
            }
            else DrawCentered(sb, PaintKiller.Inst.GetTex("GEnemy"), pos, GetColor(), 0, Order.Normal);
            if (((state == State.Attack || state == State.Sp1Atk) && frame > 10) || state == State.AtkAfter)
                DrawCentered(sb, PaintKiller.Inst.GetTex("GEnemyW"), pos, Color.White, dir + (frame - 23F) / 15, Order.Effect);
        }

        public override void PreloadContent()
        {
            PaintKiller.Inst.GetTex("GEnemy");
            PaintKiller.Inst.GetTex("GEnemyD1");
            PaintKiller.Inst.GetTex("GEnemyD2");
            PaintKiller.Inst.GetTex("GEnemyW");
        }

        public override void Update()
        {
            if (state == 0)
            {
                float dist = float.MaxValue;
                GameObj go = FindClosestEnemy(null, false, ref dist, null);
                if (go != null)
                {
                    float f = Radius + go.Radius + 9;
                    Vector2 v = go.pos - pos + go.spd * 5;
                    v.Normalize();
                    bool col = go.IsColliding();
                    if (col && dist <= f * f)
                    {
                        spd += v * GetAcc() * 2.5F;
                        SetState(PaintKiller.Rand.Next(5) == 0 ? State.Sp1Atk : State.Attack, false);
                        UpdateAngle(true);
                    }
                    else spd += v * GetAcc() * (col ? 1 : -0.5F);
                }
            }
            else if ((state == State.Sp1Atk || state == State.Attack) && ++frame > 10)
            {
                if (state == State.Sp1Atk) spd += ang * GetAcc();
                if (frame < 24)
                {
                    GColli gc = new GColli(new Vector2(ang.X * 25 + pos.X, ang.Y * 25 + pos.Y), 10);
                    GameObj go = FindClosestEnemy(gc);
                    if (go != null)
                    {
                        PaintKiller.Inst.AddBlood(this, go);
                        go.Knockback(gc.pos, 2);
                        go.Hit(4);
                        state = State.AtkAfter;
                    }
                }
                else if (frame > 36) SetState(0);
            }
            else if (state == State.AtkAfter && ++frame > 36) SetState(State.Idle);
            else if (state == State.Dying && ++frame > 40) dead = true;
        }

        public override float GetAcc() { return 0.55F; }

        public override float GetMaxSpd() { return state == State.Sp1Atk ? 2.5F : 1.75F; }

        public override short GetMaxHP() { return 15; }

        public override short GetMaxMP() { return 0; }

        public override Color GetColor() { return Color.GreenYellow; }

        public override float GetWeight() { return 0.95F; }

        public override void Kill() { if (state != State.Dying) SetState(State.Dying); }
    }
}
