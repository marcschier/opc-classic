// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dx.Tests;

public sealed class DxConfigurationStoreTests
{
    [Test]
    public async Task JsonStore_RoundTripsAndIncrementsVersion()
    {
        using var directory = new TestDirectory();
        using var store = new JsonFileDxConfigurationStore(directory.File("dx-config.json"));
        var configuration = CreateConfiguration();

        var saved = await store.SaveAsync(configuration, expectedVersion: 0);
        var loaded = await store.LoadAsync();

        await Assert.That(saved.Version).IsEqualTo(1);
        await Assert.That(loaded.Version).IsEqualTo(1);
        await Assert.That(loaded.Configuration.SourceServers[0])
            .IsEqualTo(configuration.SourceServers[0]);
        await Assert.That(loaded.Configuration.Connections[0].Name)
            .IsEqualTo(configuration.Connections[0].Name);
        await Assert.That(loaded.Configuration.Connections[0].BrowsePaths)
            .IsEquivalentTo(configuration.Connections[0].BrowsePaths);
        await Assert.That(loaded.Configuration.Connections[0].DefaultOverrideValue)
            .IsEqualTo(configuration.Connections[0].DefaultOverrideValue);
        await Assert.That(loaded.Configuration.Connections[0].SubstituteValue)
            .IsEqualTo(configuration.Connections[0].SubstituteValue);
    }

    [Test]
    public async Task JsonStore_CorruptJson_ThrowsTypedError()
    {
        using var directory = new TestDirectory();
        var path = directory.File("dx-config.json");
        await File.WriteAllTextAsync(path, "{ definitely-not-json");
        using var store = new JsonFileDxConfigurationStore(path);

        await Assert.That(async () => await store.LoadAsync())
            .Throws<DxConfigurationCorruptException>();
    }

    [Test]
    public async Task JsonStore_UnknownFormatVersion_ThrowsTypedError()
    {
        using var directory = new TestDirectory();
        var path = directory.File("dx-config.json");
        await File.WriteAllTextAsync(
            path,
            """{"formatVersion":99,"version":0,"configuration":{"sourceServers":[],"connections":[]}}""");
        using var store = new JsonFileDxConfigurationStore(path);

        await Assert.That(async () => await store.LoadAsync())
            .Throws<DxConfigurationFormatVersionException>();
    }

    [Test]
    public async Task Stores_StaleVersion_DoesNotReplaceCurrentConfiguration()
    {
        using var store = new InMemoryDxConfigurationStore();
        var first = await store.SaveAsync(CreateConfiguration(), expectedVersion: 0);

        await Assert.That(async () =>
                await store.SaveAsync(DxConfiguration.Empty, expectedVersion: 0))
            .Throws<DxConfigurationVersionException>();

        var loaded = await store.LoadAsync();
        await Assert.That(loaded.Version).IsEqualTo(first.Version);
        await Assert.That(loaded.Configuration.Connections[0].Name)
            .IsEqualTo(first.Configuration.Connections[0].Name);
    }

    [Test]
    public async Task JsonStore_StaleVersion_ThrowsTypedError()
    {
        using var directory = new TestDirectory();
        using var store = new JsonFileDxConfigurationStore(directory.File("dx-config.json"));
        await store.SaveAsync(CreateConfiguration(), expectedVersion: 0);

        var exception = await Assert.That(async () =>
                await store.SaveAsync(DxConfiguration.Empty, expectedVersion: 0))
            .Throws<DxConfigurationVersionException>();

        await Assert.That(exception!.ExpectedVersion).IsEqualTo(0);
        await Assert.That(exception.ActualVersion).IsEqualTo(1);
    }

