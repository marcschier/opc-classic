// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.Logging;
using Opc.Classic;
using Opc.Classic.Da.Hosting;

namespace Opc.Classic.Samples.DaServer;

public sealed class SampleDaServer : IOpcDaServer
{
    private static readonly Action<ILogger, Exception?> GetStatusMessage = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1, nameof(GetStatusAsync)),
        "GetStatus");

    private static readonly Action<ILogger, string, bool, int, Exception?> AddGroupMessage = LoggerMessage.Define<string, bool, int>(
        LogLevel.Information,
        new EventId(2, nameof(AddGroupAsync)),
        "AddGroup: name={Name}, active={Active}, rate={Rate}");

    private static readonly Action<ILogger, int, bool, Exception?> RemoveGroupMessage = LoggerMessage.Define<int, bool>(
        LogLevel.Information,
        new EventId(3, nameof(RemoveGroupAsync)),
        "RemoveGroup: handle={Handle}, force={Force}");

    private static readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;

    private readonly ILogger<SampleDaServer> _logger;
    private readonly TagTree _tags;

    public SampleDaServer(TagTree tags, ILogger<SampleDaServer> logger)
    {
        _tags = tags ?? throw new ArgumentNullException(nameof(tags));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        GetStatusMessage(_logger, null);
        var now = DateTimeOffset.UtcNow;
        var status = new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = StartTime,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            GroupCount = 0,
            BandWidth = 0,
            ServerVersion = new Version(1, 0, 0),
            VendorInfo = $"Opc.Classic Sample DA Server ({_tags.Tags.Count} tags)",
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
        ArgumentNullException.ThrowIfNull(name);
        AddGroupMessage(_logger, name, active, requestedUpdateRate, null);
        return Task.FromResult(unchecked(clientHandle + 0x1000));
    }

    public Task RemoveGroupAsync(
        int serverGroupHandle,
        bool force,
        CancellationToken cancellationToken = default)
    {
        RemoveGroupMessage(_logger, serverGroupHandle, force, null);
        return Task.CompletedTask;
    }

    public Task<string> GetErrorStringAsync(
        int errorCode,
        int localeId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Opc.Classic Sample DA error: 0x{errorCode:X8}");
    }
}
