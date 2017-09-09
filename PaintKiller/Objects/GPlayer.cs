using System;
using System.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaintKilling.Net;

namespace PaintKilling.Objects
{
    public abstract class GPlayer : GameObj
    {
        /// <summary>Player's most up to date controls snapshot</summary>
        internal Controls keys = new Controls(0, 0, new bool[8]);

        /// <summary>Player's endpoint</summary>
        public IPEndPoint EP { get; internal set; }

        /// <summary>Counter used for health and magic points regeneration</summary>
        private byte b = 5;

        /// <summary>Player's score</summary>
        public int Score { get; internal set; }
        
        public GPlayer(Vector2 position) : base(position, 16) { team = 1; }

        public override void Update()
        {
            if (state != State.Dying)
            {
                float f = GetAcc();
                if (state != State.Idle) f /= 2;
                spd.X += f * keys.X;
                spd.Y += f * keys.Y;
                if (state == State.Idle && keys.Keys[0]) SetState(State.Attack, !keys.Prev.Keys[0]);
                else if (state == State.Idle && keys.Keys[1] && MP > 35) { MP -= 30; SetState(State.Sp1Atk); }
                else if (state == State.Idle && keys.Keys[2] && MP > 55) { MP -= 55; SetState(State.Sp2Atk); }
                if (--b < 1)
                {
                    b = 20;
                    if (HP < GetMaxHP()) ++HP;
                }
                if (MP < GetMaxMP() && b % 6 == 0) ++MP;
            }
        }

        public override void OnDraw(SpriteBatch sb)
        {
            DrawCentered(sb, PaintKiller.Inst.GetTex("GPlayer"), pos, GetColor(), 0, Order.Normal);
            string s = "?";
            for (int i = 0; i < 4; ++i)
                if (PaintKiller.Inst.P[i] == this)
                {
                    s = (i + 1).ToString();
                    break;
                }
            sb.DrawString(PaintKiller.Font, s, pos - new Vector2(Radius, Radius), Color.Black, 0, Vector2.Zero, 0.7F, SpriteEffects.None, (float)Order.Normal / (float)Order.Max);
        }

        public override void Kill() { HP = 0; MP = 0; dead = true; }

        /// <summary>Gets the name of this character class</summary>
        public abstract string GetClassName();

        /// <summary>Gets the unique ID of this character class</summary>
        public abstract byte GetClassID();

        public override void PreloadContent()
        {
            PaintKiller.Inst.GetTex("GPlayer");
        }
    }
}
