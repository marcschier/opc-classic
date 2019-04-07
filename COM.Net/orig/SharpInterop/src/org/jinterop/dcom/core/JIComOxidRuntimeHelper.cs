using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
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
/// Vikram Roopchand  - Moving to EPL from LGPL v3.
/// 
/// </summary>

namespace org.jinterop.dcom.core {


	using SmbAuthException = jcifs.smb.SmbAuthException;
	using SmbException = jcifs.smb.SmbException;
	using NdrBuffer = ndr.NdrBuffer;
	using NdrException = ndr.NdrException;
	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	using IJICOMRuntimeWorker = org.jinterop.dcom.common.IJICOMRuntimeWorker;
	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JIException = org.jinterop.dcom.common.JIException;
	using JIRuntimeException = org.jinterop.dcom.common.JIRuntimeException;
	using JISystem = org.jinterop.dcom.common.JISystem;
	using JIComRuntimeEndpoint = org.jinterop.dcom.transport.JIComRuntimeEndpoint;
	using JIComRuntimeTransportFactory = org.jinterop.dcom.transport.JIComRuntimeTransportFactory;

	using Stub = rpc.Stub;
	using UUID = rpc.core.UUID;

	//import com.iwombat.foundation.IdentifierFactory;
	//import com.iwombat.util.GUIDUtil;



	/// <summary>
	///Used to manipulate Oxid details. one instance is created per binding
	/// call to the oxid resolver. 
	/// 
	/// @since 1.0
	/// 
	/// </summary>
	internal sealed class JIComOxidRuntimeHelper : Stub {



		public JIComOxidRuntimeHelper(Properties properties) {
			base.TransportFactory = JIComRuntimeTransportFactory.SingleTon;
			base.Properties = properties;
			base.Address = "127.0.0.1[135]"; //this is never consulted so , putting localhost here.
		}

