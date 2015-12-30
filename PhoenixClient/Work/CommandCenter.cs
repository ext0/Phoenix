using PhoenixClient.Communication;
using PhoenixClient.Work.ClientOperations;
using PhoenixLib.Datastore.CommunicationClasses;
using PhoenixLib.TCPLayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhoenixClient.Work
{
    public static class CommandCenter
    {
        public static List<Type> operationStorage = new List<Type>
        {
            typeof(SystemInfo)
        };

        public static void processCommand(Command command)
        {
            if (command.action == ActionType.NOP)
                return;
            foreach (Type type in operationStorage)
            {
                foreach (MethodInfo method in type.GetMethods())
                {
                    CommandAttribute attribute = (CommandAttribute)method.GetCustomAttributes(false)[0];
                    if ((attribute != null) && (attribute.actionType.Equals(command.action)) && (attribute.dataName.Equals(command.dataName)))
                    {
                        new Thread(() =>
                        {
                            byte[] returnData = null;
                            try
                            {
                                returnData = (byte[])method.Invoke(null, new object[] { command });
                            }
                            catch(Exception e)
                            {
                                returnData = Util.Serialization.serialize(e);
                            }
                            if (returnData != null)
                            {
                                CommandQueue.addCommandWithData(command, returnData);
                            }
                        }).Start();
                        return;
                    }
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public readonly string dataName;
        public readonly ActionType actionType;

        public CommandAttribute(String dataName, ActionType actionType)
        {
            this.dataName = dataName;
            this.actionType = actionType;
        }
    }
}
