using System.Linq;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvRowBase
    {
        public override string ToString() =>
            string.Join(",", GetType()
                .GetProperties()
                .Where(propertyInfo => propertyInfo.PropertyType == typeof(string))
                .Select(propertyInfo => propertyInfo.GetGetMethod())
                .Where(methodInfo => methodInfo != null)
                .Select(methodInfo => methodInfo.Invoke(this, null))
                .OfType<string>());
    }
}
