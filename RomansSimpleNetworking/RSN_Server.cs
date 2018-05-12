using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RomansSimpleNetworking
{

    public delegate void RSN_ServerResponse(object raw, RSN_ServerResponse_Data data);
    public class RSN_Server
    {
        private TcpListener server = null;
        private List<RSN_Server_Client> clients = new List<RSN_Server_Client>();

        private List<string> registeredTokens = new List<string>();

        private string password;

        Dictionary<int, RSN_Server_CallbackConfig> registeredDataTypes =
            new Dictionary<int, RSN_Server_CallbackConfig>(); //Stores data parse types.

        public static RSN_Server CreateServer(RSN_Server_CallbackConfig[] callbacks, string password, Int32 port)
        {
            RSN_Server ser = new RSN_Server();
            ser.password = password;
            TcpListener server = null; //Create a place for the server
            IPAddress localAddr = IPAddress.Any; //Setup the listening IP.
            server = new TcpListener(localAddr, port); //Create listener
            server.Start();
            ser.server = server;

            //Look for new clients
            ser.server.BeginAcceptTcpClient(new AsyncCallback(ser.OnNewClient), ser.server);

            //Add new callbacks
            foreach(var c in callbacks)
            {
                ser.registeredDataTypes.Add(c.id, c);
            }

            //Create the get thread
            var getThread = new System.Threading.Thread(new System.Threading.ThreadStart(ser.GetThread));
            getThread.Start();

            
            return ser;
        }

        public static void SendDataToClient(NetworkStream stream, string body, int messageId, int parseType, string token = "                ", RSN_PacketType type = RSN_PacketType.EncodedMessage)
        {
            //Create packet
            RSN_Packet packet = new RSN_Packet(token, type, messageId, body,parseType);
            //Decode
            byte[] data = packet.EncodePacket();
            //Send it over
            stream.Write(data, 0, data.Length);
        }

        private void OnNewClient(System.IAsyncResult result)
        {
            TcpListener listener = (TcpListener)result.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(result);
            //Found client. Add it to the list.
            var c = new RSN_Server_Client(client);
            clients.Add(c);
            //Wait again
            //Look for new clients
            server.BeginAcceptTcpClient(new AsyncCallback(OnNewClient), server);
        }

        private bool VerifyToken(string token)
        {
            try
            {
                foreach (string t in registeredTokens)
                {
                    if (token == t)
                        return true;
                }
                return false;
            } catch
            {
                return false;
            }
        }

        private void OnGotData(RSN_Server_Client client, RSN_Packet packet)
        {
            //Called on new data. 
            switch(packet.type)
            {
                case RSN_PacketType.EncodedMessage:
                    //Handle.
                    //Verify token. If the token is incorrect, ignore the packet.
                    if(!VerifyToken(packet.token))
                    {
                        //Incorrect. Send a packet telling the user this.
                        SendDataToClient(client.stream, "{}", packet.id, 0, "                ", RSN_PacketType.AuthFail);
                        return;
                    }
                    //Get the callback and type.
                    RSN_Server_CallbackConfig conf = null;
                    try
                    {
                        conf = registeredDataTypes[packet.parseType];
                    }
                    catch
                    {
                        //Bad. Ignore!
                        return;
                    }
                    conf.OnEvent(packet.body, client, packet);
                    break;
                case RSN_PacketType.Auth:
                    //Trying to auth. Check the password.
                    RSN_AuthPacketType auth = (RSN_AuthPacketType)RSN_Tools.DeserializeObject(packet.body, typeof(RSN_AuthPacketType));
                    string token = RSN_Tools.GenerateRandomString(16);
                    if (auth.password == password)
                    {
                        //Ok!
                        auth.wasAuthOkay = true;
                        registeredTokens.Add(token);
                        auth.token = token;
                    } else
                    {
                        //Bad!
                        auth.wasAuthOkay = false;
                        auth.token = "                ";
                    }
                    //Respond
                    string raw = RSN_Tools.SerializeObject(auth);
                    SendDataToClient(client.stream, raw, packet.id, 0, token, RSN_PacketType.Auth);
                    break;
            }
        }

        private void GetThread()
        {
            Exception ex = null;
            while (true)
            {
                //Loop through clients
                try
                {
                    foreach (var client in clients)
                    {
                        try
                        {
                            if(!client.client.Connected)
                            {
                                //Disconnected
                                client.client.Close();
                                client.client.Dispose();
                                clients.Remove(client);
                            }

                            RSN_Packet packet = RSN_Packet.AwaitPacketOverStream(client.stream);
                            //All response actions must take place here.
                            OnGotData(client,packet);
                        } catch (Exception innerEx)
                        {
                            ex = innerEx;
                        }
                    }
                } catch
                {
                    //This is always an error from the foreach. Just do it again
                }
            }
        }

    }
}
