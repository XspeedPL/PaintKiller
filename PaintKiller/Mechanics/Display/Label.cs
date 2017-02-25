using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKilling.Mechanics.Display
{
    public class Label : Component
    {
        public Label()
        {
            Text = "";
            TextFont = PaintKiller.Font;
            TextAlign = HAlign.Left;
            TextColor = Color.White;
            Scale = 1;
        }

        protected override void OnDraw(SpriteBatch sb, Vector2 pos, bool focus)
        {
            Vector2 size = TextFont.MeasureString(Text) * Scale / 2;
            sb.DrawOutString(Text, pos.X - size.X * (byte)TextAlign, pos.Y - size.Y, TextColor, focus ? Color.Blue : Color.Black, 2, Scale);
        }

        public string Text { get; set; }

        public HAlign TextAlign { get; set; }

        public Color TextColor { get; set; }

        public float Scale { get; set; }

        public SpriteFont TextFont { get; set; }
    }
}
