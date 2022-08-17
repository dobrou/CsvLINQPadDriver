using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

[assembly: CompilationRelaxations(CompilationRelaxations.NoStringInterning)]

[SimpleJob(RuntimeMoniker.Net461)]
[SimpleJob(RuntimeMoniker.Net48)]
[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[SimpleJob(RuntimeMoniker.Net50)]
[SimpleJob(RuntimeMoniker.Net60, baseline: true)]
public class StringEqualsBenchmark
{
    private const string Str        = "QazxswedcvfrtGBNHYujM,kiOL>123456";
    private const string StrSameLen = "QazxswedcvfrtGBNHYujM,kiOL>123455"; // Diff by last char.
    private const string StrDiffLen = Str + "_";
    private const string StrSame    = Str;

    [Benchmark(Description = "Equals.DiffLen")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool DiffLenStringEquals() =>
        DoStringEquals(Str, StrDiffLen);

    [Benchmark(Description = "StringComparer.DiffLen")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool DiffLenStringComparerEquals() =>
        DoStringComparerEquals(Str, StrDiffLen);

    [Benchmark(Description = "Equals.SameLen")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool SameLenStringEquals() =>
        DoStringEquals(Str, StrSameLen);

    [Benchmark(Description = "StringComparer.SameLen", Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool SameLenStringComparerEquals() =>
        DoStringComparerEquals(Str, StrSameLen);

    [Benchmark(Description = "Equals.Same")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool SameStringEquals() =>
        DoStringEquals(Str, StrSame);

    [Benchmark(Description = "StringComparer.Same")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool SameStringComparerEquals() =>
        DoStringComparerEquals(Str, StrSame);

    private static bool DoStringEquals(string s1, string s2) =>
        string.Equals(s1, s2, StringComparison.Ordinal);
    
    private static bool DoStringComparerEquals(string s1, string s2) =>
        StringComparer.Ordinal.Equals(s1, s2);
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}
