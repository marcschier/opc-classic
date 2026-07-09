// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Opc.Classic.Transport;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Tests.Transport;

public sealed class AsyncTransportFactoryExtensionsTests
{
    [Test]
    public async Task AddAsyncTransport_ReturnsSameCollectionWithoutRegisteringDescriptors()
    {
        IServiceCollection services = new ServiceCollection();

        IServiceCollection returned = services.AddAsyncTransport();

        await Assert.That(returned).IsSameReferenceAs(services);
        await Assert.That(services.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AddAsyncTransport_NullServices_ThrowsArgumentNullException()
    {
        await Assert.That(() => AsyncTransportFactoryExtensions.AddAsyncTransport(null!))
            .Throws<ArgumentNullException>();
    }
}
