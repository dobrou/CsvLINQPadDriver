using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CsvHelper.Configuration;
using CsvLINQPadDriver.Helpers;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvTableBase<TRow,TContext> : IEnumerable<TRow> where TRow : CsvRowBase, new() where TContext : CsvDataContextBase
    {
        private readonly bool isDataCached;
        private LazyEnumerable<TRow> dataCache;
        internal protected TContext dataContext;

        private readonly CsvClassMap<TRow> csvClassMap;

        private readonly char csvSeparator;
        public string FileName { get; private set; }

        public CsvTableBase(TContext dataContext, string fileName, char csvSeparator, CsvClassMap<TRow> csvClassMap, bool isDataCached = true)
        {
            this.FileName = fileName;
            this.csvSeparator = csvSeparator;
            this.csvClassMap = csvClassMap;
            this.isDataCached = isDataCached;
            this.dataContext = dataContext;
            this.dataCache = new LazyEnumerable<TRow>(() => GetDataDirect().ToList() );
        }

        private IEnumerable<TRow> GetDataDirect()
        {
            return FileUtils.CsvReadRows<TRow>(FileName, csvSeparator, csvClassMap);                   
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
    }
}
