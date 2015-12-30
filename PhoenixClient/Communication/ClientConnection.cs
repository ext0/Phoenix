using PhoenixClient.Work;
using PhoenixLib.Datastore;
using PhoenixLib.Datastore.CommunicationClasses;
using PhoenixLib.TCPLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhoenixClient.Communication
{
    public static class ClientConnection
    {
        public static Client client { get; set; }
        public static TCPConnection connection { get; set; }
        public static String host { get; set; }
        public static int port { get; set; }
        public static bool attemptConnection(String host, int port)
        {
            try
            {
                ClientConnection.host = host;
                ClientConnection.port = port;
                connection = new TCPConnection(host, port);
                connection.openStream();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static void run(bool async)
        {
            if (async)
            {
                new Thread(() => broadcast()).Start();
            }
            else
            {
                broadcast();
            }
        }
        public static void identifySelf()
        {
            Util.Integrity.sendObject(connection, client);
        }
        public static void broadcast()
        {
            identifySelf();
            byte[] data;
            object obj;
            while (connection.isConnected)
            {
                data = Util.Integrity.receiveAndConfirmMessageFromStream(connection);
                obj = Util.Serialization.deserialize(data);
                data = null;
                if (obj is Command)
                {
                    CommandCenter.processCommand(obj as Command);
                }
                Util.Integrity.sendObject(connection, CommandQueue.getCommand());
                Thread.Sleep(10); //necesarry for garbage collector to catch up to my shitty coding
            }
            bool connected = attemptConnection(host, port);
            while (!connected)
            {
                connected = attemptConnection(host, port);
            }
            broadcast();
        }
    }
}
