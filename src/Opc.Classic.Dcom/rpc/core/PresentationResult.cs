// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using System.Text;

namespace Opc.Classic.Dcom.Rpc.Core;

/// <summary>
/// Presentation result
/// </summary>
public class PresentationResult : NdrOp {

    /// <summary>
    /// Result
    /// </summary>
    public PresentationResultCode Result { get; set; }

    /// <summary>
    /// Reason code
    /// </summary>
    public PresentationResultReason Reason { get; set; }

    /// <summary>
    /// Transfer syntax
    /// </summary>
    public PresentationSyntax TransferSyntax { get; set; }

    /// <summary>
    /// Create default result
    /// </summary>
    public PresentationResult() :
        this(new PresentationSyntax(NdrCodec.NDR_SYNTAX)) {
    }

    /// <summary>
    /// Create result
    /// </summary>
    /// <param name="transferSyntax"></param>
    public PresentationResult(PresentationSyntax transferSyntax) :
        this(PresentationResultCode.ACCEPTANCE,
            PresentationResultReason.REASON_NOT_SPECIFIED, transferSyntax) {
    }

    /// <summary>
    /// Create result
    /// </summary>
    /// <param name="result"></param>
    /// <param name="reason"></param>
    public PresentationResult(PresentationResultCode result,
        PresentationResultReason reason) :
        this(result, reason, null) {
    }

    /// <summary>
    /// Create result
    /// </summary>
    /// <param name="result"></param>
    /// <param name="reason"></param>
    /// <param name="transferSyntax"></param>
    public PresentationResult(PresentationResultCode result,
        PresentationResultReason reason, PresentationSyntax transferSyntax) {
        Result = result;
        Reason = reason;
        TransferSyntax = transferSyntax;
    }

    /// <inheritdoc/>
    public override void Read(NdrCodec ndr) {
        ndr.Buffer.Align(4);
        Result = (PresentationResultCode)ndr.ReadUnsignedShort();
        Reason = (PresentationResultReason)ndr.ReadUnsignedShort();
        // commenting this since the entire packet should be decoded VRC
        // if (Result == PresentationResultCode.ACCEPTANCE)
        {
            TransferSyntax = new PresentationSyntax();
            try {
                TransferSyntax.Decode(ndr, ndr.Buffer);
            }
            catch (NdrException ex) {
                Log.Logger.Verbose(ex, "Read presentation result failed");
            }
        }
    }

    /// <inheritdoc/>
    public override void Write(NdrCodec ndr) {
        ndr.Buffer.Align(4, 0);
        ndr.WriteUnsignedShort((int)Result);
        ndr.WriteUnsignedShort((int)Reason);
        // commenting this since the entire packet should be written VRC
        // if (Result == PresentationResultCode.ACCEPTANCE && TransferSyntax != null)
        if (TransferSyntax != null) {
            try {
                TransferSyntax.Encode(ndr, ndr.Buffer);
            }
            catch (NdrException ex) {
                Log.Logger.Verbose(ex, "Write presentation result failed");
            }
        }
    }

    /// <inheritdoc/>
    public override string ToString() {
        var str = new StringBuilder();
        switch (Result) {
            case PresentationResultCode.ACCEPTANCE:
                str.Append("ACCEPTANCE");
                break;
            case PresentationResultCode.USER_REJECTION:
                str.Append("USER_REJECTION");
                break;
            case PresentationResultCode.PROVIDER_REJECTION:
                str.Append("PROVIDER_REJECTION");
                break;
            default:
                str.Append("unknown");
                break;
        }
        str.Append("; ");
        switch (Reason) {
            case PresentationResultReason.REASON_NOT_SPECIFIED:
                str.Append("REASON_NOT_SPECIFIED");
                break;
            case PresentationResultReason.ABSTRACT_SYNTAX_NOT_SUPPORTED:
                str.Append("ABSTRACT_SYNTAX_NOT_SUPPORTED");
                break;
            case PresentationResultReason.PROPOSED_TRANSFER_SYNTAXES_NOT_SUPPORTED:
                str.Append("PROPOSED_TRANSFER_SYNTAXES_NOT_SUPPORTED");
                break;
            case PresentationResultReason.LOCAL_LIMIT_EXCEEDED:
                str.Append("LOCAL_LIMIT_EXCEEDED");
                break;
            default:
                str.Append("unknown");
                break;
        }
        return str.ToString();
    }
}
