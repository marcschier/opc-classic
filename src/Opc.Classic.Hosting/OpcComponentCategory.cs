//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Hosting;

/// <summary>
/// A single OPC Foundation component-category descriptor: the CATID a server's
/// CLSID can claim membership of, plus the canonical LCID-409 (en-US) description
/// used by <c>ICatInformation::GetCategoryDesc</c>.
/// </summary>
/// <param name="CategoryId">The component-category GUID (CATID).</param>
/// <param name="Description">The human-readable description for LCID 409 (en-US).</param>
public sealed record OpcComponentCategory(Guid CategoryId, string Description);
