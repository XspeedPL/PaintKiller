using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using PaintKilling.Net;

namespace PaintKilling
{
    /// <summary>Display order enum</summary>
    public enum Order
    {
        UI = 0, BackUI, Midair, Effect, Normal, EyeCandy, Background, Max
    }

    public static class Helpers
    {
        private const ushort size = 2048;
        private const float step = MathHelper.TwoPi / size;
        private static float[] sin = new float[size];

        internal static void Init()
        {
            for (int i = 0; i < size; ++i)
                sin[i] = (float)Math.Sin(i * step);
        }

        public static float Sin(float a)
        {
            a %= MathHelper.TwoPi;
            if (a < 0) a += MathHelper.TwoPi;
            return sin[(int)(a / step)];
        }

        public static float Cos(float a)
        {
            return Sin(MathHelper.PiOver2 - a);
        }

        public static Vector2 RotateBy(this Vector2 v, float angle)
        {
            float sin = Sin(angle), cos = Cos(angle);
            return new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
        }

        /// <summary>Constructs a controls snapshot from a keyboard state object</summary>
        public static Controls GetCtrlState(this KeyboardState keyState)
        {
            float x = (keyState.IsKeyDown(Keys.Left) ? -1 : 0) + (keyState.IsKeyDown(Keys.Right) ? 1 : 0);
            float y = (keyState.IsKeyDown(Keys.Up) ? -1 : 0) + (keyState.IsKeyDown(Keys.Down) ? 1 : 0);
            bool[] k = new bool[8];
            k[0] = keyState.IsKeyDown(Keys.F);
            k[1] = keyState.IsKeyDown(Keys.G);
            k[2] = keyState.IsKeyDown(Keys.H);
            k[3] = keyState.IsKeyDown(Keys.T);
            k[6] = keyState.IsKeyDown(Keys.F1);
            k[7] = keyState.IsKeyDown(Keys.Escape);
            return new Controls(x, y, k);
        }

        /// <summary>Constructs a controls snapshot from a gamepad state object</summary>
        public static Controls GetCtrlState(this GamePadState padState)
        {
            bool[] k = new bool[8];
            k[0] = padState.IsButtonDown(Buttons.X);
            k[1] = padState.IsButtonDown(Buttons.A);
            k[2] = padState.IsButtonDown(Buttons.B);
            k[3] = padState.IsButtonDown(Buttons.Y);
            k[6] = padState.IsButtonDown(Buttons.Start);
            k[7] = padState.IsButtonDown(Buttons.Back);
            return new Controls(padState.ThumbSticks.Left.X, padState.ThumbSticks.Left.Y, k);
        }

        /// <summary>Draws a string with an outline</summary>
        public static void DrawOutString(this SpriteBatch sb, string txt, float x, float y, Color col, Color co2, float space = 2, float scale = 1)
        {
            DrawString(sb, txt, new Vector2(x + space, y), co2, Order.BackUI, scale);
            DrawString(sb, txt, new Vector2(x - space, y), co2, Order.BackUI, scale);
            DrawString(sb, txt, new Vector2(x, y + space), co2, Order.BackUI, scale);
            DrawString(sb, txt, new Vector2(x, y - space), co2, Order.BackUI, scale);
            DrawString(sb, txt, new Vector2(x, y), col, Order.UI, scale);
        }

        /// <summary>Draws a texture outline</summary>
        public static void DrawOutOnly(this SpriteBatch sb, Texture2D tex, float x, float y, Color col, Order lay, float space = 2, float scale = 1)
        {
            Draw(sb, tex, new Vector2(x + space, y), col, lay, scale);
            Draw(sb, tex, new Vector2(x - space, y), col, lay, scale);
            Draw(sb, tex, new Vector2(x, y - space), col, lay, scale);
            Draw(sb, tex, new Vector2(x, y + space), col, lay, scale);
        }

        /// <summary>Draws a texture centered at a specified position</summary>
        public static void DrawCentered(this SpriteBatch sb, Texture2D tex, Vector2 pos, Color col, float ang, Order lay, float scale = 1)
        {
            sb.Draw(tex, pos, null, col, ang, new Vector2(tex.Width / 2, tex.Height / 2), scale, SpriteEffects.None, (float)lay / (float)Order.Max);
        }

        public static void DrawString(this SpriteBatch sb, string txt, Vector2 pos, Color col, Order lay, float scale = 1)
        {
            sb.DrawString(PaintKiller.Font, txt, pos, col, 0, Vector2.Zero, scale, SpriteEffects.None, (float)lay / (float)Order.Max);
        }

        public static void Draw(this SpriteBatch sb, Texture2D tex, Vector2 pos, Color col, Order lay, float scale = 1)
        {
            sb.Draw(tex, pos, null, col, 0, new Vector2(tex.Width / 2, tex.Height / 2), scale, SpriteEffects.None, (float)lay / (float)Order.Max);
        }
    }
}
