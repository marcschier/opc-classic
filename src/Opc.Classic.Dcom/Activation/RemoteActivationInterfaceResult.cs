//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Dcom.Activation;

/// <summary>Per-IID activation outcome inside a <see cref="RemoteActivationResponse" />.</summary>
/// <param name="Hresult">Per-IID HRESULT.</param>
/// <param name="ObjRef">Encoded OBJREF bytes for the bound interface (empty on failure).</param>
public sealed record RemoteActivationInterfaceResult(int Hresult, ReadOnlyMemory<byte> ObjRef);
