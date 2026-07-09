// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic;

/// <summary>
/// Immutable <see cref="IRecordInfo"/> implementation for custom VT_RECORD layouts.
/// </summary>
public sealed class OpcRecordInfo : IRecordInfo
{
    private readonly IReadOnlyList<OpcRecordField> _fields;

    /// <summary>
    /// Creates a record layout descriptor.
    /// </summary>
    public OpcRecordInfo(Guid id, string name, IReadOnlyList<OpcRecordField> fields)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Record info id must not be empty.", nameof(id));
        }
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(fields);

        var copy = new OpcRecordField[fields.Count];
        for (int i = 0; i < fields.Count; i++)
        {
            copy[i] = fields[i];
        }

        Id = id;
        Name = name;
        _fields = Array.AsReadOnly(copy);
    }

    /// <inheritdoc />
    public Guid Id { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public IReadOnlyList<OpcRecordField> Fields => _fields;
}
