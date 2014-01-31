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
        public enum DataCacheTypeEnum 
        { 
            /// <summary>
            /// No Caching of data read from file. Multiple enumerations of file content results in multiple reads and parsing of file.
            /// Can be significantly slower for complex queries.
            /// Significantly reduces memory usage. Useful when reading very large files.
            /// </summary>
            Disabled,
            /// <summary>
            /// Parsed rows from file are cached during one query run.
            /// </summary>
            Enabled,
            /// <summary>
            /// Parsed rows from file are cached in static cache.
            /// This cache survives multiple query runs, even when query is changed.
            /// Chache is cleared as soon as LINQPad clears Application Domain of query.
            /// </summary>
            EnabledStatic,
        }

        public static DataCacheTypeEnum DataCacheType = DataCacheTypeEnum.Enabled;
    }

    public class CsvTableBase<TRow> : CsvTableBase, IEnumerable<TRow> where TRow : CsvRowBase, new()
    {
        private Lazy<IEnumerable<TRow>> dataCache;

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

        private IEnumerable<TRow> GetData()
        {
            IEnumerable<TRow> data;
            switch (DataCacheType)
            {
                case DataCacheTypeEnum.Disabled:
                    data = GetDataDirect();
                    break;
                case DataCacheTypeEnum.EnabledStatic:
                    data = LINQPad.Extensions.Cache(GetDataDirect(), typeof(TRow).Name + ":" + FilePath);
                    break;
                case DataCacheTypeEnum.Enabled:
                default:
                    data = dataCache.Value;
                    break;
            }
            return data;
        }

        public IEnumerator<TRow> GetEnumerator()
        {
            Logger.Log("CsvTableBase<{0}>.GetEnumerator cache:{1}", typeof(TRow).FullName, DataCacheType.ToString());

            try
            {
                return GetData().GetEnumerator();
            }
            catch (OutOfMemoryException oex)
            {
                if (DataCacheType == DataCacheTypeEnum.Disabled)
                {
                    throw;
                }
                throw new OutOfMemoryException("Prevent OOM exceptions by disabling CSV files cache. Add following setting to the query beginning: CsvLINQPadDriver.CodeGen.CsvTableBase.DataCacheType = CsvLINQPadDriver.CodeGen.CsvTableBase.DataCacheTypeEnum.Disabled;", oex);
            }            
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

            if (DataCacheType == DataCacheTypeEnum.Disabled)
            {
                return this.Where(r => values.Contains(getProperty(r), StringComparer.Ordinal));
            }
            else
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
        }
    }
}
