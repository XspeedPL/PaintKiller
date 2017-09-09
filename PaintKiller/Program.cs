using System;
using Microsoft.Xna.Framework;

namespace PaintKilling
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
#if DEBUG
            using (var game = new PaintKiller()) game.Run();
        }
#else
            try { using (var game = new PaintKiller()) game.Run(); }
            catch (Exception ex) { using (ErrorDisplay edisp = new ErrorDisplay(ex)) edisp.Run(); }
        }
        
        private sealed class ErrorDisplay : Game
        {
            private readonly Exception data;
            private Microsoft.Xna.Framework.Graphics.SpriteBatch sb;

            public ErrorDisplay(Exception ex)
            {
                data = ex;
                GraphicsDeviceManager graphics = new GraphicsDeviceManager(this)
                {
                    IsFullScreen = false,
                    PreferredBackBufferWidth = 800,
                    PreferredBackBufferHeight = 600
                };
                Content.RootDirectory = "Content";
            }

            protected override void Initialize()
            {
                sb = new Microsoft.Xna.Framework.Graphics.SpriteBatch(GraphicsDevice);
            }

            protected override void Draw(GameTime gameTime)
            {
                GraphicsDevice.Clear(Color.OrangeRed);
                PaintKiller.Font = Content.Load<Microsoft.Xna.Framework.Graphics.SpriteFont>("Font");
                sb.Begin();
                sb.DrawOutString("An exception has been thrown!", 25, 150, Color.White, Color.Black);
                int y = 160;
                foreach (string s in data.ToString().Split('\n'))
                    try { sb.DrawOutString(s.Trim(), 10, y += 25, Color.White, Color.Black, 2, 0.6F); }
                    catch { }
                sb.End();
                base.Draw(gameTime);
            }
        }
#endif
    }
}
