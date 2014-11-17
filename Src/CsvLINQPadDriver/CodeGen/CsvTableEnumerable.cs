using System;
using System.Collections.Generic;
using System.Linq;
using CsvLINQPadDriver.Helpers;

namespace CsvLINQPadDriver.CodeGen
{
    internal class CsvTableEnumerable<TRow> : CsvTableBase<TRow> where TRow : ICsvRowBase, new()
    {
        public CsvTableEnumerable(char csvSeparator, string filePath, ICollection<CsvColumnInfo> propertiesInfo, Action<TRow> relationsInit)
            : base(csvSeparator, filePath, propertiesInfo, relationsInit)
        {
        }

        public override IEnumerator<TRow> GetEnumerator()
        {
            Logger.Log("{0}.GetEnumerator", this.GetType().FullName);
            return GetDataDirect().GetEnumerator();
        }

        /// <summary>
        /// Get index of rows by value of property
        /// </summary>
        /// <param name="getProperty"></param>
        /// <param name="propertyName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public override IEnumerable<TRow> WhereIndexed(Func<TRow, string> getProperty, string propertyName, params string[] values)
        {
            CsvLINQPadDriver.Helpers.Logger.Log("{0}.Where({1},{2})", typeof(TRow).Name, propertyName, string.Join(",", values));
            return this.Where(r => values.Contains(getProperty(r), StringComparer.Ordinal));
        }

    }
}