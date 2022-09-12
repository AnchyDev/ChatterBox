using ChatterBox.Shared.Network;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatterBox.Client.Network
{
    public class ChatterClient
    {
        public string Username { get; set; }

        private TcpClient tcpClient;

        private IPAddress ipAddress;
        private int port;

        public ChatterClient(IPAddress ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            this.port = port;

            tcpClient = new TcpClient();

            this.Username = "Unnamed";
        }

        public void SetUsername(string newName)
        {
            if(string.IsNullOrWhiteSpace(newName))
            {
                return;
            }

            Username = newName;
        }

        public async Task ConnectAsync()
        {
            tcpClient.Connect(ipAddress, port);

            await Authenticate();
        }

        private async Task Authenticate()
        {
            var authPacket = new PacketBuilder(PacketType.Auth)
                .Append<string>(Username, true)
                .Build();

            NetworkStream ns = tcpClient.GetStream();

            if (ns.CanWrite)
            {
                await ns.WriteAsync(authPacket, 0, authPacket.Length);
                await ns.FlushAsync();
            }

            byte[] pTypeBuf = new byte[sizeof(int)];
            await ns.ReadAsync(pTypeBuf, 0, pTypeBuf.Length);

            PacketType packetType = (PacketType)BitConverter.ToInt32(pTypeBuf);

            if(packetType == PacketType.Auth)
            {
                byte[] pLenBuf = new byte[sizeof(int)];
                await ns.ReadAsync(pLenBuf, 0, pLenBuf.Length);

                int packetLen = BitConverter.ToInt32(pLenBuf);

                byte[] pEchoBuf = new byte[packetLen];
                await ns.ReadAsync(pEchoBuf, 0, pEchoBuf.Length);

                string packetEcho = Encoding.UTF8.GetString(pEchoBuf);

                if(packetEcho == Username)
                {
                    await MessageLoop();
                }
            }

            if (packetType == PacketType.Disconnect)
            {
                byte[] pLenBuf = new byte[sizeof(int)];
                await ns.ReadAsync(pLenBuf, 0, pLenBuf.Length);

                int packetLen = BitConverter.ToInt32(pLenBuf);

                byte[] pReasonBuf = new byte[packetLen];
                await ns.ReadAsync(pReasonBuf, 0, pReasonBuf.Length);

                string packetReason = Encoding.UTF8.GetString(pReasonBuf);

                Console.WriteLine($"Disconnected for reason '{packetReason}'.");
            }
        }

        private async Task MessageLoop()
        {
            while(true)
            {
                Console.Write("Enter message: ");
                string message = Console.ReadLine();

                if (!string.IsNullOrEmpty(message))
                {
                    var messagePacket = new PacketBuilder(PacketType.Message)
                        .Append<string>(message, true)
                        .Build();

                    await tcpClient.Client.SendAsync(messagePacket, SocketFlags.None);
                }
            }
        }
    }
}
