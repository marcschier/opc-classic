// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Discovery.Dcom;

/// <summary>
/// Registered OPC server details returned by OPCEnum.
/// </summary>
public sealed record OpcEnumClassDetails(string ProgId, string UserType, string VersionIndependentProgId);
