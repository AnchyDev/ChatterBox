using ChatterBox.Shared.Network;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace ChatterBox.Client.Network
{
    public class ChatterClient
    {
        private TcpClient tcpClient;

        private IPAddress ipAddress;
        private int port;

        public ChatterClient(IPAddress ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            this.port = port;

            tcpClient = new TcpClient();
        }

        public async Task ConnectAsync()
        {
            tcpClient.Connect(ipAddress, port);

            await Init();
        }

        private async Task Init()
        {
            string name = "AnchyDev";
            var sizeOfPayload = Encoding.UTF8.GetByteCount(name);
            var authPayload = new PacketBuilder(PacketTypes.Auth).AppendInt(sizeOfPayload).AppendString("AnchyDev").Build();

            await tcpClient.Client.SendAsync(authPayload, SocketFlags.None);

            while(true)
            {
                await Task.Delay(100);
            }
        }
    }
}
