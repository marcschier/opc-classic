// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using SharpCifs.Util.Sharpen;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Globalization;
using System.Linq;
using System.Threading;

#pragma warning disable MA0051 // Legacy DCOM protocol methods are intentionally kept intact during analyzer cleanup.

namespace Opc.Classic.Dcom.Core;
/// <summary>
/// Represents a local <code>COCLASS</code>.
/// Please refer to MSInternetExplorer, Test_ITestServer2_Impl, SampleTestServer
/// and MSShell examples for more details on how to use this class.
/// </summary>
[Serializable]
public sealed class LocalCoClass
{

    /// <summary>
    /// Returns <code>true</code> if the primary interface definition 
    /// represents a real <code>IID</code>.
    /// The bind-auth3 and all are then all done as per this
    /// <code>IID</code> and not IUnknown.
    /// </summary>
    public bool ICoClassUnderRealIID { get; private set; }

    /// <summary>
    /// Associate the Session with this CoClass. Called by the framework.
    /// </summary>
    internal Session Session { set; get; }

    /// <summary>
    /// Supported interfaces
    /// </summary>
    internal List<string> SupportedInterfaces { get; } = new List<string>();

    /// <summary>
    /// Sets the interface identifiers (<code>IID</code>s) of the event
    /// interfaces this class would support. This in case the same
    /// <code>clazz</code> or <code>instance</code> is implementing more
    /// than one <code>IID</code>.
    /// </summary>
    /// <seealso cref="LocalCoClass(LocalInterfaceDefinition, Type)"> </seealso>
    /// <seealso cref="LocalCoClass(LocalInterfaceDefinition, object)"> </seealso>
    public IList<string> SupportedEventInterfaces
    {
        set
        {
            if (value != null)
            {
                for (var i = 0; i < value.Count; i++)
                {
                    var s = value[i].ToUpper(CultureInfo.InvariantCulture);
                    SupportedInterfaces.Add(s);
                    _listOfSupportedEventInterfaces.Add(s);
                    _mapOfIIDsToInterfaceDefinitions.AddOrUpdate(s, InterfaceDefinition);
                }
            }
        }
    }

    /// <summary>
    /// Returns the instance representing the interface definition.
    /// </summary>
    /// <seealso cref="LocalCoClass(LocalInterfaceDefinition, object)"></seealso>
    public object ServerInstance => InterfaceDefinition.Instance;

    /// <summary>
    /// Returns the actual class representing the interface definition.
    /// </summary>
    /// <seealso cref="LocalCoClass(LocalInterfaceDefinition, Type)"></seealso>
    public Type ServerClass => InterfaceDefinition.Type;

    /// <summary>
    /// called from com runtime.
    /// </summary>
    internal byte[] ObjectId { set; get; }

    /// <summary>
    /// Interface pointer
    /// </summary>
    internal InterfacePointer AssociatedInterfacePointer
    {
        set
        {
            AlreadyExported = true;
            _interfacePointer = new WeakReference(value);
            var ipid = value.IPID.ToUpper(CultureInfo.InvariantCulture);
            var iid = value.IID.ToUpper(CultureInfo.InvariantCulture);
            _iIDvsIpid.AddOrUpdate(iid, ipid);
            _ipidVsIID.AddOrUpdate(ipid, iid);
        }
    }

    /// <summary>
    /// Associated reference alive
    /// </summary>
    internal bool AssociatedReferenceAlive =>
        _interfacePointer != null && (_interfacePointer.Target != null);

    /// <summary>
    /// Already exported
    /// </summary>
    internal bool AlreadyExported { get; private set; }

    /// <summary>
    /// Returns the interface identifier of this COCLASS.
    /// </summary>
    /// <seealso cref="LocalCoClass(LocalInterfaceDefinition, Type)"> </seealso>
    /// <seealso cref="LocalCoClass(LocalInterfaceDefinition, object)"> </seealso>
    /// <seealso cref="LocalInterfaceDefinition.InterfaceIdentifier"> </seealso>
    public string CoClassIID => InterfaceDefinition.InterfaceIdentifier;

