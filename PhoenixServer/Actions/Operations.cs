using PhoenixLib.Datastore.CommunicationClasses;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixServer.Actions
{
    public static class Operations
    {
        public static List<T> processSQLData<T>(String table, String workingFile, byte[] fileData, bool garbageCollect)
        {
            File.WriteAllBytes(Directory.GetCurrentDirectory() + "\\" + workingFile, fileData);
            List<T> returnData = new List<T>();
            using (SQLiteConnection cnn = new SQLiteConnection("data source=\"" + Directory.GetCurrentDirectory() + "\\" + workingFile + "\""))
            {
                cnn.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM " + table + ";", cnn))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            T build = Activator.CreateInstance<T>();
                            foreach (FieldInfo field in typeof(T).GetFields())
                            {
                                Object[] attributes = field.GetCustomAttributes(false);
                                if (attributes.Length != 0)
                                {
                                    SQLiteAttribute attr = (SQLiteAttribute)attributes[0];
                                    field.SetValue(build, Convert.ChangeType(reader[attr.dataName], field.FieldType));
                                }
                            }
                            returnData.Add(build);
                        }
                    }
                }
                cnn.Close();
            }
            if (garbageCollect)
                GC.Collect();
            File.Delete(Directory.GetCurrentDirectory() + "\\" + workingFile);
            return returnData;
        }
    }
}
