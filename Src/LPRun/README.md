[![NuGet](https://img.shields.io/nuget/v/LPRun.svg)](https://www.nuget.org/packages/LPRun)
[![Downloads](https://img.shields.io/nuget/dt/LPRun.svg)](https://www.nuget.org/packages/LPRun)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

# LINQPad Driver LPRun Unit/Integration Tests Runner #

## Table of Contents ##

* [Description](#description)
* [Website](#website)
* [Download](#download)
* [Usage](#usage)
  * [Setup](#setup)
  * [LINQPad Test Script Example](#linqpad-test-script-example)
  * [NUnit Test Example](#nunit-test-example)
* [Known Issues](#known-issues)
* [Authors](#authors)
* [Credits](#credits)
  * [Tools](#tools)
  * [NuGet](#nuget)
* [License](#license)

## Description ##

LINQPad driver [LPRun](https://www.linqpad.net/lprun.aspx) unit/integration tests runner. Can be used for testing [LINQPad 6](https://www.linqpad.net/LINQPad6.aspx) drivers using LPRun or for running LINQPad scripts.

## Website ##

LPRun is a part of [CsvLINQPadDriver for LINQPad 6](https://github.com/i2van/CsvLINQPadDriver). LPRun source code can be found [here](https://github.com/i2van/CsvLINQPadDriver/tree/master/Src/LPRun).

## Download ##

[![NuGet](https://img.shields.io/nuget/v/LPRun.svg)](https://www.nuget.org/packages/LPRun)

## Usage ##

Tested driver **MUST NOT** be installed via NuGet into LINQPad. In this case LPRun will use it instead of local one.

### Setup ###

1. Create test project.
2. Add LPRun [![NuGet](https://img.shields.io/nuget/v/LPRun.svg)](https://www.nuget.org/packages/LPRun)
3. Create the following folder structure in test project:

```
LPRun # Created by LPRun NuGet.
    Templates # LINQPad script templates.
    Data      # Optional: Driver data files.
```

### LINQPad Test Script Example ###

LPRun executes LINQPad test script. Test script uses [Fluent Assertions](https://github.com/fluentassertions/fluentassertions) for assertion checks.

[StringComparison.linq](https://github.com/i2van/CsvLINQPadDriver/blob/master/Tests/CsvLINQPadDriverTest/LPRun/Templates/StringComparison.linq) LINQPad test script example:

```csharp
var original = Books.First();
var copy = original with { Title = original.Title.ToUpper() };

var expectedEquality = original.Title.Equals(copy.Title, context.StringComparison);

original.Equals(copy).Should().Be(expectedEquality, Reason());

original.GetHashCode()
    .Equals(copy.GetHashCode())
    .Should()
    .Be(expectedEquality, Reason());
```

`Reason()` method (prints exact line number if assertion fails) and `context` variable are injected by [test](https://github.com/i2van/CsvLINQPadDriver/blob/master/Tests/CsvLINQPadDriverTest/LPRunTests.cs) [below](#nunit-test-example).

### NUnit Test Example ###

Full NUnit test code can be found [here](https://github.com/i2van/CsvLINQPadDriver/blob/master/Tests/CsvLINQPadDriverTest/LPRunTests.cs).

```csharp
[TestFixture]
public class LPRunTests
{
    [OneTimeSetUp]
    public void Init() =>
        // Copy driver to LPRun drivers folder.
        Driver.Install("CsvLINQPadDriver",
            "CsvLINQPadDriver.dll",
            "CsvHelper.dll",
            "Humanizer.dll"
        );

    [Test]
    [TestCaseSource(nameof(TestsData))]
    public void Execute_ScriptWithDriverProperties_Success(
        (string linqScriptName,
         string? context,
         ICsvDataContextDriverProperties driverProperties) testData)
    {
        var (linqScriptName, context, driverProperties) = testData;

        // Arrange: Create query connection header. Custom code can be added here.
        var queryConfig = GetQueryHeaders().Aggregate(
            new StringBuilder(),
            (stringBuilder, h) =>
        {
            stringBuilder.AppendLine(h);
            stringBuilder.AppendLine();
            return stringBuilder;
        }).ToString();

        // Arrange: Create test LNQPad script.
        var linqScript = LinqScript.Create($"{linqScriptName}.linq", queryConfig);

        // Act: Execute test LNQPad script.
        var (output, error, exitCode) =
            Runner.Execute(linqScript, TimeSpan.FromMinutes(2));

        // Assert.
        error.Should().BeNullOrWhiteSpace();
        exitCode.Should().Be(0);

        // Helpers.
        IEnumerable<string> GetQueryHeaders()
        {
            // Connection header.
            yield return ConnectionHeader.Get(
                "CsvLINQPadDriver",
                "CsvLINQPadDriver.CsvDataContextDriver",
                driverProperties,
                "System.Runtime.CompilerServices");

            // FluentAssertions helper.
            yield return
                @"string Reason([CallerLineNumber] int sourceLineNumber = 0) =>" +
                @" $""something went wrong at line #{sourceLineNumber}"";";

            // Test context.
            if (!string.IsNullOrWhiteSpace(context))
            {
                yield return $"var context = {context};";
            }
        }
    }

    private static IEnumerable<
        (string linqScriptName,
         string? context,
         ICsvDataContextDriverProperties driverProperties)> TestsData()
    {
        // Omitted for brevity.
    }
}
```

## Known Issues ##

* Tested with [NUnit](https://github.com/nunit/nunit). Other test frameworks should work as well.

## Authors ##

- Ivan Ivon (ivan.ivon@gmail.com)

## Credits ##

### Tools ###

- [LINQPad 6](https://www.linqpad.net/LINQPad6.aspx)
- [LINQPad Command-Line and Scripting](https://www.linqpad.net/lprun.aspx)

### NuGet ###

- [Fluent Assertions](https://github.com/fluentassertions/fluentassertions)
- [NUnit](https://github.com/nunit/nunit)

## License ##

[MIT](https://opensource.org/licenses/MIT), see [LICENSE](https://github.com/i2van/CsvLINQPadDriver/blob/master/LICENSE) file for details.
