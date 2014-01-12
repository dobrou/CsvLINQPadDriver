using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CsvLINQPadDriver.CodeGen
{
    internal class CsvRowMappingBase<TRow, TContext> : CsvClassMap<TRow> where TContext : CsvDataContextBase where TRow : CsvRowBase<TContext>, new()
    {
        readonly protected TContext dataContext;
        readonly ICollection<CsvColumnInfo<TRow>> propertiesInfo;

        internal CsvRowMappingBase(TContext dataContext, ICollection<CsvColumnInfo<TRow>> propertiesInfo)
        {
            this.dataContext = dataContext;
            this.propertiesInfo = propertiesInfo;
        }

        public override void CreateMap()
        {
            CsvLINQPadDriver.Helpers.Logger.Log("{0}.CreateMap", typeof(TRow).Name);
            Map(c => c.Context).ConvertUsing(r => dataContext);
            foreach (var property in propertiesInfo)
            {
                MapColumn(property.PropertyName, property.CsvColumnIndex);
            }
        }

        protected void MapColumn(string propertyName, int csvColumnIndex)
        {
            var param = Expression.Parameter(typeof(TRow));
            var getProperty = Expression.Lambda<Func<TRow, object>>(
                Expression.PropertyOrField(param, propertyName),
                param
            );

            Map(getProperty).Index(csvColumnIndex);
        }


    }
}
