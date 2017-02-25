using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using PaintKilling.Net;

namespace PaintKilling.Mechanics.Display
{
    public class IPInput : Label
    {
        private KeyboardState prev;

        public IPInput()
        {
            prev = new KeyboardState();
            ReadOnly = false;
        }

        protected override void OnDraw(SpriteBatch sb, Vector2 pos, bool focus)
        {
            string str = Text + (ReadOnly ? "" : " _");
            Vector2 size = TextFont.MeasureString(str) * Scale / 2;
            sb.DrawOutString(str, pos.X - size.X * (byte)TextAlign, pos.Y - size.Y, TextColor, focus ? Color.Blue : Color.Black, 2, Scale);
        }

        public override void Reset()
        {
            Text = "";
            ReadOnly = false;
        }

        public override void Update(Controls ctrl)
        {
            if (ReadOnly) return;
            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.NumPad0) && prev.IsKeyUp(Keys.NumPad0)) Text += '0';
            else if (ks.IsKeyDown(Keys.NumPad1) && prev.IsKeyUp(Keys.NumPad1)) Text += '1';
            else if (ks.IsKeyDown(Keys.NumPad2) && prev.IsKeyUp(Keys.NumPad2)) Text += '2';
            else if (ks.IsKeyDown(Keys.NumPad3) && prev.IsKeyUp(Keys.NumPad3)) Text += '3';
            else if (ks.IsKeyDown(Keys.NumPad4) && prev.IsKeyUp(Keys.NumPad4)) Text += '4';
            else if (ks.IsKeyDown(Keys.NumPad5) && prev.IsKeyUp(Keys.NumPad5)) Text += '5';
            else if (ks.IsKeyDown(Keys.NumPad6) && prev.IsKeyUp(Keys.NumPad6)) Text += '6';
            else if (ks.IsKeyDown(Keys.NumPad7) && prev.IsKeyUp(Keys.NumPad7)) Text += '7';
            else if (ks.IsKeyDown(Keys.NumPad8) && prev.IsKeyUp(Keys.NumPad8)) Text += '8';
            else if (ks.IsKeyDown(Keys.NumPad9) && prev.IsKeyUp(Keys.NumPad9)) Text += '9';
            else if (ks.IsKeyDown(Keys.D0) && prev.IsKeyUp(Keys.D0)) Text += '0';
            else if (ks.IsKeyDown(Keys.D1) && prev.IsKeyUp(Keys.D1)) Text += '1';
            else if (ks.IsKeyDown(Keys.D2) && prev.IsKeyUp(Keys.D2)) Text += '2';
            else if (ks.IsKeyDown(Keys.D3) && prev.IsKeyUp(Keys.D3)) Text += '3';
            else if (ks.IsKeyDown(Keys.D4) && prev.IsKeyUp(Keys.D4)) Text += '4';
            else if (ks.IsKeyDown(Keys.D5) && prev.IsKeyUp(Keys.D5)) Text += '5';
            else if (ks.IsKeyDown(Keys.D6) && prev.IsKeyUp(Keys.D6)) Text += '6';
            else if (ks.IsKeyDown(Keys.D7) && prev.IsKeyUp(Keys.D7)) Text += '7';
            else if (ks.IsKeyDown(Keys.D8) && prev.IsKeyUp(Keys.D8)) Text += '8';
            else if (ks.IsKeyDown(Keys.D9) && prev.IsKeyUp(Keys.D9)) Text += '9';
            else if (ks.IsKeyDown(Keys.OemPeriod) && prev.IsKeyUp(Keys.OemPeriod)) Text += '.';
            else if (ks.IsKeyDown(Keys.Back) && prev.IsKeyUp(Keys.Back) && Text.Length > 0) Text = Text.Remove(Text.Length - 1);
            prev = ks;
        }

        public bool ReadOnly { get; set; }
    }
}
