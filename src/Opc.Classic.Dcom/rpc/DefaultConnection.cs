// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Default connection object
/// </summary>
public class DefaultConnection : IConnection
{

    /// <summary>
    /// Create connection
    /// </summary>
    public DefaultConnection() :
        this(ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE)
    {
    }

    /// <summary>
    /// Create connection
    /// </summary>
    /// <param name="transmitLength">Length in bytes or elements of the value being processed.</param>
    /// <param name="receiveLength">Length in bytes or elements of the value being processed.</param>
    public DefaultConnection(int transmitLength, int receiveLength)
    {
        _ndr = new NdrCodec();
        _transmitBuffer = new NdrBuffer(new byte[transmitLength], 0);
        _receiveBuffer = new NdrBuffer(new byte[receiveLength], 0);
    }

    /// <inheritdoc/>
    public void Transmit(ConnectionOrientedPdu pdu, ITransport transport)
    {
        if (!(pdu is IFragmentable fpdu))
        {
            TransmitPdu(pdu, transport);
            return;
        }
        foreach (var fragment in fpdu.GetFragments(_transmitBuffer.GetCapacity()))
        {
            TransmitPdu(fragment, transport);
        }
    }

    /// <inheritdoc/>
    public ConnectionOrientedPdu Receive(ITransport transport)
    {
        var pdu = ReceivePdu(transport);
        if (!(pdu is IFragmentable fpdu) ||
            pdu.GetFlag(ConnectionOrientedPdu.PFC_LAST_FRAG))
        {
            return pdu;
        }
        return fpdu.Reassemble(ReceiveFragments(transport, pdu));
    }

    private IEnumerable<ConnectionOrientedPdu> ReceiveFragments(ITransport transport, ConnectionOrientedPdu firstFragment)
    {
        var currentFragment = firstFragment;
        var packetIndex = 0;
        while (currentFragment != null)
        {
            var fragment = currentFragment;
            if (fragment.GetFlag(ConnectionOrientedPdu.PFC_LAST_FRAG))
            {
                currentFragment = null;
            }
            else
            {
                try
                {
                    Log.Logger.Verbose("[Fragmented Packet] [" + packetIndex++ +
                        "] recieved, fragment decomposition is below: ");
                    currentFragment = ReceivePdu(transport);
                }
                catch (InvalidCastException e)
                {
                    throw new IOException("invalid pdu received", e);
                }
                catch (IOException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Unknown", ex);
                }
            }

            yield return fragment;
        }
    }

    /// <summary>
    /// Send fragment
    /// </summary>
    /// <param name="fragment">PDU fragment to encode and send on the transport.</param>
    /// <param name="transport">Underlying RPC transport handle, such as a TCP socket or SMB named pipe.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    private void TransmitPdu(ConnectionOrientedPdu fragment, ITransport transport)
    {
        _transmitBuffer.Reset();
        fragment.Encode(_ndr, _transmitBuffer);
        ProcessOutgoing();
        Log.Logger.Verbose("[TRANSMIT BUFFER]:\n" +
            Utils.HexString(_transmitBuffer.Buf, 0, _transmitBuffer.Length));
        transport.Send(_transmitBuffer);
    }

