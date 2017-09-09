using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKilling.Objects.Enemies
{
    class GEArch : GEnemy
    {
        public GEArch(Vector2 position) : base(position) { }

        public override bool IsColliding() { return state != State.Dying; }

        public override void OnDraw(SpriteBatch sb)
        {
            if (state == State.Dying)
            {
                DrawCentered(sb, PaintKiller.Inst.GetTex("GEnemyD1"), pos, GetColor() * ((40F - frame) / 40), 0, Order.EyeCandy);
                DrawCentered(sb, PaintKiller.Inst.GetTex("GEnemyD2"), pos, Color.White * ((15F - frame) / 15), 0, Order.EyeCandy);
            }
            else DrawCentered(sb, PaintKiller.Inst.GetTex("GEnemy"), pos, GetColor(), 0, Order.Normal);
            if (state == State.Attack || state == State.AtkAfter) DrawCentered(sb, PaintKiller.Inst.GetTex(frame < 15 ? "GPArchW1" : "GPArchW2"), pos, Color.White, dir, Order.Effect, 0.75F);
        }

        public override void PreloadContent()
        {
            base.PreloadContent();
            PaintKiller.Inst.GetTex("GPArchW1");
            PaintKiller.Inst.GetTex("GPArchW2");
        }

        public override void Update()
        {
            if (state == 0)
            {
                float dist = float.MaxValue;
                GameObj go = FindClosestEnemy(null, false, ref dist, null);
                if (go != null)
                {
                    float f2 = Radius + go.Radius + 200;
                    float f1 = Radius + go.Radius + 100;
                    Vector2 v = go.pos - pos;
                    if (dist <= f1 * f1)
                    {
                        v.Normalize();
                        spd -= v * GetAcc();
                    }
                    else if (dist <= f2 * f2)
                    {
                        v += go.spd * 50;
                        v.Normalize();
                        spd = v * GetAcc();
                        SetState(State.Attack, false);
                        SetAngle(v);
                    }
                    else
                    {
                        v.Normalize();
                        spd += v * GetAcc();
                    }
                }
            }
            else if (state == State.Attack || state == State.AtkAfter)
            {
                if (state == State.Attack && frame > 15)
                {
                    PaintKiller.Inst.AddObj(new Projectiles.GPMiniArrow(pos + ang * 3, ang, this));
                    state = State.AtkAfter;
                }
                if (++frame > 35) SetState(0);
            }
            else if (state == State.Dying && ++frame > 40) dead = true;
        }

        public override float GetAcc() { return 0.5F; }

        public override float GetMaxSpd() { return 1.8F; }

        public override short GetMaxHP() { return 12; }

        public override short GetMaxMP() { return 0; }

        public override Color GetColor() { return Color.Orange; }

        public override float GetWeight() { return 0.85F; }

        public override void Kill() { if (state != State.Dying) SetState(State.Dying); }
    }
}
