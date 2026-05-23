//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Reflection;
using Opc.Classic.Da.Dcom;
using TUnit.Core;

namespace Opc.Classic.Da.Tests;

public sealed class OpcMethodOpnumTests
{
    private const string OpcMethodAttributeFullName = "Opc.Classic.Generators.OpcMethodAttribute";

    private static readonly ExpectedOpcMethod[] OpcDaIdlOpnums =
    [
        new(typeof(IOPCServer), nameof(IOPCServer.GetErrorStringAsync), 4),
        new(typeof(IOPCServer), nameof(IOPCServer.GetStatusAsync), 6),
        new(typeof(IOPCServer), nameof(IOPCServer.RemoveGroupAsync), 7),
        new(typeof(IOPCBrowse), nameof(IOPCBrowse.GetPropertiesAsync), 3),
        new(typeof(IOPCBrowseServerAddressSpace), nameof(IOPCBrowseServerAddressSpace.QueryOrganizationAsync), 3),
        new(typeof(IOPCBrowseServerAddressSpace), nameof(IOPCBrowseServerAddressSpace.ChangeBrowsePositionAsync), 4),
        new(typeof(IOPCBrowseServerAddressSpace), nameof(IOPCBrowseServerAddressSpace.GetItemIdAsync), 6),
        new(typeof(IOPCItemMgt), nameof(IOPCItemMgt.RemoveItemsAsync), 5),
        new(typeof(IOPCItemMgt), nameof(IOPCItemMgt.SetActiveStateAsync), 6),
        new(typeof(IOPCItemMgt), nameof(IOPCItemMgt.SetClientHandlesAsync), 7),
        new(typeof(IOPCItemMgt), nameof(IOPCItemMgt.SetDatatypesAsync), 8),
        new(typeof(IOPCGroupStateMgt), nameof(IOPCGroupStateMgt.GetStateAsync), 3),
        new(typeof(IOPCGroupStateMgt), nameof(IOPCGroupStateMgt.SetNameAsync), 5),
        new(typeof(IOPCSyncIO), nameof(IOPCSyncIO.WriteAsync), 4),
        new(typeof(IOPCSyncIO2), nameof(IOPCSyncIO2.WriteAsync), 4),
        new(typeof(IOPCSyncIO2), nameof(IOPCSyncIO2.WriteVqtAsync), 6),
        new(typeof(IOPCAsyncIO2), nameof(IOPCAsyncIO2.Refresh2Async), 5),
        new(typeof(IOPCAsyncIO2), nameof(IOPCAsyncIO2.Cancel2Async), 6),
        new(typeof(IOPCAsyncIO2), nameof(IOPCAsyncIO2.SetEnableAsync), 7),
        new(typeof(IOPCAsyncIO2), nameof(IOPCAsyncIO2.GetEnableAsync), 8),
        new(typeof(IOPCAsyncIO3), nameof(IOPCAsyncIO3.Refresh2Async), 5),
        new(typeof(IOPCAsyncIO3), nameof(IOPCAsyncIO3.Cancel2Async), 6),
        new(typeof(IOPCAsyncIO3), nameof(IOPCAsyncIO3.SetEnableAsync), 7),
        new(typeof(IOPCAsyncIO3), nameof(IOPCAsyncIO3.GetEnableAsync), 8),
        new(typeof(IOPCAsyncIO3), nameof(IOPCAsyncIO3.RefreshMaxAgeAsync), 11),
        new(typeof(IOPCDataCallback), nameof(IOPCDataCallback.OnDataChangeAsync), 3),
        new(typeof(IOPCDataCallback), nameof(IOPCDataCallback.OnReadCompleteAsync), 4),
        new(typeof(IOPCDataCallback), nameof(IOPCDataCallback.OnWriteCompleteAsync), 5),
        new(typeof(IOPCDataCallback), nameof(IOPCDataCallback.OnCancelCompleteAsync), 6),
    ];

    [Test]
    public async Task OpcDaMethods_MatchOpcDaIdlOpnums()
    {
        foreach (ExpectedOpcMethod expected in OpcDaIdlOpnums)
        {
            int actual = GetOpcMethodOpnum(expected.InterfaceType, expected.MethodName);
            await Assert.That(actual).IsEqualTo(expected.Opnum);
        }
    }

    private static int GetOpcMethodOpnum(Type interfaceType, string methodName)
    {
        MethodInfo method = interfaceType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new MissingMethodException(interfaceType.FullName, methodName);

        foreach (CustomAttributeData attribute in method.CustomAttributes)
        {
            if (attribute.AttributeType.FullName == OpcMethodAttributeFullName &&
                attribute.ConstructorArguments.Count == 1 &&
                attribute.ConstructorArguments[0].Value is int opnum)
            {
                return opnum;
            }
        }

        throw new InvalidOperationException($"{interfaceType.FullName}.{methodName} is missing [OpcMethod].");
    }

    private readonly record struct ExpectedOpcMethod(Type InterfaceType, string MethodName, int Opnum);
}
