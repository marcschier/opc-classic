// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Partially implements IOxIDResolver client calls, used only for ResolveOxid.
/// </summary>
// Security audit (date: 2026-05-22):
//   - Authentication: this client-side NdrOp does not accept incoming Ping/PingObject calls; the server-side
//     handlers live in ComOxidRuntimeHelper.OxidResolverImpl and are exposed through the unauthenticated
//     ComRuntimeConnectionContext path by default.
//   - Signing: this NdrOp inherits signing/sealing from the bound RPC connection; the server-side resolver
//     default path does not require an NTLM security context, so incoming pings are not guaranteed signed.
//   - DoS posture: no rate-limiting/throttling is visible for server-side SimplePing/ComplexPing, and those
//     calls mutate ping-set state, so unauthenticated ping floods can amplify CPU/memory work.
//   - Recommendations: require authenticated RPC context before mutating ping sets, enforce negotiated
//     integrity/privacy where present, and add per-peer rate limits/back-pressure.
// TODO(p4e-security): Harden server-side SimplePing/ComplexPing before they mutate ping-set state.
internal sealed class OxidResolver : NdrOp
{
    /// <summary>
    /// Bindings
    /// </summary>
    internal DualStringArray OxidBindings { get; private set; }

    /// <summary>
    /// Ipid
    /// </summary>
    internal string IPID { get; private set; }

#pragma warning disable RECS0154 // Parameter is never used
    /// <summary>
    /// Create resolver
    /// </summary>
    /// <param name="oxid">DCOM OXID identifying the object exporter process.</param>
    internal OxidResolver(byte[] oxid) => _odix = oxid;
#pragma warning restore RECS0154 // Parameter is never used

    /// <inheritdoc/>
    public override int Opnum => 4;

    /// <inheritdoc/>
    public override void Write(NdrCodec ndr)
    {
        MarshalUnMarshalHelper.WriteOctetArrayLE(ndr, _odix);
        var context = new CodecContext();
        MarshalUnMarshalHelper.Serialize(ndr, typeof(short), (short)1, context);
        context.Flag = InteropFlags.FLAG_REPRESENTATION_ARRAY;
        MarshalUnMarshalHelper.Serialize(ndr, typeof(ComArray),
            new ComArray(new short[] { 7 }, true), context);
    }

    /// <inheritdoc/>
    public override void Read(NdrCodec ndr)
    {
        ndr.ReadUnsignedLong(); // pointer
        ndr.ReadUnsignedLong(); // some length component, irrelevant for us right now
        OxidBindings = DualStringArray.Decode(ndr);
        try
        {
            var ipid2 = new UUID();
            ipid2.Decode(ndr, ndr.Buffer);
            IPID = ipid2.ToString();
        }
        catch (NdrException e)
        {
            Log.Logger.Error(e, "RemActivation read");
        }

        // read the auth hint
        var authenticationHint = ndr.ReadUnsignedLong();
        var comVersion = new ComVersion
        {
            MajorVersion = ndr.ReadUnsignedShort(),
            MinorVersion = ndr.ReadUnsignedShort()
        };

        var hresult = ndr.ReadUnsignedLong();

        if (hresult != 0)
        {
            throw new InteropRuntimeException(hresult);
        }
    }
    private readonly byte[] _odix;
}
