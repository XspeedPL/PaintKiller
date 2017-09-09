using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKilling.Objects.Projectiles
{
    public sealed class GPArc : GProjectile
    {
        public GPArc(Vector2 position, Vector2 direction, GameObj shoot) : base(position, 5, direction, shoot) { }

        public override float GetAcc() { return 0.7F; }

        public override float GetMaxSpd() { return 5; }

        public override short GetMaxHP() { return 80; }

        public override Color GetColor() { return Color.White; }

        public override float GetWeight() { return 6; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            DrawCentered(sb, PaintKiller.Inst.GetTex("Arrow"), pos, HP < 10 ? GetColor() * (HP / 10F) : GetColor(), dir, Order.Effect);
        }

        public override void PreloadContent()
        {
            PaintKiller.Inst.GetTex("Arrow");
        }

        public override void Update()
        {
            base.Update();
            if (HP == 10)
            {
                GameObj go = FindClosestEnemy(this);
                if (go != null)
                {
                    PaintKiller.Inst.AddObj(new GEC((pos + go.pos) / 2, "BloodS", 10));
                    Shooter.OnStrike(go.Hit(7), go);
                    go.Knockback(pos, GetWeight());
                }
            }
        }

        public override bool CanBounce() { return true; }
    }
}
