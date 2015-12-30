using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhoenixLib.Log
{
    public enum DataImportance
    {
        CRITICAL,
        IMPORTANT,
        VERBOSE,
        DEBUG
    }
    public class LogMessage
    {
        public String sender { get; set; }
        public DateTime timeOf { get; set; }
        public DataImportance importance { get; set; }
        public String data { get; set; }
        public override string ToString()
        {
            return String.Format("[{0,20}] [{1,10}] [{2,9}] : {3}\n", sender, timeOf.ToLongTimeString(), importance.ToString(), data);
        }
    }
    public static class Logger
    {
        private static List<LogMessage> log = new List<LogMessage>();

        public static void addEntryToLog(object sender, String data, DataImportance importance = DataImportance.DEBUG)
        {
            return;
            /*
            log.Add(new LogMessage
            {
                sender = sender.GetType().Name,
                timeOf = DateTime.Now,
                importance = importance,
                data = data
            });
            */
        }

        public static void writeLogToFile(String path = "log.txt")
        {
            File.WriteAllLines(path, log.Select(x => (x.ToString())));
        }

        public static String buildStringFromLog()
        {
            return String.Join("", log.Select(x => (x.ToString())));
        }
    }
}
