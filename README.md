CsvLINQPadDriver
==

CsvLINQPadDriver is LINQPad data context dynamic driver for querying CSV files.

You can query data in CSV files with LINQ, just like it would be regular database. No need to write custom data model, mappings etc.

Driver automatically generates new data types for every CSV file with corresponding properties and mappings for all columns.
Based on column and file names, possible relations between CSV tables are detected and generated.

Example
--
Let's have 2 CSV files:
```
Lakes.csv:
LakeName,ID
lake1,1
etc.

Fishes.csv:
FishName,FishID,LakeID
xyz,1,1
etc.
```

CsvLINQPadDriver will generate data context similar to:
```csharp
public class CsvDataContext {
  public IEnumerable<Lakes> Lakes { get; set; }
  public IEnumerable<Fishes> Fishes { get; set; }
}
public class Lakes {
  public string LakeName { get; set; }
  public string ID { get; set; }
  public IEnumerable<Fishes> Fishes() {...} //All Fishes where Lakes.ID == Fishes.LakeID
}
public class Fishes {
  public string FishName { get; set; }
  public string FishID { get; set; }
  public string LakeID { get; set; }
  public IEnumerable<Lakes> Lakes() {...} //All Lakes where Lakes.ID == Fishes.LakeID
}
/// and mappings etc.
```

And you can query data with LINQ like:
```csharp
from lake in Lakes
where lake.LakeName.StartsWith("S") && lake.Fishes().Any()
select new { lake, fishes = lake.Fishes() }
```

Download
--
- [CsvLINQPadDriver.lpx](http://???)

Prerequisites
--
- CsvLINQPadDriver requires LINQPad 4 and .NET Framework 4.0/4.5.

Installation
--
1. Get CsvLINQPadDriver.lpx file
- Open LINQPad
- Click `Add connection` main window
- Click button `View more drivers...`
- Click button `Browse` and select downloaded .lpx file

Usage
--
You can add CSV context to LINQPad same way as any other context.
- Click `Add connection`
- Select `CSV Context Driver` and click `Next`
- Enter CSV file names or Drag&Drop files from explorer. 
  Optionally configure other options. 
- Query your data

Configuration Options
--
- **CSV Files** - list of CSV files and directories. Type one file/dir per line or Drag&Drop files from explorer. Supports special wildcards: `*` and `**`. 
  - `c:\x\*.csv` - all files in folder `c:\x`
  - `c:\x\**.csv` - all files in folder `c:\x` and all sub-directories
- CSV Separator - character used to separate columns in files. Can be `,`,`\t`, etc. If empty, separator is auto-detected.
- Detect relations - if checked, driver will try to detect and generate relations between files.
- Ignore files with invalid format - files with strange content not similar to CSV format will be ignored.
- Debug info - additional debug information will be available. For example generated Data Context source.
- Remember this connection - if checked, connection info will be saved and available after LINQPad restart.

Performance
--
When executing LINQ query on CSV context:
- Only files used in query are loaded from disk.
- As soon as any record from file is accessed, whole file is loaded into memory.
- Relations is backed up by Lookup tables.

Don't expect performance comparable with SQL server. But for reasonably sized CSV files there should not be any problem. 

Data types
--
Everything is string. Because there is no data type info in CSV files, this is best we can do.
However, driver provides few extension methods providing easy conversion from string to common types:
`"123".ToInt()` , `"123".ToDouble()`, etc.

Known Issues / TODO
--
- Some strange Unicode chracters in column names may cause errors in generated data context source code.
- Uniqueness of column names is not checked.

Author
--
- Martin Dobroucký (dobrou@gmail.com)

Credits
--
- [LINQPad](http://www.linqpad.net/)
- [CsvHelper](https://github.com/JoshClose/CsvHelper) - CSV files parsing

License
--
[MIT](http://opensource.org/licenses/MIT), see LICENSE file for details.