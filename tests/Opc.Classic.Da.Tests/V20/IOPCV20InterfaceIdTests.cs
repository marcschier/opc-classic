// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.
//

using System.Reflection;
using Opc.Classic.Da.V20.Dcom;

namespace Opc.Classic.Da.Tests.V20;

public sealed class IOPCV20InterfaceIdTests
{
    [Test]
    public async Task IOPCSyncIO_V20_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCSyncIO.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCSyncIO);
    }

    [Test]
    public async Task IOPCAsyncIO_V20_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCAsyncIO.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCAsyncIO);
    }

    [Test]
    public async Task IOPCSyncIO_V20_Read_Opnum_MatchesDa205a()
    {
        MethodInfo method = typeof(IOPCSyncIO).GetMethod(nameof(IOPCSyncIO.ReadAsync))
            ?? throw new MissingMethodException(typeof(IOPCSyncIO).FullName, nameof(IOPCSyncIO.ReadAsync));
        object? value = method.GetCustomAttributesData()
            .Single(attribute => attribute.AttributeType.FullName == "Opc.Classic.Generators.OpcMethodAttribute")
            .ConstructorArguments[0].Value;

        await Assert.That((int)value!).IsEqualTo(3);
    }

    [Test]
    public async Task V20Interfaces_AreInLegacyDcomNamespace()
    {
        await Assert.That(typeof(IOPCSyncIO).Namespace).IsEqualTo("Opc.Classic.Da.V20.Dcom");
        await Assert.That(typeof(IOPCAsyncIO).Namespace).IsEqualTo("Opc.Classic.Da.V20.Dcom");
    }
}
