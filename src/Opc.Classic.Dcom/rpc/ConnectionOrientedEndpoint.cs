// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Common.Ntlm;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Dcom.Transport;
using System;
using System.IO;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Connection oriented endpoint
/// </summary>
public class ConnectionOrientedEndpoint : IEndpoint
{

    /// <inheritdoc/>
    public ITransport Transport { get; }

    /// <inheritdoc/>
    public PresentationSyntax Syntax { get; }

    /// <summary>
    /// Current iid
    /// </summary>
    protected string CurrentIID { get; set; }

    /// <summary>
    /// Current connection context
    /// </summary>
    protected IConnectionContext Context { get; set; }

    /// <summary>
    /// Create endpoint
    /// </summary>
    /// <param name="transport"></param>
    /// <param name="syntax"></param>
    public ConnectionOrientedEndpoint(ITransport transport, PresentationSyntax syntax)
    {
        _contextIdToUse = _contextIdCounter;
        Transport = transport;
        Syntax = syntax;
    }

    /// <inheritdoc/>
    public virtual void Call(Semantics semantics, UUID objectId,
        int opnum, NdrOp ndrobj)
    {
        Bind();
        var request = new RequestCoPdu
        {
            ContextId = _contextIdToUse
        };

        var b = new byte[1024];
        var buffer = new NdrBuffer(b, 0);
        var ndr = new NdrCodec();
        ndrobj.Encode(ndr, buffer);
        var stub = new byte[buffer.Length]; // yuk
        Array.Copy(buffer.Buf, 0, stub, 0, stub.Length);

        Log.Logger.Verbose("\n" + Utils.HexString(stub, 0, stub.Length));
        request.Stub = stub;
        request.AllocationHint = buffer.Length;
        request.Opnum = opnum;
        request.Object = objectId;
        if ((semantics & Semantics.MAYBE) != Semantics.None)
        {
            request.SetFlag(ConnectionOrientedPdu.PFC_MAYBE, true);
        }
        Send(request);

        if (request.GetFlag(ConnectionOrientedPdu.PFC_MAYBE))
        {
            return;
        }
        var reply = Receive();
        if (reply is ResponseCoPdu response)
        {
            ndr.Format = reply.Format;

            buffer = new NdrBuffer(response.Stub, 0);
            Log.Logger.Verbose("\n" + Utils.HexString(buffer.Buf, 0, buffer.Buf.Length));
            ndrobj.Decode(ndr, buffer);
        }
        else if (reply is FaultCoPdu fault)
        {
            throw new FaultException("Received fault.", fault.Status, fault.Stub);
        }
        else if (reply is ShutdownPdu)
        {
            throw new RpcException("Received shutdown request from server.");
        }
        else
        {
            throw new RpcException("Received unexpected PDU from server.");
        }
    }

    /// <inheritdoc/>
    public void Detach()
    {
        _bound = false;
        Context = null;
        Transport.Close();
    }

    /// <summary>
    /// Rebind
    /// </summary>
    /// <exception cref="IOException"></exception>
    protected void Rebind()
    {
        _bound = false;
        Bind();
    }

    /// <summary>
    /// Bind
    /// </summary>
    /// <exception cref="IOException"></exception>
#pragma warning disable MA0051 // Legacy bind state machine; refactor would risk RPC handshake behavior.
    protected void Bind()
    {
        if (_bound)
        {
            return;
        }
        if (Context != null)
        {
            _bound = true;
            try
            {
                bool found = _uuidsVsContextIds.TryGetValue(Syntax.ToString().ToUpperInvariant(), out var cid);
                var pdu = Context.Alter(new PresentationContext(found ? cid : ++_contextIdCounter, Syntax));
                var sendAlter = false;
                if (!found)
                {
                    _uuidsVsContextIds[Syntax.ToString().ToUpperInvariant()] = _contextIdCounter;
                    _contextIdToUse = _contextIdCounter;
                    sendAlter = true;
                }
                else
                {
                    _contextIdToUse = (int)cid;
                }

                if (sendAlter)
                {
                    if (pdu != null)
                    {
                        Send(pdu);
                    }
                    while (!Context.Established)
                    {
                        var recieved = Receive();
                        if ((pdu = Context.Accept(recieved)) != null)
                        {
                            switch (pdu.Type)
                            {
                                case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
                                    if (((BindAcknowledgePdu)pdu).ResultList[0].Result /*was: Reason*/ !=
                                        PresentationResultCode.PROVIDER_REJECTION)
                                    {
                                        CurrentIID = ((BindPdu)recieved).ContextList[0]
                                            .AbstractSyntax.Uuid.ToString();
                                    }
                                    break;
                                case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
                                    // we need to record the iid now if this is successful and subsequent
                                    // calls will now be for this iid.
                                    if (((AlterContextResponsePdu)pdu).ResultList[0].Result /*was: Reason*/ !=
                                        PresentationResultCode.PROVIDER_REJECTION)
                                    {
                                        CurrentIID = ((AlterContextPdu)recieved).ContextList[0]
                                            .AbstractSyntax.Uuid.ToString();
                                    }
                                    break;
                                default:
                                    // nothing
                                    break;
                            }
                            Send(pdu);
                        }
                    }
                }
            }
            catch (IOException)
            {
                _bound = false;
                throw;
            }
            catch (Exception)
            {
                _bound = false;
                throw;
            }
        }
        else
        {
            Connect();
        }
    }
#pragma warning restore MA0051

