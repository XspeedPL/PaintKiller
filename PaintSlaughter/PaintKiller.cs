using System;
using System.Net;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input;
using Nuclex.Input.Devices;

namespace PaintKiller
{
    internal sealed class PaintKiller : Game
    {
        internal enum State : byte
        {
            Main, Prefs, SP, MPHost, MPJoin, Game, Paused
        }

        internal sealed class EntityList : List<GameObj>
        {
            private LinkedList<GPlayer> gplist = null;
            private LinkedList<GEnemy> gelist = null;

            new public void Add(GameObj go)
            {
                if (!Contains(go))
                {
                    if (go is GPlayer) gplist = null;
                    else if (go is GEnemy) gelist = null;
                    base.Add(go);
                }
            }

            new public void Remove(GameObj go)
            {
                if (Contains(go))
                {
                    if (go is GPlayer) gplist = null;
                    else if (go is GEnemy) gelist = null;
                    base.Remove(go);
                }
            }

            public LinkedList<GPlayer> PlrList()
            {
                if (gplist == null)
                {
                    LinkedList<GPlayer> ret = new LinkedList<GPlayer>();
                    for (int i = Count - 1; i >= 0; --i) if (this[i] is GPlayer) ret.AddLast((GPlayer)this[i]);
                    return gplist = ret;
                }
                else return gplist;
            }

            public LinkedList<GEnemy> EneList()
            {
                if (gelist == null)
                {
                    LinkedList<GEnemy> ret = new LinkedList<GEnemy>();
                    for (int i = Count - 1; i >= 0; --i) if (this[i] is GEnemy) ret.AddLast((GEnemy)this[i]);
                    return gelist = ret;
                }
                else return gelist;
            }

            public GameObj GetByID(uint id)
            {
                for (int i = Count - 1; i >= 0; --i) if (this[i].ID == id) return this[i];
                return null;
            }
        }

        private static readonly Dictionary<string, Texture2D> texs = new Dictionary<string, Texture2D>();
        public static readonly Random rand = new Random();
        private static EntityList objs = new EntityList();
        private static readonly LinkedList<GameObj> queue = new LinkedList<GameObj>();
        public static SpriteFont Font;
        private GraphicsDeviceManager graphics;
        private SpriteBatch sb;
        private InputManager input;
        private static KeyboardState prev = Keyboard.GetState();
        private byte tab = 1, side = 0, spawn = 1;
        internal static State state = State.Main;
        private string ip = "";
        private NetServer server;
        private NetClient client;
        private IGamePad pad = null;
        internal static PaintKiller Inst;

        public int Width { get { return GraphicsDevice.Viewport.Width; } }
        public int Height { get { return GraphicsDevice.Viewport.Height; } }

        public static readonly GPlayer[] P = new GPlayer[4];

        public static KeyboardState PrevKeys { get { return prev; } }

        public PaintKiller() : base()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 800;
            input = new InputManager(Services, Window.Handle);
            Components.Add(input);
            Content.RootDirectory = "Content";
            Inst = this;
        }

        public static IEnumerable<GameObj> GetObjs() { return objs; }

        public static void AddObj(GameObj go) { queue.AddLast(go); }

        public static GameObj GetObj(uint id) { return objs.GetByID(id); }

        internal static GPlayer GetPlr(IPEndPoint ep)
        {
            for (int i = 0; i < 4; ++i) if (P[i] != null && P[i].ep.Equals(ep)) return P[i];
            return null;
        }

        public static Texture2D GetTex(string txid)
        {
            return texs.ContainsKey(txid) ? texs[txid] : null;
        }

        public static LinkedList<GPlayer> GetPlrs() { return objs.PlrList(); }

        public static LinkedList<GEnemy> GetEnes() { return objs.EneList(); }

        protected override void Initialize()
        {
            base.Initialize();
            MathHelp.Init();
            IsFixedTimeStep = true;
            TargetElapsedTime = new TimeSpan(200000);
        }

        internal static void NewList(EntityList list)
        {
            objs = list;
        }

