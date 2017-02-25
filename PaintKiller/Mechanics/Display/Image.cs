using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKilling.Mechanics.Display
{
    public class Image : Component
    {
        public Image()
        {
            Scale = 1;
            TexColor = Color.White;
        }

        protected override void OnDraw(SpriteBatch sb, Vector2 pos, bool focus)
        {
            if (focus) sb.DrawOutOnly(Texture, pos.X, pos.Y, Color.Black, Order.BackUI, 2 * Scale, Scale);
            sb.DrawCentered(Texture, pos, TexColor, 0, Order.UI, Scale);
        }

        public Texture2D Texture { get; set; }

        public float Scale { get; set; }

        public Color TexColor { get; set; }
    }
}
