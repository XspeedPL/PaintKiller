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
        private readonly PaintKiller game;
        private UdpClient server;

        /// <summary>Connected clients count</summary>
        internal byte ccount = 1;

        /// <summary>Packet read thread worker</summary>
        private readonly BackgroundWorker bgw = new BackgroundWorker() { WorkerSupportsCancellation = true };


        internal NetServer(PaintKiller inst)
        {
            game = inst;
            bgw.DoWork += bgw_DoWork;
            bgw.RunWorkerAsync();
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            server = new UdpClient(new IPEndPoint(IPAddress.Any, NetHelper.Port));
            server.Client.ReceiveTimeout = 2000;
            server.Client.SendTimeout = 2000;
            while (!bgw.CancellationPending)
                Read();
        }

        public void Dispose()
        {
            bgw.CancelAsync();
            server.Close();
            bgw.Dispose();
        }

        /// <summary>Reads a packet from any source and processes it</summary>
        private void Read()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, NetHelper.Port);
            using (NetPacket np = NetHelper.SecureIn(server, ref ep))
                if (np != null)
                {
                    GPlayer gp = game.GetPlr(ep);
                    switch (np.Type)
                    {
                        case NetPacket.Types.SLobbyJoin:
                            if (game.GameState == PaintKiller.GameStates.LOBBY && ccount < 4 && gp == null)
                            {
                                byte cls = np.ReadByte();
                                lock (game.P)
                                {
                                    if (cls == 0) game.P[ccount] = new GPOrc(new Vector2(50 + ccount * 10, 150));
                                    else if (cls == 1) game.P[ccount] = new GPMage(new Vector2(50 + ccount * 10, 150));
                                    else if (cls == 2) game.P[ccount] = new GPArch(new Vector2(50 + ccount * 10, 150));
                                    game.P[ccount].EP = ep;
                                }
                                using (NetPacket.Factory npf = new NetPacket.Factory(NetPacket.Types.CLobbyConnect, 4))
                                {
                                    npf.Write(game.P[ccount++].ID);
                                    using (NetPacket np_out = npf.GetPacket()) Send(np_out, ep);
                                }
                                SendPlrList();
                            }
                            else if (game.GetPlr(ep) == null) Send(NetPacket.Prepare(NetPacket.Types.CGameEnd), ep);
                            break;
                        case NetPacket.Types.SControls:
                            if (game.GameState == PaintKiller.GameStates.GAME && gp != null)
                                gp.keys = new Controls(np.ReadSingle(), np.ReadSingle(), np.ReadByte().Unpack());
                            break;
                        case NetPacket.Types.SDisconnect:
                            if (gp != null)
                            {
                                gp.Kill();
                                if (game.GameState != PaintKiller.GameStates.GAME && game.GameState != PaintKiller.GameStates.PAUSE)
                                {
                                    byte i;
                                    lock (game.P)
                                    {
                                        for (i = 1; i < 4; ++i)
                                            if (game.P[i] == gp)
                                                game.P[i] = null;
                                        --ccount;
                                        if (i < ccount)
                                        {
                                            game.P[i] = game.P[ccount];
                                            game.P[ccount] = null;
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
                if (game.P[i] != null)
                    Send(np, game.P[i].EP);
        }

        /// <summary>Sends a packet to a specific endpoint</summary>
        /// <param name="np">The payload</param>
        /// <param name="ep">Client's endpoint</param>
        private void Send(NetPacket np, IPEndPoint ep)
        {
            NetHelper.SecureOut(server, np, ep);
        }

        /// <summary>Broadcasts a sever termination event packet</summary>
        internal void SendEnd()
        {
            Broadcast(NetPacket.Prepare(NetPacket.Types.CGameEnd));
        }

        /// <summary>Constructs and broadcasts a list of all connected players in the lobby</summary>
        internal void SendPlrList()
        {
            using (NetPacket.Factory data = new NetPacket.Factory(NetPacket.Types.CLobbyJoin, ccount * 5 + 1))
            {
                data.Write(ccount);
                for (byte i = 0; i < ccount; ++i)
                {
                    data.Write(game.P[i].ID);
                    data.Write(game.P[i].GetClassID());
                }
                using (NetPacket np = data.GetPacket()) Broadcast(np);
            }
        }

        /// <summary>Broadcasts a gamestate packet indicating a pause or resume</summary>
        /// <param name="pause">True if the new state is a pause, false otherwise</param>
        internal void SendGame(bool pause)
        {
            using (NetPacket.Factory npf = new NetPacket.Factory(NetPacket.Types.CPause, 1))
            {
                npf.Write(pause ? (byte)1 : (byte)0);
                using (NetPacket np = npf.GetPacket()) Broadcast(np);
            }
        }

        /// <summary>Constructs and broadcasts a list of all active game objects along with players and their scores</summary>
        /// <param name="objs">Game objects list snapshot</param>
        internal void SendObjs(IReadOnlyCollection<GameObj> objs)
        {
            using (NetPacket.Factory data = new NetPacket.Factory(NetPacket.Types.CEntList, objs.Count * 70 + 20))
            {
                data.Write(GameObj.uid);
                data.Write(objs.Count);
                foreach (GameObj go in objs) WriteData(data, go);
                data.Write(ccount);
                for (byte i = 0; i < ccount; ++i)
                {
                    data.Write(game.P[i].ID);
                    data.Write(game.P[i].Score);
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
