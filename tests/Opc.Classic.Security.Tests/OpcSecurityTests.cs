// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Security.Tests;

internal sealed class FakeOpcSecurity : IOpcSecurity
{
    public bool SupportsWindowsAuthentication => true;
    public bool SupportsPrivateAuthentication => true;
    public bool IsAuthenticated { get; private set; }
    public string CurrentIdentity { get; private set; } = string.Empty;

    public Task<bool> LoginAsCurrentUserAsync(CancellationToken ct = default)
    {
        IsAuthenticated = true;
        CurrentIdentity = "CORP\\alice";
        return Task.FromResult(true);
    }

    public Task<bool> LoginPrivateAsync(string username, string password, CancellationToken ct = default)
    {
        if (password == "correct")
        {
            IsAuthenticated = true;
            CurrentIdentity = $"private:{username}";
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task LogoutAsync(CancellationToken ct = default)
    {
        IsAuthenticated = false;
        CurrentIdentity = string.Empty;
        return Task.CompletedTask;
    }
}

public sealed class IOpcSecurityContractTests
{
    [Test]
    public async Task BeforeLogin_NotAuthenticated()
    {
        var sec = new FakeOpcSecurity();
        await Assert.That(sec.IsAuthenticated).IsFalse();
        await Assert.That(sec.CurrentIdentity).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task LoginAsCurrentUser_SetsAuthenticated()
    {
        var sec = new FakeOpcSecurity();
        var success = await sec.LoginAsCurrentUserAsync();
        await Assert.That(success).IsTrue();
        await Assert.That(sec.IsAuthenticated).IsTrue();
        await Assert.That(sec.CurrentIdentity).Contains("alice");
    }

    [Test]
    public async Task LoginPrivate_CorrectPassword_Succeeds()
    {
        var sec = new FakeOpcSecurity();
        var success = await sec.LoginPrivateAsync("operator", "correct");
        await Assert.That(success).IsTrue();
        await Assert.That(sec.CurrentIdentity).IsEqualTo("private:operator");
    }

    [Test]
    public async Task LoginPrivate_BadPassword_Fails()
    {
        var sec = new FakeOpcSecurity();
        var success = await sec.LoginPrivateAsync("operator", "wrong");
        await Assert.That(success).IsFalse();
        await Assert.That(sec.IsAuthenticated).IsFalse();
    }

    [Test]
    public async Task Logout_ResetsAuthState()
    {
        var sec = new FakeOpcSecurity();
        await sec.LoginAsCurrentUserAsync();
        await sec.LogoutAsync();
        await Assert.That(sec.IsAuthenticated).IsFalse();
        await Assert.That(sec.CurrentIdentity).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Capabilities_AreReadOnly()
    {
        var sec = new FakeOpcSecurity();
        await Assert.That(sec.SupportsWindowsAuthentication).IsTrue();
        await Assert.That(sec.SupportsPrivateAuthentication).IsTrue();
    }
}
