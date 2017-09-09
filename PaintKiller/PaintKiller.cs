using System;
using System.Net;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PaintKilling.Net;
using PaintKilling.Objects;
using PaintKilling.Objects.Enemies;
using PaintKilling.Objects.Players;
using PaintKilling.Mechanics.Game;
using PaintKilling.Mechanics.Display;
using PaintKilling.Mechanics.Content;

namespace PaintKilling
{
    internal sealed class PaintKiller : Game
    {
        /// <summary>Recognized game states</summary>
        internal static class GameStates
        {
            public const byte MAIN = 0, SETTS = 1, CLASS = 2, LOBBY = 3, JOIN = 4, GAME = 5, PAUSE = 6;
        }

        private enum NetSide : byte { Single = 0, Server, Client }

        /// <summary>Global random number generator</summary>
        public static readonly Random Rand = new Random();

        /// <summary>Current game object list snapshot</summary>
        private EntityList objs = new EntityList();

        /// <summary>New game objects to be added on the next game tick</summary>
        private readonly UnsafeCollection<GameObj> queue = new UnsafeCollection<GameObj>();

        /// <summary>Game-wide used single font</summary>
        public static SpriteFont Font { get; internal set; }

        private GraphicsDeviceManager graphics;
        private SpriteBatch sb;

        /// <summary>Used to check for repeating key presses</summary>
        private Controls pctrl = new Controls(0, 0, new bool[8]);

        private bool usePad = false;
        private NetSide side = 0;
        private byte spawn = 1;

        public Registry Registry { get; private set; }

        /// <summary>Current game state</summary>
        internal byte GameState { get { return _gameState; } set { _gameState = value; states[value].Reset(); } }
        private byte _gameState;

        private BaseState[] states;

        /// <summary>Server instance used only when hosting a game</summary>
        private NetServer server;

        /// <summary>Client instance used only if playing in a multiplayer mode</summary>
        private NetClient client;

        /// <summary>Global game instance object</summary>
        internal static PaintKiller Inst { get; private set; }

        public int Width => GraphicsDevice.Viewport.Width;
        public int Height => GraphicsDevice.Viewport.Height;

        /// <summary>Player game objects array</summary>
        internal GPlayer[] P { get; private set; }

