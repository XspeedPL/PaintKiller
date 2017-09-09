using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKilling.Objects.Projectiles
{
    public class GPMiniArrow : GProjectile
    {
        public GPMiniArrow(Vector2 position, Vector2 direction, GameObj shooter) : base(position, 4, direction, shooter)
        {
            spd *= GetMaxSpd();
        }

        public override float GetAcc() { return 0; }

        public override float GetDaccRate() { return 0.98F; }

        public override float GetMaxSpd() { return 8; }

        public override short GetMaxHP() { return 65; }

        public override Color GetColor() { return Color.White; }

        public override float GetWeight() { return 3; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            DrawCentered(sb, PaintKiller.Inst.GetTex("Arrow"), pos, HP < 10 ? GetColor() * (HP / 10F) : GetColor(), dir, Order.Effect, 0.75F);
        }

        public override void PreloadContent()
        {
            PaintKiller.Inst.GetTex("Arrow");
        }

        public override void Update()
        {
            base.Update();
            GameObj go = FindClosestEnemy(this);
            if (go != null)
            {
                PaintKiller.Inst.AddBlood(this, go);
                Shooter.OnStrike(go.Hit(4), go);
                go.Knockback(pos, GetWeight());
                Kill();
            }
        }

        public override bool CanBounce() { return false; }
    }
}
