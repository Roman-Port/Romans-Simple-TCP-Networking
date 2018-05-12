using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomansSimpleNetworking
{
    public class RSN_Server_CallbackConfig
    {
        //Used to let the server know what to do when a request for a specific id comes in.
        public int id; //ID to be used
        public RSN_ServerResponse callback; //Callback
        public Type deserializeAs; //The data type to deserialize this as.

        public void OnEvent(string raw, RSN_Server_Client client, RSN_Packet packet)
        {
            //Deserialize this
            object des = RSN_Tools.DeserializeObject(raw, deserializeAs);
            //Call callback
            callback(des,new RSN_ServerResponse_Data(packet,client,deserializeAs,packet.parseType));
        }

        public RSN_Server_CallbackConfig(int _id, RSN_ServerResponse _callback, Type _type)
        {
            deserializeAs = _type;
            callback = _callback;
            id = _id;
        }
    }
}
