using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RomansSimpleNetworking;
using System.Threading;

namespace RomansSimpleNetworkingExample
{
    class Program
    {
        static RSN_Client client = null;

        //Client example

        static void Main(string[] args)
        {
            //Create a list to store callbacks.
            List<RSN_Client_CallbackConfig> callbacks = new List<RSN_Client_CallbackConfig>();

            //Add the callback Test with ID 1 to the list.
            callbacks.Add(new RSN_Client_CallbackConfig(1, typeof(RSN_Example_SimpleClass)));

            //Create the client and log in with password "hello".
            //If there is an error, ErrorHandle will be called
            client = RSN_Client.Connect(callbacks.ToArray(),"hello",new RSN_Error(ErrorHandle));
            
            //Await enter press
            Console.ReadLine();

            //Create some data to send
            RSN_Example_SimpleClass example = new RSN_Example_SimpleClass();
            example.name = "Bob";
            
            //Send the data
            client.SendData(new RSN_ClientResponse(Test), example);

            //Await ENTER
            Console.ReadLine();
        }

        static void ErrorHandle(RSN_Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.description);
        }

        static void Test(object data)
        {
            //The general prodedure here would be to cast this to the correct class.
            //Deserialize
            RSN_Example_SimpleClass example = (RSN_Example_SimpleClass)data;

            //Print
            Console.WriteLine(example.name);

            //Create a new class to send
            example = new RSN_Example_SimpleClass();
            //Send some example data
            example.name = "Bob";

            //Respond with this class.
            client.SendData(new RSN_ClientResponse(Test), example);
        }
    }
}
