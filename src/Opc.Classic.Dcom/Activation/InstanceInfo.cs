// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Requested class and interface activation details.
/// </summary>
public sealed record InstanceInfo(Guid Clsid, Guid RequestedIid, int ClassContext, int Mode);
