using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CsvLINQPadDriver.Helpers;
using LINQPad;

namespace CsvLINQPadDriver.CodeGen
{
    internal class CsvTableList<TRow> : CsvTableBase<TRow>, IList<TRow> where TRow : CsvRowBase, new()
    {
        private readonly Lazy<IList<TRow>> dataCache;

        public CsvTableList(char csvSeparator, string filePath, ICollection<CsvColumnInfo> propertiesInfo, Action<TRow> relationsInit)
            : base(csvSeparator, filePath, propertiesInfo, relationsInit)
        {
            dataCache = new Lazy<IList<TRow>>(
                () => CsvTableFactory.IsCacheStatic
                    ? (IList<TRow>) GetDataDirect().Cache(typeof(TRow).Name + ":" + FilePath)
                    : GetDataDirect().ToList(), 
                LazyThreadSafetyMode.ExecutionAndPublication 
            );
        }

        /// <summary>
        /// Load data into cache on first access and return cached data.
        /// </summary>
        /// <returns></returns>
        protected IList<TRow> GetDataCached()
        {
            try
            {
                return dataCache.Value;
            }
            catch (OutOfMemoryException oex)
            {
                throw new OutOfMemoryException("Disable CSV file cache in connection properties to prevent OOM exceptions.", oex);
            }
        }

        public override IEnumerator<TRow> GetEnumerator()
        {
            Logger.Log("{0}.GetEnumerator", GetType().FullName);
            return GetDataCached().GetEnumerator();
        }

        /// <summary>
        /// Indexes cache for WhereIndexed
        /// (propertyName,(propertyValue,rowsWithValue))
        /// </summary>
        private readonly IDictionary<string, ILookup<string, TRow>> indexes = new Dictionary<string, ILookup<string, TRow>>();

        /// <summary>
        /// Get index of rows by value of property
        /// </summary>
        /// <param name="getProperty"></param>
        /// <param name="propertyName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public override IEnumerable<TRow> WhereIndexed(Func<TRow, string> getProperty, string propertyName, params string[] values)
        {
            Logger.Log("{0}.Where({1},{2})", typeof(TRow).Name, propertyName, string.Join(",", values));

            if (!indexes.TryGetValue(propertyName, out var propertyIndex))
            {
                propertyIndex = this.ToLookup(getProperty, StringComparer.Ordinal);
                indexes.Add(propertyName, propertyIndex);
            }
            var result = values.SelectMany(value => propertyIndex[value]);
            return values.Length > 1 ? result.Distinct() : result;
        }

        public void Add(TRow item)
        {
            GetDataCached().Add(item);
        }

        public void Clear()
        {
            GetDataCached().Clear();
        }

        public bool Contains(TRow item)
        {
            return GetDataCached().Contains(item);
        }

        public void CopyTo(TRow[] array, int arrayIndex)
        {
            GetDataCached().CopyTo(array, arrayIndex);
        }

        public bool Remove(TRow item)
        {
            return GetDataCached().Remove(item);
        }

        public int Count => GetDataCached().Count;

        public bool IsReadOnly => GetDataCached().IsReadOnly;

        public int IndexOf(TRow item)
        {
            return GetDataCached().IndexOf(item);
        }

        public void Insert(int index, TRow item)
        {
            GetDataCached().Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            GetDataCached().RemoveAt(index);
        }

        public TRow this[int index]
        {
            get => GetDataCached()[index];
            set => GetDataCached()[index] = value;
        }
    }
}