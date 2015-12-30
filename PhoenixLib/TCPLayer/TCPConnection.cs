using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using PhoenixLib.Log;
using System.Net;
using System.Windows.Forms;

namespace PhoenixLib.TCPLayer
{
    public class TCPConnection
    {
        private TcpClient client { get; set; }
        private NetworkStream stream { get; set; }
        private String host { get; set; }
        public bool isConnected
        {
            get
            {
                if (client != null)
                {
                    Socket s = client.Client;
                    return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
                }
                return false;
            }
        }
        public TCPConnection(String host, int port)
        {
            try
            {
                if (!Util.DNS.isIPAddress(host))
                {
                    IPAddress[] addresses = Util.DNS.resolveIP(host);
                }
                this.host = host;
                client = new TcpClient(host, port);
                //client.Connect(host, port);
                //Logger.addEntryToLog(this, "TCPConnection successfully opened to " + host + ":" + port, DataImportance.VERBOSE);
            }
            catch
            {
                //Logger.addEntryToLog(this, "Failed connecting to client (" + host + ":" + port + ") [" + e.Message + "]", DataImportance.DEBUG);
            }
        }
        public TCPConnection(TcpClient client)
        {
            this.client = client;
            this.host = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            //Logger.addEntryToLog(this, "Knowing that the client (" + client.Client.RemoteEndPoint.ToString() + ") already existed and has been bound to a TCPConnection object... it fills you with determination.");
        }
        public long pingConnection()
        {
            Ping ping = new Ping();
            PingReply reply = new Ping().Send(host);
            return reply.RoundtripTime;
        }
        public void openStream()
        {
            stream = client.GetStream();
        }
        public void readBytes(ref byte[] buffer, int position, int length)
        {
            try
            {
                for (int i = 0; i < length; i++)
                {
                    buffer[i] = readByte();
                }
                //stream.Read(buffer, position, length);
                //Logger.addEntryToLog(this, "Read " + length + " bytes from connected host (" + host + ")");
            }
            catch
            {
                //Logger.addEntryToLog(this, "Failed reading bytes from connected host (" + host + ") [" + e.Message + "]", DataImportance.CRITICAL);
            }
        }
        public void sendBytes(byte[] data, int position, int length)
        {
            try
            {
                stream.Write(data, position, length);
                //Logger.addEntryToLog(this, "Sent " + length + " bytes to connected host (" + host + ")");
            }
            catch
            {
                //Logger.addEntryToLog(this, "Failed sending bytes to connected host (" + host + ") [" + e.Message + "]", DataImportance.CRITICAL);

            }
        }
        public byte readByte() //potentially unsafe, no catching
        {
            try
            {
                return (byte)stream.ReadByte();
            }
            catch
            {
                return 0;
            }
        }
    }
}
