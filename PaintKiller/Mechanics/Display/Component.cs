using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKilling.Mechanics.Display
{
    public abstract class Component
    {
        public enum VAlign : byte { Top = 0, Center, Bottom }
        public enum HAlign : byte { Left = 0, Center, Right }

        public Component()
        {
            ID = 0;
            Position = Vector2.Zero;
            Enabled = true;
            Focusable = true;
            VerticalAlign = VAlign.Top;
            HorzontalAlign = HAlign.Left;
        }

        public void Draw(SpriteBatch sb, Point screen, bool focus)
        {
            Vector2 pos = Position;
            pos.X += screen.X * (byte)HorzontalAlign / 2F;
            pos.Y += screen.Y * (byte)VerticalAlign / 2F;
            OnDraw(sb, pos, focus);
        }

        public virtual void Reset() { }

        protected abstract void OnDraw(SpriteBatch sb, Vector2 pos, bool focus);
        
        public virtual void Update(Net.Controls ctrl) { }

        public bool Focusable { get; set; }

        public ushort ID { get; set; }

        public bool Enabled { get; set; }

        public Vector2 Position { get; set; }

        public VAlign VerticalAlign { get; set; }

        public HAlign HorzontalAlign { get; set; }
    }
}
