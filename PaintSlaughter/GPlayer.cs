using System;
using System.Net;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKiller
{
    public abstract class GPlayer : GameObj
    {
        internal Net.Control keys = new Net.Control(0, 0, Net.Unpack(0)), prev = new Net.Control(0, 0, Net.Unpack(0));
        internal IPEndPoint ep;
        private byte b = 5;
        public int score;

        public GPlayer(uint id) : base(id) { }

        public GPlayer(Vector2 position) : base(position, 16) { }

        public override void Update()
        {
            if (state != 4)
            {
                float f = GetAcc();
                if (state > 0) f /= 2;
                spd.X += f * keys.X;
                spd.Y += f * keys.Y;
                if (state == 0 && keys.Keys[0]) SetState(1, !prev.Keys[0]);
                else if (state == 0 && keys.Keys[1] && mp > 35) { mp -= 30; SetState(5); }
                else if (state == 0 && keys.Keys[2] && mp > 55) { mp -= 55; SetState(7); }
                if (--b < 1) b = 20;
                if (hp < GetMaxHP() && b == 20)
                {
                    ++hp;
                    b = 20;
                }
                if (mp < GetMaxMP() && b % 6 == 0) ++mp;
                prev = keys;
            }
        }

        public override void OnDraw(SpriteBatch sb)
        {
            DrawCentered(sb, PaintKiller.GetTex("GPlayer"), pos, GetColor(), 0, Order.Player);
            string s = "";
            for (int i = 0; i < 4; ++i) if (PaintKiller.P[i] == this) s = (i + 1).ToString();
            sb.DrawString(PaintKiller.Font, s, pos - new Vector2(16, 16), Color.Black, 0, Vector2.Zero, 0.7F, SpriteEffects.None, (float)Order.Player / (float)Order.MAX);
        }

        public override void Kill() { hp = 0; mp = 0; dead = true; }

        public abstract string GetClassName();

        public abstract byte GetClassID();

        public abstract void OnStrike(short dmg, GEnemy ge);
    }
}
