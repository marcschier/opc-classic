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




	using IJIAuthInfo = org.jinterop.dcom.common.IJIAuthInfo;
	using IJIUnreferenced = org.jinterop.dcom.common.IJIUnreferenced;
	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JIException = org.jinterop.dcom.common.JIException;
	using JISystem = org.jinterop.dcom.common.JISystem;
	using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;

	/// <summary>
	///<para>Representation of an active session with a COM server. All interface references being given out by
	/// the framework for a particular COM server are maintained by the session and an <code>IJIComObject</code>
	/// is associated with a single session only. Sessions are also responsible for the clean up once the system
	/// </para>
	/// shuts down or <code>IJIComObject</code> go out of reference scope.<para>
	/// 
	/// Please make sure that you call <seealso cref="#destroySession(JISession)"/> after you are done using the session.
	/// This will ensure that any open sockets to COM server are closed.
	/// 
	/// @since 1.0
	/// </para>
	/// </summary>
	public sealed class JISession {

		private static Random RandomGen = new Random(double.doubleToRawLongBits(new Random(1).NextDouble()));
		private int SessionIdentifier_Renamed = -1;
		private string Username = null;
		private string Password_Renamed = null;
		private string Domain_Renamed = null;
		private string TargetServer_Renamed = null;
		private static IDictionary MapOfObjects = Collections.synchronizedMap(new Hashtable());
		private static object Mutex = new object();
		private IJIAuthInfo AuthInfo_Renamed = null;
		private JIComServer Stub_Renamed = null;
		private JIRemUnknownServer Stub2_Renamed = null;
		private static int OxidResolverPort_Renamed = -1;
		private static sbyte[] Localhost = new sbyte[]{ 127,0,0,1 };
		private static string LocalhostStr = "127.0.0.1";
		private static string LocalhostStr2 = "LOCALHOST";
		private static IDictionary MapOfSessionIdsVsSessions = new Hashtable();
		private static List<object> ListOfSessions = new List<object>();
		private IList ListOfDeferencedIpids = new List<object>();
		private static Timer ReleaseRefsTimer = new Timer(true);
		private IDictionary MapOfUnreferencedHandlers = new Hashtable();
		private int Timeout = 0;
		private bool UseSessionSecurity_Renamed = false;
		private bool UseNTLMv2_Renamed = false;
		private bool IsSSO = false;
		private List<object> Links = new List<object>();
		private static readonly IDictionary MapOfOxidsVsJISessions = new Hashtable();
		private static readonly IDictionary<string, JIComCustomMarshallerUnMarshaller> MapOfCustomCLSIDs = new Dictionary<string, JIComCustomMarshallerUnMarshaller>();
		private bool SessionInDestroy_Renamed = false;
		private IDictionary MapOfIPIDsVsRefcounts = new Hashtable();
		private IDictionary MapOfIPIDsVsWeakReferences = new Hashtable();

		private class IPID_SessionID_Holder {
			public readonly string IPID;
			public readonly int? SessionID;
			public readonly bool IsOnlySessionIDPresent;
			public readonly sbyte[] Oid;
			public IPID_SessionID_Holder(string IPID, int sessionID, bool isOnlySessionId, sbyte[] oid) {
				this.IPID = IPID;
				this.IsOnlySessionIDPresent = isOnlySessionId;
				this.SessionID = new int?(sessionID);
				this.Oid = oid;
			}
		}
		//static List listOfSessions = new ArrayList();
		//will be read by the system thread for cleanup and then passed
		//to each session for clean up.
		internal static ReferenceQueue ReferenceQueueOfCOMObjects = new ReferenceQueue();
		internal static Thread cleanUpThread = new Thread(new RunnableAnonymousInnerClassHelper(),"jI_GarbageCollector");

		private class RunnableAnonymousInnerClassHelper : Runnable {
			public RunnableAnonymousInnerClassHelper() {
			}

			public virtual void Run() {
				try {
					while (true) {
						Reference r = ReferenceQueueOfCOMObjects.remove();
						if (r != null) {
							// Object is no longer referenced.
							//get from hash map and call release ref on that object
							IPID_SessionID_Holder holder = null;
							lock (MapOfObjects) {
								holder = (IPID_SessionID_Holder)MapOfObjects.Remove(r);
								if (holder == null) {
									continue;
								}
							}

							JISession session = null;
							lock (Mutex) {
								session = (JISession)MapOfSessionIdsVsSessions.GetValueOrNull(holder.SessionID);
							}
							//this means that the session got lost...but this logic does not work, since
							//session is strongly referenced from mapOfSessionIdsVsSessions and listOfSessions and even putting
							//WeakReference for JISession when adding it to the mapOfSessionIdsVsSessions/listOfSessions does not
							//make a difference as we always loose the session to GC before it come here.
							if (holder.IsOnlySessionIDPresent) {
								try {
									DestroySession(session);
								}
								catch (Exception e) {
									if (JISystem.Logger.isLoggable(Level.FINEST)) {
										JISystem.Logger.finest("exception from destroy session in clean up thread: " + e.Message);
									}
								}
							}
							else {
								//session may have been "destroySession"...
								if (session == null) {
									continue;
								}

								try {
									string IPID = holder.IPID;

									// Since we are freeing up all references for the given IPID together, ensure
									// that all weak-references for this IPID have been dereferenced before it to
									// the list of Dereferenced IPIDs. The Reference Queue mechanism ensures that
									// any reference only comes here once.

									int weakRefsRemaining = session.RemoveWeakReference(IPID);

									// Decrement the ref-count for the oid too.
									//Will call the JIComOxidRuntime, and that is synched on mutex3, but that will not cause a deadlock, since
									//it or rather any method of JIComOxidRuntime does not call back into JISession.
									JIComOxidRuntime.DelIPIDReference(IPID, new JIObjectId(holder.Oid, false), session);

									// Only proceed to de-list this IPID for clearance if all weak-references were
									// released.
									if (weakRefsRemaining > 0) {
										continue;
									}

									//JIComOxidRuntime.delIPIDReference(IPID);
									//session.releaseRef(IPID); Not doing release anymore, this causes a lot of calls to
									//go across, so will save these in this list and then the cleanup thread will deal with
									//this every 3 minutes.
									if (JISystem.Logger.isLoggable(Level.FINEST)) {
											JISystem.Logger.finest("Adding Dereferenced IPID " + IPID + " session " + session.SessionIdentifier);
									}

									session.AddDereferencedIpids(IPID);
									holder = null;
									IJIUnreferenced unreferenced = (IJIUnreferenced)session.GetUnreferencedHandler(IPID);
									if (unreferenced != null) {
										unreferenced.UnReferenced();
									}
									session.UnregisterUnreferencedHandler(IPID);
								}
								catch (Exception e) {
									if (JISystem.Logger.isLoggable(Level.INFO)) {
										JISystem.Logger.info("exception from removing a IPID from session in clean up thread: " + e.Message);
									}
								}
							}
						}


					}
				}
				catch (Exception e) {
					JISystem.Logger.throwing("JISession","CleanupThread:run()",e);
				}
			}
		}

		//from JDK bug http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=4665037
		private static string GetLocalHost(string destination) {
			DatagramSocket sock;
			InetAddress intendedDestination;
			try {
				sock = new DatagramSocket();
				intendedDestination = InetAddress.getByName(destination);
			}
			catch (Exception) {
				return "127.0.0.1";
			}
			sock.connect(intendedDestination,sock.LocalPort);
			return sock.LocalAddress.HostAddress;
		}

		static JISession() {
			JISystem.Internal_initLogger();
			try {
				InetAddress localhostAddr = InetAddress.LocalHost;
				Localhost = localhostAddr.Address;
				LocalhostStr = localhostAddr.HostAddress;
				LocalhostStr2 = localhostAddr.CanonicalHostName;
			}
			catch (UnknownHostException) {
			}

			System.setProperty("jcifs.smb.client.domain","JIDomain"); //is being put in for completing type2 message
			//somehow windows is not taking empty domain name.

			//start the cleanup thread.
			// and create a shutdown hook also.
			cleanUpThread.Daemon = true;
			//cleanUpThread.setPriority(Thread.MIN_PRIORITY);
			cleanUpThread.start();

			JIComOxidRuntime.StartResolver();
			JIComOxidRuntime.StartResolverTimer();
			OxidResolverPort_Renamed = JIComOxidRuntime.OxidResolverPort;
			// This schedule used to be every 2 mins. 
			ReleaseRefsTimer.scheduleAtFixedRate(new Release_References_TimerTask(),0,2 * 60 * 1000);

			Runtime.Runtime.addShutdownHook(new Thread(new RunnableAnonymousInnerClassHelper2(),"jI_ShutdownHook"));


		}

		private class RunnableAnonymousInnerClassHelper2 : Runnable {
			public RunnableAnonymousInnerClassHelper2() {
			}

			public virtual void Run() {
					int i = 0;
					while (i < ListOfSessions.Count) {
						JISession session = (JISession)(ListOfSessions[i]);
						try {
							JISession.DestroySession(session);
						}
						catch (JIException e) {
							JISystem.Logger.throwing("JISession","addShutDownHook Thread:run()",e);
						}
						i++;
					}
					JISystem.Internal_writeProgIdsToFile();
					JIComOxidRuntime.StopResolver();
					ReleaseRefsTimer.cancel();
					MapOfSessionIdsVsSessions.Clear();
					MapOfObjects.Clear();
					ListOfSessions.Clear();
			}
		}

		/// <summary>
		/// Cancels the existing timer used to schedule collection of un-referenced COM Objects and then restarts the same with the new frequency. Default timer schedules the GC task 
		/// every 2 mins.  
		/// </summary>
		/// <param name="timeInMilliSec"> </param>
		public static int ReleaseRefTimerFrequency {
			set {
				ReleaseRefsTimer.cancel();
				ReleaseRefsTimer = new Timer(true);
				ReleaseRefsTimer.scheduleAtFixedRate(new Release_References_TimerTask(), 0, value);
			}
		}

		private class Release_References_TimerTask : TimerTask {
			public virtual void Run() {
				try {
					// Use a clone so we dont hold on to the mutex for longer than required.
					IList listOfSessionsClone = null;
					lock (Mutex) {
						listOfSessionsClone = (IList)ListOfSessions.clone();
					}

					int i = 0;

					while (i < listOfSessionsClone.Count) {
						JISession session = (JISession)listOfSessionsClone[i];


						if (JISystem.Logger.isLoggable(Level.INFO)) {
							JISystem.Logger.info("Release_References_TimerTask:[RUN] Ipid Vs Count Map size " + session.MapOfIPIDsVsRefcounts.Count + " listOfDeferencedIpids size " + session.ListOfDeferencedIpids.Count);
							JISystem.Logger.info("Release_References_TimerTask:[RUN] Session:  " + session.SessionIdentifier + " , listOfDeferencedIpids: " + session.ListOfDeferencedIpids);
						}

						//now iterate over each sessions listOfDereferencedIpids and send a call to release for the entire lot.
						List<object> listToKill = new List<object>();
						IList dereferencedIpids = null;

						// Use a clone so we dont hold on to the mutex for longer than required.
						lock (Mutex) {
							dereferencedIpids = (IList)((List<object>)session.ListOfDeferencedIpids).clone();
						}

						for (int j = 0;j < dereferencedIpids.Count;j++) {
							try {
								string ipid = (string)dereferencedIpids[j];
								listToKill.Add(session.PrepareForReleaseRef(ipid));
							}
							catch (JIException e) {
								//eaten, will never get thrown from the try block.
								if (JISystem.Logger.isLoggable(Level.INFO)) {
										JISystem.Logger.info("Release_References_TimerTask:[RUN] Exception preparing for release " + e);
								}
							}
						}
					lock (Mutex) {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
						session.ListOfDeferencedIpids.removeAll(dereferencedIpids);
					}

					dereferencedIpids.Clear();

					if (JISystem.Logger.isLoggable(Level.INFO)) {
							JISystem.Logger.info("Release_References_TimerTask:[RUN] Ipid Vs Count Map size after preparing release " + session.MapOfIPIDsVsRefcounts.Count);
					}

					if (listToKill.Count > 0) {
						JIArray array = new JIArray(listToKill.ToArray(typeof(JIStruct)),true);
						try {
							session.ReleaseRefs(array,false);
						}
						catch (JIException e) {
								//This release cycle has to go on.
								JISystem.Logger.logp(Level.SEVERE,"JISession","Release_References_TimerTask:run()","Exception in internal GC",e);
						}
					}

					i++;
					}
				}
				catch (Exception e) {
					//This release cycle has to go on.
					JISystem.Logger.logp(Level.SEVERE,"JISession","Release_References_TimerTask:run()","Exception in internal GC",e);
				}
			}
		}



		public string TargetServer {
			set {
				if (value.Equals("127.0.0.1", StringComparison.CurrentCultureIgnoreCase)) {
					//Replace with it's actual bindings, otherwise does not work for JCIFS authentication
					this.TargetServer_Renamed = LocalhostAddressAsIPString;
				}
				else {
					this.TargetServer_Renamed = value;
    
					//will change the localhost to the actual address as well
					if (LocalhostStr.Equals("127.0.0.1", StringComparison.CurrentCultureIgnoreCase) || LocalhostStr.Equals("0.0.0.0", StringComparison.CurrentCultureIgnoreCase))
					{ //Bug in JDK , time to find alternate logic.
						LocalhostStr = GetLocalHost(value);
					}
    
				}
    
    
			}
			get {
				return TargetServer_Renamed;
			}
		}

		internal static sbyte[] LocalhostAddressAsIPbytes {
			get {
				return Localhost;
			}
		}

		internal static string LocalhostAddressAsIPString {
			get {
				return LocalhostStr;
			}
		}

		internal static string LocalhostCanonicalAddressAsString {
			get {
				return LocalhostStr2;
			}
		}



		private JISession() {
		};

		internal static int OxidResolverPort {
			get {
				return OxidResolverPort_Renamed;
			}
		}

		/// <summary>
		///Returns the <code>IJIAuthInfo</code> (if any) associated with this session.
		/// 
		/// @return
		/// </summary>
		public IJIAuthInfo AuthInfo {
			get {
				return AuthInfo_Renamed;
			}
		}

		/// <summary>
		/// Creates a session with the <code>authInfo</code> of the user. This session is not yet attached to a
		/// COM server.
		/// </summary>
		/// <param name="authInfo">
		/// @return </param>
		/// <exception cref="IllegalArgumentException"> if <code>authInfo</code> is <code>null</code>. </exception>
		/// <seealso cref= JIComServer#JIComServer(JIClsid, JISession) </seealso>
		/// <seealso cref= JIComServer#JIComServer(JIProgId, JISession) </seealso>
		public static JISession CreateSession(IJIAuthInfo authInfo) {
			if (authInfo == null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_AUTH_NOT_SUPPLIED));
			}

			JISession session = new JISession();

			session.AuthInfo_Renamed = authInfo;

			session.SessionIdentifier_Renamed = authInfo.UserName.GetHashCode() ^ authInfo.Password.GetHashCode() ^ authInfo.Domain.GetHashCode() ^ (new object()).GetHashCode() ^ (int)Runtime.Runtime.freeMemory() ^ RandomGen.Next();


			lock (Mutex) {
				MapOfSessionIdsVsSessions[new int?(session.SessionIdentifier_Renamed)] = session;
				ListOfSessions.Add(session);
			}

			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("Created Session: " + session.SessionIdentifier_Renamed);
			}
			return session;
		}




		/// <summary>
		/// Creates a session. This session is not yet attached to a
		/// COM server.
		/// </summary>
		/// <param name="domain"> domain of the user. </param>
		/// <param name="username"> name of the user </param>
		/// <param name="password"> password of the user.
		/// @return </param>
		/// <exception cref="IllegalArgumentException"> if any parameter is <code>null</code>. </exception>
		/// <seealso cref= JIComServer#JIComServer(JIClsid, JISession) </seealso>
		/// <seealso cref= JIComServer#JIComServer(JIProgId, JISession) </seealso>
		public static JISession CreateSession(string domain, string username, string password) {
			if (username == null || password == null || domain == null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_AUTH_NOT_SUPPLIED));
			}

			JISession session = new JISession();
			session.Username = username;
			session.Password_Renamed = password;
			session.Domain_Renamed = domain;
			session.SessionIdentifier_Renamed = username.GetHashCode() ^ password.GetHashCode() ^ domain.GetHashCode() ^ (new object()).GetHashCode() ^ (int)Runtime.Runtime.freeMemory() ^ RandomGen.Next();


			lock (Mutex) {
				MapOfSessionIdsVsSessions[new int?(session.SessionIdentifier_Renamed)] = session;
				ListOfSessions.Add(session);
			}

			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("Created Session: " + session.SessionIdentifier_Renamed);
			}
			//System.out.println("Created Session: " + session.sessionIdentifier);
			return session;
		}


		/// <summary>
		/// Creates a new session using credentials of the <code>session</code>parameter. The new session is not yet attached to a
		/// COM server.
		/// </summary>
		/// <param name="session">
		/// @return </param>
		/// <seealso cref= JIComServer#JIComServer(JIClsid, JISession) </seealso>
		/// <seealso cref= JIComServer#JIComServer(JIProgId, JISession) </seealso>
		public static JISession CreateSession(JISession session) {
			JISession newSession = CreateSession(session.Domain,session.UserName,session.Password);
			newSession.AuthInfo_Renamed = session.AuthInfo_Renamed;
			return newSession;

		}

		/// <summary>
		/// <b>Native</b> Single Sign On capable session. 
		/// 
		///  <b>Warning:</b> <ul><li>This method works <b>only</b> on Microsoft Windows Platform.</li>
		///  <li>It does <b>not</b> support NTLMv2 or NTLM1 Session Security.</li>
		///  <li>It supports only NTLM1 Authentication.</li>
		///  <li>This session <b>cannot</b> be used with <code>JIComServer(ProgId,...)</code> ctors. JCIFS will
		///  fail to setup a connection with Windows Registry if GUEST account is disabled.</li></ul> 
		/// 
		/// @return </summary>
		/// <seealso cref= JIComServer#JIComServer(JIClsid, JISession) </seealso>
		/// <seealso cref= JIComServer#JIComServer(JIProgId, JISession) </seealso>
		public static JISession CreateSession() {
			if (!System.getProperty("os.name").ToLower().StartsWith("windows", StringComparison.Ordinal)) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_WIN_ONLY));
			}

			JISession session = new JISession();
			session.SessionIdentifier_Renamed = (new object()).GetHashCode() ^ (int)Runtime.Runtime.freeMemory() ^ RandomGen.Next();
			session.IsSSO = true;

			lock (Mutex) {
				MapOfSessionIdsVsSessions[new int?(session.SessionIdentifier_Renamed)] = session;
				ListOfSessions.Add(session);
			}

			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("Created Session for SSO: " + session.SessionIdentifier_Renamed);
			}

			return session;
		}

		/// <summary>
		/// Returns whether this session is SSO or not.
		/// 
		/// @return
		/// </summary>
		public bool SSOEnabled {
			get {
				return IsSSO;
			}
		}

		/// <summary>
		///<para>Used to destroy the <code>session</code>, this release all references of the COM server and it's interfaces.
		/// It should be called in the end after the developer is done with the COM server.
		/// </para>
		/// <para>
		/// Note that all interface references belonging to sessions linked to this session will also be destroyed.
		/// 
		/// </para>
		/// </summary>
		/// <param name="session"> </param>
		/// <exception cref="JIException"> </exception>
		/// <seealso cref= JIObjectFactory#narrowObject </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void destroySession(JISession session) throws org.jinterop.dcom.common.JIException
		public static void DestroySession(JISession session) {
			//null session
			if (session == null) {
				return;
			}

			//if stub is null then cleanup datastructures holding the session object only
			if (session.Stub_Renamed == null) {
				lock (Mutex) {
					MapOfSessionIdsVsSessions.Remove(new int?(session.SessionIdentifier));
					ListOfSessions.Remove(session);
				}

				//now remove the links and the OIDs
				PostDestroy(session);
				return;
			}

			try {
				//session may have been destroyed and this call is from finalize.
				List<object> list = new List<object>();
				List<object> listOfFreeIPIDs = new List<object>();
				lock (Mutex) {
					if (session.SessionInDestroy_Renamed) {
						return;
					}
					session.SessionInDestroy_Renamed = true;
					//list of dereferenced IPIDs
					for (int j = 0;j < session.ListOfDeferencedIpids.Count;j++) {
						list.Add(session.PrepareForReleaseRef((string)session.ListOfDeferencedIpids[j]));
					}
					listOfFreeIPIDs.AddRange(session.ListOfDeferencedIpids);
					session.ListOfDeferencedIpids.Clear();
				}

				lock (MapOfObjects) {
					//now take all the objects registered with this session and call release on them.
	//				Iterator iterator = mapOfObjects.keySet().iterator();
					IEnumerator iterator = MapOfObjects.SetOfKeyValuePairs().GetEnumerator();
					while (iterator.hasNext()) {
						//String ipid = (String)session.mapOfObjects.get(iterator.next());
						DictionaryEntry entry = (DictionaryEntry)iterator.next();
	//					IPID_SessionID_Holder holder = (IPID_SessionID_Holder)mapOfObjects.get(iterator.next());
						IPID_SessionID_Holder holder = (IPID_SessionID_Holder)entry.Value;
						if (session.SessionIdentifier != (int)holder.SessionID) {
							continue;
						}
						string ipid = holder.IPID;
						if (ipid == null) {
							continue;
						}

						//Commenting the line below since there could be more than one reference of a COM object taken in by
						//j-Interop (via the client of j-Interop) and mapOfObjects will contain two references in this case.
						//This was identified for the issue reported by Aquafold in sql dbg.
	//					if (!listOfFreeIPIDs.contains(ipid))
						{
							list.Add(session.PrepareForReleaseRef(ipid));
							listOfFreeIPIDs.Add(ipid);
						}
						iterator.remove();
					}
				}

				//now to kill the stub itself
				if (session.Stub_Renamed.ServerInterfacePointer != null) {
					if (!listOfFreeIPIDs.Contains(session.Stub_Renamed.ServerInterfacePointer.IPID)) {
						list.Add(session.PrepareForReleaseRef(session.Stub_Renamed.ServerInterfacePointer.IPID));
						listOfFreeIPIDs.Add(session.Stub_Renamed.ServerInterfacePointer.IPID);
					}
				}

				listOfFreeIPIDs.Clear();
				//release is performed if only something is in the session.
				if (list.Count > 0) {
					JIArray array = new JIArray(list.ToArray(typeof(JIStruct)),true);
					try {
						session.Stub_Renamed.CloseStub(); //close the existing connection
						session.ReleaseRefs(array,true);
					}
					catch (JIException e) {
						//This release cycle has to go on.
						JISystem.Logger.throwing("JISession","destroySession",e);
					}
				}

				JIComOxidRuntime.ClearIPIDsforSession(session);
				if (JISystem.Logger.isLoggable(Level.INFO)) {
					JISystem.Logger.info("Destroyed Session: " + session.SessionIdentifier_Renamed);
				}
			}
			finally {
				lock (Mutex) {
					MapOfSessionIdsVsSessions.Remove(new int?(session.SessionIdentifier));
					ListOfSessions.Remove(session);
					// and remove its entry from the map
					if (session.Stub_Renamed.ServerInterfacePointer != null) {
						MapOfOxidsVsJISessions.Remove(new JIOxid(session.Stub_Renamed.ServerInterfacePointer.OXID));
					}
				}
				session.Stub_Renamed.CloseStub();
				session.Stub2_Renamed.CloseStub();
			}

			PostDestroy(session);
			session.Stub_Renamed = null; //setting it null in the end.
			session.Stub2_Renamed = null;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void postDestroy(JISession session) throws org.jinterop.dcom.common.JIException
		private static void PostDestroy(JISession session) {
			//now destroy all linked sessions
			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("About to destroy links for Session: " + session.SessionIdentifier + " , size of which is " + session.Links.Count);
			}

			for (int i = 0; i < session.Links.Count;i++) {
				JISession.DestroySession((JISession)session.Links[i]);
			}

			session.Links.Clear();
			//finally any oids exported by this session.
			JIComOxidRuntime.DestroySessionOIDs(session.SessionIdentifier);
		}

		//each session is associated with 1 and only 1 stub.
		//adding something new now another stub for IRemUnknown operations
		public JIComServer Stub {
			set {
				this.Stub_Renamed = value;
				lock (Mutex) {
					MapOfOxidsVsJISessions[new JIOxid(value.ServerInterfacePointer.OXID)] = this;
				}
			}
			get {
				return this.Stub_Renamed;
			}
		}

		//IRemUnknown Stub
		public JIRemUnknownServer Stub2 {
			set {
				this.Stub2_Renamed = value;
				//no need to add this to the Oxid vs Sessions map as we would be using the same interface pointer as the other value.
			}
			get {
				return this.Stub2_Renamed;
			}
		}



		/// <summary>
		/// @exclude </summary>
		/// <param name="IPID"> </param>
		public void AddToSession(IJIComObject comObject, sbyte[] oid) {

			//nothing will be done if the session is being destroyed.
			if (SessionInDestroy_Renamed) {
				return;
			}

			AddWeakReference(comObject, oid);

			//setting if NO PING flag has been set to true.
			AddToSession(comObject.Ipid,oid,((JIStdObjRef)comObject.Internal_getInterfacePointer().GetObjectReference(JIInterfacePointer.OBJREF_STANDARD)).Flags == 0x00001000);
			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info(" for IID: " + comObject.InterfaceIdentifier + " session: " + SessionIdentifier);
			}

			int refcount = ((JIStdObjRef)comObject.Internal_getInterfacePointer().GetObjectReference(JIInterfacePointer.OBJREF_STANDARD)).PublicRefs;
			UpdateReferenceForIPID(comObject.Ipid, refcount);


	//		Integer value = (Integer)mapOfIPIDSvsCount.get(comObject.getIpid());
	//		if (value == null)
	//		{
	//			mapOfIPIDSvsCount.put(comObject.getIpid(), new Integer(0));
	//		}

	//		debug_addIpids(comObject.getIpid(),((JIStdObjRef)comObject.internal_getInterfacePointer().getObjectReference(JIInterfacePointer.OBJREF_STANDARD)).getPublicRefs());
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void addRef_ReleaseRef(String IPID, JICallBuilder obj, int refcount) throws org.jinterop.dcom.common.JIException
		public void AddRef_ReleaseRef(string IPID, JICallBuilder obj, int refcount) {
			UpdateReferenceForIPID(IPID, refcount);
			Stub2.AddRef_ReleaseRef(obj);
		}

		private void UpdateReferenceForIPID(string ipid, int refcount) {
			int? value = (int?)MapOfIPIDsVsRefcounts.GetValueOrNull(ipid);
			if (value == null) {
				// Were we asked to release a ref that wasnt in our map?
				if (refcount < 0) {
						if (JISystem.Logger.isLoggable(Level.INFO)) {
								JISystem.Logger.info("[updateReferenceForIPID] Released IPID not found: " + ipid);
						}
						return;
				}
				else {
					value = new int?(0);
				}
			}
			int newCount = (int)value + refcount;
			if (newCount > 0) {
				MapOfIPIDsVsRefcounts[ipid] = new int?(newCount);
			}
			else {
				MapOfIPIDsVsRefcounts.Remove(ipid);
			}
		}

		public void AddWeakReference(IJIComObject comObject, sbyte[] oid) {
			IPID_SessionID_Holder holder = new IPID_SessionID_Holder(comObject.Ipid, SessionIdentifier, false, oid);
			lock (MapOfObjects) {
				MapOfObjects[new WeakReference(comObject, ReferenceQueueOfCOMObjects)] = holder;
			}
			// Increment the count for the number of weak-references for this IPID
			lock (MapOfIPIDsVsWeakReferences) {
				// Count all weak-references for a given IPID.
				int? count = (int?) MapOfIPIDsVsWeakReferences.GetValueOrNull(comObject.Ipid);
				if (count == null) {
					count = new int?(0);
				}
				MapOfIPIDsVsWeakReferences[comObject.Ipid] = new int?((int)count + 1);
			}
		}


		/* Reduce the count of weak-references stored in mapOfIPIDsVsWeakReferences and return the same. */
		public int RemoveWeakReference(string ipid) {
			if (JISystem.Logger.isLoggable(Level.FINEST)) {
				JISystem.Logger.finest("Dumping mapOfIPIDsVsWeakReferences " + MapOfIPIDsVsWeakReferences.ToString());
			}

			int weakRefsRemaining = 0;
			lock (MapOfIPIDsVsWeakReferences) {
				int? count = (int?)MapOfIPIDsVsWeakReferences.GetValueOrNull(ipid);
				if (count == null) {
					weakRefsRemaining = 0;
				}
				else {
					weakRefsRemaining = (int)count - 1;
					if (weakRefsRemaining > 0) {
						MapOfIPIDsVsWeakReferences[ipid] = new int?(weakRefsRemaining);
					}
					else {
						MapOfIPIDsVsWeakReferences.Remove(ipid);
					}
				}
			}

			return weakRefsRemaining;
		}



		//just for testing
		private static IDictionary MapOfIPIDSvsCount = Collections.synchronizedMap(new Hashtable());

		internal static void Debug_addIpids(string ipid, int num) {
	//		Integer value = (Integer)mapOfIPIDSvsCount.get(ipid);
	//		if (value == null)
	//		{
	//			value = new Integer(0);
	//		}
	//		mapOfIPIDSvsCount.put(ipid, new Integer(value.intValue() + num));
		}

		internal static void Debug_delIpids(string ipid, int num) {
	//		Integer value = (Integer)mapOfIPIDSvsCount.get(ipid);
	//		mapOfIPIDSvsCount.put(ipid, new Integer(value.intValue() - num));
		}

		/// <summary>
		/// @exclude </summary>
		/// <param name="IPID"> </param>
		private void AddToSession(string IPID, sbyte[] oid, bool dontping) {
			//Weak reference of the object
			//mapOfObjects.put(new WeakReference(IPID,referenceQueueOfCOMObjects),IPID);
			//it does not matter if we create a new OID here, the OxidCOMRunttime API uses the OID in the MAP , and not this one.
			JIObjectId joid = new JIObjectId(oid,dontping);
			JIComOxidRuntime.AddUpdateOXIDs(this,IPID,joid);
			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("[addToSession] Adding IPID: " + IPID + " to session: " + SessionIdentifier);
			}
		}

	//	private static Map mapOfIPIDSvsCount = new HashMap();



	//	public static void debug_dumpIpidVsCountMap()
	//	{
	//		if (JISystem.getLogger().isLoggable(Level.WARNING))
	//		{
	//			JISystem.getLogger().warning("Dumping mapOfIPIDSvsCount " + mapOfIPIDSvsCount.toString());
	//		}
	//	}

		//this gets called from the cleanupthread and no place else and it calls the releaseRef of session which
		//internally calls the add_releaseRef of the JIComServer, that method is synched at the instance level.
		//I was worried about a deadlock with destroySession , since that also ultimately calls the add_releaseRef, but
		//this will not happen since under a simultaneous destroy and removefromsession call , the "mutex" object will get synch.
		//If suppose a comServer.getInterface(...) is being done (which also calls releaseRef), then that is synched at instance level
		//and so is add_releaseRef (on the same instance), so deadlock won't happen there. If a simulataneous remove and getInterface call comes
		//then getInterface(which internally calls releaseRef) will go through, since releaseRef is not synched but the api it calls i.e. add_releaseRef is synched with the same lock
		//as getInterface. The remove will have to wait till that call gets over.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void releaseRef(String IPID) throws org.jinterop.dcom.common.JIException
		public void ReleaseRef(string IPID) {
			ReleaseRef(IPID, 5);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void releaseRef(String IPID,int numinstances) throws org.jinterop.dcom.common.JIException
		public void ReleaseRef(string IPID, int numinstances) {
			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("releaseRef:Reclaiming from Session: " + SessionIdentifier + " , the IPID: " + IPID + ", numinstances is " + numinstances);
			}
			JICallBuilder obj = new JICallBuilder(true);
			obj.ParentIpid = IPID;
			obj.Opnum = 2; //release
			//length
			obj.AddInParamAsShort((short)1,JIFlags.FLAG_NULL);
			//ipid to addfref on
			JIArray array = new JIArray(new rpc.core.UUID[]{ new rpc.core.UUID(IPID) },true);
			obj.AddInParamAsArray(array,JIFlags.FLAG_NULL);
			//TODO requesting 5 for now, will later build caching mechnaism to exhaust 5 refs first before asking for more
			// same with release.
			obj.AddInParamAsInt(numinstances,JIFlags.FLAG_NULL);
			obj.AddInParamAsInt(0,JIFlags.FLAG_NULL); //private refs = 0
			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.warning("releaseRef: Releasing numinstances " + numinstances + " references of IPID: " + IPID + " session: " + SessionIdentifier);
				Debug_delIpids(IPID, numinstances);
			}
			AddRef_ReleaseRef(IPID, obj, -5);
		}


		private void AddDereferencedIpids(string IPID) {
			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("addDereferencedIpids for session : " + SessionIdentifier + " , IPID is: " + IPID);
			}

			lock (Mutex) {
				if (!ListOfDeferencedIpids.Contains(IPID)) {
					ListOfDeferencedIpids.Add(IPID);
				}
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void releaseRefs(JIArray arrayOfStructs, boolean fromDestroy) throws org.jinterop.dcom.common.JIException
		private void ReleaseRefs(JIArray arrayOfStructs, bool fromDestroy) {
			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("In releaseRefs for session : " + SessionIdentifier + " , array length is: " + (short)(((object[])arrayOfStructs.ArrayInstance).Length));
			}

			JICallBuilder obj = new JICallBuilder(true);
			obj.Opnum = 2; //release
			//length
			obj.AddInParamAsShort((short)(((object[])arrayOfStructs.ArrayInstance).Length),JIFlags.FLAG_NULL);
			obj.AddInParamAsArray(arrayOfStructs,JIFlags.FLAG_NULL);
			obj.FromDestroySession = fromDestroy;
			Stub_Renamed.AddRef_ReleaseRef(obj);

			//ignore the results
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private JIStruct prepareForReleaseRef(String IPID) throws org.jinterop.dcom.common.JIException
		private JIStruct PrepareForReleaseRef(string IPID) {
			int? refcount = (int?)MapOfIPIDsVsRefcounts.GetValueOrNull(IPID);
			int releaseCount = 5 + 5; // 5 of the original and 5 for the addRef done later on.
			if (refcount != null) {
				releaseCount = (int)refcount;
			}

			return PrepareForReleaseRef(IPID, releaseCount);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private JIStruct prepareForReleaseRef(String IPID, int refcount) throws org.jinterop.dcom.common.JIException
		private JIStruct PrepareForReleaseRef(string IPID, int refcount) {
			JIStruct remInterface = new JIStruct();
			remInterface.AddMember(new rpc.core.UUID(IPID));
			remInterface.AddMember(new int?(refcount));
			remInterface.AddMember(new int?(0)); //private refs = 0
			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.warning("prepareForReleaseRef: Releasing " + refcount + "references of IPID: " + IPID + " session: " + SessionIdentifier);
				Debug_delIpids(IPID, refcount);
			}
			UpdateReferenceForIPID(IPID, -1 * refcount);

			return remInterface;
		}

		/// <summary>
		/// Gets the user name associated with this session.
		/// 
		/// @return
		/// </summary>
		public string UserName {
			get {
				return AuthInfo_Renamed == null ? Username : AuthInfo_Renamed.UserName;
			}
		}

		public string Password {
			get {
				return AuthInfo_Renamed == null ? Password_Renamed : AuthInfo_Renamed.Password;
			}
		}

		/// <summary>
		///Gets the domain of the user associated with this session.
		/// 
		/// @return
		/// </summary>
		public string Domain {
			get {
				return AuthInfo_Renamed == null ? Domain_Renamed : AuthInfo_Renamed.Domain;
			}
		}



		/// <summary>
		///Returns a unique identifier for this session.
		/// 
		/// @return
		/// </summary>
		public int SessionIdentifier {
			get {
				return SessionIdentifier_Renamed;
			}
		}

		/// <summary>
		/// @exclude
		/// </summary>
		public override bool Equals(object obj) {

			if (obj == null || !(obj is JISession)) {
				return false;
			}

			JISession temp = (JISession)obj;
			return temp.SessionIdentifier_Renamed == SessionIdentifier_Renamed;
		}

		/// <summary>
		/// @exclude
		/// </summary>
		public override int GetHashCode() {
			return SessionIdentifier_Renamed;
		}

		~JISession() {
			try {
				DestroySession(this);
			}
			catch (JIException e) {
				if (JISystem.Logger.isLoggable(Level.FINEST)) {
					JISystem.Logger.finest("Exception in finalize when destroying session " + e.Message);
				}
			}
		}

		public IJIUnreferenced GetUnreferencedHandler(string ipid) {
			lock (this) {
					return (IJIUnreferenced)MapOfUnreferencedHandlers.GetValueOrNull(ipid);
			}
		}

		public void RegisterUnreferencedHandler(string ipid, IJIUnreferenced unreferenced) {
			lock (this) {
					MapOfUnreferencedHandlers[ipid] = unreferenced;
			}
		}

		public void UnregisterUnreferencedHandler(string ipid) {
			lock (this) {
					MapOfUnreferencedHandlers.Remove(ipid);
			}
		}

		/// <summary>
		///<para> Sets the timeout for all sockets opened to (not fro) the COM server for this session. Default value is 0 (no timeout).
		/// The class level and the method level settings in case of <code>IJIComObject</code> override this timeout. </para>
		/// </summary>
		/// <param name="timeout"> in millisecs </param>
		/// <seealso cref= IJIComObject#setInstanceLevelSocketTimeout(int) </seealso>
		/// <seealso cref= IJIComObject#call(JICallBuilder, int) </seealso>
		public int GlobalSocketTimeout {
			set {
				this.Timeout = value;
			}
			get {
				return this.Timeout;
			}
		}


		/// <summary>
		///<para> Sets the use of NTLM2 Session Security. Framework will use NTLM Packet Level Privacy and Sign\Seal all packets.
		/// Once the <code>JIComServer</code> is bound to this session (using any of the <code>JIComServer</code> constructors)
		/// the use of session security <b>cannot</b> be enabled or disabled.
		/// </para>
		/// <para>
		/// Please note that session security can come at any available level of authentication (LM\NTLM\LMv2\NTLMv2). The framework
		/// currently only supports sign and seal at NTLMv1 level.
		/// </para>
		/// <para>
		/// Whether to use NTLM1 or not is dictated by this field in the Windows Registry.
		/// </para>
		/// <para>
		/// <code>
		/// </para>
		/// HKLM\System\CurrentControlSet\Control\Lsa\LmCompatibilityLevel <para>
		/// </code>
		/// 
		/// This article on MSDN talks more about it http://support.microsoft.com/default.aspx?scid=KB;en-us;239869
		/// 
		/// </para>
		/// </summary>
		/// <param name="enable"> <code>true</code> to enable, <code>false</code> to disable. </param>
		public void UseSessionSecurity(bool enable) {
			UseSessionSecurity_Renamed = enable;
	//		if (enable)
	//		{
	//			useNTLMv2 = enable;
	//		}
		}

		/// <summary>
		/// <para> Sets the use of NTLMv2 Security (default is NTLM1). This can be used in combination with <code>useSessionSecurity</code> method.
		/// Once the <code>JIComServer</code> is bound to this session (using any of the <code>JIComServer</code> constructors)
		/// the use of NTLMv2 security <b>cannot</b> be enabled or disabled.
		/// </para>
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="enable"> <code>true</code> to enable. </param>
		public void UseNTLMv2(bool enable) {
			UseNTLMv2_Renamed = enable;
		}

		/// <summary>
		///<para> Flag indicating whether session security is enabled. </para>
		/// </summary>
		/// <returns> <code>true</code> for enabled. </returns>
		public bool SessionSecurityEnabled {
			get {
				return !IsSSO & UseSessionSecurity_Renamed;
			}
		}

		/// <summary>
		///<para> Flag indicating whether NTLMv2 security is enabled. </para>
		/// </summary>
		/// <returns> <code>true</code> for enabled. </returns>
	   public bool NTLMv2Enabled {
		   get {
				return !IsSSO & UseNTLMv2_Renamed;
		   }
	   }

		/// <summary>
		///<para> Links the src with target. These two sessions can now be destroyed in a cascade effect.
		/// </para>
		/// </summary>
		/// <param name="session"> </param>
		internal static void LinkTwoSessions(JISession src, JISession target) {
			if (src.SessionInDestroy_Renamed || target.SessionInDestroy_Renamed) {
				return;
			}

			if (src.Equals(target)) {
				return;
			}

			lock (Mutex) {
				if (!src.Links.Contains(target)) {
					src.Links.Add(target);
				}
			}
		}

		/// <summary>
		/// Removes session from src sessions list.
		/// 
		/// </summary>
		internal static void UnLinkSession(JISession src, JISession tobeunlinked) {
			if (src.SessionInDestroy_Renamed) {
				return;
			}

			if (src.Equals(tobeunlinked)) {
				return;
			}

			lock (Mutex) {
				src.Links.Remove(tobeunlinked);
			}
		}



		/// <summary>
		/// Based on the oxid returns the JISession (and thus the COM Server) associated with it. This is required, since there are
		/// cases where a different JISession may be passed in JIObjectFactory for an JIInterfacePointer which does not belong to this JISession.
		/// Under those scenarios, the COM factory will create a new instance of a JISession and associate that Interface pointer with the session.
		/// But that is not the right approach as a COM Server for that interface and thus a session might already exist and these have to be tied together.
		/// 
		/// @exclude
		/// </summary>
		internal static JISession ResolveSessionForOxid(JIOxid oxid) {
			lock (Mutex) {
				return (JISession)MapOfOxidsVsJISessions.GetValueOrNull(oxid);
			}
		}

		public bool SessionInDestroy {
			get {
				return SessionInDestroy_Renamed;
			}
		}


		/// <summary>
		/// Register handlers for OBJREF_CUSTOM. customClass only serves as a Template and is of no real consequence.
		/// A new copy is returned from customClass.decode(...) and that is used by framework internally.
		/// </summary>
		/// <param name="CLSID"> </param>
		/// <param name="customClass"> </param>
		public void RegisterCustomMarshallerUnMarshallerTemplate(string CLSID, JIComCustomMarshallerUnMarshaller customClass) {
			MapOfCustomCLSIDs[CLSID.ToLower()] = customClass;
		}

		public JIComCustomMarshallerUnMarshaller GetCustomMarshallerUnMarshallerTemplate(string CLSID) {
			return MapOfCustomCLSIDs.GetValueOrNull(CLSID.ToLower());
		}
	}

}