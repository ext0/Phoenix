using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkypeScraper.Skype
{
    public static class SkypeOperations
    {
        public static List<T> processSQLDataProp<T>(String table, String workingFile, byte[] fileData, bool garbageCollect)
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
                            foreach (PropertyInfo field in typeof(T).GetProperties())
                            {
                                Object[] attributes = field.GetCustomAttributes(false);
                                if (attributes.Length != 0)
                                {
                                    SQLiteAttribute attr = (SQLiteAttribute)attributes[0];
                                    Object obj = reader[attr.dataName];
                                    if (!(obj is DBNull))
                                    {
                                        field.SetValue(build, Convert.ChangeType(reader[attr.dataName], field.PropertyType), null);
                                    }
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
    [AttributeUsage(AttributeTargets.Property)]
    public class SQLiteAttribute : Attribute
    {
        public readonly string dataName;

        public SQLiteAttribute(String dataName)
        {
            this.dataName = dataName;
        }
    }
}