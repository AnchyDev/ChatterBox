using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ChatterBox.Shared.Network
{
    public class Packet
    {
        public PacketType Type { get; private set; }
        public byte[] Payload { get => _ms.ToArray(); }

        private MemoryStream _ms;
        private BinaryWriter _writer;

        public Packet(PacketType packetType)
        {
            Type = packetType;

            _ms = new MemoryStream();
            _writer = new BinaryWriter(_ms);

            _writer.Write(BitConverter.GetBytes((int)packetType));
        }

        public static Packet Create(PacketType packetType)
        {
            return new Packet(packetType);
        }

        public Packet Append<T>(T value, bool prependLen = false)
        {
            switch (value)
            {
                case int i:
                    if (prependLen)
                    {
                        Append<int>(sizeof(int), false);
                    }

                    _writer.Write(BitConverter.GetBytes(i));
                    break;

                case string s:
                    if (prependLen)
                    {
                        Append<int>(Encoding.UTF8.GetByteCount(s), false);
                    }

                    _writer.Write(Encoding.UTF8.GetBytes(s));
                    break;

                default:
                    throw new InvalidCastException();
            }

            return this;
        }
    }
}
