using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomansSimpleNetworking
{
    public class RSN_Exception
    {
        public Exception innerException;
        public RSN_Exception_ErrorType type;
        public string description;
    }

    public enum RSN_Exception_ErrorType
    {
        AuthError,
        Exception,
        CantGetType,
        ConnectionLoss
    }
}
