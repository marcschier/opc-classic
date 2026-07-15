// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

#pragma warning disable MA0048 // Enumerator state, snapshots, wire codecs, and dispatchers are one implementation unit.

using System.Buffers;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Hosting;

internal static class OpcDaGroupEnumeratorFactory
{
    public static IOpcInterfaceRef CreateString(
        IReadOnlyList<string> names,
        OpcObjectRegistry registry)
    {
        var enumerator = new OpcDaStringEnumerator(names, registry);
        return RegisterString(enumerator, registry);
    }

    public static IOpcInterfaceRef CreateUnknown(
        IReadOnlyList<OpcDaGroup> groups,
        OpcObjectRegistry registry)
    {
        var snapshot = new OpcDaUnknownSnapshot(groups, registry);
        var enumerator = new OpcDaUnknownEnumerator(snapshot, registry);
        return RegisterUnknown(enumerator, registry);
    }

    internal static IOpcInterfaceRef RegisterString(
        OpcDaStringEnumerator enumerator,
        OpcObjectRegistry registry)
    {
        try
        {
            Guid ipid = registry.Register(
                new Dictionary<Guid, IOpcServerDispatcher>
                {
                    [IEnumString.InterfaceId] = new OpcDaEnumStringServerDispatcher(enumerator),
                },
                publicRefs: 1,
                finalRelease: enumerator.Dispose);
            return CreateRef(IEnumString.InterfaceId, ipid);
        }
        catch
        {
            enumerator.Dispose();
            throw;
        }
    }

    internal static IOpcInterfaceRef RegisterUnknown(
        OpcDaUnknownEnumerator enumerator,
        OpcObjectRegistry registry)
    {
        try
        {
            Guid ipid = registry.Register(
                new Dictionary<Guid, IOpcServerDispatcher>
                {
                    [IEnumUnknown.InterfaceId] = new OpcDaEnumUnknownServerDispatcher(enumerator),
                },
                publicRefs: 1,
                finalRelease: enumerator.Dispose);
            return CreateRef(IEnumUnknown.InterfaceId, ipid);
        }
        catch
        {
            enumerator.Dispose();
            throw;
        }
    }

    internal static IOpcInterfaceRef CreateRef(Guid iid, Guid ipid, ulong oid = 0) =>
        new OpcInterfaceRef(
            iid,
            flags: 0,
            publicRefs: 1,
            oxid: 1,
            oid,
            ipid,
            securityOffset: 0,
            resolverBindings: Array.Empty<ushort>());
}

internal sealed class OpcDaStringEnumerator : IEnumString, IDisposable
{
    private readonly string[] _snapshot;
    private readonly OpcObjectRegistry _registry;
    private readonly Lock _gate = new();
    private int _cursor;
    private bool _disposed;

    public OpcDaStringEnumerator(
        IReadOnlyList<string> snapshot,
        OpcObjectRegistry registry,
        int cursor = 0)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _snapshot = snapshot.ToArray();
        _cursor = cursor;
    }

    public Task NextStringsAsync(
        int count,
        out string[] values,
        out int fetchedCount,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            int take = Math.Min(Math.Max(count, 0), _snapshot.Length - _cursor);
            values = new string[take];
            Array.Copy(_snapshot, _cursor, values, 0, take);
            _cursor += take;
            fetchedCount = take;
            return Task.CompletedTask;
        }
    }

    public async Task SkipAsync(int count, CancellationToken cancellationToken = default)
    {
        _ = await SkipWithCountAsync(count, cancellationToken).ConfigureAwait(false);
    }

    internal Task<int> SkipWithCountAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            int skipped = Math.Min(Math.Max(count, 0), _snapshot.Length - _cursor);
            _cursor += skipped;
            return Task.FromResult(skipped);
        }
    }

    public Task ResetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _cursor = 0;
            return Task.CompletedTask;
        }
    }

    public Task<IOpcInterfaceRef> CloneAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var clone = new OpcDaStringEnumerator(_snapshot, _registry, _cursor);
            return Task.FromResult(OpcDaGroupEnumeratorFactory.RegisterString(clone, _registry));
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _disposed = true;
        }
    }
}

internal sealed class OpcDaUnknownEnumerator : IEnumUnknown, IDisposable
{
    private readonly OpcDaUnknownSnapshot _snapshot;
    private readonly OpcObjectRegistry _registry;
    private readonly Lock _gate = new();
    private int _cursor;
    private bool _disposed;

    public OpcDaUnknownEnumerator(
        OpcDaUnknownSnapshot snapshot,
        OpcObjectRegistry registry,
        int cursor = 0,
        bool addSnapshotReference = false)
    {
        _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _cursor = cursor;
        if (addSnapshotReference)
        {
            _snapshot.AddEnumerator();
        }
    }

