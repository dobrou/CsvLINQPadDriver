using System.Linq;

namespace CsvLINQPadDriver.CodeGen
{
    public interface ICsvRowBase
    {
        // ReSharper disable once UnusedMember.Global
        public string ToString() =>
            string.Join(",", GetType()
                .GetProperties()
                .Where(propertyInfo => propertyInfo.PropertyType == typeof(string))
                .Select(propertyInfo => propertyInfo.GetGetMethod())
                .Where(methodInfo => methodInfo is not null)
                .Select(methodInfo => methodInfo!.Invoke(this, null))
                .OfType<string>());
    }
}
