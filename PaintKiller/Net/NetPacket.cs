using System;
using System.IO;

namespace PaintKilling.Net
{
    internal sealed class NetPacket : BinaryReader
    {
        internal enum PType : byte
        {
            SLobbyJoin = 0, SControls, SDisconnect,
            CLobbyConnect, CEntList, CGameEnd, CLobbyJoin, CPause
        }

        private byte[] Buffer { get; }

        /// <summary>Calculates the payload's checksum, ignores first 4 bytes</summary>
        /// <param name="data">The payload</param>
        private static uint CheckSum(byte[] data)
        {
            uint ret = 0;
            for (int i = 4; i < data.Length; ++i) ret += data[i];
            return ret;
        }

        /// <summary>Constructs an empty packet</summary>
        /// <param name="type">Packet type</param>
        internal static NetPacket Prepare(PType type)
        {
            byte[] data = new byte[5];
            data[4] = (byte)type;
            BitConverter.GetBytes(CheckSum(data)).CopyTo(data, 0);
            return new NetPacket(data);
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

        private NetPacket(byte[] data) : base(new MemoryStream(data)) { Buffer = data; BaseStream.Seek(5, SeekOrigin.Current); }

        /// <summary>Gets the whole payload, including checksum and packet type</summary>
        internal byte[] Data => Buffer;

        /// <summary>Gets the packet type</summary>
        internal PType Type => (PType)Buffer[4];

        /// <summary>Gets the whole payload length, including checksum and packet type</summary>
        internal int Length => Buffer.Length;

        internal sealed class Factory : BinaryWriter
        {
            /// <param name="type">Packet type</param>
            /// <param name="capacity">Expected packet size, in bytes</param>
            public Factory(PType type, int capacity) : base(new MemoryStream(capacity + 5))
            {
                Write(int.MaxValue);
                Write((byte)type);
            }

            private byte[] GetData() => ((MemoryStream)BaseStream).ToArray();

            /// <summary>Constructs the actual packet from the writen data</summary>
            /// <param name="type">Packet type</param>
            /// <returns>A ready packet object</returns>
            public NetPacket GetPacket()
            {
                byte[] buf = GetData();
                BitConverter.GetBytes(CheckSum(buf)).CopyTo(buf, 0);
                return new NetPacket(buf);
            }
        }
    }
}
