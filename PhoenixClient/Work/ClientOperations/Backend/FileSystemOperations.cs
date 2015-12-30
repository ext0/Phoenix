using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhoenixClient.Work.ClientOperations.Backend
{
    public static class FileSystemOperations
    {
        public static String getUserAccountDirectory()
        {
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            if (Environment.OSVersion.Version.Major >= 6)
            {
                path = Directory.GetParent(path).ToString();
            }
            return path;
        }
        public static List<Object> getFileSystemEntries(String path)
        {
            List<Object> returnData = new List<Object>();
            try
            {
                foreach (String entry in Directory.EnumerateFileSystemEntries(path))
                {
                    if (File.Exists(entry))
                    {
                        returnData.Add(new FileInfo(entry));
                    }
                    if (Directory.Exists(entry))
                    {
                        returnData.Add(new DirectoryInfo(entry));
                    }
                }
            }
            catch
            {
                return returnData;
            }
            return returnData;
        }
        public static List<Object> findMatches(ref List<Object> returnData, String path, Regex regex, bool includeDirectories, bool recursive, int maxDepth, int currentDepth)
        {
            try
            {
                foreach (String entry in Directory.EnumerateFileSystemEntries(path))
                {
                    if (regex.IsMatch(entry))
                    {
                        if (File.Exists(entry))
                        {
                            returnData.Add(new FileInfo(entry));
                        }
                        else if ((includeDirectories) && (Directory.Exists(entry)))
                        {
                            returnData.Add(new DirectoryInfo(entry));
                        }
                    }
                    if ((Directory.Exists(entry)) && (recursive))
                    {
                        if (maxDepth > currentDepth)
                        {
                            findMatches(ref returnData, entry, regex, includeDirectories, recursive, maxDepth, currentDepth + 1);
                        }
                    }
                }
                return returnData;
            }
            catch
            {
                return returnData;
            }
        }
        public static bool canRead(string path)
        {
            var readAllow = false;
            var readDeny = false;
            var accessControlList = Directory.GetAccessControl(path);
            if (accessControlList == null)
                return false;
            var accessRules = accessControlList.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
            if (accessRules == null)
                return false;

            foreach (FileSystemAccessRule rule in accessRules)
            {
                if ((FileSystemRights.Read & rule.FileSystemRights) != FileSystemRights.Read) continue;

                if (rule.AccessControlType == AccessControlType.Allow)
                    readAllow = true;
                else if (rule.AccessControlType == AccessControlType.Deny)
                    readDeny = true;
            }

            return readAllow && !readDeny;
        }
    }
}
