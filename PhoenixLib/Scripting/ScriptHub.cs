using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhoenixLib.Scripting
{
    public static class ScriptHub
    {
        private static Object showMessageBox(Object obj, bool waitForInput)
        {
            if (waitForInput)
            {
                DialogResult result = MessageBox.Show(obj.ToString());
                return result == DialogResult.OK;
            }
            else
            {
                new Thread(() => { MessageBox.Show(obj.ToString()); }).Start();
            }
            return false;
        }

        private static Object execute(String command)
        {
            return CMD.executeCommand(command);
        }
        private static String shutdown()
        {
            return CMD.executeCommand("shutdown /s");
        }
        private static String restart()
        {
            return CMD.executeCommand("shutdown /r");
        }
        private static String logout()
        {
            return CMD.executeCommand("shutdown /l");
        }
        private static int[] encrypt(int[] input)
        {
            return ProtectedData.Protect(input.Cast<byte>().ToArray(), null, DataProtectionScope.LocalMachine).Cast<int>().ToArray();
        }
        private static int[] decrypt(int[] input)
        {
            return ProtectedData.Unprotect(input.Cast<byte>().ToArray(), null, DataProtectionScope.LocalMachine).Cast<int>().ToArray();
        }
        public static double wait(double sleep)
        {
            Thread.Sleep((int)(sleep * 1000));
            return sleep;
        }
        public static String speak(String input, bool waitForEnd)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            if (waitForEnd)
            {
                synth.Speak(input);
            }
            else
            {
                new Thread(() => { synth.Speak(input); }).Start();
            }
            return input;
        }
        public static String executeScript(int input, String code)
        {
            String output = String.Empty;
            if (input == 0)
            {
                dynamic lua = new DynamicLua.DynamicLua();

                lua.print = new Func<Object, Object>((x) =>
                {
                    output += x.ToString();
                    return x;
                });
                lua.showMessageBox = new Func<Object, bool, Object>(showMessageBox);
                lua.execute = new Func<String, Object>(execute);
                lua.encrypt = new Func<int[], int[]>(encrypt);
                lua.decrypt = new Func<int[], int[]>(decrypt);
                lua.speak = new Func<String, bool, String>(speak);
                lua.shutdown = new Func<String>(shutdown);
                lua.restart = new Func<String>(restart);
                lua.logout = new Func<String>(logout);
                lua.wait = new Func<double, double>(wait);
                try
                {
                    lua(code);
                }
                catch (Exception e)
                {
                    return "[ERROR] " + e.Message;
                }
            }
            else if (input == 1)
            {
                int rand = new Random().Next(0, 9999);
                String fileName = "tempBatch" + rand + ".bat";
                File.WriteAllText(fileName, code);
                output = CMD.executeCommand(Environment.CurrentDirectory + "\\" + fileName);
                File.Delete(fileName);
            }
            else if (input == 2)
            {
                int rand = new Random().Next(0, 9999);
                String fileName = "tempHTML" + rand + ".html";
                File.WriteAllText(fileName, code);
                output = CMD.executeCommand("start " + Environment.CurrentDirectory + "\\" + fileName);
            }
            return output;
        }
    }
}
