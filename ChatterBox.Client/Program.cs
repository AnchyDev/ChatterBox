using ChatterBox.Client.Network;

using System.Net;

namespace ChatterBox.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var client = new ChatterClient(IPAddress.Parse("127.0.0.1"), 4411);

            await client.ConnectAsync();
        }
    }
}
