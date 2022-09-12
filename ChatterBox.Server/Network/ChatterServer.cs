﻿using ChatterBox.Shared.Network;
using System.Net;
using System.Net.Sockets;
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
                .Append<string>(reason, true)
                .Build();

            await PacketHandler.SendAsync(client.GetStream(), disconnectPacket);

            await DisplayMessage($"Disconnecting client '{client.Client.RemoteEndPoint}' for reason '{reason}'.");

            client.Close();
        }

        private async Task<bool> AuthenticateClient(ChatterUser user)
        {
            var packetHandler = new PacketHandler(user.Client.GetStream());

            await DisplayMessage("Waiting for auth payload from " + user.Client.Client.RemoteEndPoint);

            PacketType packetType = (PacketType)await packetHandler.ReadIntAsync();

            await DisplayMessage(">> Received data from " + user.Client.Client.RemoteEndPoint);

            if (packetType != PacketType.Auth)
            {
                return false;
            }

            string authUser = await packetHandler.ReadStringAsync(hasPrependLen: true);

            user.Name = authUser;

            if (connectedClients.Any(c => c.Name == user.Name))
            {
                await ClientDisconnect(user.Client, "There is already a user connected with that username.");
                return false;
            }

            await DisplayMessage("Sending echo to " + user.Client.Client.RemoteEndPoint);

            byte[] echoPacket = new PacketBuilder(PacketType.Auth)
               .Append<string>(user.Name, true)
               .Build();

            await PacketHandler.SendAsync(user.Client.GetStream(), echoPacket);

            return true;
        }

        private async Task DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }

        private async Task<string?> ClientAcceptMessage(ChatterUser user)
        {
            var packetHandler = new PacketHandler(user.Client.GetStream());

            PacketType packetType = (PacketType)await packetHandler.ReadIntAsync();

            if(packetType != PacketType.Message)
            {
                await DisplayMessage("Packet Type not 'Message'");
                return null;
            }

            return await packetHandler.ReadStringAsync(hasPrependLen: true);
        }

        private async Task HandleClient(ChatterUser user)
        {
            if(!await AuthenticateClient(user))
            {
                await ClientDisconnect(user.Client, "Failed to authenticate client.");
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
