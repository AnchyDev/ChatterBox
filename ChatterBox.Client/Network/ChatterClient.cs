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

            var packetHandler = new PacketHandler(ns);

            PacketType packetType = (PacketType)await packetHandler.ReadIntAsync();

            if(packetType == PacketType.Auth)
            {
                string authEcho = await packetHandler.ReadStringAsync(hasPrependLen: true);

                if(authEcho == Username)
                {
                    await MessageLoop();
                }
            }

            if (packetType == PacketType.Disconnect)
            {
                string dcReason = await packetHandler.ReadStringAsync(hasPrependLen: true);

                Console.WriteLine($"Disconnected for reason '{dcReason}'.");
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
