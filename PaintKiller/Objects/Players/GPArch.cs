using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaintKilling.Objects.Projectiles;

namespace PaintKilling.Objects.Players
{
    internal sealed class GPArch : GPlayer
    {
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

        public override void OnStrike(short dmg, GameObj go)
        {
            Score += dmg;
        }

        public override void Update()
        {
            base.Update();
            if (state == State.Attack || state == State.AtkAfter)
            {
                if (state == State.Attack && frame > 5)
                {
                    PaintKiller.Inst.AddObj(new GPArrow(pos + ang * 5, ang, this));
                    state = State.AtkAfter;
                }
                if (++frame > 18) SetState(0);
            }
            else if (state == State.Sp1Atk || state == State.Sp1After)
            {
                if (state == State.Sp1Atk && frame > 15)
                {
                    PaintKiller.Inst.AddObj(new GPPiercing(pos + ang * 5, ang.RotateBy(-0.3F), this));
                    PaintKiller.Inst.AddObj(new GPPiercing(pos + ang * 5, ang, this));
                    PaintKiller.Inst.AddObj(new GPPiercing(pos + ang * 5, ang.RotateBy(0.3F), this));
                    state = State.Sp1After;
                }
                if (++frame > 38) SetState(0);
            }
            else if (state == State.Sp2Atk || state == State.Sp2After)
            {
                if (state == State.Sp2Atk && frame > 25)
                {
                    PaintKiller.Inst.AddObj(new GPRain(pos + ang * 5, ang, this));
                    state = State.Sp2After;
                }
                if (++frame > 58) SetState(0);
            }
        }

        public override void OnDraw(SpriteBatch sb)
        {
            base.OnDraw(sb);
            if (state == State.Attack || state == State.Sp1Atk || state == State.Sp2Atk)
            {
                DrawCentered(sb, PaintKiller.Inst.GetTex("GPArchW1"), pos, Color.White, dir, Order.Effect);
                if (frame > 3)
                {
                    Texture2D arrow = PaintKiller.Inst.GetTex("Arrow");
                    DrawCentered(sb, arrow, pos + ang * 5, Color.White, dir, Order.Effect);
                    if (state == State.Sp1Atk && frame > 10)
                    {
                        DrawCentered(sb, arrow, pos + ang * 5, Color.White, dir - 0.3F, Order.Effect, 1.15F);
                        DrawCentered(sb, arrow, pos + ang * 5, Color.White, dir + 0.3F, Order.Effect, 1.15F);
                    }
                }
            }
            else if (state == State.AtkAfter || state == State.Sp1After || state == State.Sp2After) DrawCentered(sb, PaintKiller.Inst.GetTex("GPArchW2"), pos, Color.White, dir, Order.Effect);
        }

        public override void PreloadContent()
        {
            base.PreloadContent();
            PaintKiller.Inst.GetTex("GPArchW1");
            PaintKiller.Inst.GetTex("GPArchW2");
            PaintKiller.Inst.GetTex("Arrow");
        }
    }
}
