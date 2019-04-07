using System;
using System.Collections;

/// <summary>
/// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
/// 
/// j-Interop (Pure Java implementation of DCOM protocol)
/// 
/// Copyright (c) 2013 Vikram Roopchand
/// 
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the Eclipse Public License v1.0
/// which accompanies this distribution, and is available at
/// http://www.eclipse.org/legal/epl-v10.html
/// 
/// Contributors:
/// Vikram Roopchand  - Moving to EPL from LGPL v1.
/// 
/// </summary>

namespace rpc {


	using Hexdump = jcifs.util.Hexdump;
	using NdrBuffer = ndr.NdrBuffer;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;
	using AuthenticationVerifier = rpc.core.AuthenticationVerifier;
	using AlterContextPdu = rpc.pdu.AlterContextPdu;
	using AlterContextResponsePdu = rpc.pdu.AlterContextResponsePdu;
	using Auth3Pdu = rpc.pdu.Auth3Pdu;
	using BindAcknowledgePdu = rpc.pdu.BindAcknowledgePdu;
	using BindNoAcknowledgePdu = rpc.pdu.BindNoAcknowledgePdu;
	using BindPdu = rpc.pdu.BindPdu;
	using CancelCoPdu = rpc.pdu.CancelCoPdu;
	using FaultCoPdu = rpc.pdu.FaultCoPdu;
	using OrphanedPdu = rpc.pdu.OrphanedPdu;
	using RequestCoPdu = rpc.pdu.RequestCoPdu;
	using ResponseCoPdu = rpc.pdu.ResponseCoPdu;
	using ShutdownPdu = rpc.pdu.ShutdownPdu;

	public class DefaultConnection : Connection {

		protected internal NetworkDataRepresentation Ndr;

		protected internal NdrBuffer TransmitBuffer;

		protected internal NdrBuffer ReceiveBuffer;

		protected internal Security Security_Renamed;

		protected internal int ContextId;

		private static readonly Logger Logger = Logger.getLogger("org.jinterop");

		public DefaultConnection() : this(ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE) {
		}

