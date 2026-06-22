// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Da;

/// <summary>
/// Identifies an OPC DA item on a server.
/// </summary>
/// <remarks>
/// Reference type with value equality on <see cref="ItemName"/> + <see cref="Path"/>.
/// The base shape consumed by all DA APIs that reference an item without
/// carrying value/quality data.
/// </remarks>
public class ItemIdentifier : IEquatable<ItemIdentifier>
{
    /// <summary>
    /// Construct an identifier.
    /// </summary>
    /// <param name="itemName">
    /// Fully-qualified item name as the server understands it (e.g.
    /// <c>"Random.Int1"</c> or <c>"PLC1.MotorSpeed"</c>).
    /// </param>
    /// <param name="path">
    /// Optional access path. Use for servers that distinguish multiple
    /// access routes to the same item; <see langword="null"/> otherwise.
    /// </param>
    public ItemIdentifier(string itemName, string? path = null)
    {
        ArgumentNullException.ThrowIfNull(itemName);
        ItemName = itemName;
        Path = path;
    }

    /// <summary>
    /// Fully-qualified item name.
    /// </summary>
    public string ItemName { get; init; }

    /// <summary>
    /// Optional access path.
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// Value equality on <see cref="ItemName"/> + <see cref="Path"/>.
    /// </summary>
    public bool Equals(ItemIdentifier? other) =>
        other is not null &&
        string.Equals(ItemName, other.ItemName, StringComparison.Ordinal) &&
        string.Equals(Path, other.Path, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is ItemIdentifier other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(ItemName, Path);

    /// <summary>
    /// Value-equality operator.
    /// </summary>
    public static bool operator ==(ItemIdentifier? left, ItemIdentifier? right) =>
        Equals(left, right);

    /// <summary>
    /// Value-inequality operator.
    /// </summary>
    public static bool operator !=(ItemIdentifier? left, ItemIdentifier? right) =>
        !Equals(left, right);

    /// <inheritdoc />
    public override string ToString() =>
        Path is null ? ItemName : $"{Path}::{ItemName}";
}
