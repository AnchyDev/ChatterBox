using ChatterBox.Client.Network;

using System.Net;

namespace ChatterBox.Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            new ChatterClient(IPAddress.Parse("margo-canoe.bnr.la"), 4411).Connect();
        }
    }
}
