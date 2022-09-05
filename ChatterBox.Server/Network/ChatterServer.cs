using ChatterBox.Shared.Network;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace ChatterBox.Server.Network
{
    public class ChatterServer
    {
        class ClientData
        {
            public TcpClient Client { get; set; }
            public string Name { get; set; }
        }

        private TcpListener _listener;

        private CancellationToken cancellationToken;
        private CancellationTokenSource cancellationTokenSrc;

        private List<ClientData> connectedClients;

        public ChatterServer(IPAddress ipAddress, int port)
        {
            _listener = new TcpListener(ipAddress, port);
            cancellationTokenSrc = new CancellationTokenSource();
            cancellationToken = cancellationTokenSrc.Token;
            connectedClients = new List<ClientData>();
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

        private async Task ClientDisconnect(TcpClient client)
        {
            client.Close();
        }

        private async Task<bool> AuthenticateClient(TcpClient client)
        {
            await DisplayMessage("Waiting for auth payload from " + client.Client.RemoteEndPoint);
            var authPayload = new byte[1024];
            await client.Client.ReceiveAsync(authPayload, SocketFlags.None);

            await DisplayMessage("Received data from " + client.Client.RemoteEndPoint);
            var packetIdBytes = authPayload.Take(sizeof(int));
            var packetId = BitConverter.ToInt32(packetIdBytes.ToArray());

            if (packetId != (int)PacketTypes.Auth)
            {
                client.Client.Close();
            }

            var packetLenBytes = authPayload.Skip(sizeof(int)).Take(sizeof(int));
            var packetLen = BitConverter.ToInt32(packetLenBytes.ToArray());

            var packetPayloadBytes = authPayload.Skip(sizeof(int) + sizeof(int)).Take(packetLen);
            var packetPayload = Encoding.UTF8.GetString(packetPayloadBytes.ToArray());

            if (connectedClients.Any(c => c.Name == packetPayload))
            {
                return false;
            }

            connectedClients.Add(new ClientData()
            {
                Name = packetPayload,
                Client = client
            });

            await DisplayMessage($"Authenticated client " + client.Client.RemoteEndPoint);

            return true;
        }

        private ClientData GetClientFromPool(TcpClient client)
        {
            return connectedClients.FirstOrDefault(c => c.Client.Client.RemoteEndPoint.Equals(client.Client.RemoteEndPoint));
        }

        private async Task DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }

        private async Task<string> ClientAcceptMessage(TcpClient client)
        {
            var messagePayload = new byte[1024];
            await client.Client.ReceiveAsync(messagePayload, SocketFlags.None);

            var packetIdBytes = messagePayload.Take(sizeof(int));
            var packetId = BitConverter.ToInt32(packetIdBytes.ToArray());

            if (packetId != (int)PacketTypes.Message)
            {
                await DisplayMessage("Invalid packet data!");
            }

            var packetLenBytes = messagePayload.Skip(sizeof(int)).Take(sizeof(int));
            var packetLen = BitConverter.ToInt32(packetLenBytes.ToArray());

            var packetPayloadBytes = messagePayload.Skip(sizeof(int) + sizeof(int)).Take(packetLen);
            var packetPayload = Encoding.UTF8.GetString(packetPayloadBytes.ToArray());

            await DisplayMessage("Received data from " + client.Client.RemoteEndPoint);

            return packetPayload;
        }

        private async Task HandleClient(TcpClient client)
        {
            if(!await AuthenticateClient(client))
            {
                await ClientDisconnect(client);
                return;
            }

            ClientData clientData = GetClientFromPool(client);

            if(clientData == null)
            {
                await ClientDisconnect(client);
            }

            while(client.Connected)
            {
                string message = await ClientAcceptMessage(client);
                await DisplayMessage($" {message}");
            }
        }
    }
}
