//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Reflection;
using Opc.Classic.Da.Dcom;
using TUnit.Core;

namespace Opc.Classic.Da.Tests;

public sealed class OpcMethodOpnumTests
{
    private const string OpcMethodAttributeFullName = "Opc.Classic.Generators.OpcMethodAttribute";

    private static readonly ExpectedOpcMethod[] OpcDaIdlOpnums =
    [
        new(typeof(IOPCServer), nameof(IOPCServer.AddGroupAsync), 3),
        new(typeof(IOPCServer), nameof(IOPCServer.GetErrorStringAsync), 4),
        new(typeof(IOPCServer), nameof(IOPCServer.GetGroupByNameAsync), 5),
        new(typeof(IOPCServer), nameof(IOPCServer.GetStatusAsync), 6),
        new(typeof(IOPCServer), nameof(IOPCServer.RemoveGroupAsync), 7),
        new(typeof(IOPCServer), nameof(IOPCServer.CreateGroupEnumeratorAsync), 8),
        new(typeof(IOPCCommon), nameof(IOPCCommon.SetLocaleIdAsync), 3),
        new(typeof(IOPCCommon), nameof(IOPCCommon.GetLocaleIdAsync), 4),
        new(typeof(IOPCCommon), nameof(IOPCCommon.QueryAvailableLocaleIdsAsync), 5),
        new(typeof(IOPCCommon), nameof(IOPCCommon.GetErrorStringAsync), 6),
        new(typeof(IOPCCommon), nameof(IOPCCommon.SetClientNameAsync), 7),
        new(typeof(IOPCBrowse), nameof(IOPCBrowse.GetPropertiesAsync), 3),
        new(typeof(IOPCBrowse), nameof(IOPCBrowse.BrowseAsync), 4),
        new(typeof(IOPCBrowseServerAddressSpace), nameof(IOPCBrowseServerAddressSpace.QueryOrganizationAsync), 3),
        new(typeof(IOPCBrowseServerAddressSpace), nameof(IOPCBrowseServerAddressSpace.ChangeBrowsePositionAsync), 4),
        new(typeof(IOPCBrowseServerAddressSpace), nameof(IOPCBrowseServerAddressSpace.BrowseOpcItemIdsAsync), 5),
        new(typeof(IOPCBrowseServerAddressSpace), nameof(IOPCBrowseServerAddressSpace.GetItemIdAsync), 6),
        new(typeof(IOPCBrowseServerAddressSpace), nameof(IOPCBrowseServerAddressSpace.BrowseAccessPathsAsync), 7),
        new(typeof(IOPCItemProperties), nameof(IOPCItemProperties.QueryAvailablePropertiesAsync), 3),
        new(typeof(IOPCItemProperties), nameof(IOPCItemProperties.GetItemPropertiesAsync), 4),
        new(typeof(IOPCItemProperties), nameof(IOPCItemProperties.LookupItemIdsAsync), 5),
        new(typeof(IOPCItemDeadbandMgt), nameof(IOPCItemDeadbandMgt.SetItemDeadbandAsync), 3),
        new(typeof(IOPCItemDeadbandMgt), nameof(IOPCItemDeadbandMgt.GetItemDeadbandAsync), 4),
        new(typeof(IOPCItemDeadbandMgt), nameof(IOPCItemDeadbandMgt.ClearItemDeadbandAsync), 5),
        new(typeof(IOPCItemSamplingMgt), nameof(IOPCItemSamplingMgt.SetItemSamplingRateAsync), 3),
        new(typeof(IOPCItemSamplingMgt), nameof(IOPCItemSamplingMgt.GetItemSamplingRateAsync), 4),
        new(typeof(IOPCItemSamplingMgt), nameof(IOPCItemSamplingMgt.ClearItemSamplingRateAsync), 5),
        new(typeof(IOPCItemSamplingMgt), nameof(IOPCItemSamplingMgt.SetItemBufferEnableAsync), 6),
        new(typeof(IOPCItemSamplingMgt), nameof(IOPCItemSamplingMgt.GetItemBufferEnableAsync), 7),
        new(typeof(IOPCItemIO), nameof(IOPCItemIO.ReadAsync), 3),
        new(typeof(IOPCItemIO), nameof(IOPCItemIO.WriteVqtAsync), 4),
        new(typeof(IOPCItemMgt), nameof(IOPCItemMgt.AddItemsAsync), 3),
        new(typeof(IOPCItemMgt), nameof(IOPCItemMgt.ValidateItemsAsync), 4),
        new(typeof(IOPCItemMgt), nameof(IOPCItemMgt.RemoveItemsAsync), 5),
        new(typeof(IOPCItemMgt), nameof(IOPCItemMgt.SetActiveStateAsync), 6),
        new(typeof(IOPCItemMgt), nameof(IOPCItemMgt.SetClientHandlesAsync), 7),
        new(typeof(IOPCItemMgt), nameof(IOPCItemMgt.SetDatatypesAsync), 8),
        new(typeof(IOPCItemMgt), nameof(IOPCItemMgt.CreateEnumeratorAsync), 9),
        new(typeof(IOPCGroupStateMgt), nameof(IOPCGroupStateMgt.GetStateAsync), 3),
        new(typeof(IOPCGroupStateMgt), nameof(IOPCGroupStateMgt.SetStateAsync), 4),
        new(typeof(IOPCGroupStateMgt), nameof(IOPCGroupStateMgt.SetNameAsync), 5),
        new(typeof(IOPCGroupStateMgt), nameof(IOPCGroupStateMgt.CloneGroupAsync), 6),
        new(typeof(IOPCGroupStateMgt2), nameof(IOPCGroupStateMgt2.SetKeepAliveAsync), 7),
        new(typeof(IOPCGroupStateMgt2), nameof(IOPCGroupStateMgt2.GetKeepAliveAsync), 8),
        new(typeof(IOPCSyncIO), nameof(IOPCSyncIO.ReadAsync), 3),
        new(typeof(IOPCSyncIO), nameof(IOPCSyncIO.WriteAsync), 4),
        new(typeof(IOPCSyncIO2), nameof(IOPCSyncIO2.ReadAsync), 3),
        new(typeof(IOPCSyncIO2), nameof(IOPCSyncIO2.WriteAsync), 4),
        new(typeof(IOPCSyncIO2), nameof(IOPCSyncIO2.ReadMaxAgeAsync), 5),
        new(typeof(IOPCSyncIO2), nameof(IOPCSyncIO2.WriteVqtAsync), 6),
        new(typeof(IOPCAsyncIO2), nameof(IOPCAsyncIO2.ReadAsync), 3),
        new(typeof(IOPCAsyncIO2), nameof(IOPCAsyncIO2.WriteAsync), 4),
        new(typeof(IOPCAsyncIO2), nameof(IOPCAsyncIO2.Refresh2Async), 5),
        new(typeof(IOPCAsyncIO2), nameof(IOPCAsyncIO2.Cancel2Async), 6),
        new(typeof(IOPCAsyncIO2), nameof(IOPCAsyncIO2.SetEnableAsync), 7),
        new(typeof(IOPCAsyncIO2), nameof(IOPCAsyncIO2.GetEnableAsync), 8),
        new(typeof(IOPCAsyncIO3), nameof(IOPCAsyncIO3.Refresh2Async), 5),
        new(typeof(IOPCAsyncIO3), nameof(IOPCAsyncIO3.Cancel2Async), 6),
        new(typeof(IOPCAsyncIO3), nameof(IOPCAsyncIO3.SetEnableAsync), 7),
        new(typeof(IOPCAsyncIO3), nameof(IOPCAsyncIO3.GetEnableAsync), 8),
        new(typeof(IOPCAsyncIO3), nameof(IOPCAsyncIO3.ReadMaxAgeAsync), 9),
        new(typeof(IOPCAsyncIO3), nameof(IOPCAsyncIO3.WriteVqtAsync), 10),
        new(typeof(IOPCAsyncIO3), nameof(IOPCAsyncIO3.RefreshMaxAgeAsync), 11),
        new(typeof(IConnectionPoint), nameof(IConnectionPoint.GetConnectionInterfaceAsync), 3),
        new(typeof(IConnectionPoint), nameof(IConnectionPoint.AdviseAsync), 5),
        new(typeof(IConnectionPoint), nameof(IConnectionPoint.UnadviseAsync), 6),
        new(typeof(IOPCShutdown), nameof(IOPCShutdown.ShutdownRequestAsync), 3),
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

    [Test]
    public async Task OpcDaMethods_DoNotDuplicateOpnumsPerInterface()
    {
        var opnumsByInterface = new Dictionary<Type, HashSet<int>>();
        foreach (ExpectedOpcMethod expected in OpcDaIdlOpnums)
        {
            if (!opnumsByInterface.TryGetValue(expected.InterfaceType, out HashSet<int>? opnums))
            {
                opnums = new HashSet<int>();
                opnumsByInterface.Add(expected.InterfaceType, opnums);
            }

            bool added = opnums.Add(GetOpcMethodOpnum(expected.InterfaceType, expected.MethodName));
            await Assert.That(added).IsTrue();
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
