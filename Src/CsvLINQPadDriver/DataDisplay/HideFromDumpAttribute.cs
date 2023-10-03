using System;

namespace CsvLINQPadDriver.DataDisplay;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HideFromDumpAttribute : Attribute
{
}
