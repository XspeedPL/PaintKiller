using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PaintKiller
{
    internal sealed class NetPacket
    {
        private readonly byte[] buffer;

        /// <summary>Calculates the payload's checksum</summary>
        /// <param name="data">The payload</param>
        private static uint CheckSum(List<byte> data)
        {
            uint ret = 0;
            foreach (byte b in data) ret += b;
            return ret;
        }

        /// <summary>Calculates the payload's checksum, ignores first 4 bytes</summary>
        /// <param name="data">The payload</param>
        private static uint CheckSum(byte[] data)
        {
            uint ret = 0;
            for (int i = 4; i < data.Length; ++i) ret += data[i];
            return ret;
        }

        /// <summary>Constructs a packet with the specified payload</summary>
        /// <param name="type">Packet type</param>
        /// <param name="data">The payload</param>
        internal static NetPacket Prepare(byte type, List<byte> data)
        {
            data.Insert(0, type);
            data.InsertRange(0, BitConverter.GetBytes(CheckSum(data)));
            return new NetPacket(data.ToArray());
        }

        /// <summary>Constructs an empty packet</summary>
        /// <param name="type">Packet type</param>
        internal static NetPacket Prepare(byte type)
        {
            List<byte> data = new List<byte>();
            data.Add(type);
            data.InsertRange(0, BitConverter.GetBytes(CheckSum(data)));
            return new NetPacket(data.ToArray());
        }

        /// <summary>Validates the payload's checksum and constructs a packet</summary>
        /// <param name="data">The payload</param>
        /// <returns>A packet object if the payload is vaild, null otherwise</returns>
        internal static NetPacket Get(byte[] data)
        {
            return IsValid(data) ? new NetPacket(data) : null;
        }

        /// <summary>Tests a payload's validity</summary>
        /// <param name="data">The payload</param>
        /// <returns>True if the payload's checksum is valid, false otherwise</returns>
        private static bool IsValid(byte[] data)
        {
            if (data.Length < 5) return false;
            return BitConverter.ToUInt32(data, 0) == CheckSum(data);
        }

        private NetPacket(byte[] data) { buffer = data; }

        public static implicit operator byte[](NetPacket p) { return p.Data; }

        public byte ReadByte() { return buffer[i++]; }
        public short ReadShort() { i += 2;  return BitConverter.ToInt16(buffer, i - 2); }
        public int ReadInt() { i += 4; return BitConverter.ToInt32(buffer, i - 4); }
        public uint ReadUInt() { i += 4; return BitConverter.ToUInt32(buffer, i - 4); }
        public float ReadFloat() { i += 4; return BitConverter.ToSingle(buffer, i - 4); }

        public string ReadString()
        {
            short len = ReadShort();
            string ret = Net.enc.GetString(buffer, i, len);
            i += len;
            return ret;
        }

        /// <summary>The data read position offset</summary>
        private int i = 5;

        /// <summary>Gets the whole payload, including checksum and packet type</summary>
        internal byte[] Data { get { return buffer; } }

        /// <summary>Gets the packet type</summary>
        internal byte Type { get { return buffer[4]; } }

        /// <summary>Gets the whole payload length, including checksum and packet type</summary>
        internal int Length { get { return buffer.Length; } }

        public class Factory
        {
            private readonly List<byte> buf;

            /// <param name="capacity">Expected packet size, in bytes</param>
            public Factory(int capacity) { buf = new List<byte>(capacity); }

            public void WriteByte(byte b) { buf.Add(b); }
            public void WriteShort(short s) { buf.AddRange(BitConverter.GetBytes(s)); }
            public void WriteInt(int i) { buf.AddRange(BitConverter.GetBytes(i)); }
            public void WriteUInt(uint u) { buf.AddRange(BitConverter.GetBytes(u)); }
            public void WriteFloat(float f) { buf.AddRange(BitConverter.GetBytes(f)); }

            public void WriteString(string s)
            {
                byte[] b = Net.enc.GetBytes(s);
                WriteShort((short)b.Length);
                buf.AddRange(b);
            }

            /// <summary>Constructs the actual packet from the writen data</summary>
            /// <param name="type">Packet type</param>
            /// <returns>A ready packet object</returns>
            public NetPacket GetPacket(byte type) { return NetPacket.Prepare(type, buf); }
        }
    }
}
