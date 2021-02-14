﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CsvLINQPadDriver.Helpers;

namespace CsvLINQPadFileOpen
{
    class LINQPadFileOpen
    {
        [STAThread]
        static void Main(string[] args)
        {

            if (!IsLINQPadOK())
                return;

            CheckDeployCsvPlugin();

            //no args - ask for input
            if (args == null || args.Length == 0)
            {
                using (var fileDialog = new System.Windows.Forms.OpenFileDialog()
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    CheckFileExists = true,
                    Multiselect = true,
                    Title = "CsvLINQPadFileOpen - Select CSV files to load",
                })
                {
                    var dialogResult = fileDialog.ShowDialog();
                    if (dialogResult != DialogResult.OK)
                        return;
                    args = fileDialog.FileNames;
                }
            }

            //CSV context
            OpenCsv(args);
        }

        private static void ShowMessage(string message, params string[] args)
        {
            message = string.Format(message, args);
            MessageBox.Show(message, "CsvLINQPadDriver", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        static bool IsLINQPadOK()
        {
            string linqpadDir = new string[]
            {
                @"%programfiles%\LINQPad6\LINQPad6.exe", 
                @"%programfiles(x86)%\LINQPad6\LINQPad6.exe", 
                @"%ProgramW6432%\LINQPad6\LINQPad6.exe"
            }
                .Select(Environment.ExpandEnvironmentVariables)
                .Where(File.Exists)
                .FirstOrDefault()
            ;

            if (linqpadDir == null)
            {
                ShowMessage("LINQPad v6 not found.\nPlease install it from http://www.linqpad.net/");
                return false;
            }
            return true;
        }

        static void CheckDeployCsvPlugin()
        {
            const string pluginDllName = "CsvLINQPadDriver.dll";
            const string pluginSubDir = "CsvLINQPadDriver";
            string pluginBaseDir = new string[] { @"%LOCALAPPDATA%\LINQPad\Drivers\DataContext\NetCore" }
                .Select(Environment.ExpandEnvironmentVariables)
                //.Where(Directory.Exists)
                .FirstOrDefault() ?? ""
            ;
            //if (pluginBaseDir == null) return; //plugin base dir not found, better to end then try something

            string pluginDir = Path.Combine(pluginBaseDir, pluginSubDir);
            string pluginDll = Path.Combine(pluginDir, pluginDllName);

            bool isUpdate = false;
            if (Directory.Exists(pluginDir) && File.Exists(pluginDll))
            {
                isUpdate = true;
                //compare file versions
                try
                {
                    Version installedVersion = AssemblyName.GetAssemblyName(pluginDll).Version;
                    Version bundledVersion;

                    
                    using (Stream bundledAssemblyData = Assembly.GetExecutingAssembly().GetManifestResourceStream("CsvLINQPadFileOpen.DriverBinaries.CsvLINQPadDriver.dll"))
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bundledAssemblyData.CopyTo(ms);
                        var bundledAssembly = Assembly.ReflectionOnlyLoad(ms.ToArray());
                        bundledVersion = bundledAssembly.GetName().Version;
                    }

                    if(installedVersion >= bundledVersion)
                        return; //plugin already installed with update version
                }
                catch
                {
                    return; //silently ignore read version errors
                }
                
            }

            try
            {
                //install plugin into LINQPad
                Directory.CreateDirectory(pluginDir);
                const string pluginResourcesPrefix = "CsvLINQPadFileOpen.DriverBinaries.";
                foreach (string resName in Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(n => n.StartsWith(pluginResourcesPrefix)))
                {
                    using (Stream res = Assembly.GetExecutingAssembly().GetManifestResourceStream(resName))
                    using (FileStream fileStream = new FileStream(Path.Combine(pluginDir, resName.Substring(pluginResourcesPrefix.Length)), FileMode.Create, FileAccess.Write))
                    {
                        res.CopyTo(fileStream);
                    }
                }
                //copy self
                string exeFile = Assembly.GetExecutingAssembly().Location;
                using (Stream res = new FileStream(exeFile, FileMode.Open, FileAccess.Read))
                using (FileStream fileStream = new FileStream(Path.Combine(pluginDir, typeof(CsvLINQPadFileOpen.LINQPadFileOpen).FullName.Split('.')[0] + ".exe"), FileMode.Create, FileAccess.Write))
                {
                    res.CopyTo(fileStream);
                }
                ShowMessage("CsvLINQPadDriver installed successfully.\nContinue with opening CSV context.", pluginDir);
            }
            catch(Exception ex)
            {
                if (isUpdate)
                {
                    return; //silently ignore write errors
                }
                else
                {
                    ShowMessage("CsvLINQPadDriver installation into '{0}' failed.\nPlease install driver manually from .lpx file.\nError:\n {1}", pluginDir, ex.ToString());
                }
            }
        }

        static void OpenCsv(string[] files)
        {
            //get csv files
            string expression;
            string linqFilePath;
            if (files == null || files.Length == 0)
            {
                expression = "this";
                linqFilePath = Path.Combine(Directory.GetCurrentDirectory(), "_.linq");
                files = new [] { Directory.GetCurrentDirectory() };
            }
            else if (files.Length == 1 && files[0].EndsWith(".csv") && !Directory.Exists(files[0]))
            {
                //if one file, try to get whole directory where file is
                expression = @"(
from x in this." + GetFileNameSafe(files[0]) + @"
//where Regex.IsMatch( x.ToString(), "".*"")
select x
)
.Dump(1);
";
                linqFilePath = files[0] + ".linq";
                files = new[] {Path.GetDirectoryName(files[0])};
            }
            else
            {
                linqFilePath = files[0] + ".linq";
                expression = "this";
            }

            //string linqfile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".linq");
            string linq = linqConfigCsv.Replace("{{FILES}}", string.Join("\n",files)) + expression;

            if (!File.Exists(linqFilePath)) //do not overwrite anything
                File.WriteAllText(linqFilePath, linq);

            var processInfo = new ProcessStartInfo {FileName = linqFilePath, UseShellExecute = true};
            Process.Start(processInfo);

            //TODO lock file for few seconds,so TC won't delete them from temp
        }

        static string GetFileNameSafe(string fileName)
        {
            return CodeGenHelper.GetSafeCodeName(Path.GetFileNameWithoutExtension(fileName));
        }

        static string linqConfigCsv =
@"<Query Kind='Statements'>
  <!--<Output>DataGrids</Output>-->
  <Connection>
    <Driver Assembly='CsvLINQPadDriver' PublicKeyToken='e2b1b697c284321f'>CsvLINQPadDriver.CsvDataContextDriver</Driver>
    <Persist>false</Persist>
    <DriverData>
      <Files>{{FILES}}</Files>
    </DriverData>
  </Connection>
</Query>

";
              
    }
}
