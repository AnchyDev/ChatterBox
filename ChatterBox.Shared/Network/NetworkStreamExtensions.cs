using System.Net.Sockets;
using System.Text;

namespace ChatterBox.Shared.Network
{
    public static class NetworkStreamExtensions
    {
        public static async Task<int?> GetIntAsync(this NetworkStream ns, CancellationToken cancelToken)
        {
            if (!ns.CanRead)
            {
                return null;
            }

            int bytesRead = 0;
            byte[] buffer = new byte[sizeof(int)];

            do
            {
                bytesRead += await ns.ReadAsync(buffer, 0, sizeof(int));
            }
            while (ns.DataAvailable && bytesRead != buffer.Length && !cancelToken.IsCancellationRequested);

            if (cancelToken.IsCancellationRequested)
            {
                return null;
            }

            return BitConverter.ToInt32(buffer);
        }

        public static async Task<string?> GetStringAsync(this NetworkStream ns, int length, CancellationToken cancelToken)
        {
            if(!ns.CanRead)
            {
                return null;
            }

            int bytesRead = 0;
            byte[] buffer = new byte[length];

            do
            {
                bytesRead += await ns.ReadAsync(buffer, 0, length);
            }
            while (ns.DataAvailable && bytesRead != buffer.Length && !cancelToken.IsCancellationRequested);

            if(cancelToken.IsCancellationRequested)
            {
                return null;
            }

            return Encoding.Default.GetString(buffer);
        }
    }
}
