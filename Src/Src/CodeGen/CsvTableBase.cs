using System.Threading;
using CsvLINQPadDriver.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CsvLINQPadDriver.CodeGen
{

    public class CsvTableBase
    {
        public static bool IsDataCached = true;
        public static bool IsDataCachedStatic = false;
    }

    public class CsvTableBase<TRow> : CsvTableBase, IEnumerable<TRow> where TRow : CsvRowBase, new()
    {
        private readonly Lazy<IEnumerable<TRow>> dataCache;
        private static IEnumerable<TRow> dataCacheStatic = null;

        internal readonly ICollection<CsvColumnInfo> PropertiesInfo;
        internal readonly Action<TRow> RelationsInit;

        public char CsvSeparator { get; private set; }
        public string FilePath { get; private set; }

        public CsvTableBase(char csvSeparator, string filePath, ICollection<CsvColumnInfo> propertiesInfo, Action<TRow> relationsInit)
        {
            this.CsvSeparator = csvSeparator;
            this.FilePath = filePath;
            this.PropertiesInfo = propertiesInfo;
            this.RelationsInit = relationsInit;
            this.dataCache = new Lazy<IEnumerable<TRow>>(() => GetDataDirect().ToList(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private IEnumerable<TRow> GetDataDirect()
        {
            return FileUtils.CsvReadRows(FilePath, CsvSeparator, new CsvRowMappingBase<TRow>(PropertiesInfo, RelationsInit));                   
        }

        private IEnumerable<TRow> GetDataCached()
        {
            if (IsDataCachedStatic)
            {
                return dataCacheStatic ?? (dataCacheStatic = dataCache.Value);
            }
            else
            {
                return dataCache.Value;
            }
        }

        public IEnumerator<TRow> GetEnumerator()
        {
            Logger.Log("CsvTableBase<{0}>.GetEnumerator cached:{1}", typeof(TRow).FullName, IsDataCached);
            return (IsDataCached ? GetDataCached() : GetDataDirect()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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

            if (IsDataCached)
            {
                ILookup<string, TRow> propertyIndex;
                if (!indexes.TryGetValue(propertyName, out propertyIndex))
                {
                    propertyIndex = this.ToLookup(getProperty, StringComparer.Ordinal);
                    indexes.Add(propertyName, propertyIndex);
                }
                var result = values.SelectMany(value => propertyIndex[value]);
                return values.Length > 1 ? result.Distinct() : result;
            }
            else
            {
                return this.Where(r => values.Contains(getProperty(r), StringComparer.Ordinal));
            }
        }
    }
}
