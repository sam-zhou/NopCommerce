using System;
using System.Runtime.InteropServices;

namespace NopImport.Common
{
    public class AssemblyLoader
    {
        /// <summary>
        /// Gets an assembly path from the GAC given a partial name.
        /// </summary>
        /// <param name="name">An assembly partial name. May not be null.</param>
        /// <returns>
        /// The assembly path if found; otherwise null;
        /// </returns>
        public static string GetAssemblyPath(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            var finalName = name;
            var assemblyInfo = new AssemblyInfo { cchBuf = 1024 };
            assemblyInfo.currentAssemblyPath = new String('\0', assemblyInfo.cchBuf);

            IAssemblyCache assemblyCache;
            var hr = CreateAssemblyCache(out assemblyCache, 0);
            if (hr >= 0)
            {
                hr = assemblyCache.QueryAssemblyInfo(0, finalName, ref assemblyInfo);
                if (hr < 0)
                    return null;
            }

            return assemblyInfo.currentAssemblyPath;
        }


        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
        private interface IAssemblyCache
        {
            void Reserved0();

            [PreserveSig]
            int QueryAssemblyInfo(int flags, [MarshalAs(UnmanagedType.LPWStr)] string assemblyName, ref AssemblyInfo assemblyInfo);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AssemblyInfo
        {
            private readonly int cbAssemblyInfo;
            private readonly int assemblyFlags;
            private readonly long assemblySizeInKB;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string currentAssemblyPath;
            public int cchBuf; // size of path buf.
        }

        [DllImport("fusion.dll")]
        private static extern int CreateAssemblyCache(out IAssemblyCache ppAsmCache, int reserved);
    }
}
