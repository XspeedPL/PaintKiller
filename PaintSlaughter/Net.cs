using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace PaintKiller
{
    internal static class Net
    {
        internal static readonly short port = 10666;
        internal static readonly Encoding enc = Encoding.GetEncoding(1250);
        private static readonly object ol = new object(), il = new object();

        internal static bool SecureOut(UdpClient sender, NetPacket data, IPEndPoint ep)
        {
            try
            {
                lock (ol) sender.Send(data, data.Length, ep);
                return true;
            }
            catch { }
            return false;
        }

        internal static NetPacket SecureIn(UdpClient sender, ref IPEndPoint ep)
        {
            try
            {
                byte[] ret;
                lock (il) ret = sender.Receive(ref ep);
                return NetPacket.Get(ret);
            }
            catch { }
            return null;
        }

        internal sealed class Control
        {
            public readonly float X, Y;
            public readonly bool[] Keys;

            public Control(float x, float y, bool[] keys) { X = x; Y = y; Keys = keys; }
        }

        public static byte Pack(bool[] data)
        {
            byte ret = 0;
            for (byte i = 7; i < 255; --i)
            {
                ret <<= 1;
                if (data[i]) ++ret;
            }
            return ret;
        }

        public static bool[] Unpack(byte data)
        {
            bool[] ret = new bool[8];
            for (byte i = 0; i < 8; ++i)
            {
                ret[i] = data % 2 == 1;
                data /= 2;
            }
            return ret;
        }
    }
}