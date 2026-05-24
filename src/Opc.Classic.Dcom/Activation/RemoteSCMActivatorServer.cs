//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;
using SharpInterop.Common;

namespace SharpInterop.Core;

/// <summary>
/// Server-side IRemoteSCMActivator v5.6 implementation for RemoteCreateInstance
/// and RemoteGetClassObject.
/// </summary>
public sealed class RemoteSCMActivatorServer : IRemoteSCMActivatorServer {
    internal const int REGDB_E_CLASSNOTREG = unchecked((int)0x80040154u);
    internal const int E_NOTIMPL = unchecked((int)0x80004001u);
    internal const int CO_E_CLASSSTRING = unchecked((int)0x800401F3u);
    internal const int CLASS_E_CLASSNOTAVAILABLE = unchecked((int)0x80040111u);

    private static readonly Guid IidIUnknown = Guid.Parse(SharpInterop.Interfaces.IID_IUnknown);
    private static readonly Guid IidIClassFactory = Guid.Parse("00000001-0000-0000-C000-000000000046");
    private readonly IClsidRegistry? _metadataRegistry;
    private readonly ClassFactoryRegistry _classFactories;
    private readonly object _sessionLock = new();
    private Session? _serverSession;

    /// <summary>Initializes an activator backed by managed class factories.</summary>
    public RemoteSCMActivatorServer(ClassFactoryRegistry classFactories) {
        _classFactories = classFactories ?? throw new ArgumentNullException(nameof(classFactories));
    }

    /// <summary>
    /// Initializes the legacy metadata-only scaffold. Known CLSIDs still return
    /// E_NOTIMPL because no managed class factory was supplied.
    /// </summary>
    public RemoteSCMActivatorServer(IClsidRegistry registry) {
        _metadataRegistry = registry ?? throw new ArgumentNullException(nameof(registry));
        _classFactories = new ClassFactoryRegistry();
    }

    /// <summary>Initializes an activator with metadata and class factories.</summary>
    public RemoteSCMActivatorServer(IClsidRegistry registry, ClassFactoryRegistry classFactories) {
        _metadataRegistry = registry ?? throw new ArgumentNullException(nameof(registry));
        _classFactories = classFactories ?? throw new ArgumentNullException(nameof(classFactories));
    }

    /// <inheritdoc />
    public async Task<int> RemoteCreateInstanceAsync(
        Guid clsid,
        Guid requestedIid,
        CancellationToken cancellationToken = default) {
        var response = await RemoteCreateInstanceAsync(
            new RemoteCreateInstanceRequest(clsid, requestedIid, Array.Empty<int>()),
            cancellationToken).ConfigureAwait(false);
        return response.Hresult;
    }

    /// <inheritdoc />
    public async Task<int> RemoteGetClassObjectAsync(
        Guid clsid,
        Guid requestedIid,
        CancellationToken cancellationToken = default) {
        var response = await RemoteGetClassObjectAsync(
            new RemoteGetClassObjectRequest(clsid, requestedIid, Array.Empty<int>()),
            cancellationToken).ConfigureAwait(false);
        return response.Hresult;
    }

    /// <inheritdoc />
    public Task<RemoteCreateInstanceResponse> RemoteCreateInstanceAsync(
        RemoteCreateInstanceRequest request,
        CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        if (!_classFactories.TryResolve(request.Clsid, out IClassFactory factory)) {
            int hresult = ResolveMissingClass(request.Clsid);
            return Task.FromResult(new RemoteCreateInstanceResponse(hresult, Guid.Empty, Guid.Empty, Array.Empty<byte>()));
        }

        ActivationProperties activationProperties = ResolveActivationProperties(request.ActivationProperties, request.RawActivationProperties);
        Guid requestedIid = activationProperties.GetRequestedIidOr(request.RequestedIid == Guid.Empty ? IidIUnknown : request.RequestedIid);
        var context = new ClassFactoryActivationContext(request.Clsid, requestedIid, activationProperties);
        ClassFactoryActivationResult activationResult = factory.CreateInstance(context);
        LocalCoClass localCoClass = CreateLocalCoClass(activationResult);
        ExportedInterface exported = Export(localCoClass, requestedIid);
        var reply = new ScmReplyInfo(0, exported.Oxid, exported.Oid, exported.Ipid, exported.ObjRef, copy: true);
        ActivationProperties responseProperties = activationProperties.WithScmReplyInfo(reply);

        return Task.FromResult(new RemoteCreateInstanceResponse(0, exported.Oxid, exported.Ipid, exported.ObjRef) {
            Oid = exported.Oid,
            ActivationProperties = responseProperties,
            EncodedActivationProperties = ActivationInfoCodec.Encode(responseProperties),
        });
    }

