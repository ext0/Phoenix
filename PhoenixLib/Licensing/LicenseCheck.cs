using PhoenixLib.Datastore.CommunicationClasses;
using PhoenixLib.TCPLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhoenixLib.Licensing
{
    public static class LicenseCheck
    {
        public static LicenseResponse checkLicense(String host, int port, String keyToCheck)
        {
            if (keyToCheck.Length != 32)
            {
                return null;
            }
            try
            {
                TCPConnection connection = new TCPConnection(host, port);
                connection.openStream();
                byte[] data = Util.Conversion.stringToBytes(keyToCheck);
                connection.sendBytes(data, 0, data.Length);
                byte[] length = new byte[4];
                connection.readBytes(ref length, 0, length.Length);
                int bitLen = BitConverter.ToInt32(length, 0);
                byte[] serialized = new byte[bitLen];
                connection.readBytes(ref serialized, 0, bitLen);
                return (LicenseResponse)Util.Serialization.deserialize(serialized);
            }
            catch
            {
                return null;
            }
        }
    }
}
