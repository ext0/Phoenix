using PhoenixLib.Datastore;
using PhoenixLib.Datastore.CommunicationClasses;
using PhoenixLib.Log;
using PhoenixLib.TCPLayer;
using PhoenixServer.ConnectionHub;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace PhoenixServer.TCPServer
{
    public class ServerConnection
    {
        private TCPConnection connection;
        private Client client;
        public ServerConnection(TcpClient client)
        {
            connection = new TCPConnection(client);
        }

        public bool isConnected()
        {
            return connection.isConnected;
        }

        public void openStream()
        {
            try
            {
                connection.openStream();
            }
            catch
            {
                throw new Exception("Could not open underlying TCPConnection stream!");
            }
        }

        public void run(bool async, Action<Client> action)
        {
            if (async)
            {
                new Thread(() => listen(action)).Start();
            }
            else
            {
                listen(action);
            }
        }

        public long ping()
        {
            return connection.pingConnection();
        }

        private void listen(Action<Client> action)
        {
            Object obj;
            byte[] data;
            data = Util.Integrity.receiveAndConfirmMessageFromStream(connection);
            obj = Util.Metadata.deserializeReadableObject(data, false);
            if (obj is Client)
            {
                client = (Client)obj;
                ServerCache.addClient(client);
                new Thread(() => action(client)).Start();
                Util.Integrity.sendObject(connection, Util.Integrity.nullCommand);
            }
            else
            {
                throw new Exception("Expected initial handshake, got " + obj.GetType());
            }
            while (connection.isConnected)
            {
                data = Util.Integrity.receiveAndConfirmMessageFromStream(connection);
                obj = Util.Metadata.deserializeReadableObject(data, false);
                if (obj is Command)
                {
                    processCommand(obj);
                    if (client.commands.Count > 0)
                    {
                        Util.Integrity.sendObject(connection, client.commands.Dequeue());
                    }
                    else
                    {
                        Util.Integrity.sendObject(connection, Util.Integrity.nullCommand);
                    }
                }
                else
                {
                    Logger.addEntryToLog(this, "Invalid type!");
                }
            }
        }

        public Object getObjectFromMessage(byte[] extracted)
        {
            return Util.Metadata.deserializeReadableObject(extracted, true);
        }

        public void processCommand(Object obj)
        {
            Command command = (Command)obj;
            if (command.action == ActionType.NOP)
                return;
            if (command.data != null)
            {
                try
                {
                    Object exception = Util.Serialization.deserialize(command.data);
                    if (exception is Exception)
                    {
                        Exception e = exception as Exception;
                        File.AppendAllText("err.log", "FATAL EXCEPTION [" + e.Message + "] STACKTRACE: " + e.StackTrace + Environment.NewLine);
                    }
                }
                catch
                {

                }
            }
            if (command.dataName.Length != 0)
            {
                client.data.modifyEntry(command.dataName, command.data, client);
            }
            Logger.addEntryToLog(this, command.dataName + " returned from client [Callback: " + command.callback + "] [Action: " + command.action + "]");
        }
    }
}
