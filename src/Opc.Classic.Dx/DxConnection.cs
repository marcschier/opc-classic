// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dx;

/// <summary>
/// OPC DX's <c>OpcDxConnection</c> — a configured data flow from a
/// source-server item into a target item hosted by the DX server.
/// </summary>
/// <remarks>
/// The optional fields correspond to the native <c>dwMask</c> presence bits in
/// <c>OpcDxConnection</c>. NDR codecs will map those fields to/from the mask in
/// Forward-compat scaffold.
/// </remarks>
public sealed record DxConnection
{
    /// <summary>
    /// Constructs a DX connection definition or query mask.
    /// </summary>
    public DxConnection(
        string? name = null,
        string? description = null,
        string? itemPath = null,
        string? itemName = null,
        string? version = null,
        string[]? browsePaths = null,
        string? keyword = null,
        bool? defaultSourceItemConnected = null,
        bool? defaultTargetItemConnected = null,
        bool? defaultOverridden = null,
        OpcVariant? defaultOverrideValue = null,
        OpcVariant? substituteValue = null,
        bool? enableSubstituteValue = null,
        string? targetItemPath = null,
        string? targetItemName = null,
        string? sourceServerName = null,
        string? sourceItemPath = null,
        string? sourceItemName = null,
        int? sourceItemQueueSize = null,
        int? updateRateMilliseconds = null,
        float? deadbandPercent = null,
        string? vendorData = null,
        int mask = 0)
    {
        if (sourceItemQueueSize is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceItemQueueSize));
        }

        if (updateRateMilliseconds is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(updateRateMilliseconds));
        }

        if (deadbandPercent is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(deadbandPercent));
        }

        Name = name;
        Description = description;
        ItemPath = itemPath;
        ItemName = itemName;
        Version = version;
        BrowsePaths = NormalizeBrowsePaths(browsePaths);
        Keyword = keyword;
        DefaultSourceItemConnected = defaultSourceItemConnected;
        DefaultTargetItemConnected = defaultTargetItemConnected;
        DefaultOverridden = defaultOverridden;
        DefaultOverrideValue = defaultOverrideValue;
        SubstituteValue = substituteValue;
        EnableSubstituteValue = enableSubstituteValue;
        TargetItemPath = targetItemPath;
        TargetItemName = targetItemName;
        SourceServerName = sourceServerName;
        SourceItemPath = sourceItemPath;
        SourceItemName = sourceItemName;
        SourceItemQueueSize = sourceItemQueueSize;
        UpdateRateMilliseconds = updateRateMilliseconds;
        DeadbandPercent = deadbandPercent;
        VendorData = vendorData;
        Mask = mask == 0 ? ComputeMask(this) : mask;
    }

    /// <summary>
    /// Native <c>dwMask</c> presence bits.
    /// </summary>
    public int Mask { get; init; }

    /// <summary>
    /// Server-assigned DX connection name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Server-defined description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Configuration item path that identifies the connection object.
    /// </summary>
    public string? ItemPath { get; init; }

    /// <summary>
    /// Configuration item name that identifies the connection object.
    /// </summary>
    public string? ItemName { get; init; }

    /// <summary>
    /// Configuration version associated with the connection object.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Browse hierarchy paths that contain this connection.
    /// </summary>
    public string[] BrowsePaths { get; init; }

    /// <summary>
    /// Optional keyword used by servers for classification/search.
    /// </summary>
    public string? Keyword { get; init; }

    /// <summary>
    /// Default source-item connectivity setting, or null when unspecified.
    /// </summary>
    public bool? DefaultSourceItemConnected { get; init; }

    /// <summary>
    /// Default target-item connectivity setting, or null when unspecified.
    /// </summary>
    public bool? DefaultTargetItemConnected { get; init; }

    /// <summary>
    /// Default override setting, or null when unspecified.
    /// </summary>
    public bool? DefaultOverridden { get; init; }

    /// <summary>
    /// Value to use when a connection is overridden.
    /// </summary>
    public OpcVariant? DefaultOverrideValue { get; init; }

    /// <summary>
    /// Substitute value to publish when live source data is unavailable.
    /// </summary>
    public OpcVariant? SubstituteValue { get; init; }

    /// <summary>
    /// Whether <see cref="SubstituteValue"/> is enabled, or null when unspecified.
    /// </summary>
    public bool? EnableSubstituteValue { get; init; }

    /// <summary>
    /// Target-server item path where values are mirrored.
    /// </summary>
    public string? TargetItemPath { get; init; }

    /// <summary>
    /// Target-server item name where values are mirrored.
    /// </summary>
    public string? TargetItemName { get; init; }

    /// <summary>
    /// Name of the registered source server used by this connection.
    /// </summary>
    public string? SourceServerName { get; init; }

    /// <summary>
    /// Source-server item path read by this connection.
    /// </summary>
    public string? SourceItemPath { get; init; }

    /// <summary>
    /// Source-server item name read by this connection.
    /// </summary>
    public string? SourceItemName { get; init; }

    /// <summary>
    /// Source-item queue size, or null when unspecified.
    /// </summary>
    public int? SourceItemQueueSize { get; init; }

    /// <summary>
    /// Requested source update rate in milliseconds, or null when unspecified.
    /// </summary>
    public int? UpdateRateMilliseconds { get; init; }

    /// <summary>
    /// Deadband percentage applied to source updates, or null when unspecified.
    /// </summary>
    public float? DeadbandPercent { get; init; }

    /// <summary>
    /// Opaque vendor-defined connection data.
    /// </summary>
    public string? VendorData { get; init; }

    private static string[] NormalizeBrowsePaths(string[]? browsePaths)
    {
        if (browsePaths is null || browsePaths.Length == 0)
        {
            return Array.Empty<string>();
        }

        var copy = new string[browsePaths.Length];
        for (var i = 0; i < browsePaths.Length; i++)
        {
            var path = browsePaths[i];
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            copy[i] = path;
        }

        return copy;
    }

    private static int ComputeMask(DxConnection connection)
    {
        var mask = DxMask.None;
        AddIf(!string.IsNullOrEmpty(connection.ItemPath), DxMask.ItemPath, ref mask);
        AddIf(!string.IsNullOrEmpty(connection.ItemName), DxMask.ItemName, ref mask);
        AddIf(!string.IsNullOrEmpty(connection.Version), DxMask.Version, ref mask);
        AddIf(connection.BrowsePaths.Length > 0, DxMask.BrowsePaths, ref mask);
        AddIf(!string.IsNullOrEmpty(connection.Name), DxMask.Name, ref mask);
        AddIf(!string.IsNullOrEmpty(connection.Description), DxMask.Description, ref mask);
        AddIf(!string.IsNullOrEmpty(connection.Keyword), DxMask.Keyword, ref mask);
        AddIf(connection.DefaultSourceItemConnected.HasValue, DxMask.DefaultSourceItemConnected, ref mask);
        AddIf(connection.DefaultTargetItemConnected.HasValue, DxMask.DefaultTargetItemConnected, ref mask);
        AddIf(connection.DefaultOverridden.HasValue, DxMask.DefaultOverridden, ref mask);
        AddIf(connection.DefaultOverrideValue.HasValue, DxMask.DefaultOverrideValue, ref mask);
        AddIf(connection.SubstituteValue.HasValue, DxMask.SubstituteValue, ref mask);
        AddIf(connection.EnableSubstituteValue.HasValue, DxMask.EnableSubstituteValue, ref mask);
        AddIf(!string.IsNullOrEmpty(connection.TargetItemPath), DxMask.TargetItemPath, ref mask);
        AddIf(!string.IsNullOrEmpty(connection.TargetItemName), DxMask.TargetItemName, ref mask);
        AddIf(!string.IsNullOrEmpty(connection.SourceServerName), DxMask.SourceServerName, ref mask);
        AddIf(!string.IsNullOrEmpty(connection.SourceItemPath), DxMask.SourceItemPath, ref mask);
        AddIf(!string.IsNullOrEmpty(connection.SourceItemName), DxMask.SourceItemName, ref mask);
        AddIf(connection.SourceItemQueueSize.HasValue, DxMask.SourceItemQueueSize, ref mask);
        AddIf(connection.UpdateRateMilliseconds.HasValue, DxMask.UpdateRate, ref mask);
        AddIf(connection.DeadbandPercent.HasValue, DxMask.DeadBand, ref mask);
        AddIf(!string.IsNullOrEmpty(connection.VendorData), DxMask.VendorData, ref mask);
        return (int)mask;
    }

    private static void AddIf(bool condition, DxMask value, ref DxMask mask)
    {
        if (condition)
        {
            mask |= value;
        }
    }
}