    /// <summary>
    /// Receive fragment
    /// </summary>
    /// <param name="transport">Underlying RPC transport handle, such as a TCP socket or SMB named pipe.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    /// <returns>The decoded connection-oriented PDU received from the transport.</returns>
#pragma warning disable MA0051 // Legacy fragment receive state machine; refactor would risk packet framing behavior.
    private ConnectionOrientedPdu ReceivePdu(ITransport transport)
    {
        var read = true;

        if (_bytesRemainingInRecieveBuffer)
        {

            // receiver buffer always falls on the boundary of a new Fragment.
            //
            // Earlier code commented out: 2-byte packets caused this logic to fail
            // and thus read was set to true (since the receiveBuffer.length was less than or equal to ConnectionOrientedPdu.TYPE_OFFSET)
            // so we read a fresh packet and whatever bytes were there in recieverBuffer already were lost !
            {
                //            if (receiveBuffer.length > ConnectionOrientedPdu.TYPE_OFFSET)
                //                receiveBuffer.setIndex(ConnectionOrientedPdu.TYPE_OFFSET);
                //                type = receiveBuffer.dec_ndr_small();
                //                if (isValidType(type))
                {
                    // this is required so that the correct length for the next fragment can be obtained.
                    // If is < 10 bytes than the fraglength would be an arbitary length.
                    while (_receiveBuffer.Length <= 10)
                    {
                        // perform a read again in a new buffer and assign that to the reciever buffer
                        // this needs to be a small buffer 10 bytes
                        var tmpBuffer = new NdrBuffer(new byte[10], 0);
                        transport.Receive(tmpBuffer);
                        Array.Copy(tmpBuffer.Buf, 0, _receiveBuffer.Buf, _receiveBuffer.Length, tmpBuffer.Length);
                        _receiveBuffer.Length += tmpBuffer.Length;
                    }
                    read = false;
                }
            }

            _bytesRemainingInRecieveBuffer = false;
        }

        // will be true for all cases and false if anything valid is already in the buffer
        if (read)
        {
            // read the transport now...
            _receiveBuffer.Reset();
            Log.Logger.Verbose("Reading bytes from RecieveBuffer Socket...Current Capacity: " +
                _receiveBuffer.GetCapacity());
            transport.Receive(_receiveBuffer);
            Log.Logger.Verbose("[RECIEVER BUFFER]:\n" +
                Utils.HexString(_receiveBuffer.Buf, 0, _receiveBuffer.Length));
        }

        var counter = 0;
        var trimSize = -1;
        var lengthOfArrayTobeRead = _receiveBuffer.Length;
        // frag length logic
        if (_receiveBuffer.Length <= 0)
        {
            // socket has been closed.
            throw new IOException("Socket Closed");
        }

        _receiveBuffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
        var frag = new byte[2]; // short
        _receiveBuffer.ReadOctetArray(frag, 0, frag.Length);
        var fragmentLength = (frag[0] & 0xFF) | ((frag[1] & 0xFF) << 8);
        //            fragmentLength = receiveBuffer.dec_ndr_short();
        Log.Logger.Verbose("\n" + " length of the fragment " + fragmentLength + "\n" +
            " size in bytes of the buffer [] " + _receiveBuffer.Buf.Length);

        // the new buffer should be equal to fragment size
        var newbuffer = new byte[fragmentLength];
        if (fragmentLength > _receiveBuffer.Length)
        {
            // this means the socket buffer is not fully read, this packet is bigger than the reciever buffer size
            var remainingBytes = fragmentLength - _receiveBuffer.Length;
            Log.Logger.Verbose("\n" + " Some bytes from RecieveBuffer Socket have not been read: Remaining  " +
                remainingBytes);

            // now reset and read again.
            while (fragmentLength > counter)
            {
                Array.Copy(_receiveBuffer.Buf, 0, newbuffer, counter, lengthOfArrayTobeRead);
                counter += lengthOfArrayTobeRead;
                if (fragmentLength == counter)
                {
                    break;
                }
                Log.Logger.Verbose("\n" + " About to read more bytes from socket, current counter is: " + counter);
                _receiveBuffer.Reset();
                // now read again so as to take it from network buffer to your buffer
                // this may actually read 2 or more packets, one is this partial one (now complete)
                // and one may be some other one, like a request packet.
                // or it may not ...and reads only the partial packet.
                transport.Receive(_receiveBuffer);
                if (fragmentLength - counter >= _receiveBuffer.Length)
                {
                    lengthOfArrayTobeRead = _receiveBuffer.Length;
                }
                else
                {
                    // this would be the last one. Now we need to trim the buffer to it's read length as well.
                    lengthOfArrayTobeRead = fragmentLength - counter;
                    trimSize = _receiveBuffer.Length - lengthOfArrayTobeRead;
                }

                Log.Logger.Verbose("lengthOfArrayTobeRead = " + lengthOfArrayTobeRead + "\n" +
                    "trimSize = " + trimSize + "\n" + "RecieveBuffer current read size: " + _receiveBuffer.Length);
                Log.Logger.Verbose("[RECIEVER BUFFER]:\n" +
                    Utils.HexString(_receiveBuffer.Buf, 0, _receiveBuffer.Length));
            }
        }
        else
        {
            Log.Logger.Verbose("fragmentLength is less than  receiveBuffer.length");
            // Since fragment length is smaller, There might be 2 or more packets in here
            // just read what is your packet.
            Array.Copy(_receiveBuffer.Buf, 0, newbuffer, 0, fragmentLength);
            // there might be more. Now we need to trim the buffer to it's read length as well.
            trimSize = _receiveBuffer.Length - fragmentLength;
        }

        if (trimSize > 0)
        {
            Log.Logger.Verbose("trimSize = " + trimSize);
            Array.Copy(_receiveBuffer.Buf, _receiveBuffer.Length - trimSize, _receiveBuffer.Buf, 0, trimSize);
            _receiveBuffer.Length = trimSize;
            _receiveBuffer.Index = 0;
            _receiveBuffer.Start = 0;
            // reciever buffer read more than it should, after we trim only the additionally read bytes will be left.
            // these have to be read in the next call to recieveFragment.
            _bytesRemainingInRecieveBuffer = true;
        }

        var bufferToBeUsed = new NdrBuffer(newbuffer, 0)
        {
            Length = newbuffer.Length // this will be fully utilized and not left empty.
        };

        Log.Logger.Verbose("bufferToBeUsed Size = " + bufferToBeUsed.Length + " : " +
            Utils.HexString(bufferToBeUsed.Buf, 0, bufferToBeUsed.Length));

        // caution, frag length is changed here...it is void of security info.
        ProcessIncoming(bufferToBeUsed);
        bufferToBeUsed.Index = ConnectionOrientedPdu.TYPE_OFFSET;
        var type = bufferToBeUsed.Dec_ndr_small();

        ConnectionOrientedPdu pdu;
        switch (type)
        {
            case AlterContextPdu.ALTER_CONTEXT_TYPE:
                pdu = new AlterContextPdu();
                break;
            case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
                pdu = new AlterContextResponsePdu();
                break;
            case Auth3Pdu.AUTH3_TYPE:
                pdu = new Auth3Pdu();
                break;
            case BindPdu.BIND_TYPE:
                pdu = new BindPdu();
                break;
            case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
                pdu = new BindAcknowledgePdu();
                break;
            case BindNoAcknowledgePdu.BIND_NO_ACKNOWLEDGE_TYPE:
                pdu = new BindNoAcknowledgePdu();
                break;
            case CancelCoPdu.CANCEL_TYPE:
                pdu = new CancelCoPdu();
                break;
            case FaultCoPdu.FAULT_TYPE:
                pdu = new FaultCoPdu();
                break;
            case OrphanedPdu.ORPHANED_TYPE:
                pdu = new OrphanedPdu();
                break;
            case RequestCoPdu.REQUEST_TYPE:
                pdu = new RequestCoPdu();
                break;
            case ResponseCoPdu.RESPONSE_TYPE:
                pdu = new ResponseCoPdu();
                break;
            case ShutdownPdu.SHUTDOWN_TYPE:
                pdu = new ShutdownPdu();
                break;
            default:
                throw new IOException("Unknown PDU type: 0x" + type.ToString("x"));
        }
        bufferToBeUsed.Index = 0;
        pdu.Decode(_ndr, bufferToBeUsed);
        return pdu;
    }
#pragma warning restore MA0051

