// Copyright (c) 2026 marcschier. Licensed under the MIT License.
// Property-based tests (CsCheck) over invariants of Opc.Classic.Core types.
// Complements the example-based TUnit tests in tests/Opc.Classic.Core.Tests/.
//

using CsCheck;

namespace Opc.Classic.PropertyTests;

public sealed class FileTimeHelperProperties
{
    [Test]
    public Task ToFromFileTime_RoundTrips_ForAnyValidDateTimeOffset()
    {
        // Ticks from Epoch (1601-01-01) up to the upper bound where adding
        // Epoch.Ticks would still fit in DateTimeOffset.Ticks.
        var maxTicks = DateTimeOffset.MaxValue.Ticks - FileTimeHelper.Epoch.Ticks;
        Gen.Long[0, maxTicks].Sample(ticks =>
        {
            var dt = FileTimeHelper.FromFileTime(ticks);
            var back = FileTimeHelper.ToFileTime(dt);
            return back == ticks;
        });
        return Task.CompletedTask;
    }

    [Test]
    public Task LowHighWordSplit_RecombinesIdentically_ForAnyFileTime()
    {
        var maxTicks = DateTimeOffset.MaxValue.Ticks - FileTimeHelper.Epoch.Ticks;
        Gen.Long[0, maxTicks].Sample(ticks =>
        {
            var dt = FileTimeHelper.FromFileTime(ticks);
            var (low, high) = FileTimeHelper.ToFileTimeWords(dt);
            var rebuilt = FileTimeHelper.FromFileTime(low, high);
            return rebuilt == dt;
        });
        return Task.CompletedTask;
    }
}

public sealed class OpcQualityProperties
{
    [Test]
    public Task Compose_RoundTrips_AllSubFields_ForAnyValidInput()
    {
        Gen.Select(Gen.Int[0, 3], Gen.Int[0, 15], Gen.Int[0, 3], Gen.Byte)
           .Sample(t =>
           {
               var kind = (OpcQualityKind)t.Item1;
               var sub = t.Item2;
               var limit = (OpcQualityLimit)t.Item3;
               var vendor = t.Item4;
               var q = OpcQuality.Compose(kind, sub, limit, vendor);

               return q.Quality == kind
                   && q.Substatus == sub
                   && q.Limit == limit
                   && q.VendorExtension == vendor;
           });
        return Task.CompletedTask;
    }

    [Test]
    public Task WithSubstatus_PreservesOtherFields_ForAnyInput()
    {
        Gen.Select(Gen.Int[0, 3], Gen.Int[0, 15], Gen.Int[0, 15], Gen.Int[0, 3], Gen.Byte)
           .Sample(t =>
           {
               var kind = (OpcQualityKind)t.Item1;
               var origSub = t.Item2;
               var newSub = t.Item3;
               var limit = (OpcQualityLimit)t.Item4;
               var vendor = t.Item5;
               var q = OpcQuality.Compose(kind, origSub, limit, vendor).WithSubstatus(newSub);

               return q.Quality == kind
                   && q.Substatus == newSub
                   && q.Limit == limit
                   && q.VendorExtension == vendor;
           });
        return Task.CompletedTask;
    }
}

public sealed class OpcUrlProperties
{
    private static readonly string[] Schemes = { "opcda", "opcae", "opchda", "opcdx", "opc.xml-da" };
    private static readonly OpcUrlScheme[] SchemeEnums =
    {
        OpcUrlScheme.Da, OpcUrlScheme.Ae, OpcUrlScheme.Hda, OpcUrlScheme.Dx, OpcUrlScheme.XmlDa,
    };

    // Letters-only char generator (a..z + A..Z).
    private static readonly Gen<char> LetterGen =
        Gen.OneOf(Gen.Char['a', 'z'], Gen.Char['A', 'Z']);

