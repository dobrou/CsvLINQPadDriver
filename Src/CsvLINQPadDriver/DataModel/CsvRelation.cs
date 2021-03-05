namespace CsvLINQPadDriver.DataModel
{
    public record CsvRelation(
        CsvTable SourceTable,
        CsvTable TargetTable,
        CsvColumn SourceColumn,
        CsvColumn TargetColumn
    ) : ICsvNames
    {
        public string? CodeName { get; set; }
        public string? DisplayName { get; set; }
    }
}