    /// <summary>
    /// Incoming rebind
    /// </summary>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    /// <param name="verifier">Authentication verifier attached to the RPC PDU.</param>
    protected internal virtual void IncomingRebind(AuthenticationVerifier verifier)
    {
        // nothing
    }

    /// <summary>
    /// Outgoing rebind
    /// </summary>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    /// <returns>The security verifier bytes attached to an outgoing rebind PDU.</returns>
    protected internal virtual AuthenticationVerifier OutgoingRebind() => null;

    /// <summary>
    /// Process incoming
    /// </summary>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    /// <param name="buffer">Buffer containing the bytes or fields being processed.</param>
#pragma warning disable MA0051 // Legacy RPC authentication PDU dispatch; preserving fall-through behavior.
    private void ProcessIncoming(NdrBuffer buffer)
    {
        buffer.Index = ConnectionOrientedPdu.TYPE_OFFSET;
        var logMsg = true;
        switch (buffer.Dec_ndr_small())
        {
            case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("Recieved BIND_ACK");
                    logMsg = false;
                }
                goto case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE;
            case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("Recieved ALTER_CTX_RESP");
                    logMsg = false;
                }
                goto case BindPdu.BIND_TYPE;
            case BindPdu.BIND_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("Recieved BIND");
                    logMsg = false;
                }
                goto case AlterContextPdu.ALTER_CONTEXT_TYPE;
            case AlterContextPdu.ALTER_CONTEXT_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("Recieved ALTER_CTX");
                }
                var verifier = DetachAuthentication(buffer);
                if (verifier != null)
                {
                    IncomingRebind(verifier);
                }
                break;
            case FaultCoPdu.FAULT_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("\n Recieved FAULT");
                    logMsg = false;
                }
                goto case CancelCoPdu.CANCEL_TYPE;
            case CancelCoPdu.CANCEL_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("\n Recieved CANCEL");
                    logMsg = false;
                }
                goto case OrphanedPdu.ORPHANED_TYPE;
            case OrphanedPdu.ORPHANED_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("\n Recieved ORPHANED");
                    logMsg = false;
                }
                goto case ResponseCoPdu.RESPONSE_TYPE;
            case ResponseCoPdu.RESPONSE_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("\n Recieved RESPONSE");
                    logMsg = false;
                }
                goto case RequestCoPdu.REQUEST_TYPE;
            case RequestCoPdu.REQUEST_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("\n Recieved REQUEST");
                }
                if (_security != null)
                {
                    var ndr2 = new NdrCodec
                    {
                        Buffer = buffer
                    };
                    VerifyAndUnseal(ndr2);
                }
                else
                {
                    DetachAuthentication(buffer); // just strip the information, do not use it.
                }
                break;
            case Auth3Pdu.AUTH3_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("\n Recieved AUTH3");
                }
                IncomingRebind(DetachAuthentication2(buffer));
                break;
            case BindNoAcknowledgePdu.BIND_NO_ACKNOWLEDGE_TYPE:
            case ShutdownPdu.SHUTDOWN_TYPE:
                return;
            default:
                throw new RpcException("Invalid incoming PDU type.");
        }
    }
