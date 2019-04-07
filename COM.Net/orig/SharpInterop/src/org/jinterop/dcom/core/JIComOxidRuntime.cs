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


	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JIException = org.jinterop.dcom.common.JIException;
	using JISystem = org.jinterop.dcom.common.JISystem;

	using Security = rpc.Security;

	//import com.iwombat.foundation.IdentifierFactory;
	//import com.iwombat.util.GUIDUtil;


	/// <summary>
	/// Thread for Oxid Resolver. Creates and accepts socket
	/// connections for resolving oxids. Gets started once for each instance
	/// of the library.
	/// 
	/// Please note that the <b>"Server" <b> Service should be running on the machine where the
	/// <br> COM server is running. 
	/// 
	/// @since 1.0 
	/// 
	/// </summary>
	internal sealed class JIComOxidRuntime {

		private static Properties Defaults = new Properties();
		private static Properties Defaults2 = new Properties();
		private static bool StopSystem = false;
		private static bool ResolverStarted = false;
	//	private static ArrayList listOfSockets = new ArrayList();
		private static int OxidResolverPort_Renamed = -1;

		private static Hashtable MapOfIPIDVsComponent = new Hashtable(); //java client , com server
		private static Hashtable MapOfJavaVsOxidDetails = new Hashtable(); //java client , com server
		private static Hashtable MapOfOxidVsOxidDetails = new Hashtable(); //java client , com server
		private static Hashtable MapOfOIDVsComponents = new Hashtable(); //java client , com server

		//list of all exported oids per session, all these oids have to be removed.
		private static Hashtable MapOfSessionIdsVsOIDs = new Hashtable(); //java server , com client

		private static Hashtable MapOfSetIdVsListOfOIDs = new Hashtable(); //com client , java server
		private static Hashtable MapOfSessionVsPingSetHolder = new Hashtable(); //com client , java server
		//private static HashMap mapOfIPIDVsOID = new HashMap(); //com client , java server, //IPID vs JIObjectId, for increasing\decreasing references 
		private static Hashtable MapOfAddressVsStub = new Hashtable(); //java client , com server, so that we don't have to keep doing bind everytime.


		private static IList ListOfExportedJavaComponents = new List<object>();

		internal static readonly object Mutex = new object(); //for access to the sockets
		private static readonly object Mutex2 = new object(); //for access to the maps
		private static readonly object Mutex3 = new object(); //for access to the AddressVsSession,Stub Map

		private static readonly object Mutex4 = new object(); //for access to the mapOfAddressVsStub

		private static ServerSocket ServerSocket = null;
		private static Random RandomGen = new Random(double.doubleToRawLongBits(new Random(1).NextDouble()));
		private static Timer PingTimer_2minutes = new Timer(true);
		private static Timer PingTimer_8minutes = new Timer(true);


		//one per session.
		private class PingSetHolder {
			internal sbyte[] SetId = null;
			internal string Username = null;
			internal string Password = null;
			internal string Domain = null;
			internal bool Modified = false;
			internal bool Closed = false;
			internal bool UseNTLMv2 = false;
			internal bool IsSSO = false;
			internal int SeqNum = 1;
			//JISession session  = null;
			internal IDictionary CurrentSetOIDs = new Hashtable(); //list of JIObjectId, this list is iterated and if the IPID ref count is 0 ,
													//it is added as a delete in set and a complex ping is sent.
			internal IDictionary PingedOnce = new Hashtable();
			public override string ToString() {
				return "SetID[" + SetId + "] , currentSetOIDs[" + CurrentSetOIDs + "]";
			}
		}

		//this task just checks for expired OIDs in the mapOfOIDVsComponents, each OID carries with itself, lastPingedTime, 
		//if that (currenttime - thattime) is < ping interval...all is okay, otherwise , all it's details are erased, thus 
		//removing any reference of the given java server from j-Interop library, after which if no one outside has references, this
		//object can be GCed.
		private class ServerPingTimerTask : TimerTask {
			public virtual void Run() {

				lock (Mutex2) {

					if (JISystem.Logger.isLoggable(Level.INFO)) {
						JISystem.Logger.info("Running ServerPingTimerTask !");
					}

					IEnumerator itr = MapOfOIDVsComponents.Keys.GetEnumerator();

					while (itr.hasNext()) {
						JIObjectId oid = (JIObjectId)itr.next();
						if (oid.HasExpired()) {
							//remove all
							JILocalCoClass component = (JILocalCoClass)MapOfOIDVsComponents.GetValueOrNull(oid);
							//this means the local system still has references and we cannot delete this object
							//since the user may reuse it.
							if (component.AssociatedReferenceAlive) {
								continue;
							}
							JIComOxidDetails details = (JIComOxidDetails)MapOfJavaVsOxidDetails.GetValueOrNull(component);
							MapOfOxidVsOxidDetails.Remove(details.Oxid);
							MapOfIPIDVsComponent.Remove(details.Ipid);
							MapOfJavaVsOxidDetails.Remove(component);
							ListOfExportedJavaComponents.Remove(component);
							itr.remove();

							//the thread associated with this will also stop.
							details.InterruptRemUnknownThreadGroup();

							component = null;
							details = null;
						}
					}

				}

			}
		}


		// Helper method to force release of a local component, so we dont
		// wait until the session is destroyed.
		internal static void ReleaseLocalComponent(JISession session, JILocalCoClass component) {
			lock (Mutex2) {
				if (JISystem.Logger.isLoggable(Level.INFO)) {
						JISystem.Logger.info("releaseLocalComponent: " + component.CoClassIID);
				}

				JIComOxidDetails details = (JIComOxidDetails)MapOfJavaVsOxidDetails.GetValueOrNull(component);
				MapOfOIDVsComponents.Remove(details.Oid);
				MapOfOxidVsOxidDetails.Remove(details.Oxid);
				MapOfIPIDVsComponent.Remove(details.Ipid);
				MapOfJavaVsOxidDetails.Remove(component);
				ListOfExportedJavaComponents.Remove(component);
				MapOfSessionIdsVsOIDs.Remove(new int?(session.SessionIdentifier));

				//the thread associated with this will also stop.
				details.InterruptRemUnknownThreadGroup();

				component = null;
				details = null;
			}
		}


		internal static void DestroySessionOIDs(int sessionId) {
			lock (Mutex2) {

				if (JISystem.Logger.isLoggable(Level.INFO)) {
					JISystem.Logger.info("destroySessionOIDs for session: " + sessionId);
				}

				IList oids = (List<object>)MapOfSessionIdsVsOIDs.Remove(new int?(sessionId));
				if (oids == null || oids.Count == 0) {
					return;
				}

				for (int i = 0 ; i < oids.Count; i++) {
					JIObjectId oid = (JIObjectId)oids[i];
					//remove all
					JILocalCoClass component = (JILocalCoClass)MapOfOIDVsComponents.Remove(oid);
					JIComOxidDetails details = (JIComOxidDetails)MapOfJavaVsOxidDetails.GetValueOrNull(component);
					if (details != null) {
						MapOfOxidVsOxidDetails.Remove(details.Oxid);
						MapOfIPIDVsComponent.Remove(details.Ipid);
					}
					MapOfJavaVsOxidDetails.Remove(component);
					ListOfExportedJavaComponents.Remove(component);
					//the thread associated with this will also stop.
					if (details != null) {
						details.InterruptRemUnknownThreadGroup();
					}
					component = null;
					details = null;
					oid = null;
				}

				oids.Clear();
			}
		}

		private class ClientPingTimerTask : TimerTask {
			public virtual void Run() {

				IEnumerator itr = null;
				lock (Mutex3) {
					itr = ((IDictionary)MapOfSessionVsPingSetHolder.clone()).SetOfKeyValuePairs().GetEnumerator();
				}


				if (JISystem.Logger.isLoggable(Level.INFO)) {
					JISystem.Logger.info("Running ClientPingTimerTask !");
				}
				//iterate over the map and get the corresponding stubs and use there sessions to 
				//stub is created here and used per address

				//if set id is null send a complex ping to get back the set id for all the OIDs in the
				//PingSetHolder

				while (itr.hasNext()) {
					DictionaryEntry entry = (DictionaryEntry)itr.next();
					PingSetHolder holder = (PingSetHolder)(entry).Value;
					string address = ((JISession)entry.Key).TargetServer;
					//will get it from the cache, since it is getting called after every 4 minutes
					//what if this stub has timed out, I guess I will have to ask the developers to increase the timeout for now.
					JIComOxidStub stub = null;
					lock (Mutex4) {
						stub = (JIComOxidStub)MapOfAddressVsStub.GetValueOrNull(address);
						if (stub == null) {
							stub = new JIComOxidStub(address,holder.Domain,holder.Username,holder.Password,holder.UseNTLMv2, holder.IsSSO);
							MapOfAddressVsStub[address] = stub;
						}
					}

					List<object> listOfAddedOIDs = new List<object>();
					List<object> listOfRemovedOIDs = new List<object>();
					//form a list if OID is 0 ref
					lock (Mutex3) {
						for (IEnumerator itr2 = holder.CurrentSetOIDs.Keys.GetEnumerator();itr2.hasNext();) {
							JIObjectId oid = (JIObjectId)itr2.next();
							if (oid.IPIDRefCount == 0) {
								if (!oid.Dontping) {
									listOfRemovedOIDs.Add(oid);
									holder.PingedOnce.Remove(oid);
									holder.Modified = true;
								}
								itr2.remove();
							}
							else {
								if (!oid.Dontping && !holder.PingedOnce.Contains(oid)) {
									listOfAddedOIDs.Add(oid);
									holder.PingedOnce[oid] = oid;
									holder.Modified = true;
								}
							}
						}
					}
					if (JISystem.Logger.isLoggable(Level.INFO)) {
						JISystem.Logger.info("Within ClientPingTimerTask: holder.currentSetOIDs, current size of which is " + holder.CurrentSetOIDs.Count);
					}

					//this is the first time this is going and objects with no references will not be added to ping set.
					if (holder.SetId == null) {
						listOfRemovedOIDs.Clear();
					}

					bool isSimplePing = false;

					//No additions and no deletions
					if (holder.SetId != null && !holder.Modified) {
						//send simple set ping
						isSimplePing = true;
					}

					//seqNum will be 0 for simple ping, but incremented for complex pings. seqNum is per setId. first one will be 0 and increments
					//there on...
					holder.SetId = stub.Call(isSimplePing,holder.SetId,listOfAddedOIDs,listOfRemovedOIDs, isSimplePing ? 0 : holder.SeqNum++);

					if (JISystem.Logger.isLoggable(Level.FINEST)) {
						JISystem.Logger.info("Within ClientPingTimerTask: holder.seqNum " + holder.SeqNum);
					}

					holder.Modified = false;
					//stub.close(); commenting this since we are caching the stub.
					if (holder.Closed) {
						//this means that this set is empty and there is no need for it. The set has emptied  itself and
						//will get removed from COM servers side as well.
						if (JISystem.Logger.isLoggable(Level.INFO)) {
							JISystem.Logger.info("Within ClientPingTimerTask: Holder " + holder + " is empty, will remove this from mapOfSessionVsPingSetHolder");
						}
						itr.remove();
						lock (Mutex3) {
							MapOfSessionVsPingSetHolder.Remove(entry.Key);
						}
					}
				}
			}
		}

		static JIComOxidRuntime() {
			Defaults2.put("rpc.ntlm.lanManagerKey","false");
			Defaults2.put("rpc.ntlm.sign","false");
			Defaults2.put("rpc.ntlm.seal","false");
			Defaults2.put("rpc.ntlm.keyExchange","false");
			Defaults2.put("rpc.connectionContext","org.jinterop.dcom.transport.JIComRuntimeNTLMConnectionContext");
			Defaults.put("rpc.connectionContext","org.jinterop.dcom.transport.JIComRuntimeConnectionContext");
		}

		//ip address
		internal static void AddUpdateOXIDs(JISession session, string IPID, JIObjectId oid) {
			lock (Mutex3) {
				//make sure this is the IP address
				PingSetHolder holder = (PingSetHolder)MapOfSessionVsPingSetHolder.GetValueOrNull(session);
				if (holder == null) {
					//new 
					holder = new PingSetHolder();
					holder.Username = session.UserName;
					holder.Password = session.Password;
					holder.Domain = session.Domain;
					holder.CurrentSetOIDs[oid] = oid;
					holder.Modified = true;
					holder.SeqNum = 0;
					holder.UseNTLMv2 = session.NTLMv2Enabled;
					holder.IsSSO = session.SSOEnabled;
					MapOfSessionVsPingSetHolder[session] = holder;
				}
				else { //found , means it is another call for a new IPID
					JIObjectId oid2 = (JIObjectId)holder.CurrentSetOIDs.GetValueOrNull(oid);
					if (oid2 != null) {
						//have to update this oid, since the one from parameters is a "new" one.
						oid = oid2;
					}
					else {
						if (JISystem.Logger.isLoggable(Level.INFO)) {
							JISystem.Logger.info("addUpdateOXIDs: Adding OID to holder " + holder + ", current size of currentSetOIDs is " + holder.CurrentSetOIDs.Count);
						}
						holder.CurrentSetOIDs[oid] = oid;
						holder.Modified = true;
					}
				}

				oid.IncrementIPIDRefCountBy1();
				if (JISystem.Logger.isLoggable(Level.INFO)) {
					JISystem.Logger.info("addUpdateOXIDs: finally this oid is " + oid);
				}
			}

		}

		internal static void DelIPIDReference(string IPID, JIObjectId oid, JISession session) {
			lock (Mutex3) {
				PingSetHolder holder = (PingSetHolder)MapOfSessionVsPingSetHolder.GetValueOrNull(session);
				//this will be non-null, since we are trying to remove an IPID reference so the PingSet for its OID should exist
				if (holder != null) {
					JIObjectId oid2 = (JIObjectId)holder.CurrentSetOIDs.GetValueOrNull(oid);
					if (oid2 != null) {
						//temp gets replaced by the real one.
						oid = oid2;
					}
					else {
						if (JISystem.Logger.isLoggable(Level.WARNING)) {
							JISystem.Logger.warning("In delIPIDReference: Could not find Original OID for this temp OID for session: " + session.SessionIdentifier + " , temp oid is " + oid + " , and IPID is " + IPID);
						}
						return;
					}

					//this is the same OID as in the PingSetHolder.
					oid.DecrementIPIDRefCountBy1();
					if (JISystem.Logger.isLoggable(Level.INFO)) {
						JISystem.Logger.info("delIPIDReference: Decrementing reference count for IPID " + IPID + " on OID " + oid);
					}

					//should we retain this now ??? , we need not send a ping for this as well. It is being retained for the last ping only. 
					if (oid.IPIDRefCount <= 0) {
						holder.CurrentSetOIDs.Remove(oid);
						//everything is gone, remove the session
						if (holder.CurrentSetOIDs.Count == 0) {
							holder.Closed = true;
							MapOfSessionVsPingSetHolder.Remove(session);
						}
						if (JISystem.Logger.isLoggable(Level.INFO)) {
							JISystem.Logger.info("delIPIDReference: sessionid " + session.SessionIdentifier + "Ref count is <= 0, for OID " + oid + ", holder status: " + holder.Closed);
						}
					}
				}
				else {
					if (JISystem.Logger.isLoggable(Level.WARNING)) {
						JISystem.Logger.warning("In delIPIDReference: Could not find PingSetHolder for this session: " + session.SessionIdentifier + " , temp oid is " + oid + " , and IPID is " + IPID);
					}
				}
			}
		}

		internal static void ClearIPIDsforSession(JISession session) {
			lock (Mutex3) {
				//make sure this is the IP address
				PingSetHolder holder = (PingSetHolder)MapOfSessionVsPingSetHolder.GetValueOrNull(session);
				if (holder != null) {
					if (JISystem.Logger.isLoggable(Level.INFO)) {
						JISystem.Logger.info("clearIPIDsforSession: holder.currentSetOIDs's size is " + holder.CurrentSetOIDs.Count);
					}

					//No need to do this we are clearing the map anyways.
	//				for (Iterator itr2 = holder.currentSetOIDs.keySet().iterator();itr2.hasNext();)
	//				{
	//					JIObjectId oid = (JIObjectId)itr2.next();
	//					oid.setIPIDRefCountTo0();
	//				}

					holder.Modified = true;
					holder.CurrentSetOIDs.Clear(); //being done since this session is being destroyed and the corresponding COM server
												   //need not be retained by us.
					holder.Closed = true;

					//Should be not remove this entry ??? I think it is being retained only for the pings ... we should let this go.
					MapOfSessionVsPingSetHolder.Remove(session);
				}
			}

			//remove the socket for this session associated with ping timer
			lock (Mutex4) {
				JIComOxidStub stub = (JIComOxidStub)MapOfAddressVsStub.Remove(session.TargetServer);
				if (stub != null) {
					stub.Close();
				}
			}

		}

		internal static void StartResolverTimer() {
			lock (typeof(JIComOxidRuntime)) {
				//schedule only 1 timer task , the task to ping the OIDs obtained.
				PingTimer_2minutes.scheduleAtFixedRate(new ClientPingTimerTask(),0,(int)(4 * 60 * 1000));
				if (JISystem.JavaCoClassAutoCollectionSet) {
					PingTimer_8minutes.scheduleAtFixedRate(new ServerPingTimerTask(),0,8 * 60 * 1000);
				}
			}
		}

		//only one thread , that is the main is expected to enter this one.
		internal static void StartResolver() {
			lock (typeof(JIComOxidRuntime)) {
				if (ResolverStarted) {
					return;
				}
        
				Runnable thread = new RunnableAnonymousInnerClassHelper();
        
				Thread thread2 = new Thread(thread,"jI_OxidResolver");
				thread2.Daemon = true;
				thread2.Start();
				ResolverStarted = true;
			}
		}

		private class RunnableAnonymousInnerClassHelper : Runnable {
			public RunnableAnonymousInnerClassHelper() {
			}

			public virtual void Run() {

				try {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.nio.channels.ServerSocketChannel serverSocketChannel = java.nio.channels.ServerSocketChannel.open();
					ServerSocketChannel serverSocketChannel = ServerSocketChannel.open();
					ServerSocket = serverSocketChannel.socket(); //new ServerSocket(0); //bind on any free port
					ServerSocket.bind(null);
					OxidResolverPort_Renamed = ServerSocket.LocalPort;
					//System.err.println("VIKRAM: oxidResolverPort: " + oxidResolverPort);
					// server infinite loop
					while (!StopSystem) {
						Socket socket = ServerSocket.accept();
						//listOfSockets.add(socket);
						//System.err.println("VIKRAM: Accepting new Call from " + socket.getPort());
						//in a multithreaded scenario this will be serialized.
						lock (Mutex) {
							JISystem.Internal_setSocket(socket);
							//now create the JIComOxidRuntimeHelper Object and start it.
							Properties properties = new Properties(Defaults);
							properties.put("IID","99fcfec4-5260-101b-bbcb-00aa0021347a:0.0".ToUpper()); //IOxidResolver
							JIComOxidRuntimeHelper oxidResolver = new JIComOxidRuntimeHelper(properties);
							oxidResolver.StartOxid(socket.LocalPort, socket.Port);
						}

					}
				}
				 catch (IOException) {
					//e.printStackTrace();
				 }

				 //close all sockets.
			//			    for (int i = 0; i < listOfSockets.size(); i++)
			//			    {
			//			    	Socket s = (Socket)listOfSockets.get(i);
			//			    	try {
			//						s.close();
			//					} catch (IOException e) {}
			//			    }
			}
		}

		internal static int OxidResolverPort {
			get {
				return OxidResolverPort_Renamed;
			}
		}

		//Will be called from shutDownHook thread.
		internal static void StopResolver() {
			lock (typeof(JIComOxidRuntime)) {
				StopSystem = true;
				try {
					ServerSocket.close();
				}
				catch (IOException) {
				}
        
				PingTimer_2minutes.cancel();
				PingTimer_8minutes.cancel();
        
				IEnumerator itr = MapOfAddressVsStub.Values.GetEnumerator();
				while (itr.hasNext()) {
					JIComOxidStub s = (JIComOxidStub)itr.next();
					s.Close();
				}
				MapOfAddressVsStub.Clear(); //will clean up all the others as well
			}
		}

		/// <summary>
		/// Returns the MIP for the Java Instance, this will also have the OXID,OID,IPID
		/// for the same.
		/// </summary>
		/// <param name="javaInstance">
		/// @return </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static JIInterfacePointer getInterfacePointer(JISession session,JILocalCoClass component) throws org.jinterop.dcom.common.JIException
		internal static JIInterfacePointer GetInterfacePointer(JISession session, JILocalCoClass component) {
			JIInterfacePointer ptr = null;

			lock (Mutex2) {
				if (component.AlreadyExported) {
					throw new JIException(JIErrorCodes.JI_JAVACOCLASS_ALREADY_EXPORTED);
				}

				component.Session = session;
	//
	//			JIComOxidDetails details = 	(JIComOxidDetails)mapOfJavaVsOxidDetails.get(component);
	//			
	//			if (details != null)
	//			{
	//				return details.getInterfacePtr();
	//			}

				//as the ID could be repeated, this is the ipid of the interface being requested.
	//			String ipid = GUIDUtil.guidStringFromHexString(IdentifierFactory.createUniqueIdentifier().toHexString()); 
				string ipid = UUID.randomUUID().ToString();
				string iid = component.CoClassUnderRealIID ? component.CoClassIID : IJIComObject_Fields.IID; //has to be IUnknown's IID.
				sbyte[] bytes = new sbyte[8];
				RandomGen.NextBytes(bytes);
				JIOxid oxid = new JIOxid(bytes);
				sbyte[] bytes2 = new sbyte[8];
				RandomGen.NextBytes(bytes2);

				JIObjectId oid = new JIObjectId(bytes2,false);

				component.ObjectId = oid.OID;

				//JIComOxidDetails details = new JIComOxidDetails();
				JIStdObjRef objref = new JIStdObjRef(ipid,oxid,oid);
				ptr = new JIInterfacePointer(iid,OxidResolverPort_Renamed,objref);

				Properties properties = new Properties(Defaults2);
				properties.put("IID","00000131-0000-0000-C000-000000000046:0.0".ToUpper()); //IRemUnknown

				properties.put("rpc.ntlm.domain",session.TargetServer);

				int protecttionLevel = 2;

				if (session.SessionSecurityEnabled) {
					protecttionLevel = 6;
					properties.setProperty("rpc.ntlm.seal", "true");
					properties.setProperty("rpc.ntlm.sign", "true");
					properties.setProperty("rpc.ntlm.keyExchange", "true");
					properties.setProperty("rpc.ntlm.keyLength", "128");
					properties.setProperty("rpc.ntlm.ntlm2", "true");
					properties.setProperty(Security.USERNAME, session.UserName);
					properties.setProperty(Security.PASSWORD, session.Password);
					properties.setProperty("rpc.ntlm.ntlm2", "true");
				}

				if (session.NTLMv2Enabled) {
					properties.setProperty("rpc.ntlm.ntlmv2", "true");
				}

				JIComOxidRuntimeHelper remUnknown = new JIComOxidRuntimeHelper(properties);


				//now create a new JIComOxidDetails
				//this carries a reference to the javaInstance , incase we do not get pings from the client
				//at the right times, the cleaup thread will remove this entry and it's OXID as well from both the maps.
				JIComOxidDetails details = new JIComOxidDetails(component,oxid,oid,iid,ipid,ptr,remUnknown,protecttionLevel);


				MapOfJavaVsOxidDetails[component] = details;

				MapOfOxidVsOxidDetails[oxid] = details;

				MapOfOIDVsComponents[oid] = component;

				ListOfExportedJavaComponents.Add(component);

				MapOfIPIDVsComponent[ipid] = details; //this is the ipid of the component.

				IList oids = (List<object>)MapOfSessionIdsVsOIDs.GetValueOrNull(new int?(session.SessionIdentifier));
				if (oids == null) {
					oids = new List<object>();
					MapOfSessionIdsVsOIDs[new int?(session.SessionIdentifier)] = oids;
				}
				oids.Add(oid);

				component.AssociatedInterfacePointer = ptr;
			}
			return ptr;
		}

		//will get called from OxidResolverImpl only
		internal static JIComOxidDetails GetOxidDetails(JIOxid oxid) {
			lock (Mutex2) {
				return (JIComOxidDetails)MapOfOxidVsOxidDetails.GetValueOrNull(oxid);
			}
		}

		//Will get called from RemQueryInterface of IRemUnknown, when it gets the IPID 
		//it will identify the correct component to act on.
		//on this component the IID (provided again by the client) will do a exportInstance, with a 
		//randomly generated IPID and this IPID will be returned to the client.
		//The oid be the one present in details object.
		//Now , when the alter context call will come with the new IID (which was just QIed), the 
		//state of RemUnknownObject will get set for the correct component using getJavaComponentForIID.
		//The next call of requestcopdu will contain the request along with the field object having the IPID of the 
		//instance to call on. Pass this to the components (identified previously) invoke API., along with the rest of params
		//How will the request get decoded with out IDL info ??? Hard code for now for toString ??
		internal static JIComOxidDetails GetComponentFromIPID(string ipid) {
			lock (Mutex2) {
				return (JIComOxidDetails)MapOfIPIDVsComponent.GetValueOrNull(ipid);
			}
		}


		internal static void AddUpdateSets(JISetId setId, List<object> objectIdsAdded, List<object> objectIdsDel) {
			lock (Mutex2) {


				List<object> listOfOIDs = (List<object>)MapOfSetIdVsListOfOIDs.GetValueOrNull(setId);

				if (listOfOIDs == null) {
					listOfOIDs = new List<object>();
					//first time
					listOfOIDs.AddRange(objectIdsAdded);
					MapOfSetIdVsListOfOIDs[setId] = listOfOIDs;
					//del list would be empty I presume

				}
				else {
					for (int i = 0; i < listOfOIDs.Count; i++) {
						JIObjectId oid = (JIObjectId)listOfOIDs[i];
						if (!objectIdsDel.Contains(oid)) {
							oid.UpdateLastPingTime();
						}
					}

					listOfOIDs.AddRange(objectIdsAdded);
				}

			}
		}

		//since the IID is unique and we have to consider nested IIDs, this API will not work for component's IID
	//	static JILocalCoClass getJavaComponentForIID(String uniqueIID) 
	//	{
	//		JILocalCoClass component = null;
	//		synchronized (mutex2) {
	//			for (int i = 0; i < listOfExportedJavaComponents.size(); i++ )
	//			{
	//				component = (JILocalCoClass)listOfExportedJavaComponents.get(i);
	//				if (component.isPresent(uniqueIID))
	//				{
	//					break;
	//				}
	//				component = null;
	//			}
	//		}
	//		
	//		return component;
	//	}

		internal static JILocalCoClass GetJavaComponentFromIPID(string ipid) {
			JILocalCoClass component = null;
			lock (Mutex2) {
				for (int i = 0; i < ListOfExportedJavaComponents.Count; i++) {
					component = (JILocalCoClass)ListOfExportedJavaComponents[i];
					//this will be unique, no two components will ever have same IPID for an IID.They will have different IPIDs for same IIDs.
					if (component.GetIIDFromIpid(ipid) != null) {
						break;
					}
					component = null;
				}
			}

			return component;
		}

	}

}