// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors

using Microsoft.Extensions.Logging;
using OpcClassic.Da.Hosting;

namespace OpcClassic.CttServer;

public sealed class CttDaServer : IOpcDaServer
{
    private static readonly Action<ILogger, Exception?> GetStatusMessage = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1, nameof(GetStatusAsync)),
        "GetStatus");

    private static readonly Action<ILogger, string, bool, int, Exception?> AddGroupMessage = LoggerMessage.Define<string, bool, int>(
        LogLevel.Information,
        new EventId(2, nameof(AddGroupAsync)),
        "AddGroup: name={Name}, active={Active}, rate={Rate}");

    private static readonly Action<ILogger, int, Exception?> RemoveGroupMessage = LoggerMessage.Define<int>(
        LogLevel.Information,
        new EventId(3, nameof(RemoveGroupAsync)),
        "RemoveGroup: handle={Handle}");

    private static readonly DateTimeOffset StartupTime = DateTimeOffset.UtcNow;

    private readonly ILogger<CttDaServer> _logger;

    public CttDaServer(ILogger<CttDaServer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        GetStatusMessage(_logger, null);
        var now = DateTimeOffset.UtcNow;
        var status = new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = StartupTime,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            GroupCount = 0,
            BandWidth = 0,
            ServerVersion = new Version(1, 0, 0),
            VendorInfo = "OpcClassic .NET CTT Sample",
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
        return Task.FromResult(clientHandle + 1000);
    }

    public Task RemoveGroupAsync(
        int serverGroupHandle,
        bool force,
        CancellationToken cancellationToken = default)
    {
        RemoveGroupMessage(_logger, serverGroupHandle, null);
        return Task.CompletedTask;
    }

    public Task<string> GetErrorStringAsync(
        int errorCode,
        int localeId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"OpcClassic CTT sample error: 0x{errorCode:X8}");
    }
}