#pragma warning restore MA0051

    /// <summary>
    /// Process outgoing
    /// </summary>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
#pragma warning disable MA0051 // Legacy RPC authentication PDU dispatch; preserving fall-through behavior.
    private void ProcessOutgoing()
    {
        _ndr.Buffer.Index = ConnectionOrientedPdu.TYPE_OFFSET;
        var logMsg = true;
        switch (_ndr.ReadUnsignedSmall())
        {
            case BindPdu.BIND_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("\n Sending BIND");
                    logMsg = false;
                }
                goto case Auth3Pdu.AUTH3_TYPE;
            case Auth3Pdu.AUTH3_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("\n Sending AUTH3");
                    logMsg = false;
                }

                goto case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE;
            case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("\n Sending BIND_ACK");
                    logMsg = false;
                }
                goto case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE;
            case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("\n Sending ALTER_CTX_RESP");
                }
                var verifier = OutgoingRebind();
                if (verifier != null)
                {
                    AttachAuthentication(verifier);
                }
                break;
            case AlterContextPdu.ALTER_CONTEXT_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("\n Sending ALTER_CTX");
                }
                break;
            case RequestCoPdu.REQUEST_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("\n Sending REQUEST");
                    logMsg = false;
                }
                //            verifier = outgoingRebind();
                //            if (verifier != null) attachAuthentication(verifier);
                goto case CancelCoPdu.CANCEL_TYPE;
            case CancelCoPdu.CANCEL_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("\n Sending CANCEL");
                    logMsg = false;
                }
                goto case OrphanedPdu.ORPHANED_TYPE;
            case OrphanedPdu.ORPHANED_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("\n Sending ORPHANED");
                    logMsg = false;
                }
                goto case FaultCoPdu.FAULT_TYPE;
            case FaultCoPdu.FAULT_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("\n Sending FAULT");
                    logMsg = false;
                }
                goto case ResponseCoPdu.RESPONSE_TYPE;
            case ResponseCoPdu.RESPONSE_TYPE:
                if (logMsg)
                {
                    Log.Logger.Information("\n Sending RESPONSE");
                }
                if (_security != null)
                {
                    SignAndSeal(_ndr);
                }
                break;
            case BindNoAcknowledgePdu.BIND_NO_ACKNOWLEDGE_TYPE:
            case ShutdownPdu.SHUTDOWN_TYPE:
                return;
            default:
                throw new RpcException("Invalid outgoing PDU type.");
        }
    }
