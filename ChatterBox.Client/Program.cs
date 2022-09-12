﻿using ChatterBox.Client.Network;

using System.Net;

namespace ChatterBox.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string? username = string.Empty;

            do
            {
                Console.Write("Enter Username: ");
                username = Console.ReadLine();
            }
            while (string.IsNullOrEmpty(username));

            //var client = new ChatterClient(Dns.GetHostEntry("localhost").AddressList[0], 4411);
            var client = new ChatterClient(IPAddress.Parse("127.0.0.1"), 4411);
            client.SetUsername(username);
            await client.ConnectAsync();
            Console.ReadLine();
        }
    }
}
