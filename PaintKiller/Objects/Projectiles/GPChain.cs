using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKilling.Objects.Projectiles
{
    public class GPChain : GProjectile
    {
        private GameObj Target { get; }

        private LightningBolt bolt;

        public GPChain(Vector2 position, GameObj shoot, GameObj target, ICollection<GameObj> list) : base(position, 14, Vector2.Zero, shoot)
        {
            MP = (short)list.Count;
            Target = target;
            list.Add(Target);
            tag = list;
            SetBolt(Target.pos);
        }

        internal void SetBolt(Vector2 target) { bolt = new LightningBolt(pos, target, 2 - MP / 8F); }

        public override float GetAcc() { return 0; }

        public override float GetMaxSpd() { return 0; }

        public override short GetMaxHP() { return 20; }

        public override Color GetColor() { return new Color(0.45f, 0.4f, 1f); }

        public override float GetWeight() { return 5; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            bolt.Draw(sb, GetColor() * ((float)HP / GetMaxHP()));
        }

        public override void PreloadContent()
        {
            PaintKiller.Inst.GetTex("GPMageS1");
            PaintKiller.Inst.GetTex("GPMageS2");
        }

        public override void Update()
        {
            base.Update();
            SetBolt(Target.pos);
            if (++frame == 2)
            {
                Shooter.OnStrike(Target.Hit(16), Target);
                PaintKiller.Inst.AddBlood(this, Target);
            }
            else
            {
                ICollection<GameObj> list = (ICollection<GameObj>)tag;
                if (frame == 6 && list.Count < 8)
                {
                    GameObj go = FindClosestEnemy(null, true, 175 * 175, list);
                    if (go != null) PaintKiller.Inst.AddObj(new GPChain(Target.pos, Shooter, go, list));
                }
                else if (frame == 11) Target.Knockback(pos, 4);
            }
        }

        public override void CloneSpecial(GameObj src)
        {
            SetBolt(((GPChain)src).bolt.End);
        }

        public override void ReadSpecial(BinaryReader reader)
        {
            SetBolt(new Vector2(reader.ReadSingle(), reader.ReadSingle()));
        }

        public override void WriteSpecial(BinaryWriter writer)
        {
            writer.Write(bolt.End.X);
            writer.Write(bolt.End.Y);
        }

        private class LightningBolt
        {
            public List<Line> Segments = new List<Line>();

            public Vector2 End { get { return Segments[Segments.Count - 1].B; } }

            public LightningBolt(Vector2 source, Vector2 dest, float thick)
            {
                Segments = CreateBolt(source, dest, thick);
            }

            public void Draw(SpriteBatch sb, Color color)
            {
                foreach (Line segment in Segments)
                    segment.Draw(sb, color);
            }

            private static List<Line> CreateBolt(Vector2 source, Vector2 dest, float thickness)
            {
                List<Line> results = new List<Line>();
                Vector2 tangent = dest - source;
                Vector2 normal = Vector2.Normalize(new Vector2(tangent.Y, -tangent.X));
                float length = tangent.Length();

                List<float> positions = new List<float> { 0 };

                for (int i = 0; i < length / 4; i++)
                    positions.Add(Rand(0, 1));

                positions.Sort();

                const float Sway = 80;
                const float Jaggedness = 1 / Sway;

                Vector2 prevPoint = source;
                float prevDisplacement = 0;
                for (int i = 1; i < positions.Count; i++)
                {
                    float pos = positions[i];

                    // used to prevent sharp angles by ensuring very close positions also have small perpendicular variation.
                    float scale = (length * Jaggedness) * (pos - positions[i - 1]);

                    // defines an envelope. Points near the middle of the bolt can be further from the central line.
                    float envelope = pos > 0.95f ? 20 * (1 - pos) : 1;

                    float displacement = Rand(-Sway, Sway);
                    displacement -= (displacement - prevDisplacement) * (1 - scale);
                    displacement *= envelope;

                    Vector2 point = source + pos * tangent + displacement * normal;
                    results.Add(new Line(prevPoint, point, thickness));
                    prevPoint = point;
                    prevDisplacement = displacement;
                }

                results.Add(new Line(prevPoint, dest, thickness));

                return results;
            }
            
            private static float Rand(float min, float max)
            {
                return (float)PaintKiller.Rand.NextDouble() * (max - min) + min;
            }

            public struct Line
            {
                public readonly Vector2 A;
                public readonly Vector2 B;
                public readonly float Thick;
                
                public Line(Vector2 a, Vector2 b, float thick)
                {
                    A = a;
                    B = b;
                    Thick = thick;
                }

                public void Draw(SpriteBatch sb, Color tint)
                {
                    Texture2D t1 = PaintKiller.Inst.GetTex("GPMageS1"), t2 = PaintKiller.Inst.GetTex("GPMageS2");

                    Vector2 tangent = B - A;
                    float theta = (float)Math.Atan2(tangent.Y, tangent.X);

                    const float ImageThickness = 8;
                    float thicknessScale = Thick / ImageThickness;

                    Vector2 capOrigin = new Vector2(t1.Width, t1.Height / 2f);
                    Vector2 middleOrigin = new Vector2(0, t2.Height / 2f);
                    Vector2 middleScale = new Vector2(tangent.Length(), thicknessScale);

                    float depth = (float)Order.Effect / (float)Order.Max;
                    sb.Draw(t2, A, null, tint, theta, middleOrigin, middleScale, SpriteEffects.None, depth);
                    sb.Draw(t1, A, null, tint, theta, capOrigin, thicknessScale, SpriteEffects.None, depth);
                    sb.Draw(t1, B, null, tint, theta + MathHelper.Pi, capOrigin, thicknessScale, SpriteEffects.None, depth);
                }
            }
        }
    }
}
