//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// byte-exact server-write fixtures. Loops back through the
// generated *ServerDispatcher classes and asserts the response payload
// matches the canonical MIDL wire layout — symmetric counterpart to AL2.
// If a generator change ever flips a byte unilaterally on the server side
// (without matching the proxy decode), these fixtures fail with a hex diff.
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Wire;

public sealed class ServerDispatchWireFixtureTests
{
    /// <summary>
    /// IConnectionPoint::GetConnectionInterface response: a single GUID
    /// (16 bytes) emitted with no leading referent / max_count. Confirms the
    /// dispatcher's response encoder matches the proxy's decoder (which
    /// reads <see cref="NdrReader.ReadGuid"/> directly).
    /// </summary>
    [Test]
    public async Task ConnectionPoint_GetConnectionInterface_EmitsBareGuid()
    {
        var iid = Guid.Parse("39C13A70-011E-11D0-9675-0020AFD8ADB3"); // IID_IOPCDataCallback
        var impl = new StubConnectionPoint { ConnectionIid = iid };
        var dispatcher = new IConnectionPointServerDispatcher(impl);

        DispatchResult result = await dispatcher.DispatchAsync(
            IConnectionPoint.Opnums.GetConnectionInterfaceAsync,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        // 16 bytes, no padding. GUID little-endian byte order matches Guid.ToByteArray().
        byte[] payload = result.Payload.ToArray();
        await Assert.That(payload.Length).IsEqualTo(16);
        byte[] expected = iid.ToByteArray();
        for (int i = 0; i < 16; i++)
        {
            await Assert.That(payload[i]).IsEqualTo(expected[i]);
        }
    }

    private sealed class StubConnectionPoint : IConnectionPoint
    {
        public Guid ConnectionIid { get; set; }
        public int NextCookie { get; set; }

        public Task<Guid> GetConnectionInterfaceAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(ConnectionIid);

        public Task<int> AdviseAsync(global::Opc.Classic.Dcom.IOpcInterfaceRef sink, CancellationToken cancellationToken = default) =>
            Task.FromResult(NextCookie);

        public Task UnadviseAsync(int cookie, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
