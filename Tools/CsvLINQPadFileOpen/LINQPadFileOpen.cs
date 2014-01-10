using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvLINQPadFileOpen
{
    class LINQPadFileOpen
    {
        [STAThread]
        static void Main(string[] args)
        {
            if(!IsLINQPadOK())
                return;

            //no args - ask for input
            if (args == null || args.Length == 0)
            {
                var fileSelector = new FileSelectDialog(files: "#No files given. Please Drag&Drop some .csv files on me or put them as arguments on commandline.\n#Drag&Drop or type file paths, or directory paths with pattern like *.csv or **.csv (** will recurse subdirectory)\nc:\\*.csv");
                bool? result = fileSelector.ShowDialog();
                if (result != true)
                    return;
                args = fileSelector.Files.Split('\n').Where(l => !l.StartsWith("#")).ToArray();
            }

            CheckDeployCsvPlugin();

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
                @"%programfiles%\LINQPad4\LINQPad.exe", 
                @"%programfiles(x86)%\LINQPad4\LINQPad.exe", 
                @"%ProgramW6432%\LINQPad4\LINQPad.exe"
            }
                .Select(Environment.ExpandEnvironmentVariables)
                .Where(File.Exists)
                .FirstOrDefault()
            ;

            if (linqpadDir == null)
            {
                ShowMessage("LINQPad v4 not found.\nPlease install it from http://www.linqpad.net/");
                return false;
            }
            return true;
        }

        static void CheckDeployCsvPlugin()
        {
            string pluginSubDir = "CsvLINQPadDriver (e2b1b697c284321f)";
            string pluginBaseDir = new string[] { @"%programdata%\LINQPad\Drivers\DataContext\4.0\" }
                .Select(Environment.ExpandEnvironmentVariables)
                //.Where(Directory.Exists)
                .FirstOrDefault()
            ;
            //if (pluginBaseDir == null) return; //plugin base dir not found, better to end then try something

            string pluginDir = Path.Combine(pluginBaseDir, pluginSubDir);
            if (Directory.Exists(pluginDir))
                return; //plugin already installed

            try
            {
                //install plugin into LINQPad
                Directory.CreateDirectory(pluginDir);
                const string pluginResourcesPrefix = "CsvLINQPadFileOpen.DriverBinaries.";
                foreach (string resName in Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(n => n.StartsWith(pluginResourcesPrefix)))
                {
                    using (Stream res = Assembly.GetExecutingAssembly().GetManifestResourceStream(resName))
                    using (FileStream fileStream = new FileStream(Path.Combine(pluginDir, resName.Substring(pluginResourcesPrefix.Length)), FileMode.CreateNew, FileAccess.Write))
                    {
                        res.CopyTo(fileStream);
                    }
                }
                //copy self
                string exeFile = Assembly.GetExecutingAssembly().Location;
                using (Stream res = new FileStream(exeFile, FileMode.Open, FileAccess.Read))
                using (FileStream fileStream = new FileStream(Path.Combine(pluginDir, Path.GetFileName(exeFile)), FileMode.CreateNew, FileAccess.Write))
                {
                    res.CopyTo(fileStream);
                }
                ShowMessage("CsvLINQPadDriver installed successfully.\nContinue with opening CSV context.", pluginDir);
            }
            catch (Exception ex)
            {
                ShowMessage("CsvLINQPadDriver installation into '{0}' failed.\nPlease install driver manually from .lpx file.\nError:\n {1}", pluginDir, ex.ToString());
            }
        }

        static void OpenCsv(string[] files)
        {
            //get csv files
            string expression = "this";
            if (files == null || files.Length == 0)
            {
                files = new [] { Directory.GetCurrentDirectory() };
            }
            else if (files.Length == 1 && files[0].EndsWith(".csv") && !Directory.Exists(files[0]))
            {
                //if one file, try to get whole directory where file is
                expression = "from x in this." + GetFileNameSafe(files[0]) + "\nwhere Regex.IsMatch( x.ToRowString(), \".*\")\nselect x";
                files = new[] {Path.GetDirectoryName(files[0])};
            }
            
            string linqfile = Path.GetTempFileName() + ".linq";
            string linq = linqConfigCsv.Replace("{{FILES}}", string.Join(",",files)) + expression;
                
            File.WriteAllText(linqfile, linq);
            Process.Start(linqfile);

            //TODO lock file for few seconds,so TC won't delete them from temp
        }

        static string GetFileNameSafe(string fileName)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            foreach (var ic in Path.GetInvalidFileNameChars().Concat(new []{' ',',','-'}))
            {
                fileName = fileName.Replace(ic, '_');
            }
            fileName = Regex.Replace(fileName, "_+", "_");
            return fileName;
        }

        static string linqConfigCsv =
@"<Query Kind='Expression'>
  <Output>DataGrids</Output>
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