        public PaintKiller()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                IsFullScreen = false,
                PreferredBackBufferWidth = 800,
                PreferredBackBufferHeight = 600
            };
            Content.RootDirectory = "Content";
            Inst = this;
        }

        /// <summary>Returns a game object list snapshot</summary>
        public EntityList GetObjs() { return objs; }

        /// <summary>Adds a new game object to the game</summary>
        /// <param name="go">The new game object</param>
        public void AddObj(GameObj go) { queue.Add(go); }

        public void AddBlood(GameObj hitter, GameObj hit)
        {
            Vector2 v = Vector2.Normalize(hitter.pos - hit.pos) * hit.Radius;
            AddObj(new GEC(hit.pos + v, "BloodS", 10));
        }

        /// <summary>Searches for a game object associated with a specified ID</summary>
        /// <param name="id">The ID to look up</param>
        /// <returns>A game object with the specified ID, null if not found</returns>
        public GameObj GetObj(uint id) { return objs.GetByID(id); }

        /// <summary>Searches for a player with a specific endpoint</summary>
        /// <param name="ep">The endpoint to look up</param>
        /// <returns>A player with the specific endpoint, null if not found</returns>
        internal GPlayer GetPlr(IPEndPoint ep)
        {
            for (int i = 0; i < 4; ++i) if (P[i] != null && P[i].EP.Equals(ep)) return P[i];
            return null;
        }

        /// <summary>Searches for a texture by it's assigned name</summary>
        /// <param name="tex">Texture name</param>
        public Texture2D GetTex(string tex)
        {
            return Content.Load<Texture2D>(tex);
        }

        protected override void Initialize()
        {
            base.Initialize();
            Helpers.Init();
            IsFixedTimeStep = true;
            TargetElapsedTime = new TimeSpan(200000);
            P = new GPlayer[4];
            usePad = GamePad.GetState(PlayerIndex.One).IsConnected;
            states = new BaseState[7];

            State s = new State("Main Menu", "F - Accept");
            s.AddComponent(new Label() { Focusable = false, Position = new Vector2(75, 100), Scale = 1.5F, Text = "Choose gamemode", TextColor = Color.Yellow });
            s.AddComponent(new Label() { Position = new Vector2(100, 175), Text = "Singleplayer", ID = GameStates.CLASS });
            s.AddComponent(new Label() { Position = new Vector2(100, 225), Text = "Multiplayer host", ID = GameStates.LOBBY });
            s.AddComponent(new Label() { Position = new Vector2(100, 275), Text = "Multiplayer join", ID = GameStates.JOIN });
            s.AddComponent(new Label() { Position = new Vector2(100, 325), Text = "Settings", ID = GameStates.SETTS });
            s.Selected += (c) =>
            {
                if (c.ID == GameStates.SETTS) GameState = GameStates.SETTS;
                else
                {
                    GameState = GameStates.CLASS;
                    side = (NetSide)(c.ID - 2);
                }
            };
            states[GameStates.MAIN] = s;

            s = new State("Settings", "F1 - Quit to main menu");
            s.AddComponent(new Label() { Focusable = false, Position = new Vector2(75, 100), Scale = 1.5F, Text = "Gameplay settings", TextColor = Color.Yellow });
            s.AddComponent(new Label() { Focusable = false, Position = new Vector2(100, 175), Text = "Input device" });
            Toggle t = new Toggle() { Position = new Vector2(200, 225), TextL = "Keyboard", TextR = "Gamepad", Toggled = usePad };
            s.AddComponent(t);
            t.Change += (c) =>
            {
                if (c.Toggled && usePad) { usePad = false; return true; }
                else if (!c.Toggled && !usePad) return usePad = GamePad.GetState(PlayerIndex.One).IsConnected;
                return false;
            };
            states[GameStates.SETTS] = s;

            s = new State("Class Select", "F - Accept, F1 - Quit to main menu");
            s.AddComponent(new Label() { Focusable = false, Position = new Vector2(75, 100), Scale = 1.5F, Text = "Choose character", TextColor = Color.Yellow });
            Texture2D player = GetTex("GPlayer");
            s.AddComponent(new Image() { Position = new Vector2(125, 200), Scale = 2, TexColor = Color.Yellow, Texture = player, ID = 1 });
            s.AddComponent(new Label() { Focusable = false, Position = new Vector2(200, 200), Scale = 1.25F, Text = "Archer" });
            s.AddComponent(new Image() { Position = new Vector2(125, 300), Scale = 2, TexColor = Color.Blue, Texture = player, ID = 2 });
            s.AddComponent(new Label() { Focusable = false, Position = new Vector2(200, 300), Scale = 1.25F, Text = "Mage" });
            s.AddComponent(new Image() { Position = new Vector2(125, 400), Scale = 2, TexColor = Color.DarkGreen, Texture = player, ID = 3 });
            s.AddComponent(new Label() { Focusable = false, Position = new Vector2(200, 400), Scale = 1.25F, Text = "Orc" });
            s.Selected += (c) =>
            {
                GPlayer gp;
                if (c.ID == 1) gp = new GPArch(new Vector2(Width / 2, Height / 2));
                else if (c.ID == 2) gp = new GPMage(new Vector2(Width / 2, Height / 2));
                else gp = new GPOrc(new Vector2(Width / 2, Height / 2));
                P[0] = gp;
                P[0].EP = new IPEndPoint(IPAddress.Loopback, NetHelper.Port);
                if (side != NetSide.Client)
                {
                    objs.Add(gp);
                    GameState = side == 0 ? GameStates.GAME : GameStates.LOBBY;
                }
                else GameState = GameStates.JOIN;
            };
            states[GameStates.CLASS] = s;

            BaseState bs = new BaseState("Game");
            for (int i = 0; i < 4; ++i)
            {
                int y = (i + 1) % 2 * 2 - 1;
                int x = (i / 2) * 2 - 1;
                Label l = new Label() { Position = new Vector2(x * -80, y * -35), Scale = 0.75F, VerticalAlign = (Component.VAlign)(y + 1) };
                l.HorzontalAlign = l.TextAlign = (Component.HAlign)(x + 1);
                bs.AddComponent(l);
                bs.AddComponent(new PlayerInfo(P, i, l) { Position = new Vector2(x * -40, y * -65), VerticalAlign = l.VerticalAlign, HorzontalAlign = l.HorzontalAlign });
            }       
            bs.AddComponent(new Image() { HorzontalAlign = Component.HAlign.Center, Texture = GetTex("MenuPause"), VerticalAlign = Component.VAlign.Center, ID = 1 });
            bs.Resetting += (state) => state.FindByID(1).Enabled = GameState == GameStates.PAUSE;
            states[GameStates.GAME] = bs;
            states[GameStates.PAUSE] = bs;

            s = new State("Join Lobby", "F1 - Quit to main menu");
            s.AddComponent(new Label() { Focusable = false, Position = new Vector2(75, 100), Scale = 1.5F, Text = "Enter lobby IP address", TextColor = Color.Yellow });
            s.AddComponent(new Label() { Focusable = false, Position = new Vector2(200, -70), Text = "IP:", VerticalAlign = Component.VAlign.Center, TextAlign = Component.HAlign.Right });
            IPInput ipIn = new IPInput() { Position = new Vector2(215, -70), VerticalAlign = Component.VAlign.Center, ID = 6 };
            s.AddComponent(ipIn);
            s.AddComponent(new MultiLabel() { Position = new Vector2(250, -30), Texts = new string[] { "1", "2", "3" }, VerticalAlign = Component.VAlign.Center, ID = 1 });
            s.AddComponent(new MultiLabel() { Position = new Vector2(250, -10), Texts = new string[] { "4", "5", "6" }, VerticalAlign = Component.VAlign.Center, ID = 2 });
            s.AddComponent(new MultiLabel() { Position = new Vector2(250, 10), Texts = new string[] { "7", "8", "9" }, VerticalAlign = Component.VAlign.Center, ID = 3 });
            s.AddComponent(new MultiLabel() { Position = new Vector2(250, 30), Texts = new string[] { "0", ".", "<" }, VerticalAlign = Component.VAlign.Center, ID = 4 });
            s.AddComponent(new Label() { Position = new Vector2(285, 70), Text = "Clear", TextAlign = Component.HAlign.Center, VerticalAlign = Component.VAlign.Center, ID = 5 });
            s.AddComponent(new Label() { Position = new Vector2(285, 135), Scale = 1.3F, Text = "Connect", TextAlign = Component.HAlign.Center, VerticalAlign = Component.VAlign.Center, ID = 6 });
            s.Selected += (c) =>
            {
                if (ipIn.ReadOnly) return;
                if (c.ID == 5) ipIn.Text = "";
                else if (c.ID == 6)
                {
                    if (IPAddress.TryParse(ipIn.Text, out IPAddress ip))
                    {
                        ipIn.Text = "Connecting...";
                        ipIn.ReadOnly = true;
                        client = new NetClient(this, ip, P[0].GetClassID());
                    }
                }
                else if (c.ID >= 1)
                {
                    MultiLabel mLab = (MultiLabel)c;
                    if (mLab.ID == 4)
                    {
                        if (mLab.SelectedIndex == 0) ipIn.Text += "0";
                        else if (mLab.SelectedIndex == 1) ipIn.Text += ".";
                        else if (ipIn.Text.Length >= 1) ipIn.Text = ipIn.Text.Remove(ipIn.Text.Length - 1);
                    }
                    else ipIn.Text += mLab.SelectedIndex + mLab.ID * 3 - 2;
                }
            };
            states[GameStates.JOIN] = s;

            s = new State("Lobby", "F1 - Quit to main menu");
            s.AddComponent(new Label());
            s.Selected += (c) =>
            {
                if (side == NetSide.Server)
                {
                    server.SendPlrList();
                    server.SendGame(false);
                    for (byte i = 1; i < 4; ++i) if (P[i] != null) objs.Add(P[i]);
                    GameState = GameStates.GAME;
                }
            };
            s.Resetting += (state) =>
            {
                if (side == NetSide.Server) server = new NetServer(this);
            };
            states[GameStates.LOBBY] = s;

            GameState = GameStates.MAIN;
        }

        /// <summary>Replaces the game object list snapshot with a new one</summary>
        /// <param name="list">A new snapshot</param>
        internal void NewList(EntityList list)
        {
            objs = list;
        }

        /// <summary>Preloads all used assets</summary>
        protected override void LoadContent()
        {
            sb = new SpriteBatch(GraphicsDevice);
            Font = Content.Load<SpriteFont>("Font");
            Registry = new Registry();
            Registry.RegisterDefaults();
            GetTex("Bar1");
            GetTex("Bar2");
            GetTex("MenuPause");
        }

        protected override void UnloadContent()
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
        }

        /// <summary>Resets the game and returns the player to the main menu</summary>
        internal void Return()
        {
            UnloadContent();
            GameState = GameStates.MAIN;
            lock (P) for (byte i = 0; i < 4; ++i) P[i] = null;
            NewList(new EntityList());
        }

        protected override void Update(GameTime gameTime)
        {
            Controls ctrl = usePad ? GamePad.GetState(PlayerIndex.One).GetCtrlState() : Keyboard.GetState().GetCtrlState();
            ctrl.SetPrev(pctrl);
            states[GameState].Update(ctrl);
            if (ctrl.IsFirstPress(Controls.Key_Escape))
            {
                if (GameState == GameStates.GAME || GameState == GameStates.PAUSE)
                {
                    if (side != NetSide.Client)
                    {
                        if (side == NetSide.Server) server.SendGame(GameState == GameStates.GAME);
                        GameState = GameState == GameStates.GAME ? GameStates.PAUSE : GameStates.GAME;
                    }
                }
                else Exit();
            }
            else if (ctrl.IsFirstPress(Controls.Key_Return) && GameState != GameStates.GAME) Return();
            else if (GameState == GameStates.GAME)
            {
                if (side == NetSide.Client) client.SendKeys(ctrl);
                else
                {
                    int enes = objs.GetTeamCount(2);
                    if (enes < 1 || --spawn < 1)
                    {
                        spawn = 100;
                        int plrs = server == null ? 1 : server.ClientCount;
                        for (int i = enes; i < 10 + 4 * plrs; ++i)
                        {
                            GameObj enemy;
                            if (Rand.Next(10) == 0)
                                enemy = new GEKnight(new Vector2(Rand.Next(2) * Width, Rand.Next(Height)));
                            else if (Rand.Next(4) == 0)
                                enemy = new GEArch(new Vector2(Rand.Next(2) * Width, Rand.Next(Height)));
                            else enemy = new GEnemy(new Vector2(Rand.Next(2) * Width, Rand.Next(Height)));
                            enemy.team = 2;
                            objs.Add(enemy);
                        }
                    }
                    P[0].keys = ctrl;
                    LinkedList<GameObj> tmp = new LinkedList<GameObj>(objs);
                    UnsafeCollection<GameObj>.Enumerator en;
                    using (en = objs.GetEnumerator())
                        while (en.MoveNext())
                        {
                            GameObj go = en.Current;
                            go.OnUpdate();
                            tmp.RemoveFirst();
                            if (go.dead) en.Remove();
                            else if (go.IsColliding())
                                foreach (GameObj gn in tmp)
                                    if (gn.IsColliding() && go.Intersects(gn))
                                        go.OnCollision(gn);
                        }
                    using (en = queue.GetEnumerator())
                        while (en.MoveNext())
                        {
                            objs.Add(en.Current);
                            en.Remove();
                        }
                    if (side == NetSide.Server) server.SendObjs(objs);
                }
            }
            base.Update(gameTime);
            pctrl = ctrl;
            pctrl.SetPrev(null);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);
            if (GameState == GameStates.GAME || GameState == GameStates.PAUSE)
            {
                sb.Begin(SpriteSortMode.BackToFront);
                foreach (GameObj go in objs) go.OnDraw(sb);
                sb.End();
            }
            sb.Begin();
            states[GameState].Draw(sb, GraphicsDevice.Viewport.Bounds.Size);
            sb.End();
            base.Draw(gameTime);
        }
    }
}
