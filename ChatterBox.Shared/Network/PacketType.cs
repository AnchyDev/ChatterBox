using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatterBox.Shared.Network
{
    public enum PacketType
    {
        Auth = 1,
        Message = 2,
        Disconnect = 3
    }
}
