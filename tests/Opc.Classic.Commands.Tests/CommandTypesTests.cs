// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Commands.Tests;

public sealed class CommandDescriptionTests
{
    [Test]
    public async Task ValueEquality_IncludesArgumentSequences()
    {
        var categoryId = Guid.NewGuid();
        var first = new CommandDescription(
            "StartMotor",
            "Motor",
            "Starts the selected motor.",
            commandResultCount: 1,
            categoryId,
            new[] { "Target", "Speed" },
            new[] { "Accepted" });

        var second = new CommandDescription(
            "StartMotor",
            "Motor",
            "Starts the selected motor.",
            commandResultCount: 1,
            categoryId,
            new[] { "Target", "Speed" },
            new[] { "Accepted" });

        await Assert.That(first).IsEqualTo(second);
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
    }

    [Test]
    public async Task Constructor_RejectsNullReferenceInputs()
    {
        var arguments = Array.Empty<string>();

        await Assert.That(() => { _ = new CommandDescription(null!, "Category", "Help", 0, Guid.Empty, arguments, arguments); })
            .Throws<ArgumentNullException>();

        await Assert.That(() => { _ = new CommandDescription("Name", null!, "Help", 0, Guid.Empty, arguments, arguments); })
            .Throws<ArgumentNullException>();

        await Assert.That(() => { _ = new CommandDescription("Name", "Category", null!, 0, Guid.Empty, arguments, arguments); })
            .Throws<ArgumentNullException>();

        await Assert.That(() => { _ = new CommandDescription("Name", "Category", "Help", 0, Guid.Empty, null!, arguments); })
            .Throws<ArgumentNullException>();

        await Assert.That(() => { _ = new CommandDescription("Name", "Category", "Help", 0, Guid.Empty, arguments, null!); })
            .Throws<ArgumentNullException>();

        await Assert.That(() => { _ = new CommandDescription("Name", "Category", "Help", 0, Guid.Empty, new List<string> { null! }, arguments); })
            .Throws<ArgumentException>();
    }
}

public sealed class CommandStateTests
{
    [Test]
    public async Task EnumValues_MatchOpcCommandsSpecification()
    {
        var created = (int)CommandState.Created;
        var queued = (int)CommandState.Queued;
        var executing = (int)CommandState.Executing;
        var complete = (int)CommandState.Complete;
        var failed = (int)CommandState.Failed;
        var cancelled = (int)CommandState.Cancelled;
        var pending = (int)CommandState.Pending;

        await Assert.That(created).IsEqualTo(1);
        await Assert.That(queued).IsEqualTo(2);
        await Assert.That(executing).IsEqualTo(3);
        await Assert.That(complete).IsEqualTo(4);
        await Assert.That(failed).IsEqualTo(5);
        await Assert.That(cancelled).IsEqualTo(6);
        await Assert.That(pending).IsEqualTo(7);
    }
}

public sealed class CommandInvocationTests
{
    [Test]
    public async Task RecordRoundTrip_PreservesInvocationSnapshot()
    {
        var invocationId = Guid.NewGuid();
        var clientHandle = Guid.NewGuid();
        var timestamp = DateTimeOffset.UtcNow;

        var invocation = new CommandInvocation(
            invocationId,
            clientHandle,
            "PlantServer",
            CommandState.Executing,
            Hresult: 0,
            timestamp);

        var roundTrip = invocation with { };

        await Assert.That(roundTrip).IsEqualTo(invocation);
        await Assert.That(roundTrip.InvocationId).IsEqualTo(invocationId);
        await Assert.That(roundTrip.ClientHandle).IsEqualTo(clientHandle);
        await Assert.That(roundTrip.ServerName).IsEqualTo("PlantServer");
        await Assert.That(roundTrip.State).IsEqualTo(CommandState.Executing);
        await Assert.That(roundTrip.Hresult).IsEqualTo(0);
        await Assert.That(roundTrip.StateTimestamp).IsEqualTo(timestamp);
    }

    [Test]
    public async Task Constructor_RejectsNullServerName()
    {
        await Assert.That(() => { _ = new CommandInvocation(Guid.NewGuid(), Guid.NewGuid(), null!, CommandState.Created, 0, DateTimeOffset.UtcNow); })
            .Throws<ArgumentNullException>();
    }
}

public sealed class CommandStateChangeTests
{
    [Test]
    public async Task RecordRoundTrip_PreservesCallbackEvent()
    {
        var invocationId = Guid.NewGuid();
        var timestamp = DateTimeOffset.UtcNow;
        var change = new CommandStateChange(invocationId, CommandState.Complete, Hresult: 0, timestamp);
        var roundTrip = change with { };

        await Assert.That(roundTrip).IsEqualTo(change);
        await Assert.That(roundTrip.InvocationId).IsEqualTo(invocationId);
        await Assert.That(roundTrip.NewState).IsEqualTo(CommandState.Complete);
        await Assert.That(roundTrip.Hresult).IsEqualTo(0);
        await Assert.That(roundTrip.Timestamp).IsEqualTo(timestamp);
    }
}
