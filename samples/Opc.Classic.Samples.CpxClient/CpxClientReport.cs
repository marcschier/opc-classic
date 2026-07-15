// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Samples.CpxClient;

public sealed record CpxClientReport(
    IReadOnlyList<string> BrowsedItems,
    IReadOnlyList<string> DecodedItems,
    IReadOnlyList<string> InvalidPayloads,
    IReadOnlyList<string> UnsupportedTypeSystems,
    int FilterResult,
    int ConversionResult,
    int UnsupportedFilterResult,
    int UnsupportedConversionResult);
