// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Microsoft.CodeAnalysis;

namespace Opc.Classic.MigrationAnalyzer;

internal static class MigrationDiagnosticDescriptors
{
    public const string Category = "Opc.Classic.Migration";

    public static readonly DiagnosticDescriptor LegacyServerCreation = Create(
        "OCMDA001",
        "Use Opc.Classic DA client creation",
        "Legacy OPC DA server construction should migrate to OpcDaClient.ConnectAsync or DI-based composition");

    public static readonly DiagnosticDescriptor LegacyBrowse = Create(
        "OCMDA002",
        "Use asynchronous OPC DA browsing",
        "Legacy OPC DA Browse calls should migrate to IOpcDaBrowse.BrowseAsync(itemId, filters, ct)");

    public static readonly DiagnosticDescriptor LegacyRead = Create(
        "OCMDA003",
        "Use asynchronous OPC DA reads",
        "Legacy OPC DA group.Read calls should migrate to IOpcDaSyncIO.ReadAsync(items, ct)");

    public static readonly DiagnosticDescriptor LegacyEventSubscription = Create(
        "OCMAE001",
        "Use async enumerable OPC AE subscriptions",
        "Legacy IOPCEventSubscription callback patterns should migrate to IAsyncEnumerable<OpcEventNotification>");

    public static readonly DiagnosticDescriptor LegacySyncReadRaw = Create(
        "OCMHDA001",
        "Use asynchronous OPC HDA raw reads",
        "Legacy SyncReadRaw calls should migrate to IOpcHdaSyncReadAsync equivalents");

    public static readonly DiagnosticDescriptor UsingOpcRcw = Create(
        "OCMGEN001",
        "Use Opc.Classic namespaces instead of OpcRcw",
        "OpcRcw interop references should migrate to Opc.Classic.* equivalents");

    public static readonly DiagnosticDescriptor ManualVariant = Create(
        "OCMGEN002",
        "Use OpcVariant factories",
        "Manual VARIANT conversion should migrate to OpcVariant.FromXxx factories");

    private static DiagnosticDescriptor Create(string id, string title, string messageFormat) => new(
        id: id,
        title: title,
        messageFormat: messageFormat,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/marcschier/opc-classic/blob/main/docs/migration/" + id + ".md");
}
