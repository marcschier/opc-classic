// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Globalization;
using System.Security.Cryptography;

namespace Opc.Classic.Samples.LoopbackDemo;

internal sealed class LoopbackTagStore
{
    private readonly Dictionary<string, LoopbackTag> _tags;

    public LoopbackTagStore()
    {
        _tags = new Dictionary<string, LoopbackTag>(StringComparer.Ordinal)
        {
            ["Random.Real4"] = LoopbackTag.ReadOnly("Random.Real4", VarType.VT_R4, ReadRandomSingle),
            ["Saw-toothed Waves.Real8"] = LoopbackTag.ReadOnly("Saw-toothed Waves.Real8", VarType.VT_R8, ReadSawtooth),
            ["Bucket Brigade.Int4"] = LoopbackTag.WritableTag("Bucket Brigade.Int4", VarType.VT_I4, OpcVariant.FromInt32(0)),
            ["Bucket Brigade.String"] = LoopbackTag.WritableTag("Bucket Brigade.String", VarType.VT_BSTR, OpcVariant.FromString(string.Empty)),
            ["Bucket Brigade.Boolean"] = LoopbackTag.WritableTag("Bucket Brigade.Boolean", VarType.VT_BOOL, OpcVariant.FromBoolean(false)),
        };
    }

    public IReadOnlyCollection<LoopbackTag> Tags => _tags.Values;

    public bool TryGet(string itemId, out LoopbackTag tag) => _tags.TryGetValue(itemId, out tag!);
    public string[] Browse() => _tags.Keys.Order(StringComparer.Ordinal).ToArray();

    private static OpcVariant ReadRandomSingle()
    {
        float value = RandomNumberGenerator.GetInt32(0, 1_000_000) / 1_000_000.0F;
        return OpcVariant.FromSingle(value);
    }

    private static OpcVariant ReadSawtooth()
    {
        const long periodMilliseconds = 10_000;
        long elapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % periodMilliseconds;
        double value = elapsed / (double)periodMilliseconds * 100.0D;
        return OpcVariant.FromDouble(value);
    }
}

internal sealed class LoopbackTag
{
    private readonly object _gate = new();
    private readonly Func<OpcVariant> _read;
    private readonly VarType _canonicalDataType;
    private OpcVariant _value;

    private LoopbackTag(
        string itemId,
        VarType canonicalDataType,
        Func<OpcVariant> read,
        bool writable,
        OpcVariant initialValue)
    {
        ItemId = itemId;
        _canonicalDataType = canonicalDataType;
        _read = read;
        Writable = writable;
        _value = initialValue;
    }

    public string ItemId { get; }
    public bool Writable { get; }
    public VarType CanonicalDataType => _canonicalDataType;
    public int AccessRights => Writable ? 0x3 : 0x1;

    public static LoopbackTag ReadOnly(string itemId, VarType canonicalDataType, Func<OpcVariant> read)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        ArgumentNullException.ThrowIfNull(read);
        return new LoopbackTag(itemId, canonicalDataType, read, writable: false, OpcVariant.Empty);
    }

    public static LoopbackTag WritableTag(string itemId, VarType canonicalDataType, OpcVariant initialValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        return new LoopbackTag(itemId, canonicalDataType, static () => OpcVariant.Empty, writable: true, initialValue);
    }

    public OpcVariant Read()
    {
        if (!Writable)
        {
            return _read();
        }

        lock (_gate)
        {
            return _value;
        }
    }

    public bool TryWrite(OpcVariant value)
    {
        if (!Writable || value.Type != _canonicalDataType)
        {
            return false;
        }

        lock (_gate)
        {
            _value = value;
        }

        return true;
    }

    public override string ToString() => string.Create(
        CultureInfo.InvariantCulture,
        $"{ItemId} ({CanonicalDataType}, rights=0x{AccessRights:X})");

    private OpcVariant ReadStoredValue()
    {
        lock (_gate)
        {
            return _value;
        }
    }
}