    [Test]
    public Task Parse_AndToString_RoundTrip_AcrossAllSchemes()
    {
        Gen.Select(
            Gen.Int[0, Schemes.Length - 1],
            LetterGen.Array[1, 16],
            LetterGen.Array[1, 32])
           .Sample(t =>
           {
               var schemeIdx = t.Item1;
               var host = new string(t.Item2);
               var progId = new string(t.Item3);
               var url = $"{Schemes[schemeIdx]}://{host}/{progId}";
               var parsed = OpcUrl.Parse(url);
               return parsed.Scheme == SchemeEnums[schemeIdx]
                   && string.Equals(parsed.Host, host, StringComparison.Ordinal)
                   && string.Equals(parsed.ServerId, progId, StringComparison.Ordinal)
                   && parsed.ToString() == url;
           });
        return Task.CompletedTask;
    }

    [Test]
    public Task Parse_WithPort_ExtractsPortAcrossAllSchemes()
    {
        Gen.Select(
            Gen.Int[0, Schemes.Length - 1],
            LetterGen.Array[1, 16],
            Gen.Int[1, 65535],
            LetterGen.Array[1, 32])
           .Sample(t =>
           {
               var schemeIdx = t.Item1;
               var host = new string(t.Item2);
               var port = t.Item3;
               var progId = new string(t.Item4);
               var url = $"{Schemes[schemeIdx]}://{host}:{port}/{progId}";
               var parsed = OpcUrl.Parse(url);
               return parsed.Port == port
                   && string.Equals(parsed.Host, host, StringComparison.Ordinal);
           });
        return Task.CompletedTask;
    }
}

public sealed class CryptoProperties
{
    [Test]
    public Task Md4_AlwaysProduces16Bytes_ForAnyInput()
    {
        Gen.Byte.Array[0, 1024].Sample(data =>
        {
            var hash = Opc.Classic.Dcom.Crypto.Md4.HashData(data);
            return hash.Length == 16;
        });
        return Task.CompletedTask;
    }

    [Test]
    public Task Md4_Deterministic_SameInputAlwaysSameOutput()
    {
        Gen.Byte.Array[0, 256].Sample(data =>
        {
            var h1 = Opc.Classic.Dcom.Crypto.Md4.HashData(data);
            var h2 = Opc.Classic.Dcom.Crypto.Md4.HashData(data);
            return Convert.ToHexString(h1) == Convert.ToHexString(h2);
        });
        return Task.CompletedTask;
    }

    [Test]
    public Task Rc4_IsSelfInverse_ForAnyKeyAndData()
    {
        Gen.Select(Gen.Byte.Array[1, 64], Gen.Byte.Array[0, 256])
           .Sample(t =>
           {
               var key = t.Item1;
               var plaintext = t.Item2;
               var ciphertext = new Opc.Classic.Dcom.Crypto.Rc4(key).Process(plaintext);
               var decrypted = new Opc.Classic.Dcom.Crypto.Rc4(key).Process(ciphertext);
               return Convert.ToHexString(decrypted) == Convert.ToHexString(plaintext);
           });
        return Task.CompletedTask;
    }

    [Test]
    public Task Rc4_OutputLength_EqualsInputLength()
    {
        Gen.Select(Gen.Byte.Array[1, 32], Gen.Byte.Array[0, 256])
           .Sample(t =>
           {
               var key = t.Item1;
               var data = t.Item2;
               var output = new Opc.Classic.Dcom.Crypto.Rc4(key).Process(data);
               return output.Length == data.Length;
           });
        return Task.CompletedTask;
    }
}

public sealed class OpcResultIdProperties
{
    [Test]
    public Task SeverityBit_DerivedConsistently_FromAnyCode()
    {
        Gen.Int.Sample(code =>
        {
            var r = new OpcResultId(code, null);
            var hasSeverityBit = (code & unchecked((int)0x80000000)) != 0;
            return r.IsFailure == hasSeverityBit
                && r.IsSuccess == !hasSeverityBit;
        });
        return Task.CompletedTask;
    }

    [Test]
    public Task Facility_ExtractedAccordingToHResultEncoding()
    {
        Gen.Int.Sample(code =>
        {
            var r = new OpcResultId(code, null);
            var expectedFacility = (code >> 16) & 0x07FF;
            return r.Facility == expectedFacility;
        });
        return Task.CompletedTask;
    }
}
