namespace CsvLINQPadDriver
{
    //Empty class only for NoWPF build
    public partial class ConnectionDialog
    {
        public CsvDataContextDriverProperties Properties;

        public ConnectionDialog(CsvDataContextDriverProperties properties)
        {
            this.Properties = properties;
        }

        internal new bool? ShowDialog()
        {
            return true;
            //throw new NotImplementedException();
        }
    }
}