    public Task NextUnknownsAsync(
        int count,
        out IOpcInterfaceRef[] values,
        out int fetchedCount,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            values = _snapshot.Fetch(_cursor, Math.Max(count, 0));
            _cursor += values.Length;
            fetchedCount = values.Length;
            return Task.CompletedTask;
        }
    }

    public async Task SkipAsync(int count, CancellationToken cancellationToken = default)
    {
        _ = await SkipWithCountAsync(count, cancellationToken).ConfigureAwait(false);
    }

    internal Task<int> SkipWithCountAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            int skipped = Math.Min(Math.Max(count, 0), _snapshot.Length - _cursor);
            _cursor += skipped;
            return Task.FromResult(skipped);
        }
    }

    public Task ResetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _cursor = 0;
            return Task.CompletedTask;
        }
    }

    public Task<IOpcInterfaceRef> CloneAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var clone = new OpcDaUnknownEnumerator(
                _snapshot,
                _registry,
                _cursor,
                addSnapshotReference: true);
            return Task.FromResult(OpcDaGroupEnumeratorFactory.RegisterUnknown(clone, _registry));
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
        }
        _snapshot.ReleaseEnumerator();
    }
}

internal sealed class OpcDaUnknownSnapshot
{
    private readonly IOpcInterfaceRef[] _groups;
    private readonly OpcObjectRegistry _registry;
    private int _enumerators = 1;

    public OpcDaUnknownSnapshot(
        IReadOnlyList<OpcDaGroup> groups,
        OpcObjectRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(groups);
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _groups = new IOpcInterfaceRef[groups.Count];
        int registered = 0;
        try
        {
            for (; registered < groups.Count; registered++)
            {
                OpcDaGroup group = groups[registered]
                    ?? throw new ArgumentException("Group snapshots must not contain null entries.", nameof(groups));
                Guid ipid = registry.Register(CreateGroupDispatchers(group), publicRefs: 1);
                _groups[registered] = OpcDaGroupEnumeratorFactory.CreateRef(
                    new Guid("00000000-0000-0000-C000-000000000046"),
                    ipid,
                    unchecked((ulong)group.ServerHandle));
            }
        }
        catch
        {
            for (int i = 0; i < registered; i++)
            {
                registry.ReleasePublicRefs(_groups[i].Ipid, 1);
            }
            throw;
        }
    }

    public int Length => _groups.Length;

    public void AddEnumerator() => Interlocked.Increment(ref _enumerators);

    public void ReleaseEnumerator()
    {
        if (Interlocked.Decrement(ref _enumerators) != 0)
        {
            return;
        }
        foreach (IOpcInterfaceRef group in _groups)
        {
            _registry.ReleasePublicRefs(group.Ipid, 1);
        }
    }

    public IOpcInterfaceRef[] Fetch(int cursor, int count)
    {
        int take = Math.Min(count, _groups.Length - cursor);
        var values = new IOpcInterfaceRef[take];
        int added = 0;
        try
        {
            for (; added < take; added++)
            {
                IOpcInterfaceRef group = _groups[cursor + added];
                if (!_registry.AddPublicRefs(group.Ipid, 1))
                {
                    throw new OpcException(new OpcResultId(unchecked((int)0x80010108), "RPC_E_DISCONNECTED"));
                }
                values[added] = group;
            }
            return values;
        }
        catch
        {
            for (int i = 0; i < added; i++)
            {
                _registry.ReleasePublicRefs(values[i].Ipid, 1);
            }
            throw;
        }
    }

    private static IReadOnlyDictionary<Guid, IOpcServerDispatcher> CreateGroupDispatchers(OpcDaGroup group) =>
        new Dictionary<Guid, IOpcServerDispatcher>
        {
            [IOPCGroupStateMgt.InterfaceId] = new IOPCGroupStateMgtServerDispatcher(group),
            [IOPCGroupStateMgt2.InterfaceId] = new IOPCGroupStateMgt2ServerDispatcher(group),
            [IOPCItemMgt.InterfaceId] = new IOPCItemMgtServerDispatcher(group),
            [IOPCSyncIO.InterfaceId] = new IOPCSyncIOServerDispatcher(group),
            [IOPCSyncIO2.InterfaceId] = new IOPCSyncIO2ServerDispatcher(group),
            [IOPCAsyncIO2.InterfaceId] = new IOPCAsyncIO2ServerDispatcher(group),
            [IOPCAsyncIO3.InterfaceId] = new IOPCAsyncIO3ServerDispatcher(group),
            [IConnectionPoint.InterfaceId] = new IConnectionPointServerDispatcher(group),
            [IConnectionPointContainer.InterfaceId] = new IConnectionPointContainerServerDispatcher(group),
            [IOPCItemDeadbandMgt.InterfaceId] = new IOPCItemDeadbandMgtServerDispatcher(group),
            [IOPCItemSamplingMgt.InterfaceId] = new IOPCItemSamplingMgtServerDispatcher(group),
        };
}

