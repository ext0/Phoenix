using PhoenixLib.Datastore;
using PhoenixLib.Datastore.CommunicationClasses;
using PhoenixLib.TCPLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;

namespace PhoenixLib.Proxy
{
    public static class ProxyControl
    {
        public static bool isOpen { get; set; }
        public static Queue<HttpRequest> queuedRequests = new Queue<HttpRequest>();
        public static bool queueRequests = false;
        public static bool forwardRequests = false;
        public static Client forwardTo = null;
        public static EventHandler<SessionEventArgs> onRequest = process;
        public static EventHandler<SessionEventArgs> onResponse;
        public static void openProxy(bool ssl, bool system)
        {
            if (isOpen)
                return;
            ProxyServer.EnableSsl = ssl;
            ProxyServer.SetAsSystemProxy = system;
            ProxyServer.BeforeRequest += onRequest;
            ProxyServer.BeforeResponse += onResponse;
            ProxyServer.Start();
            isOpen = true;
        }
        public static void stopProxy()
        {
            if (!isOpen)
                return;
            ProxyServer.BeforeRequest -= onRequest;
            ProxyServer.BeforeResponse -= onResponse;
            ProxyServer.Stop();
        }

        public static void process(Object sender, SessionEventArgs e)
        {
            bool finished = true;
            if (queueRequests)
            {
                queuedRequests.Enqueue(new HttpRequest(e));
            }
            else if ((forwardTo != null) && (forwardRequests))
            {
                finished = false;
                forwardTo.executeCommand(new Command
                {
                    action = ActionType.RequestData,
                    callback = true,
                    data = Util.Serialization.serialize(new HttpRequest(e)),
                    dataName = "httpRequest"
                }, (str, eventArgs) =>
                {
                    e.Ok(eventArgs.newValue + "<!--Phoenix-->");
                    finished = true;
                });
            }
            while (!finished)
            {
                Thread.Sleep(50);
            }
        }
    }
}
