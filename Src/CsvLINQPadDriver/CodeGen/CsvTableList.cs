using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CsvLINQPadDriver.Helpers;

namespace CsvLINQPadDriver.CodeGen
{
    internal class CsvTableList<TRow> : CsvTableBase<TRow>, IList<TRow> where TRow : ICsvRowBase, new()
    {
        private readonly Lazy<IList<TRow>> dataCache;

        public CsvTableList(char csvSeparator, string filePath, ICollection<CsvColumnInfo> propertiesInfo, Action<TRow> relationsInit)
            : base(csvSeparator, filePath, propertiesInfo, relationsInit)
        {
            this.dataCache = new Lazy<IList<TRow>>(
                () => ( CsvTableBase.IsCacheStatic
                    ? (IList<TRow>) LINQPad.Extensions.Cache(GetDataDirect(), typeof(TRow).Name + ":" + FilePath)
                    : (IList<TRow>) GetDataDirect().ToList()
                ), 
                LazyThreadSafetyMode.ExecutionAndPublication 
            );
        }

        /// <summary>
        /// Load data into cache on first access and retun cached data.
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

        override public IEnumerator<TRow> GetEnumerator()
        {
            Logger.Log("{0}.GetEnumerator", this.GetType().FullName);
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
        override public IEnumerable<TRow> WhereIndexed(Func<TRow, string> getProperty, string propertyName, params string[] values)
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

        public int Count {
            get { return GetDataCached().Count; }
        }
        public bool IsReadOnly {
            get { return GetDataCached().IsReadOnly; }
        }
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
            get { return GetDataCached()[index]; }
            set { GetDataCached()[index] = value; }
        }
    }
}