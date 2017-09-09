using System.Net;
using System.Net.Sockets;

namespace PaintKilling.Net
{
    internal static class NetHelper
    {
        /// <summary>Port used throughout the application for network communication</summary>
        internal const short Port = 10666;

        /// <summary>Used to synchronize outgoing traffic</summary>
        private static object OutLock { get; } = new object();

        /// <summary>Used to synchronize incoming traffic</summary>
        private static object InLock { get; } = new object();

        /// <summary>
        /// Sends data synchronously, ignores any exceptions, thread-safe
        /// </summary>
        /// <param name="sender">Sending client object</param>
        /// <param name="data">The payload</param>
        /// <param name="ep">Receiver's endpoint</param>
        /// <returns>True on success, false otherwise</returns>
        internal static bool SecureOut(UdpClient sender, NetPacket data, IPEndPoint ep)
        {
            try { lock (OutLock) sender.Send(data.Data, data.Length, ep); return true; }
            catch { return false; }
        }

        /// <summary>
        /// Receives data synchronously, ignores any exceptions, thread-safe
        /// </summary>
        /// <param name="sender">Receiving client object</param>
        /// <param name="ep">Sender's endpoint</param>
        /// <returns>A NetPacket on success, null otherwise</returns>
        internal static NetPacket SecureIn(UdpClient sender, ref IPEndPoint ep)
        {
            try { lock (InLock) return NetPacket.Get(sender.Receive(ref ep)); }
            catch { return null; }
        }

        /// <summary>Converts an array of 8 bits to a byte</summary>
        public static byte Pack(this bool[] data)
        {
            byte ret = 0;
            for (byte i = 7; i < 255; --i)
            {
                ret <<= 1;
                if (data[i]) ++ret;
            }
            return ret;
        }

        /// <summary>Converts a byte to an array of 8 bits</summary>
        public static bool[] Unpack(this byte data)
        {
            bool[] ret = new bool[8];
            for (byte i = 0; i < 8; ++i)
            {
                ret[i] = (data & 1) == 1;
                data >>= 1;
            }
            return ret;
        }
    }
}