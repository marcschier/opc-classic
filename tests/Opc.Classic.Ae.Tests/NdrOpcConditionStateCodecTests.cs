// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Ae.Ndr;
using Opc.Classic.Ndr;

namespace Opc.Classic.Ae.Tests;

public sealed class NdrOpcConditionStateCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 4096)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer[..writer.Position];
    }

    private static OpcConditionState ReadOne(byte[] bytes)
    {
        var reader = new NdrReader(bytes);
        return NdrOpcConditionStateCodec.Read(ref reader);
    }

    private static OpcConditionState MakeState(
        string? acknowledgerId = "operator1",
        string? comment = "Acknowledged during shift handoff",
        string?[]? subConditionNames = null,
        string?[]? subConditionDefinitions = null,
        uint[]? subConditionSeverities = null,
        string?[]? subConditionDescriptions = null,
        OpcVariant[]? eventAttributes = null,
        int[]? errors = null) =>
        new(
            state: 0x0007,
            activeSubCondition: "HighHigh",
            activeSubConditionDefinition: "Pressure above trip threshold",
            activeSubConditionSeverity: 900,
            activeSubConditionDescription: "Discharge pressure critically high",
            quality: new OpcQuality(0x00C0),
            lastAckTime: new DateTimeOffset(2026, 5, 22, 10, 31, 0, TimeSpan.Zero),
            subConditionLastActive: new DateTimeOffset(2026, 5, 22, 10, 25, 0, TimeSpan.Zero),
            conditionLastActive: new DateTimeOffset(2026, 5, 22, 10, 20, 0, TimeSpan.Zero),
            conditionLastInactive: new DateTimeOffset(2026, 5, 22, 9, 45, 0, TimeSpan.Zero),
            acknowledgerId: acknowledgerId,
            comment: comment,
            subConditionNames: subConditionNames ?? new[] { "High", "HighHigh" },
            subConditionDefinitions: subConditionDefinitions ?? new[] { "Above limit", "Above trip" },
            subConditionSeverities: subConditionSeverities ?? new[] { 700u, 900u },
            subConditionDescriptions: subConditionDescriptions ?? new[] { "Pressure high", "Pressure critically high" },
            eventAttributes: eventAttributes ?? new[]
            {
                OpcVariant.FromInt32(42),
                OpcVariant.FromString("pump-7"),
                OpcVariant.FromBoolean(true),
            },
            errors: errors ?? new[] { 0, unchecked((int)0x80004005u), 7 });

    [Test]
    public async Task RoundTrip_ZeroSubConditionsAndEventAttributes()
    {
        var input = new OpcConditionState(
            state: 0x0004,
            activeSubCondition: null,
            activeSubConditionDefinition: null,
            activeSubConditionSeverity: 0,
            activeSubConditionDescription: null,
            quality: new OpcQuality(0x00C0),
            lastAckTime: new DateTimeOffset(2026, 5, 22, 10, 0, 0, TimeSpan.Zero),
            subConditionLastActive: new DateTimeOffset(2026, 5, 22, 10, 1, 0, TimeSpan.Zero),
            conditionLastActive: new DateTimeOffset(2026, 5, 22, 10, 2, 0, TimeSpan.Zero),
            conditionLastInactive: new DateTimeOffset(2026, 5, 22, 10, 3, 0, TimeSpan.Zero),
            acknowledgerId: null,
            comment: null,
            subConditionNames: Array.Empty<string?>(),
            subConditionDefinitions: Array.Empty<string?>(),
            subConditionSeverities: Array.Empty<uint>(),
            subConditionDescriptions: Array.Empty<string?>(),
            eventAttributes: Array.Empty<OpcVariant>(),
            errors: Array.Empty<int>());

        var bytes = WriteOne((ref NdrWriter writer) => NdrOpcConditionStateCodec.Write(ref writer, input));
        var back = ReadOne(bytes);

        await Assert.That(back.State).IsEqualTo((ushort)0x0004);
        await Assert.That(back.Quality.RawValue).IsEqualTo((ushort)0x00C0);
        await Assert.That(back.SubConditionCount).IsEqualTo(0);
        await Assert.That(back.EventAttributeCount).IsEqualTo(0);
        await Assert.That(back.LastAckTime.UtcDateTime).IsEqualTo(input.LastAckTime.UtcDateTime);
    }

    [Test]
    public async Task RoundTrip_TwoSubConditionsAndThreeEventAttributes()
    {
        var input = MakeState();
        var bytes = WriteOne((ref NdrWriter writer) => NdrOpcConditionStateCodec.Write(ref writer, input));
        var back = ReadOne(bytes);

        await Assert.That(back.State).IsEqualTo((ushort)0x0007);
        await Assert.That(back.ActiveSubCondition).IsEqualTo("HighHigh");
        await Assert.That(back.ActiveSubConditionDefinition).IsEqualTo("Pressure above trip threshold");
        await Assert.That(back.ActiveSubConditionSeverity).IsEqualTo(900u);
        await Assert.That(back.ActiveSubConditionDescription).IsEqualTo("Discharge pressure critically high");
        await Assert.That(back.SubConditionNames.Length).IsEqualTo(2);
        await Assert.That(back.SubConditionNames[0]).IsEqualTo("High");
        await Assert.That(back.SubConditionDefinitions[1]).IsEqualTo("Above trip");
        await Assert.That(back.SubConditionSeverities[1]).IsEqualTo(900u);
        await Assert.That(back.SubConditionDescriptions[0]).IsEqualTo("Pressure high");
        await Assert.That(back.EventAttributes.Length).IsEqualTo(3);
        await Assert.That(back.EventAttributes[0].AsInt32()).IsEqualTo(42);
        await Assert.That(back.EventAttributes[1].AsString()).IsEqualTo("pump-7");
        await Assert.That(back.EventAttributes[2].AsBoolean()).IsEqualTo(true);
        await Assert.That(back.Errors[1]).IsEqualTo(unchecked((int)0x80004005u));
    }

    [Test]
    public async Task RoundTrip_NullAcknowledgerIdAndComment()
    {
        var input = MakeState(acknowledgerId: null, comment: null);
        var bytes = WriteOne((ref NdrWriter writer) => NdrOpcConditionStateCodec.Write(ref writer, input));
        var back = ReadOne(bytes);

        await Assert.That(back.AcknowledgerId).IsNull();
        await Assert.That(back.Comment).IsNull();
        await Assert.That(back.ActiveSubCondition).IsEqualTo(input.ActiveSubCondition);
        await Assert.That(back.EventAttributes.Length).IsEqualTo(3);
    }

    [Test]
    public async Task Constructor_MismatchedParallelArrayLengths_ThrowsArgumentException()
    {
        bool threw = false;
        try
        {
            _ = MakeState(subConditionDescriptions: new[] { "only one" });
        }
        catch (ArgumentException ex) when (ex.Message.Contains("subConditionDescriptions", StringComparison.Ordinal))
        {
            threw = true;
        }

        await Assert.That(threw).IsTrue();
    }
}
