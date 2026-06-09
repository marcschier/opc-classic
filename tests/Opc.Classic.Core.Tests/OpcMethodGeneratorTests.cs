//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// End-to-end test for the OpcMethod opnum-emission half of
// OpcInterfaceGenerator. Declares a sample partial interface decorated
// with [OpcInterface] + [OpcMethod(opnum)] on three members, then
// asserts that the generator emits the nested Opnums static class with
// the correct compile-time constants.
//

using Opc.Classic.Generators;
using TUnit.Core;

namespace Opc.Classic.Tests;

[OpcInterface("11223344-5566-7788-99AA-BBCCDDEEFF00")]
public partial interface ISampleOpcInterfaceWithOpnums {
    [OpcMethod(3)]
    int FooMethod();

    [OpcMethod(4)]
    int BarMethod(string item);

    [OpcMethod(7)]
    int BazMethod();
}

public sealed class OpcMethodGeneratorTests {
    private static int ReadFoo() => ISampleOpcInterfaceWithOpnums.Opnums.FooMethod;
    private static int ReadBar() => ISampleOpcInterfaceWithOpnums.Opnums.BarMethod;
    private static int ReadBaz() => ISampleOpcInterfaceWithOpnums.Opnums.BazMethod;

    [Test]
    public async Task Foo_Opnum_IsThree() {
        await Assert.That(ReadFoo()).IsEqualTo(3);
    }

    [Test]
    public async Task Bar_Opnum_IsFour() {
        await Assert.That(ReadBar()).IsEqualTo(4);
    }

    [Test]
    public async Task Baz_Opnum_IsSeven() {
        await Assert.That(ReadBaz()).IsEqualTo(7);
    }

    [Test]
    public async Task Opnums_AreUnique() {
        // If the generator's duplicate-opnum diagnostic ever regressed
        // and let two methods share an opnum, this test would still pass
        // — but the project wouldn't compile.  This is a sanity check
        // that the three constants are distinct.
        await Assert.That(ReadFoo()).IsNotEqualTo(ReadBar());
        await Assert.That(ReadFoo()).IsNotEqualTo(ReadBaz());
        await Assert.That(ReadBar()).IsNotEqualTo(ReadBaz());
    }
}
