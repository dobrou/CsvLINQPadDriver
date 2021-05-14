using System;
using System.IO;
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
                SelectItem(path);
            }
            else
            {
                ShellExecute("explore", path);
            }
        }

        // ReSharper disable IdentifierTypo
        // ReSharper disable InconsistentNaming
        private const int SW_SHOWNORMAL = 1;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

        [DllImport("shell32.dll")]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, IntPtr[] apidl, uint dwFlags);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern void SHParseDisplayName(string name, IntPtr bindingContext, out IntPtr pidl, uint sfgaoIn, out uint psfgaoOut);
        // ReSharper restore InconsistentNaming
        // ReSharper restore IdentifierTypo

        private static void ShellExecute(string verb, string what) =>
            ShellExecute(IntPtr.Zero, verb, what, string.Empty, string.Empty, SW_SHOWNORMAL);

        private static void SelectItem(string filePath)
        {
            var folderPath = Path.GetDirectoryName(filePath);

            SHParseDisplayName(folderPath!, IntPtr.Zero, out var folder, 0, out _);

            if (folder == IntPtr.Zero)
            {
                return;
            }

            SHParseDisplayName(filePath, IntPtr.Zero, out var file, 0, out _);

            if (file != IntPtr.Zero)
            {
                IntPtr[] files = { file };

                SHOpenFolderAndSelectItems(folder, (uint)files.Length, files, 0);
                Marshal.FreeCoTaskMem(file);
            }

            Marshal.FreeCoTaskMem(folder);
        }
    }
}
