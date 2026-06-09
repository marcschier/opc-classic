//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Core;

/// <summary>Authentication and impersonation details supplied during activation.</summary>
public sealed record SecurityInfo(int AuthenticationLevel, int ImpersonationLevel, int Capabilities);
