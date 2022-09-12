using System.Text;

namespace ChatterBox.Shared.Network
{
    public class PacketBuilder
    {
        private MemoryStream _ms;
        private BinaryWriter _writer;

        public PacketBuilder(PacketType packetType)
        {
            _ms = new MemoryStream();
            _writer = new BinaryWriter(_ms);

            byte[] packetTypeBytes = BitConverter.GetBytes((int)packetType);
            _writer.Write(packetTypeBytes);
        }

        public PacketBuilder Append<T>(T value)
        {
            switch (value)
            {
                case int i:
                    _writer.Write(BitConverter.GetBytes(i));
                    break;

                case string s:
                    _writer.Write(Encoding.UTF8.GetBytes(s));
                    break;

                default:
                    throw new InvalidCastException();
            }

            return this;
        }

        public byte[] Build()
        {
            return _ms.ToArray();
        }
    }
}
