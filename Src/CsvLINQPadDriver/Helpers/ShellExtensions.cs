using System;
using System.Runtime.InteropServices;

namespace CsvLINQPadDriver.Helpers
{
    public static class ShellExtensions
    {
        public static void ShellExecute(this string what) =>
            ShellExecute("open", what);

        public static void Explore(this string path, bool doSelect = true)
        {
            if (doSelect)
            {
                SelectFile(path);
            }
            else
            {
                ShellExecute("explore", path);
            }
        }

        private static void ShellExecute(string verb, string what) =>
            ShellExecute(IntPtr.Zero, verb, what, null, null, SW_SHOW);

        private static void SelectFile(string file)
        {
            // ReSharper disable once IdentifierTypo
            IntPtr pidl = ILCreateFromPath(file);
            if (pidl != IntPtr.Zero)
            {
                SHOpenFolderAndSelectItems(pidl, 0, IntPtr.Zero, 0);
                ILFree(pidl);
            }
        }

        // ReSharper disable IdentifierTypo
        // ReSharper disable InconsistentNaming
        private const int SW_SHOW = 5;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string? lpParameters, string? lpDirectory, int nShowCmd);

        [DllImport("shell32.dll")]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, IntPtr apidl, uint dwFlags);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr ILCreateFromPath(string pszPath);

        [DllImport("shell32.dll")]
        private static extern void ILFree(IntPtr pidl);
        // ReSharper restore InconsistentNaming
        // ReSharper restore IdentifierTypo
    }
}
