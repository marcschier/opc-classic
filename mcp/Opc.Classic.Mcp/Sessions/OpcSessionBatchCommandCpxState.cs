//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Concurrent;
using Opc.Classic.Batch.Dcom;
using Opc.Classic.Commands.Dcom;
using Opc.Classic.Cpx.Dcom;

namespace Opc.Classic.Mcp.Sessions;

/// <summary>Holds OPC Batch wire proxies.</summary>
public sealed class BatchClientState : IAsyncDisposable
{
    private readonly bool _ownsChannel;
    private bool _disposed;

    /// <summary>Creates Batch client state over an existing call channel.</summary>
    public BatchClientState(string host, string? progId, Guid? clsid, ICallChannel channel, bool ownsChannel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentNullException.ThrowIfNull(channel);

        Host = host;
        ProgId = progId;
        Clsid = clsid;
        Channel = channel;
        _ownsChannel = ownsChannel;
        BatchServer = new IOPCBatchServerClientProxy(channel);
        BatchServer2 = new IOPCBatchServer2ClientProxy(channel);
        EnumerationSets = new IOPCEnumerationSetsClientProxy(channel);
    }

    /// <summary>Target host.</summary>
    public string Host { get; }

    /// <summary>Connected Batch server ProgID, if known.</summary>
    public string? ProgId { get; }

    /// <summary>Connected Batch server CLSID, if known.</summary>
    public Guid? Clsid { get; }

    /// <summary>Underlying DCOM call channel.</summary>
    public ICallChannel Channel { get; }

    /// <summary>Batch 1.0 server proxy.</summary>
    public IOPCBatchServerClientProxy BatchServer { get; }

    /// <summary>Batch 2.0 server proxy.</summary>
    public IOPCBatchServer2ClientProxy BatchServer2 { get; }

    /// <summary>Batch enumeration sets proxy.</summary>
    public IOPCEnumerationSetsClientProxy EnumerationSets { get; }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_ownsChannel)
        {
            switch (Channel)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }
}

/// <summary>Holds OPC Commands wire proxies and poll state.</summary>
public sealed class CommandsClientState : IAsyncDisposable
{
    private readonly ICallChannel _channel;
    private readonly bool _ownsChannel;
    private bool _disposed;

    /// <summary>Creates Commands client state over an existing call channel.</summary>
    public CommandsClientState(string host, string? progId, Guid? clsid, ICallChannel channel, bool ownsChannel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentNullException.ThrowIfNull(channel);

        Host = host;
        ProgId = progId;
        Clsid = clsid;
        _channel = channel;
        _ownsChannel = ownsChannel;
        CommandInformation = new IOPCCommandInformationClientProxy(channel);
        CommandExecution = new IOPCCommandExecutionClientProxy(channel);
    }

    /// <summary>Target host.</summary>
    public string Host { get; }

    /// <summary>Connected Commands server ProgID, if known.</summary>
    public string? ProgId { get; }

    /// <summary>Connected Commands server CLSID, if known.</summary>
    public Guid? Clsid { get; }

    /// <summary>Commands metadata proxy.</summary>
    public IOPCCommandInformationClientProxy CommandInformation { get; }

    /// <summary>Commands execution proxy.</summary>
    public IOPCCommandExecutionClientProxy CommandExecution { get; }

    /// <summary>Known asynchronous invocations by invocation ID.</summary>
    public ConcurrentDictionary<string, CommandsInvocationContext> Invocations { get; } = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Invocations.Clear();
        if (_ownsChannel)
        {
            switch (_channel)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }
}

/// <summary>Tracks an asynchronous Commands invocation for MCP polling.</summary>
public sealed class CommandsInvocationContext
{
    /// <summary>Creates a Commands invocation context.</summary>
    public CommandsInvocationContext(string invocationId, string commandName, string commandNamespace, string targetId, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invocationId);
        InvocationId = invocationId;
        CommandName = commandName;
        CommandNamespace = commandNamespace;
        TargetId = targetId;
        CreatedAt = createdAt;
    }

    /// <summary>Invocation ID returned by the server.</summary>
    public string InvocationId { get; }

    /// <summary>Command name.</summary>
    public string CommandName { get; }

    /// <summary>Command namespace.</summary>
    public string CommandNamespace { get; }

    /// <summary>Command target ID.</summary>
    public string TargetId { get; }

    /// <summary>UTC creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>Number of observed state changes.</summary>
    public int EventCount { get; set; }

    /// <summary>Last permitted-controls list returned by QueryState.</summary>
    public string[] LastPermittedControls { get; set; } = [];
}

/// <summary>Holds OPC Complex Data wire proxies.</summary>
public sealed class CpxClientState : IAsyncDisposable
{
    private readonly ICallChannel _channel;
    private readonly bool _ownsChannel;
    private bool _disposed;

    /// <summary>Creates CPX client state over an existing call channel.</summary>
    public CpxClientState(string host, string? progId, Guid? clsid, ICallChannel channel, bool ownsChannel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentNullException.ThrowIfNull(channel);

        Host = host;
        ProgId = progId;
        Clsid = clsid;
        _channel = channel;
        _ownsChannel = ownsChannel;
        ComplexDataItem = new IOPCComplexDataItemClientProxy(channel);
        ComplexDataItem2 = new IOPCComplexDataItem2ClientProxy(channel);
        TypeLibrary = new IOPCTypeLibraryClientProxy(channel);
    }

    /// <summary>Target host.</summary>
    public string Host { get; }

    /// <summary>Connected server ProgID, if known.</summary>
    public string? ProgId { get; }

    /// <summary>Connected server CLSID, if known.</summary>
    public Guid? Clsid { get; }

    /// <summary>CPX item metadata proxy.</summary>
    public IOPCComplexDataItemClientProxy ComplexDataItem { get; }

    /// <summary>CPX extended item metadata proxy.</summary>
    public IOPCComplexDataItem2ClientProxy ComplexDataItem2 { get; }

    /// <summary>CPX type-library proxy.</summary>
    public IOPCTypeLibraryClientProxy TypeLibrary { get; }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_ownsChannel)
        {
            switch (_channel)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }
}
