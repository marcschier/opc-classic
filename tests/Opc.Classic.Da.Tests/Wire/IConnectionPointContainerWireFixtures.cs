// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;

namespace Opc.Classic.Da.Tests.Wire;

public sealed class IConnectionPointContainerWireFixtures
{
    [Test]
    public async Task FindConnectionPoint_generated_proxy_and_dispatcher_use_MInterfacePointer_framing()
    {
        var dispatcher = new IConnectionPointContainerServerDispatcher(new ConnectionPointContainerStub());
        ReadOnlyMemory<byte> responsePayload = ReadOnlyMemory<byte>.Empty;
        var channel = new InMemoryCallChannel(async (iid, opnum, payload, cancellationToken) =>
        {
            await Assert.That(iid).IsEqualTo(IConnectionPointContainer.InterfaceId);
            DispatchResult result = await dispatcher.DispatchAsync(opnum, payload, cancellationToken);
            responsePayload = result.Payload;
            return result.ToNdrCallResult();
        });

        IOpcInterfaceRef result = await new IConnectionPointContainerClientProxy(channel)
            .FindConnectionPointAsync(IOPCDataCallback.InterfaceId, CancellationToken.None);

        await Assert.That(result.Iid).IsEqualTo(IConnectionPoint.InterfaceId);
        await Assert.That(Convert.ToHexString(responsePayload.Span[..16]))
            .IsEqualTo("0000020044000000440000004D454F57");
    }

    private sealed class ConnectionPointContainerStub : IConnectionPointContainer
    {
        public Task<IOpcInterfaceRef> EnumConnectionPointsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateInterfaceRef(Guid.Parse("B196B285-BAB4-101A-B69C-00AA00341D07")));

        public Task<IOpcInterfaceRef> FindConnectionPointAsync(Guid iid, CancellationToken cancellationToken = default)
        {
            if (iid != IOPCDataCallback.InterfaceId)
            {
                throw new InvalidOperationException("Unexpected connection-point IID.");
            }

            return Task.FromResult(CreateInterfaceRef(IConnectionPoint.InterfaceId));
        }

        private static IOpcInterfaceRef CreateInterfaceRef(Guid iid) =>
            new OpcInterfaceRef(
                iid,
                flags: 0,
                publicRefs: 1,
                oxid: 2,
                oid: 3,
                ipid: Guid.Parse("B196B286-BAB4-101A-B69C-00AA00341D08"),
                securityOffset: 0,
                resolverBindings: []);
    }
}
