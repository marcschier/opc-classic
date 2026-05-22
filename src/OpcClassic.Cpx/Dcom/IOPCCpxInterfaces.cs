//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// OPC Complex Data (Cpx) DCOM-projection interfaces. Each [OpcInterface]
// partial interface is extended by the OpcInterfaceGenerator to carry a
// compile-time-known InterfaceId. Methods + NDR codecs are deferred to the
// Phase 9B follow-up that maps OPCBinary.xsd type dictionaries to wire codecs.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCComplexDataItem)
#pragma warning disable MA0048 // Multiple trivial interface stubs grouped for readability

using OpcClassic.Generators;

namespace OpcClassic.Cpx.Dcom;

/// <summary><c>IOPCComplexDataItem</c> — Complex Data item metadata/filter interface (IID_IOPCComplexDataItem).</summary>
[OpcInterface("7ECE6649-2C1E-494A-BB99-22D36FB3B0C3")]
public partial interface IOPCComplexDataItem
{
}

/// <summary><c>IOPCComplexDataItem2</c> — extended Complex Data item interface (IID_IOPCComplexDataItem2).</summary>
[OpcInterface("44F68398-60AF-4F02-9442-172D058CB16F")]
public partial interface IOPCComplexDataItem2
{
}

/// <summary><c>IOPCTypeLibrary</c> — Complex Data type-library metadata interface (IID_IOPCTypeLibrary).</summary>
[OpcInterface("B8C1B2C6-ACB7-4B7B-87B5-6EAC2CF63C31")]
public partial interface IOPCTypeLibrary
{
}
