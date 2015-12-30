using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixLib.Scripting
{
    public static class CMD
    {
        public static String executeCommand(String input)
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + input,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            Process process = new Process();
            process.StartInfo = info;
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }

        public static String executeFile(String filename, String input)
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = input,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            Process process = new Process();
            process.StartInfo = info;
            process.Start();
            return process.StandardError.ReadToEnd();
        }
    }
}
