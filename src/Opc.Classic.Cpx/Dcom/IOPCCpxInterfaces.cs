//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// OPC Complex Data (Cpx) DCOM-projection interfaces. Generated proxy coverage
// starts with string/Guid metadata calls; CPX codecs parse dictionary and
// binary/XML payloads in the managed type-system layer.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCComplexDataItem)
#pragma warning disable MA0048 // Multiple small interface projections grouped for readability

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Generators;

namespace Opc.Classic.Cpx.Dcom;

/// <summary><c>IOPCComplexDataItem</c> — Complex Data item metadata/filter interface (IID_IOPCComplexDataItem).</summary>
[OpcInterface("7ECE6649-2C1E-494A-BB99-22D36FB3B0C3")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCComplexDataItem
{
    /// <summary><c>IOPCComplexDataItem::GetTypeItemID</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<string> GetTypeItemIDAsync(string itemId, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCComplexDataItem::GetUnconvertedItemID</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task<string> GetUnconvertedItemIDAsync(string itemId, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCComplexDataItem::GetDataFilter</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task<string> GetDataFilterAsync(string itemId, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCComplexDataItem::SetDataFilter</c> (opnum 6).</summary>
    [OpcMethod(6)]
    Task SetDataFilterAsync(string itemId, string dataFilter, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCComplexDataItem2</c> — extended Complex Data item interface (IID_IOPCComplexDataItem2).</summary>
[OpcInterface("44F68398-60AF-4F02-9442-172D058CB16F")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCComplexDataItem2
{
    /// <summary><c>IOPCComplexDataItem2::GetTypeID</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<Guid> GetTypeIDAsync(string itemId, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCComplexDataItem2::GetDictionaryID</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task<string> GetDictionaryIDAsync(string itemId, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCComplexDataItem2::GetAvailableFilters</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task<string[]> GetAvailableFiltersAsync(string itemId, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCTypeLibrary</c> — Complex Data type-library metadata interface (IID_IOPCTypeLibrary).</summary>
[OpcInterface("B8C1B2C6-ACB7-4B7B-87B5-6EAC2CF63C31")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCTypeLibrary
{
    /// <summary><c>IOPCTypeLibrary::GetDictionary</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<string> GetDictionaryAsync(string dictionaryId, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCTypeLibrary::GetTypeID</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task<string> GetTypeIDAsync(string typeName, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCTypeLibrary::GetTypeItemID</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task<string> GetTypeItemIDAsync(string typeName, CancellationToken cancellationToken = default);
}
