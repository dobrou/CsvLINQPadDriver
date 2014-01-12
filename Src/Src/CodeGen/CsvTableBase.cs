using CsvLINQPadDriver.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvTableBase<TRow,TContext> : IEnumerable<TRow> where TRow : CsvRowBase<TContext>, new() where TContext : CsvDataContextBase
    {
        private readonly bool isDataCached;
        private LazyEnumerable<TRow> dataCache;
        internal protected TContext dataContext;

        internal readonly ICollection<CsvColumnInfo<TRow>> PropertiesInfo;

        private readonly char csvSeparator;
        public string FileName { get; private set; }

        public CsvTableBase(TContext dataContext, char csvSeparator, string fileName, ICollection<CsvColumnInfo<TRow>> propertiesInfo, bool isDataCached = true)
        {
            this.FileName = fileName;
            this.csvSeparator = csvSeparator;
            this.isDataCached = isDataCached;
            this.dataContext = dataContext;
            this.dataCache = new LazyEnumerable<TRow>(() => GetDataDirect().ToList() );
            this.PropertiesInfo = propertiesInfo;
        }

        private IEnumerable<TRow> GetDataDirect()
        {
            return FileUtils.CsvReadRows<TRow>(FileName, csvSeparator, new CsvRowMappingBase<TRow, TContext>(dataContext, PropertiesInfo));                   
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
        /// </summary>
        private readonly IDictionary<string, ILookup<string,TRow>> indexes = new Dictionary<string, ILookup<string, TRow>>();
        /// <summary>
        /// Get index of rows by value of property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public IEnumerable<TRow> WhereIndexed(string propertyName, params string[] values)
        {
            CsvLINQPadDriver.Helpers.Logger.Log("{0}.Where({1},{2})", typeof(TRow).Name, propertyName, string.Join(",", values));

            ILookup<string, TRow> propertyIndex;
            if (!indexes.TryGetValue(propertyName, out propertyIndex))
            {                
                var param = Expression.Parameter(typeof(TRow));
                var getProperty = Expression.Lambda<Func<TRow, string>>(
                    Expression.PropertyOrField(param, propertyName),
                    param
                ).Compile();

                propertyIndex = this.ToLookup(getProperty, StringComparer.Ordinal);
                //propertyIndex = this.ToLookup(r => (string)typeof(TRow).GetProperty(propertyName).GetValue(r, null), StringComparer.Ordinal);
                indexes.Add(propertyName, propertyIndex);
            }
            var result = values.SelectMany(value => propertyIndex[value]);
            return values.Length > 1 ? result.Distinct() : result;
        }  
    }
}