    /// <inheritdoc />
    public Task<RemoteGetClassObjectResponse> RemoteGetClassObjectAsync(
        RemoteGetClassObjectRequest request,
        CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        if (!_classFactories.TryResolve(request.Clsid, out IClassFactory factory)) {
            int hresult = ResolveMissingClass(request.Clsid);
            return Task.FromResult(new RemoteGetClassObjectResponse(hresult, Guid.Empty, Guid.Empty, Array.Empty<byte>()));
        }

        if (!factory.SupportsGetClassObject) {
            return Task.FromResult(new RemoteGetClassObjectResponse(CLASS_E_CLASSNOTAVAILABLE, Guid.Empty, Guid.Empty, Array.Empty<byte>()));
        }

        ActivationProperties activationProperties = ResolveActivationProperties(request.ActivationProperties, request.RawActivationProperties);
        Guid requestedIid = request.RequestedIid == Guid.Empty ? IidIClassFactory : request.RequestedIid;
        var definition = new LocalInterfaceDefinition(IidIClassFactory.ToString(), isDispInterface: false);
        var localCoClass = new LocalCoClass(definition, factory, useInterfaceDefinitionIID: true);
        ExportedInterface exported = Export(localCoClass, requestedIid);
        var reply = new ScmReplyInfo(0, exported.Oxid, exported.Oid, exported.Ipid, exported.ObjRef, copy: true);
        ActivationProperties responseProperties = activationProperties.WithScmReplyInfo(reply);

        return Task.FromResult(new RemoteGetClassObjectResponse(0, exported.Oxid, exported.Ipid, exported.ObjRef) {
            Oid = exported.Oid,
            ActivationProperties = responseProperties,
            EncodedActivationProperties = ActivationInfoCodec.Encode(responseProperties),
        });
    }

    private int ResolveMissingClass(Guid clsid) {
        if (_metadataRegistry is null) {
            return CO_E_CLASSSTRING;
        }

        return _metadataRegistry.TryResolve(clsid, out _) ? E_NOTIMPL : REGDB_E_CLASSNOTREG;
    }

    private static ActivationProperties ResolveActivationProperties(
        ActivationProperties activationProperties,
        byte[] rawActivationProperties) {
        if (rawActivationProperties.Length == 0) {
            return activationProperties ?? ActivationProperties.Empty;
        }

        return ActivationInfoCodec.TryDecode(rawActivationProperties, out ActivationProperties decoded)
            ? decoded
            : activationProperties ?? ActivationProperties.Empty;
    }

    private static LocalCoClass CreateLocalCoClass(ClassFactoryActivationResult activationResult) {
        if (activationResult.Instance is LocalCoClass localCoClass) {
            return localCoClass;
        }

        return new LocalCoClass(
            activationResult.InterfaceDefinition,
            activationResult.Instance,
            useInterfaceDefinitionIID: true);
    }

    private ExportedInterface Export(LocalCoClass localCoClass, Guid requestedIid) {
        Session session = GetOrCreateServerSession();
        InterfacePointer pointer = ComOxidRuntime.Instance.GetInterfacePointer(session, localCoClass);
        byte[] objRef = EncodeObjRef(pointer, requestedIid == Guid.Empty ? IidIUnknown : requestedIid);
        return new ExportedInterface(
            GuidFromEightBytes(pointer.OXID),
            GuidFromEightBytes(pointer.OID),
            Guid.Parse(pointer.IPID),
            objRef);
    }

    private Session GetOrCreateServerSession() {
        lock (_sessionLock) {
            if (_serverSession is not null) {
                return _serverSession;
            }

            _serverSession = Session.CreateSession(new DefaultAuthInfoImpl(".", string.Empty, string.Empty));
            _serverSession.TargetServer = "127.0.0.1";
            return _serverSession;
        }
    }

    private static byte[] EncodeObjRef(InterfacePointer pointer, Guid iid) {
        var stdObjRef = (StdObjRef)pointer.GetObjectReference(InterfacePointer.OBJREF_STANDARD);
        byte[] dualStringArray = EncodeDualStringArray(pointer.StringBindings);
        var buffer = new byte[64 + dualStringArray.Length];
        var writer = new NdrWriter(buffer);
        writer.WriteUInt32(0x574F454Du); // MEOW
        writer.WriteUInt32(InterfacePointer.OBJREF_STANDARD);
        writer.WriteGuid(iid);
        writer.WriteUInt32((uint)stdObjRef.Flags);
        writer.WriteUInt32((uint)stdObjRef.PublicRefs);
        WriteEightByteLittleEndianOctetArray(ref writer, stdObjRef.Oxid);
        WriteEightByteLittleEndianOctetArray(ref writer, stdObjRef.ObjectId);
        writer.WriteGuid(Guid.Parse(stdObjRef.Ipid));
        writer.WriteRawBytes(dualStringArray);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static byte[] EncodeDualStringArray(DualStringArray dualStringArray) {
        var buffer = new byte[dualStringArray.Length];
        var ndrBuffer = new NdrBuffer(buffer, 0);
        var ndr = new NdrCodec { Buffer = ndrBuffer };
        dualStringArray.Encode(ndr);
        int length = Math.Max(ndrBuffer.Length, ndrBuffer.Index);
        return buffer.AsSpan(0, length).ToArray();
    }

    private static void WriteEightByteLittleEndianOctetArray(ref NdrWriter writer, byte[] bytes) {
        if (bytes.Length != 8) {
            throw new InvalidOperationException("DCOM OXID/OID values must be 8 bytes.");
        }

        for (int i = bytes.Length - 1; i >= 0; i--) {
            writer.WriteByte(bytes[i]);
        }
    }

    private static Guid GuidFromEightBytes(byte[] bytes) {
        Span<byte> guidBytes = stackalloc byte[16];
        int count = Math.Min(bytes.Length, 8);
        for (int i = 0; i < count; i++) {
            guidBytes[i] = bytes[i];
        }

        return new Guid(guidBytes);
    }

    private sealed record ExportedInterface(Guid Oxid, Guid Oid, Guid Ipid, byte[] ObjRef);
}
