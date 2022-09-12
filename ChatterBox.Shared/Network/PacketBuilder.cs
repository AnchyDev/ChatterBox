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

        public PacketBuilder Append<T>(T value, bool prependLen = false)
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
                    if(prependLen)
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

        public byte[] Build()
        {
            return _ms.ToArray();
        }
    }
}
