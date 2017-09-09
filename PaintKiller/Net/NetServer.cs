using System;
using System.Net;
using System.ComponentModel;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using PaintKilling.Objects;
using PaintKilling.Objects.Players;

namespace PaintKilling.Net
{
    internal sealed class NetServer : IDisposable
    {
        private PaintKiller Game { get; }

        private UdpClient Socket { get; set; }

        /// <summary>Connected clients count</summary>
        internal byte ClientCount { get; private set; } = 1;

        /// <summary>Packet read thread worker</summary>
        private BackgroundWorker Worker { get; } = new BackgroundWorker() { WorkerSupportsCancellation = true };
        
        internal NetServer(PaintKiller inst)
        {
            Game = inst;
            Worker.DoWork += Worker_DoWork;
            Worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Socket = new UdpClient(new IPEndPoint(IPAddress.Any, NetHelper.Port));
            Socket.Client.ReceiveTimeout = 2000;
            Socket.Client.SendTimeout = 2000;
            while (!Worker.CancellationPending)
                Read();
        }

        public void Dispose()
        {
            Worker.CancelAsync();
            Socket.Close();
            Worker.Dispose();
        }

        /// <summary>Reads a packet from any source and processes it</summary>
        private void Read()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, NetHelper.Port);
            using (NetPacket np = NetHelper.SecureIn(Socket, ref ep))
                if (np != null)
                {
                    GPlayer gp = Game.GetPlr(ep);
                    switch (np.Type)
                    {
                        case NetPacket.PType.SLobbyJoin:
                            if (Game.GameState == PaintKiller.GameStates.LOBBY && ClientCount < 4 && gp == null)
                            {
                                byte cls = np.ReadByte();
                                lock (Game.P)
                                {
                                    if (cls == 0) Game.P[ClientCount] = new GPOrc(new Vector2(50 + ClientCount * 10, 150));
                                    else if (cls == 1) Game.P[ClientCount] = new GPMage(new Vector2(50 + ClientCount * 10, 150));
                                    else if (cls == 2) Game.P[ClientCount] = new GPArch(new Vector2(50 + ClientCount * 10, 150));
                                    Game.P[ClientCount].EP = ep;
                                }
                                using (NetPacket.Factory npf = new NetPacket.Factory(NetPacket.PType.CLobbyConnect, 4))
                                {
                                    npf.Write(Game.P[ClientCount++].ID);
                                    using (NetPacket np_out = npf.GetPacket()) Send(np_out, ep);
                                }
                                SendPlrList();
                            }
                            else if (Game.GetPlr(ep) == null) Send(NetPacket.Prepare(NetPacket.PType.CGameEnd), ep);
                            break;
                        case NetPacket.PType.SControls:
                            if (Game.GameState == PaintKiller.GameStates.GAME && gp != null)
                                gp.keys = new Controls(np.ReadSingle(), np.ReadSingle(), np.ReadByte().Unpack());
                            break;
                        case NetPacket.PType.SDisconnect:
                            if (gp != null)
                            {
                                gp.Kill();
                                if (Game.GameState != PaintKiller.GameStates.GAME && Game.GameState != PaintKiller.GameStates.PAUSE)
                                {
                                    byte i;
                                    lock (Game.P)
                                    {
                                        for (i = 1; i < 4; ++i)
                                            if (Game.P[i] == gp)
                                                Game.P[i] = null;
                                        --ClientCount;
                                        if (i < ClientCount)
                                        {
                                            Game.P[i] = Game.P[ClientCount];
                                            Game.P[ClientCount] = null;
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
        }

        /// <summary>Sends a packet to all registered clients</summary>
        /// <param name="np">The payload</param>
        private void Broadcast(NetPacket np)
        {
            for (int i = 1; i < 4; ++i)
                if (Game.P[i] != null)
                    Send(np, Game.P[i].EP);
        }

        /// <summary>Sends a packet to a specific endpoint</summary>
        /// <param name="np">The payload</param>
        /// <param name="ep">Client's endpoint</param>
        private void Send(NetPacket np, IPEndPoint ep)
        {
            NetHelper.SecureOut(Socket, np, ep);
        }

        /// <summary>Broadcasts a sever termination event packet</summary>
        internal void SendEnd()
        {
            Broadcast(NetPacket.Prepare(NetPacket.PType.CGameEnd));
        }

        /// <summary>Constructs and broadcasts a list of all connected players in the lobby</summary>
        internal void SendPlrList()
        {
            using (NetPacket.Factory data = new NetPacket.Factory(NetPacket.PType.CLobbyJoin, ClientCount * 5 + 1))
            {
                data.Write(ClientCount);
                for (byte i = 0; i < ClientCount; ++i)
                {
                    data.Write(Game.P[i].ID);
                    data.Write(Game.P[i].GetClassID());
                }
                using (NetPacket np = data.GetPacket()) Broadcast(np);
            }
        }

        /// <summary>Broadcasts a gamestate packet indicating a pause or resume</summary>
        /// <param name="pause">True if the new state is a pause, false otherwise</param>
        internal void SendGame(bool pause)
        {
            using (NetPacket.Factory npf = new NetPacket.Factory(NetPacket.PType.CPause, 1))
            {
                npf.Write(pause ? (byte)1 : (byte)0);
                using (NetPacket np = npf.GetPacket()) Broadcast(np);
            }
        }

        /// <summary>Constructs and broadcasts a list of all active game objects along with players and their scores</summary>
        /// <param name="objs">Game objects list snapshot</param>
        internal void SendObjs(IReadOnlyCollection<GameObj> objs)
        {
            using (NetPacket.Factory data = new NetPacket.Factory(NetPacket.PType.CEntList, objs.Count * 70 + 20))
            {
                data.Write(GameObj.UID);
                data.Write(objs.Count);
                foreach (GameObj go in objs) WriteData(data, go);
                data.Write(ClientCount);
                for (byte i = 0; i < ClientCount; ++i)
                {
                    data.Write(Game.P[i].ID);
                    data.Write(Game.P[i].Score);
                }
                using (NetPacket np = data.GetPacket()) Broadcast(np);
            }
        }

        /// <summary>Writes data of a single game object into a packet</summary>
        /// <param name="data">The output packet</param>
        /// <param name="go">A game object to extract data from</param>
        private void WriteData(NetPacket.Factory data, GameObj go)
        {
            data.Write(go.ID);
            data.Write(go.GetType().Name);
            data.Write(go.HP);
            data.Write(go.MP);
            data.Write((byte)go.state);
            data.Write(go.frame);
            data.Write(go.team);
            data.Write(go.dir);
            data.Write(go.ang.X);
            data.Write(go.ang.Y);
            data.Write(go.spd.X);
            data.Write(go.spd.Y);
            data.Write(go.pos.X);
            data.Write(go.pos.Y);
            data.Write(go.fce.X);
            data.Write(go.fce.Y);
            go.WriteSpecial(data);
        }
    }
}
