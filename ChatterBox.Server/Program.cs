using ChatterBox.Server.Network;

using System.Net;

namespace ChatterBox.Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await new ChatterServer(IPAddress.Any, 4411).StartAsync(1000);
        }
    }
}