internal sealed class OpcDaEnumStringServerDispatcher : IOpcServerDispatcher
{
    private readonly OpcDaStringEnumerator _enumerator;

    public OpcDaEnumStringServerDispatcher(OpcDaStringEnumerator enumerator) =>
        _enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));

    public ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default) =>
        OpcDaEnumeratorWire.DispatchStringAsync(_enumerator, opnum, requestPayload, cancellationToken);
}

internal sealed class OpcDaEnumUnknownServerDispatcher : IOpcServerDispatcher
{
    private readonly OpcDaUnknownEnumerator _enumerator;

    public OpcDaEnumUnknownServerDispatcher(OpcDaUnknownEnumerator enumerator) =>
        _enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));

    public ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default) =>
        OpcDaEnumeratorWire.DispatchUnknownAsync(_enumerator, opnum, requestPayload, cancellationToken);
}

internal static class OpcDaEnumeratorWire
{
    private const int EInvalidArg = unchecked((int)0x80070057);

    public static async ValueTask<DispatchResult> DispatchStringAsync(
        OpcDaStringEnumerator enumerator,
        int opnum,
        ReadOnlyMemory<byte> request,
        CancellationToken cancellationToken)
    {
        try
        {
            return opnum switch
            {
                3 => await NextStringAsync(enumerator, request, cancellationToken).ConfigureAwait(false),
                4 => await SkipStringAsync(enumerator, request, cancellationToken).ConfigureAwait(false),
                5 => await ResetStringAsync(enumerator, cancellationToken).ConfigureAwait(false),
                6 => await CloneStringAsync(enumerator, cancellationToken).ConfigureAwait(false),
                _ => DispatchResult.NotImplemented(opnum),
            };
        }
        catch (OpcException exception)
        {
            return DispatchResult.Fault(exception.ResultId.Code);
        }
        catch (InvalidOperationException)
        {
            return DispatchResult.Fault(EInvalidArg);
        }
    }

    public static async ValueTask<DispatchResult> DispatchUnknownAsync(
        OpcDaUnknownEnumerator enumerator,
        int opnum,
        ReadOnlyMemory<byte> request,
        CancellationToken cancellationToken)
    {
        try
        {
            return opnum switch
            {
                3 => await NextUnknownAsync(enumerator, request, cancellationToken).ConfigureAwait(false),
                4 => await SkipUnknownAsync(enumerator, request, cancellationToken).ConfigureAwait(false),
                5 => await ResetUnknownAsync(enumerator, cancellationToken).ConfigureAwait(false),
                6 => await CloneUnknownAsync(enumerator, cancellationToken).ConfigureAwait(false),
                _ => DispatchResult.NotImplemented(opnum),
            };
        }
        catch (OpcException exception)
        {
            return DispatchResult.Fault(exception.ResultId.Code);
        }
        catch (InvalidOperationException)
        {
            return DispatchResult.Fault(EInvalidArg);
        }
    }

    private static async ValueTask<DispatchResult> NextStringAsync(
        OpcDaStringEnumerator enumerator,
        ReadOnlyMemory<byte> request,
        CancellationToken cancellationToken)
    {
        int count = ReadCount(request);
        await enumerator.NextStringsAsync(count, out string[] values, out int fetched, cancellationToken).ConfigureAwait(false);
        byte[] payload = WritePayload((ref NdrWriter writer) =>
        {
            WriteVaryingHeader(ref writer, count, fetched);
            for (int i = 0; i < fetched; i++)
            {
                _ = writer.WriteReferentId();
            }
            for (int i = 0; i < fetched; i++)
            {
                writer.WriteUnicodeString(values[i]);
            }
            writer.WriteUInt32((uint)fetched);
        });
        return DispatchResult.Success(payload, fetched == count ? OpcResultId.Ok.Code : OpcResultId.False.Code);
    }

    private static async ValueTask<DispatchResult> NextUnknownAsync(
        OpcDaUnknownEnumerator enumerator,
        ReadOnlyMemory<byte> request,
        CancellationToken cancellationToken)
    {
        int count = ReadCount(request);
        await enumerator.NextUnknownsAsync(count, out IOpcInterfaceRef[] values, out int fetched, cancellationToken).ConfigureAwait(false);
        byte[] payload = WritePayload((ref NdrWriter writer) =>
        {
            WriteVaryingHeader(ref writer, count, fetched);
            for (int i = 0; i < fetched; i++)
            {
                _ = writer.WriteReferentId();
            }
            for (int i = 0; i < fetched; i++)
            {
                WriteMInterfacePointerBody(ref writer, values[i]);
            }
            writer.WriteUInt32((uint)fetched);
        });
        return DispatchResult.Success(payload, fetched == count ? OpcResultId.Ok.Code : OpcResultId.False.Code);
    }

