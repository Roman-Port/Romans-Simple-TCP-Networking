using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomansSimpleNetworking
{
    public class RSN_ServerResponse_Data
    {
        public Type t;
        public RSN_Server_Client client;
        public RSN_Packet packet;

        public RSN_ServerResponse_Data(RSN_Packet _packet, RSN_Server_Client _client, Type _type, int _id)
        {
            t = _type;
            client = _client;
            packet = _packet;
        }

        public void Respond<T>(T obj)
        {
            //Serialize
            string data = RSN_Tools.SerializeObject(obj);
            //Create a response packet and send.
            RSN_Server.SendDataToClient(client.stream, data, packet.id, packet.parseType, type: RSN_PacketType.EncodedMessage);
        }
    }
}
