using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKiller
{
    public sealed class GPShard : GProjectile
    {
        public GPShard(uint id) : base(id) { }

        public GPShard(Vector2 position, Vector2 direction, GPlayer shoot) : base(position, 6, direction, shoot) { }

        public override float GetAcc() { return 0.5F; }

        public override float GetMaxSpd() { return 5; }

        public override short GetMaxHP() { return 40; }

        public override short GetMaxMP() { return 0; }

        public override Color GetColor() { return Color.White; }

        public override float GetWeight() { return 2; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            DrawCentered(sb, PaintKiller.GetTex("GPMageS"), pos, hp < 10 ? GetColor() * (hp / 10F) : GetColor(), dir + D45, Order.Effect);
        }

        public override void Update()
        {
            base.Update();
            if (hp > 10 && ++frame > 3)
            {
                frame = 0;
                GEC g = new GEC(pos, PaintKiller.GetTex("GPMageS"), 15) { dir = this.dir + D45 };
                g.prefs[1] = g.prefs[2] = true;
                PaintKiller.AddObj(g);
            }
            foreach (GEnemy ge in PaintKiller.GetEnes())
                if (ge != tag && ge.IsColliding() && Intersects(ge))
                {
                    shooter.OnStrike(ge.Hit(3), ge);
                    ge.Knockback(pos, GetWeight());
                    ge.SetState(3);
                    if (tag != null) Kill();
                    tag = ge;
                    break;
                }
        }

        public override bool CanBounce() { return true; }
    }
}