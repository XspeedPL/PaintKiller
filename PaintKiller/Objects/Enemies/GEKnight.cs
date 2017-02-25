using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKilling.Objects.Enemies
{
    class GEKnight : GameObj
    {
        public GEKnight(Vector2 position) : base(position, 17) { }

        public override bool IsColliding() { return state != State.Dying; }

        public override void OnDraw(SpriteBatch sb)
        {
            Texture2D player = PaintKiller.Inst.GetTex("GPlayer");
            if (state == State.Dying)
            {
                DrawCentered(sb, PaintKiller.Inst.GetTex("GEKnightW"), new Vector2(pos.X, pos.Y - 35), Color.White * ((40F - frame) / 20), 2.35F, Order.Eyecandy, 0.75F);
                DrawCentered(sb, player, pos, GetColor() * ((25F - frame) / 25), 0, Order.Eyecandy, 1.2F);
            }
            else DrawCentered(sb, player, pos, GetColor(), 0, Order.Normal, 1.2F);
            if ((state == State.Attack && frame > 10) || state == State.AtkAfter) DrawCentered(sb, PaintKiller.Inst.GetTex("GEKnightW"), pos, Color.White, dir + (frame - 16) / 10F, Order.Effect, 0.75F);
        }

        public override void PreloadContent()
        {
            PaintKiller.Inst.GetTex("GPlayer");
            PaintKiller.Inst.GetTex("GEKnightW");
        }

        public override void Update()
        {
            if (state == State.Idle)
            {
                float dist = float.MaxValue;
                GameObj go = FindClosestEnemy(null, false, ref dist, null);
                if (go != null)
                {
                    float f2 = Radius + go.Radius + 75;
                    float f1 = Radius + go.Radius + 50;
                    Vector2 v = go.pos - pos;
                    if (dist <= f1 * f1)
                    {
                        v += go.spd * 5;
                        v.Normalize();
                        spd += v * GetAcc();
                        SetState(State.Attack, false);
                        UpdateAngle(true);
                    }
                    else if (dist <= f2 * f2)
                    {
                        v += go.spd * 35;
                        v.Normalize();
                        spd += v * GetAcc();
                        SetState(State.Attack, false);
                        UpdateAngle(true);
                    }
                    else
                    {
                        v.Normalize();
                        spd += v * GetAcc();
                    }
                }
            }
            else if (state == State.Attack && ++frame > 4)
            {
                if (frame < 30)
                {
                    spd += ang * GetAcc();
                    fce += ang * GetAcc() / 2;
                }
                if (frame > 12 && frame < 24)
                {
                    GColli gc = new GColli(new Vector2(ang.X * Radius * 1.5F + pos.X, ang.Y * Radius * 1.5F + pos.Y), 14);
                    GameObj go = FindClosestEnemy(gc);
                    if (go != null)
                    {
                        PaintKiller.Inst.AddBlood(this, go);
                        go.Knockback(gc.pos, 7);
                        go.Hit(8);
                        state = State.AtkAfter;
                    }
                }
                else if (frame > 36) SetState(0);
            }
            else if (state == State.AtkAfter && ++frame > 36) SetState(0);
            else if (state == State.Dying && ++frame > 40) dead = true;
        }

        public override float GetAcc() { return 0.35F; }

        public override float GetMaxSpd() { return state == State.Attack ? 3.5F : 1.9F; }

        public override short GetMaxHP() { return 35; }

        public override short GetMaxMP() { return 0; }

        public override Color GetColor() { return new Color(25, 25, 25); }

        public override float GetWeight() { return 5; }

        public override void Kill() { if (state != State.Dying) SetState(State.Dying); }
    }
}
