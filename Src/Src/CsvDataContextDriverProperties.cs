using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;
using CsvLINQPadDriver.Helpers;

namespace CsvLINQPadDriver
{
    /// <summary>
    /// Wrapper to expose typed properties over ConnectionInfo.DriverData.
    /// </summary>
    public class CsvDataContextDriverProperties
    {
        readonly IConnectionInfo cxInfo;
        readonly XElement driverData;

        public CsvDataContextDriverProperties(IConnectionInfo cxInfo)
        {
            this.cxInfo = cxInfo;
            driverData = cxInfo.DriverData;
        }

        public bool Persist
        {
            get { return cxInfo.Persist; }
            set { cxInfo.Persist = value; }
        }

        /// <summary>
        /// Path to directory with csv files or directly to csv file
        /// </summary>
        public string Files
        {
            get { return (string)driverData.Element ("Files") ?? ""; }
            set { driverData.SetElementValue ("Files", value); }
        }

        /// <summary>
        /// Default csv separator. If empty or null, separator will be autodetected
        /// </summary>
        public string CsvSeparator 
        {
            get { return (string)driverData.Element("CsvSeparator") ?? ""; }
            set { driverData.SetElementValue("CsvSeparator", value); }
        }

        public char? CsvSeparatorChar
        {
            get
            {
                string separator = CsvSeparator;
                if (string.IsNullOrEmpty(separator))
                    return null;
                try
                {
                    return Regex.Unescape(separator).FirstOrDefault();
                }
                catch (ArgumentException)
                {
                    return separator.First();
                }
            }
        }

        /// <summary>
        /// If True - relations between csv files/tables will be detected and created. (based on files and column names)
        /// </summary>
        public bool DetectRelations
        {
            get { return ((string)driverData.Element("DetectRelations")).ToBool() ?? true; }
            set { driverData.SetElementValue("DetectRelations", value); }
        }

        /// <summary>
        /// If True - relations will be generated as Methods. So LINQPad will not follow relations in .Dump()
        /// If False - relations will be generated as Properties
        /// </summary>
        public bool RelationsAsMethods
        {
            get { return ((string)driverData.Element("RelationsAsMethods")).ToBool() ?? true; }
            set { driverData.SetElementValue("RelationsAsMethods", value); }
        }

        /// <summary>
        /// If True - some additional debug info is accessible
        /// </summary>
        public bool DebugInfo
        {
            get { return ((string)driverData.Element("DebugInfo")).ToBool() ?? false; }
            set { driverData.SetElementValue("DebugInfo", value); }
        }

        /// <summary>
        /// Beginning of every file will be scanned and suspicious files with format not similar to CSV will be ingored
        /// </summary>
        public bool IgnoreInvalidFiles
        {
            get { return ((string)driverData.Element("IgnoreInvalidFiles")).ToBool() ?? false; }
            set { driverData.SetElementValue("IgnoreInvalidFiles", value); }
        }

    }
}