    /// <summary>
    /// Returns the primary interfaceDefinition.
    /// </summary>
    /// <seealso cref="LocalCoClass(LocalInterfaceDefinition, Type)"></seealso>
    /// <seealso cref="LocalCoClass(LocalInterfaceDefinition, object)"></seealso>
    /// <returns>primary interfaceDefinition. </returns>
    public LocalInterfaceDefinition InterfaceDefinition { get; private set; }


    /// <summary>
    /// Creates a local class instance. The framework will try to create
    /// a instance of the <code>type</code>
    /// using <code>Class.newInstance</code>. Make sure that <code>type</code>
    /// has a visible <code>null</code> constructor.
    /// </summary>
    /// <param name="interfaceDefinition"> implementing structurally the
    /// definition of the COM callback interface.
    /// </param>
    /// <param name="type"> <code>class</code> to instantiate for serving
    /// requests from COM client. Must implement the <code>interfaceDefinition</code>
    /// fully. </param>
    /// <exception cref="ArgumentException"> if <code>interfaceDefinition</code> or
    /// <code>type</code> are <code>null</code>.</exception>
    public LocalCoClass(LocalInterfaceDefinition interfaceDefinition, Type type)
    {
        if (interfaceDefinition == null || type == null)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_COM_RUNTIME_INVALID_CONTAINER_INFO), nameof(interfaceDefinition));
        }
        _identifier = type.GetHashCode() ^ new object().GetHashCode() ^ kRandomGen.Next();
        Init(interfaceDefinition, type, null, false);
    }

    /// <summary>
    /// Refer <seealso cref="LocalCoClass(LocalInterfaceDefinition, Type)"/>.
    /// </summary>
    /// <param name="interfaceDefinition"> implementing structurally
    /// the definition of the COM callback interface. </param>
    /// <param name="type"> <code>class</code> to instantiate for serving
    /// requests from COM client. Must implement
    /// the <code>interfaceDefinition</code> fully. </param>
    /// <param name="useInterfaceDefinitionIID"> <code>true</code> if
    /// the <code>IID</code> of <code>interfaceDefinition</code>
    /// should be used as to create the local COM Object. Use this when a
    /// reference other than <code>IUnknown*</code> is required.
    /// For all <seealso cref="ObjectFactory.AttachEventHandler(IComObject, string, IComObject)"/>
    /// operations this should be set to <code>false</code> since the
    /// <code>IConnectionPoint::Advise</code> method takes in a
    /// <code>IUnknown*</code> reference. </param>
    /// <exception cref="ArgumentException"> if <code>interfaceDefinition</code>
    /// or <code>clazz</code> are <code>null</code>. </exception>
    public LocalCoClass(LocalInterfaceDefinition interfaceDefinition, Type type,
        bool useInterfaceDefinitionIID)
    {
        if (interfaceDefinition == null || type == null)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_COM_RUNTIME_INVALID_CONTAINER_INFO), nameof(interfaceDefinition));
        }
        _identifier = type.GetHashCode() ^ new object().GetHashCode() ^ kRandomGen.Next();
        Init(interfaceDefinition, type, null, useInterfaceDefinitionIID);
    }

    /// <summary>
    ///Creates a local class instance.
    /// </summary>
    /// <param name="interfaceDefinition"> implementing structurally the
    /// definition of the COM callback interface. </param>
    /// <param name="instance"> instance for serving requests from COM
    /// client. Must implement
    /// the <code>interfaceDefinition</code> fully. </param>
    /// <exception cref="ArgumentException"> if <code>interfaceDefinition</code>
    /// or <code>instance</code>
    /// are <code>null</code>. </exception>
    public LocalCoClass(LocalInterfaceDefinition interfaceDefinition,
        object instance)
    {
        if (interfaceDefinition == null || instance == null)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_COM_RUNTIME_INVALID_CONTAINER_INFO), nameof(interfaceDefinition));
        }
        _identifier = instance.GetHashCode() ^ new object().GetHashCode() ^ kRandomGen.Next();
        Init(interfaceDefinition, null, instance, false);
    }

    /// <summary>
    /// Creates a local class instance.
    /// </summary>
    /// <param name="interfaceDefinition"> implementing structurally
    /// the definition of the COM callback interface. </param>
    /// <param name="instance"> instance for serving requests from
    /// COM client. Must implement
    /// the <code>interfaceDefinition</code> fully. </param>
    /// <param name="useInterfaceDefinitionIID"> <code>true</code> if
    /// the <code>IID</code> of <code>interfaceDefinition</code>
    /// should be used as to create the local COM Object. Use this when
    /// a reference other than <code>IUnknown*</code> is required.
    /// For all <seealso cref="ObjectFactory.AttachEventHandler(IComObject, string, IComObject)"/>
    /// operations this should be set to <code>false</code> since the
    /// <code>IConnectionPoint::Advise</code> method takes in a
    /// <code>IUnknown*</code> reference. </param>
    /// <exception cref="ArgumentException"> if <code>interfaceDefinition</code>
    /// or <code>instance</code> are <code>null</code>. </exception>
    public LocalCoClass(LocalInterfaceDefinition interfaceDefinition,
        object instance, bool useInterfaceDefinitionIID)
    {
        if (interfaceDefinition == null || instance == null)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_COM_RUNTIME_INVALID_CONTAINER_INFO), nameof(interfaceDefinition));
        }
        _identifier = instance.GetHashCode() ^ new object().GetHashCode() ^ kRandomGen.Next();
        Init(interfaceDefinition, null, instance, useInterfaceDefinitionIID);
    }

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="interfaceDefinition"></param>
    /// <param name="type"></param>
    /// <param name="instance"></param>
    /// <param name="realIID"></param>
    private void Init(LocalInterfaceDefinition interfaceDefinition, Type type,
        object instance, bool realIID)
    {
        SupportedInterfaces.Add(Interfaces.IID_IDispatch);
        SupportedInterfaces.Add(Interfaces.IID_IRemUnknown);
        InterfaceDefinition = interfaceDefinition;
        interfaceDefinition.Type = type;
        interfaceDefinition.Instance = instance;
        SupportedInterfaces.Add(interfaceDefinition.InterfaceIdentifier.ToUpper(CultureInfo.InvariantCulture));
        _mapOfIIDsToInterfaceDefinitions.AddOrUpdate(
            interfaceDefinition.InterfaceIdentifier.ToUpper(CultureInfo.InvariantCulture), interfaceDefinition);
        ICoClassUnderRealIID = realIID;
    }

    /// <summary>
    /// Add another interface definition and it's supporting object
    /// instance.
    /// </summary>
    /// <param name="interfaceDefinition"> implementing structurally
    /// the definition of the COM callback interface. </param>
    /// <param name="instance"> instance for serving requests from
    /// COM client. Must implement
    /// the <code>interfaceDefinition</code> fully. </param>
    /// <exception cref="ArgumentException"> if <code>interfaceDefinition
    /// </code> or <code>instance</code> are <code>null</code>. </exception>
    public void AddInterfaceDefinition(LocalInterfaceDefinition interfaceDefinition,
        object instance)
    {
        if (interfaceDefinition == null || instance == null)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_COM_RUNTIME_INVALID_CONTAINER_INFO), nameof(interfaceDefinition));
        }
        interfaceDefinition.Instance = instance;
        var s = interfaceDefinition.InterfaceIdentifier.ToUpper(CultureInfo.InvariantCulture);
        SupportedInterfaces.Add(s);
        _listOfSupportedEventInterfaces.Add(s);
        _mapOfIIDsToInterfaceDefinitions.AddOrUpdate(s, interfaceDefinition);
    }

    /// <summary>
    /// Add another interface definition and it's class. Make sure
    /// that this class has a default constructor,
    /// so that instantiation using <i>reflection</i> can take place.
    /// </summary>
    /// <param name="interfaceDefinition"> implementing structurally
    /// the definition of the COM callback interface. </param>
    /// <param name="type"> instance for serving requests from COM
    /// client. Must implement
    /// the <code>interfaceDefinition</code> fully. </param>
    /// <exception cref="ArgumentException"> if <code>interfaceDefinition</code>
    /// or <code>clazz</code> are <code>null</code>. </exception>
    public void AddInterfaceDefinition(LocalInterfaceDefinition interfaceDefinition,
        Type type)
    {
        if (interfaceDefinition == null || type == null)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_COM_RUNTIME_INVALID_CONTAINER_INFO), nameof(interfaceDefinition));
        }
        interfaceDefinition.Type = type;
        var s = interfaceDefinition.InterfaceIdentifier.ToUpper(CultureInfo.InvariantCulture);
        SupportedInterfaces.Add(s);
        _listOfSupportedEventInterfaces.Add(s);
        _mapOfIIDsToInterfaceDefinitions.AddOrUpdate(s, interfaceDefinition);
    }

    /// <summary>
    /// Returns the interface definition based on the IID of the interface.
    /// </summary>
    /// <returns> <code>null</code> if no interface definition matching the <code>IID</code>
    /// has been found. </returns>
    public LocalInterfaceDefinition GetInterfaceDefinition(string IID) =>
        _mapOfIIDsToInterfaceDefinitions.GetOrDefault(IID.ToUpper(CultureInfo.InvariantCulture));

    /// <summary>
    /// Registers a generated dispatch table for an IID.
    /// </summary>
    internal void AddDispatchTable(string IID, IDispatchTable dispatchTable)
    {
        ArgumentNullException.ThrowIfNull(IID);
        ArgumentNullException.ThrowIfNull(dispatchTable);

        var key = IID.ToUpper(CultureInfo.InvariantCulture);
        _mapOfIIDsToDispatchTables.AddOrUpdate(key, dispatchTable);
        if (_mapOfIIDsToInterfaceDefinitions.TryGetValue(key, out var interfaceDefinition))
        {
            interfaceDefinition.DispatchTable = dispatchTable;
        }
    }

    /// <summary>
    /// Iid present
    /// </summary>
    /// <param name="iid"></param>
    /// <returns></returns>
    internal bool IsIIDPresent(string iid)
    {
        iid = iid.ToUpper(CultureInfo.InvariantCulture);
        return SupportedInterfaces.Contains(iid, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Get interface definition from ipid
    /// </summary>
    /// <param name="IPID"></param>
    /// <returns></returns>
    internal LocalInterfaceDefinition GetInterfaceDefinitionFromIPID(string IPID)
    {
        if (_ipidVsIID.TryGetValue(IPID.ToUpper(CultureInfo.InvariantCulture), out var iid))
        {
            return _mapOfIIDsToInterfaceDefinitions.GetOrDefault(iid);
        }
        throw new ArgumentException("Unknown IPID.", nameof(IPID));
    }

    /// <summary>
    /// Get ipid from iid helper
    /// </summary>
    /// <param name="uniqueIID"></param>
    /// <returns></returns>
    internal string GetIpidFromIID(string uniqueIID) =>
        _iIDvsIpid.GetOrDefault(uniqueIID.ToUpper(CultureInfo.InvariantCulture));

    /// <summary>
    /// Get iid from ipid helper
    /// </summary>
    /// <param name="ipid"></param>
    /// <returns></returns>
    internal string GetIIDFromIpid(string ipid) =>
        _ipidVsIID.GetOrDefault(ipid.ToUpper(CultureInfo.InvariantCulture));

    /// <summary>
    /// advances the index...it cannot be reversed.
    /// </summary>
    /// <param name="uniqueIID"> </param>
    /// <param name="IPID"> </param>
    internal bool ExportInstance(string uniqueIID, string IPID)
    {
        lock (_syncRoot)
        {
            // Object retval = null;
            IPID = IPID.ToUpper(CultureInfo.InvariantCulture);

            if (!IsIIDPresent(uniqueIID))
            {
                // not supported IID.
                return false;
            }

            _iIDvsIpid.AddOrUpdate(uniqueIID.ToUpper(CultureInfo.InvariantCulture), IPID);
            _ipidVsIID.AddOrUpdate(IPID, uniqueIID.ToUpper(CultureInfo.InvariantCulture));
            return true;
        }
    }

    /// <summary>
    /// Invoke method - This will invoke the API via reflection and return the
    /// results of the call back to the actual COM object.
    /// This API is to be invoked via the RemUnknown Object
    /// </summary>
    /// <param name="IPID"> </param>
    /// <param name="Opnum"> </param>
    /// <param name="ndr"></param>
    /// <exception cref="InteropException"> </exception>
    internal object[] InvokeMethod(string IPID, int Opnum, NdrCodec ndr)
    {
        IPID = IPID.ToUpper(CultureInfo.InvariantCulture);
        // somehow identify the method from the Opnum
        // this will come from the IDL.

        object retVal = null; // will be an array.

        var iid = _ipidVsIID.GetOrDefault(IPID);
        if (iid == null)
        {
            throw new InteropException(ErrorCode.RPC_E_INVALID_OBJECT);
        }

        var interfaceDefinitionOfClass = _mapOfIIDsToInterfaceDefinitions.GetOrDefault(iid);
        interfaceDefinitionOfClass = interfaceDefinitionOfClass ?? InterfaceDefinition;

        LocalMethodDescriptor methodDescriptor = null;
        var execute = false;
        object[] parameters = null;

        // that means the calls will come as IUnknown + IDispatch op numbers...0,1,2 & 3,4,5,6
        // from 7th (inclusive) onwards are the actual COM servers calls
        // now check for dispinterface and take a call...
        // if dispinterface is supported then all calls will come with base of 6 {0,1,2 & 3,4,5,6}
        // i.e 6th will be invoke and 7th(inclusive) onwards will be standard api calls.
        // if not supported than it will be base 2 {0,1,2} i.e real method calls will start from 3(inclusive) onwards.
        var isStandardCall = true;
        if (InterfaceDefinition.DispInterface)
        {
            isStandardCall = false;
            switch (Opnum)
            {
                case 3: // GetTypeInfoCount
                    // not supported
                    retVal = new object[1];
                    ((object[])retVal)[0] = 0; // not supported
                    break;
                case 4: // GetTypeInfo
                    throw new InteropException(ErrorCode.E_NOTIMPL);
                case 5: // GetIDOfNames
                    var paramObject = new LocalParamsDescriptor();
                    paramObject.AddInParamAsType(typeof(UUID));
                    paramObject.AddInParamAsObject(new ComArray(
                        new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR), null, 1, true));
                    paramObject.AddInParamAsType(typeof(int));
                    paramObject.AddInParamAsType(typeof(int));

                    // now read and then send the result back.
                    var array = (ComArray)paramObject.Read(ndr)[1];
                    var arrayObj = (object[])array.ArrayInstance;
                    var dispIds = new int[arrayObj.Length];
                    // get the first member of the Array, which is the APINAME and send the retVal with it's dispId
                    var apiName = (ComString)arrayObj[0];
                    var info = interfaceDefinitionOfClass.GetMethodDescriptor(apiName.String);
                    if (info == null)
                    {
                        dispIds[0] = unchecked((int)ErrorCode.DISP_E_UNKNOWNNAME);
                    }
                    else
                    {
                        dispIds[0] = info.MethodNum;
                    }

                    // rest are all 0,1,2...parameters
                    for (var i = 1; i < arrayObj.Length; i++)
                    {
                        dispIds[i] = i - 1;
                    }
                    var results = new ComArray(dispIds);
                    retVal = new object[1];
                    ((object[])retVal)[0] = results;
                    break;
                case 6: // invoke of IDispatch
                    paramObject = new LocalParamsDescriptor();
                    paramObject.SetSession(Session);
                    paramObject.AddInParamAsType(typeof(int));
                    paramObject.AddInParamAsType(typeof(UUID));
                    paramObject.AddInParamAsType(typeof(int));
                    paramObject.AddInParamAsType(typeof(int));

                    var dispParams = new Struct();
                    dispParams.AddMember(new ComPointer(new ComArray(typeof(Variant), null, 1, true)));
                    dispParams.AddMember(new ComPointer(new ComArray(typeof(int), null, 1, true)));
                    dispParams.AddMember(typeof(int));
                    dispParams.AddMember(typeof(int));

                    paramObject.AddInParamAsObject(dispParams, InteropFlags.FLAG_REPRESENTATION_IDISPATCH_INVOKE);
                    paramObject.AddInParamAsType(typeof(int));
                    paramObject.AddInParamAsObject(new ComArray(typeof(int), null, 1, true));
                    paramObject.AddInParamAsObject(new ComArray(typeof(Variant), null, 1, true));

                    var retresults = paramObject.Read(ndr);
                    // named params not supported
                    var dispId = (int)retresults[0];

                    info = interfaceDefinitionOfClass.GetMethodDescriptorForDispId(dispId);
                    if (info == null)
                    {
                        Log.Logger.Error("MethodDescriptor not found for DispId : " + dispId);
                        throw new InteropException(ErrorCode.DISP_E_MEMBERNOTFOUND);
                    }

                    dispParams = (Struct)retresults[4];
                    var ptrToParamsArray = (ComPointer)dispParams.GetMember(0);

                    parameters = Array.Empty<object>();
                    if (!ptrToParamsArray.IsNull)
                    {
                        // form the real array
                        array = (ComArray)ptrToParamsArray.Referent;
                        var variants = (object[])array.ArrayInstance;
                        parameters = new object[variants.Length];
                        for (var i = 0; i < variants.Length; i++)
                        {
                            parameters[i] = ((Variant)variants[i]).Object;
                        }
                    }

                    if ((int)retresults[5] != 0)
                    {
                        // now replace the params at index from the index array.
                        array = (ComArray)retresults[6];
                        var indexs = (int[])array.ArrayInstance;
                        array = (ComArray)retresults[7];
                        var variants = (Variant[])array.ArrayInstance;
                        for (var i = 0; i < indexs.Length; i++)
                        {
                            parameters[indexs[i]] = variants[i];
                        }
                    }

                    // now to reverse this array of params.
                    var halflength = parameters.Length / 2;
                    for (var i = 0; i < halflength; i++)
                    {
                        var t = parameters[i];
                        parameters[i] = parameters[parameters.Length - 1 - i];
                        parameters[parameters.Length - 1 - i] = t;
                    }
                    methodDescriptor = info;
                    execute = true;
                    break;
                default: // others are normal API calls ...Opnum - 6 is there real Opnum. 0,1,2 and 3,4,5,6
                    isStandardCall = true;
                    Opnum -= 4; // adjust for only IDispatch(3,4,5,6), IUnknown(0,1,2) will get adjusted below.
                    Log.Logger.Information("Standard call came: Opnum is " + Opnum);
                    break;
            }
        }

        if (isStandardCall)
        {
            methodDescriptor = interfaceDefinitionOfClass.GetMethodDescriptor(Opnum - 3); // adjust for IUnknown
            if (methodDescriptor == null)
            {
                throw new InteropException(ErrorCode.RPC_S_PROCNUM_OUT_OF_RANGE);
            }
            methodDescriptor.ParameterObject.SetSession(Session);
            parameters = methodDescriptor.ParameterObject.Read(ndr);
            execute = true;
        }

        if (execute)
        {
            try
            {
                Log.Logger.Information("methodDescriptor: " + methodDescriptor.MethodName);

                var dispatchIid = new Guid(iid);
                if (!TryGetDispatcher(interfaceDefinitionOfClass, dispatchIid, methodDescriptor,
                    out var dispatcher, out var calleeInstance))
                {
                    var calleeType = interfaceDefinitionOfClass.Instance == null ?
                        interfaceDefinitionOfClass.Type : interfaceDefinitionOfClass.Instance.GetType();
                    throw new MissingMethodException(calleeType.FullName, methodDescriptor.MethodName);
                }

                Log.Logger.Information("Call Back Method to be executed: " +
                    methodDescriptor.MethodName + ", to be executed on " + calleeInstance);
                var result = dispatcher(parameters);
                if (result == null)
                {
                    retVal = null;
                }
                else
                {
                    if (!(result is object[]))
                    {
                        retVal = new object[1];
                        ((object[])retVal)[0] = result;
                    }
                    else
                    {
                        retVal = result;
                    }
                }
            }
            catch (ArgumentException e)
            {
                Log.Logger.Error(e, "LocalCoClass invokeMethod");
                throw new InteropException(ErrorCode.E_INVALIDARG);
            }
            catch (MethodAccessException e)
            {
                Log.Logger.Error(e, "LocalCoClass invokeMethod", e);
                throw new InteropException(ErrorCode.ERROR_ACCESS_DENIED);
            }
            catch (TargetInvocationException e)
            {
                Log.Logger.Error(e, "LocalCoClass invokeMethod");
                throw new InteropException(ErrorCode.E_UNEXPECTED, e);
            }
            catch (SecurityException e)
            {
                Log.Logger.Error(e, "LocalCoClass invokeMethod");
                throw new InteropException(ErrorCode.ERROR_ACCESS_DENIED, e);
            }
            catch (MissingMethodException e)
            {
                Log.Logger.Error(e, "LocalCoClass invokeMethod");
                throw new InteropException(ErrorCode.RPC_S_PROCNUM_OUT_OF_RANGE);
            }
            catch (InstantiationException e)
            {
                Log.Logger.Error(e, "LocalCoClass invokeMethod");
                throw new InteropException(ErrorCode.E_UNEXPECTED, e);
            }
        }
        return (object[])retVal;
    }

    private bool TryGetDispatcher(LocalInterfaceDefinition interfaceDefinition, Guid iid,
        LocalMethodDescriptor methodDescriptor, out Func<object[], object?> dispatcher,
        out object calleeInstance)
    {
        var key = iid.ToString().ToUpper(CultureInfo.InvariantCulture);
        if (_mapOfIIDsToDispatchTables.TryGetValue(key, out var dispatchTable) &&
            dispatchTable.TryGetDispatcher(iid, methodDescriptor.MethodNum, out dispatcher))
        {
            calleeInstance = interfaceDefinition.Instance;
            return true;
        }

        dispatchTable = interfaceDefinition.DispatchTable;
        if (dispatchTable != null &&
            dispatchTable.TryGetDispatcher(iid, methodDescriptor.MethodNum, out dispatcher))
        {
            calleeInstance = interfaceDefinition.Instance;
            return true;
        }

        dispatcher = null;
        calleeInstance = null;
        return false;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (!(obj is LocalCoClass other))
        {
            return false;
        }
        return _identifier == other._identifier;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => _identifier;

    private static readonly Random kRandomGen = new Random();
    private readonly int _identifier;
    private WeakReference _interfacePointer;
    private readonly List<string> _listOfSupportedEventInterfaces = new List<string>();
    private readonly Lock _syncRoot = new();
    private readonly Dictionary<string, LocalInterfaceDefinition> _mapOfIIDsToInterfaceDefinitions =
        new Dictionary<string, LocalInterfaceDefinition>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IDispatchTable> _mapOfIIDsToDispatchTables =
        new Dictionary<string, IDispatchTable>(StringComparer.OrdinalIgnoreCase);
    // will use this to identify which IID is being talked about
    // if it is IDispatch then delegate to it's invoke.
    private readonly Dictionary<string, string> _ipidVsIID = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    // will use this to identify which IPID is being talked about
    private readonly Dictionary<string, string> _iIDvsIpid = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
