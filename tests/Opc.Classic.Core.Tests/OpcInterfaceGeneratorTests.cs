//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// End-to-end test for OpcInterfaceGenerator. If this test file compiles and
// passes, the [OpcInterface(iid)] -> static Guid InterfaceId pipeline is
// alive end-to-end:
//
//   1. The generator emits Opc.Classic.Generators.OpcInterfaceAttribute into
//      this project at build time.
//   2. The application of [OpcInterface(...)] below is matched by
//      ForAttributeWithMetadataName.
//   3. The generator emits the partial-interface continuation carrying
//      InterfaceId.
//   4. Runtime sees a sealed Guid value matching the attribute argument.
//

using System;
using Opc.Classic.Generators;
using TUnit.Core;

namespace Opc.Classic.Tests;

[OpcInterface("39C13A4D-011E-11D0-9675-0020AFD8ADB3")]
public partial interface IOpcInterfaceGeneratorSample
{
}

public sealed class OpcInterfaceGeneratorTests
{
    private static Guid ReadInterfaceId() => IOpcInterfaceGeneratorSample.InterfaceId;

    [Test]
    public async Task GeneratedInterfaceId_MatchesAttribute()
    {
        var expected = new Guid("39C13A4D-011E-11D0-9675-0020AFD8ADB3");
        await Assert.That(ReadInterfaceId()).IsEqualTo(expected);
    }

    [Test]
    public async Task GeneratedInterfaceId_IsNotEmpty()
    {
        await Assert.That(ReadInterfaceId()).IsNotEqualTo(Guid.Empty);
    }
}
