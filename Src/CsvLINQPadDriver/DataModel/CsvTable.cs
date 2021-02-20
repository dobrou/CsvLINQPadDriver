using System.Collections.Generic;

namespace CsvLINQPadDriver.DataModel
{
    public class CsvTable : ICsvNames
    {
        public string CodeName { get; set; }
        public string DisplayName { get; set; }

        public string FilePath { get; set; }
        public char CsvSeparator { get; set; }

        public IList<CsvColumn> Columns { get; set; } = new List<CsvColumn>();
        public IList<CsvRelation> Relations { get; set; } = new List<CsvRelation>();
    }
}
