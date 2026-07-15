// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Rpc.Auth;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dcom.Tests.Auth;

public sealed class RpcServerAuthenticationProviderRegistryTests
{
    [Test]
    public async Task Registry_selects_provider_by_authentication_service()
    {
        var provider = new StubProvider(42);
        var registry = new RpcServerAuthenticationProviderRegistry([provider]);

        bool found = registry.TryGetProvider(42, out IRpcServerAuthenticationProvider? selected);
        bool missing = registry.TryGetProvider(43, out _);

        await Assert.That(registry.HasProviders).IsTrue();
        await Assert.That(found).IsTrue();
        await Assert.That(selected).IsSameReferenceAs(provider);
        await Assert.That(missing).IsFalse();
    }

    [Test]
    public async Task Registry_rejects_duplicate_authentication_service()
    {
        var registry = new RpcServerAuthenticationProviderRegistry([new StubProvider(42)]);

        await Assert.That(() => registry.Register(new StubProvider(42)))
            .Throws<InvalidOperationException>();
    }

    private sealed class StubProvider : IRpcServerAuthenticationProvider
    {
        public StubProvider(int authenticationService) =>
            AuthenticationService = authenticationService;

        public int AuthenticationService { get; }

        public IRpcServerAuthenticationAcceptor CreateAcceptor() =>
            throw new InvalidOperationException("Not used by registry tests.");
    }
}
