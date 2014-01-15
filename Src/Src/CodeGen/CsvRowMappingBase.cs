using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CsvLINQPadDriver.DataModel;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvRowMappingBase<TRow> where TRow : CsvRowBase, new()
    {
        private readonly ICollection<CsvColumnInfo> propertiesInfo;
        private readonly Action<TRow> relationsInit;

        public CsvRowMappingBase(ICollection<CsvColumnInfo> propertiesInfo, Action<TRow> relationsInit)
        {
            this.propertiesInfo = propertiesInfo;
            this.relationsInit = relationsInit;
        }

        public TRow InitRowObject(string[] data)
        {
            var row = new TRow();

            foreach (var column in propertiesInfo)
            {
                var value = column.CsvColumnIndex >= data.Length ? null : data[column.CsvColumnIndex];
                typeof(TRow).GetProperty(column.PropertyName).SetValue(row, value, null);
            }
            relationsInit(row);
            
            return row;
        }

    }
}
