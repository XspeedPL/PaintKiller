using System;
using System.Text;
using System.Collections.Generic;

namespace PaintKiller
{
    public abstract class Packet
    {
        private static readonly Dictionary<byte, Packet> Types = new Dictionary<byte, Packet>();
        public static readonly Encoding enc = Encoding.ASCII;

        protected abstract void Process(InPacket data);
        protected abstract void Write(OutPacket data);

        public static void RegisterPacket(byte type, Packet p)
        {
            Types.Add(type, p);
        }
    }

    public class InPacket
    {

    }

    public class OutPacket : List<byte>
    {
        protected void AppendInt(int val) { AddRange(BitConverter.GetBytes(val)); }
        protected void AppendLong(long val) { AddRange(BitConverter.GetBytes(val)); }
        protected void AppendShort(short val) { AddRange(BitConverter.GetBytes(val)); }
        protected void AppendFloat(float val) { AddRange(BitConverter.GetBytes(val)); }
        protected void AppendByte(byte val) { AddRange(BitConverter.GetBytes(val)); }

        protected void AppendString(string val)
        {
            byte[] data = Packet.enc.GetBytes(val);
            AddRange(BitConverter.GetBytes(data.Length));
            AddRange(data);
        }
    }
}
