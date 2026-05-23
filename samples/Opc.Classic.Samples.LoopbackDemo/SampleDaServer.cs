// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.Logging;
using Opc.Classic.Da.Hosting;

namespace Opc.Classic.Samples.LoopbackDemo;

internal sealed class SampleDaServer : IOpcDaServer
{
    private static readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;

    private static readonly Action<ILogger, Exception?> GetStatusMessage = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1, nameof(GetStatusAsync)),
        "Loopback GetStatus");

    private static readonly Action<ILogger, string, bool, int, Exception?> AddGroupMessage = LoggerMessage.Define<string, bool, int>(
        LogLevel.Information,
        new EventId(2, nameof(AddGroupAsync)),
        "Loopback AddGroup: name={Name}, active={Active}, rate={Rate}");

    private static readonly Action<ILogger, int, bool, Exception?> RemoveGroupMessage = LoggerMessage.Define<int, bool>(
        LogLevel.Information,
        new EventId(3, nameof(RemoveGroupAsync)),
        "Loopback RemoveGroup: handle={Handle}, force={Force}");

    private readonly ILogger<SampleDaServer> _logger;
    private readonly LoopbackTagStore _tags;

    public SampleDaServer(LoopbackTagStore tags, ILogger<SampleDaServer> logger)
    {
        _tags = tags ?? throw new ArgumentNullException(nameof(tags));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        GetStatusMessage(_logger, null);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        var status = new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = StartTime,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            ServerVersion = new Version(1, 0, 0),
            VendorInfo = $"Opc.Classic loopback SampleDaServer ({_tags.Tags.Count} tags)",
            GroupCount = 0,
            BandWidth = 0xFFFFFFFF,
        };

        return Task.FromResult(status);
    }

    public Task<int> AddGroupAsync(
        string name,
        bool active,
        int requestedUpdateRate,
        int clientHandle,
        int localeId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        cancellationToken.ThrowIfCancellationRequested();
        AddGroupMessage(_logger, name, active, requestedUpdateRate, null);
        return Task.FromResult(unchecked(clientHandle + 0x1000));
    }

    public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RemoveGroupMessage(_logger, serverGroupHandle, force, null);
        return Task.CompletedTask;
    }

    public Task<string> GetErrorStringAsync(
        int errorCode,
        int localeId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult($"Loopback SampleDaServer error: 0x{errorCode:X8}");
    }
}
