using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKiller
{
    public class GPWave : GProjectile
    {
        public GPWave(uint id) : base(id) { }

        public GPWave(Vector2 position, Vector2 direction, GPlayer shoot) : base(position, 41, direction, shoot)
        {
            tag = new List<GameObj>();
        }

        public override float GetAcc() { return 1; }

        public override float GetMaxSpd() { return 5; }

        public override short GetMaxHP() { return 10; }

        public override short GetMaxMP() { return 0; }

        public override Color GetColor() { return Color.White; }

        public override float GetWeight() { return 16; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            DrawCentered(sb, PaintKiller.GetTex("GPMageW1"), pos, hp < 5 ? GetColor() * (hp / 5F) : GetColor(), dir, Order.Effect);
        }

        public override void Update()
        {
            base.Update();
            if (hp > 3 && ++state > 1)
            {
                PaintKiller.AddObj(new GEC(pos, PaintKiller.GetTex("GPMageW1"), 7) { dir = this.dir });
                state = 0;
            }
            foreach (GameObj ge in PaintKiller.GetObjs())
                if (!((List<GameObj>)tag).Contains(ge) && ge.IsColliding() && Intersects(ge) && ge is GEnemy)
                {
                    ge.fce += ang * GetWeight() * 2 / ge.GetWeight();
                    shooter.OnStrike(ge.Hit(2), (GEnemy)ge);
                    ((List<GameObj>)tag).Add(ge);
                    break;
                }
                else if (ge is GPArrowE && Intersects(ge) && PaintKiller.rand.Next(5) < 2) ge.Kill();
        }

        public override bool CanBounce() { return false; }
    }
}
