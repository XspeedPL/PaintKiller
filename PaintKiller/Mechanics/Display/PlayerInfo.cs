using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaintKilling.Objects;

namespace PaintKilling.Mechanics.Display
{
    public class PlayerInfo : Component
    {
        public GPlayer[] Array { get; }

        public int Index { get; }

        public Label Score { get; }

        public PlayerInfo(GPlayer[] plrArray, int plrIndex, Label score)
        {
            Array = plrArray;
            Index = plrIndex;
            Score = score;
        }

        protected override void OnDraw(SpriteBatch sb, Vector2 pos, bool focus)
        {
            GPlayer plr = Array[Index];
            if (plr != null)
            {
                Vector2 offset = 15 * ((int)HorzontalAlign - 1) * Vector2.UnitX;
                Texture2D tex = PaintKiller.Inst.GetTex("Bar1");
                sb.DrawCentered(tex, pos - offset, Color.White, 0, Order.BackUI);
                sb.DrawCentered(tex, pos + offset, Color.White, 0, Order.BackUI);
                tex = PaintKiller.Inst.GetTex("Bar2");
                sb.Draw(tex, pos - offset, null, Color.Red, 0, new Vector2(tex.Width / 2, tex.Height / 2), new Vector2(1, plr.HP / (float)plr.GetMaxHP()), SpriteEffects.None, 0);
                sb.Draw(tex, pos + offset, null, Color.Blue, 0, new Vector2(tex.Width / 2, tex.Height / 2), new Vector2(1, plr.MP / (float)plr.GetMaxMP()), SpriteEffects.None, 0);
                Score.Text = Array[Index].Score.ToString();
            }
        }
    }
}
