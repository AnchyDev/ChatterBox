using ChatterBox.Shared.Network;
using System.Net;
using System.Net.Sockets;
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

            PacketType packetType = (PacketType)await GetIntFromStreamAsync(client.GetStream());

            await DisplayMessage("Received data from " + client.Client.RemoteEndPoint);

            if (packetType != PacketType.Auth)
            {
                return false;
            }

            int packetLength = await GetIntFromStreamAsync(client.GetStream());
            string packetAuth = await GetStringFromStreamAsync(client.GetStream(), packetLength);

            if (connectedClients.Any(c => c.Name == packetAuth))
            {
                return false;
            }

            connectedClients.Add(new ClientData()
            {
                Name = packetAuth,
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
            PacketType packetType = (PacketType)await GetIntFromStreamAsync(client.GetStream());

            if (packetType != PacketType.Message)
            {
                await DisplayMessage("Invalid packet data!");
                return string.Empty;
            }

            int packetLength = await GetIntFromStreamAsync(client.GetStream());
            string packetMessage = await GetStringFromStreamAsync(client.GetStream(), packetLength);

            await DisplayMessage("Received data from " + client.Client.RemoteEndPoint);

            return packetMessage;
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

                if(!string.IsNullOrEmpty(message))
                    await DisplayMessage($"{message}");
            }
        }

        private async Task<int> GetIntFromStreamAsync(NetworkStream ns)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[4];

            do
            {
                bytesRead = await ns.ReadAsync(buffer);
            }
            while (bytesRead < buffer.Length);

            return BitConverter.ToInt32(buffer);
        }

        private async Task<string> GetStringFromStreamAsync(NetworkStream ns, int len)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[len];

            do
            {
                bytesRead = await ns.ReadAsync(buffer);
            }
            while (bytesRead < buffer.Length);

            return Encoding.UTF8.GetString(buffer);
        }
    }
}
