//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CsCheck;

namespace Opc.Classic.Tests.Fuzz;

/// <summary>
/// Parser-fuzz helpers shared by every Opc.Classic fuzz test project.
/// Each helper deliberately catches a closed set of documented parser
/// exceptions and fails on any other exception type.
/// All helpers are deterministic — seed via OPCCLASSIC_FUZZ_SEED or explicit CsCheck overloads.
/// </summary>
public static class FuzzHarness
{
    /// <summary>
    /// Quick CI iteration default.
    /// </summary>
    public const int DefaultIterations = 200;

    /// <summary>
    /// Deep workflow_dispatch / weekly iteration default.
    /// </summary>
    public const int DeepIterations = 10_000;
    private const int MaxFragmentSize = 65_535;
    private const string IterationsEnvironmentVariable = "OPCCLASSIC_FUZZ_ITERATIONS";
    private const string SeedEnvironmentVariable = "OPCCLASSIC_FUZZ_SEED";

    /// <summary>
    /// Gets the iteration count from OPCCLASSIC_FUZZ_ITERATIONS when it parses as a positive
    /// integer; otherwise returns <see cref="DefaultIterations"/>.
    /// </summary>
    public static int Iterations { get; } = ParsePositiveIntEnvironment(IterationsEnvironmentVariable, DefaultIterations);

    /// <summary>
    /// Gets the deterministic fuzz seed from OPCCLASSIC_FUZZ_SEED when it parses as an unsigned
    /// integer; otherwise returns 0, CsCheck's deterministic default.
    /// </summary>
    public static ulong Seed { get; } = ParseULongEnvironment(SeedEnvironmentVariable);

    /// <summary>
    /// Edge-weighted byte-array generator with short (0..16), medium (17..1024), and long
    /// (1025..65535) buckets, plus explicit empty and max-fragment arrays.
    /// </summary>
    public static Gen<byte[]> BytesEdgeWeighted { get; } = Gen.Frequency(
        (1, Gen.Const(Array.Empty<byte>())),
        (6, Gen.Byte.Array[0, 16]),
        (6, Gen.Byte.Array[17, 1_024]),
        (6, Gen.Byte.Array[1_025, MaxFragmentSize]),
        (1, Gen.Byte.Array[MaxFragmentSize]));

    /// <summary>
    /// Mutates a valid encoded blob by truncating, appending, flipping, filling, or corrupting
    /// little-endian 32-bit length fields.
    /// </summary>
    /// <param name="validInput">Known-good encoded input to mutate.</param>
    /// <returns>A deterministic CsCheck generator for mutated copies of <paramref name="validInput"/>.</returns>
    public static Gen<byte[]> MutateValid(ReadOnlyMemory<byte> validInput)
    {
        byte[] source = validInput.ToArray();
        return Gen.Select(
            Gen.Int[0, 7],
            Gen.Int,
            Gen.Int,
            Gen.Byte,
            Gen.Byte.Array[1, 64],
            (operation, first, second, value, fill) => Mutate(source, operation, first, second, value, fill));
    }