    [Test]
    public async Task JsonStore_PreCanceledSave_DoesNotReplaceCurrentConfiguration()
    {
        using var directory = new TestDirectory();
        using var store = new JsonFileDxConfigurationStore(directory.File("dx-config.json"));
        var first = await store.SaveAsync(CreateConfiguration(), expectedVersion: 0);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.That(async () =>
                await store.SaveAsync(
                    DxConfiguration.Empty,
                    first.Version,
                    cancellation.Token))
            .Throws<OperationCanceledException>();

        var loaded = await store.LoadAsync();
        await Assert.That(loaded.Version).IsEqualTo(first.Version);
        await Assert.That(loaded.Configuration.Connections[0].Name)
            .IsEqualTo(first.Configuration.Connections[0].Name);
    }

    [Test]
    public async Task JsonStore_RoundTripsCompoundVariantsDeterministically()
    {
        using var directory = new TestDirectory();
        var record = new OpcRecordValue(
            new Guid("5B4FB8FC-7811-4050-BE04-897CDA1947EC"),
            new object?[]
            {
                42,
                "Pump",
                OpcVariant.FromSafeArray(new OpcSafeArray(
                    VarType.VT_R8,
                    new[] { double.NaN, double.PositiveInfinity },
                    lengths: new[] { 1, 2 },
                    lowerBounds: new[] { -1, 4 })),
                null,
            });
        var overrideValue = OpcVariant.FromSafeArray(OpcSafeArray.OfVariant(
        [
            OpcVariant.FromRecord(record),
            OpcVariant.FromDouble(double.NegativeInfinity),
            new OpcVariant(VarType.VT_BSTR, null),
        ]));
        var substituteValue = OpcVariant.FromRecord(record);
        var configuration = CreateConfiguration(
            overrideValue,
            substituteValue);
        var firstPath = directory.File("compound-1.json");
        var secondPath = directory.File("compound-2.json");
        using var firstStore = new JsonFileDxConfigurationStore(firstPath);
        using var secondStore = new JsonFileDxConfigurationStore(secondPath);

        await firstStore.SaveAsync(configuration, expectedVersion: 0);
        await secondStore.SaveAsync(configuration, expectedVersion: 0);
        var loaded = await firstStore.LoadAsync();

        await Assert.That(loaded.Configuration.Connections[0].DefaultOverrideValue)
            .IsEqualTo(overrideValue);
        await Assert.That(loaded.Configuration.Connections[0].SubstituteValue)
            .IsEqualTo(substituteValue);
        await Assert.That(await File.ReadAllTextAsync(firstPath))
            .IsEqualTo(await File.ReadAllTextAsync(secondPath));
    }

    [Test]
    public async Task JsonStore_RejectsNonPersistableByRefBeforeCommit()
    {
        using var directory = new TestDirectory();
        using var store = new JsonFileDxConfigurationStore(directory.File("byref.json"));
        var configuration = CreateConfiguration(
            OpcVariant.FromByRef(VarType.VT_I4, 42));

        await Assert.That(async () => await store.SaveAsync(configuration, expectedVersion: 0))
            .Throws<System.Text.Json.JsonException>();

        var loaded = await store.LoadAsync();
        await Assert.That(loaded.Version).IsEqualTo(0);
        await Assert.That(loaded.Configuration.Connections).IsEmpty();
    }

    [Test]
    public async Task JsonStore_RejectsArrayByRefAndVectorModifiersWithoutLosingFlags()
    {
        using var directory = new TestDirectory();
        var safeArray = OpcSafeArray.OfInt32([1, 2]);
        var valid = OpcVariant.FromSafeArray(safeArray);
        VarType[] modifiers = [VarType.VT_BYREF, VarType.VT_VECTOR];

        foreach (var modifier in modifiers)
        {
            var modifiedType = (VarType)((ushort)valid.Type | (ushort)modifier);
            using (var writeStore = new JsonFileDxConfigurationStore(
                directory.File($"write-{(ushort)modifier}.json")))
            {
                var configuration = CreateConfiguration(
                    new OpcVariant(modifiedType, safeArray));
                await Assert.That(async () =>
                        await writeStore.SaveAsync(configuration, expectedVersion: 0))
                    .Throws<System.Text.Json.JsonException>();
                await Assert.That((await writeStore.LoadAsync()).Version).IsEqualTo(0);
            }

            var readPath = directory.File($"read-{(ushort)modifier}.json");
            using var readStore = new JsonFileDxConfigurationStore(readPath);
            await readStore.SaveAsync(CreateConfiguration(valid), expectedVersion: 0);
            var json = await File.ReadAllTextAsync(readPath);
            var originalType = $"\"type\": {(ushort)valid.Type}";
            var replacementType = $"\"type\": {(ushort)modifiedType}";
            await File.WriteAllTextAsync(
                readPath,
                json.Replace(
                    originalType,
                    replacementType,
                    StringComparison.Ordinal));

            await Assert.That(async () => await readStore.LoadAsync())
                .Throws<DxConfigurationCorruptException>();
        }
    }