		public string Syntax {
			get {
				//return "99fcfec4-5260-101b-bbcb-00aa0021347a:0.0";//IOxidResolver IID
				return UUID.NIL_UUID + ":0.0"; //returning nothing
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void startOxid(int portNumLocal,int portNumRemote) throws java.io.IOException
		public void StartOxid(int portNumLocal, int portNumRemote) {
			Thread oxidResolverThread = new Thread(new RunnableAnonymousInnerClassHelper(this),"jI_OxidResolver_Client[" + portNumLocal + " , " + portNumRemote + "]");
			oxidResolverThread.Daemon = true;
			oxidResolverThread.Start();
		}

		private class RunnableAnonymousInnerClassHelper : Runnable {
			private readonly JIComOxidRuntimeHelper OuterInstance;

			public RunnableAnonymousInnerClassHelper(JIComOxidRuntimeHelper outerInstance) {
				this.OuterInstance = outerInstance;
			}

			public virtual void Run() {
				try {
					if (JISystem.Logger.isLoggable(Level.INFO)) {
						JISystem.Logger.info("started startOxid thread: " + Thread.CurrentThread.Name);
					}
					attach();
					((JIComRuntimeEndpoint)Endpoint).ProcessRequests(new OxidResolverImpl(Properties),null,new List<object>());
				}
				catch (Exception e) {
					if (JISystem.Logger.isLoggable(Level.WARNING)) {
						JISystem.Logger.throwing("Oxid Resolver Thread", "run", e);
						JISystem.Logger.warning("Oxid Resolver Thread: " + e.Message + " , on thread Id: " + Thread.CurrentThread.Name);
					}
				}
				finally {
					try {
						((JIComRuntimeEndpoint)Endpoint).detach();
					}
					catch (IOException) {
					}
				}
				if (JISystem.Logger.isLoggable(Level.INFO)) {
					JISystem.Logger.info("terminating startOxid thread: " + Thread.CurrentThread.Name);
				}
			}
		}

		//returns the port to which the server is listening.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Object[] startRemUnknown(final String baseIID, final String ipidOfRemUnknown, final String ipidOfComponent, final java.util.List listOfSupportedInterfaces) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		public object[] StartRemUnknown(string baseIID, string ipidOfRemUnknown, string ipidOfComponent, IList listOfSupportedInterfaces) {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.nio.channels.ServerSocketChannel serverSocketChannel = java.nio.channels.ServerSocketChannel.open();
			ServerSocketChannel serverSocketChannel = ServerSocketChannel.open();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.net.ServerSocket serverSocket = serverSocketChannel.socket();
			ServerSocket serverSocket = serverSocketChannel.socket(); //new ServerSocket(0);
	//	    serverSocket.setSoTimeout(120*1000); //2 min timeout.
			serverSocket.bind(null);
			int remUnknownPort = serverSocket.LocalPort;
			//have to pick up a random name so adding the ipid of remunknown this is a uuid so the string is quite random.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ThreadGroup remUnknownForThisListener = new ThreadGroup("ThreadGroup - " + baseIID + "[" + ipidOfRemUnknown + "]");
			ThreadGroup remUnknownForThisListener = new ThreadGroup("ThreadGroup - " + baseIID + "[" + ipidOfRemUnknown + "]");
			remUnknownForThisListener.Daemon = true;
			Thread remUnknownThread = new Thread(remUnknownForThisListener,new RunnableAnonymousInnerClassHelper2(this, baseIID, ipidOfRemUnknown, ipidOfComponent, listOfSupportedInterfaces, serverSocket, remUnknownForThisListener),"jI_RemUnknownListener[" + baseIID + " , " + remUnknownPort + "]");

			remUnknownThread.Daemon = true;
			remUnknownThread.Start();
			return new object[]{ new int?(remUnknownPort),remUnknownForThisListener };
		}

		private class RunnableAnonymousInnerClassHelper2 : Runnable {
			private readonly JIComOxidRuntimeHelper OuterInstance;

			private string BaseIID;
			private string IpidOfRemUnknown;
			private string IpidOfComponent;
			private IList ListOfSupportedInterfaces;
			private ServerSocket ServerSocket;
			private ThreadGroup RemUnknownForThisListener;

			public RunnableAnonymousInnerClassHelper2(JIComOxidRuntimeHelper outerInstance, string baseIID, string ipidOfRemUnknown, string ipidOfComponent, IList listOfSupportedInterfaces, ServerSocket serverSocket, ThreadGroup remUnknownForThisListener) {
				this.OuterInstance = outerInstance;
				this.BaseIID = baseIID;
				this.IpidOfRemUnknown = ipidOfRemUnknown;
				this.IpidOfComponent = ipidOfComponent;
				this.ListOfSupportedInterfaces = listOfSupportedInterfaces;
				this.ServerSocket = serverSocket;
				this.RemUnknownForThisListener = remUnknownForThisListener;
			}

			public virtual void Run() {
				if (JISystem.Logger.isLoggable(Level.INFO)) {
					JISystem.Logger.info("started RemUnknown listener thread for : " + Thread.CurrentThread.Name);
				}
				try {

					while (true) {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.net.Socket socket = serverSocket.accept();
						Socket socket = ServerSocket.accept();
						if (JISystem.Logger.isLoggable(Level.INFO)) {
							JISystem.Logger.info("RemUnknown listener: Got Connection from " + socket.Port);
						}

						//now create the JIComOxidRuntimeHelper Object and start it. We need a new one since the old one is already attached to the listener.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final JIComOxidRuntimeHelper remUnknownHelper = new JIComOxidRuntimeHelper(getProperties());
						JIComOxidRuntimeHelper remUnknownHelper = new JIComOxidRuntimeHelper(Properties);
						lock (JIComOxidRuntime.Mutex) {
							JISystem.Internal_setSocket(socket);
							remUnknownHelper.attach();
						}

						//now start a new thread with this socket 
						Thread remUnknown = new Thread(RemUnknownForThisListener,new RunnableAnonymousInnerClassHelper3(this, remUnknownHelper),"jI_RemUnknown[" + BaseIID + " , L(" + socket.LocalPort + "):R(" + socket.Port + ")]");
						remUnknown.Daemon = true;
						remUnknown.Start();
					}
				}
				catch (ClosedByInterruptException) {
					JISystem.Logger.info("JIComOxidRuntimeHelper RemUnknownListener" + Thread.CurrentThread.Name + " is purposefully closed by interruption.");
				}
				catch (IOException e) {
					if (JISystem.Logger.isLoggable(Level.WARNING)) {
						JISystem.Logger.log(Level.WARNING,"JIComOxidRuntimeHelper RemUnknownListener",e);
						JISystem.Logger.warning("RemUnknownListener Thread: " + e.Message + " , on thread Id: " + (Thread.CurrentThread.Name));
					}
					//e.printStackTrace();
				}
				catch (Exception e) {
					if (JISystem.Logger.isLoggable(Level.WARNING)) {
						JISystem.Logger.log(Level.WARNING,"JIComOxidRuntimeHelper RemUnknownListener",e);
					}
				}


				if (JISystem.Logger.isLoggable(Level.INFO)) {
					JISystem.Logger.info("terminating RemUnknownListener thread: " + Thread.CurrentThread.Name);
				}
			}

			private class RunnableAnonymousInnerClassHelper3 : Runnable {
				private readonly RunnableAnonymousInnerClassHelper2 OuterInstance;

				private org.jinterop.dcom.core.JIComOxidRuntimeHelper RemUnknownHelper;

				public RunnableAnonymousInnerClassHelper3(RunnableAnonymousInnerClassHelper2 outerInstance, org.jinterop.dcom.core.JIComOxidRuntimeHelper remUnknownHelper) {
					this.outerInstance = outerInstance;
					this.RemUnknownHelper = remUnknownHelper;
				}

				public virtual void Run() {
					try {
						((JIComRuntimeEndpoint)RemUnknownHelper.Endpoint).ProcessRequests(new RemUnknownObject(OuterInstance.IpidOfRemUnknown,OuterInstance.IpidOfComponent),OuterInstance.BaseIID,OuterInstance.ListOfSupportedInterfaces);
					}
					catch (SmbAuthException e) {
						JISystem.Logger.log(Level.WARNING,"JIComOxidRuntimeHelper RemUnknownThread (not listener)",e);
						throw new JIRuntimeException(JIErrorCodes.JI_CALLBACK_AUTH_FAILURE);
					}
					catch (SmbException e) {
						//System.out.println(e.getMessage());
						JISystem.Logger.log(Level.WARNING,"JIComOxidRuntimeHelper RemUnknownThread (not listener)",e);
						throw new JIRuntimeException(JIErrorCodes.JI_CALLBACK_SMB_FAILURE);
					}
					catch (ClosedByInterruptException) {
						JISystem.Logger.info("JIComOxidRuntimeHelper RemUnknownThread (not listener)" + Thread.CurrentThread.Name + " is purposefully closed by interruption.");
					}
					catch (IOException e) {
						JISystem.Logger.log(Level.WARNING,"JIComOxidRuntimeHelper RemUnknownThread (not listener)",e);
					}
					finally {
						try {
							RemUnknownHelper.detach();
						}
						catch (IOException) {
						}
					}

				}
			}
		}
	}

