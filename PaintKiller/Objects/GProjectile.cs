using System;
using Microsoft.Xna.Framework;

namespace PaintKilling.Objects
{
    public abstract class GProjectile : GameObj
    {
        public readonly GameObj shooter;
        
        public GProjectile(Vector2 position, short radius, Vector2 direction, GameObj shoot) : base(position, radius)
        {
            spd = direction; shooter = shoot; team = shoot.team; SetAngle(direction);
        }

        public sealed override short GetMaxMP() { return short.MaxValue; }

        public sealed override bool IsColliding() { return false; }

        public sealed override bool IsProjectile() { return true; }

        public override void Update()
        {
            --HP;
            spd += ang * GetAcc();
        }
    }
}
