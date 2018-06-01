import socket
import json


#This was a simple client I made for a Python program. I don't use Python very much, so don't get mad at me.


class RPPacket:
    
    def __init__(self,packetType,msgId,parseType,token,body):
        self.packetType = packetType
        self.msgId = msgId
        self.parseType = parseType
        self.token = token
        self.body = body
    
    @staticmethod
    def CreatePacket(packetType, msgId, parseType, token, body):
        #Create packet to send.
        data = bytearray()
        #Convert strings.
        a_body = RPPacket.ToByteFromString(body)
        a_token = RPPacket.ToByteFromString(token)
        #Add to the data
        data = RPPacket.WriteBytesToBytearray(data, RPPacket.ToByteFromInt(len(a_body)) ) #Write int32 body size
        data = RPPacket.WriteBytesToBytearray(data, RPPacket.ToByteFromInt( packetType ) ) #Write int32 packet type
        data = RPPacket.WriteBytesToBytearray(data, RPPacket.ToByteFromInt( msgId ) ) #Write int32 packet id
        data = RPPacket.WriteBytesToBytearray(data, RPPacket.ToByteFromInt( parseType ) ) #Write int32 parse type for the server
        
        #Write strings, first the access token.
        data = RPPacket.WriteBytesToBytearray(data, a_token)
        #Now, write the body
        data = RPPacket.WriteBytesToBytearray(data, a_body)
        #Write end padding
        data.append(0)
        #Data has been written
        return data
    
    @staticmethod
    def ImportPacket(raw):
        #This will take in the data and turn it back into a class.
        bodyLen = RPPacket.FromByteToInt( RPPacket.SubArray(raw,0,4) ) #Read in int32 body length
        packetType = RPPacket.FromByteToInt( RPPacket.SubArray(raw,4,4) ) #Read in int32 packet type
        messageId = RPPacket.FromByteToInt( RPPacket.SubArray(raw,8,4) ) #Read in int32 message id
        parseType = RPPacket.FromByteToInt( RPPacket.SubArray(raw,12,4) ) #Read in int32 parse type
        #Now read in the strings
        token = RPPacket.FromByteToString( RPPacket.SubArray(raw,16,16) ) 
        body = RPPacket.FromByteToString( RPPacket.SubArray(raw,32,bodyLen) ) 
        
        #Pack this into the packet
        out = RPPacket(packetType,messageId,parseType,token,body)
        return out
        
    
    @staticmethod
    def SubArray(data,offset,length):
        i = 0
        output = bytearray()
        while(i<length):
            output.append(data[i+offset])
            i+=1
        return output
      
    @staticmethod     
    def WriteBytesToBytearray(data,write):
        i = 0
        while(i<len(write)):
            data.append(write[i])
            i+=1
        return data
    
    @staticmethod
    def DebugWriteToDisk(data):
        f = open("C:\\Users\\Roman\\Desktop\\test.bin",'wb')
        f.write(data)
        f.close()
    
    @staticmethod
    def ToByteFromInt(data):
        return int(data).to_bytes(4,byteorder='little')
    
    @staticmethod
    def FromByteToInt(data):
        i = int(0)
        return i.from_bytes(data,byteorder='little')  
    
    @staticmethod
    def ToByteFromString(data):
        return bytes(data, 'ascii')
    
    @staticmethod
    def FromByteToString(data):
        return str(data,'ascii')   

class TcpConnection:
    def __init__(self):
        self.CONN_IP = '10.0.1.13';
        self.CONN_PORT = 13000
        self.CONN_BUFFER_SIZE = 9999
        self.token = "                "
        self.password = "hello"
        
        #connect
        self.conn = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.conn.connect((self.CONN_IP, self.CONN_PORT))
        print("Connected")
        #Login and handshake with the server. We'll need to get a token.
        #Create data
        loginData = {
            "password":self.password,
            "wasAuthOkay":False,
            "token":""
        }
        loginRes = self.SendData(TcpConnection.SerJSON(loginData),0,self.token,1)
        #Check the handshake status
        #Read in
        logReq = TcpConnection.DeSerJSON(loginRes.body)
        if(logReq["wasAuthOkay"]==True):
            print("RSN auth okay!")
        else:
            print("Auth failed. "+logReq["wasAuthOkay"])
        
    def SendData(self,body,parseType, token=None, packetType=0):
        if(token==None):
            token = self.token
        #Create packet.
        data = RPPacket.CreatePacket(packetType,0,parseType,token,body)
        #Send this data.
        self.conn.send(data)
        print("Sent")
        #Get
        got = self.conn.recv(self.CONN_BUFFER_SIZE)
        print("Got")
        #Convert this to a readable packet
        packet = RPPacket.ImportPacket(got)
        return packet
    
    @staticmethod
    def SerJSON(data):
        return json.dumps(data)
    @staticmethod
    def DeSerJSON(data):
        return json.loads(data)    

conn = TcpConnection()
