using System.Text;

namespace ChatterBox.Shared.Network
{
    public class PacketBuilder
    {
        private MemoryStream _ms;
        private BinaryWriter _writer;

        public PacketBuilder(PacketTypes packetType)
        {
            _ms = new MemoryStream();
            _writer = new BinaryWriter(_ms);

            byte[] packetTypeBytes = BitConverter.GetBytes((int)packetType);
            _writer.Write(packetTypeBytes);
        }

        public PacketBuilder AppendInt(int value)
        {
            byte[] valueBytes = BitConverter.GetBytes(value);
            _writer.Write(valueBytes);

            return this;
        }
        public PacketBuilder AppendString(string value)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            _writer.Write(valueBytes);

            return this;
        }

        public byte[] Build()
        {
            return _ms.ToArray();
        }
    }
}
