// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Authentication and impersonation details supplied during activation.
/// </summary>
public sealed record SecurityInfo(int AuthenticationLevel, int ImpersonationLevel, int Capabilities);
