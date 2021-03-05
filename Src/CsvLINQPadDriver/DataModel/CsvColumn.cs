namespace CsvLINQPadDriver.DataModel
{
    public record CsvColumn
    (
        string CsvColumnName,
        int CsvColumnIndex
    ) : ICsvNames
    {
        public string? CodeName { get; set; }
        public string? DisplayName { get; set; }
    }
}
