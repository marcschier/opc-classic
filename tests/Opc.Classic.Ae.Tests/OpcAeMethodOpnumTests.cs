//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Reflection;
using Opc.Classic.Ae.Dcom;
using TUnit.Core;

namespace Opc.Classic.Ae.Tests;

public sealed class OpcAeMethodOpnumTests {
    private const string OpcMethodAttributeFullName = "Opc.Classic.Generators.OpcMethodAttribute";

    private static readonly ExpectedOpcMethod[] OpcAeIdlOpnums =
    [
        new(typeof(IOPCEventServer), nameof(IOPCEventServer.GetStatusAsync), 3),
        new(typeof(IOPCEventServer), nameof(IOPCEventServer.CreateEventSubscriptionAsync), 4),
        new(typeof(IOPCEventServer), nameof(IOPCEventServer.QueryAvailableFiltersAsync), 5),
        new(typeof(IOPCEventServer), nameof(IOPCEventServer.QueryEventCategoriesAsync), 6),
        new(typeof(IOPCEventServer), nameof(IOPCEventServer.QueryConditionNamesAsync), 7),
        new(typeof(IOPCEventServer), nameof(IOPCEventServer.QuerySubConditionNamesAsync), 8),
        new(typeof(IOPCEventServer), nameof(IOPCEventServer.QuerySourceConditionsAsync), 9),
        new(typeof(IOPCEventServer), nameof(IOPCEventServer.QueryEventAttributesAsync), 10),
        new(typeof(IOPCEventServer), nameof(IOPCEventServer.TranslateToItemIDsAsync), 11),
        new(typeof(IOPCEventServer), nameof(IOPCEventServer.GetConditionStateAsync), 12),
        new(typeof(IOPCEventServer), nameof(IOPCEventServer.EnableConditionByAreaAsync), 13),
        new(typeof(IOPCEventServer), nameof(IOPCEventServer.EnableConditionBySourceAsync), 14),
        new(typeof(IOPCEventServer), nameof(IOPCEventServer.DisableConditionByAreaAsync), 15),
        new(typeof(IOPCEventServer), nameof(IOPCEventServer.DisableConditionBySourceAsync), 16),
        new(typeof(IOPCEventServer), nameof(IOPCEventServer.AckConditionAsync), 17),
        new(typeof(IOPCEventServer), nameof(IOPCEventServer.CreateAreaBrowserAsync), 18),
        new(typeof(IOPCEventServer2), nameof(IOPCEventServer2.EnableConditionByArea2Async), 19),
        new(typeof(IOPCEventServer2), nameof(IOPCEventServer2.EnableConditionBySource2Async), 20),
        new(typeof(IOPCEventServer2), nameof(IOPCEventServer2.DisableConditionByArea2Async), 21),
        new(typeof(IOPCEventServer2), nameof(IOPCEventServer2.DisableConditionBySource2Async), 22),
        new(typeof(IOPCEventServer2), nameof(IOPCEventServer2.GetEnableStateByAreaAsync), 23),
        new(typeof(IOPCEventServer2), nameof(IOPCEventServer2.GetEnableStateBySourceAsync), 24),
        new(typeof(IOPCEventSubscriptionMgt), nameof(IOPCEventSubscriptionMgt.SetFilterAsync), 3),
        new(typeof(IOPCEventSubscriptionMgt), nameof(IOPCEventSubscriptionMgt.GetFilterAsync), 4),
        new(typeof(IOPCEventSubscriptionMgt), nameof(IOPCEventSubscriptionMgt.SetReturnedAttributesAsync), 5),
        new(typeof(IOPCEventSubscriptionMgt), nameof(IOPCEventSubscriptionMgt.GetReturnedAttributesAsync), 6),
        new(typeof(IOPCEventSubscriptionMgt), nameof(IOPCEventSubscriptionMgt.RefreshAsync), 7),
        new(typeof(IOPCEventSubscriptionMgt), nameof(IOPCEventSubscriptionMgt.CancelRefreshAsync), 8),
        new(typeof(IOPCEventSubscriptionMgt), nameof(IOPCEventSubscriptionMgt.GetStateAsync), 9),
        new(typeof(IOPCEventSubscriptionMgt), nameof(IOPCEventSubscriptionMgt.SetStateAsync), 10),
        new(typeof(IOPCEventSubscriptionMgt2), nameof(IOPCEventSubscriptionMgt2.SetKeepAliveAsync), 11),
        new(typeof(IOPCEventSubscriptionMgt2), nameof(IOPCEventSubscriptionMgt2.GetKeepAliveAsync), 12),
        new(typeof(IOPCEventAreaBrowser), nameof(IOPCEventAreaBrowser.ChangeBrowsePositionAsync), 3),
        new(typeof(IOPCEventAreaBrowser), nameof(IOPCEventAreaBrowser.BrowseOPCAreasAsync), 4),
        new(typeof(IOPCEventAreaBrowser), nameof(IOPCEventAreaBrowser.GetQualifiedAreaNameAsync), 5),
        new(typeof(IOPCEventAreaBrowser), nameof(IOPCEventAreaBrowser.GetQualifiedSourceNameAsync), 6),
        new(typeof(IOPCEventSink), nameof(IOPCEventSink.OnEventAsync), 3),
    ];

    [Test]
    public async Task OpcAeMethods_MatchOpcAeIdlOpnums() {
        foreach (ExpectedOpcMethod expected in OpcAeIdlOpnums) {
            int actual = GetOpcMethodOpnum(expected.InterfaceType, expected.MethodName);
            await Assert.That(actual).IsEqualTo(expected.Opnum);
        }
    }

    [Test]
    public async Task OpcAeMethods_DoNotDuplicateOpnumsPerInterface() {
        var opnumsByInterface = new Dictionary<Type, HashSet<int>>();
        foreach (ExpectedOpcMethod expected in OpcAeIdlOpnums) {
            if (!opnumsByInterface.TryGetValue(expected.InterfaceType, out HashSet<int>? opnums)) {
                opnums = new HashSet<int>();
                opnumsByInterface.Add(expected.InterfaceType, opnums);
            }

            bool added = opnums.Add(GetOpcMethodOpnum(expected.InterfaceType, expected.MethodName));
            await Assert.That(added).IsTrue();
        }
    }

    private static int GetOpcMethodOpnum(Type interfaceType, string methodName) {
        MethodInfo method = interfaceType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new MissingMethodException(interfaceType.FullName, methodName);

        foreach (CustomAttributeData attribute in method.CustomAttributes) {
            if (attribute.AttributeType.FullName == OpcMethodAttributeFullName &&
                attribute.ConstructorArguments.Count == 1 &&
                attribute.ConstructorArguments[0].Value is int opnum) {
                return opnum;
            }
        }

        throw new InvalidOperationException($"{interfaceType.FullName}.{methodName} is missing [OpcMethod].");
    }

    private readonly record struct ExpectedOpcMethod(Type InterfaceType, string MethodName, int Opnum);
}