    /// <summary>
    /// Runs <paramref name="parse"/> and fails when it throws an undocumented exception, exceeds
    /// <paramref name="timeoutMs"/>, or violates <paramref name="resultInvariant"/>.
    /// </summary>
    /// <typeparam name="T">Parser return type.</typeparam>
    /// <param name="input">Input bytes being fuzzed.</param>
    /// <param name="parse">Parser under test.</param>
    /// <param name="allowedExceptions">Closed set of documented parser exception types.</param>
    /// <param name="resultInvariant">Optional invariant checked only after a successful parse.</param>
    /// <param name="timeoutMs">Maximum allowed wall-clock parse time in milliseconds.</param>
    public static void AssertParseDoesNotCrash<T>(
        ReadOnlyMemory<byte> input,
        Func<ReadOnlyMemory<byte>, T> parse,
        IReadOnlyCollection<Type> allowedExceptions,
        Action<T>? resultInvariant = null,
        int timeoutMs = 1_000)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(allowedExceptions);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timeoutMs);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            T result = parse(input);
            stopwatch.Stop();
            AssertWithinTimeout(input, stopwatch.ElapsedMilliseconds, timeoutMs);
            resultInvariant?.Invoke(result);
        }
        catch (Exception ex) when (IsAllowedParserException(ex, allowedExceptions))
        {
            stopwatch.Stop();
            AssertWithinTimeout(input, stopwatch.ElapsedMilliseconds, timeoutMs);
        }
        catch (Exception ex) when (ex is not FuzzHarnessFailureException && !IsNeverCaught(ex))
        {
            stopwatch.Stop();
            throw CreateFailure(
                input,
                $"Unexpected parser exception {ex.GetType().FullName} after {stopwatch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)} ms.",
                ex);
        }
    }

    /// <summary>
    /// Returns the lower-cased SHA-256 hex digest used as the corpus filename for <paramref name="input"/>.
    /// </summary>
    /// <param name="input">Input bytes to hash.</param>
    /// <returns>Lower-cased SHA-256 hex with a .bin extension.</returns>
    public static string CorpusFileName(ReadOnlyMemory<byte> input) => string.Concat(Convert.ToHexString(
        SHA256.HashData(input.Span)).ToLowerInvariant(), ".bin");

    /// <summary>
    /// Loads every tests/_Fixtures/Fuzz/&lt;surface&gt;/*.bin corpus file as TUnit [Arguments]-compatible rows.
    /// </summary>
    /// <param name="surface">Fuzzing surface directory name under tests/_Fixtures/Fuzz.</param>
    /// <returns>Rows whose first item is the corpus file bytes.</returns>
    public static IEnumerable<object[]> LoadCorpus(string surface)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(surface);

        string? repositoryRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
        if (repositoryRoot is null)
        {
            repositoryRoot = FindRepositoryRoot(AppContext.BaseDirectory);
        }

        if (repositoryRoot is null)
        {
            yield break;
        }

        string directory = Path.Combine(repositoryRoot, "tests", "_Fixtures", "Fuzz", surface);
        if (!Directory.Exists(directory))
        {
            yield break;
        }

        foreach (string file in Directory.EnumerateFiles(directory, "*.bin", SearchOption.TopDirectoryOnly))
        {
            yield return [File.ReadAllBytes(file)];
        }
    }

    /// <summary>
    /// Hex-dumps the first <paramref name="maxBytes"/> bytes of <paramref name="input"/> with offsets.
    /// </summary>
    /// <param name="input">Input bytes to dump.</param>
    /// <param name="maxBytes">Maximum number of input bytes to include.</param>
    /// <returns>Offset-prefixed hex dump.</returns>
    public static string HexDump(ReadOnlySpan<byte> input, int maxBytes = 256)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxBytes);

        int length = Math.Min(input.Length, maxBytes);
        if (length == 0)
        {
            return "00000000  <empty>";
        }

        var builder = new StringBuilder(((length + 15) / 16) * 80);
        for (int offset = 0; offset < length; offset += 16)
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(offset.ToString("x8", CultureInfo.InvariantCulture));
            builder.Append("  ");
            int lineLength = Math.Min(16, length - offset);
            for (int index = 0; index < lineLength; index++)
            {
                if (index == 8)
                {
                    builder.Append(' ');
                }

                builder.Append(input[offset + index].ToString("x2", CultureInfo.InvariantCulture));
                builder.Append(' ');
            }
        }

        if (input.Length > length)
        {
            builder.AppendLine();
            builder.Append("... truncated ");
            builder.Append((input.Length - length).ToString(CultureInfo.InvariantCulture));
            builder.Append(" byte(s)");
        }

        return builder.ToString();
    }

    private static byte[] Mutate(byte[] source, int operation, int first, int second, byte value, byte[] fill) => operation switch
    {
        0 => Truncate(source, first),
        1 => Append(source, fill),
        2 => FlipBit(source, first, second),
        3 => FlipByte(source, first, value),
        4 => FillBytes(source, first, second, 0),
        5 => FillBytes(source, first, second, fill),
        6 => WriteUInt32Length(source, first, uint.MaxValue),
        _ => NegateUInt32Length(source, first),
    };

    private static byte[] Truncate(byte[] source, int selector)
    {
        if (source.Length == 0)
        {
            return [];
        }

        int length = PositiveModulo(selector, source.Length + 1);
        byte[] mutated = new byte[length];
        Array.Copy(source, mutated, length);
        return mutated;
    }

    private static byte[] Append(byte[] source, byte[] suffix)
    {
        byte[] mutated = new byte[source.Length + suffix.Length];
        Array.Copy(source, mutated, source.Length);
        Array.Copy(suffix, 0, mutated, source.Length, suffix.Length);
        return mutated;
    }

    private static byte[] FlipBit(byte[] source, int offsetSelector, int bitSelector)
    {
        if (source.Length == 0)
        {
            return [1];
        }

        byte[] mutated = Copy(source);
        int offset = PositiveModulo(offsetSelector, mutated.Length);
        mutated[offset] = (byte)(mutated[offset] ^ (1 << PositiveModulo(bitSelector, 8)));
        return mutated;
    }

    private static byte[] FlipByte(byte[] source, int offsetSelector, byte value)
    {
        if (source.Length == 0)
        {
            return [value];
        }

        byte[] mutated = Copy(source);
        mutated[PositiveModulo(offsetSelector, mutated.Length)] ^= value;
        return mutated;
    }

    private static byte[] FillBytes(byte[] source, int offsetSelector, int lengthSelector, byte value)
    {
        if (source.Length == 0)
        {
            return [value];
        }

        byte[] mutated = Copy(source);
        int offset = PositiveModulo(offsetSelector, mutated.Length);
        int count = 1 + PositiveModulo(lengthSelector, mutated.Length - offset);
        Array.Fill(mutated, value, offset, count);
        return mutated;
    }

    private static byte[] FillBytes(byte[] source, int offsetSelector, int lengthSelector, byte[] fill)
    {
        if (source.Length == 0)
        {
            return Copy(fill);
        }

        byte[] mutated = Copy(source);
        int offset = PositiveModulo(offsetSelector, mutated.Length);
        int count = Math.Min(1 + PositiveModulo(lengthSelector, mutated.Length - offset), fill.Length);
        Array.Copy(fill, 0, mutated, offset, count);
        return mutated;
    }

    private static byte[] WriteUInt32Length(byte[] source, int offsetSelector, uint value)
    {
        byte[] mutated = source.Length < sizeof(uint) ? new byte[sizeof(uint)] : Copy(source);
        if (source.Length < sizeof(uint))
        {
            Array.Copy(source, mutated, source.Length);
        }

        int offset = PositiveModulo(offsetSelector, mutated.Length - sizeof(uint) + 1);
        BinaryPrimitives.WriteUInt32LittleEndian(mutated.AsSpan(offset, sizeof(uint)), value);
        return mutated;
    }

    private static byte[] NegateUInt32Length(byte[] source, int offsetSelector)
    {
        byte[] mutated = source.Length < sizeof(uint) ? new byte[sizeof(uint)] : Copy(source);
        if (source.Length < sizeof(uint))
        {
            Array.Copy(source, mutated, source.Length);
        }

        int offset = PositiveModulo(offsetSelector, mutated.Length - sizeof(uint) + 1);
        uint value = BinaryPrimitives.ReadUInt32LittleEndian(mutated.AsSpan(offset, sizeof(uint)));
        BinaryPrimitives.WriteUInt32LittleEndian(mutated.AsSpan(offset, sizeof(uint)), unchecked(0u - value));
        return mutated;
    }

    private static byte[] Copy(byte[] source)
    {
        byte[] copy = new byte[source.Length];
        Array.Copy(source, copy, source.Length);
        return copy;
    }

    private static int PositiveModulo(int value, int divisor)
    {
        int remainder = value % divisor;
        return remainder < 0 ? remainder + divisor : remainder;
    }

    private static void AssertWithinTimeout(ReadOnlyMemory<byte> input, long elapsedMs, int timeoutMs)
    {
        if (elapsedMs > timeoutMs)
        {
            throw CreateFailure(
                input,
                $"Parser exceeded timeout of {timeoutMs.ToString(CultureInfo.InvariantCulture)} ms; elapsed {elapsedMs.ToString(CultureInfo.InvariantCulture)} ms.",
                innerException: null);
        }
    }

    private static bool IsAllowedParserException(Exception exception, IReadOnlyCollection<Type> allowedExceptions)
    {
        if (IsAlwaysEscalated(exception))
        {
            return false;
        }

        Type exceptionType = exception.GetType();
        foreach (Type allowedException in allowedExceptions)
        {
            if (allowedException.IsAssignableFrom(exceptionType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAlwaysEscalated(Exception exception) =>
        exception is NullReferenceException
            or IndexOutOfRangeException
            or OverflowException
            or StackOverflowException
            or OutOfMemoryException
            or AccessViolationException
            or InvalidProgramException;

    private static bool IsNeverCaught(Exception exception) =>
        exception is StackOverflowException
            or OutOfMemoryException
            or AccessViolationException
            or InvalidProgramException;

    private static FuzzHarnessFailureException CreateFailure(ReadOnlyMemory<byte> input, string reason, Exception? innerException)
    {
        string message = string.Concat(
            reason,
            Environment.NewLine,
            "Corpus: ",
            CorpusFileName(input),
            Environment.NewLine,
            HexDump(input.Span));
        return new FuzzHarnessFailureException(message, innerException);
    }

    private static int ParsePositiveIntEnvironment(string variableName, int fallback)
    {
        string? value = Environment.GetEnvironmentVariable(variableName);
        return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int parsed) && parsed > 0
            ? parsed
            : fallback;
    }

    private static ulong ParseULongEnvironment(string variableName)
    {
        string? value = Environment.GetEnvironmentVariable(variableName);
        return ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out ulong parsed) ? parsed : 0;
    }

    private static string? FindRepositoryRoot(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Opc.Classic.slnx"))
                && Directory.Exists(Path.Combine(directory.FullName, "tests")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private sealed class FuzzHarnessFailureException : InvalidOperationException
    {
        public FuzzHarnessFailureException()
        {
        }

        public FuzzHarnessFailureException(string? message) : base(message)
        {
        }

        public FuzzHarnessFailureException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
