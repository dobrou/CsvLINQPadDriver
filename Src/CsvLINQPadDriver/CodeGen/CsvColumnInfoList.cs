using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvColumnInfoList<TRow> : List<CsvColumnInfo>
    {
        public void Add(int csvColumnIndex, Expression<Func<TRow,string>> propertyExpression)
        {
            if (!(propertyExpression.Body is MemberExpression memberExpression))
            {
                throw new ArgumentException($"{nameof(propertyExpression)} expression must be only property access", nameof(propertyExpression));
            }

            var propertyName = memberExpression.Member.Name;

            Add(new CsvColumnInfo
            {
                CsvColumnIndex = csvColumnIndex,
                PropertyName = propertyName
            });
        }
    }
}