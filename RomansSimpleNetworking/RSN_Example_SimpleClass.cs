using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RomansSimpleNetworking
{
    [DataContract]
    public class RSN_Example_SimpleClass
    {
        [DataMember]
        public string name;
        [DataMember]
        public int age;
        [DataMember]
        public bool canDrive; 
    }
}
