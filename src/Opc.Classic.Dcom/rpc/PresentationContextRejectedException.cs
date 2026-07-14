// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Rpc.Core;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// A DCE/RPC presentation context was rejected for a requested interface.
/// </summary>
public sealed class PresentationContextRejectedException : InvalidOperationException
{
    public PresentationContextRejectedException()
    {
    }

    public PresentationContextRejectedException(string message)
        : base(message)
    {
    }

    public PresentationContextRejectedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public PresentationContextRejectedException(Guid interfaceId, string message)
        : base(message)
    {
        InterfaceId = interfaceId;
    }

    public PresentationContextRejectedException(Guid interfaceId, PresentationResult result, string message)
        : base(message)
    {
        InterfaceId = interfaceId;
        Result = result.Result;
        Reason = result.Reason;
    }

    public Guid InterfaceId { get; }

    public PresentationResultCode? Result { get; }

    public PresentationResultReason? Reason { get; }

    public bool IsAbstractSyntaxNotSupported =>
        Reason == PresentationResultReason.ABSTRACT_SYNTAX_NOT_SUPPORTED;
}
