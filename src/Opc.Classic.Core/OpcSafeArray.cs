//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic;

/// <summary>
/// Managed projection of the COM/OAUT <c>SAFEARRAY</c> structure: a
/// multi-dimensional typed array carrier with explicit element type
/// (<see cref="VarType"/>), dimension counts, and lower bounds.
/// </summary>
/// <remarks>
/// The .NET-side representation stores the element data as a one-
/// dimensional typed array (in row-major order for multi-dimensional
/// SAFEARRAYs); the dimension metadata in <see cref="Lengths"/> and
/// <see cref="LowerBounds"/> recovers the logical shape.
/// <para>
/// SAFEARRAY is the OPC mechanism for transferring uniform arrays
/// (e.g., an OPCITEMVQT array of timestamps or values), as well as
/// the VT_ARRAY|VT_VARIANT case where each element is itself a
/// heterogeneous VARIANT.
/// </para>
/// </remarks>
public sealed record OpcSafeArray {
    /// <summary>Constructs a SAFEARRAY descriptor.</summary>
    /// <param name="elementType">The element VARTYPE.</param>
    /// <param name="data">The element data as a 1-D array. For multi-dimensional logical shapes, callers row-major-pack into a 1-D array sized as the product of <paramref name="lengths"/>.</param>
    /// <param name="lengths">Per-dimension element counts. <see langword="null"/> defaults to a single dimension of <c>data.Length</c>.</param>
    /// <param name="lowerBounds">Per-dimension lower bounds. <see langword="null"/> defaults to zero per dimension.</param>
    /// <param name="features">SAFEARRAY FADF_* descriptor flags to preserve on the wire.</param>
    public OpcSafeArray(
        VarType elementType,
        Array data,
        int[]? lengths = null,
        int[]? lowerBounds = null,
        SafeArrayFeatures features = SafeArrayFeatures.HaveVartype) {
        ArgumentNullException.ThrowIfNull(data);

        ElementType = elementType;
        Data = data;
        Features = features;
        _lengths = lengths is null
            ? new[] { data.Length }
            : (int[])lengths.Clone();
        _lowerBounds = lowerBounds is null
            ? new int[_lengths.Length]
            : (int[])lowerBounds.Clone();

        if (_lengths.Length == 0) {
            throw new ArgumentException("SAFEARRAY rank must be at least 1.", nameof(lengths));
        }
        if (_lengths.Length > 256) {
            throw new ArgumentOutOfRangeException(nameof(lengths), _lengths.Length, "SAFEARRAY rank must not exceed 256.");
        }
        if (VarTypeMask.IsArray(elementType) || VarTypeMask.IsByRef(elementType)) {
            throw new ArgumentException("SAFEARRAY element type must not include VT_ARRAY or VT_BYREF modifiers.", nameof(elementType));
        }

        ValidateFeatures(elementType, features);

        if (_lengths.Length != _lowerBounds.Length) {
            throw new ArgumentException(
                $"Lengths.Length ({_lengths.Length}) must equal LowerBounds.Length ({_lowerBounds.Length}).",
                nameof(lowerBounds));
        }

        long product = 1;
        for (int i = 0; i < _lengths.Length; i++) {
            if (_lengths[i] < 0) {
                throw new ArgumentOutOfRangeException(nameof(lengths),
                    $"Dimension {i} has negative length {_lengths[i]}.");
            }
            product *= _lengths[i];
        }

        if (product != data.Length) {
            throw new ArgumentException(
                $"Product of Lengths ({product}) must equal data.Length ({data.Length}).",
                nameof(data));
        }
    }

    private readonly int[] _lengths;
    private readonly int[] _lowerBounds;

    /// <summary>The element <see cref="VarType"/> (without VT_ARRAY).</summary>
    public VarType ElementType { get; }

    /// <summary>Per-dimension element counts. The number of dimensions equals <c>Lengths.Length</c>.</summary>
    public ReadOnlySpan<int> Lengths => _lengths;

    /// <summary>Per-dimension lower bounds (typically all zero).</summary>
    public ReadOnlySpan<int> LowerBounds => _lowerBounds;

    /// <summary>SAFEARRAY FADF_* descriptor flags preserved on encode/decode.</summary>
    public SafeArrayFeatures Features { get; }

    /// <summary>The element data as a 1-D array (row-major for multi-dimensional logical shapes).</summary>
    public Array Data { get; }

    /// <summary>Number of dimensions.</summary>
    public int Rank => _lengths.Length;

    /// <summary>Total element count (product of <see cref="Lengths"/>).</summary>
    public int TotalElements => Data.Length;

    // ---- Factory methods for the common 1-D scalar arrays ----

