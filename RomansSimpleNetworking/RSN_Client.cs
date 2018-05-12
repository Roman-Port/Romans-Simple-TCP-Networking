using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RomansSimpleNetworking
{
    public delegate void RSN_ClientResponse(object data);
    public delegate void RSN_Error(RSN_Exception data);

    public class RSN_Client
    {
        public TcpClient client = null;
        NetworkStream stream = null;

        private BinaryWriter networkWriter;
        private BinaryReader networkReader;

        private RSN_Error errorCallback;

        public bool isAuth = false;

        public string token = "                ";
        public int currentMessage = 0; //Message IDs are used so messages can be sent out of order. Response messages should always have the same ID.
        Dictionary<int, RSN_ClientResponse> callbacks =
            new Dictionary<int, RSN_ClientResponse>(); //Stores callbacks.

        Dictionary<int, Type> registeredDataTypes =
            new Dictionary<int, Type>(); //Stores data parse types.

        public static RSN_Client Connect(RSN_Client_CallbackConfig[] callbacks, string password, RSN_Error onError = null)
        {
            //Create client
            Int32 port = 13000;
            TcpClient client = new TcpClient("10.0.1.13", port);
            
            //Create class
            RSN_Client output = new RSN_Client();
            output.client = client;
            output.stream = client.GetStream();
            output.networkReader = new BinaryReader(output.stream);
            output.networkWriter = new BinaryWriter(output.stream);
            output.errorCallback = onError;

            //Add data parse types
            foreach (RSN_Client_CallbackConfig c in callbacks)
            {
                if(c.id<1)
                {
                    throw new Exception("Bad id: Must be greater than zero.");
                }
                output.registeredDataTypes.Add(c.id, c.type);
            }

            //Begin thread
            var getThread = new System.Threading.Thread(new System.Threading.ThreadStart(output.GetThread));
            getThread.Start();

            //Log into the server
            //Create class
            RSN_AuthPacketType authPacket = new RSN_AuthPacketType();
            authPacket.password = password;
            RSN_Packet packet = new RSN_Packet(output.token, RSN_PacketType.Auth, output.currentMessage,RSN_Tools.SerializeObject(authPacket), 0);
            output.currentMessage++;
            //Write
            output.RawWrite(packet.EncodePacket());
            //When we get a response for that, we'll set our token.

            return output;
        }

        public void SendData<T>(RSN_ClientResponse callback,T data)
        {
            try
            {
                int msgId = currentMessage;
                currentMessage++;
                //Add the callback to the dictonary
                callbacks.Add(msgId, callback);
                //serialize data
                string dataString = RSN_Tools.SerializeObject(data);
                int type = 0;
                try
                {
                    //Reverse lookup the dictonary so we know what type this is.
                    type = registeredDataTypes.FirstOrDefault(x => x.Value == data.GetType()).Key;
                }
                catch
                {
                    //Bad type.
                    throw new Exception("Bad type when sending data! Make sure it is registered.");
                }
                if (type == 0)
                    throw new Exception("Bad type when sending data! Make sure it is registered.");
                //Create a packet
                RSN_Packet packet = new RSN_Packet(token, RSN_PacketType.EncodedMessage, msgId, dataString, type);
                //Submit
                RawWrite(packet.EncodePacket());
            } catch (Exception ex)
            {
                OnError(RSN_Exception_ErrorType.Exception, "", ex);
            }
        }

        private void OnGotMessage(RSN_Packet packet)
        {
            if(packet.type== RSN_PacketType.AuthFail)
            {
                //Error.
                OnError(RSN_Exception_ErrorType.AuthError, "Token provided wasn't accepted by the server. Maybe you're sending requests before the server has served the token?");
                return;
            }

            try
            {
                switch (packet.type)
                {
                    case RSN_PacketType.EncodedMessage:
                        //Find callback
                        RSN_ClientResponse callback = null;
                        try
                        {
                            //Get the callback
                            callback = callbacks[packet.id];
                        }
                        catch
                        {
                            //Wasn't found. Ignore.
                            break;
                        }
                        //Find the data type we should parse this as.
                        Type type = null;
                        try
                        {
                            type = registeredDataTypes[packet.parseType];
                        }
                        catch
                        {
                            //Bad! Ignore
                            return;
                        }
                        Type t = type;
                        //Deserialize
                        object obj = RSN_Tools.DeserializeObject(packet.body, t);
                        callback(obj);
                        break;
                    case RSN_PacketType.Auth:
                        //Auth message. Check if login was okay, then set the token or fail
                        RSN_AuthPacketType auth = (RSN_AuthPacketType)RSN_Tools.DeserializeObject(packet.body, typeof(RSN_AuthPacketType));
                        if (auth.wasAuthOkay)
                        {
                            //OK!
                            token = auth.token;
                            isAuth = true;
                        }
                        else
                        {
                            //Bad...
                            OnError(RSN_Exception_ErrorType.AuthError, "Couldn't be authorized with the server. Check the password.");
                        }
                        break;
                    
                }
            } catch (Exception ex)
            {
                OnError(RSN_Exception_ErrorType.Exception, "Failed to process request. The request goes as follows: '" + packet.body + "'.", ex);
            }
            
        }

        private void OnError(RSN_Exception_ErrorType type, string msg = null, Exception ex = null)
        {
            RSN_Exception err = new RSN_Exception();
            err.type = type;
            err.description = msg;
            err.innerException = ex;
            if(errorCallback!=null)
            {
                //Call
                errorCallback(err);
            }
        }

        private void GetThread()
        {
            while (true)
            {
                if(!client.Connected)
                {
                    //Not connected!
                    OnError(RSN_Exception_ErrorType.ConnectionLoss, "Connection to the server was lost.", null);
                }

                try { 
                    RSN_Packet packet = RSN_Packet.AwaitPacketOverStream(networkReader);
                    if (packet != null)
                    {
                        //Got packet.
                        OnGotMessage(packet);
                    }
                }
                catch (Exception ex)
                {
                    OnError(RSN_Exception_ErrorType.Exception, "", ex);
                }
            }
        }

        private void RawWrite(byte[] data)
        {
            try { 
                networkWriter.Write(data, 0, data.Length);
                networkWriter.Flush();
            }
            catch (Exception ex)
            {
                OnError(RSN_Exception_ErrorType.Exception, "", ex);
            }
        }
    }
}
