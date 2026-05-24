//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace SharpInterop.Core;

/// <summary>Requested class and interface activation details.</summary>
public sealed record InstanceInfo(Guid Clsid, Guid RequestedIid, int ClassContext, int Mode);
