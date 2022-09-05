using ChatterBox.Shared.Network;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace ChatterBox.Server.Network
{
    public class ChatterServer
    {
        private TcpListener _listener;
        private IPAddress _ipAddress;
        private int _port;

        private CancellationToken cancellationToken;
        private CancellationTokenSource cancellationTokenSrc;

        private List<ChatterUser> connectedClients;

        public ChatterServer(IPAddress ipAddress, int port)
        {
            _listener = new TcpListener(ipAddress, port);
            _ipAddress = ipAddress;
            _port = port;

            cancellationTokenSrc = new CancellationTokenSource();
            cancellationToken = cancellationTokenSrc.Token;
            connectedClients = new List<ChatterUser>();
        }

        public async Task StartAsync(int backlog)
        {
            Console.WriteLine("Starting server..");

            _listener.Start(backlog);

            Console.WriteLine($">> Binded to '{_ipAddress}' and listening on port '{_port}'.");

            Console.WriteLine("Waiting for client connections..");

            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                Console.WriteLine($">> Client {client.Client.RemoteEndPoint} connected.");
                HandleClient(new ChatterUser() { Client = client });
            }

            await Cleanup();
        }

        public void Stop()
        {
            cancellationTokenSrc.Cancel();
        }

        private async Task Cleanup()
        {
            foreach(var client in connectedClients)
            {
                await ClientDisconnect(client.Client, "The server has closed.");
            }

            connectedClients.Clear();

            _listener.Stop();
        }

        private async Task ClientDisconnect(TcpClient client, string reason)
        {

            byte[] disconnectPacket = new PacketBuilder(PacketType.Disconnect)
                .AppendInt(Encoding.UTF8.GetByteCount(reason))
                .AppendString(reason)
                .Build();

            NetworkStream ns = client.GetStream();

            if(ns.CanWrite)
            {
                await ns.WriteAsync(disconnectPacket, 0, disconnectPacket.Length);
                await ns.FlushAsync();
            }

            client.Close();
        }

        private async Task<bool> AuthenticateClient(ChatterUser user)
        {
            await DisplayMessage("Waiting for auth payload from " + user.Client.Client.RemoteEndPoint);

            PacketType packetType = (PacketType)await user.Client.GetStream().GetIntAsync();

            await DisplayMessage(">> Received data from " + user.Client.Client.RemoteEndPoint);

            if (packetType != PacketType.Auth)
            {
                return false;
            }

            int? packetLength = await user.Client.GetStream().GetIntAsync();
            string? packetAuth = await user.Client.GetStream().GetStringAsync(packetLength.Value);

            user.Name = packetAuth;

            return true;
        }

        private async Task DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }

        private async Task<string?> ClientAcceptMessage(ChatterUser user)
        {
            int? packetType = await user.Client.GetStream().GetIntAsync();

            if (!packetType.HasValue)
            {
                await DisplayMessage("Failed to get packetType.");
                return null;
            }

            if((PacketType)packetType != PacketType.Message)
            {
                await DisplayMessage("Packet Type not 'Message'");
                return null;
            }

            int? packetLength = await user.Client.GetStream().GetIntAsync();
            string? packetMessage = await user.Client.GetStream().GetStringAsync(packetLength.Value);

            return packetMessage;
        }

        private async Task HandleClient(ChatterUser user)
        {
            if(!await AuthenticateClient(user))
            {
                await ClientDisconnect(user.Client, "Failed to authenticate client.");
                return;
            }

            if (connectedClients.Contains(user))
            {
                await ClientDisconnect(user.Client, "There is already a user connected from this endpoint.");
                return;
            }

            await DisplayMessage($">> Authenticated client [{user.Client.Client.RemoteEndPoint}]: {user.Name}");

            connectedClients.Add(user);

            while(user.Client.Connected)
            {
                string? message = await ClientAcceptMessage(user);

                if (!string.IsNullOrEmpty(message))
                {
                    await DisplayMessage($">> Received data from [{user.Client.Client.RemoteEndPoint}]: {user.Name}");
                    await DisplayMessage($">> {user.Name}: {message}");
                }
            }
        }
    }
}
