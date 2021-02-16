using System;
using System.Collections;
using System.Collections.Generic;
using CsvLINQPadDriver.Helpers;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvTableFactory
    {
        public static bool IsCacheStatic = true;

        public static CsvTableBase<TRow> CreateTable<TRow>(bool isStringInternEnabled, bool isCacheEnabled, char csvSeparator, string filePath, ICollection<CsvColumnInfo> propertiesInfo, Action<TRow> relationsInit) where TRow : CsvRowBase, new()
        {
            var table = isCacheEnabled
                ? (CsvTableBase<TRow>)new CsvTableList<TRow>(csvSeparator, filePath, propertiesInfo, relationsInit)
                : new CsvTableEnumerable<TRow>(csvSeparator, filePath, propertiesInfo, relationsInit)
            ;
            table.isStringInternEnabled = isStringInternEnabled;
            return table;
        }
    }

    public class CsvTableBase
    {
        internal bool isStringInternEnabled;
    }

    public abstract class CsvTableBase<TRow> : CsvTableBase, IEnumerable<TRow> where TRow : CsvRowBase, new()
    {
        public char CsvSeparator { get; }
        public string FilePath { get; }

        internal ICollection<CsvColumnInfo> PropertiesInfo;
        internal Action<TRow> RelationsInit;

        protected CsvTableBase(char csvSeparator, string filePath, ICollection<CsvColumnInfo> propertiesInfo, Action<TRow> relationsInit)
        {
            CsvSeparator = csvSeparator;
            FilePath = filePath;
            PropertiesInfo = propertiesInfo;
            RelationsInit = relationsInit;
        }

        protected IEnumerable<TRow> GetDataDirect()
        {
            return FileUtils.CsvReadRows(FilePath, CsvSeparator, isStringInternEnabled, new CsvRowMappingBase<TRow>(PropertiesInfo, RelationsInit));
        }

        /// <summary>
        /// Get index of rows by value of property
        /// </summary>
        /// <param name="getProperty"></param>
        /// <param name="propertyName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public abstract IEnumerable<TRow> WhereIndexed(Func<TRow, string> getProperty, string propertyName, params string[] values);

        public abstract IEnumerator<TRow> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}