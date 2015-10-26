using System;
using System.IO;
using System.Net;
using System.Text;
using System.ComponentModel;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace PaintKiller
{
    internal sealed class NetServer : IDisposable
    {
        /// <summary>Constants for packet types recognized by the server side</summary>
        internal static class Packets
        {
            internal const byte LobbyJoin = 0, Controls = 1, Disconnect = 2;
        }

        private UdpClient server;

        /// <summary>Connected clients count</summary>
        internal byte ccount = 1;

        /// <summary>Packet read thread worker</summary>
        private readonly BackgroundWorker bgw = new BackgroundWorker() { WorkerSupportsCancellation = true };


        internal NetServer()
        {
            bgw.DoWork += bgw_DoWork;
            bgw.RunWorkerAsync();
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            server = new UdpClient(new IPEndPoint(IPAddress.Any, Net.port));
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
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, Net.port);
            NetPacket np = Net.SecureIn(server, ref ep);
            if (np != null)
                switch (np.Type)
                {
                    case Packets.LobbyJoin:
                        if (PaintKiller.state == PaintKiller.State.MPHost && ccount < 4 && PaintKiller.GetPlr(ep) == null)
                        {
                            byte ch = np.ReadByte();
                            if (ch == 0) PaintKiller.P[ccount] = new GPOrc(new Vector2(50 + ccount * 10, 150));
                            else if (ch == 1) PaintKiller.P[ccount] = new GPMage(new Vector2(50 + ccount * 10, 150));
                            else if (ch == 2) PaintKiller.P[ccount] = new GPArch(new Vector2(50 + ccount * 10, 150));
                            PaintKiller.P[ccount].ep = ep;
                            Send(NetPacket.Prepare(0, new List<byte>(BitConverter.GetBytes(PaintKiller.P[ccount++].ID))), ep);
                            SendPlrList();
                        }
                        else if (PaintKiller.GetPlr(ep) == null) Send(NetPacket.Prepare(2), ep);
                        break;
                    case Packets.Controls:
                        if (PaintKiller.state == PaintKiller.State.Game)
                        {
                            GPlayer gp = PaintKiller.GetPlr(ep);
                            if (gp != null) gp.keys = new Net.Control(np.ReadFloat(), np.ReadFloat(), Net.Unpack(np.ReadByte()));
                        }
                        break;
                    case Packets.Disconnect:
                        GPlayer g = PaintKiller.GetPlr(ep);
                        if (g != null)
                        {
                            g.Kill();
                            if (PaintKiller.state != PaintKiller.State.Game && PaintKiller.state != PaintKiller.State.Paused)
                            {
                                byte i;
                                for (i = 1; i < 4; ++i)
                                    if (PaintKiller.P[i] == g)
                                        PaintKiller.P[i] = null;
                                --ccount;
                                if (i < ccount)
                                {
                                    PaintKiller.P[i] = PaintKiller.P[ccount];
                                    PaintKiller.P[ccount] = null;
                                }
                            }
                        }
                        break;
                }
        }

        /// <summary>Sends a packet to all registered clients</summary>
        /// <param name="np">The payload</param>
        private void Broadcast(NetPacket np)
        {
            for (int i = 1; i < 4; ++i)
                if (PaintKiller.P[i] != null)
                    Send(np, PaintKiller.P[i].ep);
        }

        /// <summary>Sends a packet to a specific endpoint</summary>
        /// <param name="np">The payload</param>
        /// <param name="ep">Client's endpoint</param>
        private void Send(NetPacket np, IPEndPoint ep)
        {
            Net.SecureOut(server, np, ep);
        }

        /// <summary>Broadcasts a sever termination event packet</summary>
        internal void SendEnd()
        {
            Broadcast(NetPacket.Prepare(NetClient.Packets.GameEnd));
        }

        /// <summary>Constructs and broadcasts a list of all connected players in the lobby</summary>
        internal void SendPlrList()
        {
            NetPacket.Factory data = new NetPacket.Factory(ccount * 5 + 1);
            data.WriteByte(ccount);
            for (byte i = 0; i < ccount; ++i)
            {
                data.WriteUInt(PaintKiller.P[i].ID);
                if (PaintKiller.P[i] is GPOrc) data.WriteByte(0);
                else if (PaintKiller.P[i] is GPMage) data.WriteByte(1);
                else data.WriteByte(2);
            }
            Broadcast(data.GetPacket(NetClient.Packets.LobbyJoin));
        }

        /// <summary>Broadcasts a gamestate packet indicating a pause or resume</summary>
        /// <param name="pause">True if the new state is a pause, false otherwise</param>
        internal void SendGame(bool pause)
        {
            Broadcast(NetPacket.Prepare(NetClient.Packets.Pause, new List<byte>(new byte[] { (byte)(pause ? 1 : 0) })));
        }

        /// <summary>Constructs and broadcasts a list of all active game objects along with players and their scores</summary>
        /// <param name="objs">Game objects list snapshot</param>
        internal void SendObjs(List<GameObj> objs)
        {
            NetPacket.Factory data = new NetPacket.Factory(objs.Count * 70 + 20);
            data.WriteUInt(GameObj.id);
            data.WriteInt(objs.Count);
            foreach (GameObj go in objs) WriteData(data, go);
            data.WriteByte(ccount);
            for (byte i = 0; i < ccount; ++i)
            {
                data.WriteUInt(PaintKiller.P[i].ID);
                data.WriteInt(PaintKiller.P[i].score);
            }
            Broadcast(data.GetPacket(NetClient.Packets.EntList));
        }

        /// <summary>Writes data of a single game object into a packet</summary>
        /// <param name="data">The output packet</param>
        /// <param name="go">A game object to extract data from</param>
        private void WriteData(NetPacket.Factory data, GameObj go)
        {
            data.WriteUInt(go.ID);
            data.WriteString(go.GetType().FullName);
            data.WriteShort(go.hp);
            data.WriteShort(go.mp);
            data.WriteByte(go.state);
            data.WriteByte(go.frame);
            data.WriteFloat(go.dir);
            data.WriteFloat(go.ang.X);
            data.WriteFloat(go.ang.Y);
            data.WriteFloat(go.spd.X);
            data.WriteFloat(go.spd.Y);
            data.WriteFloat(go.pos.X);
            data.WriteFloat(go.pos.Y);
            data.WriteFloat(go.fce.X);
            data.WriteFloat(go.fce.Y);
            // TODO: Add a member method into GameObj for writing/reading?
            if (go is GEC)
            {
                GEC g = (GEC)go;
                data.WriteString(g.gfx.Name);
                data.WriteByte(g.GetPrefs());
            }
            else if (go is GPChain)
            {
                GPChain g = (GPChain)go;
                data.WriteFloat(g.GetBoltDest().X);
                data.WriteFloat(g.GetBoltDest().Y);
            }
        }
    }
}
