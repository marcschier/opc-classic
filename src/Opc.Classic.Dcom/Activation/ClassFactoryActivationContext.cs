// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Activation context supplied to registered class factories.
/// </summary>
public sealed record ClassFactoryActivationContext(
    Guid Clsid,
    Guid RequestedIid,
    ActivationProperties ActivationProperties);
