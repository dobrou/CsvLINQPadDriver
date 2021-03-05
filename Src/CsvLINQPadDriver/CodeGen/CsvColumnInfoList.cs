using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvColumnInfoList<TRow> : List<CsvColumnInfo>
    {
        // ReSharper disable once UnusedMember.Global
        public void Add(int csvColumnIndex, Expression<Func<TRow, string>> propertyExpression)
        {
            var memberExpression = propertyExpression.Body as MemberExpression ?? 
                throw new ArgumentException($"{nameof(propertyExpression)} expression must be only property access", nameof(propertyExpression));

            Add(new CsvColumnInfo(csvColumnIndex, memberExpression.Member.Name));
        }
    }
}