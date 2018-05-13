# Romans-Simple-TCP-Networking
A simple TCP client/server I wrote in an afternoon for another project.

### Using The Client
The client is easy to set up. This program relies on mapping classes to serialize/deserialize to ids. To do this, first make a list.

```c#
//Create a list to store types.
List<RSN_Client_CallbackConfig> types = new List<RSN_Client_CallbackConfig>();
```

Now, add our example class to this.

```c#
//Add the type with ID 1.
types.Add(new RSN_Client_CallbackConfig(1, typeof(RSN_Example_SimpleClass)));
```

This will map the class `RSN_Example_SimpleClass` to ID 1. You don't have to worry about this for the most part, but you do need to make sure all of your classes are mapped.

Now, to connect, just put in the following line.

```c#
var client = RSN_Client.Connect(types.ToArray(),"hello","10.0.1.13", 13000, new RSN_Error(ErrorHandle));
```

Or if you prefer...

```c#
var client = RSN_Client.Connect(types.ToArray(),[ *SERVER PASSWORD* ], [ *SERVER IP* ], [ *SERVER PORT* ], new RSN_Error( [ *NAME OF A ONERROR FUNCTION* ] ));
```

This function will connect with the following arguments...

| Description        | Example           |
| ------------------ | ------------- |
| This is the server password that is required to log in.        | "hello" |
| This is the address of the machine you'd like to connect to.      | "10.0.1.13" |
| This is the port of the machine you'd like to connect to. | "13000" |
| This is a "callback" that is called if an error appears. | new RSN_Error(ErrorHandle) |

For more examples of callbacks, take a look at the examples.

We'll now send data to the server. This could be done from anywhere.

First, we'll make some placeholder data

```c#
//Create some data to send
RSN_Example_SimpleClass example = new RSN_Example_SimpleClass();
example.name = "Bob";
```

We can do this because we mapped RSN_Example_SimpleClass earlier. Now, we'll send the data.

```c#
//Send the data
client.SendData(new RSN_ClientResponse(Test), example);
```

Client.SendData takes two arguments. The first of these is a callback for the result. The other is the string input.

Upon getting data, the function Test() will be called. Make sure that callbacks always take in a single ``object``.

```c#
static void Test(object data)
{
    //The general prodedure here would be to cast this to the correct class.
    //Deserialize
    RSN_Example_SimpleClass example = (RSN_Example_SimpleClass)data;

    //Print
    Console.WriteLine(example.name);
}
```

### Using The Server
The server uses a similar structure to the client. You must map specific datatypes to functions that will be called when they are recived from the client. These share the same IDs as the client.

To do this, we make a list for these, just like before.

```c#
//Create a list of functions that will be called when the client requests a specific one.
List<RSN_Server_CallbackConfig> callbacks = new List<RSN_Server_CallbackConfig>();
```

Now, map the same class as before to another function.
```c#
//Add the callback with ID 1
callbacks.Add(new RSN_Server_CallbackConfig(1,new RSN_ServerResponse(Test),typeof(RSN_Example_SimpleClass)));
```

The first argument is the ID of the type. This should be the same as the client.

The second argument is the callback, or function, that will be used when this is called. These will be explained below.

The third argument is the type of class this will be assigned to. This must be the same as the client.

The callback for this server must be mapped to a function that takes in data type ``object`` for the raw data (you must cast this to your class) and a ``RSN_ServerResponse_Data`` data type to respond.

An example of a callback is below.

```c#
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
```

The function above will read in the data sent by the client, print the "name" variable, then create a new example to send and set the name. This will be sent over to the client as a response.

To start the server, use the line

```c#
RSN_Server server = RSN_Server.CreateServer(callbacks.ToArray(),"hello", 13000);
```

This will start a server with password "hello" on port 13000. The callbacks are passed in as a array.
