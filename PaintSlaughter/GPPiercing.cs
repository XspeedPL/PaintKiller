using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKiller
{
    public class GPPiercing : GProjectile
    {
        public GPPiercing(uint id) : base(id) { }

        public GPPiercing(Vector2 position, Vector2 direction, GPlayer shoot) : base(position, 6, direction, shoot)
        {
            tag = new List<GameObj>();
        }

        public override float GetAcc() { return 999; }

        public override float GetMaxSpd() { return 7; }

        public override short GetMaxHP() { return 80; }

        public override short GetMaxMP() { return 0; }

        public override Color GetColor() { return new Color(1, 0.25F, 0.275F, 1); }

        public override float GetWeight() { return 6; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            DrawCentered(sb, PaintKiller.GetTex("Arrow"), pos, hp < 10 ? GetColor() * (hp / 10F) : GetColor(), dir, Order.Effect, 1.15F);
        }

        public override void Update()
        {
            base.Update();
            if (hp > 7 && ++frame > 2)
            {
                frame = 0;
                PaintKiller.AddObj(new GEC(pos, PaintKiller.GetTex("Arrow"), 11) { dir = this.dir });
            }
            foreach (GEnemy ge in PaintKiller.GetEnes())
                if (!((List<GameObj>)tag).Contains(ge) && ge.IsColliding() && Intersects(ge))
                {
                    PaintKiller.AddObj(new GEC((pos + ge.pos) / 2, PaintKiller.GetTex("BloodS"), 10));
                    shooter.OnStrike(ge.Hit(10), ge);
                    ge.Knockback(pos, GetWeight());
                    ((List<GameObj>)tag).Add(ge);
                    hp -= 18;
                    break;
                }
        }

        public override bool CanBounce() { return true; }
    }
}