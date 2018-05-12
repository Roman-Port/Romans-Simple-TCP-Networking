using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RomansSimpleNetworking
{
    [DataContract]
    class RSN_AuthPacketType
    {
        //Just used for serialization and auth
        [DataMember]
        public string password;

        [DataMember]
        public bool wasAuthOkay = false;

        [DataMember]
        public string token;
    }
}
