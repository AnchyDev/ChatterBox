using ChatterBox.Shared.Network;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatterBox.Server.Network
{
    public class ChatterServer
    {
        private TcpListener _listener;

        private CancellationToken cancellationToken;
        private CancellationTokenSource cancellationTokenSrc;

        public ChatterServer(IPAddress ipAddress, int port)
        {
            _listener = new TcpListener(ipAddress, port);
            cancellationTokenSrc = new CancellationTokenSource();
            cancellationToken = cancellationTokenSrc.Token;
        }

        public async Task StartAsync(int backlog)
        {
            _listener.Start(backlog);

            Console.WriteLine("Listening for client connections..");

            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                Console.WriteLine($"Client {client.Client.RemoteEndPoint} connected.");
                HandleClient(client);
            }
        }

        public void Stop()
        {
            cancellationTokenSrc.Cancel();
        }

        private async Task HandleClient(TcpClient client)
        {
            Console.WriteLine("Waiting for auth payload from " + client.Client.RemoteEndPoint);
            var authPayload = new byte[1024];
            await client.Client.ReceiveAsync(authPayload, SocketFlags.None);

            Console.WriteLine("Received data from " + client.Client.RemoteEndPoint);
            var packetIdBytes = authPayload.Take(sizeof(int));
            var packetId = BitConverter.ToInt32(packetIdBytes.ToArray());

            if(packetId != (int)PacketTypes.Auth)
            {
                client.Client.Close();
            }

            var packetLenBytes = authPayload.Skip(sizeof(int)).Take(sizeof(int));
            var packetLen = BitConverter.ToInt32(packetLenBytes.ToArray());

            var packetPayloadBytes = authPayload.Skip(sizeof(int) + sizeof(int)).Take(packetLen);
            var packetPayload = Encoding.UTF8.GetString(packetPayloadBytes.ToArray());

            Console.WriteLine("Welcome " + packetPayload);
        }
    }
}
