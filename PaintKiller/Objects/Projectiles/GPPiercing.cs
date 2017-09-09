using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKilling.Objects.Projectiles
{
    public class GPPiercing : GProjectile
    {
        public GPPiercing(Vector2 position, Vector2 direction, GameObj shoot) : base(position, 6, direction, shoot)
        {
            spd *= GetMaxSpd() * 2;
            tag = new List<GameObj>();
        }

        public override float GetAcc() { return 0; }

        public override float GetDaccRate() { return 0.9875F; }

        public override float GetMaxSpd() { return 9; }

        public override short GetMaxHP() { return 80; }

        public override Color GetColor() { return new Color(1, 0.25F, 0.275F, 1); }

        public override float GetWeight() { return 6; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            DrawCentered(sb, PaintKiller.Inst.GetTex("Arrow"), pos, HP < 10 ? GetColor() * (HP / 10F) : GetColor(), dir, Order.Effect, 1.15F);
        }

        public override void PreloadContent()
        {
            PaintKiller.Inst.GetTex("Arrow");
        }

        public override void Update()
        {
            base.Update();
            if (HP > 7 && ++frame > 2)
            {
                frame = 0;
                PaintKiller.Inst.AddObj(new GEC(pos, "Arrow", 11) { dir = dir });
            }
            List<GameObj> list = (List<GameObj>)tag;
            GameObj go = FindClosestEnemy(this, true, float.MaxValue, list);
            if (go != null)
            {
                PaintKiller.Inst.AddBlood(this, go);
                Shooter.OnStrike(go.Hit(10), go);
                go.Knockback(pos, GetWeight());
                list.Add(go);
                HP -= 18;
            }
        }

        public override bool CanBounce() { return true; }
    }
}