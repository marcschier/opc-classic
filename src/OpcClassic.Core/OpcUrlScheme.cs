//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

namespace OpcClassic;

/// <summary>The OPC Classic URL scheme.</summary>
public enum OpcUrlScheme
{
    /// <summary><c>opcda://</c> — OPC Data Access (DA 2.x / 3.0).</summary>
    Da,
    /// <summary><c>opcae://</c> — OPC Alarms &amp; Events.</summary>
    Ae,
    /// <summary><c>opchda://</c> — OPC Historical Data Access.</summary>
    Hda,
    /// <summary><c>opcdx://</c> — OPC Data eXchange.</summary>
    Dx,
    /// <summary><c>opc.xml-da://</c> — OPC XML-DA over HTTP/SOAP.</summary>
    XmlDa,
}
