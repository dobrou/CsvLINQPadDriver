using CsvLINQPadDriver.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvTableBase<TRow,TContext> : IEnumerable<TRow> where TRow : CsvRowBase, new() where TContext : CsvDataContextBase
    {
        private readonly bool isDataCached;
        private LazyEnumerable<TRow> dataCache;
        internal protected TContext dataContext;

        internal readonly ICollection<CsvColumnInfo> PropertiesInfo;
        internal readonly Action<TRow> RelationsInit;

        private readonly char csvSeparator;
        public string FileName { get; private set; }

        public CsvTableBase(TContext dataContext, char csvSeparator, string fileName, ICollection<CsvColumnInfo> propertiesInfo, Action<TRow> relationsInit, bool isDataCached = true)
        {
            this.FileName = fileName;
            this.csvSeparator = csvSeparator;
            this.isDataCached = isDataCached;
            this.dataContext = dataContext;
            this.dataCache = new LazyEnumerable<TRow>(() => GetDataDirect().ToList() );
            this.PropertiesInfo = propertiesInfo;
            this.RelationsInit = relationsInit;
        }

        private IEnumerable<TRow> GetDataDirect()
        {
            return FileUtils.CsvReadRows(FileName, csvSeparator, new CsvRowMappingBase<TRow>(PropertiesInfo, RelationsInit));                   
        }

        private IEnumerable<TRow> GetDataCached()
        {
            return dataCache;
        }

        public IEnumerator<TRow> GetEnumerator()
        {
            Logger.Log("CsvTableBase<{0}>.GetEnumerator", typeof(TRow).FullName);
            return (isDataCached ? GetDataCached() : GetDataDirect()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void OpenInNotepad()
        {
            Process.Start("notepad", this.FileName);
        }

        /// <summary>
        /// Indexes cache for WhereIndexed
        /// (propertyName,(propertyValue,rowsWithValue))
        /// </summary>
        private readonly IDictionary<string, ILookup<string,TRow>> indexes = new Dictionary<string, ILookup<string, TRow>>();

        /// <summary>
        /// Get index of rows by value of property
        /// </summary>
        /// <param name="getProperty"></param>
        /// <param name="propertyName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public IEnumerable<TRow> WhereIndexed(Func<TRow, string> getProperty, string propertyName, params string[] values)
        {
            CsvLINQPadDriver.Helpers.Logger.Log("{0}.Where({1},{2})", typeof(TRow).Name, propertyName, string.Join(",", values));

            ILookup<string, TRow> propertyIndex;
            if (!indexes.TryGetValue(propertyName, out propertyIndex))
            {
                propertyIndex = this.ToLookup(getProperty, StringComparer.Ordinal);
                indexes.Add(propertyName, propertyIndex);
            }
            var result = values.SelectMany(value => propertyIndex[value]);
            return values.Length > 1 ? result.Distinct() : result;
        }
    }
}
