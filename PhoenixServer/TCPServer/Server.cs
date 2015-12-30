using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhoenixServer.TCPServer
{
    public class Server
    {
        private TcpListener listener;
        public bool isOpen
        {
            get
            {
                if (listener != null)
                {
                    return listener.Server.IsBound;
                }
                return false;
            }
        }
        public void bind(int port)
        {
            listener = new TcpListener(IPAddress.Parse("0.0.0.0"), port);
        }
        public void start()
        {
            listener.Start();
        }
        public void stop()
        {
            listener.Stop();
        }
        public void listenASync(Action<TcpClient> callback)
        {
            new Thread(() =>
            {
                while (true)
                {
                    try 
                    {
                        if (listener != null)
                        {
                            TcpClient client = listener.AcceptTcpClient();
                            new Thread((thread) => callback(client)).Start(); //thread abuse! someone call TPS!
                        }
                        else
                            throw new Exception("Listener was not open upon attempting to listen!");
                    }
                    catch
                    {
                        return;
                    }
                }
            }).Start();
        }
        public void kill()
        {
            stop();
            listener = null;
        }
    }
}
