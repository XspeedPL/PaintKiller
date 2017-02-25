using System.Collections.Generic;
using PaintKilling.Objects;

namespace PaintKilling.Mechanics.Content
{
    /// <summary>Specialized List class for faster player and enemy game object lookups</summary>
    internal sealed class EntityList : UnsafeList<GameObj>
    {
        private readonly Dictionary<byte, ushort> teamCounts = new Dictionary<byte, ushort>();

        public ushort GetTeamCount(byte team)
        {
            return teamCounts.ContainsKey(team) ? teamCounts[team] : (ushort)0;
        }

        public override void Add(GameObj value)
        {
            base.Add(value);
            if (teamCounts.ContainsKey(value.team)) ++teamCounts[value.team];
            else teamCounts.Add(value.team, 1);
        }

        public override void Remove(Node node)
        {
            --teamCounts[node.Value.team];
            base.Remove(node);
        }

        public GameObj GetByID(uint id)
        {
            foreach (GameObj go in this) if (go.ID == id) return go;
            return null;
        }
    }

}
