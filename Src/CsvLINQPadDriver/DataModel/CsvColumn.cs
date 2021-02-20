namespace CsvLINQPadDriver.DataModel
{
    public class CsvColumn : ICsvNames
    {
        public string CodeName { get; set; }
        public string DisplayName { get; set; }

        public string CsvColumnName { get; set; }
        public int CsvColumnIndex { get; set; }
    }
}