#pragma warning restore MA0051

    /// <summary>
    /// Add auth
    /// </summary>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    private void AttachAuthentication(AuthenticationVerifier verifier)
    {
        try
        {
            var buffer = _ndr.Buffer;
            var length = buffer.Length;
            buffer.Index = length;
            verifier.Encode(_ndr, buffer);
            length = buffer.Length;
            buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
            _ndr.WriteUnsignedShort(length);
            _ndr.WriteUnsignedShort(verifier.Body.Length);
            // buffer.setIndex(ConnectionOrientedPdu.FLAGS_OFFSET);
            // ndr.writeUnsignedSmall(0);
        }
        catch (Exception ex)
        {
            throw new IOException("Error attaching authentication to PDU: " + ex.Message);
        }
    }

    /// <summary>
    /// Remove auth
    /// </summary>
    /// <param name="buffer">Buffer containing the bytes or fields being processed.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    /// <returns>The authentication trailer removed from the received PDU buffer.</returns>
    private AuthenticationVerifier DetachAuthentication2(NdrBuffer buffer)
    {
        try
        {
            buffer.Index = ConnectionOrientedPdu.AUTH_LENGTH_OFFSET;
            var length = buffer.Dec_ndr_short(); // auth body size
            var index = 20;
            buffer.Index = index; // exactly at the auth type.
            var verifier = new AuthenticationVerifier(length);
            verifier.Decode(_ndr, buffer);
            buffer.Index = index + 2; // auth padding
            length = index - buffer.Dec_ndr_small();
            buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
            buffer.Enc_ndr_short(length);
            buffer.Enc_ndr_short(0);
            buffer.Index = length;
            return verifier;
        }
        catch (Exception ex)
        {
            throw new IOException("Error stripping authentication from PDU: " + ex);
        }
    }

    /// <summary>
    /// Remove auth
    /// </summary>
    /// <param name="buffer">Buffer containing the bytes or fields being processed.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    /// <returns>The PDU bytes with the authentication verifier detached.</returns>
    private AuthenticationVerifier DetachAuthentication(NdrBuffer buffer)
    {
        try
        {
            buffer.Index = ConnectionOrientedPdu.AUTH_LENGTH_OFFSET;
            var length = buffer.Dec_ndr_short(); // auth body size
            if (length == 0)
            {
                Log.Logger.Verbose("In [detachAuthentication] No authn info present...");
                return null;
            }
            var index = buffer.Length - length - 8; // 8 = auth header size
            buffer.Index = index;
            var verifier = new AuthenticationVerifier(length);
            verifier.Decode(_ndr, buffer);
            buffer.Index = index + 2; // auth padding
            length = index - buffer.Dec_ndr_small();
            buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
            buffer.Enc_ndr_short(length);
            buffer.Enc_ndr_short(0);
            buffer.Index = length;
            Log.Logger.Verbose("In [detachAuthentication] (after stripping authn info) " +
                "setting new FRAG_LENGTH_OFFSET for the packet as = " + length);
            return verifier;
        }
        catch (Exception ex)
        {
            throw new IOException("Error stripping authentication from PDU: " + ex);
        }
    }

    /// <summary>
    /// Sign and seal
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    /// <returns>The signed and sealed PDU bytes ready for transmission.</returns>
    private void SignAndSeal(NdrCodec ndr)
    {
        var protectionLevel = _security.Protection;
        if (protectionLevel < ProtectionLevel.PROTECTION_LEVEL_INTEGRITY)
        {
            return;
        }
        var verifierLength = _security.VerifierLength;
        var verifier = new AuthenticationVerifier(_security.AuthenticationService, protectionLevel, _contextId, verifierLength);
        var buffer = ndr.Buffer;
        var length = buffer.Length;
        buffer.Index = length;
        verifier.Encode(ndr, buffer);
        length = buffer.Length;
        buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
        ndr.WriteUnsignedShort(length);
        ndr.WriteUnsignedShort(verifierLength);
        var verifierIndex = length - verifierLength;
        length -= verifierLength + 8; // less verifier + header
        var index = ConnectionOrientedPdu.HEADER_LENGTH;
        buffer.Index = ConnectionOrientedPdu.TYPE_OFFSET;
        switch (ndr.ReadUnsignedSmall())
        {
            case RequestCoPdu.REQUEST_TYPE:
                index += 8;
                buffer.Index = ConnectionOrientedPdu.FLAGS_OFFSET;
                if ((ndr.ReadUnsignedSmall() & ConnectionOrientedPdu.PFC_OBJECT_UUID) != 0)
                {
                    index += 16;
                }
                break;
            case FaultCoPdu.FAULT_TYPE:
                index += 16;
                break;
            case ResponseCoPdu.RESPONSE_TYPE:
                index += 8;
                break;
            case CancelCoPdu.CANCEL_TYPE:
            case OrphanedPdu.ORPHANED_TYPE:
                index = length;
                break;
            default:
                throw new IntegrityException("Not an authenticated PDU type.");
        }
        var isFragmented = true;
        buffer.Index = ConnectionOrientedPdu.FLAGS_OFFSET;
        var flags = ndr.ReadUnsignedSmall();
        if ((flags & ConnectionOrientedPdu.PFC_FIRST_FRAG) == ConnectionOrientedPdu.PFC_FIRST_FRAG &&
            (flags & ConnectionOrientedPdu.PFC_LAST_FRAG) == ConnectionOrientedPdu.PFC_LAST_FRAG)
        {
            isFragmented = false;
        }
        length -= index;
        _security.ProcessOutgoing(ndr, index, length, verifierIndex, isFragmented);
    }

    /// <summary>
    /// Verify and unseal
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    /// <returns>The verified and unsealed PDU bytes.</returns>
    private void VerifyAndUnseal(NdrCodec ndr)
    {
        var buffer = ndr.Buffer;
        buffer.Index = ConnectionOrientedPdu.AUTH_LENGTH_OFFSET;
        var verifierLength = ndr.ReadUnsignedShort();
        if (verifierLength <= 0)
        {
            return;
        }
        var verifierIndex = buffer.Length - verifierLength;
        var length = verifierIndex - 8;
        var index = ConnectionOrientedPdu.HEADER_LENGTH;
        buffer.Index = ConnectionOrientedPdu.TYPE_OFFSET;
        switch (ndr.ReadUnsignedSmall())
        {
            case RequestCoPdu.REQUEST_TYPE:
                index += 8;
                buffer.Index = ConnectionOrientedPdu.FLAGS_OFFSET;
                if ((ndr.ReadUnsignedSmall() & ConnectionOrientedPdu.PFC_OBJECT_UUID) != 0)
                {
                    index += 16;
                }
                break;
            case FaultCoPdu.FAULT_TYPE:
                index += 16;
                break;
            case ResponseCoPdu.RESPONSE_TYPE:
                index += 8;
                break;
            case CancelCoPdu.CANCEL_TYPE:
            case OrphanedPdu.ORPHANED_TYPE:
                index = length;
                break;
            default:
                throw new IntegrityException("Not an authenticated PDU type.");
        }

        length -= index;

        var isFragmented = true;
        buffer.Index = ConnectionOrientedPdu.FLAGS_OFFSET;
        var flags = ndr.ReadUnsignedSmall();
        if ((flags & ConnectionOrientedPdu.PFC_FIRST_FRAG) == ConnectionOrientedPdu.PFC_FIRST_FRAG &&
            (flags & ConnectionOrientedPdu.PFC_LAST_FRAG) == ConnectionOrientedPdu.PFC_LAST_FRAG)
        {
            isFragmented = false;
        }

        _security.ProcessIncoming(ndr, index, length, verifierIndex, isFragmented);
        buffer.Index = verifierIndex - 6; // auth padding field
        length = verifierIndex - ndr.ReadUnsignedSmall() - 8;
        buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
        // "doctor" the PDU by removing the auth and padding
        ndr.WriteUnsignedShort(length);
        ndr.WriteUnsignedShort(0);
        buffer.Length = length;
    }

    /// <summary>
    /// Check if valid type id
    /// </summary>
    /// <param name="type">COM or NDR type descriptor for the value being processed.</param>
    /// <returns><c>true</c> when is valid type is satisfied; otherwise <c>false</c>.</returns>