		public DefaultConnection(int transmitLength, int receiveLength) {
			Ndr = new NetworkDataRepresentation();
			TransmitBuffer = new NdrBuffer(new sbyte[transmitLength], 0);
			ReceiveBuffer = new NdrBuffer(new sbyte[receiveLength], 0);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void transmit(ConnectionOrientedPdu pdu, Transport transport) throws java.io.IOException
		public virtual void Transmit(ConnectionOrientedPdu pdu, Transport transport) {
			if (!(pdu is Fragmentable)) {
				TransmitFragment(pdu, transport);
				return;
			}
			IEnumerator fragments = ((Fragmentable) pdu).Fragment(TransmitBuffer.Capacity);
			while (fragments.hasNext()) {
				TransmitFragment((ConnectionOrientedPdu) fragments.next(), transport);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ConnectionOrientedPdu receive(final Transport transport) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		public virtual ConnectionOrientedPdu Receive(Transport transport) {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ConnectionOrientedPdu fragment = receiveFragment(transport);
			ConnectionOrientedPdu fragment = ReceiveFragment(transport);
			if (!(fragment is Fragmentable) || fragment.GetFlag(ConnectionOrientedPdu.PFC_LAST_FRAG)) {
				return fragment;
			}
			return (ConnectionOrientedPdu)((Fragmentable) fragment).Assemble(new IteratorAnonymousInnerClassHelper(this, transport, fragment));
		}

		private class IteratorAnonymousInnerClassHelper : IEnumerator {
			private readonly DefaultConnection OuterInstance;

			private rpc.Transport Transport;
			private rpc.ConnectionOrientedPdu Fragment;

			public IteratorAnonymousInnerClassHelper(DefaultConnection outerInstance, rpc.Transport transport, rpc.ConnectionOrientedPdu fragment) {
				this.OuterInstance = outerInstance;
				this.Transport = transport;
				this.Fragment = fragment;
				currentFragment = fragment;
				i = 0;
			}

			internal ConnectionOrientedPdu currentFragment;
			public virtual bool HasNext() {
				return (currentFragment != null);
			}
			private int i;
			public virtual object Next() {
				if (currentFragment == null) {
					throw new NoSuchElementException();
				}
				try {
					return currentFragment;
				}
				finally {
					if (currentFragment.getFlag(ConnectionOrientedPdu.PFC_LAST_FRAG)) {
						currentFragment = null;
					}
					else {
						try {
							//fragLengthOfReceiveBuffer = -1;//clear the buffer here.
							//System.out.println("VIKRAM VIKRAM ");
							if (Logger.isLoggable(Level.FINEST)) {
								Logger.finest("[Fragmented Packet] [" + i++ + "] recieved , fragment decomposition is below:- ");
							}
							currentFragment = outerInstance.ReceiveFragment(Transport);
						}
						catch (Exception ex) {
							Console.WriteLine(ex.ToString());
							Console.Write(ex.StackTrace);
							throw new System.InvalidOperationException();
						}
					}
				}
			}
			public virtual void Remove() {
				throw new System.NotSupportedException();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void transmitFragment(ConnectionOrientedPdu fragment, Transport transport) throws java.io.IOException
		public virtual void TransmitFragment(ConnectionOrientedPdu fragment, Transport transport) {
			TransmitBuffer.Reset();

			fragment.Encode(Ndr, TransmitBuffer);

			ProcessOutgoing();


			//jcifs.util.Hexdump.hexdump(System.err, transmitBuffer.getBuffer(), 0, transmitBuffer.length);
			if (Logger.isLoggable(Level.FINEST)) {
				ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
				Hexdump.hexdump(new PrintStream(byteArrayOutputStream), TransmitBuffer.Buffer, 0, TransmitBuffer.Length_Renamed);
				Logger.finest("[TRANSMIT BUFFER]:-\n" + byteArrayOutputStream.ToString());
			}
			transport.Send(TransmitBuffer);
		}


		private bool BytesRemainingInRecieveBuffer = false;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected ConnectionOrientedPdu receiveFragment(Transport transport) throws java.io.IOException
		public virtual ConnectionOrientedPdu ReceiveFragment(Transport transport) {

			int fragmentLength = -1;
			int type = -1;
			bool read = true;

			if (BytesRemainingInRecieveBuffer) {
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
						while (ReceiveBuffer.Length_Renamed <= 10) {
							//perform a read again in a new buffer and assign that to the reciever buffer
							//this needs to be a small buffer 10 bytes
							NdrBuffer tmpBuffer = new NdrBuffer(new sbyte[10], 0);
							transport.Receive(tmpBuffer);
							Array.Copy(tmpBuffer.Buf, 0, ReceiveBuffer.Buf, ReceiveBuffer.Length_Renamed, tmpBuffer.Length_Renamed);
							ReceiveBuffer.Length_Renamed = ReceiveBuffer.Length_Renamed + tmpBuffer.Length_Renamed;
						}
						read = false;
					}
			}

				BytesRemainingInRecieveBuffer = false;
			}

			//will be true for all cases and false if anything valid is already in the buffer
			if (read) {
				//read the transport now...
				ReceiveBuffer.Reset();
				if (Logger.isLoggable(Level.FINEST)) {
					Logger.finest("\n" + " Reading bytes from RecieveBuffer Socket...Current Capacity:- " + ReceiveBuffer.Capacity);
				}

				transport.Receive(ReceiveBuffer);

				if (Logger.isLoggable(Level.FINEST)) {
					Logger.finest("[RECIEVER BUFFER] Full packet is dumped below...");
					ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
					Hexdump.hexdump(new PrintStream(byteArrayOutputStream), ReceiveBuffer.Buffer, 0, ReceiveBuffer.Length_Renamed);
					Logger.finest("\n" + byteArrayOutputStream.ToString());
					Logger.finest("\n" + " Bytes read from RecieveBuffer Socket:- " + ReceiveBuffer.Length_Renamed);
				}

			}

			sbyte[] newbuffer = null;
			int counter = 0;
			int trimSize = -1;
			int lengthOfArrayTobeRead = ReceiveBuffer.Length_Renamed;
			//frag length logic
			if (ReceiveBuffer.Length_Renamed > 0) {
				ReceiveBuffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
				sbyte[] frag = new sbyte[2]; //short
				ReceiveBuffer.ReadOctetArray(frag, 0, frag.Length);
				fragmentLength = ((frag[0] & 0xFF) | ((frag[1] & 0xFF) << 8)); //receiveBuffer.dec_ndr_short(); is looping over.
	//			fragmentLength = receiveBuffer.dec_ndr_short(); 
				if (Logger.isLoggable(Level.FINEST)) {
					Logger.finest("\n" + " length of the fragment " + fragmentLength + "\n" + " size in bytes of the buffer [] " + ReceiveBuffer.Buf.Length);
				}

				if (fragmentLength < 0) {
					int h = 0;
					h++;
					Hexdump.hexdump(System.out, ReceiveBuffer.Buf, 0, ReceiveBuffer.Buf.Length);
				}
				//the new buffer should be equal to fragment size
				newbuffer = new sbyte[fragmentLength];

				if (fragmentLength > ReceiveBuffer.Length_Renamed) { //this means the socket buffer is not fully read, this packet is bigger than the reciever buffer size
					int remainingBytes = fragmentLength - ReceiveBuffer.Length_Renamed;
					if (Logger.isLoggable(Level.FINEST)) {
						Logger.finest("\n" + " Some bytes from RecieveBuffer Socket have not been read: Remaining  " + remainingBytes);
					}



					//now reset and read again.

					while (fragmentLength > counter) {
						Array.Copy(ReceiveBuffer.Buf,0,newbuffer,counter,lengthOfArrayTobeRead);
						counter = counter + lengthOfArrayTobeRead;
						if (fragmentLength == counter) {
							break;
						}
						if (Logger.isLoggable(Level.FINEST)) {
							Logger.finest("\n" + " About to read more bytes from socket , current counter is: " + counter);
						}

						ReceiveBuffer.Reset();
						transport.Receive(ReceiveBuffer); //now read again so as to take it from network buffer to your buffer
						//this may actually read 2 or more packets , one is this partial one (now complete) and one may be some other one , like a request packet.
						//or it may not ...and reads only the partial packet.
						if (fragmentLength - counter >= ReceiveBuffer.Length_Renamed) {
							lengthOfArrayTobeRead = ReceiveBuffer.Length_Renamed;
						}
						else {
							//this would be the last one. Now we need to trim the buffer to it's read length as well.
							lengthOfArrayTobeRead = fragmentLength - counter;
							trimSize = ReceiveBuffer.Length_Renamed - lengthOfArrayTobeRead;
						}

						if (Logger.isLoggable(Level.FINEST)) {
							Logger.finest("\n" + "lengthOfArrayTobeRead = " + lengthOfArrayTobeRead + "\n" + "trimSize = " + trimSize + "\n" + "RecieveBuffer current read size: " + ReceiveBuffer.Length_Renamed);
							Logger.finest("\n\n[RECIEVER BUFFER] and the read packet is dumped below...");
							ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
							Hexdump.hexdump(new PrintStream(byteArrayOutputStream), ReceiveBuffer.Buffer, 0, ReceiveBuffer.Length_Renamed);
							Logger.finest("\n" + byteArrayOutputStream.ToString());

						}

					}



				}
				else {
					if (Logger.isLoggable(Level.FINEST)) {
						Logger.finest("\n" + "fragmentLength is less than  receiveBuffer.length");
					}

					//Since fragment length is smaller, There might be 2 or more packets in here
					//just read what is your packet.
					Array.Copy(ReceiveBuffer.Buf,0,newbuffer,0,fragmentLength);
					//there might be more. Now we need to trim the buffer to it's read length as well.
					trimSize = ReceiveBuffer.Length_Renamed - fragmentLength;
				}

				if (trimSize > 0) {
					if (Logger.isLoggable(Level.FINEST)) {
						Logger.finest("\n" + "trimSize = " + trimSize);
					}

					Array.Copy(ReceiveBuffer.Buf,ReceiveBuffer.Length_Renamed - trimSize,ReceiveBuffer.Buf,0,trimSize);
					ReceiveBuffer.Length_Renamed = trimSize;
					ReceiveBuffer.Index_Renamed = 0;
					ReceiveBuffer.Start = 0;
					BytesRemainingInRecieveBuffer = true; //reciever buffer read more than it should , after we trim only the additionally read bytes will be left.
					//these have to be read in the next call to recieveFragment.
				}

				NdrBuffer bufferToBeUsed = new NdrBuffer(newbuffer,0);
				bufferToBeUsed.Length_Renamed = newbuffer.Length; //this will be fully utilized  and not left empty.

				if (Logger.isLoggable(Level.FINEST)) {
					Logger.finest("\n" + "bufferToBeUsed Size = " + bufferToBeUsed.Length_Renamed);
					Logger.finest("\n\n[bufferToBeUsed] packet is dumped below...");
					ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
					Hexdump.hexdump(new PrintStream(byteArrayOutputStream), bufferToBeUsed.Buffer, 0, bufferToBeUsed.Length_Renamed);
					Logger.finest("\n" + byteArrayOutputStream.ToString());
					Logger.finest("\n*********************************************************************************");
				}

				 //caution , frag length is changed here...it is void of security info.
				ProcessIncoming(bufferToBeUsed);
				bufferToBeUsed.Index = ConnectionOrientedPdu.TYPE_OFFSET;
				type = bufferToBeUsed.Dec_ndr_small();

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
				pdu.Decode(Ndr, bufferToBeUsed);
				return pdu;


			}
			else {
				//socket has been closed.
				throw new IOException("Socket Closed"); //Vikram
			}


		}

		private bool IsValidType(int type) {
			switch (type) {
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
		public virtual void ProcessIncoming(NdrBuffer buffer) {
			buffer.Index = ConnectionOrientedPdu.TYPE_OFFSET;
			bool logMsg = true;
			switch (buffer.Dec_ndr_small()) {
			case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
				if (logMsg) {
					Logger.info("\n Recieved BIND_ACK");
					logMsg = false;
				}

				goto case rpc.pdu.AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE;
			case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
				if (logMsg) {
					Logger.info("\n Recieved ALTER_CTX_RESP");
					logMsg = false;
				}

				goto case rpc.pdu.BindPdu.BIND_TYPE;
			case BindPdu.BIND_TYPE:
				if (logMsg) {
					Logger.info("\n Recieved BIND");
					logMsg = false;
				}

				goto case rpc.pdu.AlterContextPdu.ALTER_CONTEXT_TYPE;
			case AlterContextPdu.ALTER_CONTEXT_TYPE:
				if (logMsg) {
					Logger.info("\n Recieved ALTER_CTX");
					logMsg = false;
				}

				AuthenticationVerifier verifier = DetachAuthentication(buffer);
				if (verifier != null) {
					IncomingRebind(verifier);
				}
				break;

			case FaultCoPdu.FAULT_TYPE:
				if (logMsg) {
					Logger.info("\n Recieved FAULT");
					logMsg = false;
				}

				goto case rpc.pdu.CancelCoPdu.CANCEL_TYPE;
			case CancelCoPdu.CANCEL_TYPE:
				if (logMsg) {
					Logger.info("\n Recieved CANCEL");
					logMsg = false;
				}

				goto case rpc.pdu.OrphanedPdu.ORPHANED_TYPE;
			case OrphanedPdu.ORPHANED_TYPE:
				if (logMsg) {
					Logger.info("\n Recieved ORPHANED");
					logMsg = false;
				}

				goto case rpc.pdu.ResponseCoPdu.RESPONSE_TYPE;
			case ResponseCoPdu.RESPONSE_TYPE:
				if (logMsg) {
					Logger.info("\n Recieved RESPONSE");
					logMsg = false;
				}

				goto case rpc.pdu.RequestCoPdu.REQUEST_TYPE;
			case RequestCoPdu.REQUEST_TYPE:
				if (logMsg) {
					Logger.info("\n Recieved REQUEST");
					logMsg = false;
				}

				if (Security_Renamed != null) {
					NetworkDataRepresentation ndr2 = new NetworkDataRepresentation();
					ndr2.Buffer = buffer;
					VerifyAndUnseal(ndr2);
				}
				else {
					DetachAuthentication(buffer); //just strip the information , do not use it.
				}
				break;
			case Auth3Pdu.AUTH3_TYPE:
				if (logMsg) {
					Logger.info("\n Recieved AUTH3");
					logMsg = false;
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void processOutgoing() throws java.io.IOException
		public virtual void ProcessOutgoing() {
			Ndr.Buffer.Index = ConnectionOrientedPdu.TYPE_OFFSET;
			bool logMsg = true;
			switch (Ndr.ReadUnsignedSmall()) {


			case BindPdu.BIND_TYPE:
				if (logMsg) {
					Logger.info("\n Sending BIND");
					logMsg = false;
				}
				goto case rpc.pdu.Auth3Pdu.AUTH3_TYPE;
			case Auth3Pdu.AUTH3_TYPE:
				if (logMsg) {
					Logger.info("\n Sending AUTH3");
					logMsg = false;
				}

				goto case rpc.pdu.BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE;
			case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
				if (logMsg) {
					Logger.info("\n Sending BIND_ACK");
					logMsg = false;
				}



				goto case rpc.pdu.AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE;
			case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
				if (logMsg) {
					Logger.info("\n Sending ALTER_CTX_RESP");
					logMsg = false;
				}

				AuthenticationVerifier verifier = OutgoingRebind();
				if (verifier != null) {
					AttachAuthentication(verifier);
				}

				break;
			case AlterContextPdu.ALTER_CONTEXT_TYPE:
				if (logMsg) {
					Logger.info("\n Sending ALTER_CTX");
					logMsg = false;
				}
				break;
			case RequestCoPdu.REQUEST_TYPE:
				if (logMsg) {
					Logger.info("\n Sending REQUEST");
					logMsg = false;
				}
	//        	verifier = outgoingRebind();
	//            if (verifier != null) attachAuthentication(verifier);
				goto case rpc.pdu.CancelCoPdu.CANCEL_TYPE;
			case CancelCoPdu.CANCEL_TYPE:
				if (logMsg) {
					Logger.info("\n Sending CANCEL");
					logMsg = false;
				}

				goto case rpc.pdu.OrphanedPdu.ORPHANED_TYPE;
			case OrphanedPdu.ORPHANED_TYPE:
				if (logMsg) {
					Logger.info("\n Sending ORPHANED");
					logMsg = false;
				}

				goto case rpc.pdu.FaultCoPdu.FAULT_TYPE;
			case FaultCoPdu.FAULT_TYPE:
				if (logMsg) {
					Logger.info("\n Sending FAULT");
					logMsg = false;
				}

				goto case rpc.pdu.ResponseCoPdu.RESPONSE_TYPE;
			case ResponseCoPdu.RESPONSE_TYPE:
				if (logMsg) {
					Logger.info("\n Sending RESPONSE");
					logMsg = false;
				}

				if (Security_Renamed != null) {
					SignAndSeal(Ndr);
				}
				break;
			case BindNoAcknowledgePdu.BIND_NO_ACKNOWLEDGE_TYPE:
			case ShutdownPdu.SHUTDOWN_TYPE:
				return;
			default:
				throw new RpcException("Invalid outgoing PDU type.");
			}
		}

		public virtual Security Security {
			set {
				this.Security_Renamed = value;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void attachAuthentication(rpc.core.AuthenticationVerifier verifier) throws java.io.IOException
		private void AttachAuthentication(AuthenticationVerifier verifier) {
			try {
				NdrBuffer buffer = Ndr.Buffer;
				int length = buffer.Length;
				buffer.Index = length;
				verifier.Encode(Ndr, buffer);
				length = buffer.Length;
				buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
				Ndr.WriteUnsignedShort(length);
				Ndr.WriteUnsignedShort(verifier.Body.Length);
			   // buffer.setIndex(ConnectionOrientedPdu.FLAGS_OFFSET);
			   // ndr.writeUnsignedSmall(0);
			}
			catch (Exception ex) {
				throw new IOException("Error attaching authentication to PDU: " + ex.Message);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private rpc.core.AuthenticationVerifier detachAuthentication2(ndr.NdrBuffer buffer) throws java.io.IOException
		private AuthenticationVerifier DetachAuthentication2(NdrBuffer buffer) {
			try {
				//NdrBuffer buffer = ndr.getBuffer();
				buffer.Index = ConnectionOrientedPdu.AUTH_LENGTH_OFFSET;
				int length = buffer.Dec_ndr_short(); //ndr.readUnsignedShort(); // auth body size
				int index = 20;
				buffer.Index = index; //exactly at the auth type.
				AuthenticationVerifier verifier = new AuthenticationVerifier(length);
				verifier.Decode(Ndr, buffer);
				buffer.Index = index + 2; // auth padding
				length = index - buffer.Dec_ndr_small(); //ndr.readUnsignedSmall();
				buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
				buffer.Enc_ndr_short(length);
				buffer.Enc_ndr_short(0);
				//ndr.writeUnsignedShort(length);
				//ndr.writeUnsignedShort(0);
				buffer.Index = length;
				return verifier;
			}
			catch (Exception ex) {
				throw new IOException("Error stripping authentication from PDU: " + ex);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private rpc.core.AuthenticationVerifier detachAuthentication(ndr.NdrBuffer buffer) throws java.io.IOException
		private AuthenticationVerifier DetachAuthentication(NdrBuffer buffer) {
			try {
				//NdrBuffer buffer = ndr.getBuffer();
				buffer.Index = ConnectionOrientedPdu.AUTH_LENGTH_OFFSET;
				int length = buffer.Dec_ndr_short(); //ndr.readUnsignedShort(); // auth body size

				if (length == 0) {
					if (Logger.isLoggable(Level.FINEST)) {
						Logger.finest("\n" + "In [detachAuthentication] No authn info present...");
					}
					return null;
				}

				int index = buffer.Length - length - 8; // 8 = auth header size
				buffer.Index = index;
				AuthenticationVerifier verifier = new AuthenticationVerifier(length);
				verifier.Decode(Ndr, buffer);
				buffer.Index = index + 2; // auth padding
				length = index - buffer.Dec_ndr_small(); //ndr.readUnsignedSmall();
				buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
				buffer.Enc_ndr_short(length);
				buffer.Enc_ndr_short(0);
				buffer.Index = length;
				if (Logger.isLoggable(Level.FINEST)) {
					Logger.finest("\n" + "In [detachAuthentication] (after stripping authn info) setting new FRAG_LENGTH_OFFSET for the packet as = " + length);
				}

				return verifier;
			}
			catch (Exception ex) {
				throw new IOException("Error stripping authentication from PDU: " + ex);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void signAndSeal(ndr.NetworkDataRepresentation ndr) throws java.io.IOException
		private void SignAndSeal(NetworkDataRepresentation ndr) {
			int protectionLevel = Security_Renamed.ProtectionLevel;
			if (protectionLevel < Security_Fields.PROTECTION_LEVEL_INTEGRITY) {
				return;
			}
			int verifierLength = Security_Renamed.VerifierLength;
			AuthenticationVerifier verifier = new AuthenticationVerifier(Security_Renamed.AuthenticationService, protectionLevel, ContextId, verifierLength);
			NdrBuffer buffer = ndr.Buffer;
			int length = buffer.Length;
			buffer.Index = length;
			verifier.Encode(ndr, buffer);
			length = buffer.Length;
			buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
			ndr.WriteUnsignedShort(length);
			ndr.WriteUnsignedShort(verifierLength);
			int verifierIndex = length - verifierLength;
			length -= verifierLength + 8; // less verifier + header
			int index = ConnectionOrientedPdu.HEADER_LENGTH;
			buffer.Index = ConnectionOrientedPdu.TYPE_OFFSET;
			switch (ndr.ReadUnsignedSmall()) {
			case RequestCoPdu.REQUEST_TYPE:
				index += 8;
				buffer.Index = ConnectionOrientedPdu.FLAGS_OFFSET;
				if ((ndr.ReadUnsignedSmall() & ConnectionOrientedPdu.PFC_OBJECT_UUID) != 0) {
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
			bool isFragmented = true;
			buffer.Index = ConnectionOrientedPdu.FLAGS_OFFSET;
			int flags = ndr.ReadUnsignedSmall();
			if ((flags & ConnectionOrientedPdu.PFC_FIRST_FRAG) == ConnectionOrientedPdu.PFC_FIRST_FRAG && (flags & ConnectionOrientedPdu.PFC_LAST_FRAG) == ConnectionOrientedPdu.PFC_LAST_FRAG) {
				isFragmented = false;
			}
			length -= index;
			Security_Renamed.ProcessOutgoing(ndr, index, length, verifierIndex,isFragmented);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void verifyAndUnseal(ndr.NetworkDataRepresentation ndr) throws java.io.IOException
		private void VerifyAndUnseal(NetworkDataRepresentation ndr) {
			NdrBuffer buffer = ndr.Buffer;
			buffer.Index = ConnectionOrientedPdu.AUTH_LENGTH_OFFSET;
			int verifierLength = ndr.ReadUnsignedShort();
			if (verifierLength <= 0) {
				return;
			}
			int verifierIndex = buffer.Length - verifierLength;
			int length = verifierIndex - 8;
			int index = ConnectionOrientedPdu.HEADER_LENGTH;
			buffer.Index = ConnectionOrientedPdu.TYPE_OFFSET;
			switch (ndr.ReadUnsignedSmall()) {
			case RequestCoPdu.REQUEST_TYPE:
				index += 8;
				buffer.Index = ConnectionOrientedPdu.FLAGS_OFFSET;
				if ((ndr.ReadUnsignedSmall() & ConnectionOrientedPdu.PFC_OBJECT_UUID) != 0) {
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

			bool isFragmented = true;
			buffer.Index = ConnectionOrientedPdu.FLAGS_OFFSET;
			int flags = ndr.ReadUnsignedSmall();
			if ((flags & ConnectionOrientedPdu.PFC_FIRST_FRAG) == ConnectionOrientedPdu.PFC_FIRST_FRAG && (flags & ConnectionOrientedPdu.PFC_LAST_FRAG) == ConnectionOrientedPdu.PFC_LAST_FRAG) {
				isFragmented = false;
			}

			Security_Renamed.ProcessIncoming(ndr, index, length, verifierIndex,isFragmented);
			buffer.Index = verifierIndex - 6; // auth padding field
			length = verifierIndex - ndr.ReadUnsignedSmall() - 8;
			buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
			// "doctor" the PDU by removing the auth and padding
			ndr.WriteUnsignedShort(length);
			ndr.WriteUnsignedShort(0);
			buffer.Length_Renamed = length;
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void incomingRebind(rpc.core.AuthenticationVerifier verifier) throws java.io.IOException
		public virtual void IncomingRebind(AuthenticationVerifier verifier) {
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected rpc.core.AuthenticationVerifier outgoingRebind() throws java.io.IOException
		public virtual AuthenticationVerifier OutgoingRebind() {
			return null;
		}

	}

}