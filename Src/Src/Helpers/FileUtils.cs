using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvLINQPadDriver.CodeGen;

namespace CsvLINQPadDriver.Helpers
{
    public class FileUtils
    {
        public static IList<T> CsvReadRows<T>(string fileName, char csvSeparator, CsvRowMappingBase<T> csvClassMap) where T : CsvRowBase, new()
        {
            Logger.Log("CsvReadRows<{0}> started.", typeof(T).FullName);
            
            var rows = new List<T>();
            try
            {
                foreach (var rowRaw in CsvReadRows(fileName, csvSeparator).Skip(1) /*skip csv header*/)
                {
                    var row = csvClassMap.InitRowObject(rowRaw);
                    rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("CsvReadRows<{0}> failed: {1}", typeof(T).FullName, ex.ToString());
                throw;
            }

            Logger.Log("CsvReadRows<{0}> finished. Loaded {1} items.", typeof(T).FullName, rows.Count);
            return rows;
        }

        private static IEnumerable<string[]> CsvReadRows(string fileName, char csvSeparator)
        {
            var csvOptions = new CsvConfiguration()
            {
                Delimiter = csvSeparator.ToString(),
                HasHeaderRecord = false,
                DetectColumnCountChanges = false,
                IgnoreReadingExceptions = true,
                WillThrowOnMissingField = false,
                BufferSize = 1024 * 1024 * 5,
            };

            using (var cp = new CsvParser(new StreamReader(fileName, true), csvOptions))
            {
                string[] row;
                while ((row = cp.Read()) != null)
                {
                    yield return row;
                }
            }
        }

        public static string[] CsvReadHeader(string fileName, char csvSeparator)
        {
            var csvOptions = new CsvConfiguration()
            {
                Delimiter = csvSeparator.ToString(),
                HasHeaderRecord = false,
            };

            try
            {
                using (var cp = new CsvParser(new StreamReader(fileName, true), csvOptions))
                {
                    return cp.Read();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("CsvReadHeader failed: {0}", ex.ToString());
                return new string[] { "Error: " + ex.ToString() };
            }
        }

        /// <summary>
        /// Detects if file is in CSV column format.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="csvSeparator"></param>
        /// <returns>True if file contains reasonable csv data. False if file does not look like CSV formatted data.</returns>
        public static bool CsvIsFormatValid(string fileName, char csvSeparator)
        {
            Logger.Log("CsvIsFormatValid<{0}> started.", fileName);
            if (!File.Exists(fileName))
                return false;

            try
            {
                var csvOptions = new CsvConfiguration()
                {
                    Delimiter = csvSeparator.ToString(),
                    HasHeaderRecord = false,
                    DetectColumnCountChanges = true,
                };

                using (var cr = new CsvParser(new StreamReader(fileName, true), csvOptions))
                {                    
                    string[] r1 = cr.Read();
                    if (r1 == null)
                        return false;

                    //0 or 1 columns
                    if (r1.Length <= 1)
                        return false;

                    string[] r2 = cr.Read();
                    if (r2 == null)
                        return false;

                    //different count of columns
                    if (r1.Length != r2.Length)
                        return false;

                    //too many strange characters
                    int charsCount = r1.Concat(r2).Sum(s => (s ?? "").Length);
                    int validCharsCount = r1.Concat(r2).Sum(s => Enumerable.Range(0, (s ?? "").Length).Count(i => char.IsLetterOrDigit(s, i)));
                    const double validCharsMinOKRation = 0.5;
                    if (validCharsCount < validCharsMinOKRation * charsCount)
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Format detection failed: {0}", ex.ToString());
                return false;
            }
            finally
            {
                Logger.Log("CsvIsFormatValid<{0}> finished.", fileName);
            }
        }

        public static char CsvDetectSeparator(string fileName, string[] csvData = null)
        {
            var defaultCsvSeparators = new char[] { ',', ';', '\t' };
            if (Path.HasExtension(fileName) && Path.GetExtension(fileName) == "tsv")
                defaultCsvSeparators = new char[] { '\t', ',', ';' };

            if (File.Exists(fileName))
            {
                try
                {
                    //get most used char from separators as separator
                    var bestSeparators = (csvData ?? File.ReadLines(fileName).Take(1).ToArray())
                        .SelectMany(l => l.ToCharArray())
                        .Where(defaultCsvSeparators.Contains)
                        .GroupBy(c => c)
                        .OrderByDescending(cg => cg.Count())
                        .Select(sg => sg.Key)
                        .ToArray()
                        ;
                    if (bestSeparators.Any())
                        return bestSeparators.First();
                }
                catch (Exception ex)
                {
                    Logger.Log("Separator detection failed: {0}", ex.ToString());
                }
            }

            return defaultCsvSeparators.First();
        }

        public static string GetLongestCommonPrefixPath(string[] paths)
        {
            string[] pathsValid =
                paths
                .Where(p => !p.StartsWith("#"))
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();
            //get longest common path prefix
            var file1Path = (pathsValid.FirstOrDefault() ?? "").Split(Path.DirectorySeparatorChar);
            var prefixes = Enumerable.Range(1, file1Path.Length).Select(c => string.Join(Path.DirectorySeparatorChar.ToString(), file1Path.Take(c).ToArray()));
            string baseDir = prefixes.LastOrDefault(prefix => pathsValid.All(path => path.StartsWith(prefix, StringComparison.Ordinal))) ?? "";
            return baseDir;
        }

        public static string[] EnumFiles(IEnumerable<string> paths)
        {
            string[] files =
                paths
                .Where(p => !p.StartsWith("#"))
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .SelectMany(EnumFiles)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            return files;
        }

        private static string[] EnumFiles(string path)
        {
            try
            {
                path = path ?? "";

                //directly one file            
                if (File.Exists(path))
                {
                    return new[] { path };
                }

                var file = Path.GetFileName(path);
                string baseDir;

                bool isPathOnlyDir = string.IsNullOrEmpty(file) || Directory.Exists(path);
                if (isPathOnlyDir)
                {
                    //default pattern in given dir
                    const string defaultFilePattern = "*.csv";
                    file = defaultFilePattern;
                    baseDir = path;
                }
                else
                {
                    //files in dir by given pattern
                    baseDir = Path.GetDirectoryName(path) ?? "";
                }

                if (!Directory.Exists(baseDir))
                    return new string[] { };

                var files = Directory.EnumerateFiles(baseDir, file, file.Contains("**") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToArray();
                return files;
            }
            catch (Exception ex)
            {
                Logger.Log("Path resolve error: {0}", ex.ToString());
                return new string[] { };
            }
        }


        private static readonly string[] sizeUnits = new string[] { "B", "KB", "MB", "GB", "TB" };
        private const long sizeUnitsStep = 1024;

        /// <summary>
        /// Return file size in human readable format with best matching size units. (B,KB...)
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileSizeInfo(string fileName)
        {
            string sizeInfo;
            try
            {
                var size = new FileInfo(fileName).Length;
                if (size <= 0)
                {
                    sizeInfo = "0" + sizeUnits.First();
                }
                else
                {
                    sizeInfo = GetSizeInfo(size);
                }
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                {
                    sizeInfo = "??" + sizeUnits.First();
                }
                else
                {
                    throw;
                }
            }
            return sizeInfo;            
        }
        /// <summary>
        /// Return file size in human readable format with best matching size units. (B,KB...)
        /// </summary>
        /// <param name="sizeBytes"></param>
        /// <returns></returns>
        public static string GetSizeInfo(long sizeBytes)
        {
            int sizeUnitRank = Math.Min(sizeUnits.Length - 1, (int)Math.Log(sizeBytes, sizeUnitsStep));
            double sizeInUnit = sizeBytes / Math.Pow(sizeUnitsStep, sizeUnitRank);
            var sizeInfo = sizeInUnit.ToString("0.#") + " " + sizeUnits[sizeUnitRank];
            return sizeInfo;
        }

    }

}
