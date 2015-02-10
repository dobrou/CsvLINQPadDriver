using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsvLINQPadDriver
{
    public class ConnectionDialog
    {
        private CsvDataContextDriverProperties properties;

        public ConnectionDialog(CsvDataContextDriverProperties properties)
        {
            this.properties = properties;
        }

        internal bool? ShowDialog()
        {
            throw new NotImplementedException();
        }
    }
}
