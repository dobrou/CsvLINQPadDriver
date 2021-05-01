[![Latest build](https://github.com/i2van/CsvLINQPadDriver/workflows/build/badge.svg)](https://github.com/i2van/CsvLINQPadDriver/actions)
[![NuGet](https://img.shields.io/nuget/v/CsvLINQPadDriver.svg)](https://www.nuget.org/packages/CsvLINQPadDriver/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

# CsvLINQPadDriver for LINQPad 6 #

## Table of Contents ##

* [Description](#description)
* [Website](#website)
* [Download](#download)
* [Example](#example)
* [Prerequisites](#prerequisites)
* [Installation](#installation)
  * [NuGet](#nuget-)
  * [Manual](#manual)
* [Usage](#usage)
* [Configuration Options](#configuration-options)
  * [General](#general)
  * [Files](#files)
  * [Format](#format)
  * [Memory](#memory)
  * [Generation](#generation)
  * [Relations](#relations)
  * [Misc](#misc)
* [Relations](#relations-1)
* [Performance](#performance)
* [Data Types](#data-types)
* [Generated Data Object](#generated-data-object)
  * [Methods](#methods)
     * [ToString](#tostring)
     * [GetHashCode](#gethashcode)
     * [Equals](#equals)
     * [Indexers](#indexers)
  * [Properties Access](#properties-access)
  * [Extension Methods](#extension-methods)
* [Known Issues](#known-issues)
* [Authors](#authors)
* [Credits](#credits)
  * [Tools](#tools)
  * [NuGet](#nuget)
* [License](#license)

## Description ##

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

`Authors.csv`

```
Id,Name
1,Author 1
2,Author 2
3,Author 3
```

`Books.csv`

```
Id,Title,AuthorId
11,Author 1 Book 1,1
12,Author 1 Book 2,1
21,Author 2 Book 1,2
```

CsvLINQPadDriver will generate data context similar to (simplified):

```csharp
public class CsvDataContext
{
    public CsvTableBase<RAuthor> Authors { get; private set; }
    public CsvTableBase<RBook> Books { get; private set; }
}

public sealed record RAuthor
{
    public string Id { get; set; }
    public string Name { get; set; }

    public IEnumerable<RBook> Books { get; set; }
}

public sealed record RBook
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string AuthorId { get; set; }

    public IEnumerable<RAuthor> Authors { get; set; }
}
```

And you can query data with LINQ like:

```csharp
from book in Books
join author in Authors on book.AuthorId equals author.Id
select new { author.Name, book.Title }
```

## Prerequisites ##

- [LINQPad 6](https://www.linqpad.net/LINQPad6.aspx)
- [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)

## Installation ##

### NuGet [![NuGet](https://img.shields.io/nuget/v/CsvLINQPadDriver.svg)](https://www.nuget.org/packages/CsvLINQPadDriver/) ###

  - Open LINQPad 6.
  - Click `Add connection` main window.
  - Click button `View more drivers...`.
  - Click radio button `Show all drivers` and type `CsvLINQPadDriver`.
  - Install.

### Manual ###

Get latest [CsvLINQPadDriver.\*.lpx6](https://github.com/i2van/CsvLINQPadDriver/releases) file.

  - Open LINQPad 6.
  - Click `Add connection` main window.
  - Click button `View more drivers...`.
  - Click button `Install driver from .LPX6 file...` and select downloaded `lpx6` file.

## Usage ##

CSV context can be added to LINQPad 6 same way as any other context.

- Click `Add connection`.
- Select `CSV Context Driver` and click `Next`.
- Enter CSV file names or Drag&Drop (`Ctrl` adds files) from Explorer. Optionally configure other options.
- Query your data.

## Configuration Options ##

### General ###

- **CSV files** - list of CSV files and directories. Type one file/dir per line or Drag&Drop (`Ctrl` adds files) from explorer. Supports special wildcards: `*` and `**`.
  - `c:\x\*.csv` - all files in folder `c:\x`.
  - `c:\x\**.csv` - all files in folder `c:\x` and all sub-directories.

- No BOM encoding - specifies encoding for files without [BOM](https://en.wikipedia.org/wiki/Byte_order_mark). `UTF-8` is default.

### Files ###

- Order by - specifies files sort order. Affects similar files order.

### Format ###

- CSV separator - character used to separate columns in files. Can be `,`, `\t`, etc. Auto-detected if empty.
- Ignore files with invalid format - files with strange content not similar to CSV format will be ignored.
- Allow comments - lines starting with `#` will be ignored.

### Memory ###

- Cache CSV data in memory
  - if checked: parsed rows from file are cached in memory. This cache survives multiple query runs, even when query is changed. Cache is cleared as soon as LINQPad clears query data.
  - if unchecked: disable cache. Multiple enumerations of file content results in multiple reads and parsing of file. Can be significantly slower for complex queries. Significantly reduces memory usage.  Useful when reading very large files.
- Intern CSV strings - intern strings. Significantly reduce memory consumption when CSV contains repeatable values.

### Generation ###

- Generate single class for similar files - single class will be generated for similar files which allows to query them as a single one. Might not work well for files with relations.
- String comparison - string comparison for `Equals` and `GetHashCode` methods.

### Relations ###

- Detect relations - driver will try to detect and generate relations between files.
  - Hide relations from `Dump()` - LINQPad will not show relations content when `Dump()`ed. This prevents loading too many data.

### Misc ##

- Debug info - additional debug information will be available. For example generated Data Context source.
- Remember this connection - connection info will be saved and available after LINQPad restart.

## Relations ##

There is no definition of relations between CSV files, but we can guess some relations from files and columns names.
Relations between `fileName.columnName` are detected in cases similar to following examples:
```
Books.AuthorId  <-> Authors.Id
Books.AuthorsId <-> Authors.Id
Books.AuthorId  <-> Authors.AuthorId
Books.Id        <-> Authors.BookId
```

## Performance ##

When executing LINQ query on CSV context:
- Only files used in query are loaded from disk.
- As soon as any record from file is accessed, whole file is loaded into memory.
- Relations are lazily evaluated and retrieved using cached lookup tables.

Don't expect performance comparable with SQL server. But for reasonably sized CSV files there should not be any problem.

## Data Types ##

Everything is string. Because there is no data type info in CSV files, this is best we can do.

## Generated Data Object ##

Generated data object is sealed mutable [record](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/records). You can create record's shallow copy using [with](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/with-expression) expression.

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

> Relations are not participated.

#### GetHashCode ####

```csharp
int GetHashCode();
```

Returns object hash code. Hash code is not cached and recalculated each time method is called. Also note that each time driver is reloaded hash codes will be [different](https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/).

> Depends on string comparison driver setting. Relations are not participated.

#### Equals ####

```csharp
bool Equals(T obj);
bool Equals(object obj);
```

> Depends on string comparison driver setting. Relations are not participated.

#### Indexers ####

```csharp
string this[int index] { get; set; }
string this[string index] { get; set; }
```

See [properties access](#properties-access) below.

> Relations are not participated.

### Properties Access ###

- Generated data objects are mutable, however saving changes is not supported.
- Generated data object properties can be accessed either by name or via indexer.
- Index can be integer (zero-based property index) or string (property name). If there is no index `IndexOutOfRangeException` will be thrown.
- Relations can not be accessed via indexers.

```csharp
var author = Authors.First();

// Property (preferable).
var name = author.Name;
author.Name = name;

// Integer indexer.
var name = author[0];
author[0] = name;

// String indexer.
var name = author["Name"];
author["Name"] = name;
```

Property index can be found by hovering over property name at the connection pane or by using code below:

```csharp
Authors.First()
    .GetType().GetProperties()
    .Where(p => !p.GetCustomAttributes().Any())
    .Select((p, i) => new { Index = i, p.Name })
```

### Extension Methods ###

Driver provides extension methods for converting string to `T?`. `CultureInfo.InvariantCulture` is used by default.

```csharp
// Bool.
bool? ToBool(CultureInfo? cultureInfo = null);

// Int.
int? ToInt(CultureInfo? cultureInfo = null);

// Long.
long? ToLong(CultureInfo? cultureInfo = null);

// Float.
float? ToFloat(CultureInfo? cultureInfo = null);

// Double.
double? ToDouble(CultureInfo? cultureInfo = null);

// Decimal.
decimal? ToDecimal(CultureInfo? cultureInfo = null);

// Guid.
Guid? ToGuid();
Guid? ToGuid(string format);
Guid? ToGuid(string[] formats);

// DateTime.
DateTime? ToDateTime(
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);

DateTime? ToDateTime(
    string format,
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);

DateTime? ToDateTime(
    string[] formats,
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);

// TimeSpan.
TimeSpan? ToTimeSpan(CultureInfo? cultureInfo = null);

TimeSpan? ToTimeSpan(
    string format,
    TimeSpanStyles timeSpanStyles = TimeSpanStyles.None,
    CultureInfo? cultureInfo = null);

TimeSpan? ToTimeSpan(
    string[] formats,
    TimeSpanStyles timeSpanStyles = TimeSpanStyles.None,
    CultureInfo? cultureInfo = null);
```

## Known Issues ##

- Default encoding for files without BOM is UTF-8.
- Some strange Unicode characters in column names may cause errors in generated data context source code.
- Writing changed objects back to CSV is not directly supported, there is no `SubmitChanges()`. But you can use LINQPad's `Util.WriteCsv`.
- Similar files single class generation does not detect relations correctly. However, you can query over related multiple files.

## Authors ##

- Martin Dobroucký (dobrou@gmail.com)
- Ivan Ivon (ivan.ivon@gmail.com)

## Credits ##

### Tools ###

- [LINQPad 6](https://www.linqpad.net/LINQPad6.aspx)
- [LINQPad Command-Line and Scripting](https://www.linqpad.net/lprun.aspx)

### NuGet ###

- [CsvHelper](https://github.com/JoshClose/CsvHelper)
- [Fluent Assertions](https://github.com/fluentassertions/fluentassertions)
- [Humanizer](https://github.com/Humanizr/Humanizer)
- [Moq](https://github.com/moq/moq4)
- [NUnit](https://github.com/nunit/nunit)

## License ##

[MIT](https://opensource.org/licenses/MIT), see [LICENSE](LICENSE) file for details.
