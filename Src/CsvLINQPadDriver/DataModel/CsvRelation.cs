namespace CsvLINQPadDriver.DataModel
{
    public class CsvRelation
    {
        public string CodeName { get; set; }
        public string DisplayName { get; set; }

        public CsvTable SourceTable { get; set; }
        public CsvTable TargetTable { get; set; }

        public CsvColumn SourceColumn { get; set; }
        public CsvColumn TargetColumn { get; set; }

    }
}