    private static async ValueTask<DispatchResult> SkipStringAsync(
        OpcDaStringEnumerator enumerator,
        ReadOnlyMemory<byte> request,
        CancellationToken cancellationToken)
    {
        int count = ReadCount(request);
        int skipped = await enumerator.SkipWithCountAsync(count, cancellationToken).ConfigureAwait(false);
        return DispatchResult.Success(
            ReadOnlyMemory<byte>.Empty,
            skipped == count ? OpcResultId.Ok.Code : OpcResultId.False.Code);
    }

    private static async ValueTask<DispatchResult> SkipUnknownAsync(
        OpcDaUnknownEnumerator enumerator,
        ReadOnlyMemory<byte> request,
        CancellationToken cancellationToken)
    {
        int count = ReadCount(request);
        int skipped = await enumerator.SkipWithCountAsync(count, cancellationToken).ConfigureAwait(false);
        return DispatchResult.Success(
            ReadOnlyMemory<byte>.Empty,
            skipped == count ? OpcResultId.Ok.Code : OpcResultId.False.Code);
    }

    private static async ValueTask<DispatchResult> ResetStringAsync(
        OpcDaStringEnumerator enumerator,
        CancellationToken cancellationToken)
    {
        await enumerator.ResetAsync(cancellationToken).ConfigureAwait(false);
        return DispatchResult.Success(ReadOnlyMemory<byte>.Empty);
    }

    private static async ValueTask<DispatchResult> ResetUnknownAsync(
        OpcDaUnknownEnumerator enumerator,
        CancellationToken cancellationToken)
    {
        await enumerator.ResetAsync(cancellationToken).ConfigureAwait(false);
        return DispatchResult.Success(ReadOnlyMemory<byte>.Empty);
    }

    private static async ValueTask<DispatchResult> CloneStringAsync(
        OpcDaStringEnumerator enumerator,
        CancellationToken cancellationToken)
    {
        IOpcInterfaceRef clone = await enumerator.CloneAsync(cancellationToken).ConfigureAwait(false);
        return DispatchResult.Success(WritePayload((ref NdrWriter writer) =>
            OpcMInterfacePointerCodec.Write(ref writer, clone)));
    }

    private static async ValueTask<DispatchResult> CloneUnknownAsync(
        OpcDaUnknownEnumerator enumerator,
        CancellationToken cancellationToken)
    {
        IOpcInterfaceRef clone = await enumerator.CloneAsync(cancellationToken).ConfigureAwait(false);
        return DispatchResult.Success(WritePayload((ref NdrWriter writer) =>
            OpcMInterfacePointerCodec.Write(ref writer, clone)));
    }

    private static int ReadCount(ReadOnlyMemory<byte> request)
    {
        var reader = new NdrReader(request.Span);
        uint count = reader.ReadUInt32();
        if (count > int.MaxValue)
        {
            throw new InvalidOperationException("Enumerator count exceeds the supported managed range.");
        }
        return (int)count;
    }

    private static void WriteVaryingHeader(ref NdrWriter writer, int requested, int fetched)
    {
        writer.WriteUInt32((uint)requested);
        writer.WriteUInt32(0);
        writer.WriteUInt32((uint)fetched);
    }

    private static void WriteMInterfacePointerBody(ref NdrWriter writer, IOpcInterfaceRef interfaceRef)
    {
        byte[] scratch = ArrayPool<byte>.Shared.Rent(1024);
        try
        {
            while (true)
            {
                try
                {
                    var inner = new NdrWriter(scratch);
                    OpcInterfaceRefCodec.Write(ref inner, interfaceRef);
                    writer.WriteUInt32((uint)inner.Position);
                    writer.WriteUInt32((uint)inner.Position);
                    writer.WriteRawBytes(scratch.AsSpan(0, inner.Position));
                    return;
                }
                catch (InvalidOperationException)
                {
                    ArrayPool<byte>.Shared.Return(scratch);
                    scratch = ArrayPool<byte>.Shared.Rent(scratch.Length * 2);
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(scratch);
        }
    }

    private static byte[] WritePayload(NdrWriteAction write)
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);
        try
        {
            while (true)
            {
                try
                {
                    var writer = new NdrWriter(buffer);
                    write(ref writer);
                    return buffer.AsSpan(0, writer.Position).ToArray();
                }
                catch (InvalidOperationException)
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                    buffer = ArrayPool<byte>.Shared.Rent(buffer.Length * 2);
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);
}
