// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using CsCheck;
using Opc.Classic.Tests.Fuzz;

namespace Opc.Classic.PropertyTests.Fuzz;

public sealed class FuzzHarnessSelfTests
{
    [Test]
    public async Task BytesEdgeWeighted_SampledFiftyTimes_HitsAllSizeBuckets()
    {
        var buckets = new bool[3];

        FuzzHarness.BytesEdgeWeighted.Sample(bytes =>
        {
            buckets[BucketIndex(bytes.Length)] = true;
        }, iter: 50, threads: 1);

        await Assert.That(buckets.All(static hit => hit)).IsEqualTo(true);
    }

    [Test]
    public async Task AssertParseDoesNotCrash_AllowedInvalidDataException_Passes()
    {
        bool completed = false;

        FuzzHarness.AssertParseDoesNotCrash(
            new byte[] { 0x01, 0x02 },
            static int (ReadOnlyMemory<byte> _) => throw new InvalidDataException("documented rejection"),
            [typeof(InvalidDataException)]);

        completed = true;

        await Assert.That(completed).IsEqualTo(true);
    }

    [Test]
    public async Task AssertParseDoesNotCrash_NullReferenceException_FailsWithHexDump()
    {
        var input = new byte[] { 0xde, 0xad, 0xbe, 0xef };

        try
        {
            FuzzHarness.AssertParseDoesNotCrash(
                input,
                static int (ReadOnlyMemory<byte> _) => throw new NullReferenceException("boom"),
                [typeof(InvalidDataException)]);
            throw new InvalidOperationException("Harness did not fail.");
        }
        catch (InvalidOperationException ex)
        {
            await Assert.That(ex.Message).Contains("Unexpected parser exception");
            await Assert.That(ex.Message).Contains("00000000  de ad be ef");
        }
    }

    [Test]
    public async Task AssertParseDoesNotCrash_ParserExceedsTimeout_FailsWithTimeoutMessage()
    {
        try
        {
            FuzzHarness.AssertParseDoesNotCrash(
                new byte[] { 0x01 },
                static input =>
                {
                    Thread.Sleep(25);
                    return input.Length;
                },
                [],
                timeoutMs: 1);
            throw new InvalidOperationException("Harness did not fail.");
        }
        catch (InvalidOperationException ex)
        {
            await Assert.That(ex.Message).Contains("exceeded timeout");
        }
    }

    [Test]
    public async Task LoadCorpus_SyntheticCorpusDirectory_YieldsArgumentsRows()
    {
        string surface = string.Concat("_selftest_", Guid.NewGuid().ToString("N"));
        string directory = Path.Combine(FindRepositoryRoot(), "tests", "_Fixtures", "Fuzz", surface);

        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(Path.Combine(directory, "one.bin"), [0x01]);
            File.WriteAllBytes(Path.Combine(directory, "two.bin"), [0x02, 0x03]);
            File.WriteAllText(Path.Combine(directory, "ignored.txt"), "not corpus");

            object[][] rows = FuzzHarness.LoadCorpus(surface).ToArray();

            await Assert.That(rows.Length).IsEqualTo(2);
            await Assert.That(rows.All(static row => row.Length == 1 && row[0] is byte[])).IsEqualTo(true);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    private static int BucketIndex(int length) => length switch
    {
        <= 16 => 0,
        <= 1_024 => 1,
        _ => 2,
    };

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Opc.Classic.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root.");
    }
}
