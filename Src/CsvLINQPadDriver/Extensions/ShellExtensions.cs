using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace CsvLINQPadDriver.Extensions
{
    internal static partial class ShellExtensions
    {
        public sealed record ShellResult(string Message)
        {
            public ShellResult() : this(string.Empty)
            {
            }

            public static implicit operator string(ShellResult shellResult) =>
                shellResult.Message;

            public static implicit operator bool(ShellResult shellResult) =>
                string.IsNullOrWhiteSpace(shellResult.Message);
        }

        public static void ShellExecute(this string what) =>
            DoShellExecute("open", what);

        public static ShellResult Explore(this string path, bool doSelect = true) =>
            doSelect
                ? SelectFile(path)
                : DoShellExecute("explore", path);

        private static ShellResult DoShellExecute(string verb, string what)
        {
            var errorCode = ShellExecute(IntPtr.Zero, verb, what, null, null, SW_SHOW).ToInt32();
            return new ShellResult(errorCode > 32 ? string.Empty : new Win32Exception(errorCode).Message);
        }

        private static ShellResult SelectFile(string filePath)
        {
            var folder = IntPtr.Zero;
            var file = IntPtr.Zero;

            var folderPath = Path.GetDirectoryName(filePath) ?? filePath;

            try
            {
                Marshal.ThrowExceptionForHR(SHParseDisplayName(folderPath, IntPtr.Zero, out folder, 0, out _));
                Marshal.ThrowExceptionForHR(SHParseDisplayName(filePath, IntPtr.Zero, out file, 0, out _));

                IntPtr[] files = { file };
                Marshal.ThrowExceptionForHR(SHOpenFolderAndSelectItems(folder, (uint) files.Length, files, 0));
            }
            catch (Exception exception) when (exception.CanBeHandled())
            {
                return new ShellResult(SelectFileRegex().Replace(exception.Message, string.Empty));
            }
            finally
            {
                Marshal.FreeCoTaskMem(file);
                Marshal.FreeCoTaskMem(folder);
            }

            return new ShellResult();
        }

        // ReSharper disable IdentifierTypo
        // ReSharper disable InconsistentNaming
        private const int SW_SHOW = 5;

#if NET7_0_OR_GREATER
        [LibraryImport("shell32.dll", EntryPoint = nameof(ShellExecute)+"W", StringMarshalling = StringMarshalling.Utf16)] private static partial
#else
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)] private static extern
#endif
        IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string? lpParameters, string? lpDirectory, int nShowCmd);

#if NET7_0_OR_GREATER
        [LibraryImport("shell32.dll")] private static partial
#else
        [DllImport("shell32.dll")] private static extern
#endif
        int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, uint dwFlags);

#if NET7_0_OR_GREATER
        [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)] private static partial
#else
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)] private static extern
#endif
        int SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, out IntPtr pidl, uint sfgaoIn, out uint psfgaoOut);
        // ReSharper restore InconsistentNaming
        // ReSharper restore IdentifierTypo
    }
}
