[![Latest build](https://github.com/i2van/CsvLINQPadDriver/workflows/build/badge.svg)](https://github.com/i2van/CsvLINQPadDriver/actions)
[![NuGet](https://img.shields.io/nuget/v/CsvLINQPadDriver.svg)](https://www.nuget.org/packages/CsvLINQPadDriver/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

# CsvLINQPadDriver for LINQPad 6 #

CsvLINQPadDriver is LINQPad 6 data context dynamic driver for querying CSV files.

- You can query data in CSV files with LINQ, just like it would be regular database. No need to write custom data model, mappings, etc.
- Driver automatically generates new data types for every CSV file with corresponding properties and mappings for all columns.
- Based on column and file names, possible relations between CSV tables are detected and generated.
- Single class generation allows to join similar files and query over them. Might not work well for files with relations.

## Website ##

* [This project](https://github.com/i2van/CsvLINQPadDriver)
* [Original project](https://github.com/dobrou/CsvLINQPadDriver)

## Download ##

Latest [CsvLINQPadDriver.\*.lpx6](https://github.com/i2van/CsvLINQPadDriver/releases) for LINQPad 6 manual installation.

## Example ##

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
public class CsvDataContext
{
    public IEnumerable<Lakes> Lakes { get; set; }
    public IEnumerable<Fishes> Fishes { get; set; }
}

public class Lakes
{
    public string LakeName { get; set; }
    public string ID { get; set; }

    // All Fishes where Lakes.ID == Fishes.LakeID
    public IEnumerable<Fishes> Fishes { get; set; }
}

public class Fishes
{
    public string FishName { get; set; }
    public string FishID { get; set; }
    public string LakeID { get; set; }

    // All Lakes where Lakes.ID == Fishes.LakeID
    public IEnumerable<Lakes> Lakes { get; set; }
}

/// and mappings etc.
```

And you can query data with LINQ like:

```csharp
from lake in Lakes
where lake.LakeName.StartsWith("S") && lake.Fishes.Any()
select new { lake, fishes = lake.Fishes }
```

## Prerequisites ##

- [LINQPad 6](https://www.linqpad.net/LINQPad6.aspx)
- [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)

## Installation ##

### NuGet [![NuGet](https://img.shields.io/nuget/v/CsvLINQPadDriver.svg)](https://www.nuget.org/packages/CsvLINQPadDriver/) ###

  - Open LINQPad 6.
  - Click `Add connection` main window.
  - Click button `View more drivers...`
  - Click radio button `Show all drivers` and type `CsvLINQPadDriver`.
  - Install.

### Manual ###

Get latest [CsvLINQPadDriver.\*.lpx6](https://github.com/i2van/CsvLINQPadDriver/releases) file.

  - Open LINQPad 6.
  - Click `Add connection` main window.
  - Click button `View more drivers...`
  - Click button `Install driver from .LPX6 file...` and select downloaded `lpx6` file.

## Usage ##

CSV context can be added to LINQPad 6 same way as any other context.

- Click `Add connection`
- Select `CSV Context Driver` and click `Next`
- Enter CSV file names or Drag&Drop files from explorer.
  Optionally configure other options.
  Optionally configure other options.
- Query your data.

## Configuration Options ##

### General ###

- **CSV files** - list of CSV files and directories. Type one file/dir per line or Drag&Drop files from explorer. Supports special wildcards: `*` and `**`.
  - `c:\x\*.csv` - all files in folder `c:\x`
  - `c:\x\**.csv` - all files in folder `c:\x` and all sub-directories

### File Format ###

- CSV separator - character used to separate columns in files. Can be `,`,`\t`, etc. If empty, separator is auto-detected.
- Ignore files with invalid format - files with strange content not similar to CSV format will be ignored.

### Memory ###

- Cache CSV data in memory
  - if checked: parsed rows from file are cached in memory. This cache survives multiple query runs, even when query is changed. Cache is cleared as soon as LINQPad clears query data.
  - if unchecked: disable cache. Multiple enumerations of file content results in multiple reads and parsing of file. Can be significantly slower for complex queries. Significantly reduces memory usage. Useful when reading very large files.
- Intern CSV strings - intern strings. Significantly reduce memory consumption when CSV contains repeatable values.

### Generation ###

- Generate single class for similar files - single class will be generated for similar files which allows to query them as single one. Might not work well for files with relations.
- String comparison - string comparison for `Equals` and `GetHashCode` methods.

### Relations ###

- Detect relations - driver will try to detect and generate relations between files.
  - Hide relations from `.Dump()` - LINQPad will not show relations content in `.Dump()`. This prevents loading too many data.

### Misc ##

- Debug info - additional debug information will be available. For example generated Data Context source.
- Remember this connection - connection info will be saved and available after LINQPad restart.

## Relations ##

There is no definition of relations between CSV files, but we can guess some relations from files and columns names.
Relations between `fileName.columnName` are detected in cases similar to following examples:
- `Fishes.LakeID` <-> `Lakes.ID`
- `Fishes.LakesID` <-> `Lakes.ID`
- `Fishes.LakeID` <-> `Lakes.LakeID`
- `Fishes.ID` <-> `Lakes.FishID`

## Performance ##

When executing LINQ query on CSV context:
- Only files used in query are loaded from disk.
- As soon as any record from file is accessed, whole file is loaded into memory.
- Relations are lazily evaluated and retrieved using cached lookup tables.

Don't expect performance comparable with SQL server. But for reasonably sized CSV files there should not be any problem.

## Data Types ##

Everything is string. Because there is no data type info in CSV files, this is best we can do.

## Generated Data Object ##

### Methods ##

```csharp
string ToString();

bool Equals(T obj);
bool Equals(object obj);

int GetHashCode();

string this[int index] { get; set; }
string this[string index] { get; set; }
```

#### ToString ####

```csharp
string ToString();
```

Formats object the way PowerShell [Format-List](https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/format-list) does.

#### GetHashCode ####

```csharp
int GetHashCode();
```

Returns object hash code. Hash code is not cached and recalculated each time method is called. Depends on string comparison driver setting.

Also note that each time driver is reloaded hash codes will be [different](https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/).

#### Equals ####

```csharp
bool Equals(T obj);
bool Equals(object obj);
```

Depend on string comparison driver setting.

#### Indexers ####

```csharp
string this[int index] { get; set; }
string this[string index] { get; set; }
```

See below.

### Properties Access ###

- Generated data objects are mutable, however saving changes is not supported.
- Generated data object properties can be accessed either by name or via indexer.
- Index can be integer (zero-based property index) or string (property name). If there is no index `IndexOutOfRangeException` will be thrown.

```csharp
// Property. Preferable.
var val = table.First().prop0;

// Integer indexer.
var val = table.First()[0];

// String indexer.
var val = table.First()["prop0"];
```

Property index can be found by hovering over property name at the connection pane or by using code below:

```csharp
// Prepend your table name.
.First().GetType().GetProperties().Where(p => !p.GetCustomAttributes().Any()).Select((p, i) => new { Index = i, p.Name })
```

### Extension Methods ###

Driver provides few extension methods providing easy conversion from string to nullable of common types:

```csharp
int? ToInt(CultureInfo? cultureInfo = null);
long? ToLong(CultureInfo? cultureInfo = null);
double? ToDouble(CultureInfo? cultureInfo = null);
decimal? ToDecimal(CultureInfo? cultureInfo = null);
DateTime? ToDateTime(DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null);
DateTime? ToDateTime(string format, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null);
DateTime? ToDateTime(string[] formats, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null);
TimeSpan? ToTimeSpan(CultureInfo? cultureInfo = null);
TimeSpan? ToTimeSpan(string format, TimeSpanStyles timeSpanStyles = TimeSpanStyles.None, CultureInfo? cultureInfo = null);
TimeSpan? ToTimeSpan(string[] formats, TimeSpanStyles timeSpanStyles = TimeSpanStyles.None, CultureInfo? cultureInfo = null);
bool? ToBool(CultureInfo? cultureInfo = null);
```

## Known Issues ##

- Some strange Unicode characters in column names may cause errors in generated data context source code.
- Writing changed objects back to CSV is not directly supported, there is no `.SubmitChanges()` . But you can use LINQPad's `Util.WriteCsv`.
- Similar files single class generation might not work well for files with relations.

## Authors ##

- Martin Dobroucký (dobrou@gmail.com)
- Ivan Ivon (ivan.ivon@gmail.com)

## Credits ##

### Tools ###

- [LINQPad 6](https://www.linqpad.net/LINQPad6.aspx)

### NuGet ###

- [CsvHelper](https://github.com/JoshClose/CsvHelper)
- [Fluent Assertions](https://github.com/fluentassertions/fluentassertions)
- [Humanizer](https://github.com/Humanizr/Humanizer)
- [Moq](https://github.com/moq/moq4)
- [NUnit](https://github.com/nunit/nunit)

## License ##

[MIT](https://opensource.org/licenses/MIT), see [LICENSE](LICENSE) file for details.
