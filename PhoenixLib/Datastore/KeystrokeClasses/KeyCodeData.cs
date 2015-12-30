using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixLib.Datastore.KeystrokeClasses
{
    [Serializable()]
    public enum KeyCode
    {
        None = 0,
        LButton = 1,
        RButton = 2,
        Cancel = 3,
        MButton = 4,
        XButton1 = 5,
        XButton2 = 6,
        Back = 8,
        Tab = 9,
        LineFeed = 10,
        Clear = 12,
        Return = 13,
        Enter = 13,
        ShiftKey = 16,
        ControlKey = 17,
        Menu = 18,
        Pause = 19,
        Capital = 20,
        CapsLock = 20,
        KanaMode = 21,
        HanguelMode = 21,
        HangulMode = 21,
        JunjaMode = 23,
        FinalMode = 24,
        HanjaMode = 25,
        KanjiMode = 25,
        Escape = 27,
        IMEConvert = 28,
        IMENonconvert = 29,
        IMEAccept = 30,
        IMEAceept = 30,
        IMEModeChange = 31,
        Space = 32,
        Prior = 33,
        PageUp = 33,
        Next = 34,
        PageDown = 34,
        End = 35,
        Home = 36,
        Left = 37,
        Up = 38,
        Right = 39,
        Down = 40,
        Select = 41,
        Print = 42,
        Execute = 43,
        Snapshot = 44,
        PrintScreen = 44,
        Insert = 45,
        Delete = 46,
        Help = 47,
        D0 = 48,
        D1 = 49,
        D2 = 50,
        D3 = 51,
        D4 = 52,
        D5 = 53,
        D6 = 54,
        D7 = 55,
        D8 = 56,
        D9 = 57,
        A = 65,
        B = 66,
        C = 67,
        D = 68,
        E = 69,
        F = 70,
        G = 71,
        H = 72,
        I = 73,
        J = 74,
        K = 75,
        L = 76,
        M = 77,
        N = 78,
        O = 79,
        P = 80,
        Q = 81,
        R = 82,
        S = 83,
        T = 84,
        U = 85,
        V = 86,
        W = 87,
        X = 88,
        Y = 89,
        Z = 90,
        LWin = 91,
        RWin = 92,
        Apps = 93,
        Sleep = 95,
        NumPad0 = 96,
        NumPad1 = 97,
        NumPad2 = 98,
        NumPad3 = 99,
        NumPad4 = 100,
        NumPad5 = 101,
        NumPad6 = 102,
        NumPad7 = 103,
        NumPad8 = 104,
        NumPad9 = 105,
        Multiply = 106,
        Add = 107,
        Separator = 108,
        Subtract = 109,
        Decimal = 110,
        Divide = 111,
        F1 = 112,
        F2 = 113,
        F3 = 114,
        F4 = 115,
        F5 = 116,
        F6 = 117,
        F7 = 118,
        F8 = 119,
        F9 = 120,
        F10 = 121,
        F11 = 122,
        F12 = 123,
        F13 = 124,
        F14 = 125,
        F15 = 126,
        F16 = 127,
        F17 = 128,
        F18 = 129,
        F19 = 130,
        F20 = 131,
        F21 = 132,
        F22 = 133,
        F23 = 134,
        F24 = 135,
        NumLock = 144,
        Scroll = 145,
        LShiftKey = 160,
        RShiftKey = 161,
        LControlKey = 162,
        RControlKey = 163,
        LMenu = 164,
        RMenu = 165,
        BrowserBack = 166,
        BrowserForward = 167,
        BrowserRefresh = 168,
        BrowserStop = 169,
        BrowserSearch = 170,
        BrowserFavorites = 171,
        BrowserHome = 172,
        VolumeMute = 173,
        VolumeDown = 174,
        VolumeUp = 175,
        MediaNextTrack = 176,
        MediaPreviousTrack = 177,
        MediaStop = 178,
        MediaPlayPause = 179,
        LaunchMail = 180,
        SelectMedia = 181,
        LaunchApplication1 = 182,
        LaunchApplication2 = 183,
        OemSemicolon = 186,
        Oem1 = 186,
        Oemplus = 187,
        Oemcomma = 188,
        OemMinus = 189,
        OemPeriod = 190,
        OemQuestion = 191,
        Oem2 = 191,
        Oemtilde = 192,
        Oem3 = 192,
        OemOpenBrackets = 219,
        Oem4 = 219,
        OemPipe = 220,
        Oem5 = 220,
        OemCloseBrackets = 221,
        Oem6 = 221,
        OemQuotes = 222,
        Oem7 = 222,
        Oem8 = 223,
        OemBackslash = 226,
        Oem102 = 226,
        ProcessKey = 229,
        Packet = 231,
        Attn = 246,
        Crsel = 247,
        Exsel = 248,
        EraseEof = 249,
        Play = 250,
        Zoom = 251,
        NoName = 252,
        Pa1 = 253,
        OemClear = 254,
        KeyCode = 65535,
        Shift = 65536,
        Control = 131072,
        Alt = 262144,
        Modifiers = -65536
    }
    [Serializable()]
    public class KeyPressed
    {
        public KeyCode KeyCode { get; set; }
        public bool CapsLockOn { get; set; }
        public bool ShiftPressed { get; set; }
        public string CurrentWindow { get; set; }
        public long dateTime { get; set; } //bin value

        public KeyPressed(KeyCode keyCode, bool shiftPressed, bool capsLockOn, string currentWindow)
        {
            this.KeyCode = keyCode;
            ShiftPressed = shiftPressed;
            this.CapsLockOn = capsLockOn;
            this.CurrentWindow = currentWindow;
            this.dateTime = DateTime.Now.ToBinary();
        }

        public override string ToString()
        {
            var character = ConvertKeyCodeToString();

            if (IsAlphabeticKey())
            {
                //If both (shift and caps) are active then the string remains lowercase
                if (ShiftPressed == CapsLockOn)
                    return character;
                else
                    return character.ToUpper();
            }
            else if (ShiftPressed)
            {
                return ShiftCharacterIfItIsShiftable(character);
            }

            return character;
        }

        private bool IsAlphabeticKey()
        {
            return (int)KeyCode > 64 && (int)KeyCode < 91;
        }

        private string ConvertKeyCodeToString()
        {
            if (KeyCode == KeyCode.F1) return "<F1>";
            if (KeyCode == KeyCode.F2) return "<F2>";
            if (KeyCode == KeyCode.F3) return "<F3>";
            if (KeyCode == KeyCode.F4) return "<F4>";
            if (KeyCode == KeyCode.F5) return "<F5>";
            if (KeyCode == KeyCode.F6) return "<F6>";
            if (KeyCode == KeyCode.F7) return "<F7>";
            if (KeyCode == KeyCode.F8) return "<F8>";
            if (KeyCode == KeyCode.F9) return "<F9>";
            if (KeyCode == KeyCode.F10) return "<F10>";
            if (KeyCode == KeyCode.F11) return "<F11>";
            if (KeyCode == KeyCode.F12) return "<F12>";
            if (KeyCode == KeyCode.Snapshot) return "<print screen>";
            if (KeyCode == KeyCode.Scroll) return "<scroll>";
            if (KeyCode == KeyCode.Pause) return "<pause>";
            if (KeyCode == KeyCode.Insert) return "<insert>";
            if (KeyCode == KeyCode.Home) return "<home>";
            if (KeyCode == KeyCode.Delete) return "<delete>";
            if (KeyCode == KeyCode.End) return "<end>";
            if (KeyCode == KeyCode.Prior) return "<page up>";
            if (KeyCode == KeyCode.Next) return "<page down>";
            if (KeyCode == KeyCode.Escape) return "<esc>";
            if (KeyCode == KeyCode.NumLock) return "<numlock>";
            if (KeyCode == KeyCode.Tab) return "<tab>";
            if (KeyCode == KeyCode.Back) return "<backspace>";
            if (KeyCode == KeyCode.Return) return "<enter>";
            if (KeyCode == KeyCode.Space) return " ";
            if (KeyCode == KeyCode.Left) return "<left>";
            if (KeyCode == KeyCode.Up) return "<up>";
            if (KeyCode == KeyCode.Right) return "<right>";
            if (KeyCode == KeyCode.Down) return "<down>";
            if (KeyCode == KeyCode.Multiply) return "*";
            if (KeyCode == KeyCode.Add) return "+";
            if (KeyCode == KeyCode.Separator) return "|";
            if (KeyCode == KeyCode.Subtract) return "-";
            if (KeyCode == KeyCode.Decimal) return ".";
            if (KeyCode == KeyCode.Divide) return "/";
            if (KeyCode == KeyCode.Oem1) return ";";
            if (KeyCode == KeyCode.Oemplus) return "=";
            if (KeyCode == KeyCode.Oemcomma) return ",";
            if (KeyCode == KeyCode.OemMinus) return "-";
            if (KeyCode == KeyCode.OemPeriod) return ".";
            if (KeyCode == KeyCode.Oem2) return "/";
            if (KeyCode == KeyCode.Oem3) return "`";
            if (KeyCode == KeyCode.Oem4) return "´";
            if (KeyCode == KeyCode.Oem5) return @"]";
            if (KeyCode == KeyCode.Oem6) return "[";
            //EN-US
            //case KeyboardKeyValues.Oem4)
            //	temp = "[";
            //	break;
            //case KeyboardKeyValues.Oem5)
            //	temp = @"\";
            //	break;
            //case KeyboardKeyValues.Oem6)
            //	temp = "]";
            if (KeyCode == KeyCode.Oem7) return "'";
            if (KeyCode == KeyCode.NumPad0) return "0";
            if (KeyCode == KeyCode.NumPad1) return "1";
            if (KeyCode == KeyCode.NumPad2) return "2";
            if (KeyCode == KeyCode.NumPad3) return "3";
            if (KeyCode == KeyCode.NumPad4) return "4";
            if (KeyCode == KeyCode.NumPad5) return "5";
            if (KeyCode == KeyCode.NumPad6) return "6";
            if (KeyCode == KeyCode.NumPad7) return "7";
            if (KeyCode == KeyCode.NumPad8) return "8";
            if (KeyCode == KeyCode.NumPad9) return "9";
            if (KeyCode == KeyCode.Q) return "q";
            if (KeyCode == KeyCode.W) return "w";
            if (KeyCode == KeyCode.E) return "e";
            if (KeyCode == KeyCode.R) return "r";
            if (KeyCode == KeyCode.T) return "t";
            if (KeyCode == KeyCode.Y) return "y";
            if (KeyCode == KeyCode.U) return "u";
            if (KeyCode == KeyCode.I) return "i";
            if (KeyCode == KeyCode.O) return "o";
            if (KeyCode == KeyCode.P) return "p";
            if (KeyCode == KeyCode.A) return "a";
            if (KeyCode == KeyCode.S) return "s";
            if (KeyCode == KeyCode.D) return "d";
            if (KeyCode == KeyCode.F) return "f";
            if (KeyCode == KeyCode.G) return "g";
            if (KeyCode == KeyCode.H) return "h";
            if (KeyCode == KeyCode.J) return "j";
            if (KeyCode == KeyCode.K) return "k";
            if (KeyCode == KeyCode.L) return "l";
            if (KeyCode == KeyCode.Z) return "z";
            if (KeyCode == KeyCode.X) return "x";
            if (KeyCode == KeyCode.C) return "c";
            if (KeyCode == KeyCode.V) return "v";
            if (KeyCode == KeyCode.B) return "b";
            if (KeyCode == KeyCode.N) return "n";
            if (KeyCode == KeyCode.M) return "m";
            if (KeyCode == KeyCode.D0) return "0";
            if (KeyCode == KeyCode.D1) return "1";
            if (KeyCode == KeyCode.D2) return "2";
            if (KeyCode == KeyCode.D3) return "3";
            if (KeyCode == KeyCode.D4) return "4";
            if (KeyCode == KeyCode.D5) return "5";
            if (KeyCode == KeyCode.D6) return "6";
            if (KeyCode == KeyCode.D7) return "7";
            if (KeyCode == KeyCode.D8) return "8";
            if (KeyCode == KeyCode.D9) return "9";
            return string.Empty;
        }

        private string ShiftCharacterIfItIsShiftable(string character)
        {
            if (KeyCode == KeyCode.D1) return "!";
            if (KeyCode == KeyCode.D2) return "@";
            if (KeyCode == KeyCode.D3) return "#";
            if (KeyCode == KeyCode.D4) return "$";
            if (KeyCode == KeyCode.D5) return "%";
            if (KeyCode == KeyCode.D6) return "^";
            if (KeyCode == KeyCode.D7) return "&";
            if (KeyCode == KeyCode.D8) return "*";
            if (KeyCode == KeyCode.D9) return "(";
            if (KeyCode == KeyCode.D0) return ")";
            if (KeyCode == KeyCode.Oem1) return ":";
            if (KeyCode == KeyCode.Oem2) return "?";
            if (KeyCode == KeyCode.Oem3) return "~";
            if (KeyCode == KeyCode.Oemcomma) return "<";
            if (KeyCode == KeyCode.OemMinus) return "_";
            if (KeyCode == KeyCode.OemPeriod) return ">";
            if (KeyCode == KeyCode.Oemplus) return "+";
            if (KeyCode == KeyCode.Oem4) return "`";
            if (KeyCode == KeyCode.Oem5) return "}";
            if (KeyCode == KeyCode.Oem6) return "{";
            //EN-US
            //if (vk == KeyboardKeyValues.Oem4) return "{";
            //if (vk == KeyboardKeyValues.Oem5) return "|";
            //if (vk == KeyboardKeyValues.Oem6) return "}";
            if (KeyCode == KeyCode.Oem7) return "\"";

            //Character not "shiftable"
            return character;
        }
    }
}
