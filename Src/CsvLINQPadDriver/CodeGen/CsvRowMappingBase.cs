using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvRowMappingBase<TRow> where TRow : CsvRowBase, new()
    {
        private readonly Action<TRow, string[]> propertiesInit;
        private readonly Action<TRow> relationsInit;

        public CsvRowMappingBase(IEnumerable<CsvColumnInfo> propertiesInfo, Action<TRow> relationsInit = null)
        {
            this.relationsInit = relationsInit;

            //assign all properties in one expression
            var paramRow = Expression.Parameter(typeof(TRow));
            var paramValues = Expression.Parameter(typeof(string[]));
            var exprAssignProperties =
                Expression.Lambda<Action<TRow, string[]>>(
                    Expression.Block(propertiesInfo.Select(property =>
                        Expression.Assign(
                            Expression.PropertyOrField(paramRow, property.PropertyName),
                            Expression.Condition(
                                Expression.LessThan(Expression.Constant(property.CsvColumnIndex), Expression.ArrayLength(paramValues)),
                                Expression.ArrayIndex(paramValues, Expression.Constant(property.CsvColumnIndex)),
                                Expression.Constant(null, typeof(string))
                            )
                        )
                    )),
                    paramRow, paramValues
                );
            propertiesInit = exprAssignProperties.Compile();
        }

        public TRow InitRowObject(string[] data)
        {
            var row = new TRow();

            propertiesInit(row, data);
            if (relationsInit != null)
            {
                relationsInit(row);
            }

            return row;
        }

    }
}
