using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKilling.Objects.Projectiles
{
    public sealed class GPShard : GProjectile
    {
        public GPShard(Vector2 position, Vector2 direction, GameObj shoot) : base(position, 6, direction, shoot) { }

        public override float GetAcc() { return 0.5F; }

        public override float GetMaxSpd() { return 5; }

        public override short GetMaxHP() { return 40; }

        public override Color GetColor() { return Color.White; }

        public override float GetWeight() { return 2; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            DrawCentered(sb, PaintKiller.Inst.GetTex("GPMageS"), pos, HP < 10 ? GetColor() * (HP / 10F) : GetColor(), dir + MathHelper.PiOver4, Order.Effect);
        }

        public override void PreloadContent()
        {
            PaintKiller.Inst.GetTex("GPMageS");
        }

        public override void Update()
        {
            base.Update();
            if (HP > 10 && ++frame > 3)
            {
                frame = 0;
                GEC g = new GEC(pos, "GPMageS", 15);
                g.dir = dir + MathHelper.PiOver4;
                g.prefs[1] = g.prefs[2] = true;
                PaintKiller.Inst.AddObj(g);
            }
            GameObj go = FindClosestEnemy(this, true, float.MaxValue, (System.Collections.Generic.ICollection<GameObj>)tag);
            if (go != null)
            {
                shooter.OnStrike(go.Hit(3), go);
                go.Knockback(pos, GetWeight());
                go.SetState(State.Frozen);
                if (tag != null) Kill();
                tag = Array.AsReadOnly(new GameObj[] { go });
            }
        }

        public override bool CanBounce() { return true; }
    }
}