using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;

namespace RomansSimpleNetworking
{
    public class RSN_Packet
    {
        //Used for data exchange.
        public string token;
        public RSN_PacketType type; //None, auth, message
        public int id; //Used by the user of this application. Might be used for callbacks? 
        public string body;
        public int parseType;

        public RSN_Packet(string _token, RSN_PacketType _type, int _id, string _body, int _parseType)
        {
            body = _body;
            token = _token;
            type = _type;
            id = _id;
            parseType = _parseType;
        }

        public byte[] EncodePacket()
        {
            byte[] bodyBytes = Encoding.ASCII.GetBytes(body);
            byte[] tokenBytes = Encoding.ASCII.GetBytes(token);
            //Check to see if tokenBytes is length of 16 like it is required to.
            if(tokenBytes.Length!=16)
            {
                //Throw an error
                throw new Exception("Token length is not 16 like required.");
            }
            byte[] bodyLengthBytes = ConvertIntToByte(bodyBytes.Length);
            byte[] typeBytes = ConvertIntToByte((int)type);
            byte[] friendlyTypeBytes = ConvertIntToByte(id);
            byte[] parseTypeBytes = ConvertIntToByte(parseType);
            //Put this into a buffer.
            int packetLength = bodyLengthBytes.Length + typeBytes.Length + friendlyTypeBytes.Length + tokenBytes.Length + bodyBytes.Length + parseTypeBytes.Length+ 1;
            byte[] buf = new byte[packetLength];
            MemoryStream stream = new MemoryStream(buf);
            //Write data
            stream.Write(bodyLengthBytes, 0, bodyLengthBytes.Length);
            stream.Write(typeBytes, 0, typeBytes.Length);
            stream.Write(friendlyTypeBytes, 0, friendlyTypeBytes.Length);
            stream.Write(parseTypeBytes, 0, parseTypeBytes.Length);
            stream.Write(tokenBytes, 0, tokenBytes.Length);
            stream.Write(bodyBytes, 0, bodyBytes.Length);
            //Add padding.
            stream.Write(new byte[1], 0, 1);
            //Close stream
            stream.Close();
            //Return buffer
            return buf;
        }

        public static RSN_Packet DecodePacket(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data); //Open stream on data.
            //Read in first three ints.
            int bodyLength = ConvertByteToInt(ReadFromBufferAtPos(stream, 4));
            int packetType = ConvertByteToInt(ReadFromBufferAtPos(stream, 4));
            int friendlyPacketType = ConvertByteToInt(ReadFromBufferAtPos(stream, 4));
            int parseType = ConvertByteToInt(ReadFromBufferAtPos(stream, 4));
            //Read in 16 bytes of token.
            string token = Encoding.ASCII.GetString(ReadFromBufferAtPos(stream, 16));
            //Read the body in
            string body = Encoding.ASCII.GetString(ReadFromBufferAtPos(stream, bodyLength));
            //Read the byte of padding and discard the data
            ReadFromBufferAtPos(stream, 1);
            //Set packet data
            RSN_Packet packet = new RSN_Packet(token,(RSN_PacketType)packetType,friendlyPacketType,body, parseType);
            //Close stream
            stream.Close();
            //return
            return packet;
        }

        private static byte[] ReadFromBufferAtPos(MemoryStream stream,int length)
        {
            byte[] buf = new byte[length];
            stream.Read(buf, 0, length);
            return buf;
        }

        public static byte[] ConvertIntToByte(int input)
        {
            byte[] bytes = BitConverter.GetBytes(input);
            
            return bytes;
        }

        public static int ConvertByteToInt(byte[] input)
        {
            //Reverse if incorrect
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(input);
            int i = BitConverter.ToInt32(input, 0);
            return i;
        }

        public static int GetEntirePacketLengthByLengthData(int len)
        {
            return 29 + len;
        }


        public static RSN_Packet AwaitPacketOverStream(BinaryReader stream)
        {
            try
            {
                //Read in the data
                byte[] lenBuf = new byte[4];
                //Read header length property.
                stream.Read(lenBuf, 0, 4);
                int length = RSN_Packet.ConvertByteToInt(lenBuf);
                //If length is zero, ignore
                if (length == 0)
                    return null;
                //Read the remainder of the data.
                int len = RSN_Packet.GetEntirePacketLengthByLengthData(length);
                byte[] bodyBuf = new byte[len];
                int i = 0;
                stream.Read(bodyBuf, 0, bodyBuf.Length);
                //Combine both of these byte arrays.
                byte[] buf = new byte[bodyBuf.Length + 4];
                using (MemoryStream memStream = new MemoryStream(buf))
                {
                    memStream.Write(lenBuf, 0, 4);
                    memStream.Write(bodyBuf, 0, bodyBuf.Length);
                }
                //Buf now contains the packet data.
                //We now have the sent data. Decode it into a packet.
                RSN_Packet packet = RSN_Packet.DecodePacket(buf);
                
                return packet;
            } catch
            {
                return null;
            }
        }

        public static RSN_Packet AwaitPacketOverStream(NetworkStream stream)
        {
            try
            {
                //Read in the data
                byte[] lenBuf = new byte[4];
                //Read header length property.
                stream.Read(lenBuf, 0, 4);
                int length = RSN_Packet.ConvertByteToInt(lenBuf);
                //If length is zero, ignore
                if (length == 0)
                    return null;
                //Read the remainder of the data.
                int len = RSN_Packet.GetEntirePacketLengthByLengthData(length);
                byte[] bodyBuf = new byte[len];
                int i = 0;
                stream.Read(bodyBuf, 0, bodyBuf.Length);
                //Combine both of these byte arrays.
                byte[] buf = new byte[bodyBuf.Length + 4];
                using (MemoryStream memStream = new MemoryStream(buf))
                {
                    memStream.Write(lenBuf, 0, 4);
                    memStream.Write(bodyBuf, 0, bodyBuf.Length);
                }
                //Buf now contains the packet data.
                //We now have the sent data. Decode it into a packet.
                RSN_Packet packet = RSN_Packet.DecodePacket(buf);

                return packet;
            }
            catch
            {
                return null;
            }
        }

        public static void ClearStream(NetworkStream stream)
        {
            byte[] buf = new byte[2048];
            
            while (stream.Read(buf, 0, buf.Length) > 0) ;
        }
    }

    public enum RSN_PacketType
    {
        None,
        Auth,
        EncodedMessage,
        AuthFail
    }
}
