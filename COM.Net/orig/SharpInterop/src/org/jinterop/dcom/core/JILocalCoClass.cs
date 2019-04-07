using System;
using System.Collections;
using System.Collections.Generic;

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


	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JIException = org.jinterop.dcom.common.JIException;
	using JISystem = org.jinterop.dcom.common.JISystem;
	using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;

	using UUID = rpc.core.UUID;


	/// <summary>
	///<para>Represents a Java <code>COCLASS</code>.
	/// </para>
	/// <para>
	/// <i>Please refer to MSInternetExplorer, Test_ITestServer2_Impl, SampleTestServer
	/// and MSShell examples for more details on how to use this class.</i><br>
	/// 
	/// @since 2.0 (formerly JIJavaCoClass)
	/// 
	/// </para>
	/// </summary>
	[Serializable]
	public sealed class JILocalCoClass {

		private const long SerialVersionUID = 5542223845228327383L;
		private static Random RandomGen = new Random(double.doubleToRawLongBits(new Random(1).NextDouble()));
		private readonly int Identifier;
		private WeakReference InterfacePointer = null;
		private bool IsAlreadyExported = false;
		private sbyte[] ObjectID = null;
		private JILocalInterfaceDefinition InterfaceDefinition_Renamed = null;

		private const string IID_IDispatch = "00020400-0000-0000-c000-000000000046";

		private List<object> ListOfSupportedInterfaces = new List<object>();

		private List<object> ListOfSupportedEventInterfaces = new List<object>();

		private Hashtable MapOfIIDsToInterfaceDefinitions = new Hashtable();

		private JISession Session_Renamed = null;

		private bool RealIID = false;

		static JILocalCoClass() {

		}

		private IDictionary IpidVsIID = new Hashtable(); // will use this to identify which IID is being talked about
											  //if it is IDispatch then delegate to it's invoke.

		private IDictionary IIDvsIpid = new Hashtable(); // will use this to identify which IPID is being talked about

		private void Init(JILocalInterfaceDefinition interfaceDefinition, Type clazz, object instance, bool realIID) {
			ListOfSupportedInterfaces.Add(IID_IDispatch.ToUpper()); //IDispatch
			ListOfSupportedInterfaces.Add("00000131-0000-0000-C000-000000000046"); //IRemUnknown
			this.InterfaceDefinition_Renamed = interfaceDefinition;
			interfaceDefinition.Clazz = clazz;
			interfaceDefinition.Instance = instance;
			ListOfSupportedInterfaces.Add(interfaceDefinition.InterfaceIdentifier.ToUpper());
			MapOfIIDsToInterfaceDefinitions[interfaceDefinition.InterfaceIdentifier.ToUpper()] = interfaceDefinition;
			this.RealIID = realIID;
		}



		/// <summary>
		/// Creates a local class instance. The framework will try to create a instance of the <code>clazz</code>
		///  using <code>Class.newInstance</code>. Make sure that <code>clazz</code> has a visible <code>null</code>
		///  constructor.
		/// </summary>
		/// <param name="interfaceDefinition"> implementing structurally the definition of the COM callback interface. </param>
		/// <param name="clazz"> <code>class</code> to instantiate for serving requests from COM client. Must implement
		/// the <code>interfaceDefinition</code> fully. </param>
		/// <exception cref="IllegalArgumentException"> if <code>interfaceDefinition</code> or <code>clazz</code> are <code>null</code>. </exception>
		public JILocalCoClass(JILocalInterfaceDefinition interfaceDefinition, Type clazz) {
			if (interfaceDefinition == null || clazz == null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COM_RUNTIME_INVALID_CONTAINER_INFO));
			}
			this.Identifier = clazz.GetHashCode() ^ (new object()).GetHashCode() ^ RandomGen.Next();
			Init(interfaceDefinition,clazz,null,false);
		}

		/// <summary>
		/// Refer <seealso cref="#JILocalCoClass(JILocalInterfaceDefinition, Class)"/>.
		/// </summary>
		/// <param name="interfaceDefinition"> implementing structurally the definition of the COM callback interface. </param>
		/// <param name="clazz"> <code>class</code> to instantiate for serving requests from COM client. Must implement
		/// the <code>interfaceDefinition</code> fully. </param>
		/// <param name="useInterfaceDefinitionIID"> <code>true</code> if the <code>IID</code> of <code>interfaceDefinition</code
		/// should be used as to create the local COM Object. Use this when a reference other than <code>IUnknown*</code> is required.
		/// For all <seealso cref="JIObjectFactory#attachEventHandler(IJIComObject, String, IJIComObject)"/> operations this should be set to
		/// <code>false</code> since the <code>IConnectionPoint::Advise</code> method takes in a <code>IUnknown*</code> reference. </param>
		/// <exception cref="IllegalArgumentException"> if <code>interfaceDefinition</code> or <code>clazz</code> are <code>null</code>. </exception>
		public JILocalCoClass(JILocalInterfaceDefinition interfaceDefinition, Type clazz, bool useInterfaceDefinitionIID) {
			if (interfaceDefinition == null || clazz == null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COM_RUNTIME_INVALID_CONTAINER_INFO));
			}
			this.Identifier = clazz.GetHashCode() ^ (new object()).GetHashCode() ^ RandomGen.Next();
			Init(interfaceDefinition,clazz,null,useInterfaceDefinitionIID);
		}

		/// <summary>
		///Creates a local class instance.
		/// </summary>
		/// <param name="interfaceDefinition"> implementing structurally the definition of the COM callback interface. </param>
		/// <param name="instance"> instance for serving requests from COM client. Must implement
		/// the <code>interfaceDefinition</code> fully. </param>
		/// <exception cref="IllegalArgumentException"> if <code>interfaceDefinition</code> or <code>instance</code> are <code>null</code>. </exception>
		public JILocalCoClass(JILocalInterfaceDefinition interfaceDefinition, object instance) {
			if (interfaceDefinition == null || instance == null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COM_RUNTIME_INVALID_CONTAINER_INFO));
			}
			this.Identifier = instance.GetHashCode() ^ (new object()).GetHashCode() ^ RandomGen.Next();
			Init(interfaceDefinition,null,instance,false);
		}

		/// <summary>
		///Creates a local class instance.
		/// </summary>
		/// <param name="interfaceDefinition"> implementing structurally the definition of the COM callback interface. </param>
		/// <param name="instance"> instance for serving requests from COM client. Must implement
		/// the <code>interfaceDefinition</code> fully. </param>
		/// <param name="useInterfaceDefinitionIID"> <code>true</code> if the <code>IID</code> of <code>interfaceDefinition</code
		/// should be used as to create the local COM Object. Use this when a reference other than <code>IUnknown*</code> is required.
		/// For all <seealso cref="JIObjectFactory#attachEventHandler(IJIComObject, String, IJIComObject)"/> operations this should be set to
		/// <code>false</code> since the <code>IConnectionPoint::Advise</code> method takes in a <code>IUnknown*</code> reference. </param>
		/// <exception cref="IllegalArgumentException"> if <code>interfaceDefinition</code> or <code>instance</code> are <code>null</code>. </exception>
		public JILocalCoClass(JILocalInterfaceDefinition interfaceDefinition, object instance, bool useInterfaceDefinitionIID) {
			if (interfaceDefinition == null || instance == null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COM_RUNTIME_INVALID_CONTAINER_INFO));
			}
			this.Identifier = instance.GetHashCode() ^ (new object()).GetHashCode() ^ RandomGen.Next();
			Init(interfaceDefinition,null,instance,useInterfaceDefinitionIID);
		}



		/// <summary>
		///Sets the interface identifiers (<code>IID</code>s) of the event interfaces this class would support. This in case the same
		/// <code>clazz</code> or <code>instance</code> is implementing more than one <code>IID</code>.
		/// </summary>
		/// <param name="listOfIIDs"> </param>
		/// <seealso cref= #JILocalCoClass(JILocalInterfaceDefinition, Class) </seealso>
		/// <seealso cref= #JILocalCoClass(JILocalInterfaceDefinition, Object) </seealso>
		public IList SupportedEventInterfaces {
			set {
				if (value != null) {
					for (int i = 0;i < value.Count; i++) {
						string s = ((string)value[i]).ToUpper();
						ListOfSupportedInterfaces.Add(s);
						ListOfSupportedEventInterfaces.Add(s);
						MapOfIIDsToInterfaceDefinitions[s] = InterfaceDefinition_Renamed;
					}
    
				}
			}
		}

		/// <summary>
		///Add another interface definition and it's supporting object instance.
		/// </summary>
		/// <param name="interfaceDefinition"> implementing structurally the definition of the COM callback interface. </param>
		/// <param name="instance"> instance for serving requests from COM client. Must implement
		/// the <code>interfaceDefinition</code> fully. </param>
		/// <exception cref="IllegalArgumentException"> if <code>interfaceDefinition</code> or <code>instance</code> are <code>null</code>. </exception>
		public void AddInterfaceDefinition(JILocalInterfaceDefinition interfaceDefinition, object instance) {
			if (interfaceDefinition == null || instance == null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COM_RUNTIME_INVALID_CONTAINER_INFO));
			}
			interfaceDefinition.Instance = instance;
			string s = interfaceDefinition.InterfaceIdentifier.ToUpper();
			ListOfSupportedInterfaces.Add(s);
			ListOfSupportedEventInterfaces.Add(s);
			MapOfIIDsToInterfaceDefinitions[s] = interfaceDefinition;
		}

		/// <summary>
		/// Add another interface definition and it's class. Make sure that this class has a default constructor,
		/// so that instantiation using <i>reflection</i> can take place.
		/// </summary>
		/// <param name="interfaceDefinition"> implementing structurally the definition of the COM callback interface. </param>
		/// <param name="clazz"> instance for serving requests from COM client. Must implement
		/// the <code>interfaceDefinition</code> fully. </param>
		/// <exception cref="IllegalArgumentException"> if <code>interfaceDefinition</code> or <code>clazz</code> are <code>null</code>. </exception>
		public void AddInterfaceDefinition(JILocalInterfaceDefinition interfaceDefinition, Type clazz) {
			if (interfaceDefinition == null || clazz == null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COM_RUNTIME_INVALID_CONTAINER_INFO));
			}
			interfaceDefinition.Clazz = clazz;
			string s = interfaceDefinition.InterfaceIdentifier.ToUpper();
			ListOfSupportedInterfaces.Add(s);
			ListOfSupportedEventInterfaces.Add(s);
			MapOfIIDsToInterfaceDefinitions[s] = interfaceDefinition;
		}

		/// <summary>
		/// Returns the instance representing the interface definition. <br>
		/// @return </summary>
		/// <seealso cref= #JILocalCoClass(JILocalInterfaceDefinition, Object) </seealso>
		public object ServerInstance {
			get {
				return InterfaceDefinition_Renamed.Instance;
			}
		}

		/// <summary>
		/// Returns the actual class representing the interface definition. <br>
		/// @return </summary>
		/// <seealso cref= #JILocalCoClass(JILocalInterfaceDefinition, Class) </seealso>
		public Type ServerClass {
			get {
				return InterfaceDefinition_Renamed.Clazz;
			}
		}

	//	public boolean isDispatchSupported()
	//	{
	//		return isDispatchSupported;
	//	}

		//called from com runtime.
		/// <summary>
		/// @exclude
		/// </summary>
		public sbyte[] ObjectId {
			set {
				this.ObjectID = value;
			}
			get {
				return ObjectID;
			}
		}

		/// <summary>
		/// @exclude
		/// </summary>
		 public JIInterfacePointer AssociatedInterfacePointer {
			 set {
				 IsAlreadyExported = true;
				 this.InterfacePointer = new WeakReference(value);
				 string ipid = value.IPID.ToUpper();
				 string iid = value.IID.ToUpper();
				 IIDvsIpid[iid] = ipid;
				 IpidVsIID[ipid] = iid;
			 }
		 }

		/// 
		/// <summary>
		/// @exclude
		/// </summary>
		public bool AssociatedReferenceAlive {
			get {
				return InterfacePointer == null ? false : (InterfacePointer.get() == null ? false : true);
			}
		}

		 public bool AlreadyExported {
			 get {
				 return IsAlreadyExported;
			 }
		 }


		/// <summary>
		/// @exclude </summary>
		/// <param name="iid">
		/// @return </param>
		 public bool IsPresent(string iid) {
			iid = iid.ToUpper();
			return ListOfSupportedInterfaces.Contains(iid);
		 }

		/// <summary>
		/// @exclude </summary>
		/// <param name="uniqueIID"> </param>
		/// <param name="IPID"> </param>
		/// <exception cref="InstantiationException"> </exception>
		/// <exception cref="IllegalAccessException"> </exception>
		//advances the index...it cannot be reversed.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: synchronized boolean exportInstance(String uniqueIID,String IPID) throws InstantiationException, IllegalAccessException
		 public bool ExportInstance(string uniqueIID, string IPID) {
			 lock (this) {
				//Object retval = null;
				IPID = IPID.ToUpper();
        
				if (!IsPresent(uniqueIID)) { //not supported IID.
					return false;
				}
        
				IIDvsIpid[uniqueIID.ToUpper()] = IPID;
				IpidVsIID[IPID] = uniqueIID.ToUpper();
				return true;
			 }
		 }

		/// <summary>
		/// Returns the interface identifier of this COCLASS. <br>
		/// @return </summary>
		/// <seealso cref= #JILocalCoClass(JILocalInterfaceDefinition, Class) </seealso>
		/// <seealso cref= #JILocalCoClass(JILocalInterfaceDefinition, Object) </seealso>
		/// <seealso cref= JILocalInterfaceDefinition#getInterfaceIdentifier() </seealso>
		public string CoClassIID {
			get {
				return InterfaceDefinition_Renamed.InterfaceIdentifier;
			}
		}

		/// <summary>
		/// @exclude </summary>
		/// <param name="IPID"> </param>
		/// <param name="Opnum"> </param>
		/// <param name="inparams">
		/// @return </param>
		/// <exception cref="JIException"> </exception>
		//This will invoke the API via reflection and return the results of the call back to the
		//actual COM object. This API is to be invoked via the RemUnknown Object
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Object[] invokeMethod(String IPID,int Opnum,ndr.NetworkDataRepresentation ndr) throws org.jinterop.dcom.common.JIException
		public object[] InvokeMethod(string IPID, int Opnum, NetworkDataRepresentation ndr) {
			IPID = IPID.ToUpper();
			//somehow identify the method from the Opnum
			//this will come from the IDL.

			object retVal = null; //will be an array.

			string iid = (string)IpidVsIID.GetValueOrNull(IPID);
			if (iid == null) {
				throw new JIException(JIErrorCodes.RPC_E_INVALID_OBJECT);
			}

			JILocalInterfaceDefinition interfaceDefinitionOfClass = (JILocalInterfaceDefinition)MapOfIIDsToInterfaceDefinitions.GetValueOrNull(iid);
			interfaceDefinitionOfClass = interfaceDefinitionOfClass == null ? InterfaceDefinition_Renamed : interfaceDefinitionOfClass;

			JILocalMethodDescriptor methodDescriptor = null;
			bool execute = false;
			object[] @params = null;

			//that means the calls will come as IUnknown + IDispatch op numbers...0,1,2 & 3,4,5,6
			//from 7th (inclusive) onwards are the actual COM servers calls
			//now check for dispinterface and take a call...
			//if dispinterface is supported then all calls will come with base of 6 {0,1,2 & 3,4,5,6}
			//i.e 6th will be invoke and 7th(inclusive) onwards will be standard api calls.
			//if not supported than it will be base 2 {0,1,2} i.e real method calls will start from 3(inclusive) onwards.
			bool isStandardCall = true;
			if (InterfaceDefinition_Renamed.DispInterface) {
				isStandardCall = false;
				switch (Opnum) {
					case 3: //getTypeInfoCount
						//not supported
						retVal = new object[1];
						((object[])retVal)[0] = new int?(0); //not supported
						break;
					case 4: //getTypeInfo
						throw new JIException(JIErrorCodes.E_NOTIMPL);
					case 5: //getIDOfNames

						JILocalParamsDescriptor paramObject = new JILocalParamsDescriptor();

						paramObject.AddInParamAsType(typeof(UUID),JIFlags.FLAG_NULL);
						paramObject.AddInParamAsObject(new JIArray(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR),null,1,true),JIFlags.FLAG_NULL);
						paramObject.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
						paramObject.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);

						//now read and then send the result back.
						JIArray array = (JIArray)paramObject.Read(ndr)[1];

						object[] arrayObj = (object[])array.ArrayInstance;
						int?[] dispIds = new int?[arrayObj.Length];
						//get the first member of the Array , which is the APINAME and send the retVal with it's dispId
						JIString apiName = (JIString)arrayObj[0];
						JILocalMethodDescriptor info = interfaceDefinitionOfClass.GetMethodDescriptor(apiName.String);
						if (info == null) {
							dispIds[0] = new int?(JIErrorCodes.DISP_E_UNKNOWNNAME);
						}
						else {
							dispIds[0] = new int?(info.MethodNum);
						}

						//rest are all 0,1,2...parameters
						for (int i = 1;i < arrayObj.Length;i++) {
							dispIds[i] = new int?(i - 1);
						}

						JIArray results = new JIArray(dispIds);

						retVal = new object[1];
						((object[])retVal)[0] = results;

						break;
					case 6: //invoke of IDispatch

						paramObject = new JILocalParamsDescriptor();
						paramObject.Session = Session_Renamed;
						paramObject.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
						paramObject.AddInParamAsType(typeof(UUID),JIFlags.FLAG_NULL);
						paramObject.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
						paramObject.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);

						JIStruct dispParams = new JIStruct();
						dispParams.AddMember(new JIPointer(new JIArray(typeof(JIVariant),null,1,true)));
						dispParams.AddMember(new JIPointer(new JIArray(typeof(int?),null,1,true)));
						dispParams.AddMember(typeof(int?));
						dispParams.AddMember(typeof(int?));

						paramObject.AddInParamAsObject(dispParams,JIFlags.FLAG_REPRESENTATION_IDISPATCH_INVOKE);
						paramObject.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
						paramObject.AddInParamAsObject(new JIArray(typeof(int?),null,1,true),JIFlags.FLAG_NULL);
						paramObject.AddInParamAsObject(new JIArray(typeof(JIVariant),null,1,true),JIFlags.FLAG_NULL);

						object[] retresults = paramObject.Read(ndr);
						//named params not supported
						int dispId = (int)((int?)retresults[0]);

						info = interfaceDefinitionOfClass.GetMethodDescriptorForDispId(dispId);
						if (info == null) {
							if (JISystem.Logger.isLoggable(Level.SEVERE)) {
								JISystem.Logger.severe("MethodDescriptor not found for DispId :- " + dispId);
							}

							throw new JIException(JIErrorCodes.DISP_E_MEMBERNOTFOUND);
						}

						dispParams = (JIStruct)retresults[4];
						JIPointer ptrToParamsArray = (JIPointer)dispParams.GetMember(0);

						@params = new object[0];
						if (!ptrToParamsArray.Null) {
							//form the real array
							array = (JIArray)ptrToParamsArray.GetReferent();
							object[] variants = (object[])array.ArrayInstance;
							@params = new object[variants.Length];
							for (int i = 0;i < variants.Length;i++) {
								@params[i] = ((JIVariant)variants[i]).Object;
							}
						}

						if ((int)((int?)retresults[5]) != 0) {
							//now replace the params at index from the index array.
							array = (JIArray)retresults[6];
							int?[] indexs = (int?[])array.ArrayInstance;
							array = (JIArray)retresults[7];
							JIVariant[] variants = (JIVariant[])array.ArrayInstance;
							for (int i = 0;i < indexs.Length; i++) {
								@params[(int)indexs[i]] = variants[i];
							}


						}

						//now to reverse this array of params.
						int halflength = @params.Length / 2;
						for (int i = 0;i < halflength; i++) {
							object t = @params[i];
							@params[i] = @params[@params.Length - 1 - i];
							@params[@params.Length - 1 - i] = t;
						}



						methodDescriptor = info;
						execute = true;
						break;
					default: //others are normal API calls ...Opnum - 6 is there real Opnum. 0,1,2 and 3,4,5,6
						isStandardCall = true;
						Opnum = Opnum - 4; //adjust for only IDispatch(3,4,5,6) , IUnknown(0,1,2) will get adjusted below.
						if (JISystem.Logger.isLoggable(Level.INFO)) {
							JISystem.Logger.info("Standard call came: Opnum is " + Opnum);
						}

					break;
				}
			}

			if (isStandardCall) {
				methodDescriptor = interfaceDefinitionOfClass.GetMethodDescriptor(Opnum - 3); //adjust for IUnknown
				if (methodDescriptor == null) {
					throw new JIException(JIErrorCodes.RPC_S_PROCNUM_OUT_OF_RANGE);
				}
				methodDescriptor.ParameterObject.Session = Session_Renamed;
				@params = methodDescriptor.ParameterObject.Read(ndr);
				execute = true;
			}

			if (execute) {
				//JILocalInterfaceDefinition interfaceDefinitionOfCall = interfaceDefinition;
				Type calleeClazz = interfaceDefinitionOfClass.Instance == null ? interfaceDefinitionOfClass.Clazz : interfaceDefinitionOfClass.Instance.GetType();
				Method method = null;
				try {
					if (JISystem.Logger.isLoggable(Level.INFO)) {
						JISystem.Logger.info("methodDescriptor: " + methodDescriptor.MethodName);
					}
					method = calleeClazz.getDeclaredMethod(methodDescriptor.MethodName,methodDescriptor.InparametersAsClass);
					object calleeInstance = interfaceDefinitionOfClass.Instance == null ? calleeClazz.newInstance() : interfaceDefinitionOfClass.Instance;
					if (JISystem.Logger.isLoggable(Level.INFO)) {
						JISystem.Logger.info("Call Back Method to be executed: " + method + " , to be executed on " + calleeInstance);
					}
					object result = method.invoke(calleeInstance,@params);

					if (result == null) {
						retVal = null;
					}
					else {
					if (!(result is object[])) {
						retVal = new object[1];
						((object[])retVal)[0] = result;
					}
					else {
						retVal = result;
					}
					}


				}
				catch (System.ArgumentException e) {
					JISystem.Logger.throwing("JILocalCoClass","invokeMethod",e);
					throw new JIException(JIErrorCodes.E_INVALIDARG,e);
				}
				catch (IllegalAccessException e) {
					JISystem.Logger.throwing("JILocalCoClass","invokeMethod",e);
					throw new JIException(JIErrorCodes.ERROR_ACCESS_DENIED,e);
				}
				catch (InvocationTargetException e) {
					JISystem.Logger.throwing("JILocalCoClass","invokeMethod",e);
					throw new JIException(JIErrorCodes.E_UNEXPECTED,e);
				}
				catch (SecurityException e) {
					JISystem.Logger.throwing("JILocalCoClass","invokeMethod",e);
					throw new JIException(JIErrorCodes.ERROR_ACCESS_DENIED,e);
				}
				catch (NoSuchMethodException e) {
					JISystem.Logger.throwing("JILocalCoClass","invokeMethod",e);
					throw new JIException(JIErrorCodes.RPC_S_PROCNUM_OUT_OF_RANGE,e);
				}
				catch (InstantiationException e) {
					JISystem.Logger.throwing("JILocalCoClass","invokeMethod",e);
					throw new JIException(JIErrorCodes.E_UNEXPECTED,e);
				}

			}

			return (object[])retVal;
		}


		/// <summary>
		///Returns the primary interfaceDefinition. <br>
		/// 
		/// @return </summary>
		/// <seealso cref= #JILocalCoClass(JILocalInterfaceDefinition, Class) </seealso>
		/// <seealso cref= #JILocalCoClass(JILocalInterfaceDefinition, Object) </seealso>
		public JILocalInterfaceDefinition InterfaceDefinition {
			get {
				return InterfaceDefinition_Renamed;
			}
		}

		/// <summary>
		/// @exclude
		/// </summary>
		public override bool Equals(object target) {
			if (target == null || !(target is JILocalCoClass)) {
				return false;
			}

			return Identifier == ((JILocalCoClass)target).Identifier;
		}
		/// <summary>
		/// @exclude
		/// </summary>
		public override int GetHashCode() {
			return Identifier;
		}

		/// <summary>
		///Returns the interface definition based on the IID of the interface.
		/// </summary>
		/// <returns> <code>null</code> if no interface definition matching the <code>IID</code> has been found. </returns>
		public JILocalInterfaceDefinition GetInterfaceDefinition(string IID) {
			return (JILocalInterfaceDefinition)MapOfIIDsToInterfaceDefinitions.GetValueOrNull(IID.ToUpper());
		}

		/// <summary>
		/// @exclude </summary>
		/// <param name="IPID">
		/// @return </param>
		 public JILocalInterfaceDefinition GetInterfaceDefinitionFromIPID(string IPID) {
			return (JILocalInterfaceDefinition)MapOfIIDsToInterfaceDefinitions.GetValueOrNull((string)IpidVsIID.GetValueOrNull(IPID.ToUpper()));
		 }
		/// <summary>
		/// @exclude
		/// </summary>
		 public string GetIpidFromIID(string uniqueIID) {
			return (string)IIDvsIpid.GetValueOrNull(uniqueIID.ToUpper());
		 }

		/// 
		/// <param name="uniqueIID">
		/// @return </param>
		 public string GetIIDFromIpid(string ipid) {
			return (string)IpidVsIID.GetValueOrNull(ipid.ToUpper());
		 }

		/// <summary>
		/// <para> Returns <code>true</code> if the primary interface definition represents a real <code>IID</code> .
		/// 
		/// @return
		/// </para>
		/// </summary>
	//	 The bind-auth3 and all are then all done as per this <code>IID</code> and not IUnknown.
		public bool CoClassUnderRealIID {
			get {
				return RealIID;
			}
		}

		/// <summary>
		/// Associate the Session with this CoClass. Called by the framework.
		/// @exclude </summary>
		/// <param name="session"> </param>
		public JISession Session {
			set {
				this.Session_Renamed = value;
			}
			get {
				return Session_Renamed;
			}
		}


		public IList SupportedInterfaces {
			get {
				return ListOfSupportedInterfaces;
			}
		}
	}

}