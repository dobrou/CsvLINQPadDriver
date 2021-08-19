using System.Collections.Generic;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvColumnInfoList : List<CsvColumnInfo>
    {
        // ReSharper disable once UnusedMember.Global
        public void Add(int csvColumnIndex, string name) =>
            Add(new CsvColumnInfo(csvColumnIndex, name));
    }
}