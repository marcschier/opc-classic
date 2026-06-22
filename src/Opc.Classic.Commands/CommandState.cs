// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Commands;

/// <summary>
/// OPC Commands invocation state values.
/// </summary>
public enum CommandState
{
    /// <summary>
    /// The invocation exists but has not been queued.
    /// </summary>
    Created = 1,

    /// <summary>
    /// The invocation is queued for execution.
    /// </summary>
    Queued = 2,

    /// <summary>
    /// The server is executing the command.
    /// </summary>
    Executing = 3,

    /// <summary>
    /// The command completed successfully.
    /// </summary>
    Complete = 4,

    /// <summary>
    /// The command failed.
    /// </summary>
    Failed = 5,

    /// <summary>
    /// The command was cancelled.
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// The command is pending a server-side transition.
    /// </summary>
    Pending = 7,
}
