using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace CsvLINQPadDriver.Helpers
{
    public class LazyEnumerable<T> : IEnumerable<T>
    {
        private readonly Lazy<IEnumerable<T>> _data;

        public LazyEnumerable(Func<IEnumerable<T>> dataSource) =>
            _data = new Lazy<IEnumerable<T>>(dataSource, LazyThreadSafetyMode.ExecutionAndPublication);

        public IEnumerator<T> GetEnumerator() =>
            _data.Value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