    /// <summary>
    /// Send
    /// </summary>
    /// <param name="request"></param>
    /// <exception cref="IOException"></exception>
    protected void Send(ConnectionOrientedPdu request)
    {
        Bind();
        Context.Connection.Transmit(request, Transport);
    }

    /// <summary>
    /// Receive
    /// </summary>
    /// <exception cref="IOException"></exception>
    protected ConnectionOrientedPdu Receive() => Context.Connection.Receive(Transport);

    /// <summary>
    /// Connect
    /// </summary>
    /// <exception cref="IOException"></exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Meziantou.Analyzer", "MA0051:Method is too long", Justification = "DCE/RPC connection-oriented bind sequence orchestrates context allocation, auth negotiation, and PDU handshake; splitting fragments the state machine.")]
    private void Connect()
    {
        _bound = true;
        _contextIdCounter = 0;
        CurrentIID = null;
        try
        {
            _uuidsVsContextIds[Syntax.ToString().ToUpperInvariant()] = _contextIdCounter;
            Context = CreateContext();
            var pdu = Context.Init(new PresentationContext(_contextIdCounter, Syntax), Transport.Properties);
            _contextIdToUse = _contextIdCounter;
            if (pdu != null)
            {
                Send(pdu);
            }
            while (!Context.Established)
            {
                var recieved = Receive();
                if ((pdu = Context.Accept(recieved)) != null)
                {
                    switch (pdu.Type)
                    {
                        case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
                            if (((BindAcknowledgePdu)pdu).ResultList[0].Result /*was: Reason*/ !=
                                PresentationResultCode.PROVIDER_REJECTION)
                            {
                                CurrentIID = ((BindPdu)recieved).ContextList[0].AbstractSyntax.Uuid.ToString();
                            }
                            break;
                        case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
                            // we need to record the iid now if this is successful and subsequent calls
                            // will now be for this iid.
                            if (((AlterContextResponsePdu)pdu).ResultList[0].Result /*was: Reason*/ !=
                                PresentationResultCode.PROVIDER_REJECTION)
                            {
                                CurrentIID = ((AlterContextPdu)recieved).ContextList[0].AbstractSyntax.Uuid.ToString();
                            }
                            break;
                    }
                    Send(pdu);
                }
            }
        }
        catch (IOException)
        {
            try
            {
                Detach();
            }
            catch (IOException)
            {
            }
            throw;
        }
        catch (Exception)
        {
            try
            {
                Detach();
            }
            catch (IOException)
            {
            }
            throw;
        }
    }

    /// <summary>
    /// Create context
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ProviderException"></exception>
    private IConnectionContext CreateContext()
    {
        var properties = Transport.Properties;
        if (properties == null)
        {
            return new BasicConnectionContext();
        }
        var context = (string)properties.GetProperty("rpc.connectionContext");
        if (context == null)
        {
            return new BasicConnectionContext();
        }
        return context switch
        {
            "Opc.Classic.Dcom.Rpc.BasicConnectionContext" => new BasicConnectionContext(),
            "rpc.security.ntlm.NtlmConnectionContext" or "Opc.Classic.Dcom.Rpc.Auth.ntlm.NtlmConnectionContext" => new NtlmConnectionContext(),
            "Opc.Classic.Dcom.Transport.ComRuntimeConnectionContext" => new ComRuntimeConnectionContext(),
            "Opc.Classic.Dcom.Transport.ComRuntimeNTLMConnectionContext" or "Opc.Classic.Dcom.Transport.ComRuntimeNtlmConnectionContext" => new ComRuntimeNtlmConnectionContext(),
            _ => throw new ProviderException("Unsupported RPC connection context: " + context),
        };
    }

    private bool _bound;
    private int _contextIdCounter;
    private int _contextIdToUse;
    private readonly Dictionary<string, int> _uuidsVsContextIds = [];
}
