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

            await Init();
        }

        private async Task Init()
        {
            var sizeOfPayload = Encoding.UTF8.GetByteCount(Username);
            var authPayload = new PacketBuilder(PacketType.Auth).AppendInt(sizeOfPayload).AppendString(Username).Build();

            await tcpClient.Client.SendAsync(authPayload, SocketFlags.None);

            while(true)
            {
                Console.Write("Enter message: ");
                string message = Console.ReadLine();

                if (!string.IsNullOrEmpty(message))
                {
                    sizeOfPayload = Encoding.UTF8.GetByteCount(message);
                    var messagePayload = new PacketBuilder(PacketType.Message).AppendInt(sizeOfPayload).AppendString(message).Build();
                    await tcpClient.Client.SendAsync(messagePayload, SocketFlags.None);
                }
            }
        }
    }
}
