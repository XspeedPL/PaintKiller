using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace PaintKiller
{
    internal sealed class NetClient : IDisposable
    {
        /// <summary>Constants for packet types recognized by the client side</summary>
        internal static class Packets
        {
            internal const byte LobbyConnect = 0, EntList = 1, GameEnd = 2, LobbyJoin = 3, Pause = 4;
        }

        private UdpClient client;

        /// <summary>Unique client's game object ID</summary>
        internal uint cID = 0;

        /// <summary>Server connection and packet read thread worker</summary>
        private readonly BackgroundWorker bgw = new BackgroundWorker() { WorkerSupportsCancellation = true };

        /// <summary>The server's endpoint</summary>
        private IPEndPoint epS;

        internal NetClient(IPAddress ip, byte cls)
        {
            epS = new IPEndPoint(ip, Net.port);
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
                if (PaintKiller.state == PaintKiller.State.MPJoin) SendConnect((byte)e.Argument);
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
            Net.SecureOut(client, NetPacket.Prepare(NetServer.Packets.LobbyJoin, new List<byte>(new byte[] { cls })), epS);
        }

        /// <summary>Reads a packet from the server and processes it</summary>
        private void Read()
        {
            NetPacket np = Net.SecureIn(client, ref epS);
            int i; byte c;
            if (np != null)
                switch (np.Type)
                {
                    case Packets.LobbyConnect:
                        if (PaintKiller.state == PaintKiller.State.MPJoin)
                        {
                            cID = np.ReadUInt();
                            PaintKiller.state = PaintKiller.State.MPHost;
                        }
                        break;
                    case Packets.EntList:
                        GameObj.id = np.ReadUInt();
                        int cnt = np.ReadInt();
                        PaintKiller.EntityList data = new PaintKiller.EntityList();
                        data.Capacity = cnt;
                        for (i = 0; i < cnt; ++i) data.Add(ReadEntData(np));
                        PaintKiller.NewList(data);
                        c = np.ReadByte();
                        for (i = 0; i < c; ++i)
                        {
                            uint id = np.ReadUInt();
                            PaintKiller.P[i] = (GPlayer)PaintKiller.GetObj(id);
                            if (PaintKiller.P[i] == null) PaintKiller.P[i] = new GPOrc(id);
                            PaintKiller.P[i].score = np.ReadInt();
                        }
                        for (; i < 4; ++i) PaintKiller.P[i] = null;
                        break;
                    case Packets.GameEnd:
                        PaintKiller.Inst.Return();
                        break;
                    case Packets.LobbyJoin:
                        if (PaintKiller.state == PaintKiller.State.MPHost)
                        {
                            c = np.ReadByte();
                            for (i = 0; i < c; ++i)
                            {
                                uint id = np.ReadUInt();
                                byte cl = np.ReadByte();
                                if (cl == 0) PaintKiller.P[i] = new GPOrc(id);
                                else if (cl == 1) PaintKiller.P[i] = new GPMage(id);
                                else PaintKiller.P[i] = new GPArch(id);
                            }
                            for (; i < 4; ++i) PaintKiller.P[i] = null;
                        }
                        break;
                    case Packets.Pause:
                        if (np.ReadByte() == 0) PaintKiller.state = PaintKiller.State.Game;
                        else PaintKiller.state = PaintKiller.State.Paused;
                        break;
                }
        }

        /// <summary>Sends a player controls snapshot to the server</summary>
        /// <param name="c">The controls snapshot</param>
        internal void SendKeys(Net.Control c)
        {
            NetPacket.Writer data = new NetPacket.Writer(9);
            data.WriteFloat(c.X);
            data.WriteFloat(c.Y);
            data.WriteByte(Net.Pack(c.Keys));
            Net.SecureOut(client, data.GetPacket(NetServer.Packets.Controls), epS);
        }

        /// <summary>Sends a disconnect event packet to the server</summary>
        internal void SendEnd()
        {
            Net.SecureOut(client, NetPacket.Prepare(NetServer.Packets.Disconnect), epS);
        }

        /// <summary>Reads a single game object's data from the packet</summary>
        /// <param name="np"></param>
        /// <returns></returns>
        private static GameObj ReadEntData(NetPacket np)
        {
            uint id = np.ReadUInt();
            GameObj ret;
            string cls = np.ReadString();
            if ((ret = PaintKiller.GetObj(id)) == null)
                ret = (GameObj)Type.GetType(cls).GetConstructor(new Type[] { typeof(uint) }).Invoke(new object[] { id });
            ret.SetHM(np.ReadShort(), np.ReadShort());
            ret.state = np.ReadByte();
            ret.frame = np.ReadByte();
            ret.dir = np.ReadFloat();
            ret.ang = new Vector2(np.ReadFloat(), np.ReadFloat());
            ret.spd = new Vector2(np.ReadFloat(), np.ReadFloat());
            ret.pos = new Vector2(np.ReadFloat(), np.ReadFloat());
            ret.fce = new Vector2(np.ReadFloat(), np.ReadFloat());
            if (ret is GEC)
            {
                GEC g = (GEC)ret;
                g.gfx = PaintKiller.GetTex(np.ReadString());
                g.SetPrefs(np.ReadByte());
            }
            else if (ret is GPChain) ((GPChain)ret).SetBolt(new Vector2(np.ReadFloat(), np.ReadFloat()));
            return ret;
        }
    }
}
