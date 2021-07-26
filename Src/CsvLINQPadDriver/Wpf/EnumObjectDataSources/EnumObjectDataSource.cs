using System;
using System.ComponentModel;
using System.Linq;

namespace CsvLINQPadDriver.Wpf.EnumObjectDataSources
{
    internal abstract class EnumObjectDataSource<T> where T: Enum
    {
        // ReSharper disable once UnusedMethodReturnValue.Global
        public Tuple<T, string>[] GetValues() =>
            Enum.GetValues(typeof(T)).OfType<T>().Select(value =>
            {
                var valueAsString = value.ToString();
                var fieldInfo = typeof(T).GetField(valueAsString);
                var descriptionAttributes = (DescriptionAttribute[])fieldInfo!.GetCustomAttributes(typeof(DescriptionAttribute), true);
                return Tuple.Create(value, descriptionAttributes.FirstOrDefault()?.Description ?? valueAsString);
            }).ToArray();
    }
}
