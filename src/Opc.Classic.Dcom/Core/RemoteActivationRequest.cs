// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Decoded server-side IRemoteActivation::RemoteActivation request fields.
/// </summary>
public sealed record RemoteActivationRequest(
    Guid Clsid,
    Guid Iid,
    int DwFlags,
    IReadOnlyList<int> ProtocolSeqs);
