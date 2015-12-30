using PhoenixLib.Datastore.CommunicationClasses;
using PhoenixLib.TCPLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixClient.Communication
{
    public static class CommandQueue
    {
        private static Queue<Command> commands = new Queue<Command>();
        
        public static void addCommand(Command command)
        {
            commands.Enqueue(command);
        }
        public static void addCommandWithData(Command command, byte[] data)
        {
            command.data = data;
            commands.Enqueue(command);
        }
        public static void addCommand(ActionType action, byte[] data, String dataName = null)
        {
            Command command = new Command
            {
                action = action,
                data = data,
                dataName = dataName
            };
            commands.Enqueue(command);
        }
        public static Command getCommand()
        {
            if (commands.Count != 0)
            {
                return commands.Dequeue();
            }
            return Util.Integrity.nullCommand;
        }
    }
}
