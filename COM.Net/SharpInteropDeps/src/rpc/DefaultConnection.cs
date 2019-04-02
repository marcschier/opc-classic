using System;
using System.Collections;

// 
// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
// 
// j-Interop (Pure Java implementation of DCOM protocol)
// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace rpc
{


	using Hexdump = jcifs.util.Hexdump;
	using NdrBuffer = ndr.NdrBuffer;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;
	using AuthenticationVerifier = core.AuthenticationVerifier;
	using AlterContextPdu = pdu.AlterContextPdu;
	using AlterContextResponsePdu = pdu.AlterContextResponsePdu;
	using Auth3Pdu = pdu.Auth3Pdu;
	using BindAcknowledgePdu = pdu.BindAcknowledgePdu;
	using BindNoAcknowledgePdu = pdu.BindNoAcknowledgePdu;
	using BindPdu = pdu.BindPdu;
	using CancelCoPdu = pdu.CancelCoPdu;
	using FaultCoPdu = pdu.FaultCoPdu;
	using OrphanedPdu = pdu.OrphanedPdu;
	using RequestCoPdu = pdu.RequestCoPdu;
	using ResponseCoPdu = pdu.ResponseCoPdu;
	using ShutdownPdu = pdu.ShutdownPdu;

	public class DefaultConnection : Connection
	{

		protected internal NetworkDataRepresentation ndr;

		protected internal NdrBuffer transmitBuffer;

		protected internal NdrBuffer receiveBuffer;

		protected internal Security security;

		protected internal int contextId;

		private static readonly Logger logger = Logger.getLogger("org.jinterop");

		public DefaultConnection() : this(ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE)
		{
		}

		public DefaultConnection(int transmitLength, int receiveLength)
		{
			ndr = new NetworkDataRepresentation();
			transmitBuffer = new NdrBuffer(new sbyte[transmitLength], 0);
			receiveBuffer = new NdrBuffer(new sbyte[receiveLength], 0);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void transmit(ConnectionOrientedPdu pdu, Transport transport) throws java.io.IOException
		public virtual void transmit(ConnectionOrientedPdu pdu, Transport transport)
		{
			if (!(pdu is Fragmentable))
			{
				transmitFragment(pdu, transport);
				return;
			}
			var fragments = ((Fragmentable) pdu).fragment(transmitBuffer.Capacity);
			while (fragments.hasNext())
			{
				transmitFragment((ConnectionOrientedPdu) fragments.next(), transport);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ConnectionOrientedPdu receive(final Transport transport) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		public virtual ConnectionOrientedPdu receive(Transport transport)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ConnectionOrientedPdu fragment = receiveFragment(transport);
			var fragment = receiveFragment(transport);
			if (!(fragment is Fragmentable) || fragment.getFlag(ConnectionOrientedPdu.PFC_LAST_FRAG))
			{
				return fragment;
			}
			return (ConnectionOrientedPdu)((Fragmentable) fragment).assemble(new IteratorAnonymousInnerClassHelper(this, transport, fragment));
		}

		private class IteratorAnonymousInnerClassHelper : IEnumerator
		{
			private readonly DefaultConnection outerInstance;

			private readonly Transport transport;
			private readonly ConnectionOrientedPdu fragment;

			public IteratorAnonymousInnerClassHelper(DefaultConnection outerInstance, Transport transport, ConnectionOrientedPdu fragment)
			{
				this.outerInstance = outerInstance;
				this.transport = transport;
				this.fragment = fragment;
				currentFragment = fragment;
				i = 0;
			}

			internal ConnectionOrientedPdu currentFragment;
			public virtual bool hasNext()
			{
				return currentFragment != null;
			}
			private int i;
			public virtual object next()
			{
				if (currentFragment == null)
				{
					throw new NoSuchElementException();
				}
				try
				{
					return currentFragment;
				}
				finally
				{
					if (currentFragment.getFlag(ConnectionOrientedPdu.PFC_LAST_FRAG))
					{
						currentFragment = null;
					}
					else
					{
						try
						{
							//fragLengthOfReceiveBuffer = -1;//clear the buffer here.
							//System.out.println("VIKRAM VIKRAM ");
							if (logger.isLoggable(Level.FINEST))
							{
								logger.finest("[Fragmented Packet] [" + i++ + "] recieved , fragment decomposition is below:- ");
							}
							currentFragment = outerInstance.receiveFragment(transport);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.ToString());
							Console.Write(ex.StackTrace);
							throw new InvalidOperationException();
						}
					}
				}
			}
			public virtual void remove()
			{
				throw new NotSupportedException();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void transmitFragment(ConnectionOrientedPdu fragment, Transport transport) throws java.io.IOException
		protected internal virtual void transmitFragment(ConnectionOrientedPdu fragment, Transport transport)
		{
			transmitBuffer.reset();

			fragment.encode(ndr, transmitBuffer);

			processOutgoing();


			//jcifs.util.Hexdump.hexdump(System.err, transmitBuffer.getBuffer(), 0, transmitBuffer.length);
			if (logger.isLoggable(Level.FINEST))
			{
				var byteArrayOutputStream = new ByteArrayOutputStream();
				Hexdump.hexdump(new PrintStream(byteArrayOutputStream), transmitBuffer.Buffer, 0, transmitBuffer.length);
				logger.finest("[TRANSMIT BUFFER]:-\n" + byteArrayOutputStream.ToString());
			}
			transport.send(transmitBuffer);
		}


		private bool bytesRemainingInRecieveBuffer;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected ConnectionOrientedPdu receiveFragment(Transport transport) throws java.io.IOException
		protected internal virtual ConnectionOrientedPdu receiveFragment(Transport transport)
		{

			var fragmentLength = -1;
			var type = -1;
			var read = true;

			if (bytesRemainingInRecieveBuffer)
			{
				//Vikram - 26th Feb 2013.
				//receiver buffer always falls on the boundary of a new Fragment. 
				//

				//Vikram - 26th Feb 2013, commenting belwo as we were getting packets which are 2 bytes in length causing this logic to fail
				//and thus read was set to true (since the receiveBuffer.length was less than or equal to ConnectionOrientedPdu.TYPE_OFFSET)
				//so we read a fresh packet and whatever bytes were there in recieverBuffer already were lost !

				{
	//    		if (receiveBuffer.length > ConnectionOrientedPdu.TYPE_OFFSET)
	//    			receiveBuffer.setIndex(ConnectionOrientedPdu.TYPE_OFFSET);
	//	    		type = receiveBuffer.dec_ndr_small();
	//				if (isValidType(type))
					{
						//this is required so that the correct length for the next fragment can be obtained. If is < 10 bytes than the fraglength would be an arbitary length.
						while (receiveBuffer.length <= 10)
						{
							//perform a read again in a new buffer and assign that to the reciever buffer
							//this needs to be a small buffer 10 bytes
							var tmpBuffer = new NdrBuffer(new sbyte[10], 0);
							transport.receive(tmpBuffer);
							Array.Copy(tmpBuffer.buf, 0, receiveBuffer.buf, receiveBuffer.length, tmpBuffer.length);
							receiveBuffer.length = receiveBuffer.length + tmpBuffer.length;
						}
						read = false;
					}
				}

				bytesRemainingInRecieveBuffer = false;
			}

			//will be true for all cases and false if anything valid is already in the buffer
			if (read)
			{
				//read the transport now...
				receiveBuffer.reset();
				if (logger.isLoggable(Level.FINEST))
				{
					logger.finest("\n" + " Reading bytes from RecieveBuffer Socket...Current Capacity:- " + receiveBuffer.Capacity);
				}

				transport.receive(receiveBuffer);

				if (logger.isLoggable(Level.FINEST))
				{
					logger.finest("[RECIEVER BUFFER] Full packet is dumped below...");
					var byteArrayOutputStream = new ByteArrayOutputStream();
					Hexdump.hexdump(new PrintStream(byteArrayOutputStream), receiveBuffer.Buffer, 0, receiveBuffer.length);
					logger.finest("\n" + byteArrayOutputStream.ToString());
					logger.finest("\n" + " Bytes read from RecieveBuffer Socket:- " + receiveBuffer.length);
				}

			}

			sbyte[] newbuffer = null;
			var counter = 0;
			var trimSize = -1;
			var lengthOfArrayTobeRead = receiveBuffer.length;
            //frag length logic
            if (receiveBuffer.length > 0) {
                receiveBuffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
                var frag = new sbyte[2]; //short
                receiveBuffer.readOctetArray(frag, 0, frag.Length);
                fragmentLength = (frag[0] & 0xFF) | ((frag[1] & 0xFF) << 8); //receiveBuffer.dec_ndr_short(); is looping over.
                                                                             //			fragmentLength = receiveBuffer.dec_ndr_short(); 
                if (logger.isLoggable(Level.FINEST)) {
                    logger.finest("\n" + " length of the fragment " + fragmentLength + "\n" + " size in bytes of the buffer [] " + receiveBuffer.buf.Length);
                }

                if (fragmentLength < 0) {
                    var h = 0;
                    h++;
                    Hexdump.hexdump(System.out, receiveBuffer.buf, 0, receiveBuffer.buf.Length);
                }
                //the new buffer should be equal to fragment size
                newbuffer = new sbyte[fragmentLength];

                if (fragmentLength > receiveBuffer.length) //this means the socket buffer is not fully read, this packet is bigger than the reciever buffer size
                {
                    var remainingBytes = fragmentLength - receiveBuffer.length;
                    if (logger.isLoggable(Level.FINEST)) {
                        logger.finest("\n" + " Some bytes from RecieveBuffer Socket have not been read: Remaining  " + remainingBytes);
                    }



                    //now reset and read again.

                    while (fragmentLength > counter) {
                        Array.Copy(receiveBuffer.buf, 0, newbuffer, counter, lengthOfArrayTobeRead);
                        counter = counter + lengthOfArrayTobeRead;
                        if (fragmentLength == counter) {
                            break;
                        }
                        if (logger.isLoggable(Level.FINEST)) {
                            logger.finest("\n" + " About to read more bytes from socket , current counter is: " + counter);
                        }

                        receiveBuffer.reset();
                        transport.receive(receiveBuffer); //now read again so as to take it from network buffer to your buffer
                                                          //this may actually read 2 or more packets , one is this partial one (now complete) and one may be some other one , like a request packet.
                                                          //or it may not ...and reads only the partial packet.
                        if (fragmentLength - counter >= receiveBuffer.length) {
                            lengthOfArrayTobeRead = receiveBuffer.length;
                        }
                        else {
                            //this would be the last one. Now we need to trim the buffer to it's read length as well.
                            lengthOfArrayTobeRead = fragmentLength - counter;
                            trimSize = receiveBuffer.length - lengthOfArrayTobeRead;
                        }

                        if (logger.isLoggable(Level.FINEST)) {
                            logger.finest("\n" + "lengthOfArrayTobeRead = " + lengthOfArrayTobeRead + "\n" + "trimSize = " + trimSize + "\n" + "RecieveBuffer current read size: " + receiveBuffer.length);
                            logger.finest("\n\n[RECIEVER BUFFER] and the read packet is dumped below...");
                            var byteArrayOutputStream = new ByteArrayOutputStream();
                            Hexdump.hexdump(new PrintStream(byteArrayOutputStream), receiveBuffer.Buffer, 0, receiveBuffer.length);
                            logger.finest("\n" + byteArrayOutputStream.ToString());

                        }

                    }



                }
                else {
                    if (logger.isLoggable(Level.FINEST)) {
                        logger.finest("\n" + "fragmentLength is less than  receiveBuffer.length");
                    }

                    //Since fragment length is smaller, There might be 2 or more packets in here
                    //just read what is your packet.
                    Array.Copy(receiveBuffer.buf, 0, newbuffer, 0, fragmentLength);
                    //there might be more. Now we need to trim the buffer to it's read length as well.
                    trimSize = receiveBuffer.length - fragmentLength;
                }

                if (trimSize > 0) {
                    if (logger.isLoggable(Level.FINEST)) {
                        logger.finest("\n" + "trimSize = " + trimSize);
                    }

                    Array.Copy(receiveBuffer.buf, receiveBuffer.length - trimSize, receiveBuffer.buf, 0, trimSize);
                    receiveBuffer.length = trimSize;
                    receiveBuffer.index = 0;
                    receiveBuffer.start = 0;
                    bytesRemainingInRecieveBuffer = true; //reciever buffer read more than it should , after we trim only the additionally read bytes will be left.
                                                          //these have to be read in the next call to recieveFragment.
                }

                var bufferToBeUsed = new NdrBuffer(newbuffer, 0) {
                    length = newbuffer.Length //this will be fully utilized  and not left empty.
                };

                if (logger.isLoggable(Level.FINEST)) {
                    logger.finest("\n" + "bufferToBeUsed Size = " + bufferToBeUsed.length);
                    logger.finest("\n\n[bufferToBeUsed] packet is dumped below...");
                    var byteArrayOutputStream = new ByteArrayOutputStream();
                    Hexdump.hexdump(new PrintStream(byteArrayOutputStream), bufferToBeUsed.Buffer, 0, bufferToBeUsed.length);
                    logger.finest("\n" + byteArrayOutputStream.ToString());
                    logger.finest("\n*********************************************************************************");
                }

                //caution , frag length is changed here...it is void of security info.
                processIncoming(bufferToBeUsed);
                bufferToBeUsed.Index = ConnectionOrientedPdu.TYPE_OFFSET;
                type = bufferToBeUsed.dec_ndr_small();

                ConnectionOrientedPdu pdu = null;
                switch (type) {
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
                pdu.decode(ndr, bufferToBeUsed);
                return pdu;


            }
            //socket has been closed.
            throw new IOException("Socket Closed"); //Vikram


        }

		private bool isValidType(int type)
		{
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void processIncoming(ndr.NdrBuffer buffer) throws java.io.IOException
		protected internal virtual void processIncoming(NdrBuffer buffer)
		{
			buffer.Index = ConnectionOrientedPdu.TYPE_OFFSET;
			var logMsg = true;
			switch (buffer.dec_ndr_small())
			{
			case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
				if (logMsg)
				{
					logger.info("\n Recieved BIND_ACK");
					logMsg = false;
				}

				goto case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE;
			case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
				if (logMsg)
				{
					logger.info("\n Recieved ALTER_CTX_RESP");
					logMsg = false;
				}

				goto case BindPdu.BIND_TYPE;
			case BindPdu.BIND_TYPE:
				if (logMsg)
				{
					logger.info("\n Recieved BIND");
					logMsg = false;
				}

				goto case AlterContextPdu.ALTER_CONTEXT_TYPE;
			case AlterContextPdu.ALTER_CONTEXT_TYPE:
				if (logMsg)
				{
					logger.info("\n Recieved ALTER_CTX");
					logMsg = false;
				}

				var verifier = detachAuthentication(buffer);
				if (verifier != null)
				{
					incomingRebind(verifier);
				}
				break;

			case FaultCoPdu.FAULT_TYPE:
				if (logMsg)
				{
					logger.info("\n Recieved FAULT");
					logMsg = false;
				}

				goto case CancelCoPdu.CANCEL_TYPE;
			case CancelCoPdu.CANCEL_TYPE:
				if (logMsg)
				{
					logger.info("\n Recieved CANCEL");
					logMsg = false;
				}

				goto case OrphanedPdu.ORPHANED_TYPE;
			case OrphanedPdu.ORPHANED_TYPE:
				if (logMsg)
				{
					logger.info("\n Recieved ORPHANED");
					logMsg = false;
				}

				goto case ResponseCoPdu.RESPONSE_TYPE;
			case ResponseCoPdu.RESPONSE_TYPE:
				if (logMsg)
				{
					logger.info("\n Recieved RESPONSE");
					logMsg = false;
				}

				goto case RequestCoPdu.REQUEST_TYPE;
			case RequestCoPdu.REQUEST_TYPE:
				if (logMsg)
				{
					logger.info("\n Recieved REQUEST");
					logMsg = false;
				}

				if (security != null)
				{
                        var ndr2 = new NetworkDataRepresentation {
                            Buffer = buffer
                        };
                        verifyAndUnseal(ndr2);
				}
				else
				{
					detachAuthentication(buffer); //just strip the information , do not use it.
				}
				break;
			case Auth3Pdu.AUTH3_TYPE:
				if (logMsg)
				{
					logger.info("\n Recieved AUTH3");
					logMsg = false;
				}

				incomingRebind(detachAuthentication2(buffer));
				break;

			case BindNoAcknowledgePdu.BIND_NO_ACKNOWLEDGE_TYPE:
			case ShutdownPdu.SHUTDOWN_TYPE:
				return;
			default:
				throw new RpcException("Invalid incoming PDU type.");
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void processOutgoing() throws java.io.IOException
		protected internal virtual void processOutgoing()
		{
			ndr.Buffer.Index = ConnectionOrientedPdu.TYPE_OFFSET;
			var logMsg = true;
			switch (ndr.readUnsignedSmall())
			{


			case BindPdu.BIND_TYPE:
				if (logMsg)
				{
					logger.info("\n Sending BIND");
					logMsg = false;
				}
				goto case Auth3Pdu.AUTH3_TYPE;
			case Auth3Pdu.AUTH3_TYPE:
				if (logMsg)
				{
					logger.info("\n Sending AUTH3");
					logMsg = false;
				}

				goto case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE;
			case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
				if (logMsg)
				{
					logger.info("\n Sending BIND_ACK");
					logMsg = false;
				}



				goto case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE;
			case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
				if (logMsg)
				{
					logger.info("\n Sending ALTER_CTX_RESP");
					logMsg = false;
				}

				var verifier = outgoingRebind();
				if (verifier != null)
				{
					attachAuthentication(verifier);
				}

				break;
			case AlterContextPdu.ALTER_CONTEXT_TYPE:
				if (logMsg)
				{
					logger.info("\n Sending ALTER_CTX");
					logMsg = false;
				}
				break;
			case RequestCoPdu.REQUEST_TYPE:
				if (logMsg)
				{
					logger.info("\n Sending REQUEST");
					logMsg = false;
				}
	//        	verifier = outgoingRebind();
	//            if (verifier != null) attachAuthentication(verifier);
				goto case CancelCoPdu.CANCEL_TYPE;
			case CancelCoPdu.CANCEL_TYPE:
				if (logMsg)
				{
					logger.info("\n Sending CANCEL");
					logMsg = false;
				}

				goto case OrphanedPdu.ORPHANED_TYPE;
			case OrphanedPdu.ORPHANED_TYPE:
				if (logMsg)
				{
					logger.info("\n Sending ORPHANED");
					logMsg = false;
				}

				goto case FaultCoPdu.FAULT_TYPE;
			case FaultCoPdu.FAULT_TYPE:
				if (logMsg)
				{
					logger.info("\n Sending FAULT");
					logMsg = false;
				}

				goto case ResponseCoPdu.RESPONSE_TYPE;
			case ResponseCoPdu.RESPONSE_TYPE:
				if (logMsg)
				{
					logger.info("\n Sending RESPONSE");
					logMsg = false;
				}

				if (security != null)
				{
					signAndSeal(ndr);
				}
				break;
			case BindNoAcknowledgePdu.BIND_NO_ACKNOWLEDGE_TYPE:
			case ShutdownPdu.SHUTDOWN_TYPE:
				return;
			default:
				throw new RpcException("Invalid outgoing PDU type.");
			}
		}

		protected internal virtual Security Security {
            set => security = value;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private void attachAuthentication(rpc.core.AuthenticationVerifier verifier) throws java.io.IOException
        private void attachAuthentication(AuthenticationVerifier verifier)
		{
			try
			{
				var buffer = ndr.Buffer;
				var length = buffer.Length;
				buffer.Index = length;
				verifier.encode(ndr, buffer);
				length = buffer.Length;
				buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
				ndr.writeUnsignedShort(length);
				ndr.writeUnsignedShort(verifier.body.Length);
			   // buffer.setIndex(ConnectionOrientedPdu.FLAGS_OFFSET);
			   // ndr.writeUnsignedSmall(0);
			}
			catch (Exception ex)
			{
				throw new IOException("Error attaching authentication to PDU: " + ex.Message);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private rpc.core.AuthenticationVerifier detachAuthentication2(ndr.NdrBuffer buffer) throws java.io.IOException
		private AuthenticationVerifier detachAuthentication2(NdrBuffer buffer)
		{
			try
			{
				//NdrBuffer buffer = ndr.getBuffer();
				buffer.Index = ConnectionOrientedPdu.AUTH_LENGTH_OFFSET;
				var length = buffer.dec_ndr_short(); //ndr.readUnsignedShort(); // auth body size
				var index = 20;
				buffer.Index = index; //exactly at the auth type.
				var verifier = new AuthenticationVerifier(length);
				verifier.decode(ndr, buffer);
				buffer.Index = index + 2; // auth padding
				length = index - buffer.dec_ndr_small(); //ndr.readUnsignedSmall();
				buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
				buffer.enc_ndr_short(length);
				buffer.enc_ndr_short(0);
				//ndr.writeUnsignedShort(length);
				//ndr.writeUnsignedShort(0);
				buffer.Index = length;
				return verifier;
			}
			catch (Exception ex)
			{
				throw new IOException("Error stripping authentication from PDU: " + ex);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private rpc.core.AuthenticationVerifier detachAuthentication(ndr.NdrBuffer buffer) throws java.io.IOException
		private AuthenticationVerifier detachAuthentication(NdrBuffer buffer)
		{
			try
			{
				//NdrBuffer buffer = ndr.getBuffer();
				buffer.Index = ConnectionOrientedPdu.AUTH_LENGTH_OFFSET;
				var length = buffer.dec_ndr_short(); //ndr.readUnsignedShort(); // auth body size

				if (length == 0)
				{
					if (logger.isLoggable(Level.FINEST))
					{
						logger.finest("\n" + "In [detachAuthentication] No authn info present...");
					}
					return null;
				}

				var index = buffer.Length - length - 8; // 8 = auth header size
				buffer.Index = index;
				var verifier = new AuthenticationVerifier(length);
				verifier.decode(ndr, buffer);
				buffer.Index = index + 2; // auth padding
				length = index - buffer.dec_ndr_small(); //ndr.readUnsignedSmall();
				buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
				buffer.enc_ndr_short(length);
				buffer.enc_ndr_short(0);
				buffer.Index = length;
				if (logger.isLoggable(Level.FINEST))
				{
					logger.finest("\n" + "In [detachAuthentication] (after stripping authn info) setting new FRAG_LENGTH_OFFSET for the packet as = " + length);
				}

				return verifier;
			}
			catch (Exception ex)
			{
				throw new IOException("Error stripping authentication from PDU: " + ex);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void signAndSeal(ndr.NetworkDataRepresentation ndr) throws java.io.IOException
		private void signAndSeal(NetworkDataRepresentation ndr)
		{
			var protectionLevel = security.ProtectionLevel;
			if (protectionLevel < Security_Fields.PROTECTION_LEVEL_INTEGRITY)
			{
				return;
			}
			var verifierLength = security.VerifierLength;
			var verifier = new AuthenticationVerifier(security.AuthenticationService, protectionLevel, contextId, verifierLength);
			var buffer = ndr.Buffer;
			var length = buffer.Length;
			buffer.Index = length;
			verifier.encode(ndr, buffer);
			length = buffer.Length;
			buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
			ndr.writeUnsignedShort(length);
			ndr.writeUnsignedShort(verifierLength);
			var verifierIndex = length - verifierLength;
			length -= verifierLength + 8; // less verifier + header
			var index = ConnectionOrientedPdu.HEADER_LENGTH;
			buffer.Index = ConnectionOrientedPdu.TYPE_OFFSET;
			switch (ndr.readUnsignedSmall())
			{
			case RequestCoPdu.REQUEST_TYPE:
				index += 8;
				buffer.Index = ConnectionOrientedPdu.FLAGS_OFFSET;
				if ((ndr.readUnsignedSmall() & ConnectionOrientedPdu.PFC_OBJECT_UUID) != 0)
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
			var flags = ndr.readUnsignedSmall();
			if ((flags & ConnectionOrientedPdu.PFC_FIRST_FRAG) == ConnectionOrientedPdu.PFC_FIRST_FRAG && (flags & ConnectionOrientedPdu.PFC_LAST_FRAG) == ConnectionOrientedPdu.PFC_LAST_FRAG)
			{
				isFragmented = false;
			}
			length -= index;
			security.processOutgoing(ndr, index, length, verifierIndex,isFragmented);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void verifyAndUnseal(ndr.NetworkDataRepresentation ndr) throws java.io.IOException
		private void verifyAndUnseal(NetworkDataRepresentation ndr)
		{
			var buffer = ndr.Buffer;
			buffer.Index = ConnectionOrientedPdu.AUTH_LENGTH_OFFSET;
			var verifierLength = ndr.readUnsignedShort();
			if (verifierLength <= 0)
			{
				return;
			}
			var verifierIndex = buffer.Length - verifierLength;
			var length = verifierIndex - 8;
			var index = ConnectionOrientedPdu.HEADER_LENGTH;
			buffer.Index = ConnectionOrientedPdu.TYPE_OFFSET;
			switch (ndr.readUnsignedSmall())
			{
			case RequestCoPdu.REQUEST_TYPE:
				index += 8;
				buffer.Index = ConnectionOrientedPdu.FLAGS_OFFSET;
				if ((ndr.readUnsignedSmall() & ConnectionOrientedPdu.PFC_OBJECT_UUID) != 0)
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
			var flags = ndr.readUnsignedSmall();
			if ((flags & ConnectionOrientedPdu.PFC_FIRST_FRAG) == ConnectionOrientedPdu.PFC_FIRST_FRAG && (flags & ConnectionOrientedPdu.PFC_LAST_FRAG) == ConnectionOrientedPdu.PFC_LAST_FRAG)
			{
				isFragmented = false;
			}

			security.processIncoming(ndr, index, length, verifierIndex,isFragmented);
			buffer.Index = verifierIndex - 6; // auth padding field
			length = verifierIndex - ndr.readUnsignedSmall() - 8;
			buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
			// "doctor" the PDU by removing the auth and padding
			ndr.writeUnsignedShort(length);
			ndr.writeUnsignedShort(0);
			buffer.length = length;
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void incomingRebind(rpc.core.AuthenticationVerifier verifier) throws java.io.IOException
		protected internal virtual void incomingRebind(AuthenticationVerifier verifier)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected rpc.core.AuthenticationVerifier outgoingRebind() throws java.io.IOException
		protected internal virtual AuthenticationVerifier outgoingRebind()
		{
			return null;
		}

	}

}