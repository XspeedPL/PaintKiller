using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaintKilling.Objects.Projectiles;

namespace PaintKilling.Objects.Players
{
    internal sealed class GPMage : GPlayer
    {
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

        public override void OnStrike(short dmg, GameObj go)
        {
            Score += dmg;
            MP += (short)(dmg / 4);
        }

        public override void OnDraw(SpriteBatch sb)
        {
            base.OnDraw(sb);
            if (IsAtkState() || IsSp1State() || IsSp2State())
                DrawCentered(sb, PaintKiller.Inst.GetTex("GPMageW"), pos, Color.White, dir + 0.2F, Order.Effect);
        }

        public override void Update()
        {
            base.Update();
            if (IsAtkState())
            {
                if (state == State.Attack && frame > 7)
                {
                    PaintKiller.Inst.AddObj(new GPWave(pos + ang * Radius, ang, this));
                    state = State.AtkAfter;
                }
                if (++frame > 30) SetState(State.Idle);
            }
            else if (IsSp1State())
            {
                if (state == State.Sp1Atk && frame > 10)
                {
                    for (byte i = 0; i < 16; ++i)
                    {
                        Vector2 v = Vector2.UnitX.RotateBy(MathHelper.TwoPi * i / 16);
                        PaintKiller.Inst.AddObj(new GPShard(pos + ang * Radius + v * Radius / 2, v, this));
                    }
                    state = State.Sp1After;
                }
                if (++frame > 30) SetState(State.Idle);
            }
            else if (IsSp2State())
            {
                if (state == State.Sp2Atk && frame > 10)
                {
                    GameObj go = FindClosestEnemy(null, true, 200 * 200);
                    if (go != null)
                    {
                        PaintKiller.Inst.AddObj(new GPChain(pos + ang * Radius, this, go, new List<GameObj>()));
                        state = State.Sp2After;
                    }
                    else
                    {
                        MP += 55;
                        SetState(State.Idle);
                    }
                }
                if (++frame > 30) SetState(State.Idle);
            }
        }

        public override void PreloadContent()
        {
            base.PreloadContent();
            PaintKiller.Inst.GetTex("GPMageW");
        }
    }
}
