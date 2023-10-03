namespace CsvLINQPadDriver.DataModel;

internal sealed record CsvColumn
(
    string Name,
    int Index
) : ICsvNames
{
    public string? CodeName { get; set; }
    public string? DisplayName { get; set; }
}
