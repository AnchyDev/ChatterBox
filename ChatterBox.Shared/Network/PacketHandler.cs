using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatterBox.Shared.Network
{
    public class PacketHandler
    {
        private NetworkStream _ns;
        
        public PacketHandler(NetworkStream networkStream)
        {
            _ns = networkStream;
        }

        public async Task<string> ReadStringAsync(int length = 0, bool hasPrependLen = false)
        {
            if (hasPrependLen)
            {
                byte[] pLenBuf = new byte[sizeof(int)];
                await _ns.ReadAsync(pLenBuf, 0, pLenBuf.Length);

                length = BitConverter.ToInt32(pLenBuf);
            }

            if(length == 0)
            {
                throw new Exception("Length parameter must be more than 0.");
            }

            byte[] pReasonBuf = new byte[length];
            await _ns.ReadAsync(pReasonBuf, 0, pReasonBuf.Length);

            return Encoding.UTF8.GetString(pReasonBuf);
        }

        public async Task<int> ReadIntAsync()
        {
            byte[] pLenBuf = new byte[sizeof(int)];
            await _ns.ReadAsync(pLenBuf, 0, pLenBuf.Length);

            return BitConverter.ToInt32(pLenBuf);
        }

        public static async Task SendAsync(NetworkStream networkStream, byte[] payload)
        {
            if (networkStream.CanWrite)
            {
                await networkStream.WriteAsync(payload, 0, payload.Length);
                await networkStream.FlushAsync();
            }
        }
    }
}
