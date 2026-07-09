// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Ae.Hosting;

/// <summary>
/// Dispatches Windows CCW calls for one AE area-browser cursor.
/// </summary>
public interface IOpcAeAreaBrowserDispatcher
{
    /// <summary>
    /// Moves the browser cursor.
    /// </summary>
    Task ChangeBrowsePositionAsync(int browseDirection, string? position, CancellationToken cancellationToken = default);

    /// <summary>
    /// Browses the current cursor and returns area or source names matching the filter.
    /// </summary>
    Task<string[]> BrowseAreasAsync(int browseFilterType, string filterCriteria, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a fully-qualified area name for a name relative to the current cursor.
    /// </summary>
    Task<string> GetQualifiedAreaNameAsync(string areaName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a fully-qualified source name for a name relative to the current cursor.
    /// </summary>
    Task<string> GetQualifiedSourceNameAsync(string sourceName, CancellationToken cancellationToken = default);
}
