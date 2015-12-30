using Newtonsoft.Json;
using PhoenixLib.Datastore.CommunicationClasses;
using PhoenixLib.TCPLayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixLib.Datastore
{
    [Serializable()]
    public class Client //holy shit this actually serializes!
    {
        public InternalDatastore<String, Object> data { get; set; }
        public byte[] identification { get; set; }
        public IPRequest ipData { get; set; }
        public String customIdentifier { get; set; }
        public Queue<Command> commands { get; set; }
        public Client(byte[] identification, IPRequest ipData, String customIdentifier = "None")
        {
            data = new InternalDatastore<String, Object>();
            data.addEntry("ipRequest", ipData, this);
            data.addEntry("environmentUsername", Environment.UserName, this);
            commands = new Queue<Command>();
            this.identification = identification;
            this.ipData = ipData;
            this.customIdentifier = customIdentifier;
        }
        public void executeCommand(Command command, Action<object, DatastoreUpdateEventArgs<String, object>> callback)
        {
            if (command.callback)
            {
                data.addListener(command.dataName, callback);
            }
            commands.Enqueue(command);
        }
        public void executeCommand(ActionType action, String dataName, byte[] data, Action<object, DatastoreUpdateEventArgs<String, object>> callback = null)
        {
            Command command = new Command();
            if (callback != null)
            {
                command.callback = true;
                this.data.addListener(dataName, callback);
            }
            command.action = action;
            command.dataName = dataName;
            command.data = data;
            commands.Enqueue(command);
        }
        public override bool Equals(object obj)
        {
            if (obj is Client)
            {
                return (obj as Client).identification.SequenceEqual(identification);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return identification.GetHashCode();
        }
        public static Client buildLocalClient(String customIdentifier)
        {
            byte[] buffer = new byte[256]; //important number!
            if (File.Exists("id.dat"))
            {
                try
                {
                    buffer = File.ReadAllBytes("id.dat");
                }
                catch
                {
                    new Random().NextBytes(buffer);
                }
            }
            else
            {
                new Random().NextBytes(buffer);
                try
                {
                    File.WriteAllBytes("id.dat", buffer);
                }
                catch
                {

                }
            }
            IPRequest request;
            try
            {
                request = JsonConvert.DeserializeObject<IPRequest>(new WebClient().DownloadString("https://wtfismyip.com/json"));
            }
            catch
            {
                request = new IPRequest
                {
                    Hostname = "Could not resolve hostname",
                    IPAddress = "255.255.255.255",
                    ISP = "Could not resolve ISP",
                    Location = "Could not resolve location"
                };
            }
            return new Client(buffer, request, customIdentifier);
        }
    }
}
