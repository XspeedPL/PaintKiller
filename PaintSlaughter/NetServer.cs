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
        internal static class Packets
        {
            internal const byte LobbyJoin = 0, Controls = 1, Disconnect = 2;
        }

        private UdpClient server;
        internal byte ccount = 1;
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

        private void Broadcast(NetPacket np)
        {
            for (int i = 1; i < 4; ++i)
                if (PaintKiller.P[i] != null)
                    Send(np, PaintKiller.P[i].ep);
        }

        private void Send(NetPacket np, IPEndPoint ep)
        {
            Net.SecureOut(server, np, ep);
        }

        internal void SendEnd()
        {
            Broadcast(NetPacket.Prepare(2));
        }

        internal void SendPlrList()
        {
            NetPacket.Writer data = new NetPacket.Writer(ccount * 5 + 1);
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

        internal void SendGame(bool pause)
        {
            Broadcast(NetPacket.Prepare(4, new List<byte>(new byte[] { (byte)(pause ? 1 : 0) })));
        }

        internal void SendObjs(List<GameObj> objs)
        {
            NetPacket.Writer data = new NetPacket.Writer(objs.Count * 70 + 20);
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

        private void WriteData(NetPacket.Writer data, GameObj go)
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
