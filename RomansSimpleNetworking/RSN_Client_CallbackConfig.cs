using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomansSimpleNetworking
{
    public class RSN_Client_CallbackConfig
    {
        //Mostly the same as RSN_Server_CallbackConfig
        public int id;
        public Type type;

        public RSN_Client_CallbackConfig(int _id, Type _type)
        {
            id = _id;
            type = _type;
        }

    }
}
