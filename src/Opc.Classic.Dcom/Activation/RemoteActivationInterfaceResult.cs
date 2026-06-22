// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Activation;

/// <summary>
/// Per-IID activation outcome inside a <see cref="RemoteActivationResponse" />.
/// </summary>
/// <param name="Hresult">Per-IID HRESULT.</param>
/// <param name="ObjRef">Encoded OBJREF bytes for the bound interface (empty on failure).</param>
public sealed record RemoteActivationInterfaceResult(int Hresult, ReadOnlyMemory<byte> ObjRef);
