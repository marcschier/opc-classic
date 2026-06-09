//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace Opc.Classic.Commands;

/// <summary>
/// Describes a command exposed by an OPC Commands server.
/// </summary>
public sealed record CommandDescription {
    private readonly string[] _inputArguments;
    private readonly string[] _returnArguments;

    /// <summary>Create a command description.</summary>
    public CommandDescription(
        string commandName,
        string commandCategory,
        string commandHelp,
        int commandResultCount,
        Guid categoryId,
        IReadOnlyList<string> inputArguments,
        IReadOnlyList<string> returnArguments) {
        ArgumentNullException.ThrowIfNull(commandName);
        ArgumentNullException.ThrowIfNull(commandCategory);
        ArgumentNullException.ThrowIfNull(commandHelp);

        CommandName = commandName;
        CommandCategory = commandCategory;
        CommandHelp = commandHelp;
        CommandResultCount = commandResultCount;
        CategoryId = categoryId;
        _inputArguments = CopyArguments(inputArguments, nameof(inputArguments));
        _returnArguments = CopyArguments(returnArguments, nameof(returnArguments));
        InputArguments = Array.AsReadOnly(_inputArguments);
        ReturnArguments = Array.AsReadOnly(_returnArguments);
    }

    /// <summary>Command name used for invocation.</summary>
    public string CommandName { get; }

    /// <summary>Human-readable command category.</summary>
    public string CommandCategory { get; }

    /// <summary>Help text supplied by the server for operators or clients.</summary>
    public string CommandHelp { get; }

    /// <summary>Expected number of result items the command produces.</summary>
    public int CommandResultCount { get; }

    /// <summary>Stable category identifier from the Commands metadata.</summary>
    public Guid CategoryId { get; }

    /// <summary>Command input argument names, in wire order.</summary>
    public IReadOnlyList<string> InputArguments { get; }

    /// <summary>Command return argument names, in wire order.</summary>
    public IReadOnlyList<string> ReturnArguments { get; }

    /// <inheritdoc />
    public bool Equals(CommandDescription? other) =>
        other is not null
        && StringComparer.Ordinal.Equals(CommandName, other.CommandName)
        && StringComparer.Ordinal.Equals(CommandCategory, other.CommandCategory)
        && StringComparer.Ordinal.Equals(CommandHelp, other.CommandHelp)
        && CommandResultCount == other.CommandResultCount
        && CategoryId == other.CategoryId
        && _inputArguments.SequenceEqual(other._inputArguments, StringComparer.Ordinal)
        && _returnArguments.SequenceEqual(other._returnArguments, StringComparer.Ordinal);

    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(CommandName, StringComparer.Ordinal);
        hash.Add(CommandCategory, StringComparer.Ordinal);
        hash.Add(CommandHelp, StringComparer.Ordinal);
        hash.Add(CommandResultCount);
        hash.Add(CategoryId);

        foreach (var inputArgument in _inputArguments) {
            hash.Add(inputArgument, StringComparer.Ordinal);
        }

        foreach (var returnArgument in _returnArguments) {
            hash.Add(returnArgument, StringComparer.Ordinal);
        }

        return hash.ToHashCode();
    }

    private static string[] CopyArguments(IReadOnlyList<string> arguments, string parameterName) {
        ArgumentNullException.ThrowIfNull(arguments, parameterName);

        var copy = new string[arguments.Count];
        for (var i = 0; i < arguments.Count; i++) {
            copy[i] = arguments[i] ?? throw new ArgumentException("Command argument names cannot be null.", parameterName);
        }

        return copy;
    }
}
