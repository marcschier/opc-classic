//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic;

/// <summary>A VT_RECORD value paired with the GUID of its registered layout.</summary>
public sealed class OpcRecordValue : IEquatable<OpcRecordValue>
{
    private readonly object?[] _values;
    private readonly IReadOnlyList<object?> _readOnlyValues;

    /// <summary>Creates a record value for the supplied layout id.</summary>
    public OpcRecordValue(Guid recordInfoId, IReadOnlyList<object?> values)
    {
        if (recordInfoId == Guid.Empty)
        {
            throw new ArgumentException("Record info id must not be empty.", nameof(recordInfoId));
        }
        ArgumentNullException.ThrowIfNull(values);

        RecordInfoId = recordInfoId;
        _values = new object?[values.Count];
        for (int i = 0; i < values.Count; i++)
        {
            _values[i] = values[i];
        }
        _readOnlyValues = Array.AsReadOnly(_values);
    }

    /// <summary>Creates a record value for the supplied registered layout.</summary>
    public OpcRecordValue(IRecordInfo recordInfo, IReadOnlyList<object?> values)
        : this((recordInfo ?? throw new ArgumentNullException(nameof(recordInfo))).Id, values)
    {
        if (values.Count != recordInfo.Fields.Count)
        {
            throw new ArgumentException(
                $"Record value count ({values.Count}) must match field count ({recordInfo.Fields.Count}).",
                nameof(values));
        }
    }

    /// <summary>The GUID identifying the record layout.</summary>
    public Guid RecordInfoId { get; }

    /// <summary>Field values in <see cref="IRecordInfo.Fields"/> order.</summary>
    public IReadOnlyList<object?> Values => _readOnlyValues;

    /// <inheritdoc />
    public bool Equals(OpcRecordValue? other)
    {
        if (other is null)
        {
            return false;
        }
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        if (RecordInfoId != other.RecordInfoId || _values.Length != other._values.Length)
        {
            return false;
        }
        for (int i = 0; i < _values.Length; i++)
        {
            if (!Equals(_values[i], other._values[i]))
            {
                return false;
            }
        }
        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is OpcRecordValue other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hc = new HashCode();
        hc.Add(RecordInfoId);
        for (int i = 0; i < _values.Length; i++)
        {
            hc.Add(_values[i]);
        }
        return hc.ToHashCode();
    }
}
