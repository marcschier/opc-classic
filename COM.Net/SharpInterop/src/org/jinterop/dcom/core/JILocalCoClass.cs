// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.core {
    using SharpCifs.Dcerpc.Ndr;
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.impls;
    using rpc.core;
    using Serilog;
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security;

    /// <summary>
    /// Represents a Java <code>COCLASS</code>. 
    /// Please refer to MSInternetExplorer, Test_ITestServer2_Impl, SampleTestServer
    /// and MSShell examples for more details on how to use this class.
    /// </summary>
    [Serializable]
    public sealed class JILocalCoClass {

        /// <summary>
        /// Creates a local class instance. The framework will try to create a instance of the <code>clazz</code>
        /// using <code>Class.newInstance</code>. Make sure that <code>clazz</code> has a visible <code>null</code>
        /// constructor.
        /// </summary>
        /// <param name="interfaceDefinition"> implementing structurally the definition of the COM callback interface. 
        /// </param>
        /// <param name="clazz"> <code>class</code> to instantiate for serving requests from COM client. Must implement
        /// the <code>interfaceDefinition</code> fully. </param>
        /// <exception cref="ArgumentException"> if <code>interfaceDefinition</code> or 
        /// <code>clazz</code> are <code>null</code>. </exception>
        public JILocalCoClass(JILocalInterfaceDefinition interfaceDefinition, Type clazz) {
            if (interfaceDefinition == null || clazz == null) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_COM_RUNTIME_INVALID_CONTAINER_INFO));
            }
            _identifier = clazz.GetHashCode() ^ new object().GetHashCode() ^ _randomGen.Next();
            init(interfaceDefinition, clazz, null, false);
        }

        /// <summary>
        /// Refer <seealso cref="JILocalCoClass(JILocalInterfaceDefinition, Type)"/>.
        /// </summary>
        /// <param name="interfaceDefinition"> implementing structurally the definition of the COM callback interface. </param>
        /// <param name="clazz"> <code>class</code> to instantiate for serving requests from COM client. Must implement
        /// the <code>interfaceDefinition</code> fully. </param>
        /// <param name="useInterfaceDefinitionIID"> <code>true</code> if the <code>IID</code> of <code>interfaceDefinition</code>
        /// should be used as to create the local COM Object. Use this when a reference other than <code>IUnknown*</code> is required.
        /// For all <seealso cref="JIObjectFactory.attachEventHandler(IJIComObject, string, IJIComObject)"/> operations this should be set to
        /// <code>false</code> since the <code>IConnectionPoint::Advise</code> method takes in a <code>IUnknown*</code> reference. </param>
        /// <exception cref="ArgumentException"> if <code>interfaceDefinition</code> or <code>clazz</code> are <code>null</code>. </exception>
        public JILocalCoClass(JILocalInterfaceDefinition interfaceDefinition, Type clazz, bool useInterfaceDefinitionIID) {
            if (interfaceDefinition == null || clazz == null) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_COM_RUNTIME_INVALID_CONTAINER_INFO));
            }
            _identifier = clazz.GetHashCode() ^ new object().GetHashCode() ^ _randomGen.Next();
            init(interfaceDefinition, clazz, null, useInterfaceDefinitionIID);
        }

        /// <summary>
        ///Creates a local class instance.
        /// </summary>
        /// <param name="interfaceDefinition"> implementing structurally the definition of the COM callback interface. </param>
        /// <param name="instance"> instance for serving requests from COM client. Must implement
        /// the <code>interfaceDefinition</code> fully. </param>
        /// <exception cref="ArgumentException"> if <code>interfaceDefinition</code> or <code>instance</code> 
        /// are <code>null</code>. </exception>
        public JILocalCoClass(JILocalInterfaceDefinition interfaceDefinition, object instance) {
            if (interfaceDefinition == null || instance == null) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_COM_RUNTIME_INVALID_CONTAINER_INFO));
            }
            _identifier = instance.GetHashCode() ^ new object().GetHashCode() ^ _randomGen.Next();
            init(interfaceDefinition, null, instance, false);
        }

        /// <summary>
        ///Creates a local class instance.
        /// </summary>
        /// <param name="interfaceDefinition"> implementing structurally the definition of the COM callback interface. </param>
        /// <param name="instance"> instance for serving requests from COM client. Must implement
        /// the <code>interfaceDefinition</code> fully. </param>
        /// <param name="useInterfaceDefinitionIID"> <code>true</code> if the <code>IID</code> of <code>interfaceDefinition</code>
        /// should be used as to create the local COM Object. Use this when a reference other than <code>IUnknown*</code> is required.
        /// For all <seealso cref="JIObjectFactory.attachEventHandler(IJIComObject, string, IJIComObject)"/> operations this should be set to
        /// <code>false</code> since the <code>IConnectionPoint::Advise</code> method takes in a <code>IUnknown*</code> reference. </param>
        /// <exception cref="ArgumentException"> if <code>interfaceDefinition</code> or <code>instance</code> are <code>null</code>. </exception>
        public JILocalCoClass(JILocalInterfaceDefinition interfaceDefinition, object instance, bool useInterfaceDefinitionIID) {
            if (interfaceDefinition == null || instance == null) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_COM_RUNTIME_INVALID_CONTAINER_INFO));
            }
            _identifier = instance.GetHashCode() ^ new object().GetHashCode() ^ _randomGen.Next();
            init(interfaceDefinition, null, instance, useInterfaceDefinitionIID);
        }


        private void init(JILocalInterfaceDefinition interfaceDefinition, Type clazz, object instance, bool realIID) {
            _listOfSupportedInterfaces.Add(IID_IDispatch.ToUpper()); //IDispatch
            _listOfSupportedInterfaces.Add("00000131-0000-0000-C000-000000000046"); //IRemUnknown
            InterfaceDefinition = interfaceDefinition;
            interfaceDefinition.clazz = clazz;
            interfaceDefinition.instance = instance;
            _listOfSupportedInterfaces.Add(interfaceDefinition.InterfaceIdentifier.ToUpper());
            mapOfIIDsToInterfaceDefinitions[interfaceDefinition.InterfaceIdentifier.ToUpper()] = interfaceDefinition;
            CoClassUnderRealIID = realIID;
        }


        /// <summary>
        ///Sets the interface identifiers (<code>IID</code>s) of the event interfaces this class would support. This in case the same
        /// <code>clazz</code> or <code>instance</code> is implementing more than one <code>IID</code>.
        /// </summary>
        /// <seealso cref="JILocalCoClass(JILocalInterfaceDefinition, Type)"> </seealso>
        /// <seealso cref="JILocalCoClass(JILocalInterfaceDefinition, object)"> </seealso>
        public IList SupportedEventInterfaces {
            set {
                if (value != null) {
                    for (var i = 0; i < value.Count; i++) {
                        var s = ((string)value[i]).ToUpper();
                        _listOfSupportedInterfaces.Add(s);
                        _listOfSupportedEventInterfaces.Add(s);
                        mapOfIIDsToInterfaceDefinitions[s] = InterfaceDefinition;
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
        /// <exception cref="ArgumentException"> if <code>interfaceDefinition</code> or <code>instance</code> are <code>null</code>. </exception>
        public void addInterfaceDefinition(JILocalInterfaceDefinition interfaceDefinition, object instance) {
            if (interfaceDefinition == null || instance == null) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_COM_RUNTIME_INVALID_CONTAINER_INFO));
            }
            interfaceDefinition.instance = instance;
            var s = interfaceDefinition.InterfaceIdentifier.ToUpper();
            _listOfSupportedInterfaces.Add(s);
            _listOfSupportedEventInterfaces.Add(s);
            mapOfIIDsToInterfaceDefinitions[s] = interfaceDefinition;
        }

        /// <summary>
        /// Add another interface definition and it's class. Make sure that this class has a default constructor,
        /// so that instantiation using <i>reflection</i> can take place.
        /// </summary>
        /// <param name="interfaceDefinition"> implementing structurally the definition of the COM callback interface. </param>
        /// <param name="clazz"> instance for serving requests from COM client. Must implement
        /// the <code>interfaceDefinition</code> fully. </param>
        /// <exception cref="ArgumentException"> if <code>interfaceDefinition</code> or <code>clazz</code> are <code>null</code>. </exception>
        public void addInterfaceDefinition(JILocalInterfaceDefinition interfaceDefinition, Type clazz) {
            if (interfaceDefinition == null || clazz == null) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_COM_RUNTIME_INVALID_CONTAINER_INFO));
            }
            interfaceDefinition.clazz = clazz;
            var s = interfaceDefinition.InterfaceIdentifier.ToUpper();
            _listOfSupportedInterfaces.Add(s);
            _listOfSupportedEventInterfaces.Add(s);
            mapOfIIDsToInterfaceDefinitions[s] = interfaceDefinition;
        }

        /// <summary>
        /// Returns the instance representing the interface definition. 
        /// </summary>
        /// <seealso cref="JILocalCoClass(JILocalInterfaceDefinition, Object)"></seealso>
        public object ServerInstance => InterfaceDefinition.instance;

        /// <summary>
        /// Returns the actual class representing the interface definition. 
        /// </summary>
        /// <seealso cref="JILocalCoClass(JILocalInterfaceDefinition, Type)"></seealso>
        public Type ServerClass => InterfaceDefinition.clazz;

        /// <summary>
        /// called from com runtime.
        /// </summary>
        internal byte[] ObjectId { set; get; } = null;

        /// <summary>
        /// Interface pointer
        /// </summary>
        internal JIInterfacePointer AssociatedInterfacePointer {
            set {
                AlreadyExported = true;
                _interfacePointer = new WeakReference(value);
                var ipid = value.IPID.ToUpper();
                var iid = value.IID.ToUpper();
                IIDvsIpid[iid] = ipid;
                ipidVsIID[ipid] = iid;
            }
        }

        /// <summary>
        /// Associated reference alive
        /// </summary>
        internal bool AssociatedReferenceAlive => 
            _interfacePointer == null ? false : (_interfacePointer.Target == null ? false : true);

        /// <summary>
        /// Already exported
        /// </summary>
        internal bool AlreadyExported { get; private set; }

        /// <summary>
        /// Iid present
        /// </summary>
        /// <param name="iid"></param>
        /// <returns></returns>
        internal bool isPresent(string iid) {
            iid = iid.ToUpper();
            return _listOfSupportedInterfaces.Contains(iid);
        }

        /// <summary>
        /// advances the index...it cannot be reversed.
        /// </summary>
        /// <param name="uniqueIID"> </param>
        /// <param name="IPID"> </param>
        internal bool exportInstance(string uniqueIID, string IPID) {
            lock (this) {
                //Object retval = null;
                IPID = IPID.ToUpper();

                if (!isPresent(uniqueIID)) //not supported IID.
                {
                    return false;
                }

                IIDvsIpid[uniqueIID.ToUpper()] = IPID;
                ipidVsIID[IPID] = uniqueIID.ToUpper();
                return true;
            }
        }

        /// <summary>
        /// Returns the interface identifier of this COCLASS. 
        /// </summary>
        /// <seealso cref="JILocalCoClass(JILocalInterfaceDefinition, Type)"> </seealso>
        /// <seealso cref="JILocalCoClass(JILocalInterfaceDefinition, object)"> </seealso>
        /// <seealso cref="JILocalInterfaceDefinition.InterfaceIdentifier"> </seealso>
        public string CoClassIID => InterfaceDefinition.InterfaceIdentifier;

        /// <summary>
        /// Invoke method - This will invoke the API via reflection and return the
        /// results of the call back to the actual COM object. 
        /// This API is to be invoked via the RemUnknown Object
        /// </summary>
        /// <param name="IPID"> </param>
        /// <param name="Opnum"> </param>
        /// <param name="ndr"></param>
        /// <exception cref="JIException"> </exception>
        internal object[] invokeMethod(string IPID, int Opnum, NdrCodec ndr) {
            IPID = IPID.ToUpper();
            //somehow identify the method from the Opnum
            //this will come from the IDL.

            object retVal = null; //will be an array.

            var iid = (string)ipidVsIID[IPID];
            if (iid == null) {
                throw new JIException(JIErrorCodes.RPC_E_INVALID_OBJECT);
            }

            var interfaceDefinitionOfClass = (JILocalInterfaceDefinition)mapOfIIDsToInterfaceDefinitions[iid];
            interfaceDefinitionOfClass = interfaceDefinitionOfClass ?? InterfaceDefinition;

            JILocalMethodDescriptor methodDescriptor = null;
            var execute = false;
            object[] @params = null;

            //that means the calls will come as IUnknown + IDispatch op numbers...0,1,2 & 3,4,5,6
            //from 7th (inclusive) onwards are the actual COM servers calls
            //now check for dispinterface and take a call...
            //if dispinterface is supported then all calls will come with base of 6 {0,1,2 & 3,4,5,6}
            //i.e 6th will be invoke and 7th(inclusive) onwards will be standard api calls.
            //if not supported than it will be base 2 {0,1,2} i.e real method calls will start from 3(inclusive) onwards.
            var isStandardCall = true;
            if (InterfaceDefinition.DispInterface) {
                isStandardCall = false;
                switch (Opnum) {
                    case 3: //getTypeInfoCount
                            //not supported
                        retVal = new object[1];
                        ((object[])retVal)[0] = 0; //not supported
                        break;
                    case 4: //getTypeInfo
                        throw new JIException(JIErrorCodes.E_NOTIMPL);
                    case 5: //getIDOfNames

                        var paramObject = new JILocalParamsDescriptor();

                        paramObject.addInParamAsType(typeof(UUID), JIFlags.FLAG_NULL);
                        paramObject.addInParamAsObject(new JIArray(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR), null, 1, true), JIFlags.FLAG_NULL);
                        paramObject.addInParamAsType(typeof(int?), JIFlags.FLAG_NULL);
                        paramObject.addInParamAsType(typeof(int?), JIFlags.FLAG_NULL);

                        //now read and then send the result back.
                        var array = (JIArray)paramObject.read(ndr)[1];

                        var arrayObj = (object[])array.ArrayInstance;
                        var dispIds = new int?[arrayObj.Length];
                        //get the first member of the Array , which is the APINAME and send the retVal with it's dispId
                        var apiName = (JIString)arrayObj[0];
                        var info = interfaceDefinitionOfClass.getMethodDescriptor(apiName.String);
                        if (info == null) {
                            dispIds[0] = JIErrorCodes.DISP_E_UNKNOWNNAME;
                        }
                        else {
                            dispIds[0] = info.MethodNum;
                        }

                        //rest are all 0,1,2...parameters
                        for (var i = 1; i < arrayObj.Length; i++) {
                            dispIds[i] = i - 1;
                        }

                        var results = new JIArray(dispIds);

                        retVal = new object[1];
                        ((object[])retVal)[0] = results;

                        break;
                    case 6: //invoke of IDispatch

                        paramObject = new JILocalParamsDescriptor {
                            Session = Session
                        };
                        paramObject.addInParamAsType(typeof(int?), JIFlags.FLAG_NULL);
                        paramObject.addInParamAsType(typeof(UUID), JIFlags.FLAG_NULL);
                        paramObject.addInParamAsType(typeof(int?), JIFlags.FLAG_NULL);
                        paramObject.addInParamAsType(typeof(int?), JIFlags.FLAG_NULL);

                        var dispParams = new JIStruct();
                        dispParams.addMember(new JIPointer(new JIArray(typeof(JIVariant), null, 1, true)));
                        dispParams.addMember(new JIPointer(new JIArray(typeof(int?), null, 1, true)));
                        dispParams.addMember(typeof(int?));
                        dispParams.addMember(typeof(int?));

                        paramObject.addInParamAsObject(dispParams, JIFlags.FLAG_REPRESENTATION_IDISPATCH_INVOKE);
                        paramObject.addInParamAsType(typeof(int?), JIFlags.FLAG_NULL);
                        paramObject.addInParamAsObject(new JIArray(typeof(int?), null, 1, true), JIFlags.FLAG_NULL);
                        paramObject.addInParamAsObject(new JIArray(typeof(JIVariant), null, 1, true), JIFlags.FLAG_NULL);

                        var retresults = paramObject.read(ndr);
                        //named params not supported
                        var dispId = (int)(int?)retresults[0];

                        info = interfaceDefinitionOfClass.getMethodDescriptorForDispId(dispId);
                        if (info == null) {
                            Log.Logger.Error("MethodDescriptor not found for DispId :- " + dispId);
                            throw new JIException(JIErrorCodes.DISP_E_MEMBERNOTFOUND);
                        }

                        dispParams = (JIStruct)retresults[4];
                        var ptrToParamsArray = (JIPointer)dispParams.getMember(0);

                        @params = new object[0];
                        if (!ptrToParamsArray.Null) {
                            //form the real array
                            array = (JIArray)ptrToParamsArray.getReferent();
                            var variants = (object[])array.ArrayInstance;
                            @params = new object[variants.Length];
                            for (var i = 0; i < variants.Length; i++) {
                                @params[i] = ((JIVariant)variants[i]).Object;
                            }
                        }

                        if ((int)(int?)retresults[5] != 0) {
                            //now replace the params at index from the index array.
                            array = (JIArray)retresults[6];
                            var indexs = (int?[])array.ArrayInstance;
                            array = (JIArray)retresults[7];
                            var variants = (JIVariant[])array.ArrayInstance;
                            for (var i = 0; i < indexs.Length; i++) {
                                @params[(int)indexs[i]] = variants[i];
                            }
                        }

                        //now to reverse this array of params.
                        var halflength = @params.Length / 2;
                        for (var i = 0; i < halflength; i++) {
                            var t = @params[i];
                            @params[i] = @params[@params.Length - 1 - i];
                            @params[@params.Length - 1 - i] = t;
                        }

                        methodDescriptor = info;
                        execute = true;
                        break;
                    default: //others are normal API calls ...Opnum - 6 is there real Opnum. 0,1,2 and 3,4,5,6
                        isStandardCall = true;
                        Opnum = Opnum - 4; //adjust for only IDispatch(3,4,5,6) , IUnknown(0,1,2) will get adjusted below.
                        Log.Logger.Information("Standard call came: Opnum is " + Opnum);

                        break;
                }
            }

            if (isStandardCall) {
                methodDescriptor = interfaceDefinitionOfClass.getMethodDescriptor(Opnum - 3); //adjust for IUnknown
                if (methodDescriptor == null) {
                    throw new JIException(JIErrorCodes.RPC_S_PROCNUM_OUT_OF_RANGE);
                }
                methodDescriptor.ParameterObject.Session = Session;
                @params = methodDescriptor.ParameterObject.read(ndr);
                execute = true;
            }

            if (execute) {
                //JILocalInterfaceDefinition interfaceDefinitionOfCall = interfaceDefinition;
                var calleeClazz = interfaceDefinitionOfClass.instance == null ? interfaceDefinitionOfClass.clazz : interfaceDefinitionOfClass.instance.GetType();
                Method method = null;
                try {
                    Log.Logger.Information("methodDescriptor: " + methodDescriptor.MethodName);
                    method = calleeClazz.getDeclaredMethod(methodDescriptor.MethodName, methodDescriptor.InparametersAsClass);
                    object calleeInstance = interfaceDefinitionOfClass.instance ?? calleeClazz.newInstance();
                    Log.Logger.Information("Call Back Method to be executed: " + method + " , to be executed on " + calleeInstance);
                    object result = method.invoke(calleeInstance, @params);

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
                catch (ArgumentException e) {
                    Log.Logger.Error(e, "JILocalCoClass invokeMethod");
                    throw new JIException(JIErrorCodes.E_INVALIDARG);
                }
                catch (MethodAccessException e) {
                    Log.Logger.Error(e, "JILocalCoClass invokeMethod", e);
                    throw new JIException(JIErrorCodes.ERROR_ACCESS_DENIED);
                }
                catch (TargetInvocationException e) {
                    Log.Logger.Error(e, "JILocalCoClass invokeMethod");
                    throw new JIException(JIErrorCodes.E_UNEXPECTED, e);
                }
                catch (SecurityException e) {
                    Log.Logger.Error(e, "JILocalCoClass invokeMethod");
                    throw new JIException(JIErrorCodes.ERROR_ACCESS_DENIED, e);
                }
                catch (MissingMethodException e) {
                    Log.Logger.Error(e, "JILocalCoClass invokeMethod");
                    throw new JIException(JIErrorCodes.RPC_S_PROCNUM_OUT_OF_RANGE);
                }
                catch (InstantiationException e) {
                    Log.Logger.Error(e, "JILocalCoClass invokeMethod");
                    throw new JIException(JIErrorCodes.E_UNEXPECTED, e);
                }
            }
            return (object[])retVal;
        }

        /// <summary>
        /// Returns the primary interfaceDefinition. 
        /// </summary>
        /// <seealso cref="JILocalCoClass(JILocalInterfaceDefinition, Type)"></seealso>
        /// <seealso cref="JILocalCoClass(JILocalInterfaceDefinition, object)"></seealso>
        /// <returns>primary interfaceDefinition. </returns>
        public JILocalInterfaceDefinition InterfaceDefinition { get; private set; } = null;

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            if (!(obj is JILocalCoClass other)) {
                return false;
            }
            return _identifier == other._identifier;
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            return _identifier;
        }

        /// <summary>
        /// Returns the interface definition based on the IID of the interface.
        /// </summary>
        /// <returns> <code>null</code> if no interface definition matching the <code>IID</code>
        /// has been found. </returns>
        public JILocalInterfaceDefinition getInterfaceDefinition(string IID) {
            return (JILocalInterfaceDefinition)mapOfIIDsToInterfaceDefinitions[IID.ToUpper()];
        }

        /// <summary>
        /// Get interface definition from ipid
        /// </summary>
        /// <param name="IPID"></param>
        /// <returns></returns>
        internal JILocalInterfaceDefinition getInterfaceDefinitionFromIPID(string IPID) {
            return (JILocalInterfaceDefinition)mapOfIIDsToInterfaceDefinitions[(string)ipidVsIID[IPID.ToUpper()]];
        }

        /// <summary>
        /// Get ipid from iid helper
        /// </summary>
        /// <param name="uniqueIID"></param>
        /// <returns></returns>
        internal string getIpidFromIID(string uniqueIID) {
            return (string)IIDvsIpid[uniqueIID.ToUpper()];
        }

        /// <summary>
        /// Get iid from ipid helper
        /// </summary>
        /// <param name="ipid"></param>
        /// <returns></returns>
        internal string getIIDFromIpid(string ipid) {
            return (string)ipidVsIID[ipid.ToUpper()];
        }

        /// <summary>
        /// Returns <code>true</code> if the primary interface definition represents a real <code>IID</code> .
        /// The bind-auth3 and all are then all done as per this <code>IID</code> and not IUnknown.
        /// </summary>
        public bool CoClassUnderRealIID { get; private set; } = false;

        /// <summary>
        /// Associate the Session with this CoClass. Called by the framework.
        /// </summary>
        internal JISession Session { set; get; }

        /// <summary>
        /// Supported interfaces
        /// </summary>
        internal IList SupportedInterfaces => _listOfSupportedInterfaces;


        private const string IID_IDispatch = "00020400-0000-0000-c000-000000000046";
        private static Random _randomGen = new Random();
        private readonly int _identifier;
        private WeakReference _interfacePointer;
        private ArrayList _listOfSupportedInterfaces = new ArrayList();
        private ArrayList _listOfSupportedEventInterfaces = new ArrayList();
        private readonly Hashtable mapOfIIDsToInterfaceDefinitions = new Hashtable();
        // will use this to identify which IID is being talked about
        // if it is IDispatch then delegate to it's invoke.
        private readonly IDictionary ipidVsIID = new Hashtable();
        // will use this to identify which IPID is being talked about
        private readonly IDictionary IIDvsIpid = new Hashtable();
    }
}