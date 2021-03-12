namespace CsvLINQPadDriver.DataModel
{
    public record CsvColumn
    (
        string Name,
        int Index
    ) : ICsvNames
    {
        public string? CodeName { get; set; }
        public string? DisplayName { get; set; }
    }
}
