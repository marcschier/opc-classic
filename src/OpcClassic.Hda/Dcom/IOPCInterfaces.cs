//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// OPC HDA DCOM-projection interfaces. Each [OpcInterface] partial interface is
// extended by the OpcInterfaceGenerator to carry a compile-time-known
// InterfaceId. Methods will be added in Phase 8B/8C.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCHDA_Server with underscore)
#pragma warning disable MA0048 // Multiple trivial interface stubs grouped for readability

using OpcClassic.Generators;

namespace OpcClassic.Hda.Dcom;

/// <summary><c>IOPCHDA_Server</c> — top-level HDA server interface (IID_IOPCHDA_Server).</summary>
[OpcInterface("1F1217B0-DEE0-11D2-A5E5-000086339399")]
public partial interface IOPCHDA_Server
{
}

/// <summary><c>IOPCHDA_Browser</c> — HDA address-space browse (IID_IOPCHDA_Browser).</summary>
[OpcInterface("1F1217B1-DEE0-11D2-A5E5-000086339399")]
public partial interface IOPCHDA_Browser
{
}

/// <summary><c>IOPCHDA_SyncRead</c> — synchronous HDA read (IID_IOPCHDA_SyncRead).</summary>
[OpcInterface("1F1217B2-DEE0-11D2-A5E5-000086339399")]
public partial interface IOPCHDA_SyncRead
{
}

/// <summary><c>IOPCHDA_SyncUpdate</c> — synchronous HDA insert/replace/delete (IID_IOPCHDA_SyncUpdate).</summary>
[OpcInterface("1F1217B3-DEE0-11D2-A5E5-000086339399")]
public partial interface IOPCHDA_SyncUpdate
{
}

/// <summary><c>IOPCHDA_SyncAnnotations</c> — synchronous HDA annotation management (IID_IOPCHDA_SyncAnnotations).</summary>
[OpcInterface("1F1217B4-DEE0-11D2-A5E5-000086339399")]
public partial interface IOPCHDA_SyncAnnotations
{
}

/// <summary><c>IOPCHDA_AsyncRead</c> — asynchronous HDA read (IID_IOPCHDA_AsyncRead).</summary>
[OpcInterface("1F1217B5-DEE0-11D2-A5E5-000086339399")]
public partial interface IOPCHDA_AsyncRead
{
}

/// <summary><c>IOPCHDA_AsyncUpdate</c> — asynchronous HDA insert/replace/delete (IID_IOPCHDA_AsyncUpdate).</summary>
[OpcInterface("1F1217B6-DEE0-11D2-A5E5-000086339399")]
public partial interface IOPCHDA_AsyncUpdate
{
}

/// <summary><c>IOPCHDA_AsyncAnnotations</c> — asynchronous HDA annotation management (IID_IOPCHDA_AsyncAnnotations).</summary>
[OpcInterface("1F1217B7-DEE0-11D2-A5E5-000086339399")]
public partial interface IOPCHDA_AsyncAnnotations
{
}

/// <summary><c>IOPCHDA_Playback</c> — HDA playback, server pushes history at rate (IID_IOPCHDA_Playback).</summary>
[OpcInterface("1F1217B8-DEE0-11D2-A5E5-000086339399")]
public partial interface IOPCHDA_Playback
{
}

/// <summary><c>IOPCHDA_DataCallback</c> — HDA async-read / playback callback sink (IID_IOPCHDA_DataCallback).</summary>
[OpcInterface("1F1217B9-DEE0-11D2-A5E5-000086339399")]
public partial interface IOPCHDA_DataCallback
{
}
