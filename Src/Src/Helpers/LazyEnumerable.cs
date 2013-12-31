using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace CsvLINQPadDriver.Helpers
{
    public class LazyEnumerable<T> : IEnumerable<T>
    {
        private readonly Lazy<IEnumerable<T>> data;

        public LazyEnumerable(Func<IEnumerable<T>> dataSource)
        {
            this.data = new Lazy<IEnumerable<T>>(dataSource, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return data.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
