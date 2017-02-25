using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaintKilling.Net;

namespace PaintKilling.Mechanics.Display
{
    public class Toggle : Component
    {
        public event System.Func<Toggle, bool> Change;

        public Toggle()
        {
            TextL = "";
            TextR = "";
            TextFont = PaintKiller.Font;
            Scale = 1;
            Toggled = false;
        }

        protected override void OnDraw(SpriteBatch sb, Vector2 pos, bool focus)
        {
            Vector2 size = TextFont.MeasureString(TextL + " ") * Scale;
            sb.DrawOutString(TextL, pos.X - size.X, pos.Y - size.Y / 2, Toggled ? Color.Gray : Color.White, focus ? Color.Blue : Color.Black, 2, Scale);
            sb.DrawOutString(" " + TextR, pos.X, pos.Y - size.Y / 2, Toggled ? Color.White : Color.Gray, focus ? Color.Blue : Color.Black, 2, Scale);
        }

        public override void Update(Controls ctrl)
        {
            int dirX = ctrl.FirstX;
            if (dirX == -1 && Toggled && Change.Invoke(this)) Toggled = false;
            else if (dirX == 1 && !Toggled && Change.Invoke(this)) Toggled = true;
        }

        public string TextL { get; set; }

        public string TextR { get; set; }

        public float Scale { get; set; }

        public SpriteFont TextFont { get; set; }

        public bool Toggled { get; set; }
    }
}