	//This object should have serialized access only , i.e at a time only 1 read --> write , cycle should happen
	// it is not multithreaded safe.
	internal class OxidResolverImpl : NdrObject, IJICOMRuntimeWorker {
		//override read\write\opnum etc. here, use the util apis to decompose this.
		private int Opnum_Renamed = -1;
		private NdrBuffer Buffer = null;
		private Properties p = null;
		public OxidResolverImpl(Properties p) : base() {
			this.p = p;
		}

		public virtual UUID CurrentObjectID {
			set {
				//does nothing.
			}
			get {
				return null;
			}
		}
	//	public void setCurrentJavaInstanceFromIID(String iid)
	//	{
	//		//does nothing.
	//	}

		public virtual int Opnum {
			set {
				this.Opnum_Renamed = value;
			}
			get {
				return Opnum_Renamed;
			}
		}


		public virtual void Write(NetworkDataRepresentation ndr) {
			ndr.Buffer = Buffer; //this buffer is prepared via read.
		}

		public virtual void Read(NetworkDataRepresentation ndr) {
			//will read according to the opnum. The setOpnum should have been called before this
			//call.	

			switch (Opnum_Renamed) {
				case 1:
					Buffer = SimplePing(ndr);
					break;
				case 2:
					Buffer = ComplexPing(ndr);
					break;
				case 3: //ServerAlive
					Buffer = ServerAlive(ndr);
					break;
				case 5: //This is ServerAlive2
					Buffer = ServerAlive2(ndr);
					break;
				case 4: //This is ResolveOxid2
					Buffer = ResolveOxid2(ndr);
					break;
				default: //should not have arrived here.
					if (JISystem.Logger.isLoggable(Level.WARNING)) {
						JISystem.Logger.warning("Oxid Object: DEFAULTED !!!");
					}
					throw new JIRuntimeException(JIErrorCodes.RPC_S_PROCNUM_OUT_OF_RANGE);
			}


		}

		private Random Random = new Random(DateTimeHelperClass.CurrentUnixTimeMillis());


		private NdrBuffer SimplePing(NetworkDataRepresentation ndr) {
			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("Oxid Object: SimplePing");
			}
			sbyte[] b = JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr,8); //setid
			JIComOxidRuntime.AddUpdateSets(new JISetId(b),new List<object>(),new List<object>());
			Buffer = new NdrBuffer(new sbyte[16],0);
			Buffer.enc_ndr_long(0);
			Buffer.enc_ndr_long(0);
			Buffer.enc_ndr_long(0);
			Buffer.enc_ndr_long(0);
			return Buffer;
		}