    [Test]
    public async Task InMemoryStore_DeepClonesNestedVariantDataAtEveryBoundary()
    {
        var arrayData = new[] { 1, 2, 3 };
        var nestedData = new[] { 4.5, 6.75 };
        var record = new OpcRecordValue(
            new Guid("45D6428C-9FD0-43D8-9EF3-A3BA8BD0C599"),
            new object?[]
            {
                OpcVariant.FromSafeArray(OpcSafeArray.OfDouble(nestedData)),
            });
        var configuration = CreateConfiguration(
            OpcVariant.FromSafeArray(OpcSafeArray.OfInt32(arrayData)),
            OpcVariant.FromRecord(record));
        using var store = new InMemoryDxConfigurationStore(configuration);

        arrayData[0] = 99;
        nestedData[0] = 99;
        var first = await store.LoadAsync();
        var firstArray = (int[])first.Configuration.Connections[0]
            .DefaultOverrideValue!.Value.AsSafeArray()!.Data;
        var firstRecord = first.Configuration.Connections[0]
            .SubstituteValue!.Value.AsRecord()!;
        var firstNested = (double[])((OpcVariant)firstRecord.Values[0]!)
            .AsSafeArray()!.Data;

        await Assert.That(firstArray[0]).IsEqualTo(1);
        await Assert.That(firstNested[0]).IsEqualTo(4.5);

        firstArray[0] = 77;
        firstNested[0] = 77;
        first.Configuration.Connections[0].BrowsePaths[0] = "mutated";
        var second = await store.LoadAsync();
        var secondArray = (int[])second.Configuration.Connections[0]
            .DefaultOverrideValue!.Value.AsSafeArray()!.Data;
        var secondRecord = second.Configuration.Connections[0]
            .SubstituteValue!.Value.AsRecord()!;
        var secondNested = (double[])((OpcVariant)secondRecord.Values[0]!)
            .AsSafeArray()!.Data;

        await Assert.That(secondArray[0]).IsEqualTo(1);
        await Assert.That(secondNested[0]).IsEqualTo(4.5);
        await Assert.That(second.Configuration.Connections[0].BrowsePaths[0])
            .IsEqualTo("Area1");
    }

    private static DxConfiguration CreateConfiguration(
        OpcVariant? defaultOverrideValue = null,
        OpcVariant? substituteValue = null) =>
        new(
            new[]
            {
                new DxSourceServer(
                    "PLC1",
                    "opcda://plc1/Vendor.Server",
                    defaultConnected: true),
            },
            new[]
            {
                new DxConnection(
                    "TankLevel",
                    browsePaths: new[] { "Area1", "Area1/Tank1" },
                    defaultOverrideValue:
                        defaultOverrideValue ?? OpcVariant.FromDouble(12.5),
                    substituteValue:
                        substituteValue ?? OpcVariant.FromString("offline"),
                    targetItemName: "HMI.Tank1.Level",
                    sourceServerName: "PLC1",
                    sourceItemName: "Tank1.Level",
                    updateRateMilliseconds: 250),
            });

    private sealed class TestDirectory : IDisposable
    {
        public TestDirectory()
        {
            Path = System.IO.Path.Combine(
                AppContext.BaseDirectory,
                "dx-store-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string File(string name) => System.IO.Path.Combine(Path, name);

        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
