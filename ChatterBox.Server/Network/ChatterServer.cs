using System.Net;
using System.Net.Sockets;

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

            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                HandleClient(client);
            }
        }

        public void Stop()
        {
            cancellationTokenSrc.Cancel();
        }

        private async Task HandleClient(TcpClient client)
        {

        }
    }
}
