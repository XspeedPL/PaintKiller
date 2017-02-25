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
        private readonly PaintKiller game;
        private UdpClient client;

        /// <summary>Unique client's game object ID</summary>
        internal uint cID = 0;

        /// <summary>Server connection and packet read thread worker</summary>
        private readonly BackgroundWorker bgw = new BackgroundWorker() { WorkerSupportsCancellation = true };

        /// <summary>The server's endpoint</summary>
        private IPEndPoint epS;

        internal NetClient(PaintKiller inst, IPAddress ip, byte cls)
        {
            game = inst;
            epS = new IPEndPoint(ip, NetHelper.Port);
            bgw.DoWork += bgw_DoWork;
            bgw.RunWorkerAsync(cls);
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            client = new UdpClient();
            client.Client.ReceiveTimeout = 2000;
            client.Client.SendTimeout = 2000;
            while (!bgw.CancellationPending)
            {
                if (game.GameState == PaintKiller.GameStates.JOIN) SendConnect((byte)e.Argument);
                Read();
            }
        }

        public void Dispose()
        {
            bgw.CancelAsync();
            client.Close();
            bgw.Dispose();
        }

        /// <summary>Sends out a lobby connection request packet to the server</summary>
        /// <param name="cls">Selected player character class number</param>
        public void SendConnect(byte cls)
        {
            using (NetPacket.Factory npf = new NetPacket.Factory(NetPacket.Types.SLobbyJoin, 1))
            {
                npf.Write(cls);
                using (NetPacket np = npf.GetPacket()) NetHelper.SecureOut(client, np, epS);
            }
        }

        /// <summary>Reads a packet from the server and processes it</summary>
        private void Read()
        {
            using (NetPacket np = NetHelper.SecureIn(client, ref epS))
            {
                int i; byte c;
                if (np != null)
                    switch (np.Type)
                    {
                        case NetPacket.Types.CLobbyConnect:
                            if (game.GameState == PaintKiller.GameStates.JOIN)
                            {
                                cID = np.ReadUInt32();
                                game.GameState = PaintKiller.GameStates.LOBBY;
                            }
                            break;
                        case NetPacket.Types.CEntList:
                            GameObj.uid = np.ReadUInt32();
                            int cnt = np.ReadInt32();
                            EntityList data = new EntityList();
                            for (i = 0; i < cnt; ++i) data.Add(ReadEntData(np));
                            game.NewList(data);
                            c = np.ReadByte();
                            lock (game.P)
                            {
                                for (i = 0; i < c; ++i)
                                {
                                    uint id = np.ReadUInt32();
                                    game.P[i] = (GPlayer)game.GetObj(id);
                                    if (game.P[i] == null) game.P[i] = (GPlayer)new GPOrc(Vector2.Zero).Clone(id);
                                    game.P[i].Score = np.ReadInt32();
                                }
                                for (; i < 4; ++i) game.P[i] = null;
                            }
                            break;
                        case NetPacket.Types.CGameEnd:
                            game.Return();
                            break;
                        case NetPacket.Types.CLobbyJoin:
                            if (game.GameState == PaintKiller.GameStates.LOBBY)
                            {
                                c = np.ReadByte();
                                lock (game.P)
                                {
                                    for (i = 0; i < c; ++i)
                                    {
                                        uint id = np.ReadUInt32();
                                        byte cl = np.ReadByte();
                                        if (cl == 0) game.P[i] = (GPlayer)new GPOrc(Vector2.Zero).Clone(id);
                                        else if (cl == 1) game.P[i] = (GPlayer)new GPMage(Vector2.Zero).Clone(id);
                                        else game.P[i] = (GPlayer)new GPArch(Vector2.Zero).Clone(id);
                                    }
                                    for (; i < 4; ++i) game.P[i] = null;
                                }
                            }
                            break;
                        case NetPacket.Types.CPause:
                            if (np.ReadByte() == 0) game.GameState = PaintKiller.GameStates.GAME;
                            else game.GameState = PaintKiller.GameStates.PAUSE;
                            break;
                    }
            }
        }

        /// <summary>Sends a player controls snapshot to the server</summary>
        /// <param name="c">The controls snapshot</param>
        internal void SendKeys(Controls c)
        {
            using (NetPacket.Factory data = new NetPacket.Factory(NetPacket.Types.SControls, 9))
            {
                data.Write(c.X);
                data.Write(c.Y);
                data.Write(c.Keys.Pack());
                using (NetPacket np = data.GetPacket()) NetHelper.SecureOut(client, np, epS);
            }
        }

        /// <summary>Sends a disconnect event packet to the server</summary>
        internal void SendEnd()
        {
            NetHelper.SecureOut(client, NetPacket.Prepare(NetPacket.Types.SDisconnect), epS);
        }

        /// <summary>Reads a single game object's data from the packet</summary>
        /// <param name="np"></param>
        /// <returns></returns>
        private GameObj ReadEntData(NetPacket np)
        {
            uint id = np.ReadUInt32();
            string cls = np.ReadString();
            GameObj ret = game.GetObj(id);
            if (ret == null) ret = game.Registry.GetClone(cls, id);
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
