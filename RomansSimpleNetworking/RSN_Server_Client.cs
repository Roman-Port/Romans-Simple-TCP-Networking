using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RomansSimpleNetworking
{
    public class RSN_Server_Client
    {
        public TcpClient client;
        public NetworkStream stream;

        public RSN_Server_Client(TcpClient _client)
        {
            client = _client;
            stream = client.GetStream();
        }
    }
}
