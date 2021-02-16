﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvColumnInfo
    {
        public int CsvColumnIndex { get; set; }
        public string PropertyName { get; set; }
    }

    public class CsvColumnInfoList<TRow> : List<CsvColumnInfo>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="csvColumnIndex"></param>
        /// <param name="property">Property selector. Must be simple lambda like x => x.Property</param>
        public void Add(int csvColumnIndex, Expression<Func<TRow,string>> property)
        {
            var member = property.Body as MemberExpression;
            if (member == null) throw new ArgumentException($"{nameof(property)} must be only property access", nameof(property));
            string propertyName = member.Member.Name; //it's PropertyInfo or FieldInfo

            Add(new CsvColumnInfo
            {
                CsvColumnIndex = csvColumnIndex,
                PropertyName = propertyName,
            });
        }
    }
}
