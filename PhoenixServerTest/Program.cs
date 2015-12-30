using Microsoft.Win32;
using PhoenixLib.Datastore;
using PhoenixLib.Datastore.Audio;
using PhoenixLib.Datastore.CommunicationClasses;
using PhoenixLib.Log;
using PhoenixLib.Proxy;
using PhoenixLib.TCPLayer;
using PhoenixServer.Actions;
using PhoenixServer.TCPServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhoenixServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            /*

            Server.bind(8080);
            Server.start();
            Console.WriteLine("Server opened! (" + Server.isOpen + ")");
            Server.listenASync((client) =>
            {
                Console.WriteLine("New connection opened!");
                ServerConnection connection = new ServerConnection(client);
                connection.openStream();
                connection.run(false, (newClient) =>
                {
                    Program.client = newClient;
                    Console.WriteLine("New client added!");
                    newClient.executeCommand(new Command
                    {
                        action = ActionType.SendData,
                        callback = true,
                        data = Util.Conversion.stringToBytes("Skype"),
                        dataName = "killProcess"
                    }, killProcess);
                    Console.WriteLine("ping: " + connection.ping() + "ms");
                });
            });
            */
        }
        static void killProcess(Object sender, DatastoreUpdateEventArgs<String, Object> e)
        {
            Console.WriteLine(Util.Conversion.bytesToString((byte[])e.newValue));
        }
    }
}
