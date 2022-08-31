using System.Net;
using System.Net.Sockets;

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

        public void Connect()
        {
            tcpClient.Connect(ipAddress, port);
        }
    }
}