    /// <summary>Creates a 1-D VT_I2 SAFEARRAY from a managed short[].</summary>
    public static OpcSafeArray OfInt16(short[] values) => new(VarType.VT_I2, values);

    /// <summary>Creates a 1-D VT_I4 SAFEARRAY from a managed int[].</summary>
    public static OpcSafeArray OfInt32(int[] values) => new(VarType.VT_I4, values);

    /// <summary>Creates a 1-D VT_I8 SAFEARRAY from a managed long[].</summary>
    public static OpcSafeArray OfInt64(long[] values) => new(VarType.VT_I8, values);

    /// <summary>Creates a 1-D VT_UI1 SAFEARRAY from a managed byte[].</summary>
    public static OpcSafeArray OfUInt8(byte[] values) => new(VarType.VT_UI1, values);

    /// <summary>Creates a 1-D VT_R4 SAFEARRAY from a managed float[].</summary>
    public static OpcSafeArray OfSingle(float[] values) => new(VarType.VT_R4, values);

    /// <summary>Creates a 1-D VT_R8 SAFEARRAY from a managed double[].</summary>
    public static OpcSafeArray OfDouble(double[] values) => new(VarType.VT_R8, values);

    /// <summary>Creates a 1-D VT_BSTR SAFEARRAY from a managed string[].</summary>
    public static OpcSafeArray OfString(string[] values) => new(VarType.VT_BSTR, values);

    /// <summary>Creates a 1-D VT_BOOL SAFEARRAY from a managed bool[].</summary>
    public static OpcSafeArray OfBoolean(bool[] values) => new(VarType.VT_BOOL, values);

    /// <summary>Creates a 1-D VT_VARIANT SAFEARRAY from managed variants.</summary>
    public static OpcSafeArray OfVariant(OpcVariant[] values) =>
        new(VarType.VT_VARIANT, values, features: SafeArrayFeatures.HaveVartype | SafeArrayFeatures.Variant);

    private static void ValidateFeatures(VarType elementType, SafeArrayFeatures features) {
        if ((features & SafeArrayFeatures.Variant) != SafeArrayFeatures.None && elementType != VarType.VT_VARIANT) {
            throw new ArgumentException("FADF_VARIANT requires VT_VARIANT elements.", nameof(features));
        }
        if ((features & SafeArrayFeatures.Bstr) != SafeArrayFeatures.None && elementType != VarType.VT_BSTR) {
            throw new ArgumentException("FADF_BSTR requires VT_BSTR elements.", nameof(features));
        }
        if ((features & SafeArrayFeatures.Record) != SafeArrayFeatures.None && elementType != VarType.VT_RECORD) {
            throw new ArgumentException("FADF_RECORD requires VT_RECORD elements.", nameof(features));
        }
        if ((features & SafeArrayFeatures.Unknown) != SafeArrayFeatures.None && elementType != VarType.VT_UNKNOWN) {
            throw new ArgumentException("FADF_UNKNOWN requires VT_UNKNOWN elements.", nameof(features));
        }
        if ((features & SafeArrayFeatures.Dispatch) != SafeArrayFeatures.None && elementType != VarType.VT_DISPATCH) {
            throw new ArgumentException("FADF_DISPATCH requires VT_DISPATCH elements.", nameof(features));
        }
        if ((features & SafeArrayFeatures.HaveIID) != SafeArrayFeatures.None && elementType != VarType.VT_UNKNOWN && elementType != VarType.VT_DISPATCH) {
            throw new ArgumentException("FADF_HAVEIID requires VT_UNKNOWN or VT_DISPATCH elements.", nameof(features));
        }
    }

    /// <inheritdoc />
    public bool Equals(OpcSafeArray? other) {
        if (other is null) {
            return false;
        }
        if (ReferenceEquals(this, other)) {
            return true;
        }
        if (ElementType != other.ElementType || Features != other.Features) {
            return false;
        }
        if (_lengths.Length != other._lengths.Length) {
            return false;
        }
        for (int i = 0; i < _lengths.Length; i++) {
            if (_lengths[i] != other._lengths[i]) {
                return false;
            }
            if (_lowerBounds[i] != other._lowerBounds[i]) {
                return false;
            }
        }
        if (Data.Length != other.Data.Length) {
            return false;
        }
        for (int i = 0; i < Data.Length; i++) {
            object? a = Data.GetValue(i);
            object? b = other.Data.GetValue(i);
            if (!Equals(a, b)) {
                return false;
            }
        }
        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode() {
        var hc = new HashCode();
        hc.Add(ElementType);
        hc.Add(Features);
        hc.Add(Data.Length);
        for (int i = 0; i < _lengths.Length; i++) {
            hc.Add(_lengths[i]);
            hc.Add(_lowerBounds[i]);
        }
        return hc.ToHashCode();
    }
}
