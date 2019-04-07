// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.core
{


	using SmbAuthException = SharpCifs.smb.SmbAuthException;
	using SmbException = SharpCifs.smb.SmbException;
	using NdrBuffer = SharpCifs.Dcerpc.Ndr.NdrBuffer;
	using NdrException = SharpCifs.Dcerpc.Ndr.NdrException;
	using NdrOp = SharpCifs.Dcerpc.Ndr.NdrOp;
	using NdrCodec = SharpCifs.Dcerpc.Ndr.NdrCodec;

	using IJICOMRuntimeWorker = common.IJICOMRuntimeWorker;
	using JIErrorCodes = common.JIErrorCodes;
	using JIException = common.JIException;
	using JIRuntimeException = common.JIRuntimeException;
	using JISystem = common.JISystem;
	using JIComRuntimeEndpoint = transport.JIComRuntimeEndpoint;
	using JIComRuntimeTransportFactory = transport.JIComRuntimeTransportFactory;

	using Stub = rpc.Stub;
	using UUID = rpc.core.UUID;
    using System.IO;
    using System;
    using Serilog;
    using System.Collections;

    //import com.iwombat.foundation.IdentifierFactory;
    //import com.iwombat.util.GUIDUtil;



    /// <summary>
    ///Used to manipulate Oxid details. one instance is created per binding
    /// call to the oxid resolver. 
    /// 
    /// @since 1.0
    /// 
    /// </summary>
    internal sealed class JIComOxidRuntimeHelper : Stub
	{



		internal JIComOxidRuntimeHelper(SharpCifs.Util.Sharpen.Properties properties)
		{
            TransportFactory = JIComRuntimeTransportFactory.SingleTon;
            SharpCifs.Util.Sharpen.Properties = properties;
            Address = "127.0.0.1[135]"; //this is never consulted so , putting localhost here.
		}

        protected internal string Syntax =>
                //return "99fcfec4-5260-101b-bbcb-00aa0021347a:0.0";//IOxidResolver IID
                UUID.NIL_UUID + ":0.0"; //returning nothing

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: void startOxid(int portNumLocal,int portNumRemote) throws java.io.IOException
        internal void startOxid(int portNumLocal, int portNumRemote)
		{
            var oxidResolverThread = new Thread(new RunnableAnonymousInnerClassHelper(this), "jI_OxidResolver_Client[" + portNumLocal + " , " + portNumRemote + "]") {
                Daemon = true
            };
            oxidResolverThread.Start();
		}

		private class RunnableAnonymousInnerClassHelper : Runnable
		{
			private readonly JIComOxidRuntimeHelper outerInstance;

			public RunnableAnonymousInnerClassHelper(JIComOxidRuntimeHelper outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void run()
			{
				try
				{
                    Log.Logger.Information("started startOxid thread: " + Thread.CurrentThread.Name);
                    Attach();
					((JIComRuntimeEndpoint)Endpoint).processRequests(new OxidResolverImpl(SharpCifs.Util.Sharpen.Properties),null,new ArrayList());
				}
				catch (Exception e)
				{
                    Log.Logger.Error(e, "Oxid Resolver Thread);
                    Log.Logger.Warning("Oxid Resolver Thread: " + e.Message + " , on thread Id: " + Thread.CurrentThread.Name);
                }
                finally
				{
					try
					{
						((JIComRuntimeEndpoint)Endpoint).Detach();
					}
					catch (IOException)
					{
					}
				}
                Log.Logger.Information("terminating startOxid thread: " + Thread.CurrentThread.Name);
            }
        }

		//returns the port to which the server is listening.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Object[] startRemUnknown(final String baseIID, final String ipidOfRemUnknown, final String ipidOfComponent, final java.util.List listOfSupportedInterfaces) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		internal object[] startRemUnknown(string baseIID, string ipidOfRemUnknown, string ipidOfComponent, IList listOfSupportedInterfaces)
		{
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
            var remUnknownForThisListener = new ThreadGroup("ThreadGroup - " + baseIID + "[" + ipidOfRemUnknown + "]") {
                Daemon = true
            };
            var remUnknownThread = new Thread(remUnknownForThisListener, new RunnableAnonymousInnerClassHelper2(this, baseIID, ipidOfRemUnknown, ipidOfComponent, listOfSupportedInterfaces, serverSocket, remUnknownForThisListener), "jI_RemUnknownListener[" + baseIID + " , " + remUnknownPort + "]") {
                Daemon = true
            };
            remUnknownThread.Start();
			return new object[]{ remUnknownPort, remUnknownForThisListener};
		}

		private class RunnableAnonymousInnerClassHelper2 : Runnable
		{
			private readonly JIComOxidRuntimeHelper outerInstance;

			private string baseIID;
			private string ipidOfRemUnknown;
			private string ipidOfComponent;
			private IList listOfSupportedInterfaces;
			private ServerSocket serverSocket;
			private readonly ThreadGroup remUnknownForThisListener;

			public RunnableAnonymousInnerClassHelper2(JIComOxidRuntimeHelper outerInstance, string baseIID, string ipidOfRemUnknown, string ipidOfComponent, IList listOfSupportedInterfaces, ServerSocket serverSocket, ThreadGroup remUnknownForThisListener)
			{
				this.outerInstance = outerInstance;
				this.baseIID = baseIID;
				this.ipidOfRemUnknown = ipidOfRemUnknown;
				this.ipidOfComponent = ipidOfComponent;
				this.listOfSupportedInterfaces = listOfSupportedInterfaces;
				this.serverSocket = serverSocket;
				this.remUnknownForThisListener = remUnknownForThisListener;
			}

			public virtual void run()
			{
                Log.Logger.Information("started RemUnknown listener thread for : " + Thread.CurrentThread.Name);
                try {

					while (true)
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.net.Socket socket = serverSocket.accept();
						Socket socket = serverSocket.accept();
                        Log.Logger.Information("RemUnknown listener: Got Connection from " + socket.Port);

                        //now create the JIComOxidRuntimeHelper Object and start it. We need a new one since the old one is already attached to the listener.
                        //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                        //ORIGINAL LINE: final JIComOxidRuntimeHelper remUnknownHelper = new JIComOxidRuntimeHelper(getProperties());
                        var remUnknownHelper = new JIComOxidRuntimeHelper(SharpCifs.Util.Sharpen.Properties);
						lock (JIComOxidRuntime.mutex)
						{
							JISystem.internal_setSocket(socket);
							remUnknownHelper.Attach();
						}

                        //now start a new thread with this socket 
                        var remUnknown = new Thread(remUnknownForThisListener, new RunnableAnonymousInnerClassHelper3(this, remUnknownHelper), "jI_RemUnknown[" + baseIID + " , L(" + socket.LocalPort + "):R(" + socket.Port + ")]") {
                            Daemon = true
                        };
                        remUnknown.Start();
					}
				}
				catch (ClosedByInterruptException)
				{
					Log.Logger.Information("JIComOxidRuntimeHelper RemUnknownListener" + Thread.CurrentThread.Name + " is purposefully closed by interruption.");
				}
				catch (IOException e)
				{
                    Log.Logger.Warning(e, "JIComOxidRuntimeHelper RemUnknownListener");
                    Log.Logger.Warning("RemUnknownListener Thread: " + e.Message + " , on thread Id: " + Thread.CurrentThread.Name);
                }
                catch (Exception e)
				{
                    Log.Logger.Warning(e, "JIComOxidRuntimeHelper RemUnknownListener");
                }


                Log.Logger.Information("terminating RemUnknownListener thread: " + Thread.CurrentThread.Name);
            }

            private class RunnableAnonymousInnerClassHelper3 : Runnable
			{
				private readonly RunnableAnonymousInnerClassHelper2 outerInstance;

				private JIComOxidRuntimeHelper remUnknownHelper;

				public RunnableAnonymousInnerClassHelper3(RunnableAnonymousInnerClassHelper2 outerInstance, JIComOxidRuntimeHelper remUnknownHelper)
				{
					this.outerInstance = outerInstance;
					this.remUnknownHelper = remUnknownHelper;
				}

				public virtual void run()
				{
					try
					{
						((JIComRuntimeEndpoint)remUnknownHelper.Endpoint).processRequests(new RemUnknownObject(outerInstance.ipidOfRemUnknown,outerInstance.ipidOfComponent),outerInstance.baseIID,outerInstance.listOfSupportedInterfaces);
					}
					catch (SmbAuthException e)
					{
						Log.Logger.log(Level.WARNING,"JIComOxidRuntimeHelper RemUnknownThread (not listener)",e);
						throw new JIRuntimeException(JIErrorCodes.JI_CALLBACK_AUTH_FAILURE);
					}
					catch (SmbException e)
					{
						//System.out.println(e.getMessage());
						Log.Logger.log(Level.WARNING,"JIComOxidRuntimeHelper RemUnknownThread (not listener)",e);
						throw new JIRuntimeException(JIErrorCodes.JI_CALLBACK_SMB_FAILURE);
					}
					catch (ClosedByInterruptException)
					{
						Log.Logger.Information("JIComOxidRuntimeHelper RemUnknownThread (not listener)" + Thread.CurrentThread.Name + " is purposefully closed by interruption.");
					}
					catch (IOException e)
					{
						Log.Logger.log(Level.WARNING,"JIComOxidRuntimeHelper RemUnknownThread (not listener)",e);
					}
					finally
					{
						try
						{
							remUnknownHelper.Detach();
						}
						catch (IOException)
						{
						}
					}

				}
			}
		}
	}

	//This object should have serialized access only , i.e at a time only 1 read --> write , cycle should happen
	// it is not multithreaded safe.
	internal class OxidResolverImpl : NdrOp, IJICOMRuntimeWorker
	{
		//override read\write\opnum etc. here, use the util apis to decompose this.
		private int opnum = -1;
		private NdrBuffer buffer;
		private readonly SharpCifs.Util.Sharpen.Properties p;
		public OxidResolverImpl(SharpCifs.Util.Sharpen.Properties p) 		{
			this.p = p;
		}

		public virtual UUID CurrentObjectID {
            set {
                //does nothing.
            }
            get => null;
        }
        //	public void setCurrentJavaInstanceFromIID(String iid)
        //	{
        //		//does nothing.
        //	}

        public virtual int Opnum {
            set => opnum = value;
            get => opnum;
        }


        public virtual void write(NdrCodec ndr)
		{
			ndr.Buffer = buffer; //this buffer is prepared via read.
		}

		public virtual void read(NdrCodec ndr)
		{
			//will read according to the opnum. The setOpnum should have been called before this
			//call.	

			switch (opnum)
			{
				case 1:
					buffer = SimplePing(ndr);
					break;
				case 2:
					buffer = ComplexPing(ndr);
					break;
				case 3: //ServerAlive
					buffer = ServerAlive(ndr);
					break;
				case 5: //This is ServerAlive2
					buffer = ServerAlive2(ndr);
					break;
				case 4: //This is ResolveOxid2
					buffer = ResolveOxid2(ndr);
					break;
				default: //should not have arrived here.
                    Log.Logger.Warning("Oxid Object: DEFAULTED !!!");
                    throw new JIRuntimeException(JIErrorCodes.RPC_S_PROCNUM_OUT_OF_RANGE);
			}


		}

		private Random random = new Random();


		private NdrBuffer SimplePing(NdrCodec ndr)
		{
			Log.Logger.Information("Oxid Object: SimplePing");
			var b = JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr,8); //setid
			JIComOxidRuntime.addUpdateSets(new JISetId(b),new ArrayList(),new ArrayList());
			buffer = new NdrBuffer(new byte[16],0);
			buffer.enc_ndr_long(0);
			buffer.enc_ndr_long(0);
			buffer.enc_ndr_long(0);
			buffer.enc_ndr_long(0);
			return buffer;
		}

		private NdrBuffer ComplexPing(NdrCodec ndr)
		{
            Log.Logger.Information("Oxid Object: ComplexPing");
            var b = JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr,8); //setid
			JIMarshalUnMarshalHelper.Deserialize(ndr,typeof(short?),null,JIFlags.FLAG_NULL,null); //seqId.
			var lengthAdds = (short?)JIMarshalUnMarshalHelper.Deserialize(ndr,typeof(short?),null,JIFlags.FLAG_NULL,null);
			var lengthDels = (short?)JIMarshalUnMarshalHelper.Deserialize(ndr,typeof(short?),null,JIFlags.FLAG_NULL,null);
			JIMarshalUnMarshalHelper.Deserialize(ndr,typeof(int?),null,JIFlags.FLAG_NULL,null);

			JIMarshalUnMarshalHelper.Deserialize(ndr,typeof(int?),null,JIFlags.FLAG_NULL,null); //length
			var listOfAdds = new ArrayList();
			for (var i = 0; i < (int)lengthAdds; i++)
			{
				listOfAdds.Add(new JIObjectId(JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr,8),false));
			}

			JIMarshalUnMarshalHelper.Deserialize(ndr,typeof(int?),null,JIFlags.FLAG_NULL,null); //length
			var listOfDels = new ArrayList();
			for (var i = 0; i < (int)lengthDels; i++)
			{
				listOfDels.Add(new JIObjectId(JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr,8),false));
			}

			if (Arrays.Equals(b,new byte[]{0,0,0,0,0,0,0,0}))
			{
				random.NextBytes(b);
			}

			JIComOxidRuntime.addUpdateSets(new JISetId(b),listOfAdds,listOfDels);

			buffer = new NdrBuffer(new byte[32],0);
            var ndr2 = new NdrCodec {
                Buffer = buffer
            };

            JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr2,b);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(short?), (short)0, null,JIFlags.FLAG_NULL);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), 0, null,JIFlags.FLAG_NULL); //hresult
			return buffer;
		}

		private NdrBuffer ServerAlive(NdrCodec ndr)
		{
            Log.Logger.Information("Oxid Object: ServerAlive");
            var buffer = new byte[32]; //16 + 16=just in case
			var ndrBuffer = new NdrBuffer(buffer,0);
			ndrBuffer.enc_ndr_long(0);
			ndrBuffer.enc_ndr_long(0);
			ndrBuffer.enc_ndr_long(0);
			ndrBuffer.enc_ndr_long(0);
			return ndrBuffer;
		}
		private NdrBuffer ServerAlive2(NdrCodec ndr)
		{
            Log.Logger.Information("Oxid Object: ServerAlive2");
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

            var dualStringArray = new JIDualStringArray(-1);

			var buffer = new byte[dualStringArray.Length + 4 + 16 + 16]; //just in case - 2 unknown 8 bytes - COMVERSION
			var ndrBuffer = new NdrBuffer(buffer,0);


            var ndr2 = new NdrCodec {
                Buffer = ndrBuffer
            };

            //serialize COMVERSION
            //		JIMarshalUnMarshalHelper.serialize(ndr2,Short.class, new Short((short)JISystem.getCOMVersion().getMajorVersion()),null,JIFlags.FLAG_NULL);
            //		JIMarshalUnMarshalHelper.serialize(ndr2,Short.class, new Short((short)JISystem.getCOMVersion().getMinorVersion()),null,JIFlags.FLAG_NULL);

            //Vikram June 19th 2013: Forcing the JILocalCoClass's server to 5.4. This is so that we stay at 5.4 DCOM until we upgrade the 
            //local server to 5.7 as well.
            JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(short?), (short)5, null,JIFlags.FLAG_NULL);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(short?), (short)4, null,JIFlags.FLAG_NULL);

			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), 0, null,JIFlags.FLAG_NULL);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), dualStringArray.Length, null,JIFlags.FLAG_NULL);
			dualStringArray.Encode(ndr2);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), 0, null,JIFlags.FLAG_NULL);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), 0, null,JIFlags.FLAG_NULL);
			return ndrBuffer;
		}
		//will prepare a NdrBuffer for reply to this call 
		private NdrBuffer ResolveOxid2(NdrCodec ndr)
		{
            Log.Logger.Information("Oxid Object: ResolveOxid2");
            //System.err.println("VIKRAM: resolve oxid thread Id = " + Thread.currentThread().getId());
            //first read the OXID, then consult the oxid master about it's details.
            var oxid = new JIOxid(JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr,8));

			//now get the RequestedProtoSeq length.
			var length = (int)(short?)JIMarshalUnMarshalHelper.Deserialize(ndr,typeof(short?),null,JIFlags.FLAG_NULL,null);

			//now for the array.
			var array = (JIArray)JIMarshalUnMarshalHelper.Deserialize(ndr,new JIArray(typeof(short?),null,1,true),null,JIFlags.FLAG_REPRESENTATION_ARRAY,null);

			//now query the Resolver master for this data.
			var details = JIComOxidRuntime.getOxidDetails(oxid);

			if (details == null)
			{
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
			var uuid = details.RemUnknownIpid == null ? new UUID(java.util.UUID.randomUUID().ToString()) : new UUID(details.RemUnknownIpid);

			//create the bindings for this Java Object.
			//this port will go in the new bindings sent to the COM client.
			var port = -1;
			try
			{
				//this is so that repeated calls for Oxid resolution return the same rem unknwon.
				port = details.PortForRemUnknown;
				if (port == -1)
				{
					var remunknownipid = uuid.ToString();
					var portandthread = details.COMRuntimeHelper.startRemUnknown(details.IID,remunknownipid,details.Ipid, details.Referent.SupportedInterfaces);
					port = (int)(int?)portandthread[0];
					details.RemUnknownThreadGroup = (ThreadGroup)portandthread[1];
					details.RemUnknownIpid = remunknownipid;
				}
				details.PortForRemUnknown = port;
			}
			catch (IOException)
			{

				throw new JIRuntimeException(JIErrorCodes.E_UNEXPECTED);
			}

			//can support only TCP connections
			//JIDualStringArray.test = true;
			var dualStringArray = new JIDualStringArray(port);


			var authnHint = new int?(details.ProtectionLevel);


			var buffer = new byte[4 + 4 + dualStringArray.Length + 16 + 4 + 2 + 2 + 4 + 16];

			//have all data now prepare the response
			//the response expected here is defines the byte array size.
			var ndrBuffer = new NdrBuffer(buffer,0);

            var ndr2 = new NdrCodec {
                Buffer = ndrBuffer
            };

            JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), new object().GetHashCode(), null,JIFlags.FLAG_NULL);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), (dualStringArray.Length - 4) / 2, null,JIFlags.FLAG_NULL);
			dualStringArray.Encode(ndr2);

			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(UUID), uuid,null,JIFlags.FLAG_NULL);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), authnHint,null,JIFlags.FLAG_NULL);
	//		JIMarshalUnMarshalHelper.serialize(ndr2,Short.class, new Short((short)JISystem.getCOMVersion().getMajorVersion()),null,JIFlags.FLAG_NULL);
	//		JIMarshalUnMarshalHelper.serialize(ndr2,Short.class, new Short((short)JISystem.getCOMVersion().getMinorVersion()),null,JIFlags.FLAG_NULL);

			//Vikram June 19th 2013: Forcing the JILocalCoClass's server to 5.4. This is so that we stay at 5.4 DCOM until we upgrade the 
			//local server to 5.7 as well.
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(short?), (short)5, null,JIFlags.FLAG_NULL);
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(short?), (short)4, null,JIFlags.FLAG_NULL);

			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), 0, null,JIFlags.FLAG_NULL); //hresult


			return ndrBuffer;
		}

        public virtual IList QIedIIDs => null;


        public virtual bool Resolver => true;

        public virtual string CurrentIID
		{
			set
			{
				//does nothing
			}
		}

		public virtual bool workerOver()
		{
			//oxid resolver gets over when the client connected to it releases socket.
			return false;
		}
	}

	//This object should have serialized access only , i.e at a time only 1 read --> write , cycle should happen
	//it is not multithreaded safe.
	internal class RemUnknownObject : NdrOp, IJICOMRuntimeWorker
	{
		//override read\write\opnum etc. here, use the util apis to decompose this.
		private int opnum = -1;
		private NdrBuffer buffer;

		//component tells you the JILocalCoClass to act on , sent via the AlterContext calls
		//for all Altercontexts with IRemUnknown , this will be null.
		private JILocalCoClass component; //will hold the current instance to act on.
		/* the component and object id duo work together. 1 component could export many ipids.
		 * 
		 */
		//ObjectID tells you the IPID to act on, sent via the Request calls
		private UUID objectId;

		//this would be the ipid of this RemUnknownObject
		private readonly string selfIPID;

		private string currentIID;

		private IList listOfIIDsQIed = new ArrayList();

		internal RemUnknownObject(string ipidOfme, string ipidOfComponent)
		{
			selfIPID = ipidOfme;
			mapOfIpidsVsRef[ipidOfComponent.ToUpper()] = 5;
		}

        //this list will get cleared after this call.
        public virtual IList QIedIIDs => listOfIIDsQIed;

        public virtual bool Resolver => false;

        public virtual int Opnum {
            set => opnum = value;
            get => opnum;
        }


        public virtual void write(NdrCodec ndr)
		{
			ndr.Buffer = buffer; //this buffer is prepared via read.
		}

		private static readonly JIStruct remInterfaceRef = new JIStruct();
		static RemUnknownObject()
		{
			try
			{
				remInterfaceRef.AddMember(typeof(UUID));
				remInterfaceRef.AddMember(typeof(int?));
				remInterfaceRef.AddMember(typeof(int?));
			}
			catch (JIException shouldnothappen)
			{
				Log.Logger.Error(e, "RemUnknownObject", "Static Initialiser", shouldnothappen);
			}
		}
		private static readonly JIArray remInterfaceRefArray = new JIArray(remInterfaceRef,null,1,true);

		private IDictionary mapOfIpidsVsRef = new Hashtable();
		private bool workerOver_Renamed;

		public virtual void read(NdrCodec ndr)
		{
			//will read according to the opnum. The setOpnum should have been called before this
			//call.	
			var ipid = objectId.ToString();

	//		if (!mapOfIpidsVsRef.containsKey(ipid.toUpperCase()))
	//		{
	//		    System.out.println(Thread.currentThread() + " -->> " + ipid.toUpperCase());
	//		    //we always give 5 references
	//		    mapOfIpidsVsRef.put(ipid.toUpperCase(),new Integer(5));
	//		}

			//this means the call came for IRemUnknown apis, since selfIpid is null or matches the objectID
			//if (selfIPID == null || selfIPID.equalsIgnoreCase(ipid))
	//		if ("00000131-0000-0000-C000-000000000046".equalsIgnoreCase(currentIID))
			if (selfIPID.Equals(ipid, StringComparison.CurrentCultureIgnoreCase))
			{
				switch (opnum)
				{
					case 3: //IRemUnknown QI.
						buffer = QueryInterface(ndr);
						break;
					case 4: //addref
							JIOrpcThis.decode(ndr);
							var length = ndr.ReadUnsignedShort();

							var retvals = new int[length];
							var array = (JIArray)JIMarshalUnMarshalHelper.deSerialize(ndr, remInterfaceRefArray, new ArrayList(), JIFlags.FLAG_REPRESENTATION_ARRAY, new Hashtable());
							//saving the ipids with there references. considering public + private references together for now.
							var structs = (JIStruct[])array.ArrayInstance;
							for (var i = 0;i < length;i++)
							{
								var ipidref = ((UUID)structs[i].GetMember(0)).ToString().ToUpper();
								var publicRefs = (int)(int?)structs[i].GetMember(1);
								var privateRefs = (int)(int?)structs[i].GetMember(2);

                            if (!mapOfIpidsVsRef.Contains(ipidref)) {
                                //this would be strange, since all the ipids we give should be part of the map already.
                                //have to set 0x80000003 (INVALID ARG here)
                                retvals[i] = unchecked((int)0x80000003);
                                continue;
                            }
                            // StoredIQ - Satwik - native C++ says 01 here 
                            retvals[i] = 0x1;


                            var total = (int)(int?)mapOfIpidsVsRef[ipidref] + publicRefs + privateRefs;
								mapOfIpidsVsRef[ipidref] = total;
							}


							//preparing the response
							buffer = new NdrBuffer(new byte[length * 4 + 16],0);
                        var ndr2 = new NdrCodec {
                            Buffer = buffer
                        };
                        JIOrpcThat.encode(ndr2);
							for (var i = 0;i < length;i++)
							{
								buffer.enc_ndr_long(retvals[i]);
							}

							buffer.enc_ndr_long(0);
							buffer.enc_ndr_long(0);

						break;
					case 5: //release


						JIOrpcThis.decode(ndr);
						length = ndr.ReadUnsignedShort();
						array = (JIArray)JIMarshalUnMarshalHelper.deSerialize(ndr, remInterfaceRefArray, new ArrayList(), JIFlags.FLAG_REPRESENTATION_ARRAY, new Hashtable());
						//saving the ipids with there references. considering public + private references together for now.
						structs = (JIStruct[])array.ArrayInstance;
						for (var i = 0;i < length;i++)
						{
							var ipidref = ((UUID)structs[i].GetMember(0)).ToString().ToUpper();
							var publicRefs = (int)(int?)structs[i].GetMember(1);
							var privateRefs = (int)(int?)structs[i].GetMember(2);
							if (!mapOfIpidsVsRef.Contains(ipidref))
							{
								continue;
							}

							var total = (int)(int?)mapOfIpidsVsRef[ipidref] - publicRefs - privateRefs;
							if (total == 0)
							{
								mapOfIpidsVsRef.Remove(ipidref);
							}
							else
							{
								mapOfIpidsVsRef[ipidref] = total;
							}
						}

						//all references to all IPIDs exported are over, this is now done.
						if (mapOfIpidsVsRef.Count == 0)
						{
							workerOver_Renamed = true;
						}

						//I have 1 OID == 1 IPID == 1 java instance.
						buffer = new NdrBuffer(new sbyte[32],0);
                        ndr2 = new NdrCodec {
                            Buffer = buffer
                        };
                        JIOrpcThat.encode(ndr2);
						buffer.enc_ndr_long(0);
						buffer.enc_ndr_long(0);
						break;
					default:
						throw new JIRuntimeException(JIErrorCodes.RPC_S_PROCNUM_OUT_OF_RANGE);
				}
			}
			else
			{
				//now use the objectId , just set in before this call to read. That objectId is the IPID on which the
				//call is being made , and was previously exported during Q.I. The component value was filled during an
				//alter context or bind, again made some calls before.
				if (component == null)
				{
					Log.Logger.severe("JIComOxidRuntimeHelper RemUnknownObject read(): component is null , opnum is " + opnum + " , IPID is " + ipid + " , selfIpid is " + selfIPID);
				}
				sbyte[] b = null;
				object result = null;
				var ndr2 = new NdrCodec();
				var hresult = 0;
				object[] retArray = null;
				try
				{
					result = component.InvokeMethod(ipid,opnum,ndr);
				}
				catch (JIException e)
				{
					hresult = e.ErrorCode;
					Log.Logger.severe("Exception occured: " + e.ErrorCode);
					Log.Logger.Error(e, "RemUnknownObject","read",e);
				}


				//now if opnum was 6 then this is a dispatch call , so response has to be dispatch response
				//not the normal one.
				if (component.GetInterfaceDefinitionFromIPID(ipid).DispInterface && opnum == 6)
				{
					var result2 = result;
					//orpcthat
					//[out] VARIANT * pVarResult,
					//[out] EXCEPINFO * pExcepInfo,
					//[out] UINT * pArgErr,
					//[in, out, size_is(cVarRef)] VARIANTARG * rgVarRef
					result = new object[4]; //orpcthat gets filled outside
					var excepInfo = new JIStruct();
					try
					{
						excepInfo.AddMember((short)0);
						excepInfo.AddMember((short)0);
						excepInfo.AddMember(new JIString(""));
						excepInfo.AddMember(new JIString(""));
						excepInfo.AddMember(new JIString(""));
						excepInfo.AddMember(0);
						excepInfo.AddMember(new JIPointer(null,true));
						excepInfo.AddMember(new JIPointer(null,true));
						excepInfo.AddMember(0);
					}
					catch (JIException e)
					{ //not expecting any here
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}

					if (result2 == null)
					{
						((object[])result)[0] = JIVariant.CreateEMPTY();
					}
					else
					{
						//now check whether the variant is by ref or not.
						var variant = (JIVariant)((object[])result2)[0];

						try
						{
							if (variant.IsByRef)
							{
								//add empty inplace of this.
								((object[])result)[0] = JIVariant.CreateEMPTY();
								//now update the array at the end.
								((object[])result)[3] = new JIArray(new JIVariant[]{variant},true);

							}
							else
							{
								((object[])result)[0] = ((object[])result2)[0]; //will have only a single index.
								((object[])result)[3] = 0; //Array
							}
						}
						catch (JIException e)
						{
							throw new JIRuntimeException(e.ErrorCode);
						}
					}

					((object[])result)[1] = excepInfo;

					((object[])result)[2] = 0; //argErr is null, for now.


					retArray = (object[]) result;

				}


				buffer = new NdrBuffer(b,0);
				ndr2.Buffer = buffer;

				//JIOrpcThat.encode(ndr2);
				//have to create a call Object, since these return types could be structs , unions etc. having deffered pointers 
				var callObject = new JICallBuilder();
				callObject.AttachSession(component.Session);
				if (result != null)
				{

					if (retArray != null)
					{
						//serialize all members sequentially.
						for (var i = 0;i < retArray.Length;i++)
						{
							callObject.addInParamAsObject(retArray[i],JIFlags.FLAG_NULL);
						}
					}
					else
					{
						//serialize all members sequentially.
						for (var i = 0;i < ((object[])result).Length;i++)
						{
							callObject.addInParamAsObject(((object[])result)[i],JIFlags.FLAG_NULL);
						}

					}



				}
				callObject.write2(ndr2);
				JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), hresult, null,JIFlags.FLAG_NULL);




			}


		}


		private NdrBuffer QueryInterface(NdrCodec ndr)
		{
            //now to decompose all
            Log.Logger.Verbose("Within RemUnknownObject: QueryInterface");
            Log.Logger.Verbose("RemUnknownObject: [QI] Before call terminated listOfIIDsQIed are: " + listOfIIDsQIed);
            JIOrpcThis.decode(ndr);

			//now get the IPID and export the component with a new IPID and IID. 
			var ipid = new UUID();
			try
			{
				ipid.Decode(ndr,ndr.Buffer);
			}
			catch (NdrException e)
			{
				Log.Logger.Error(e, "JIComOxidRuntimeHelper","QueryInterface",e);
			}

			Log.Logger.Verbose("RemUnknownObject: [QI] IPID is " + ipid);
			//set the JILocalCoClass., the ipid should not be null in this call.
			var details = JIComOxidRuntime.getComponentFromIPID(ipid.ToString());

			if (details == null)
			{
				//not found, now throw an JIRuntimeException , so that a FaultPdu could be sent.
				throw new JIRuntimeException(JIErrorCodes.RPC_E_INVALID_OXID);
			}

			var component = details.Referent;

            Log.Logger.Verbose("RemUnknownObject: [QI] JIJavcCoClass is " + component.CoClassIID);

            (int)(int?)JIMarshalUnMarshalHelper.Deserialize(ndr,typeof(int?),null, JIFlags.FLAG_NULL,null); //refs , don't really care about this.

			var length = (int)(short?)JIMarshalUnMarshalHelper.Deserialize(ndr,typeof(short?),null, JIFlags.FLAG_NULL,null); //length of the requested Interfaces

			var array = (JIArray)JIMarshalUnMarshalHelper.Deserialize(ndr,new JIArray(typeof(UUID),null,1,true),null,JIFlags.FLAG_REPRESENTATION_ARRAY,null);

			//now to build the buffer and export the IIDs with new IPIDs
			var b = new sbyte[8 + 4 + 4 + length * (4 + 4 + 40) + 16];
			var buffer = new NdrBuffer(b,0);

            //start with response
            var ndr2 = new NdrCodec {
                Buffer = buffer
            };

            JIOrpcThat.encode(ndr2);

			//pointer
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), new object().GetHashCode(), null,JIFlags.FLAG_NULL);
			//length of array
			JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), length, null,JIFlags.FLAG_NULL);

			var arrayOfUUIDs = (object[])array.ArrayInstance;

			for (var i = 0; i < arrayOfUUIDs.Length; i++)
			{
				var iid = (UUID)arrayOfUUIDs[i];
                Log.Logger.Verbose("RemUnknownObject: [QI] Array iid[" + i + "] is " + iid);
                //now for each QueryResult
                try {
					var hresult = 0;
	//				String ipid2 = GUIDUtil.guidStringFromHexString(IdentifierFactory.createUniqueIdentifier().toHexString());
					string ipid2 = java.util.UUID.randomUUID().ToString();
					if (!component.IsPresent(iid.ToString()))
					{
						hresult = JIErrorCodes.E_NOINTERFACE;
	//					ipid2 = GUIDUtil.guidStringFromHexString("00000000000000000000000000000000");
						ipid2 = java.util.UUID.fromString("00000000000000000000000000000000").ToString();
					}
					else
					{
						string tmpIpid = null;
						try
						{
							tmpIpid = component.GetIpidFromIID(iid.ToString());
						}
						catch (Exception e)
						{
							Log.Logger.Error(e, "JIComOxidRuntimeHelper", "QueryInterface", e);
						}

						if (tmpIpid == null)
						{
                            Log.Logger.Verbose("RemUnknownObject: [QI] tmpIpid is null for iid " + iid);
                            component.ExportInstance(iid.ToString(), ipid2);
						}
						else
						{
                            Log.Logger.Verbose("RemUnknownObject: [QI] tmpIpid is NOT null for iid " + iid + " and ipid sent back is " + ipid2);
                            ipid2 = tmpIpid;
						}
					}
					//hresult
					JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), hresult, null,JIFlags.FLAG_NULL);
					JIMarshalUnMarshalHelper.Serialize(ndr2,typeof(int?), unchecked((int)0xCCCCCCCC), null,JIFlags.FLAG_NULL);

					//now generate the IPID and export a java instance with this.
					JIStdObjRef objRef = null;
					if (hresult == 0)
					{
						objRef = new JIStdObjRef(ipid2,details.Oxid,details.Oid);
					}
					else
					{
						objRef = new JIStdObjRef(ipid2);
					}
					objRef.Encode(ndr2);

					//add it to the exported Ipids map
					if (hresult == 0)
					{
						mapOfIpidsVsRef[ipid2.ToUpper()] = objRef.PublicRefs;
					}

                    Log.Logger.Verbose("RemUnknownObject: [QI] for which the stdObjRef is " + objRef);

                }
                catch (IllegalAccessException e)
				{
					Log.Logger.Error(e, "JIComOxidRuntimeHelper","QueryInterface",e);
				}
				catch (InstantiationException e)
				{
					Log.Logger.Error(e, "JIComOxidRuntimeHelper","QueryInterface",e);
				}

				var iidtemp = iid.ToString().ToUpper() + ":0.0";
				if (!listOfIIDsQIed.Contains(iidtemp))
				{
					listOfIIDsQIed.Add(iidtemp);
				}
			}

			Log.Logger.Verbose("RemUnknownObject: [QI] After call terminated listOfIIDsQIed are: " + listOfIIDsQIed);

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
                objectId = value;
                component = JIComOxidRuntime.getJavaComponentFromIPID(value.ToString());
            }
            get => objectId;
        }


        public virtual string CurrentIID {
            set => currentIID = value;
        }

        public virtual bool workerOver()
		{
			return workerOver_Renamed;
		}
	}
}