        private void AddTex(string tex)
        {
            Texture2D t = Content.Load<Texture2D>(tex);
            t.Name = tex;
            texs.Add(tex, t);
        }

        protected override void LoadContent()
        {
            sb = new SpriteBatch(GraphicsDevice);
            Font = Content.Load<SpriteFont>("Font");
            AddTex("GPlayer");
            AddTex("GPOrcW1");
            AddTex("GPOrcW2");
            AddTex("GPOrcW3");
            AddTex("GPOrcS1");
            AddTex("GPOrcS2");
            AddTex("GPMageS");
            AddTex("GPMageS1");
            AddTex("GPMageS2");
            AddTex("GPArchW1");
            AddTex("GPArchW2");
            AddTex("GPMageW");
            AddTex("GPMageW1");
            AddTex("GEnemy");
            AddTex("GEnemyW");
            AddTex("GEKnight");
            AddTex("GEKnightW");
            AddTex("GEnemyD1");
            AddTex("GEnemyD2");
            AddTex("MenuPause");
            AddTex("BloodS");
            AddTex("Arrow");
            AddTex("ArrowD");
            AddTex("ArrowU");
            AddTex("Bar1");
            AddTex("Bar2");
        }

        protected override void UnloadContent()
        {
            if (server != null)
            {
                server.SendEnd();
                server.Dispose();
            }
            if (client != null)
            {
                client.SendEnd();
                client.Dispose();
            }
        }

