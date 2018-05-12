using RomansSimpleNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomansSimpleNetworkingExampleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create a list of functions that will be called when the client requests a specific one.
            List<RSN_Server_CallbackConfig> callbacks = new List<RSN_Server_CallbackConfig>();

            //Add the callback with ID 1
            callbacks.Add(new RSN_Server_CallbackConfig(1,new RSN_ServerResponse(Test),typeof(RSN_Example_SimpleClass)));

            //Create the server with password "hello"
            RSN_Server server = RSN_Server.CreateServer(callbacks.ToArray(),"hello", 13000);

            //Await ENTER
            Console.ReadLine();
        }

        static void Test(object obj, RSN_ServerResponse_Data data)
        {
            //Deserialize
            RSN_Example_SimpleClass example = (RSN_Example_SimpleClass)obj;

            //Print the name of the person
            Console.WriteLine(example.name);

            //Create a new class and add placeholder data
            example = new RSN_Example_SimpleClass();
            example.name = "Larry";

            //Respond
            data.Respond(example);
        }
    }
}
