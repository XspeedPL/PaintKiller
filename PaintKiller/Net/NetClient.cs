using System;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using PaintKilling.Objects;
using PaintKilling.Objects.Players;
using PaintKilling.Mechanics.Content;

namespace PaintKilling.Net
{
    internal sealed class NetClient : IDisposable
    {
        private PaintKiller Game { get; }

        private UdpClient Socket { get; set; }

        /// <summary>Unique client's game object ID</summary>
        public uint ClientID { get; private set; }

        /// <summary>Server connection and packet read thread worker</summary>
        private BackgroundWorker Worker { get; }

        /// <summary>The server's endpoint</summary>
        private IPEndPoint remoteAddress;

        internal NetClient(PaintKiller inst, IPAddress ip, byte cls)
        {
            Game = inst;
            remoteAddress = new IPEndPoint(ip, NetHelper.Port);
            Worker = new BackgroundWorker() { WorkerSupportsCancellation = true };
            Worker.DoWork += Worker_DoWork;
            Worker.RunWorkerAsync(cls);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Socket = new UdpClient();
            Socket.Client.ReceiveTimeout = 2000;
            Socket.Client.SendTimeout = 2000;
            while (!Worker.CancellationPending)
            {
                if (Game.GameState == PaintKiller.GameStates.JOIN) SendConnect((byte)e.Argument);
                Read();
            }
        }

        public void Dispose()
        {
            Worker.CancelAsync();
            Socket.Close();
            Worker.Dispose();
        }

        /// <summary>Sends out a lobby connection request packet to the server</summary>
        /// <param name="cls">Selected player character class number</param>
        public void SendConnect(byte cls)
        {
            using (NetPacket.Factory npf = new NetPacket.Factory(NetPacket.PType.SLobbyJoin, 1))
            {
                npf.Write(cls);
                using (NetPacket np = npf.GetPacket()) NetHelper.SecureOut(Socket, np, remoteAddress);
            }
        }

        /// <summary>Reads a packet from the server and processes it</summary>
        private void Read()
        {
            using (NetPacket np = NetHelper.SecureIn(Socket, ref remoteAddress))
            {
                int i; byte c;
                if (np != null)
                    switch (np.Type)
                    {
                        case NetPacket.PType.CLobbyConnect:
                            if (Game.GameState == PaintKiller.GameStates.JOIN)
                            {
                                ClientID = np.ReadUInt32();
                                Game.GameState = PaintKiller.GameStates.LOBBY;
                            }
                            break;
                        case NetPacket.PType.CEntList:
                            GameObj.UID = np.ReadUInt32();
                            int cnt = np.ReadInt32();
                            EntityList data = new EntityList();
                            for (i = 0; i < cnt; ++i) data.Add(ReadEntData(np));
                            Game.NewList(data);
                            c = np.ReadByte();
                            lock (Game.P)
                            {
                                for (i = 0; i < c; ++i)
                                {
                                    uint id = np.ReadUInt32();
                                    Game.P[i] = (GPlayer)Game.GetObj(id);
                                    if (Game.P[i] == null) Game.P[i] = (GPlayer)new GPOrc(Vector2.Zero).Clone(id);
                                    Game.P[i].Score = np.ReadInt32();
                                }
                                for (; i < 4; ++i) Game.P[i] = null;
                            }
                            break;
                        case NetPacket.PType.CGameEnd:
                            Game.Return();
                            break;
                        case NetPacket.PType.CLobbyJoin:
                            if (Game.GameState == PaintKiller.GameStates.LOBBY)
                            {
                                c = np.ReadByte();
                                lock (Game.P)
                                {
                                    for (i = 0; i < c; ++i)
                                    {
                                        uint id = np.ReadUInt32();
                                        byte cl = np.ReadByte();
                                        if (cl == 0) Game.P[i] = (GPlayer)new GPOrc(Vector2.Zero).Clone(id);
                                        else if (cl == 1) Game.P[i] = (GPlayer)new GPMage(Vector2.Zero).Clone(id);
                                        else Game.P[i] = (GPlayer)new GPArch(Vector2.Zero).Clone(id);
                                    }
                                    for (; i < 4; ++i) Game.P[i] = null;
                                }
                            }
                            break;
                        case NetPacket.PType.CPause:
                            if (np.ReadByte() == 0) Game.GameState = PaintKiller.GameStates.GAME;
                            else Game.GameState = PaintKiller.GameStates.PAUSE;
                            break;
                    }
            }
        }

        /// <summary>Sends a player controls snapshot to the server</summary>
        /// <param name="c">The controls snapshot</param>
        internal void SendKeys(Controls c)
        {
            using (NetPacket.Factory data = new NetPacket.Factory(NetPacket.PType.SControls, 9))
            {
                data.Write(c.X);
                data.Write(c.Y);
                data.Write(c.Keys.Pack());
                using (NetPacket np = data.GetPacket()) NetHelper.SecureOut(Socket, np, remoteAddress);
            }
        }

        /// <summary>Sends a disconnect event packet to the server</summary>
        internal void SendEnd()
        {
            NetHelper.SecureOut(Socket, NetPacket.Prepare(NetPacket.PType.SDisconnect), remoteAddress);
        }

        /// <summary>Reads a single game object's data from the packet</summary>
        /// <param name="np"></param>
        /// <returns></returns>
        private GameObj ReadEntData(NetPacket np)
        {
            uint id = np.ReadUInt32();
            string cls = np.ReadString();
            GameObj ret = Game.GetObj(id);
            if (ret == null) ret = Game.Registry.GetClone(cls, id);
            ret.SetHM(np.ReadInt16(), np.ReadInt16());
            ret.state = (GameObj.State)np.ReadByte();
            ret.frame = np.ReadByte();
            ret.team = np.ReadByte();
            ret.dir = np.ReadSingle();
            ret.ang = new Vector2(np.ReadSingle(), np.ReadSingle());
            ret.spd = new Vector2(np.ReadSingle(), np.ReadSingle());
            ret.pos = new Vector2(np.ReadSingle(), np.ReadSingle());
            ret.fce = new Vector2(np.ReadSingle(), np.ReadSingle());
            ret.ReadSpecial(np);
            return ret;
        }
    }
}
