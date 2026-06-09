// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;
using System;
using System.Linq;

namespace Opc.Classic.Dcom.Rpc.Core;

/// <summary>
/// Verifier
/// </summary>
public class AuthenticationVerifier : NdrOp {

    /// <summary>
    /// Service
    /// </summary>
    public int AuthenticationService { get; set; }

    /// <summary>
    /// Protection level
    /// </summary>
    public ProtectionLevel Protection { get; set; }

    /// <summary>
    /// Context
    /// </summary>
    public int ContextId { get; set; }

    /// <summary>
    /// Body
    /// </summary>
    public byte[] Body { get; set; }

    /// <summary>
    /// Create verifier
    /// </summary>
    public AuthenticationVerifier() :
        this(Opc.Classic.Dcom.Rpc.Security.AUTHENTICATION_SERVICE_NONE,
            ProtectionLevel.PROTECTION_LEVEL_NONE, 0, null) {
    }

    /// <summary>
    /// Create verifier
    /// </summary>
    /// <param name="authenticatorLength"></param>
    public AuthenticationVerifier(int authenticatorLength) :
        this(Opc.Classic.Dcom.Rpc.Security.AUTHENTICATION_SERVICE_NONE,
            ProtectionLevel.PROTECTION_LEVEL_NONE, 0, authenticatorLength) {
    }

    /// <summary>
    /// Create verifier
    /// </summary>
    /// <param name="authenticationService"></param>
    /// <param name="protectionLevel"></param>
    /// <param name="contextId"></param>
    /// <param name="authenticatorLength"></param>
    public AuthenticationVerifier(int authenticationService,
        ProtectionLevel protectionLevel, int contextId, int authenticatorLength) :
        this(authenticationService, protectionLevel, contextId,
            new byte[authenticatorLength]) {
    }

    /// <summary>
    /// Create verifier
    /// </summary>
    /// <param name="authenticationService"></param>
    /// <param name="protectionLevel"></param>
    /// <param name="contextId"></param>
    /// <param name="body"></param>
    public AuthenticationVerifier(int authenticationService,
        ProtectionLevel protectionLevel, int contextId, byte[] body) {
        AuthenticationService = authenticationService;
        Protection = protectionLevel;
        ContextId = contextId;
        Body = body;
    }

    /// <inheritdoc/>
    public override void Decode(NdrCodec ndr, NdrBuffer src) {
        src.Align(4);
        AuthenticationService = src.Dec_ndr_small();
        Protection = (ProtectionLevel)src.Dec_ndr_small();
        src.Dec_ndr_small(); // padding count
        ContextId = src.Dec_ndr_long();
        Array.Copy(src.Buf, src.Index, Body, 0, Body.Length);
        src.Index += Body.Length;
    }

    /// <inheritdoc/>
    public override void Encode(NdrCodec ndr, NdrBuffer dst) {
        var padding = dst.Align(4, 0);
        dst.Enc_ndr_small(AuthenticationService);
        dst.Enc_ndr_small((int)Protection);
        dst.Enc_ndr_small(padding);
        dst.Enc_ndr_small(0); // Reserved
        dst.Enc_ndr_long(ContextId);
        Array.Copy(Body, 0, dst.Buf, dst.Index, Body.Length);
        // dst.index += body.length;
        dst.Advance(Body.Length);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) {
        if (!(obj is AuthenticationVerifier other)) {
            return false;
        }
        if (AuthenticationService != other.AuthenticationService ||
            Protection != other.Protection ||
            ContextId != other.ContextId) {
            return false;
        }
        if (Body == null) {
            return other.Body == null;
        }
        if (other.Body == null) {
            return false;
        }
        return Body.SequenceEqual(other.Body);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => AuthenticationService ^ (int)Protection ^ ContextId;
}
