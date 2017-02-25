using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKilling.Objects.Projectiles
{
    public class GPWave : GProjectile
    {
        public GPWave(Vector2 position, Vector2 direction, GameObj shoot) : base(position, 41, direction, shoot)
        {
            tag = new List<GameObj>();
        }

        public override float GetAcc() { return 1; }

        public override float GetMaxSpd() { return 5; }

        public override short GetMaxHP() { return 10; }

        public override Color GetColor() { return Color.White; }

        public override float GetWeight() { return 16; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            DrawCentered(sb, PaintKiller.Inst.GetTex("GPMageW1"), pos, HP < 5 ? GetColor() * (HP / 5F) : GetColor(), dir, Order.Effect);
        }

        public override void PreloadContent()
        {
            PaintKiller.Inst.GetTex("GPMageW1");
        }

        public override void Update()
        {
            base.Update();
            if (HP > 3 && ++MP > 1)
            {
                PaintKiller.Inst.AddObj(new GEC(pos, "GPMageW1", 7) { dir = dir });
                MP = 0;
            }
            List<GameObj> list = (List<GameObj>)tag;
            foreach (GameObj go in PaintKiller.Inst.GetObjs())
                if (go.IsEnemyOf(this) && Intersects(go))
                {
                    if (go.IsProjectile())
                    {
                        if (PaintKiller.Rand.Next(3) != 0) go.Kill();
                    }
                    else if (go.IsColliding() && !list.Contains(go))
                    {
                        go.fce += ang * GetWeight() * 2 / go.GetWeight();
                        shooter.OnStrike(go.Hit(2), go);
                        list.Add(go);
                    }
                }
        }
    }
}
