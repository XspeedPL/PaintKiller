using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PaintKiller
{
    internal sealed class NetPacket
    {
        private readonly byte[] buffer;

        private static uint CheckSum(List<byte> data)
        {
            uint ret = 0;
            foreach (byte b in data) ret += b;
            return ret;
        }

        private static uint CheckSum(byte[] data)
        {
            uint ret = 0;
            for (int i = 4; i < data.Length; ++i) ret += data[i];
            return ret;
        }

        internal static NetPacket Prepare(byte type, List<byte> data)
        {
            data.Insert(0, type);
            data.InsertRange(0, BitConverter.GetBytes(CheckSum(data)));
            return new NetPacket(data.ToArray());
        }

        internal static NetPacket Prepare(byte type)
        {
            List<byte> data = new List<byte>();
            data.Add(type);
            data.InsertRange(0, BitConverter.GetBytes(CheckSum(data)));
            return new NetPacket(data.ToArray());
        }

        internal static NetPacket Get(byte[] data)
        {
            return IsValid(data) ? new NetPacket(data) : null;
        }

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

        private int i = 5;

        internal byte[] Data { get { return buffer; } }
        internal byte Type { get { return buffer[4]; } }
        internal int Length { get { return buffer.Length; } }

        public class Writer
        {
            private readonly List<byte> buf;

            public Writer(int capacity) { buf = new List<byte>(capacity); }

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

            public NetPacket GetPacket(byte type) { return NetPacket.Prepare(type, buf); }
        }
    }
}
