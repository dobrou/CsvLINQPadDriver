using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsvLINQPadDriver
{
    //Empty class only for NoWPF build
    public class ConnectionDialog
    {
        public CsvDataContextDriverProperties properties;

        public ConnectionDialog(CsvDataContextDriverProperties properties)
        {
            this.properties = properties;
        }

        internal bool? ShowDialog()
        {
            return true;
            //throw new NotImplementedException();
        }
    }
}
