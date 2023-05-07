using System;
using System.Collections;
using System.Collections.Generic;

namespace CsvLINQPadDriver.CodeGen
{
    public sealed class LazyEnumerable<T> : IEnumerable<T>
    {
        private readonly Lazy<IEnumerable<T>> _data;

        public LazyEnumerable(Func<IEnumerable<T>> data) =>
            _data = new Lazy<IEnumerable<T>>(data);

        public IEnumerator<T> GetEnumerator() =>
            _data.Value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