#pragma warning disable IDE0051 // Remove unused private members
    private bool IsValidType(int type)
    {
#pragma warning restore IDE0051 // Remove unused private members
        switch (type)
        {
            case AlterContextPdu.ALTER_CONTEXT_TYPE:
            case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
            case Auth3Pdu.AUTH3_TYPE:
            case BindPdu.BIND_TYPE:
            case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
            case BindNoAcknowledgePdu.BIND_NO_ACKNOWLEDGE_TYPE:
            case CancelCoPdu.CANCEL_TYPE:
            case FaultCoPdu.FAULT_TYPE:
            case OrphanedPdu.ORPHANED_TYPE:
            case RequestCoPdu.REQUEST_TYPE:
            case ResponseCoPdu.RESPONSE_TYPE:
            case ShutdownPdu.SHUTDOWN_TYPE:
                return true;
            default:
                return false;
        }
    }

    /// <summary> Codec </summary>
    protected internal NdrCodec _ndr;
    /// <summary> Buffer </summary>
    protected internal NdrBuffer _transmitBuffer;
    /// <summary> Buffer </summary>
    protected internal NdrBuffer _receiveBuffer;
    /// <summary> Securit </summary>
    protected internal ISecurity _security;
    /// <summary> Context </summary>
    protected internal int _contextId;
    private bool _bytesRemainingInRecieveBuffer;
}
