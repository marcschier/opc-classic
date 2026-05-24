//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace SharpInterop.Rpc.Core; 
/// <summary>
/// Presentation layer context
/// </summary>
public class PresentationContext : NdrOp {

    /// <summary>
    /// Context
    /// </summary>
    public int ContextId { get; set; }

    /// <summary>
    /// Syntax
    /// </summary>
    public PresentationSyntax AbstractSyntax { get; set; }

    /// <summary>
    /// Transfer syntax
    /// </summary>
    public PresentationSyntax[] TransferSyntaxes { get; set; }

    /// <summary>
    /// Create default context
    /// </summary>
    public PresentationContext() :
        this(0, new PresentationSyntax(), new PresentationSyntax[] {
            new PresentationSyntax(NdrCodec.NDR_SYNTAX)
        }) {
    }

    /// <summary>
    /// Create context
    /// </summary>
    /// <param name="contextId"></param>
    /// <param name="abstractSyntax"></param>
    public PresentationContext(int contextId, PresentationSyntax abstractSyntax) :
        this(contextId, abstractSyntax, new PresentationSyntax[] {
            new PresentationSyntax(NdrCodec.NDR_SYNTAX)
        }) {
    }

    /// <summary>
    /// Create context
    /// </summary>
    /// <param name="contextId"></param>
    /// <param name="abstractSyntax"></param>
    /// <param name="transferSyntaxes"></param>
    public PresentationContext(int contextId, PresentationSyntax abstractSyntax,
        PresentationSyntax[] transferSyntaxes) {
        ContextId = contextId;
        AbstractSyntax = abstractSyntax;
        TransferSyntaxes = transferSyntaxes;
    }

    /// <inheritdoc/>
    public override void Read(NdrCodec ndr) {
        ndr.Buffer.Align(4);
        ContextId = ndr.ReadUnsignedShort();
        var count = ndr.ReadUnsignedSmall();

        try {
            AbstractSyntax.Decode(ndr, ndr.Buffer);
            TransferSyntaxes = new PresentationSyntax[count];
            for (var i = 0; i < count; i++) {
                TransferSyntaxes[i] = new PresentationSyntax();
                TransferSyntaxes[i].Decode(ndr, ndr.Buffer);
            }
        }
        catch (NdrException ex) {
            Log.Logger.Verbose(ex, "Read presentation context failed");
        }
    }

    /// <inheritdoc/>
    public override void Write(NdrCodec ndr) {
        ndr.Buffer.Align(4, unchecked(0xcc));
        ndr.WriteUnsignedShort(ContextId);
        ndr.WriteUnsignedShort((short)TransferSyntaxes.Length);

        try {
            AbstractSyntax.Encode(ndr, ndr.Buffer);
            for (var i = 0; i < TransferSyntaxes.Length; i++) {
                TransferSyntaxes[i].Encode(ndr, ndr.Buffer);
            }
        }
        catch (NdrException ex) {
            Log.Logger.Verbose(ex, "Write presentation context failed");
        }
    }
}
