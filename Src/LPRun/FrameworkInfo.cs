using System;
using System.Runtime.InteropServices;

using static System.Runtime.InteropServices.RuntimeInformation;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace LPRun;

/// <summary>
/// Provides collection of .NET information related properties.
/// </summary>
public static class FrameworkInfo
{
    /// <summary>
    /// Indicates that .NET is classic .NET Framework.
    /// </summary>
    /// <returns><see langword="true" /> if .NET is classic .NET Framework; otherwise, <see langword="false" />.</returns>
    public static bool IsNetFramework { get; }

    /// <summary>
    /// Indicates that .NET is modern .NET.
    /// </summary>
    /// <returns><see langword="true" /> if .NET is modern .NET; otherwise, <see langword="false" />.</returns>
    public static bool IsNet { get; }

    /// <summary>
    /// Indicates that .NET is .NET Core.
    /// </summary>
    /// <returns><see langword="true" /> if .NET is .NET Core; otherwise, <see langword="false" />.</returns>
    public static bool IsNetCore { get; }

    /// <summary>
    /// Indicates that .NET is .NET Native.
    /// </summary>
    /// <returns><see langword="true" /> if .NET is .NET Native; otherwise, <see langword="false" />.</returns>
    public static bool IsNetNative { get; }

    /// <summary>
    /// Indicates that CPU is supported.
    /// </summary>
    /// <returns><see langword="true" /> if CPU is supported; otherwise, <see langword="false" />.</returns>
    public static bool IsSupportedCpu { get; }

    /// <summary>
    /// Indicates that OS is supported.
    /// </summary>
    /// <returns><see langword="true" /> if OS is supported; otherwise, <see langword="false" />.</returns>
    public static bool IsSupportedOs { get; }

    /// <summary>
    /// Indicates that platform is ARM.
    /// </summary>
    /// <returns><see langword="true" /> if platform is ARM; otherwise, <see langword="false" />.</returns>
    public static bool IsArm { get; }

    /// <summary>
    /// Indicates that platform is 64-bit.
    /// </summary>
    /// <returns><see langword="true" /> if platform is 64-bit; otherwise, <see langword="false" />.</returns>
    public static bool Is64Bit { get; }

    /// <summary>
    /// Gets .NET version.
    /// </summary>
    /// <returns>The .NET version.</returns>
    public static Version Version { get; }

    static FrameworkInfo()
    {
        IsNetFramework = Is(".NET Framework");
        IsNetCore      = Is(".NET Core");
        IsNetNative    = Is(".NET Native");
        IsNet          = Is(".NET"); // Should be last.
        Is64Bit        = Environment.Is64BitProcess;
        IsArm          = ProcessArchitecture is Architecture.Arm64 && Is64Bit;
        IsSupportedCpu = ProcessArchitecture is Architecture.X86 or Architecture.X64 || IsArm;
        IsSupportedOs  = IsOSPlatform(OSPlatform.Windows);

        Version = Environment.Version;

        static bool Is(string what) =>
            FrameworkDescription.StartsWith(what);
    }
}
