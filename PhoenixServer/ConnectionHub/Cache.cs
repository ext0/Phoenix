using PhoenixLib.Datastore;
using PhoenixLib.Datastore.CommunicationClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixServer.ConnectionHub
{
    public static class ServerCache
    {
        private static List<Client> clients = new List<Client>();
        public static void addClient(Client client)
        {
            clients.Add(client);
        }
        public static void removeClient(Client client)
        {
            clients.Remove(client);
        }
        public static Client getClientFromIdentification(byte[] identification)
        {
            Client[] temp = clients.Where((client => (client.identification == identification))).ToArray();
            if (temp.Length == 1)
            {
                return temp[0];
            }
            else if (temp.Length > 1)
            {
                throw new Exception("Multiple clients with the same identifier!");
            }
            else
            {
                return null;
            }
        }
        public static List<Object> getAllValues(String key)
        {
            return clients.Where((client) => (client.data.hasKey(key))).Select((client) => (client.data.getValue(key))).ToList();
        }
        public static List<Command> getCommandsToExecute()
        {
            return clients.Where((client) => (client.commands.Count != 0)).Select((client) => (client.commands.Dequeue())).ToList(); //takes one command from each client
        }
    }
}
