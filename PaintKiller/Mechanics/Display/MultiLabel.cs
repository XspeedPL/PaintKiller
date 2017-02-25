using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaintKilling.Net;

namespace PaintKilling.Mechanics.Display
{
    public class MultiLabel : Component
    {
        public MultiLabel()
        {
            Texts = new string[0];
            TextFont = PaintKiller.Font;
            TextAlign = HAlign.Left;
            TextColor = Color.White;
            Scale = 1;
            TextSpacing = 20;
            SelectedIndex = 0;
        }

        public override void Reset()
        {
            SelectedIndex = 0;
        }

        protected override void OnDraw(SpriteBatch sb, Vector2 pos, bool focus)
        {
            float offX = 0;
            for (int i = 0; i < Texts.Length; ++i)
            {
                Vector2 size = TextFont.MeasureString(Texts[i]) * Scale / 2;
                sb.DrawOutString(Texts[i], pos.X - size.X * (byte)TextAlign + offX, pos.Y - size.Y, TextColor, focus && i == SelectedIndex ? Color.Blue : Color.Black, 2, Scale);
                offX += size.X + TextSpacing;
            }
        }

        public override void Update(Controls ctrl)
        {
            SelectedIndex += ctrl.FirstX;
            if (SelectedIndex >= Texts.Length) SelectedIndex = 0;
            else if (SelectedIndex < 0) SelectedIndex = Texts.Length - 1;
        }

        public string[] Texts { get; set; }

        public int TextSpacing { get; set; }

        public HAlign TextAlign { get; set; }

        public Color TextColor { get; set; }

        public float Scale { get; set; }

        public SpriteFont TextFont { get; set; }

        public int SelectedIndex { get; set; }
    }
}
