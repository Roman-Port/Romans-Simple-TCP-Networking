using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace RomansSimpleNetworking
{
    public class RSN_Tools
    {
        public static object DeserializeObject(string value, Type objType)
        {
            try
            {
                //Get a data stream
                MemoryStream mainStream = GenerateStreamFromString(value);

                DataContractJsonSerializer ser = new DataContractJsonSerializer(objType);
                //Load it in.
                mainStream.Position = 0;
                var obj = ser.ReadObject(mainStream);
                return Convert.ChangeType(obj, objType);
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

        public static string SerializeObject(object obj)
        {
            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(obj.GetType());
            ser.WriteObject(stream1, obj);
            stream1.Position = 0;
            StreamReader sr = new StreamReader(stream1);
            return sr.ReadToEnd();
        }

        public static string GenerateRandomString(int length, Random rand = null)
        {
            if (rand == null)
                rand = new Random();
            char[] c = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHINKLMNOPQRSTUVWXYZ".ToCharArray();
            string o = "";
            while (o.Length < length)
            {
                o += c[rand.Next(0, c.Length)];
            }
            return o;
        }
    }
}
