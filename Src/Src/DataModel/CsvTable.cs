using System.Collections.Generic;

namespace CsvLINQPadDriver.DataModel
{
    public class CsvTable
    {
        public string FilePath { get; set; }
        public char CsvSeparator { get; set; }

        public string CodeName { get; set; }
        public string DisplayName { get; set; }
        
        public List<CsvColumn> Columns { get; set; }
        public List<CsvRelation> Relations { get; set; } 

        public CsvTable()
        {
            Columns = new List<CsvColumn>();
            Relations = new List<CsvRelation>();
        }
    }
}
