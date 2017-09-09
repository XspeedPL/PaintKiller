using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PaintKilling.Objects;
using PaintKilling.Objects.Enemies;
using PaintKilling.Objects.Players;
using PaintKilling.Objects.Projectiles;

namespace PaintKilling.Mechanics.Content
{
    public sealed class Registry
    {
        private readonly Dictionary<string, GameObj> entities = new Dictionary<string, GameObj>();

        internal Registry() { }

        internal void RegisterDefaults()
        {
            GameObj empty = new GEC(Vector2.Zero, "BloodS", 0);
            Register(empty);
            Register(new GPArc(Vector2.Zero, Vector2.Zero, empty));
            Register(new GPArrow(Vector2.Zero, Vector2.Zero, empty));
            Register(new GPMiniArrow(Vector2.Zero, Vector2.Zero, empty));
            Register(new GPChain(Vector2.Zero, empty, empty, new List<GameObj>()));
            Register(new GPPiercing(Vector2.Zero, Vector2.Zero, empty));
            Register(new GPRain(Vector2.Zero, Vector2.Zero, empty));
            Register(new GPRain.GPRainA(Vector2.Zero, Vector2.Zero, empty));
            Register(new GPShard(Vector2.Zero, Vector2.Zero, empty));
            Register(new GPWave(Vector2.Zero, Vector2.Zero, empty));

            Register(new GPArch(Vector2.Zero));
            Register(new GPMage(Vector2.Zero));
            Register(new GPOrc(Vector2.Zero));

            Register(new GEnemy(Vector2.Zero));
            Register(new GEArch(Vector2.Zero));
            Register(new GEKnight(Vector2.Zero));
        }

        public void Register(GameObj template)
        {
            entities.Add(template.GetType().Name, template);
            template.PreloadContent();
        }

        public GameObj GetClone(string type, uint id)
        {
            return entities[type].Clone(id);
        }
    }
}
