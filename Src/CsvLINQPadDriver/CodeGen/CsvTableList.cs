using System;
using System.Collections.Generic;
using System.Linq;

using LINQPad;

namespace CsvLINQPadDriver.CodeGen
{
    internal class CsvTableList<TRow> : CsvTableBase<TRow>, IList<TRow>
        where TRow : CsvRowBase, new()
    {
        private readonly IDictionary<string, ILookup<string, TRow>> _indices = new Dictionary<string, ILookup<string, TRow>>();
        private readonly Lazy<IList<TRow>> _dataCache;

        public CsvTableList(bool isStringInternEnabled, char csvSeparator, string filePath, ICollection<CsvColumnInfo> propertiesInfo, Action<TRow> relationsInit)
            : base(isStringInternEnabled, csvSeparator, filePath, propertiesInfo, relationsInit) =>
            _dataCache = new Lazy<IList<TRow>>(
                () => CsvTableFactory.IsCacheStatic
                    ? (IList<TRow>) ReadData().Cache($"{typeof(TRow).Name}:{FilePath}")
                    : ReadData().ToList()
            );

        protected IList<TRow> DataCache =>
            _dataCache.Value;

        public override IEnumerator<TRow> GetEnumerator() =>
            DataCache.GetEnumerator();

        public override IEnumerable<TRow> WhereIndexed(Func<TRow, string> getProperty, string propertyName, params string[] values)
        {
            if (!_indices.TryGetValue(propertyName, out var propertyIndex))
            {
                propertyIndex = this.ToLookup(getProperty, StringComparer);
                _indices.Add(propertyName, propertyIndex);
            }

            var result = values.SelectMany(value => propertyIndex[value]);

            return values.Length > 1 ? result.Distinct() : result;
        }

        public void Add(TRow item) =>
            DataCache.Add(item);

        public void Clear() =>
            DataCache.Clear();

        public bool Contains(TRow item) =>
            DataCache.Contains(item);

        public void CopyTo(TRow[] array, int arrayIndex) =>
            DataCache.CopyTo(array, arrayIndex);

        public bool Remove(TRow item) =>
            DataCache.Remove(item);

        public int Count =>
            DataCache.Count;

        public bool IsReadOnly =>
            DataCache.IsReadOnly;

        public int IndexOf(TRow item) =>
            DataCache.IndexOf(item);

        public void Insert(int index, TRow item) =>
            DataCache.Insert(index, item);

        public void RemoveAt(int index) =>
            DataCache.RemoveAt(index);

        public TRow this[int index]
        {
            get => DataCache[index];
            set => DataCache[index] = value;
        }
    }
}