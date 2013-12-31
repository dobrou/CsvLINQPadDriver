using CsvHelper.Configuration;

namespace CsvLINQPadDriver.CodeGen
{
    public abstract class CsvRowMappingBase<TRow, TContext> : CsvClassMap<TRow> where TContext : CsvDataContextBase where TRow : CsvRowBase
    {
        readonly protected TContext dataContext;

        protected CsvRowMappingBase(TContext dataContext)
        {
            this.dataContext = dataContext;
        }
    }
}
