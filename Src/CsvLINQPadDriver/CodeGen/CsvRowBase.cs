using System.Linq;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvRowBase
    {
        /// <summary>
        /// All string properties in one string, usefull for fulltext search
        /// </summary>
        /// <returns></returns>
        public override string ToString()
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
