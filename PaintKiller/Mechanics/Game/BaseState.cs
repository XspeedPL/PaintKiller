using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaintKilling.Net;
using PaintKilling.Mechanics.Display;
using PaintKilling.Mechanics.Content;

namespace PaintKilling.Mechanics.Game
{
    public class BaseState
    {
        public event System.Action<BaseState> Resetting;

        protected UnsafeCollection<Component> Components { get; private set; }

        protected UnsafeCollection<Component>.Node Focused { get; set; }

        public string Name { get; private set; }

        public BaseState(string name)
        {
            Components = new UnsafeCollection<Component>();
            Focused = new UnsafeCollection<Component>.Node(null);
            Name = name;
        }

        public Component FindByID(ushort id)
        {
            foreach (Component c in Components) if (c.ID == id) return c;
            return null;
        }

        public void AddComponent(Component c)
        {
            Components.Add(c);
        }

        public void Draw(SpriteBatch sb, Point screen)
        {
            foreach (Component c in Components)
                if (c.Enabled) c.Draw(sb, screen, c == Focused.Value);
        }

        public virtual void Reset()
        {
            foreach (Component c in Components) c.Reset();
            Resetting?.Invoke(this);
        }

        public virtual void Update(Controls ctrl) { }
    }
}
