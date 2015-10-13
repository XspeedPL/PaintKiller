using System;
using Microsoft.Xna.Framework;

namespace PaintKiller
{
    public abstract class GProjectile : GameObj
    {
        public readonly GPlayer shooter;

        public GProjectile(uint id) : base(id) { }

        public GProjectile(Vector2 position, short radius, Vector2 direction, GPlayer shoot) : base(position, radius)
        {
            spd = direction; shooter = shoot; UpdateAngle();
        }

        public override bool IsColliding() { return false; }

        public override void Update()
        {
            --hp;
            spd += ang * GetAcc();
        }

        public abstract bool CanBounce();
    }
}
