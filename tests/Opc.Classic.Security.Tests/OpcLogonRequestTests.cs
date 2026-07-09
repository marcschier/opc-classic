// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Security.Tests;

public sealed class OpcLogonRequestTests
{
    [Test]
    public async Task Equality_UsesUserIdAndPassword()
    {
        var left = new OpcLogonRequest("operator", "correct");
        var right = new OpcLogonRequest("operator", "correct");
        var different = new OpcLogonRequest("operator", "other");

        var equal = left == right;
        var notEqual = left == different;

        await Assert.That(equal).IsTrue();
        await Assert.That(notEqual).IsFalse();
    }

    [Test]
    public async Task Constructor_RejectsNullOrEmptyUserId()
    {
        await Assert.That(() =>
        {
            _ = new OpcLogonRequest(null!, "password");
        }).Throws<ArgumentException>();

        await Assert.That(() =>
        {
            _ = new OpcLogonRequest(string.Empty, "password");
        }).Throws<ArgumentException>();
    }

    [Test]
    public async Task Constructor_RejectsNullPassword()
    {
        await Assert.That(() =>
        {
            _ = new OpcLogonRequest("operator", null!);
        }).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_AllowsEmptyPassword()
    {
        var request = new OpcLogonRequest("operator", string.Empty);

        await Assert.That(request.Password).IsEqualTo(string.Empty);
    }
}