        internal void Return()
        {
            if (server != null)
            {
                server.SendEnd();
                server.Dispose();
                server = null;
            }
            if (client != null)
            {
                client.SendEnd();
                client.Dispose();
                client = null;
            }
            tab = 1;
            ip = "";
            for (byte i = 0; i < 4; ++i) P[i] = null;
            state = State.Main;
            NewList(new EntityList());
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Escape) && prev.IsKeyUp(Keys.Escape))
                switch (state)
                {
                    case State.Main:
                    case State.Prefs:
                    case State.MPHost:
                    case State.MPJoin:
                    case State.SP: Exit(); break;
                    case State.Game:
                        if (side != 2)
                        {
                            if (side == 1) server.SendGame(true);
                            state = State.Paused;
                        }
                        break;
                    case State.Paused:
                        if (side != 2)
                        {
                            if (side == 1) server.SendGame(false);
                            state = State.Game;
                        }
                        break;
                }
            else if (ks.IsKeyDown(Keys.F1) && prev.IsKeyUp(Keys.F1)) Return();
            if (state == State.Main)
            {
                if (ks.IsKeyDown(Keys.Up) && prev.IsKeyUp(Keys.Up) && --tab < 1) tab = 4;
                else if (ks.IsKeyDown(Keys.Down) && prev.IsKeyUp(Keys.Down) && ++tab > 4) tab = 1;
                else if (ks.IsKeyDown(Keys.Enter) && prev.IsKeyUp(Keys.Enter))
                {
                    if (tab == 4) state = State.Prefs;
                    else
                    {
                        state = State.SP;
                        side = (byte)(tab - 1);
                        tab = 1;
                    }
                }
            }
            else if (state == State.Prefs)
            {
                if (ks.IsKeyDown(Keys.Left) && pad != null) pad = null;
                else if (ks.IsKeyDown(Keys.Right) && pad == null)
                    foreach (ExtendedPlayerIndex pi in Enum.GetValues(typeof(ExtendedPlayerIndex)))
                    {
                        pad = input.GetGamePad(pi);
                        if (pad.IsAttached) break;
                        else pad = null;
                    }
            }
            else if (state == State.SP)
            {
                if (ks.IsKeyDown(Keys.Left) && prev.IsKeyUp(Keys.Left) && --tab < 1) tab = 3;
                else if (ks.IsKeyDown(Keys.Right) && prev.IsKeyUp(Keys.Right) && ++tab > 3) tab = 1;
                else if (ks.IsKeyDown(Keys.Enter) && prev.IsKeyUp(Keys.Enter))
                {
                    if (side < 2)
                    {
                        GPlayer gp;
                        if (tab == 1) objs.Add(gp = new GPArch(new Vector2(50, 150)));
                        else if (tab == 2) objs.Add(gp = new GPMage(new Vector2(50, 150)));
                        else objs.Add(gp = new GPOrc(new Vector2(50, 150)));
                        P[0] = gp;
                        P[0].ep = new IPEndPoint(IPAddress.Loopback, Net.port);
                    }
                    if (side == 0) state = State.Game;
                    else if (side == 1) state = State.MPHost;
                    else
                    {
                        GPlayer gp;
                        if (tab == 1) gp = new GPArch(new Vector2(50, 150));
                        else if (tab == 2) gp = new GPMage(new Vector2(50, 150));
                        else gp = new GPOrc(new Vector2(50, 150));
                        P[0] = gp;
                        P[0].ep = new IPEndPoint(IPAddress.Loopback, Net.port);
                        state = State.MPJoin;
                    }
                }
            }
            else if (state == State.MPHost && side == 1)
            {
                if (server == null) server = new NetServer();
                else if (ks.IsKeyDown(Keys.Enter) && prev.IsKeyUp(Keys.Enter))
                {
                    server.SendPlrList();
                    server.SendGame(false);
                    for (byte i = 1; i < 4; ++i)
                        if (P[i] != null) objs.Add(P[i]);
                    state = State.Game;
                }
            }
            else if (state == State.MPJoin)
            {
                if (client == null)
                {
                    if (ks.IsKeyDown(Keys.NumPad0) && prev.IsKeyUp(Keys.NumPad0)) ip += '0';
                    else if (ks.IsKeyDown(Keys.NumPad1) && prev.IsKeyUp(Keys.NumPad1)) ip += '1';
                    else if (ks.IsKeyDown(Keys.NumPad2) && prev.IsKeyUp(Keys.NumPad2)) ip += '2';
                    else if (ks.IsKeyDown(Keys.NumPad3) && prev.IsKeyUp(Keys.NumPad3)) ip += '3';
                    else if (ks.IsKeyDown(Keys.NumPad4) && prev.IsKeyUp(Keys.NumPad4)) ip += '4';
                    else if (ks.IsKeyDown(Keys.NumPad5) && prev.IsKeyUp(Keys.NumPad5)) ip += '5';
                    else if (ks.IsKeyDown(Keys.NumPad6) && prev.IsKeyUp(Keys.NumPad6)) ip += '6';
                    else if (ks.IsKeyDown(Keys.NumPad7) && prev.IsKeyUp(Keys.NumPad7)) ip += '7';
                    else if (ks.IsKeyDown(Keys.NumPad8) && prev.IsKeyUp(Keys.NumPad8)) ip += '8';
                    else if (ks.IsKeyDown(Keys.NumPad9) && prev.IsKeyUp(Keys.NumPad9)) ip += '9';
                    else if (ks.IsKeyDown(Keys.D0) && prev.IsKeyUp(Keys.D0)) ip += '0';
                    else if (ks.IsKeyDown(Keys.D1) && prev.IsKeyUp(Keys.D1)) ip += '1';
                    else if (ks.IsKeyDown(Keys.D2) && prev.IsKeyUp(Keys.D2)) ip += '2';
                    else if (ks.IsKeyDown(Keys.D3) && prev.IsKeyUp(Keys.D3)) ip += '3';
                    else if (ks.IsKeyDown(Keys.D4) && prev.IsKeyUp(Keys.D4)) ip += '4';
                    else if (ks.IsKeyDown(Keys.D5) && prev.IsKeyUp(Keys.D5)) ip += '5';
                    else if (ks.IsKeyDown(Keys.D6) && prev.IsKeyUp(Keys.D6)) ip += '6';
                    else if (ks.IsKeyDown(Keys.D7) && prev.IsKeyUp(Keys.D7)) ip += '7';
                    else if (ks.IsKeyDown(Keys.D8) && prev.IsKeyUp(Keys.D8)) ip += '8';
                    else if (ks.IsKeyDown(Keys.D9) && prev.IsKeyUp(Keys.D9)) ip += '9';
                    else if (ks.IsKeyDown(Keys.OemPeriod) && prev.IsKeyUp(Keys.OemPeriod)) ip += '.';
                    else if (ks.IsKeyDown(Keys.Back) && prev.IsKeyUp(Keys.Back) && ip.Length > 0) ip = ip.Remove(ip.Length - 1);
                    else if (ks.IsKeyDown(Keys.Delete) && prev.IsKeyUp(Keys.Delete)) ip = "";
                    else if (ks.IsKeyDown(Keys.Enter) && prev.IsKeyUp(Keys.Enter))
                    {
                        IPAddress ipa;
                        if (IPAddress.TryParse(ip, out ipa))
                            client = new NetClient(ipa, P[0].GetClassID());
                        else ip = "";
                    }
                }
            }
            else if (state == State.Game)
            {
                if (side == 2) client.SendKeys(pad == null ? GetCtrlState(ks) : GetCtrlState(pad.GetExtendedState()));
                else
                {
                    int enes = objs.EneList().Count;
                    if (enes < 1 || --spawn < 1)
                    {
                        spawn = 100;
                        int plrs = server == null ? 1 : server.ccount;
                        for (int i = enes; i < 10 + 4 * plrs; ++i)
                            if (rand.Next(10) == 0)
                                objs.Add(new GEKnight(new Vector2(rand.Next(Width), rand.Next(2) * Height)));
                            else if (rand.Next(4) == 0)
                                objs.Add(new GEArch(new Vector2(rand.Next(Width), rand.Next(2) * Height)));
                            else objs.Add(new GEnemy(new Vector2(rand.Next(Width), rand.Next(2) * Height)));
                    }
                    if (side != 2) P[0].keys = pad == null ? GetCtrlState(ks) : GetCtrlState(pad.GetExtendedState());
                    LinkedList<GameObj> tmp = new LinkedList<GameObj>(objs), del = new LinkedList<GameObj>();
                    foreach (GameObj go in objs)
                    {
                        go.OnUpdate();
                        tmp.Remove(go);
                        if (go.dead) del.AddLast(go);
                        else if (go.IsColliding())
                            foreach (GameObj gn in tmp)
                                if (gn.IsColliding() && go.Intersects(gn)) Collis.Handle(go, gn);
                        if (!(go is GProjectile))
                        {
                            if (go.pos.X - go.Radius < 0) go.fce.X += go.GetAcc() * 2;
                            else if (go.pos.X + go.Radius > Width) go.fce.X -= go.GetAcc() * 2;
                            if (go.pos.Y - go.Radius < 0) go.fce.Y += go.GetAcc() * 2;
                            else if (go.pos.Y + go.Radius > Height) go.fce.Y -= go.GetAcc() * 2;
                        }
                        else if (((GProjectile)go).CanBounce())
                        {
                            if (go.pos.X - go.Radius < 0 || go.pos.X + go.Radius > Width)
                            {
                                go.spd.X = -go.spd.X;
                                go.spd *= 0.9F;
                                go.UpdateAngle();
                                go.Hit((short)(go.GetMaxSpd() * 2));
                            }
                            if (go.pos.Y - go.Radius < 0 || go.pos.Y + go.Radius > Height)
                            {
                                go.ang.Y = -go.ang.Y;
                                go.spd.Y = -go.spd.Y;
                                go.spd *= 0.9F;
                                go.UpdateAngle();
                                go.Hit((short)(go.GetMaxSpd() * 2));
                            }
                        }
                    }
                    foreach (GameObj go in del) objs.Remove(go);
                }
                while (queue.Last != null) { objs.Add(queue.First.Value); queue.RemoveFirst(); }
                if (side == 1) server.SendObjs(objs);
            }
            base.Update(gameTime);
            prev = ks;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);
            sb.Begin();
            if (state == State.Main)
            {
                DrawOutString("Choose gamemode:", Width / 8, Height / 6, Color.Yellow, Color.Black, 2, 1.5F);
                DrawOutString("Singleplayer", Width / 6, Height / 3, Color.White, tab == 1 ? Color.Blue : Color.Black, 2, 1);
                DrawOutString("Multiplayer host", Width / 6, Height / 3 + 40, Color.White, tab == 2 ? Color.Blue : Color.Black, 2, 1);
                DrawOutString("Multiplayer join", Width / 6, Height / 3 + 80, Color.White, tab == 3 ? Color.Blue : Color.Black, 2, 1);
                DrawOutString("Settings", Width / 6, Height / 3 + 120, Color.White, tab == 4 ? Color.Blue : Color.Black, 2, 1);
                DrawOutString("Enter - Accept", 15, Height - 20, Color.White, Color.Black, 2, 0.8F);
            }
            else if (state == State.Prefs)
            {
                DrawOutString("Gameplay settings:", Width / 8, Height / 6, Color.Yellow, Color.Black, 2, 1.5F);
                DrawOutString("Input device", Width / 6, Height / 3, Color.White, Color.Black, 2, 1.1F);
                DrawOutString("Keyboard", Width / 4, Height / 2, Color.White, pad == null ? Color.Blue : Color.Black, 2, 0.9F);
                DrawOutString("Gamepad", Width / 2, Height / 2, Color.White, pad == null ? Color.Black : Color.Blue, 2, 0.9F);
                DrawOutString("F1 - Quit to main menu", 15, Height - 20, Color.White, Color.Black, 2, 0.8F);
            }
            else if (state == State.SP)
            {
                DrawOutString("Choose character:", Width / 8, Height / 5, Color.Yellow, Color.Black, 2, 1.5F);
                Texture2D gp = GetTex("GPlayer");
                DrawOut(gp, Width * tab / 4, Height / 2, Color.Black, 4, 2);
                Draw(gp, new Vector2(Width / 4, Height / 2), Color.Yellow, 2);
                Draw(gp, new Vector2(Width / 2, Height / 2), Color.Blue, 2);
                Draw(gp, new Vector2(Width * 3 / 4, Height / 2), Color.DarkGreen, 2);
                DrawOutString("Archer    Mage      Orc", Width / 4 - 78, Height / 2 + 48, Color.White, Color.Black, 2, 1.75F);
                DrawOutString("Enter - Accept, F1 - Quit to main menu", 15, Height - 20, Color.White, Color.Black, 2, 0.8F);
            }
            else if (state == State.MPHost)
            {
                DrawOutString("Multiplayer lobby", Width / 8, Height / 8, Color.Yellow, Color.Black, 2, 1.5F);
                DrawOutString("Waiting for players...", Width / 8, Height / 5, Color.White, Color.Black, 1, 1);
                for (int i = 0; i < 4; ++i)
                    if (P[i] != null)
                        if (side == 2)
                            DrawOutString(P[i].GetClassName() + (P[i].ID == client.cID ? " (You)" : ""), Width / 6, Height * (3 + i) / 10, Color.White, Color.Black, 1, 0.75F);
                        else DrawOutString(P[i].ep.Address.ToString() + " (" + P[i].GetClassName() + ")", Width / 6, Height * (3 + i) / 10, Color.White, Color.Black, 1, 0.75F);
                if (side == 1)
                {
                    DrawOutString("Start game", Width / 3, Height * 5 / 6, Color.Blue, Color.Black, 2, 1.3F);
                    DrawOutString("Enter - Start game, F1 - Quit to main menu", 15, Height - 20, Color.White, Color.Black, 2, 0.8F);
                }
            }
            else if (state == State.MPJoin)
            {
                DrawOutString("Multiplayer lobby", Width / 8, Height / 8, Color.Yellow, Color.Black, 2, 1.5F);
                if (client == null)
                {
                    DrawOutString("Enter remote address", Width / 8, Height / 5, Color.White, Color.Black, 1, 1);
                    DrawOutString("IP: " + ip + " _", Width / 7, Height / 2, Color.Blue, Color.Black, 1, 1);
                }
                else DrawOutString("Connecting to " + ip + " ...", Width / 8, Height / 5, Color.White, Color.Black, 1, 1);
                DrawOutString("Delete - Clear text, F1 - Quit to main menu", 15, Height - 20, Color.White, Color.Black, 2, 0.8F);
            }
            else if (state == State.Game || state == State.Paused)
            {
                foreach (GameObj go in objs) go.OnDraw(sb);
                Texture2D t1 = GetTex("Bar1"), t2 = GetTex("Bar2");
                int scale;
                if (P[0] != null)
                {
                    sb.Draw(t1, new Vector2(5, Height - t1.Height - 5), Color.White);
                    scale = t2.Height * P[0].hp / P[0].GetMaxHP();
                    sb.Draw(t2, new Rectangle(8, Height - 8 - scale, t2.Width, scale), new Rectangle(0, t2.Height - scale, t2.Width, scale), Color.Red * (side < 2 ? 0.8F : 0.4F));
                    sb.Draw(t1, new Vector2(10 + t1.Width, Height - t1.Height - 5), Color.White);
                    scale = t2.Height * P[0].mp / P[0].GetMaxMP();
                    sb.Draw(t2, new Rectangle(13 + t1.Width, Height - 8 - scale, t2.Width, scale), new Rectangle(0, t2.Height - scale, t2.Width, scale), Color.Blue * (side < 2 ? 0.8F : 0.4F));
                    DrawOutString(P[0].score.ToString(), 21 + t1.Width + t2.Width, Height - 8 - t2.Height / 6, Color.LightGray, Color.Black, 2, 0.75F);
                }
                if (P[1] != null)
                {
                    sb.Draw(t1, new Vector2(Width - t1.Width - 5, Height - t1.Height - 5), Color.White);
                    scale = t2.Height * P[1].hp / P[1].GetMaxHP();
                    sb.Draw(t2, new Rectangle(Width - t2.Width - 8, Height - 8 - scale, t2.Width, scale), new Rectangle(0, t2.Height - scale, t2.Width, scale), Color.Red * (side == 2 && client.cID == P[1].ID ? 0.8F : 0.4F));
                    sb.Draw(t1, new Vector2(Width - t1.Width - t1.Width - 10, Height - t1.Height - 5), Color.White);
                    scale = t2.Height * P[1].mp / P[1].GetMaxMP();
                    sb.Draw(t2, new Rectangle(Width - t2.Width - t1.Width - 13, Height - 8 - scale, t2.Width, scale), new Rectangle(0, t2.Height - scale, t2.Width, scale), Color.Blue * (side == 2 && client.cID == P[1].ID ? 0.8F : 0.4F));
                    DrawOutString(P[1].score.ToString(), Width - 21 - t1.Width - t2.Width - Font.MeasureString(P[1].score.ToString()).X, Height - 8 - t2.Height / 6, Color.LightGray, Color.Black, 2, 0.75F);
                }
                if (P[2] != null)
                {
                    sb.Draw(t1, new Vector2(5, 5), Color.White);
                    scale = t2.Height * P[2].hp / P[2].GetMaxHP();
                    sb.Draw(t2, new Rectangle(8, 8, t2.Width, scale), new Rectangle(0, t2.Height - scale, t2.Width, scale), Color.Red * (side == 2 && client.cID == P[2].ID ? 0.8F : 0.4F));
                    sb.Draw(t1, new Vector2(10 + t1.Width, 5), Color.White);
                    scale = t2.Height * P[2].mp / P[2].GetMaxMP();
                    sb.Draw(t2, new Rectangle(13 + t1.Width, 8, t2.Width, scale), new Rectangle(0, t2.Height - scale, t2.Width, scale), Color.Blue * (side == 2 && client.cID == P[2].ID ? 0.8F : 0.4F));
                    DrawOutString(P[2].score.ToString(), 21 + t1.Width + t2.Width, 8 + t2.Height / 6, Color.LightGray, Color.Black, 2, 0.75F);
                }
                if (P[3] != null)
                {
                    sb.Draw(t1, new Vector2(Width - t1.Width - 5, 5), Color.White);
                    scale = t2.Height * P[3].hp / P[3].GetMaxHP();
                    sb.Draw(t2, new Rectangle(Width - t2.Width - 8, 8, t2.Width, scale), new Rectangle(0, t2.Height - scale, t2.Width, scale), Color.Red * (side == 2 && client.cID == P[3].ID ? 1 : 0.6F));
                    sb.Draw(t1, new Vector2(Width - t1.Width - t1.Width - 10, 5), Color.White);
                    scale = t2.Height * P[3].mp / P[3].GetMaxMP();
                    sb.Draw(t2, new Rectangle(Width - t2.Width - t1.Width - 13, 8, t2.Width, scale), new Rectangle(0, t2.Height - scale, t2.Width, scale), Color.Blue * (side == 2 && client.cID == P[3].ID ? 1 : 0.6F));
                    DrawOutString(P[3].score.ToString(), Width - 21 - t1.Width - t2.Width - Font.MeasureString(P[3].score.ToString()).X, 8 + t2.Height / 6, Color.LightGray, Color.Black, 2, 0.75F);
                }
                if (state == State.Paused)
                {
                    Texture2D pau = GetTex("MenuPause");
                    sb.Draw(pau, new Vector2(Width / 2F - pau.Width / 2F, Height / 2F - pau.Height / 2F), Color.White);
                    DrawOutString("ESC - Resume, F1 - Quit to main menu", 15, Height - 20, Color.White, Color.Black, 2, 0.8F);
                }
            }
            sb.End();
            base.Draw(gameTime);
        }

        private void DrawOutString(string txt, float x, float y, Color col, Color co2, byte space = 2, float scale = 1)
        {
            DrawString(txt, new Vector2(x + space, y), co2, scale);
            DrawString(txt, new Vector2(x - space, y), co2, scale);
            DrawString(txt, new Vector2(x, y + space), co2, scale);
            DrawString(txt, new Vector2(x, y - space), co2, scale);
            DrawString(txt, new Vector2(x, y), col, scale);
        }

        private void DrawOut(Texture2D tex, float x, float y, Color col, byte space = 2, float scale = 1)
        {
            Draw(tex, new Vector2(x + space, y), col, scale);
            Draw(tex, new Vector2(x - space, y), col, scale);
            Draw(tex, new Vector2(x, y - space), col, scale);
            Draw(tex, new Vector2(x, y + space), col, scale);
        }

        private void DrawString(string txt, Vector2 pos, Color col, float scale = 1)
        {
            sb.DrawString(Font, txt, pos, col, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        private void Draw(Texture2D tex, Vector2 pos, Color col, float scale = 1)
        {
            sb.Draw(tex, pos, null, col, 0, new Vector2(tex.Width * scale / 2, tex.Height * scale / 2), scale, SpriteEffects.None, 0);
        }

        private Net.Control GetCtrlState(KeyboardState ks)
        {
            float x = (ks.IsKeyDown(Keys.Left) ? -1 : 0) + (ks.IsKeyDown(Keys.Right) ? 1 : 0);
            float y = (ks.IsKeyDown(Keys.Up) ? -1 : 0) + (ks.IsKeyDown(Keys.Down) ? 1 : 0);
            bool[] k = new bool[8];
            k[0] = ks.IsKeyDown(Keys.F);
            k[1] = ks.IsKeyDown(Keys.G);
            k[2] = ks.IsKeyDown(Keys.H);
            k[3] = ks.IsKeyDown(Keys.T);
            return new Net.Control(x, y, k);
        }

        private Net.Control GetCtrlState(ExtendedGamePadState gs)
        {
            bool[] k = new bool[8];
            k[0] = gs.IsButtonDown(0);
            k[1] = gs.IsButtonDown(1);
            k[2] = gs.IsButtonDown(2);
            k[3] = gs.IsButtonDown(3);
            return new Net.Control(gs.GetAxis(ExtendedAxes.X), -gs.GetAxis(ExtendedAxes.Y), k);
        }
    }
}
