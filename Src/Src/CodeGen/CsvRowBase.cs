using System.Linq;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvRowBase<TContext>
    {
        internal protected TContext Context { get; set; }

        /// <summary>
        /// All properties in one string, usefull for fulltext search
        /// </summary>
        /// <returns></returns>
        public string ToRowString()
        {
            //get all public string properties
            var data = this.GetType()
                .GetProperties()
                .Where(p => p.PropertyType == typeof(string))
                .Select(p => p.GetGetMethod())
                .Where(g => g != null)
                .Select(g => g.Invoke(this, null))
                .OfType<string>();
            return string.Join(",", data);
        }
    }    
}
