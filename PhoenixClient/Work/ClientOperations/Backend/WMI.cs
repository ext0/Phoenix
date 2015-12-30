using PhoenixLib.Datastore.CommunicationClasses;
using PhoenixLib.TCPLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixClient.Work.ClientOperations.Backend
{
    public static class WMI
    {
        internal static ManagementObjectCollection getWMIInfo(String classInfo)
        {
            ManagementObjectSearcher mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM " + classInfo);
            return mos.Get();
        }

        internal static T[] buildWMIObjects<T>(String classInfo) where T : class
        {
            ManagementObjectCollection collection = getWMIInfo(classInfo);
            T[] returnData = new T[collection.Count];
            PropertyInfo[] info = typeof(T).GetProperties();
            int j = 0;
            foreach (ManagementObject obj in collection)
            {
                T build = Activator.CreateInstance<T>();
                foreach (PropertyInfo field in info)
                {
                    WMIAttribute attr = (WMIAttribute)field.GetCustomAttributes(false)[0];
                    Object data = obj[attr.dataName];
                    if (field.PropertyType.Equals(typeof(DateTime)) && (obj[attr.dataName] != null))
                    {
                        data = ManagementDateTimeConverter.ToDateTime((String)obj[attr.dataName]);
                    }
                    else if (obj[attr.dataName] != null)
                    {
                        data = Convert.ChangeType(obj[attr.dataName], field.PropertyType);
                    }
                    field.SetValue(build, data, null);
                }
                returnData[j] = build;
                j++;
            }
            return returnData;
        }
    }
}
