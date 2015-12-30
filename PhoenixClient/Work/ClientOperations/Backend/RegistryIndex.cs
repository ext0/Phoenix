using Microsoft.Win32;
using PhoenixLib.Datastore.CommunicationClasses;
using PhoenixLib.TCPLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixClient.Work.ClientOperations.Backend
{
    public static class RegistryIndex
    {
        public static Dictionary<RegistryHive, RegistryKeyData> data;
        public static int counter;
        public static void index(RegistryHive hive, RegistryView view)
        {
            if (RegistryIndex.data == null)
                RegistryIndex.data = new Dictionary<RegistryHive, RegistryKeyData>();
            RegistryKey root = RegistryKey.OpenBaseKey(hive, view);
            RegistryKeyData data = buildDataFromKey(-1, hive, root, true, null);
            processChildren(hive, data);
        }
        public static void processChildren(RegistryHive hive, RegistryKeyData key)
        {
            int loopCounter = 0;
            foreach (String subKey in key.value.GetSubKeyNames())
            {
                try
                {
                    processChildren(hive, buildDataFromKey(loopCounter, hive, key.value.OpenSubKey(subKey), false, key));
                }
                catch { }
                loopCounter++;
            }
        }

        public static RegistryKeyData buildDataFromKey(int loopCounter, RegistryHive hive, RegistryKey key, bool root, RegistryKeyData parent)
        {
            String[] values = key.GetValueNames();
            RegistryValue[] valueData = new RegistryValue[values.Length];
            for (int i = 0; i < valueData.Length; i++)
            {
                valueData[i] = new RegistryValue
                {
                    name = values[i],
                    value = key.GetValue(values[i])
                };
            }
            RegistryKeyData value = new RegistryKeyData
            {
                data = new RegistryKeyData[key.GetSubKeyNames().Length],
                key = key.Name,
                values = valueData,
                hive = hive,
                value = key
            };
            if (root)
            {
                if (!data.ContainsKey(hive))
                {
                    data.Add(hive, value);
                }
            }
            else
            {
                parent.data[loopCounter] = value;
            }
            return value;
        }

        public static byte[] processModificationRequest(Command command)
        {
            RegistryRequest request = (RegistryRequest)Util.Serialization.deserialize(command.data);
            bool success = false;
            String errMessage = null;
            if (RegistryIndex.data.ContainsKey((RegistryHive)request.hive))
            {
                RegistryKeyData entry = Util.Metadata.Find(RegistryIndex.data[(RegistryHive)request.hive], request.data);
                if (entry != null)
                {
                    try
                    {
                        entry.value.SetValue(request.newValue.name, request.newValue.value);
                        success = true;
                    }
                    catch (Exception e)
                    {
                        errMessage = e.Message;
                    }
                }
                else
                {
                    errMessage = "Error with internal storage of registry client-side";
                }
            }
            return Util.Serialization.serialize(new RegistryResponse
            {
                error = (success) ? "Success!" : (errMessage == null) ? "Unknown error!" : errMessage,
                keyData = null
            });
        }
    }

}
