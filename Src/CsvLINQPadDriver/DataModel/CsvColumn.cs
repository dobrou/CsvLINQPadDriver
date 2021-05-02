namespace CsvLINQPadDriver.DataModel
{
    internal record CsvColumn
    (
        string Name,
        int Index
    ) : ICsvNames
    {
        public string? CodeName { get; set; }
        public string? DisplayName { get; set; }
    }
}
