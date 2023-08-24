# LINQPad Driver LPRun Unit/Integration Tests Runner #

[![Latest build](https://github.com/i2van/CsvLINQPadDriver/workflows/build/badge.svg)](https://github.com/i2van/CsvLINQPadDriver/actions)
[![NuGet](https://img.shields.io/nuget/v/LPRun)](https://www.nuget.org/packages/LPRun)
[![Downloads](https://img.shields.io/nuget/dt/LPRun)](https://www.nuget.org/packages/LPRun)
[![License](https://img.shields.io/badge/license-MIT-yellow)](https://opensource.org/licenses/MIT)

## Table of Contents ##

* [Description](#description)
* [Website](#website)
* [Download](#download)
* [Usage](#usage)
  * [Setup](#setup)
  * [LINQPad Test Script Example](#linqpad-test-script-example)
  * [NUnit Test Example](#nunit-test-example)
* [Known Issues](#known-issues)
  * [Unit-testing Frameworks Support](#unit-testing-frameworks-support)
  * [LINQPad Runtime Reference](#linqpad-runtime-reference)
* [Troubleshooting](#troubleshooting)
* [Authors](#authors)
* [Credits](#credits)
  * [Tools](#tools)
  * [NuGet](#nuget)
* [Licenses](#licenses)

## Description ##

LINQPad driver [LPRun](https://www.linqpad.net/lprun.aspx) unit/integration tests runner. Can be used for testing [LINQPad 7](https://www.linqpad.net/LINQPad7.aspx)/[LINQPad 6](https://www.linqpad.net/LINQPad6.aspx) drivers using LPRun or for running LINQPad scripts.

## Website ##

LPRun is a part of [CsvLINQPadDriver for LINQPad 7/6/5](https://github.com/i2van/CsvLINQPadDriver). LPRun source code can be found [here](https://github.com/i2van/CsvLINQPadDriver/tree/master/Src/LPRun).

## Download ##

[![NuGet](https://img.shields.io/nuget/v/LPRun)](https://www.nuget.org/packages/LPRun)

## Usage ##

Tested driver **MUST NOT** be installed via NuGet into LINQPad. In this case LPRun will use it instead of local one.

### Setup ###

1. Create test project.
2. Add LPRun [![NuGet](https://img.shields.io/nuget/v/LPRun)](https://www.nuget.org/packages/LPRun)
3. Create the following folder structure in test project:

```text
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
        Driver.InstallWithDepsJson(
            // The directory to copy driver files to.
            "CsvLINQPadDriver",
            // The LINQPad driver files.
            "CsvLINQPadDriver.dll",
            // The test folder path.
            "Tests");

    [Test]
    [TestCaseSource(nameof(TestsData))]
    public async Task Execute_ScriptWithDriverProperties_Success(
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
        var linqScript = LinqScript.Create(
            $"{linqScriptName}.linq",
            queryConfig);

        // Act: Execute test LNQPad script.
        var (output, error, exitCode) =
            await Runner.ExecuteAsync(linqScript);

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

Tests can also be run in parallel:

```csharp
[TestFixture]
public class LPRunTests
{
    [Test]
    [Parallelizable(ParallelScope.Children)]
    [TestCaseSource(nameof(ParallelizableTestsData))]
    public void Execute_ScriptWithDriverProperties_Success(
        (string linqScriptName,
         string? context,
         ICsvDataContextDriverProperties driverProperties,
         int index) testData)
    {
        // ...

        // Arrange: Create test LNQPad script.
        var linqScript = LinqScript.Create(
            $"{linqScriptName}.linq",
            queryConfig,
            $"{linqScriptName}_{testData.index}");

        // ...
    }

    private static IEnumerable<
        (string linqScriptName,
         string? context,
         ICsvDataContextDriverProperties driverProperties,
         int index)> TestsData()
    {
        // Omitted for brevity.
    }

    // Parallelized tests data.
    private static IEnumerable<
        (string linqScriptName,
         string? context,
         ICsvDataContextDriverProperties driverProperties,
         int index)> ParallelizableTestsData() =>
        TestsData().AugmentWithFileIndex(
            static testData => testData.linqScriptName,
            static (testData, index) => { testData.index = index; return testData; });
}
```

## Known Issues ##

### Unit-testing Frameworks Support ###

Tested with [NUnit](https://github.com/nunit/nunit). Other test frameworks should work as well.

### LINQPad Runtime Reference ###

Avoid referencing `LINQPad.Runtime.dll` in your tests, e.g. for [Moq](https://github.com/moq/moq4):

```csharp
// LINQPad.Runtime.dll IConnectionInfo reference:
var connectionInfoMock = new Mock<LINQPad.Extensibility.DataContext.IConnectionInfo>();
var driverProperties   = new DriverProperties(connectionInfoMock.Object);
```

This code compiles but fails in runtime with `Could not load file or assembly LINQPad.Runtime` error. If you still need to reference it, add the following target which copies assembly to output folder of the test project:

```xml
<Target Name="CopyLINQPadRuntimeToOutput" AfterTargets="Build">
  <Copy SourceFiles="$(OutputPath)\LPRun\Bin\LINQPad.Runtime.dll" DestinationFolder="$(OutputPath)" UseHardlinksIfPossible="true" />
</Target>
```

Your can avoid referencing `LINQPad.Runtime.dll` assembly by using mock framework facilities, e.g. for [Moq](https://github.com/moq/moq4) you can extract `IDriverProperties` interface and setup driver properties as follows:

```csharp
var driverProperties = Mock.Of<IDriverProperties>(props =>
    props.BoolProp   == true     &&
    props.IntProp    == 42       &&
    props.StringProp == "string"
);
```

## Authors ##

* Ivan Ivon (ivan.ivon@gmail.com)

## Credits ##

### Tools ###

* [LINQPad 7](https://www.linqpad.net/LINQPad7.aspx)
* [LINQPad Command-Line and Scripting](https://www.linqpad.net/lprun.aspx)

### NuGet ###

* [Fluent Assertions](https://github.com/fluentassertions/fluentassertions)
* [Moq](https://github.com/moq/moq4)
* [NUnit](https://github.com/nunit/nunit)

## Licenses ##

* [LICENSE](https://github.com/i2van/CsvLINQPadDriver/blob/master/Src/LPRun/LICENSE) ([MIT](https://opensource.org/licenses/MIT))
* [LINQPad End User License Agreement](https://www.linqpad.net/eula.txt)
