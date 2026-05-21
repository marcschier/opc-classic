//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// OPC AE DCOM-projection interfaces. Each [OpcInterface] partial interface is
// extended by the OpcInterfaceGenerator to carry a compile-time-known
// InterfaceId. Methods will be added in Phase 7B/7C.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCEventServer not IOpcEventServer)
#pragma warning disable MA0048 // Multiple trivial interface stubs grouped for readability

using OpcClassic.Generators;

namespace OpcClassic.Ae.Dcom;

/// <summary><c>IOPCEventServer</c> — top-level AE server interface (IID_IOPCEventServer).</summary>
[OpcInterface("65168851-5783-11D1-84A0-00608CB8A7E9")]
public partial interface IOPCEventServer
{
}

/// <summary><c>IOPCEventServer2</c> — AE 1.10 enable-/disable-conditions extensions (IID_IOPCEventServer2).</summary>
[OpcInterface("71BBE88E-9564-4BCD-BCFC-71C558D94F2D")]
public partial interface IOPCEventServer2
{
}

/// <summary><c>IOPCEventSubscriptionMgt</c> — AE event subscription management (IID_IOPCEventSubscriptionMgt).</summary>
[OpcInterface("65168855-5783-11D1-84A0-00608CB8A7E9")]
public partial interface IOPCEventSubscriptionMgt
{
}

/// <summary><c>IOPCEventSubscriptionMgt2</c> — AE 1.10 keep-alive extensions (IID_IOPCEventSubscriptionMgt2).</summary>
[OpcInterface("94C955DC-3684-4CCB-AFAB-F898CE19AAC3")]
public partial interface IOPCEventSubscriptionMgt2
{
}

/// <summary><c>IOPCEventAreaBrowser</c> — AE area-namespace browser (IID_IOPCEventAreaBrowser).</summary>
[OpcInterface("65168857-5783-11D1-84A0-00608CB8A7E9")]
public partial interface IOPCEventAreaBrowser
{
}

/// <summary><c>IOPCEventSink</c> — AE event-delivery callback sink (IID_IOPCEventSink).</summary>
[OpcInterface("6516885F-5783-11D1-84A0-00608CB8A7E9")]
public partial interface IOPCEventSink
{
}
