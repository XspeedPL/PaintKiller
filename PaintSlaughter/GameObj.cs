using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKiller
{
    public enum Order
    {
        Midair, Effect, Boss, Player, Enemy, Eyecandy, Background, MAX
    }

    public abstract class GameObj
    {
        public const float D45 = (float)Math.PI / 4;

        internal static uint id = 0;

        public GameObj(uint id) { ID = id; }

        public GameObj(Vector2 position, short radius)
        {
            pos = position;
            hp = GetMaxHP();
            mp = GetMaxMP();
            ang = new Vector2(1, 0);
            Radius = radius;
            ID = id++;
        }

        public uint ID { get; private set; }
        public short hp { get; protected set; }
        public short mp { get; protected set; }
        public byte state, frame;
        public Vector2 pos, spd, fce, ang;
        public float dir;
        public bool dead;
        public readonly short Radius;
        protected Object tag;

        public abstract float GetAcc();

        public abstract short GetMaxHP();

        public abstract short GetMaxMP();

        public abstract float GetMaxSpd();

        public abstract Color GetColor();

        public abstract float GetWeight();

        public abstract void Kill();

        public abstract bool IsColliding();

        public abstract void Update();

        public abstract void OnDraw(SpriteBatch sb);

        public virtual short Hit(short dmg)
        {
            if (state == 3) dmg *= 2;
            short ret = Math.Min(dmg, hp);
            hp -= dmg;
            return ret;
        }

        public void SetHM(short h, short m)
        {
            hp = h; mp = m;
        }

        public virtual void OnUpdate()
        {
            Update();
            if (state == 3 && ++frame > 65) SetState(0);
            if (spd.LengthSquared() > GetMaxSpd() * GetMaxSpd())
            {
                float fx = Math.Abs(spd.X) / GetMaxSpd();
                float fy = Math.Abs(spd.Y) / GetMaxSpd();
                float f = 1 / (fx + fy);
                spd *= f;
            }
            pos += spd + fce;
            spd *= 0.81F;
            fce *= 0.81F;
            if (hp < 1) Kill();
            else if (hp > GetMaxHP()) hp = GetMaxHP();
            if (mp > GetMaxMP()) mp = GetMaxMP();
        }

        public void UpdateAngle(bool force = false)
        {
            if (force || spd.LengthSquared() > 0.2F)
            {
                ang = spd;
                ang.Normalize();
                dir = (float)Math.Atan2(ang.Y, ang.X);
            }
        }

        public bool Intersects(GameObj go)
        {
            int r = go.Radius + Radius;
            return DistanceSq(go) < r * r;
        }

        public float DistanceSq(GameObj go)
        {
            return (go.pos - pos).LengthSquared();
        }

        public void SetState(byte s, bool update = true)
        {
            state = s; frame = 0; tag = null; if (update && s != 0 && s != 3) UpdateAngle();
        }

        public void Knockback(Vector2 src, float str)
        {
            Vector2 v = pos - src;
            v.Normalize();
            fce += v * str * 2 / GetWeight();
        }

        public void DrawCentered(SpriteBatch sb, Texture2D tex, Vector2 pos, Color col, float ang, Order lay, float scale = 1)
        {
            if (state == 3)
            {
                col.R = (byte)(col.R * 3 / 4);
                col.G = (byte)(col.G * 3 / 4);
                col.B = (byte)(Math.Min(255, col.B + 60));
            }
            sb.Draw(tex, pos, null, col, ang, new Vector2(tex.Width / 2, tex.Height / 2), scale, SpriteEffects.None, (float)lay / (float)Order.MAX);
        }

        public void SetID(uint id) { ID = id; }
    }
}
