using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using PhoenixLib.Datastore.CommunicationClasses;
using PhoenixLib.Scripting;
using PhoenixLib.TCPLayer;
using PhoenixServer.TCPServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using System.Net.Sockets;

namespace PhoenixLicenseBuildServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server licenseServer = new Server();
            licenseServer.bind(7812);
            licenseServer.start();
            licenseServer.listenASync((x) =>
            {
                try
                {
                    TCPConnection connection = new TCPConnection(x);
                    connection.openStream();
                    String remote = x.Client.RemoteEndPoint.ToString();
                    byte[] buffer = new byte[32];
                    connection.readBytes(ref buffer, 0, buffer.Length);
                    String key = Util.Conversion.bytesToString(buffer);
                    LicenseResponse response = Util.Serialization.deserializeJson<LicenseResponse>(
                        new WebClient().DownloadString("http://pyro.solutions/?edd_action=check_license&item_name=Phoenix&license=" + key)
                    );
                    if (response.active)
                    {
                        Console.WriteLine("[LICENSE] " + response.customer_name + " requested license check. [Key: " + key + "]");
                    }
                    if (!verifiedIP(x, key, response))
                    {
                        response.license = "ipMismatch";
                    }
                    byte[] data = Util.Serialization.serialize(response);
                    byte[] metadata = BitConverter.GetBytes(data.Length);
                    connection.sendBytes(metadata, 0, metadata.Length);
                    connection.sendBytes(data, 0, data.Length);
                    x.Close();
                }
                catch
                {

                }
            });
            Server buildServer = new Server();
            buildServer.bind(7813);
            buildServer.start();
            buildServer.listenASync((x) =>
            {
                try
                {
                    TCPConnection connection = new TCPConnection(x);
                    connection.openStream();
                    byte[] bitLen = new byte[4];
                    connection.readBytes(ref bitLen, 0, bitLen.Length);
                    int length = BitConverter.ToInt32(bitLen, 0);
                    byte[] data = new byte[length];
                    connection.readBytes(ref data, 0, data.Length);
                    BuildRequest request = (BuildRequest)Util.Serialization.deserialize(data);
                    String baseRequest = "\"C:\\Documents and Settings\\Administrator\\Desktop\\Reactor\\dotNET_Reactor.Console.exe\"";
                    String baseRequestParams =
                        " -internalization " + bin(request.internalization) +
                        " -antitamp " + bin(request.antiTampering) +
                        " -control_flow_obfuscation " + bin(true) +
                        " -flow_level " + request.flowLevel +
                        " -nativexe " + bin(request.nativeExe) +
                        " -prejit " + bin(request.preJit) +
                        " -resourceencryption " + bin(true) +
                        " -antistrong " + bin(true) +
                        " -necrobit " + bin(true) +
                        " -unprintable_characters " + bin(true) +
                        " -obfuscate_public_types " + bin(true) +
                        " -q";
                    String iconTemp = Path.Combine(Environment.CurrentDirectory, "tempIcon" + new Random().Next(0, 9999));
                    if (request.customIcon)
                    {
                        File.WriteAllBytes(iconTemp, request.icon);
                        baseRequest += " -icon \"" + iconTemp + "\"";
                    }
                    LicenseResponse response = Util.Serialization.deserializeJson<LicenseResponse>(
                        new WebClient().DownloadString("http://pyro.solutions/?edd_action=check_license&item_name=Phoenix&license=" + request.license)
                    );
                    if ((response.active)&&(verifiedIP(x, request.license, response)))
                    {
                        data = Util.Serialization.serialize(new BuildResponse
                        {
                            done = false,
                            message = "Creating remote build environment...",
                            buildData = null,
                            buildString = "",
                            containsBuildData = false
                        });
                        bitLen = BitConverter.GetBytes(data.Length);
                        connection.sendBytes(bitLen, 0, bitLen.Length);
                        connection.sendBytes(data, 0, data.Length);
                        String workingDir = "C:\\Documents and Settings\\Administrator\\Desktop\\PhoenixServer\\" + request.license + "-" + new Random().Next(0, 999999);
                        copy(
                            "C:\\Documents and Settings\\Administrator\\Desktop\\PhoenixServer\\BaseBuild\\Phoenix-master",
                            workingDir);
                        String modifyDir = Path.Combine(workingDir, "PhoenixClient", "Program.cs");
                        String src = File.ReadAllText(modifyDir);
                        src = src.Replace("NONE", request.identifier);
                        src = src.Replace("localhost", request.hostname);
                        src = src.Replace("8888", request.port + "");
                        File.WriteAllText(modifyDir, src);
                        Console.WriteLine("[BUILD] Compiling...");

                        /*
                        Project project = new Project(Path.Combine(workingDir, "PhoenixClient", "PhoenixClient.csproj"), null, "4.0");
                        bool ok = project.Build();
                        ProjectCollection.GlobalProjectCollection.UnloadProject(project);

                        Project library = new Project(Path.Combine(workingDir, "PhoenixLib", "PhoenixLib.csproj"), null, "4.0");
                        bool libraryOk = library.Build();
                        ProjectCollection.GlobalProjectCollection.UnloadProject(library);
                        */

                        Dictionary<String, string> props = new Dictionary<string, string>();
                        props["Configuration"] = "Release";
                        BuildRequestData buildRequest = new BuildRequestData(Path.Combine(workingDir, "Phoenix.sln"), props, null, new string[] { "Build" }, null);
                        BuildParameters parms = new BuildParameters();
                        ConsoleLogger logger = new ConsoleLogger(LoggerVerbosity.Minimal);
                        parms.EnableNodeReuse = false;
                        parms.Loggers = new List<ILogger> { logger };

                        BuildResult result = BuildManager.DefaultBuildManager.Build(parms, buildRequest);
                        if (result.Exception != null)
                            Console.WriteLine("[BUILD] Result exception : " + result.Exception.Message);
                        bool ok = result.OverallResult == BuildResultCode.Success;
                        BuildManager.DefaultBuildManager.ResetCaches();

                        data = Util.Serialization.serialize(new BuildResponse
                        {
                            done = false,
                            message = "Protecting compiled assembly...",
                            buildData = null,
                            buildString = "",
                            containsBuildData = false
                        });
                        bitLen = BitConverter.GetBytes(data.Length);
                        connection.sendBytes(bitLen, 0, bitLen.Length);
                        connection.sendBytes(data, 0, data.Length);
                        String workingAssembly = Path.Combine(workingDir, "PhoenixClient", "bin", "Release", "PhoenixClient.exe");
                        Console.WriteLine("[BUILD] Solution built: " + ok + "(" + result.OverallResult + ")");
                        File.Copy(
                             Path.Combine(workingDir, "PhoenixLib", "bin", "Release", "PhoenixLib.dll"),
                             Path.Combine(workingDir, "PhoenixClient", "bin", "Release", "PhoenixLib.dll")
                        );
                        Console.WriteLine("[BUILD] Starting working assembly protection...");
                        CMD.executeFile(baseRequest, baseRequestParams + " -file \"" + workingAssembly + "\"");
                        String protectedAssembly = Path.Combine(workingDir, "PhoenixClient", "bin", "Release", "PhoenixClient_Secure", "PhoenixClient.exe");
                        while (!File.Exists(protectedAssembly))
                        {
                            Thread.Sleep(100);
                        }
                        Console.WriteLine("[BUILD] Finished compilation, transmitting build...");
                        byte[] assemblyData = File.ReadAllBytes(protectedAssembly);
                        data = Util.Serialization.serialize(new BuildResponse
                        {
                            done = true,
                            message = "Transmitting protected assembly (" + request.outputExeName + ")...",
                            buildData = assemblyData,
                            buildString = request.outputExeName,
                            containsBuildData = true
                        });
                        bitLen = BitConverter.GetBytes(data.Length);
                        connection.sendBytes(bitLen, 0, bitLen.Length);
                        connection.sendBytes(data, 0, data.Length);
                        Console.WriteLine("[BUILD] Sending " + data.Length + " bytes");
                        DeleteDirectory(workingDir);
                    }
                    else
                    {
                        data = Util.Serialization.serialize(new BuildResponse
                        {
                            done = true,
                            message = "License not activated! Build cancelled.",
                            buildData = null,
                            buildString = "",
                            containsBuildData = false
                        });
                        bitLen = BitConverter.GetBytes(data.Length);
                        connection.sendBytes(bitLen, 0, bitLen.Length);
                        connection.sendBytes(data, 0, data.Length);
                    }
                    Thread.Sleep(20000);
                    x.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("[BUILD] Error: " + e.Message);
                }
            });
            Console.WriteLine("LICENSE AND BUILD SERVERS LIVE");
            Console.ReadLine();
        }
        private static String bin(bool value)
        {
            return (value) ? "1" : "0";
        }
        public static void copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            copyAll(diSource, diTarget);
        }

        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                catch
                {

                }
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }
            try
            {
                Directory.Delete(target_dir, false);
            }
            catch
            {

            }
        }

        public static void copyAll(DirectoryInfo source, DirectoryInfo target)
        {
            try
            {
                if (Directory.Exists(target.FullName))
                {
                    DeleteDirectory(target.FullName);
                }
                Directory.CreateDirectory(target.FullName);

                // Copy each file into the new directory.
                foreach (FileInfo fi in source.GetFiles())
                {
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                }

                // Copy each subdirectory using recursion.
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                    copyAll(diSourceSubDir, nextTargetSubDir);
                }
            }
            catch
            {

            }
        }
        public static bool verifiedIP(TcpClient client, String license, LicenseResponse response)
        {
            try
            {
                if (!response.active)
                    return false;
                String askingIp = client.Client.RemoteEndPoint.ToString();
                askingIp = askingIp.Substring(0, askingIp.IndexOf(":"));
                String licenseInfo = "licenseInfo";
                String licenseLock = "licenseLock";
                if (!File.Exists(licenseInfo))
                {
                    Dictionary<String, String> licensesInit = new Dictionary<String, String>();
                    byte[] serializedInit = Util.Serialization.serialize(licensesInit);
                    File.WriteAllBytes(licenseInfo, serializedInit);
                }
                while (File.Exists(licenseLock))
                {
                    Thread.Sleep(100);
                }
                FileStream stream = File.Create(licenseLock);
                stream.Close();
                byte[] data = File.ReadAllBytes(licenseInfo);
                Dictionary<String, String> licenses = (Dictionary<String, String>)Util.Serialization.deserialize(data);
                bool ret = true;
                if (licenses.ContainsKey(license))
                {
                    String storedIP = licenses[license];
                    if (storedIP.IndexOf(":") != -1)
                    {
                        storedIP = storedIP.Substring(0, storedIP.IndexOf(":"));
                    }
                    Console.WriteLine("[VERIFY] Stored: " + storedIP + "\n[VERIFY] Requesting: " + askingIp);
                    if (!storedIP.Equals(askingIp))
                    {
                        ret = false;
                    }
                }
                else
                {
                    licenses.Add(license, askingIp);
                }
                byte[] serialized = Util.Serialization.serialize(licenses);
                File.WriteAllBytes(licenseInfo, serialized);
                File.Delete(licenseLock);
                return ret;
            }
            catch(Exception e)
            {
                Console.WriteLine("[FATAL ERROR] " + e.Message);
                return false;
            }
        }
    }
    public class BasicLogger : Logger
    {
        public override void Initialize(IEventSource eventSource)
        {
            throw new NotImplementedException();
        }
    }
}
