using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKiller
{
    /// <summary>Display order enum</summary>
    public enum Order
    {
        Midair, Effect, Boss, Player, Enemy, Eyecandy, Background, MAX
    }

    public abstract class GameObj
    {
        public const float D45 = (float)Math.PI / 4;

        /// <summary>Incremental UID for internal use only</summary>
        internal static uint id = 0;

        /// <summary>Client-side only constructor</summary>
        /// <param name="id">Assigned ID</param>
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

        /// <summary>Unique ID</summary>
        public readonly uint ID;

        public short hp { get; protected set; }
        public short mp { get; protected set; }

        public byte state, frame;

        public Vector2 pos, spd, fce, ang;
        public float dir;

        /// <summary>Marks the object for final deletion</summary>
        public bool dead;

        /// <summary>Used for circle-based collision detection</summary>
        public readonly short Radius;

        /// <summary>Additional field for object-specific data</summary>
        protected Object tag;

        /// <summary>Gets movement acceleration rate</summary>
        public abstract float GetAcc();

        /// <summary>Gets maximum amount of health points</summary>
        public abstract short GetMaxHP();

        /// <summary>Gets maximum amount of magic points</summary>
        public abstract short GetMaxMP();

        /// <summary>Gets maximum movement speed</summary>
        public abstract float GetMaxSpd();

        /// <summary>Gets the color used for dying the circle sprite</summary>
        public abstract Color GetColor();

        /// <summary>Gets the object's weight used in collisions and knockbacks</summary>
        public abstract float GetWeight();

        /// <summary>Requests the removal of this object</summary>
        public abstract void Kill();

        /// <summary>Used to determine whenever this object is collidable at the moment</summary>
        public abstract bool IsColliding();

        /// <summary>Updates the object state, including position, animation, etc</summary>
        public abstract void Update();

        /// <summary>Draws the current object state</summary>
        public abstract void OnDraw(SpriteBatch sb);

        /// <summary>Deals damage to this object</summary>
        /// <param name="dmg">Amount of damage</param>
        /// <returns>Actual amount of damage dealt</returns>
        public virtual short Hit(short dmg)
        {
            if (state == 3) dmg *= 2;
            short ret = Math.Min(dmg, hp);
            hp -= dmg;
            return ret;
        }

        /// <summary>Sets the object's health and magic points</summary>
        /// <param name="h">Health points</param>
        /// <param name="m">Magic points</param>
        public void SetHM(short h, short m)
        {
            hp = h; mp = m;
        }

        /// <summary>Outer update function. Default implementation limits speed, health and magic points, kills the object if out of health points</summary>
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

        /// <summary>Updates the object's orientation based on speed</summary>
        /// <param name="force">True if update should happen even if the speed isn't significant</param>
        public void UpdateAngle(bool force = false)
        {
            if (force || spd.LengthSquared() > 0.2F)
            {
                ang = spd;
                ang.Normalize();
                dir = (float)Math.Atan2(ang.Y, ang.X);
            }
        }

        /// <summary>Determines whenever this object intersects with another</summary>
        /// <param name="go">The another object</param>
        public bool Intersects(GameObj go)
        {
            int r = go.Radius + Radius;
            return DistanceSq(go) < r * r;
        }

        /// <summary>Checks distance to another object</summary>
        /// <param name="go">The another object</param>
        /// <returns>The squared distance</returns>
        public float DistanceSq(GameObj go)
        {
            return (go.pos - pos).LengthSquared();
        }

        /// <summary>Sets the object animation state</summary>
        /// <param name="s">New state identifier</param>
        /// <param name="update">Whenever the orientation should be updated</param>
        public void SetState(byte s, bool update = true)
        {
            state = s; frame = 0; tag = null; if (update && s != 0 && s != 3) UpdateAngle();
        }

        /// <summary>Applies force to the object from a specific position</summary>
        /// <param name="src">Source position</param>
        /// <param name="str">Strength</param>
        public void Knockback(Vector2 src, float str)
        {
            Vector2 v = pos - src;
            v.Normalize();
            fce += v * str * 2 / GetWeight();
        }

        /// <summary>Draws a texture centered at a specified position</summary>
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
    }
}
