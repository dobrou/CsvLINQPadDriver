using System;
using System.Collections.Generic;
using System.Linq;

using static System.Linq.Expressions.Expression;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvRowMappingBase<TRow>
        where TRow : ICsvRowBase, new()
    {
        private readonly Action<TRow, string[]> _propertiesInit;
        private readonly Action<TRow> _relationsInit;

        public CsvRowMappingBase(IEnumerable<CsvColumnInfo> propertiesInfo, Action<TRow> relationsInit = null)
        {
            _relationsInit = relationsInit;

            var paramRow = Parameter(typeof(TRow));
            var paramValues = Parameter(typeof(string[]));

            _propertiesInit =
                Lambda<Action<TRow, string[]>>(
                    Block(propertiesInfo.Select(property =>
                        Assign(
                        PropertyOrField(paramRow, property.PropertyName),
                        Condition(
                            LessThan(Constant(property.CsvColumnIndex), ArrayLength(paramValues)),
                            ArrayIndex(paramValues, Constant(property.CsvColumnIndex)),
                            Constant(null, typeof(string))
                            )
                        )
                    )),
                    paramRow, paramValues
                ).Compile();
        }

        public TRow InitRowObject(string[] data)
        {
            var row = new TRow();

            _propertiesInit(row, data);
            _relationsInit?.Invoke(row);

            return row;
        }
    }
}
