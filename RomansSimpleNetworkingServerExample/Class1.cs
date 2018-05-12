using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomansSimpleNetworkingServerExample
{
    public class Class1
    {
        Thread.Sleep(1000);
            Console.WriteLine("starting to write  data");
            RSN_Packet packet = new RSN_Packet("1234567890123456", RSN_PacketType.Auth, 0, "Woah there, packet!");
        byte[] data = packet.EncodePacket();
        RSN_Client client = RSN_Client.Connect();
        client.RawWrite(data);
    }
}
