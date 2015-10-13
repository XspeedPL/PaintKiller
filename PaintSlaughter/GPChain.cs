using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintKiller
{
    public class GPChain : GProjectile
    {
        private LightningBolt bolt;
        private readonly GEnemy tar;

        public GPChain(uint id) : base(id) { }

        public GPChain(Vector2 position, GPlayer shoot, GEnemy target, List<GEnemy> list) : base(position, 14, Vector2.Zero, shoot)
        {
            tar = target;
            list.Add(tar);
            tag = list;
            bolt = new LightningBolt(pos, tar.pos);
        }

        internal void SetBolt(Vector2 target) { bolt = new LightningBolt(pos, target); }

        internal Vector2 GetBoltDest() { return bolt.End; }

        public override float GetAcc() { return 0; }

        public override float GetMaxSpd() { return 0; }

        public override short GetMaxHP() { return 20; }

        public override short GetMaxMP() { return 0; }

        public override Color GetColor() { return Color.White; }

        public override float GetWeight() { return 5; }

        public override void Kill() { dead = true; }

        public override void OnDraw(SpriteBatch sb)
        {
            bolt.Draw(sb, (float)hp / GetMaxHP());
        }

        public override void Update()
        {
            base.Update();
            SetBolt(tar.pos);
            if (++frame == 2)
            {
                shooter.OnStrike(tar.Hit(16), tar);
                PaintKiller.AddObj(new GEC(tar.pos, PaintKiller.GetTex("BloodS"), 10));
            }
            else if (frame == 6 && ((List<GEnemy>)tag).Count < 8)
            {
                GEnemy gt = null;
                float dist = 175 * 175, d2;
                foreach (GEnemy g in PaintKiller.GetEnes())
                    if (!((List<GEnemy>)tag).Contains(g) && g.IsColliding())
                    {
                        d2 = (g.pos - tar.pos).LengthSquared();
                        if (d2 < dist)
                        {
                            gt = g; dist = d2;
                        }
                    }
                if (gt != null) PaintKiller.AddObj(new GPChain(tar.pos, shooter, gt, (List<GEnemy>)tag));
            }
            else if (frame == 11) tar.Knockback(pos, 4);
        }

        public override bool CanBounce() { return false; }

        private class LightningBolt
        {
            public List<Line> Segments = new List<Line>();
            public Vector2 Start { get { return Segments[0].A; } }
            public Vector2 End { get { return Segments[Segments.Count - 1].B; } }
            public float FadeOutRate { get; set; }
            public Color Tint { get; set; }

            static Random rand = new Random();

            public LightningBolt(Vector2 source, Vector2 dest) : this(source, dest, new Color(0.45f, 0.4f, 1f)) { }

            public LightningBolt(Vector2 source, Vector2 dest, Color color)
            {
                Segments = CreateBolt(source, dest, 2);
                Tint = color;
                FadeOutRate = 0.03f;
            }

            public void Draw(SpriteBatch spriteBatch, float alpha)
            {
                foreach (var segment in Segments)
                    segment.Draw(spriteBatch, Tint * alpha);
            }

            private static List<Line> CreateBolt(Vector2 source, Vector2 dest, float thickness)
            {
                var results = new List<Line>();
                Vector2 tangent = dest - source;
                Vector2 normal = Vector2.Normalize(new Vector2(tangent.Y, -tangent.X));
                float length = tangent.Length();

                List<float> positions = new List<float>();
                positions.Add(0);

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

            // Returns the point where the bolt is at a given fraction of the way through the bolt. Passing
            // zero will return the start of the bolt, and passing 1 will return the end.
            public Vector2 GetPoint(float position)
            {
                var start = Start;
                float length = Vector2.Distance(start, End);
                Vector2 dir = (End - start) / length;
                position *= length;

                var line = Segments.Find(x => Vector2.Dot(x.B - start, dir) >= position);
                float lineStartPos = Vector2.Dot(line.A - start, dir);
                float lineEndPos = Vector2.Dot(line.B - start, dir);
                float linePos = (position - lineStartPos) / (lineEndPos - lineStartPos);

                return Vector2.Lerp(line.A, line.B, linePos);
            }

            private static float Rand(float min, float max)
            {
                return (float)rand.NextDouble() * (max - min) + min;
            }

            private static float Square(float x)
            {
                return x * x;
            }

            public class Line
            {
                public Vector2 A;
                public Vector2 B;
                public float Thickness;

                public Line() { }
                public Line(Vector2 a, Vector2 b, float thickness = 1)
                {
                    A = a;
                    B = b;
                    Thickness = thickness;
                }

                public void Draw(SpriteBatch spriteBatch, Color tint)
                {
                    Texture2D t1 = PaintKiller.GetTex("GPMageS1"), t2 = PaintKiller.GetTex("GPMageS2");

                    Vector2 tangent = B - A;
                    float theta = (float)Math.Atan2(tangent.Y, tangent.X);

                    const float ImageThickness = 8;
                    float thicknessScale = Thickness / ImageThickness;

                    Vector2 capOrigin = new Vector2(t1.Width, t1.Height / 2f);
                    Vector2 middleOrigin = new Vector2(0, t2.Height / 2f);
                    Vector2 middleScale = new Vector2(tangent.Length(), thicknessScale);

                    spriteBatch.Draw(t2, A, null, tint, theta, middleOrigin, middleScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(t1, A, null, tint, theta, capOrigin, thicknessScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(t1, B, null, tint, theta + MathHelper.Pi, capOrigin, thicknessScale, SpriteEffects.None, 0f);
                }
            }
        }
    }
}
