using System.Net.Sockets;
using System.Text;

namespace ChatterBox.Shared.Network
{
    public static class NetworkStreamExtensions
    {
        public static async Task<byte[]> GetBytesAsync(this NetworkStream ns, int length)
        {
            if (!ns.CanRead)
            {
                return null;
            }

            int bytesRead = 0;
            byte[] buffer = new byte[length];

            await ns.ReadAsync(buffer, 0, length);

            return buffer;
        }

        public static async Task<int?> GetIntAsync(this NetworkStream ns)
        {
            byte[] buffer = await ns.GetBytesAsync(sizeof(int));

            if(buffer == null)
            {
                return null;
            }

            return BitConverter.ToInt32(buffer);
        }

        public static async Task<string?> GetStringAsync(this NetworkStream ns, int length)
        {
            byte[] buffer = await ns.GetBytesAsync(length);

            if (buffer == null)
            {
                return null;
            }

            return Encoding.Default.GetString(buffer);
        }
    }
}