using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security;

using static LPRun.LPRunException;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace LPRun;

/// <summary>
/// Provides method for getting the LINQPad script connection header.
/// </summary>
public static class ConnectionHeader
{
    /// <summary>
    /// Gets the LINQPad script connection header.
    /// </summary>
    /// <typeparam name="T">The type of driver configuration object.</typeparam>
    /// <param name="driverAssemblyName">The driver assembly name.</param>
    /// <param name="driverNamespace">The driver namespace.</param>
    /// <param name="driverConfig">The driver configuration object.</param>
    /// <param name="additionalNamespaces">The optional additional namespaces.</param>
    /// <returns>The XML LINQPad connection header.</returns>
    /// <exception cref="LPRunException">Keeps the original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
    /// <example>
    /// This shows how to get the LINQPad script connection header:
    /// <code>
    /// var connectionHeader = ConnectionHeader.Get(
    ///     // The driver assembly name.
    ///     "CsvLINQPadDriver",
    ///     // The driver namespace.
    ///     "CsvLINQPadDriver.CsvDataContextDriver",
    ///     // The driver configuration object.
    ///     driverProperties,
    ///     // The optional additional namespaces.
    ///     "System.Globalization",
    ///     "System.Runtime.CompilerServices"
    /// );
    /// </code>
    /// </example>
    public static string Get<T>(string driverAssemblyName, string driverNamespace, T driverConfig, params string[] additionalNamespaces)
        where T : notnull =>
        Wrap(() =>
            $@"<Query Kind=""Statements"">
  <Connection>
    <Driver Assembly=""{driverAssemblyName}"">{driverNamespace}</Driver>
    <DriverData>
{string.Join(Environment.NewLine, GetKeyValues(driverConfig).Select(keyValuePair => $"      <{keyValuePair.Key}>{SecurityElement.Escape(keyValuePair.Value)}</{keyValuePair.Key}>"))}
    </DriverData>
  </Connection>
  <NuGetReference>FluentAssertions</NuGetReference>
{string.Join(Environment.NewLine, new[] { "FluentAssertions" }.Concat(additionalNamespaces).Select(additionalNamespace => $"  <Namespace>{additionalNamespace}</Namespace>"))}
</Query>");

    private static IEnumerable<(string Key, string Value)> GetKeyValues<T>(T driverConfig)
        where T : notnull
    {
        return driverConfig.GetType().GetProperties()
            .Where(propertyInfo => propertyInfo.CanRead && propertyInfo.CanWrite)
            .Select(propertyInfo => (propertyInfo.Name, ValueToString(propertyInfo)));

        string ValueToString(PropertyInfo propertyInfo) =>
            propertyInfo.GetValue(driverConfig) switch
            {
                null           => string.Empty,
                bool v         => v ? "true" : "false",
                IConvertible v => v.ToString(CultureInfo.InvariantCulture),
                _              => throw new NotSupportedException($"Could not convert {propertyInfo.Name} of type {propertyInfo.PropertyType} to string")
            };
    }
}
