// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Tests;

public sealed class OpcRecordValueRegistryTests
{
    [Test]
    public async Task Constructor_WithGuid_CopiesValuesAndPreservesNullEntries()
    {
        var id = new Guid("2F65BD9F-E371-4B16-B37B-89D61F0C3F38");
        object?[] values = [42, "Pump", null];

        var record = new OpcRecordValue(id, values);
        values[0] = 100;
        values[1] = "Changed";

        await Assert.That(record.RecordInfoId).IsEqualTo(id);
        await Assert.That(record.Values.Count).IsEqualTo(3);
        await Assert.That((int?)record.Values[0]).IsEqualTo(42);
        await Assert.That((string?)record.Values[1]).IsEqualTo("Pump");
        await Assert.That(record.Values[2]).IsNull();
    }

    [Test]
    public async Task Constructor_WithInvalidArguments_ThrowsSpecificExceptions()
    {
        var id = new Guid("6D38D9EF-73E9-4069-8DE6-D9DDDFD6929E");

        await Assert.That(() => new OpcRecordValue(Guid.Empty, Array.Empty<object?>()))
            .Throws<ArgumentException>();
        await Assert.That(() => new OpcRecordValue(id, null!))
            .Throws<ArgumentNullException>();
        await Assert.That(() => new OpcRecordValue(null!, Array.Empty<object?>()))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_WithRecordInfo_RequiresValueCountToMatchFields()
    {
        var info = CreateRecordInfo(new Guid("98BA8BB8-A6F2-444E-A9B1-A26788E49455"), "TwoFieldRecord");

        var record = new OpcRecordValue(info, new object?[] { 7, "seven" });

        await Assert.That(record.RecordInfoId).IsEqualTo(info.Id);
        await Assert.That(record.Values.Count).IsEqualTo(2);
        await Assert.That(() => new OpcRecordValue(info, new object?[] { 7 }))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task Equality_ComparesRecordIdAndEachValue()
    {
        var id = new Guid("56AB7DEC-253D-4CD0-81EC-71CF584B4B66");
        var same = new OpcRecordValue(id, new object?[] { 1, "A" });
        var equal = new OpcRecordValue(id, new object?[] { 1, "A" });
        var differentId = new OpcRecordValue(new Guid("A14D9394-C05D-4E0D-9BEF-7D9EACD8BB00"), new object?[] { 1, "A" });
        var differentLength = new OpcRecordValue(id, new object?[] { 1, "A", 3 });
        var differentValue = new OpcRecordValue(id, new object?[] { 1, "B" });

        await Assert.That(same.Equals(same)).IsTrue();
        await Assert.That(same.Equals(equal)).IsTrue();
        await Assert.That(object.Equals(same, equal)).IsTrue();
        await Assert.That(same.GetHashCode()).IsEqualTo(equal.GetHashCode());
        await Assert.That(same.Equals(differentId)).IsFalse();
        await Assert.That(same.Equals(differentLength)).IsFalse();
        await Assert.That(same.Equals(differentValue)).IsFalse();
        await Assert.That(same.Equals(null)).IsFalse();
        object nonRecord = "not a record";
        await Assert.That(object.Equals(same, nonRecord)).IsFalse();
    }

    [Test]
    public async Task RecordInfoRegistry_RegisterTryGetGetAndUnregister_RoundTrip()
    {
        var id = new Guid("44BB7021-4A72-49E5-B098-1A4D62E5E66B");
        var first = CreateRecordInfo(id, "FirstLayout");
        var replacement = CreateRecordInfo(id, "ReplacementLayout");

        RecordInfoRegistry.Register(first);
        try
        {
            bool foundFirst = RecordInfoRegistry.TryGet(id, out IRecordInfo? registeredFirst);
            await Assert.That(foundFirst).IsTrue();
            await Assert.That(registeredFirst).IsSameReferenceAs(first);

            RecordInfoRegistry.Register(replacement);

            IRecordInfo registeredReplacement = RecordInfoRegistry.Get(id);
            await Assert.That(registeredReplacement).IsSameReferenceAs(replacement);
            await Assert.That(registeredReplacement.Name).IsEqualTo("ReplacementLayout");
        }
        finally
        {
            _ = RecordInfoRegistry.Unregister(id);
        }

        bool foundAfterUnregister = RecordInfoRegistry.TryGet(id, out IRecordInfo? missingAfterUnregister);
        await Assert.That(foundAfterUnregister).IsFalse();
        await Assert.That(missingAfterUnregister).IsNull();
        await Assert.That(RecordInfoRegistry.Unregister(id)).IsFalse();
    }

    [Test]
    public async Task RecordInfoRegistry_InvalidOrMissingRecords_ThrowExpectedExceptions()
    {
        var missingId = new Guid("33997F1C-514D-4AA9-8B1E-CC0374106970");
        var emptyIdInfo = new TestRecordInfo(Guid.Empty);

        await Assert.That(() => RecordInfoRegistry.Register(null!))
            .Throws<ArgumentNullException>();
        await Assert.That(() => RecordInfoRegistry.Register(emptyIdInfo))
            .Throws<ArgumentException>();

        var exception = await Assert.That(() => RecordInfoRegistry.Get(missingId))
            .Throws<KeyNotFoundException>();
        await Assert.That(exception!.Message).Contains(missingId.ToString());
    }

    private static OpcRecordInfo CreateRecordInfo(Guid id, string name)
        => new(
            id,
            name,
            new[]
            {
                new OpcRecordField("Id", VarType.VT_I4),
                new OpcRecordField("Name", VarType.VT_BSTR),
            });

    private sealed class TestRecordInfo(Guid id) : IRecordInfo
    {
        public Guid Id { get; } = id;
        public string Name => "Invalid";
        public IReadOnlyList<OpcRecordField> Fields => Array.Empty<OpcRecordField>();
    }
}
