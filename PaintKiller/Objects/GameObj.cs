using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKilling.Objects
{
    public abstract class GameObj : IEquatable<GameObj>
    {
        public enum State : byte
        {
            Idle = 0, Attack, AtkAfter, Frozen, Dying, Sp1Atk, Sp1After, Sp2Atk, Sp2After
        }

        public bool IsAtkState() { return state == State.Attack || state == State.AtkAfter; }
        public bool IsSp1State() { return state == State.Sp1Atk || state == State.Sp1After; }
        public bool IsSp2State() { return state == State.Sp2Atk || state == State.Sp2After; }

        public bool Equals(GameObj obj) { return ID == obj.ID; }

        public GameObj Clone(uint id)
        {
            GameObj ret = (GameObj)MemberwiseClone();
            ret.ID = id;
            ret.CloneSpecial(this);
            return ret;
        }

        /// <summary>Incremental UID for internal use only</summary>
        internal static uint uid = 0;

        public GameObj(Vector2 position, short radius)
        {
            team = 0;
            pos = position;
            HP = GetMaxHP();
            MP = GetMaxMP();
            ang = new Vector2(1, 0);
            Radius = radius;
            ID = uid++;
        }

        /// <summary>Unique ID</summary>
        public uint ID { get; private set; }

        public short HP { get; protected set; }
        public short MP { get; protected set; }

        public State state;
        public byte frame, team;

        public Vector2 pos, spd, fce, ang;
        public float dir;

        /// <summary>Marks the object for final deletion</summary>
        public bool dead;

        /// <summary>Used for circle-based collision detection</summary>
        public readonly short Radius;

        /// <summary>Additional field for object-specific data</summary>
        protected object tag;

        /// <summary>Gets movement acceleration rate</summary>
        public abstract float GetAcc();

        public virtual float GetDaccRate() { return 0.81F; }

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

        public virtual bool CanBounce() { return false; }

        public virtual bool IsProjectile() { return false; }

        /// <summary>Updates the object state, including position, animation, etc</summary>
        public abstract void Update();

        /// <summary>Draws the current object state</summary>
        public abstract void OnDraw(SpriteBatch sb);

        /// <summary>Deals damage to this object</summary>
        /// <param name="dmg">Amount of damage</param>
        /// <returns>Actual amount of damage dealt</returns>
        public virtual short Hit(short dmg)
        {
            if (state == State.Frozen) dmg = (short)(dmg * 1.5F);
            short ret = Math.Min(dmg, HP);
            HP -= dmg;
            return ret;
        }

        /// <summary>Sets the object's health and magic points</summary>
        /// <param name="h">Health points</param>
        /// <param name="m">Magic points</param>
        internal void SetHM(short h, short m)
        {
            HP = h; MP = m;
        }

        /// <summary>Outer update function. Default implementation limits speed, health and magic points, kills the object if out of health points</summary>
        public virtual void OnUpdate()
        {
            Update();
            if (state == State.Frozen && ++frame > 65) SetState(0);
            if (spd.LengthSquared() > GetMaxSpd() * GetMaxSpd())
            {
                float fx = Math.Abs(spd.X) / GetMaxSpd();
                float fy = Math.Abs(spd.Y) / GetMaxSpd();
                float f = 1 / (fx + fy);
                spd *= f;
            }
            pos += spd + fce;
            spd *= GetDaccRate();
            fce *= GetDaccRate();
            if (HP < 1) Kill();
            else if (HP > GetMaxHP()) HP = GetMaxHP();
            if (MP > GetMaxMP()) MP = GetMaxMP();
            if (CanBounce())
            {
                if (pos.X - Radius < 0 || pos.X + Radius > PaintKiller.Inst.Width)
                {
                    spd.X = -spd.X;
                    spd *= 0.9F;
                    UpdateAngle();
                    Hit((short)(GetMaxSpd() * 2));
                }
                if (pos.Y - Radius < 0 || pos.Y + Radius > PaintKiller.Inst.Height)
                {
                    ang.Y = -ang.Y;
                    spd.Y = -spd.Y;
                    spd *= 0.9F;
                    UpdateAngle();
                    Hit((short)(GetMaxSpd() * 2));
                }
            }
            else if (!IsProjectile() || IsColliding())
            {
                if (pos.X - Radius < 0) fce.X += GetAcc() * 2;
                else if (pos.X + Radius > PaintKiller.Inst.Width) fce.X -= GetAcc() * 2;
                if (pos.Y - Radius < 0) fce.Y += GetAcc() * 2;
                else if (pos.Y + Radius > PaintKiller.Inst.Height) fce.Y -= GetAcc() * 2;
            }
        }

        /// <summary>Updates the object's orientation based on speed</summary>
        /// <param name="force">True if update should happen even if the speed isn't significant</param>
        public void UpdateAngle(bool force = false)
        {
            if (force || spd.LengthSquared() > 0.2F)
            {
                ang = Vector2.Normalize(spd);
                dir = (float)Math.Atan2(ang.Y, ang.X);
            }
        }

        public void SetAngle(Vector2 vector)
        {
            ang = vector;
            dir = (float)Math.Atan2(ang.Y, ang.X);
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
        public void SetState(State s, bool update = true)
        {
            state = s; frame = 0; tag = null; if (update && s != 0 && s != State.Frozen) UpdateAngle();
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
            if (state == State.Frozen)
            {
                col.R = (byte)(col.R * 3 / 4);
                col.G = (byte)(col.G * 3 / 4);
                col.B = (byte)(Math.Min(255, col.B + 60));
            }
            sb.DrawCentered(tex, pos, col, ang, lay, scale);
        }

        /// <summary>Applies force to two objects based on their weight and angle between them</summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        public void OnCollision(GameObj go)
        {
            Vector2 v = pos - go.pos;
            v.Normalize();
            v *= 2;
            fce += v * go.GetWeight() / GetWeight();
            go.fce -= v * GetWeight() / go.GetWeight();
        }

        /// <summary>Called when this object has dealt damage to another</summary>
        /// <param name="dmg">Amount of damage dealt</param>
        /// <param name="go">The struck enemy</param>
        public virtual void OnStrike(short dmg, GameObj go) { }
        
        protected GameObj FindClosestEnemy(GameObj intersect = null, bool colliding = true, float maxdist = float.MaxValue)
        {
            return FindClosestEnemy(intersect, colliding, ref maxdist, null);
        }

        protected GameObj FindClosestEnemy(GameObj intersect, bool colliding, float maxdist, ICollection<GameObj> ignore)
        {
            return FindClosestEnemy(intersect, colliding, ref maxdist, ignore);
        }

        protected GameObj FindClosestEnemy(GameObj intersect, bool colliding, ref float dist, ICollection<GameObj> ignore)
        {
            GameObj ret = null;
            foreach (GameObj go in PaintKiller.Inst.GetObjs())
                if (!go.IsProjectile() && go.IsEnemyOf(this) && (ignore == null || !ignore.Contains(go)) && (!colliding || go.IsColliding()) && (intersect == null || intersect.Intersects(go)))
                {
                    float ndist = DistanceSq(go);
                    if (ndist < dist)
                    {
                        dist = ndist;
                        ret = go;
                    }
                }
            return ret;
        }

        public virtual bool IsEnemyOf(GameObj attacker)
        {
            return team != 0 && team != attacker.team;
        }

        public abstract void PreloadContent();

        public virtual void CloneSpecial(GameObj src) { }

        public virtual void ReadSpecial(BinaryReader br) { }

        public virtual void WriteSpecial(BinaryWriter bw) { }
    }
}
