using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CsvLINQPadDriver.DataModel;

namespace CsvLINQPadDriver.CodeGen
{
    internal class CsvRowMappingBase<TRow, TContext> : CsvClassMap<TRow> where TContext : CsvDataContextBase where TRow : CsvRowBase<TContext>, new()
    {
        readonly protected TContext dataContext;
        readonly ICollection<CsvColumnInfo> propertiesInfo;

        internal CsvRowMappingBase(TContext dataContext, ICollection<CsvColumnInfo> propertiesInfo)
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

        protected void MapRelation<TTargetTable,TTargetRow>(string propertyName, string sourcePropertyName, TTargetTable targetTable, string targetPropertyName) where TTargetTable : CsvTableBase<TRow,TContext> where TTargetRow : CsvRowBase<TContext>
        {
            //public IEnumerable<TEngines_DBRow> Engines_DB() { { return base.Context.Engines_DB.WhereIndexed("EngineID", this.EngineID); } }
            //public IEnumerable<" + rel.TargetTable.GetCodeRowClassName() + @"> " + rel.CodeName + (relationsAsMethods ? "() {" : " { get") + @" { return base.Context." + rel.TargetTable.CodeName + @".WhereIndexed(""" + rel.TargetColumn.CodeName + @""", this." + rel.SourceColumn.CodeName + @"); } }"
            //TODO
            var param = Expression.Parameter(typeof(TRow));
            var getProperty = Expression.Lambda<Func<TRow, object>>(
                Expression.PropertyOrField(param, propertyName),
                param
            );

            Map(getProperty).ConvertUsing(row =>
            {
                //targetTable.WhereIndexed(targetPropertyName, )
                return "";
            });
        }
    }
}
