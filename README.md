[![Latest build](https://github.com/i2van/CsvLINQPadDriver/workflows/build/badge.svg)](https://github.com/i2van/CsvLINQPadDriver/actions)
[![NuGet](https://img.shields.io/nuget/v/CsvLINQPadDriver)](https://www.nuget.org/packages/CsvLINQPadDriver)
[![Downloads](https://img.shields.io/nuget/dt/CsvLINQPadDriver)](https://www.nuget.org/packages/CsvLINQPadDriver)
[![License](https://img.shields.io/badge/license-MIT-yellow)](https://opensource.org/licenses/MIT)

# CsvLINQPadDriver for LINQPad 7/6/5 #

## Table of Contents ##

* [Description](#description)
* [Website](#website)
* [Download](#download)
* [Example](#example)
* [Prerequisites](#prerequisites)
* [Installation](#installation)
  * [LINQPad 7/6](#linqpad-76)
    * [NuGet](#nuget)
    * [Manual](#manual)
  * [LINQPad 5](#linqpad-5)
* [Usage](#usage)
* [Configuration Options](#configuration-options)
  * [CSV Files](#csv-files)
  * [Format](#format)
  * [Memory](#memory)
  * [Generation](#generation)
  * [Relations](#relations)
  * [Misc](#misc)
* [Relations Detection](#relations-detection)
* [Performance](#performance)
* [Data Types](#data-types)
* [Generated Data Object](#generated-data-object)
  * [Methods](#methods)
    * [ToString](#tostring)
    * [GetHashCode](#gethashcode)
    * [Equals](#equals)
    * [Overloaded Operators](#overloaded-operators)
    * [Indexers](#indexers)
  * [Properties Access](#properties-access)
  * [Extension Methods](#extension-methods)
* [Known Issues](#known-issues)
* [Troubleshooting](#troubleshooting)
* [Authors](#authors)
* [Credits](#credits)
  * [Tools](#tools)
  * [Libraries](#libraries)
* [License](#license)

## Description ##

CsvLINQPadDriver is LINQPad 7/6/5 data context dynamic driver for querying [CSV](https://en.wikipedia.org/wiki/Comma-separated_values) files.

* You can query data in CSV files with LINQ, just like it would be regular database. No need to write custom data model, mappings, etc.
* Driver automatically generates new data types for every CSV file with corresponding properties and mappings for all columns.
* Based on column and file names, possible relations between CSV tables are detected and generated.
* Single class generation allows to join similar files and query over them. Might not work well for files with relations.

## Website ##

* [This project](https://github.com/i2van/CsvLINQPadDriver)
* [Original project](https://github.com/dobrou/CsvLINQPadDriver)

## Download ##

Latest [CsvLINQPadDriver.\*.lpx6/CsvLINQPadDriver.\*.lpx](https://github.com/i2van/CsvLINQPadDriver/releases) for LINQPad 7/6/5 manual installation.

## Example ##

Let's have 2 CSV files:

`Authors.csv`

```text
Id,Name
1,Author 1
2,Author 2
3,Author 3
```

`Books.csv`

```text
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

// record/class for LINQPad 7/6, class for LINQPad 5.
public sealed record RAuthor
{
    public string? Id { get; set; }
    public string? Name { get; set; }

    public IEnumerable<RBook>? Books { get; set; }
}

public sealed record RBook
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? AuthorId { get; set; }

    public IEnumerable<RAuthor>? Authors { get; set; }
}
```

And you can query data with LINQ like:

```csharp
from book in Books
join author in Authors on book.AuthorId equals author.Id
select new { author.Name, book.Title }
```

## Prerequisites ##

* [LINQPad 7](https://www.linqpad.net/LINQPad7.aspx): [.NET 6](https://dotnet.microsoft.com/download/dotnet/6.0)
* [LINQPad 6](https://www.linqpad.net/LINQPad6.aspx): [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0)/[.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
* [LINQPad 5](https://www.linqpad.net/LINQPad5.aspx): [.NET Framework 4.6.1](https://www.microsoft.com/en-us/download/details.aspx?id=49981)

## Installation ##

### LINQPad 7/6 ###

#### NuGet ####

[![NuGet](https://img.shields.io/nuget/v/CsvLINQPadDriver)](https://www.nuget.org/packages/CsvLINQPadDriver)

* Open LINQPad 7/6.
* Click `Add connection` link.
* Click button `View more drivers...`
* Click radio button `Show all drivers` and type `CsvLINQPadDriver`
* Install.

#### Manual ####

Get latest [CsvLINQPadDriver.\*.lpx6](https://github.com/i2van/CsvLINQPadDriver/releases) file.

* Open LINQPad 7/6.
* Click `Add connection` link.
* Click button `View more drivers...`
* Click button `Install driver from .LPX6 file...` and select downloaded `lpx6` file.

### LINQPad 5 ###

Get latest [CsvLINQPadDriver.\*.lpx](https://github.com/i2van/CsvLINQPadDriver/releases) file.

* Open LINQPad 5.
* Click `Add connection` link.
* Click button `View more drivers...`
* Click button `Browse...` and select downloaded `lpx` file.

## Usage ##

CSV files connection can be added to LINQPad 7/6/5 the same way as any other connection.

* Click `Add connection`
* Select `CSV Context Driver` and click `Next`
* Enter CSV file names or Drag&Drop (`Ctrl` adds files) from Explorer. Optionally configure other options.
* Query your data.

## Configuration Options ##

### CSV Files ###

* CSV files: list of CSV files and folders. Can be added via files/folder dialogs, context menu, hotkeys, by typing one file/folder per line or by Drag&drop (`Ctrl` adds files, `Alt` toggles `*` and `**` masks). Wildcards `?` and `*` are supported; `**.csv` searches in folder and its sub-folders.
  * `c:\Books\Books?.csv`: `Books.csv`, `Books1.csv`, etc. files in folder `c:\Books`
  * `c:\Books\*.csv`: all `*.csv` files in folder `c:\Books`
  * `c:\Books\**.csv`: all `*.csv` files in folder `c:\Books` and its sub-folders.
* Order files by: files sort order. Affects similar files order.
* Fallback encoding: [encoding](https://docs.microsoft.com/en-us/windows/win32/intl/code-page-identifiers) to use if file encoding could not be detected. `UTF-8` is default.
* Auto-detect file encodings: try to detect file encodings.
* Validate file paths: check if file paths are valid.
* Ignore files with invalid format: files with content which does not resemble CSV will be ignored.
* Do not lock files being read: allow other processes to modify files being read. Note that driver does not track files changes.

### Format ###

* Separator: characters used to separate columns in files. Can be `,`, `\t`, etc. Auto-detected if empty.
* Use [CsvHelper](https://joshclose.github.io/CsvHelper) library separator auto-detection: use CsvHelper library separator auto-detection instead of internal one.
* Ignore bad data: ignore malformed files.
* Ignore blank lines: do not process blank lines.
* Trim spaces: allow trimming spaces around fields and/or inside quotes around fields.
* Allow comments: allow single-line comments - lines starting with `#` (default) will be ignored.
* Header detection: detect or add header with specific column fallback name format if header could not be detected.
  * Header detection approach: specify whether header is present or not, or how to detect it by symbols it consists of.
  * Header column fallback name format: define generated columns names if there is no header.

### Memory ###

* Cache data in memory:
  * if checked: parsed rows from file are cached in memory. This cache survives multiple query runs, even when query is changed. Cache is cleared as soon as LINQPad clears query data.
  * if unchecked: disable cache. Multiple enumerations of file content results in multiple reads and parsing of file. Can be significantly slower for complex queries. Significantly reduces memory usage.  Useful when reading very large files.
* Intern strings: intern strings. Significantly reduce memory consumption when CSV contains repeatable values.
  * Use [generation](#generation) string comparison: compare interned strings using generation string comparison.

### Generation ###

* Use [record](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/records) type: generate records instead of classes (LINQPad 7/6 only).
* Generate single type for similar files: single type will be generated for similar files which allows to join similar files and query over them. Relations support is limited.
  * Also show similar files non-grouped: show similar files non-grouped in addition to similar files groups.
* String comparison: string comparison for `Equals` and `GetHashCode` methods.

### Relations ###

* Detect relations: driver will try to detect and generate relations between files.
  * Hide relations from `Dump()`: LINQPad will not show relations content when `Dump()`ed. This prevents loading too many data.

### Misc ##

* Debug info: show additional driver debug info, e.g. generated data context source, and enable logging.
* Remember this connection: connection will be available on next run.
* Contains production data: files contain production data.

## Relations Detection ##

There is no definition of relations between CSV files, but we can guess some relations from files and columns names.
Relations between `fileName.columnName` are detected in cases similar to following examples:

```text
Books.AuthorId  <-> Authors.Id
Books.AuthorsId <-> Authors.Id
Books.AuthorId  <-> Authors.AuthorId
Books.Id        <-> Authors.BookId
```

## Performance ##

When executing LINQ query for CSV connection:

* Only files used in query are loaded from disk.
* As soon as any record from file is accessed, whole file is loaded into memory.
* Relations are lazily evaluated and retrieved using cached lookup tables.

Don't expect performance comparable with SQL server. But for reasonably sized CSV files there should not be any problem.

## Data Types ##

Everything is string. Because there is no data type info in CSV files, this is best we can do.

## Generated Data Object ##

Generated data object is sealed mutable class or [record](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/records) (LINQPad 7/6 only). You can create record's shallow copy using [with](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/with-expression) expression.

### Methods ##

```csharp
string? ToString();

bool Equals(T? obj);
bool Equals(object? obj);

static bool operator == (T? obj1, T? obj2);
static bool operator != (T? obj1, T? obj2);

int GetHashCode();

string? this[int index] { get; set; }
string? this[string index] { get; set; }
```

#### ToString ####

```csharp
string? ToString();
```

Formats object the way PowerShell [Format-List](https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/format-list) does.

> Relations are not participated.

#### GetHashCode ####

```csharp
int GetHashCode();
```

Returns object hash code.

Note that:

* Generated data object is mutable.
* Hash code is not cached and recalculated each time method is called.
* Each time driver is reloaded string hash codes will be [different](https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/).

> Depends on string comparison driver setting. Relations are not participated.

#### Equals ####

```csharp
bool Equals(T? obj);
bool Equals(object? obj);
```

> Depends on string comparison driver setting. Relations are not participated.

#### Overloaded Operators ####

```csharp
static bool operator == (T? obj1, T? obj2);
static bool operator != (T? obj1, T? obj2);
```

> Depends on string comparison driver setting. Relations are not participated.

#### Indexers ####

```csharp
string? this[int index] { get; set; }
string? this[string index] { get; set; }
```

See [properties access](#properties-access) below.

> Relations are not participated.

### Properties Access ###

* Generated data objects are mutable, however saving changes is not supported.
* Generated data object properties can be accessed either by case-sensitive name or via indexer.
* Index can be integer (zero-based property index) or string (case-sensitive property name). If there is no index `IndexOutOfRangeException` will be thrown.
* Relations can not be accessed via indexers.

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

Driver provides extension methods for converting `string` (and `ReadOnlySpan<char>` for .NET Core/.NET) to `T?`. `CultureInfo.InvariantCulture` is used by default.

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

// .NET Core/.NET only.
DateTime? ToDateTime(
    ReadOnlySpan<char> format,
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);

DateTime? ToDateTime(
    string[] formats,
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);

// DateTimeOffset.
DateTimeOffset? ToDateTimeOffset(
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);

DateTimeOffset? ToDateTimeOffset(
    string format,
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);

// .NET Core/.NET only.
DateTimeOffset? ToDateTimeOffset(
    ReadOnlySpan<char> format,
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);

DateTimeOffset? ToDateTimeOffset(
    string[] formats,
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);

// TimeSpan.
TimeSpan? ToTimeSpan(CultureInfo? cultureInfo = null);

TimeSpan? ToTimeSpan(
    string format,
    TimeSpanStyles timeSpanStyles = TimeSpanStyles.None,
    CultureInfo? cultureInfo = null);

// .NET Core/.NET only.
TimeSpan? ToTimeSpan(
    ReadOnlySpan<char> format,
    TimeSpanStyles timeSpanStyles = TimeSpanStyles.None,
    CultureInfo? cultureInfo = null);

TimeSpan? ToTimeSpan(
    string[] formats,
    TimeSpanStyles timeSpanStyles = TimeSpanStyles.None,
    CultureInfo? cultureInfo = null);

// .NET 6+: DateOnly.
DateOnly? ToDateOnly(
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);

DateOnly? ToDateOnly(
    string format,
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);

// .NET Core/.NET only.
DateOnly? ToDateOnly(
    ReadOnlySpan<char> format,
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);

DateOnly? ToDateOnly(
    string[] formats,
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);

// .NET 6+: TimeOnly.
TimeOnly? ToTimeOnly(
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);

TimeOnly? ToTimeOnly(
    string format,
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);

// .NET Core/.NET only.
TimeOnly? ToTimeOnly(
    ReadOnlySpan<char> format,
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);

TimeOnly? ToTimeOnly(
    string[] formats,
    DateTimeStyles dateTimeStyles = DateTimeStyles.None,
    CultureInfo? cultureInfo = null);
```

## Known Issues ##

* Default encoding for files without BOM is UTF-8.
* Some strange Unicode characters in column names may cause errors in generated data context source code.
* Writing changed objects back to CSV is not directly supported, there is no `SubmitChanges()`. But you can use LINQPad's `Util.WriteCsv`
* Relations detection does not work well for similar files single class generation. However, you can query over related multiple files.
* Relations detection with file sorting might produce broken source code for similar files single class generation.

## Troubleshooting ##

* In case of `Cannot load type 'LINQPad.User.***' from cache` error use connection context menu **Close all connections**.

## Authors ##

* Martin Dobroucký (dobrou@gmail.com)
* Ivan Ivon (ivan.ivon@gmail.com)

## Credits ##

### Tools ###

* [LINQPad 7](https://www.linqpad.net/LINQPad7.aspx)/[LINQPad 6](https://www.linqpad.net/LINQPad6.aspx)/[LINQPad 5](https://www.linqpad.net/LINQPad5.aspx)
* [LINQPad Command-Line and Scripting](https://www.linqpad.net/lprun.aspx)

### Libraries ###

* [CsvHelper](https://github.com/JoshClose/CsvHelper)
* [Fluent Assertions](https://github.com/fluentassertions/fluentassertions)
* [Humanizer](https://github.com/Humanizr/Humanizer)
* [IsExternalInit](https://github.com/manuelroemer/IsExternalInit)
* [Microsoft.Bcl.HashCode](https://www.nuget.org/packages/Microsoft.Bcl.HashCode) (for LINQPad 5 only).
* [Moq](https://github.com/moq/moq4)
* [Nullable](https://github.com/manuelroemer/Nullable)
* [NUnit](https://github.com/nunit/nunit)
* [UnicodeCharsetDetector](https://github.com/i2van/UnicodeCharsetDetector)
* [UTF.Unknown](https://www.nuget.org/packages/UTF.Unknown)
* [Windows API Code Pack](https://github.com/contre/Windows-API-Code-Pack-1.1)
* [Windows API Code Pack Shell](https://github.com/contre/Windows-API-Code-Pack-1.1)

## License ##

[MIT](https://opensource.org/licenses/MIT), see [LICENSE](LICENSE) file for details.