		private NdrBuffer ComplexPing(NetworkDataRepresentation ndr) {
			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("Oxid Object: ComplexPing");
			}
			sbyte[] b = JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr,8); //setid
			JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(short?),null,JIFlags.FLAG_NULL,null); //seqId.
			short? lengthAdds = (short?)JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(short?),null,JIFlags.FLAG_NULL,null);
			short? lengthDels = (short?)JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(short?),null,JIFlags.FLAG_NULL,null);
			JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(int?),null,JIFlags.FLAG_NULL,null);

			JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(int?),null,JIFlags.FLAG_NULL,null); //length
			List<object> listOfAdds = new List<object>();
			for (int i = 0; i < (int)lengthAdds; i++) {
				listOfAdds.Add(new JIObjectId(JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr,8),false));
			}

			JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(int?),null,JIFlags.FLAG_NULL,null); //length
			List<object> listOfDels = new List<object>();
			for (int i = 0; i < (int)lengthDels; i++) {
				listOfDels.Add(new JIObjectId(JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr,8),false));
			}

			if (Arrays.Equals(b,new sbyte[]{ 0,0,0,0,0,0,0,0 })) {
				Random.NextBytes(b);
			}

			JIComOxidRuntime.AddUpdateSets(new JISetId(b),listOfAdds,listOfDels);

			Buffer = new NdrBuffer(new sbyte[32],0);
			NetworkDataRepresentation ndr2 = new NetworkDataRepresentation();
			ndr2.Buffer = Buffer;

			JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr2,b);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(short?),new short?((short)0),null,JIFlags.FLAG_NULL);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?),new int?(0),null,JIFlags.FLAG_NULL); //hresult
			return Buffer;
		}

		private NdrBuffer ServerAlive(NetworkDataRepresentation ndr) {
			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("Oxid Object: ServerAlive");
			}
			sbyte[] buffer = new sbyte[32]; //16 + 16=just in case
			NdrBuffer ndrBuffer = new NdrBuffer(buffer,0);
			ndrBuffer.enc_ndr_long(0);
			ndrBuffer.enc_ndr_long(0);
			ndrBuffer.enc_ndr_long(0);
			ndrBuffer.enc_ndr_long(0);
			return ndrBuffer;
		}
		private NdrBuffer ServerAlive2(NetworkDataRepresentation ndr) {
			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("Oxid Object: ServerAlive2");
			}
			//there is no in params for this.
			//only out params

			//want no port information associated with this.
	//		byte[] buffer = new byte[120];
	//		FileInputStream inputStream;
	//		try {
	//			inputStream = new FileInputStream("c:/serveralive2");
	//			inputStream.read(buffer,0,120);
	//		} catch (Exception e) {
	//			// TODO Auto-generated catch block
	//			e.printStackTrace();
	//		}
	//		
	//		NdrBuffer ndrBuffer = new NdrBuffer(buffer,0);

			JIDualStringArray dualStringArray = new JIDualStringArray(-1);

			sbyte[] buffer = new sbyte[dualStringArray.Length + 4 + 16 + 16]; //just in case - 2 unknown 8 bytes - COMVERSION
			NdrBuffer ndrBuffer = new NdrBuffer(buffer,0);


			NetworkDataRepresentation ndr2 = new NetworkDataRepresentation();
			ndr2.Buffer = ndrBuffer;

			//serialize COMVERSION
	//		JIMarshalUnMarshalHelper.serialize(ndr2,Short.class, new Short((short)JISystem.getCOMVersion().getMajorVersion()),null,JIFlags.FLAG_NULL);
	//		JIMarshalUnMarshalHelper.serialize(ndr2,Short.class, new Short((short)JISystem.getCOMVersion().getMinorVersion()),null,JIFlags.FLAG_NULL);

			//Vikram June 19th 2013: Forcing the JILocalCoClass's server to 5.4. This is so that we stay at 5.4 DCOM until we upgrade the 
			//local server to 5.7 as well.
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(short?), new short?((short)5),null,JIFlags.FLAG_NULL);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(short?), new short?((short)4),null,JIFlags.FLAG_NULL);

			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), new int?(0),null,JIFlags.FLAG_NULL);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), new int?(dualStringArray.Length),null,JIFlags.FLAG_NULL);
			dualStringArray.Encode(ndr2);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), new int?(0),null,JIFlags.FLAG_NULL);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), new int?(0),null,JIFlags.FLAG_NULL);
			return ndrBuffer;
		}
		//will prepare a NdrBuffer for reply to this call 
		private NdrBuffer ResolveOxid2(NetworkDataRepresentation ndr) {
			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("Oxid Object: ResolveOxid2");
			}
			//System.err.println("VIKRAM: resolve oxid thread Id = " + Thread.currentThread().getId());
			//first read the OXID, then consult the oxid master about it's details.
			JIOxid oxid = new JIOxid(JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr,8));

			//now get the RequestedProtoSeq length.
			int length = (int)((short?)JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(short?),null,JIFlags.FLAG_NULL,null));

			//now for the array.
			JIArray array = (JIArray)JIMarshalUnMarshalHelper.DeSerialize(ndr,new JIArray(typeof(short?),null,1,true),null,JIFlags.FLAG_REPRESENTATION_ARRAY,null);

			//now query the Resolver master for this data.
			JIComOxidDetails details = JIComOxidRuntime.GetOxidDetails(oxid);

			if (details == null) {
				//not found, now throw an JIRuntimeException , so that a FaultPdu could be sent.
				throw new JIRuntimeException(JIErrorCodes.RPC_E_INVALID_OXID);
			}

	//		byte[] buffer = new byte[424];
	//		FileInputStream inputStream;
	//		try {
	//			inputStream = new FileInputStream("c:/resolveoxid2");
	//			inputStream.read(buffer,0,424);
	//		} catch (Exception e) {
	//			// TODO Auto-generated catch block
	//			e.printStackTrace();
	//		}
	//		
	//		try {
	//			details.getCOMRuntimeHelper().startRemUnknown();
	//		} catch (IOException e) {
	//			// TODO Auto-generated catch block
	//			e.printStackTrace();
	//		}
	//		
	//		NdrBuffer ndrBuffer = new NdrBuffer(buffer,0);
	//		

			//randomly create IPID and send, this is the ipid of the remunknown, we store it with remunknown object
	//        UUID uuid = details.getRemUnknownIpid() == null ? new UUID(GUIDUtil.guidStringFromHexString(IdentifierFactory.createUniqueIdentifier().toHexString())) : new UUID(details.getRemUnknownIpid());
			UUID uuid = details.RemUnknownIpid == null ? new UUID(java.util.UUID.randomUUID().ToString()) : new UUID(details.RemUnknownIpid);

			//create the bindings for this Java Object.
			//this port will go in the new bindings sent to the COM client.
			int port = -1;
			try {
				//this is so that repeated calls for Oxid resolution return the same rem unknwon.
				port = details.PortForRemUnknown;
				if (port == -1) {
					string remunknownipid = uuid.ToString();
					object[] portandthread = details.COMRuntimeHelper.StartRemUnknown(details.IID,remunknownipid,details.Ipid, details.Referent.SupportedInterfaces);
					port = (int)((int?)portandthread[0]);
					details.RemUnknownThreadGroup = (ThreadGroup)portandthread[1];
					details.RemUnknownIpid = remunknownipid;
				}
				details.PortForRemUnknown = port;
			}
			catch (IOException) {

				throw new JIRuntimeException(JIErrorCodes.E_UNEXPECTED);
			}

			//can support only TCP connections
			//JIDualStringArray.test = true;
			JIDualStringArray dualStringArray = new JIDualStringArray(port);


			int? authnHint = new int?(details.ProtectionLevel);


			sbyte[] buffer = new sbyte[4 + 4 + dualStringArray.Length + 16 + 4 + 2 + 2 + 4 + 16];

			//have all data now prepare the response
			//the response expected here is defines the byte array size.
			NdrBuffer ndrBuffer = new NdrBuffer(buffer,0);

			NetworkDataRepresentation ndr2 = new NetworkDataRepresentation();
			ndr2.Buffer = ndrBuffer;

			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), new int?((new object()).GetHashCode()),null,JIFlags.FLAG_NULL);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), new int?((dualStringArray.Length - 4) / 2),null,JIFlags.FLAG_NULL);
			dualStringArray.Encode(ndr2);

			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(UUID), uuid,null,JIFlags.FLAG_NULL);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), authnHint,null,JIFlags.FLAG_NULL);
	//		JIMarshalUnMarshalHelper.serialize(ndr2,Short.class, new Short((short)JISystem.getCOMVersion().getMajorVersion()),null,JIFlags.FLAG_NULL);
	//		JIMarshalUnMarshalHelper.serialize(ndr2,Short.class, new Short((short)JISystem.getCOMVersion().getMinorVersion()),null,JIFlags.FLAG_NULL);

			//Vikram June 19th 2013: Forcing the JILocalCoClass's server to 5.4. This is so that we stay at 5.4 DCOM until we upgrade the 
			//local server to 5.7 as well.
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(short?), new short?((short)5),null,JIFlags.FLAG_NULL);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(short?), new short?((short)4),null,JIFlags.FLAG_NULL);

			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), new int?(0),null,JIFlags.FLAG_NULL); //hresult


			return ndrBuffer;
		}

		public virtual IList QIedIIDs {
			get {
				return null;
			}
		}


		public virtual bool Resolver {
			get {
				return true;
			}
		}

		public virtual string CurrentIID {
			set {
				//does nothing
			}
		}

		public virtual bool WorkerOver() {
			//oxid resolver gets over when the client connected to it releases socket.
			return false;
		}
	}

	//This object should have serialized access only , i.e at a time only 1 read --> write , cycle should happen
	//it is not multithreaded safe.
	internal class RemUnknownObject : NdrObject, IJICOMRuntimeWorker {
		//override read\write\opnum etc. here, use the util apis to decompose this.
		private int Opnum_Renamed = -1;
		private NdrBuffer Buffer = null;

		//component tells you the JILocalCoClass to act on , sent via the AlterContext calls
		//for all Altercontexts with IRemUnknown , this will be null.
		private JILocalCoClass Component = null; //will hold the current instance to act on.
		/* the component and object id duo work together. 1 component could export many ipids.
		 * 
		 */
		//ObjectID tells you the IPID to act on, sent via the Request calls
		private UUID ObjectId = null;

		//this would be the ipid of this RemUnknownObject
		private readonly string SelfIPID;

		private string CurrentIID_Renamed = null;

		private IList ListOfIIDsQIed = new List<object>();

		public RemUnknownObject(string ipidOfme, string ipidOfComponent) {
			SelfIPID = ipidOfme;
			MapOfIpidsVsRef[ipidOfComponent.ToUpper()] = new int?(5);
		}

		//this list will get cleared after this call.
		public virtual IList QIedIIDs {
			get {
				return ListOfIIDsQIed;
			}
		}

		 public virtual bool Resolver {
			 get {
				return false;
			 }
		 }

		public virtual int Opnum {
			set {
				this.Opnum_Renamed = value;
			}
			get {
				return Opnum_Renamed;
			}
		}


		public virtual void Write(NetworkDataRepresentation ndr) {
			ndr.Buffer = Buffer; //this buffer is prepared via read.
		}

		private static readonly JIStruct RemInterfaceRef = new JIStruct();
		static RemUnknownObject() {
			try {
				RemInterfaceRef.AddMember(typeof(UUID));
				RemInterfaceRef.AddMember(typeof(int?));
				RemInterfaceRef.AddMember(typeof(int?));
			}
			catch (JIException shouldnothappen) {
				JISystem.Logger.throwing("RemUnknownObject", "Static Initialiser", shouldnothappen);
			}
		}
		private static readonly JIArray RemInterfaceRefArray = new JIArray(RemInterfaceRef,null,1,true);

		private IDictionary MapOfIpidsVsRef = new Hashtable();
		private bool WorkerOver_Renamed = false;

		public virtual void Read(NetworkDataRepresentation ndr) {
			//will read according to the opnum. The setOpnum should have been called before this
			//call.	
			string ipid = ObjectId.ToString();

	//		if (!mapOfIpidsVsRef.containsKey(ipid.toUpperCase()))
	//		{
	//		    System.out.println(Thread.currentThread() + " -->> " + ipid.toUpperCase());
	//		    //we always give 5 references
	//		    mapOfIpidsVsRef.put(ipid.toUpperCase(),new Integer(5));
	//		}

			//this means the call came for IRemUnknown apis, since selfIpid is null or matches the objectID
			//if (selfIPID == null || selfIPID.equalsIgnoreCase(ipid))
	//		if ("00000131-0000-0000-C000-000000000046".equalsIgnoreCase(currentIID))
			if (SelfIPID.Equals(ipid, StringComparison.CurrentCultureIgnoreCase)) {
				switch (Opnum_Renamed) {
					case 3: //IRemUnknown QI.
						Buffer = QueryInterface(ndr);
						break;
					case 4: //addref
							JIOrpcThis.Decode(ndr);
							int length = ndr.readUnsignedShort();

							int[] retvals = new int[length];
							JIArray array = (JIArray)JIMarshalUnMarshalHelper.DeSerialize(ndr, RemInterfaceRefArray, new List<object>(), JIFlags.FLAG_REPRESENTATION_ARRAY, new Hashtable());
							//saving the ipids with there references. considering public + private references together for now.
							JIStruct[] structs = (JIStruct[])array.ArrayInstance;
							for (int i = 0;i < length;i++) {
								string ipidref = ((UUID)structs[i].GetMember(0)).ToString().ToUpper();
								int publicRefs = (int)((int?)structs[i].GetMember(1));
								int privateRefs = (int)((int?)structs[i].GetMember(2));

								if (!MapOfIpidsVsRef.Contains(ipidref)) {
									//this would be strange, since all the ipids we give should be part of the map already.
									//have to set 0x80000003 (INVALID ARG here)
									retvals[i] = unchecked((int)0x80000003);
									continue;
								}
								else {
									// StoredIQ - Satwik - native C++ says 01 here 
									retvals[i] = 0x1;
								}


								int total = (int)((int?)MapOfIpidsVsRef.GetValueOrNull(ipidref)) + publicRefs + privateRefs;
								MapOfIpidsVsRef[ipidref] = new int?(total);
							}


							//preparing the response
							Buffer = new NdrBuffer(new sbyte[length * 4 + 16],0);
							NetworkDataRepresentation ndr2 = new NetworkDataRepresentation();
							ndr2.Buffer = Buffer;
							JIOrpcThat.Encode(ndr2);
							for (int i = 0;i < length;i++) {
								Buffer.enc_ndr_long(retvals[i]);
							}

							Buffer.enc_ndr_long(0);
							Buffer.enc_ndr_long(0);

						break;
					case 5: //release


						JIOrpcThis.Decode(ndr);
						length = ndr.readUnsignedShort();
						array = (JIArray)JIMarshalUnMarshalHelper.DeSerialize(ndr, RemInterfaceRefArray, new List<object>(), JIFlags.FLAG_REPRESENTATION_ARRAY, new Hashtable());
						//saving the ipids with there references. considering public + private references together for now.
						structs = (JIStruct[])array.ArrayInstance;
						for (int i = 0;i < length;i++) {
							string ipidref = ((UUID)structs[i].GetMember(0)).ToString().ToUpper();
							int publicRefs = (int)((int?)structs[i].GetMember(1));
							int privateRefs = (int)((int?)structs[i].GetMember(2));
							if (!MapOfIpidsVsRef.Contains(ipidref)) {
								continue;
							}

							int total = (int)((int?)MapOfIpidsVsRef.GetValueOrNull(ipidref)) - publicRefs - privateRefs;
							if (total == 0) {
								MapOfIpidsVsRef.Remove(ipidref);
							}
							else {
								MapOfIpidsVsRef[ipidref] = new int?(total);
							}
						}

						//all references to all IPIDs exported are over, this is now done.
						if (MapOfIpidsVsRef.Count == 0) {
							WorkerOver_Renamed = true;
						}

						//I have 1 OID == 1 IPID == 1 java instance.
						Buffer = new NdrBuffer(new sbyte[32],0);
						ndr2 = new NetworkDataRepresentation();
						ndr2.Buffer = Buffer;
						JIOrpcThat.Encode(ndr2);
						Buffer.enc_ndr_long(0);
						Buffer.enc_ndr_long(0);
						break;
					default:
						throw new JIRuntimeException(JIErrorCodes.RPC_S_PROCNUM_OUT_OF_RANGE);
				}
			}
			else {
				//now use the objectId , just set in before this call to read. That objectId is the IPID on which the
				//call is being made , and was previously exported during Q.I. The component value was filled during an
				//alter context or bind, again made some calls before.
				if (Component == null) {
					JISystem.Logger.severe("JIComOxidRuntimeHelper RemUnknownObject read(): component is null , opnum is " + Opnum_Renamed + " , IPID is " + ipid + " , selfIpid is " + SelfIPID);
				}
				sbyte[] b = null;
				object result = null;
				NetworkDataRepresentation ndr2 = new NetworkDataRepresentation();
				int hresult = 0;
				object[] retArray = null;
				try {
					result = Component.InvokeMethod(ipid,Opnum_Renamed,ndr);
				}
				catch (JIException e) {
					hresult = e.ErrorCode;
					JISystem.Logger.severe("Exception occured: " + e.ErrorCode);
					JISystem.Logger.throwing("RemUnknownObject","read",e);
				}


				//now if opnum was 6 then this is a dispatch call , so response has to be dispatch response
				//not the normal one.
				if (Component.GetInterfaceDefinitionFromIPID(ipid).DispInterface && Opnum_Renamed == 6) {
					object result2 = result;
					//orpcthat
					//[out] VARIANT * pVarResult,
					//[out] EXCEPINFO * pExcepInfo,
					//[out] UINT * pArgErr,
					//[in, out, size_is(cVarRef)] VARIANTARG * rgVarRef
					result = new object[4]; //orpcthat gets filled outside
					JIStruct excepInfo = new JIStruct();
					try {
						excepInfo.AddMember(new short?((short)0));
						excepInfo.AddMember(new short?((short)0));
						excepInfo.AddMember(new JIString(""));
						excepInfo.AddMember(new JIString(""));
						excepInfo.AddMember(new JIString(""));
						excepInfo.AddMember(new int?(0));
						excepInfo.AddMember(new JIPointer(null,true));
						excepInfo.AddMember(new JIPointer(null,true));
						excepInfo.AddMember(new int?(0));
					}
					catch (JIException e)
					{ //not expecting any here
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}

					if (result2 == null) {
						((object[])result)[0] = JIVariant.EMPTY();
					}
					else {
						//now check whether the variant is by ref or not.
						JIVariant variant = (JIVariant)((object[])result2)[0];

						try {
							if (variant.ByRefFlagSet) {
								//add empty inplace of this.
								((object[])result)[0] = JIVariant.EMPTY();
								//now update the array at the end.
								((object[])result)[3] = new JIArray(new JIVariant[]{ variant },true);

							}
							else {
								((object[])result)[0] = ((object[])result2)[0]; //will have only a single index.
								((object[])result)[3] = new int?(0); //Array
							}
						}
						catch (JIException e) {
							throw new JIRuntimeException(e.ErrorCode);
						}
					}

					((object[])result)[1] = excepInfo;

					((object[])result)[2] = new int?(0); //argErr is null, for now.


					retArray = (object[]) result;

				}


				Buffer = new NdrBuffer(b,0);
				ndr2.Buffer = Buffer;

				//JIOrpcThat.encode(ndr2);
				//have to create a call Object, since these return types could be structs , unions etc. having deffered pointers 
				JICallBuilder callObject = new JICallBuilder();
				callObject.AttachSession(Component.Session);
				if (result != null) {

					if (retArray != null) {
						//serialize all members sequentially.
						for (int i = 0;i < retArray.Length;i++) {
							callObject.AddInParamAsObject(retArray[i],JIFlags.FLAG_NULL);
						}
					}
					else {
						//serialize all members sequentially.
						for (int i = 0;i < ((object[])result).Length;i++) {
							callObject.AddInParamAsObject(((object[])result)[i],JIFlags.FLAG_NULL);
						}

					}



				}
				callObject.Write2(ndr2);
				JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?),new int?(hresult),null,JIFlags.FLAG_NULL);




			}


		}


		private NdrBuffer QueryInterface(NetworkDataRepresentation ndr) {
			//now to decompose all

			if (JISystem.Logger.isLoggable(Level.FINEST)) {
				JISystem.Logger.finest("Within RemUnknownObject: QueryInterface");
				JISystem.Logger.finest("RemUnknownObject: [QI] Before call terminated listOfIIDsQIed are: " + ListOfIIDsQIed);
			}
			JIOrpcThis.Decode(ndr);

			//now get the IPID and export the component with a new IPID and IID. 
			UUID ipid = new UUID();
			try {
				ipid.decode(ndr,ndr.Buffer);
			}
			catch (NdrException e) {
				JISystem.Logger.throwing("JIComOxidRuntimeHelper","QueryInterface",e);
			}

			if (JISystem.Logger.isLoggable(Level.FINEST)) {
				JISystem.Logger.finest("RemUnknownObject: [QI] IPID is " + ipid);
			}
			//set the JILocalCoClass., the ipid should not be null in this call.
			JIComOxidDetails details = JIComOxidRuntime.GetComponentFromIPID(ipid.ToString());

			if (details == null) {
				//not found, now throw an JIRuntimeException , so that a FaultPdu could be sent.
				throw new JIRuntimeException(JIErrorCodes.RPC_E_INVALID_OXID);
			}

			JILocalCoClass component = details.Referent;

			if (JISystem.Logger.isLoggable(Level.FINEST)) {
				JISystem.Logger.finest("RemUnknownObject: [QI] JIJavcCoClass is " + component.CoClassIID);
			}

			(int)((int?)(JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(int?),null, JIFlags.FLAG_NULL,null))); //refs , don't really care about this.

			int length = (int)((short?)(JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(short?),null, JIFlags.FLAG_NULL,null))); //length of the requested Interfaces

			JIArray array = (JIArray)JIMarshalUnMarshalHelper.DeSerialize(ndr,new JIArray(typeof(UUID),null,1,true),null,JIFlags.FLAG_REPRESENTATION_ARRAY,null);

			//now to build the buffer and export the IIDs with new IPIDs
			sbyte[] b = new sbyte[8 + 4 + 4 + length * (4 + 4 + 40) + 16];
			NdrBuffer buffer = new NdrBuffer(b,0);

			//start with response
			NetworkDataRepresentation ndr2 = new NetworkDataRepresentation();
			ndr2.Buffer = buffer;

			JIOrpcThat.Encode(ndr2);

			//pointer
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?),new int?((new object()).GetHashCode()),null,JIFlags.FLAG_NULL);
			//length of array
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?),new int?(length),null,JIFlags.FLAG_NULL);

			object[] arrayOfUUIDs = (object[])array.ArrayInstance;

			for (int i = 0; i < arrayOfUUIDs.Length; i++) {
				UUID iid = (UUID)arrayOfUUIDs[i];
				if (JISystem.Logger.isLoggable(Level.FINEST)) {
					JISystem.Logger.finest("RemUnknownObject: [QI] Array iid[" + i + "] is " + iid);
				}
				//now for each QueryResult
				try {
					int hresult = 0;
	//				String ipid2 = GUIDUtil.guidStringFromHexString(IdentifierFactory.createUniqueIdentifier().toHexString());
					string ipid2 = java.util.UUID.randomUUID().ToString();
					if (!component.IsPresent(iid.ToString())) {
						hresult = JIErrorCodes.E_NOINTERFACE;
	//					ipid2 = GUIDUtil.guidStringFromHexString("00000000000000000000000000000000");
						ipid2 = java.util.UUID.fromString("00000000000000000000000000000000").ToString();
					}
					else {
						string tmpIpid = null;
						try {
							tmpIpid = component.GetIpidFromIID(iid.ToString());
						}
						catch (Exception e) {
							JISystem.Logger.throwing("JIComOxidRuntimeHelper", "QueryInterface", e);
						}

						if (tmpIpid == null) {
							if (JISystem.Logger.isLoggable(Level.FINEST)) {
								JISystem.Logger.finest("RemUnknownObject: [QI] tmpIpid is null for iid " + iid);
							}
							component.ExportInstance(iid.ToString(), ipid2);
						}
						else {
							if (JISystem.Logger.isLoggable(Level.FINEST)) {
								JISystem.Logger.finest("RemUnknownObject: [QI] tmpIpid is NOT null for iid " + iid + " and ipid sent back is " + ipid2);
							}
							ipid2 = tmpIpid;
						}
					}
					//hresult
					JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?),new int?(hresult),null,JIFlags.FLAG_NULL);
					JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?),new int?(unchecked((int)0xCCCCCCCC)),null,JIFlags.FLAG_NULL);

					//now generate the IPID and export a java instance with this.
					JIStdObjRef objRef = null;
					if (hresult == 0) {
						objRef = new JIStdObjRef(ipid2,details.Oxid,details.Oid);
					}
					else {
						objRef = new JIStdObjRef(ipid2);
					}
					objRef.Encode(ndr2);

					//add it to the exported Ipids map
					if (hresult == 0) {
						MapOfIpidsVsRef[ipid2.ToUpper()] = new int?(objRef.PublicRefs);
					}

					if (JISystem.Logger.isLoggable(Level.FINEST)) {
						JISystem.Logger.finest("RemUnknownObject: [QI] for which the stdObjRef is " + objRef);
					}

				}
				catch (IllegalAccessException e) {
					JISystem.Logger.throwing("JIComOxidRuntimeHelper","QueryInterface",e);
				}
				catch (InstantiationException e) {
					JISystem.Logger.throwing("JIComOxidRuntimeHelper","QueryInterface",e);
				}

				string iidtemp = iid.ToString().ToUpper() + ":0.0";
				if (!ListOfIIDsQIed.Contains(iidtemp)) {
					ListOfIIDsQIed.Add(iidtemp);
				}
			}

			if (JISystem.Logger.isLoggable(Level.FINEST)) {
				JISystem.Logger.finest("RemUnknownObject: [QI] After call terminated listOfIIDsQIed are: " + ListOfIIDsQIed);
			}

			return buffer;
		}


		//for all remunknown methods and calls component is null, alter context for IRemUnknown will make this
		//null.
	//	public void setCurrentJavaInstanceFromIID(String  iid)
	//	{
	//		int i = iid.indexOf(":");
	//		if (i != -1)
	//		{
	//			iid = iid.substring(0,i);
	//		}
	//		this.component = JIComOxidRuntime.getJavaComponentForIID(iid);
	//		if (component == null)
	//		{
	//			objectId = null;
	//		}
	//	}

		public virtual UUID CurrentObjectID {
			set {
				this.ObjectId = value;
				Component = JIComOxidRuntime.GetJavaComponentFromIPID(value.ToString());
			}
			get {
				return ObjectId;
			}
		}


		public virtual string CurrentIID {
			set {
				this.CurrentIID_Renamed = value;
    
			}
		}

		public virtual bool WorkerOver() {
			return WorkerOver_Renamed;
		}
	}
}