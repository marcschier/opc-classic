//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace SharpInterop.Core;

/// <summary>Activation context supplied to registered class factories.</summary>
public sealed record ClassFactoryActivationContext(
    Guid Clsid,
    Guid RequestedIid,
    ActivationProperties ActivationProperties);
