//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.common {

    /// <summary>
    /// All errorcodes. ErrorCodes begining with "JI" are j-Interop error codes.
    /// </summary>
    public enum JIErrorCodes : int {

        /// <summary>
        /// Incorrect function.
        /// </summary>
        ERROR_INVALID_FUNCTION = 0x00000001,

        /// <summary>
        /// The system cannot find the file specified.
        /// </summary>

        ERROR_FILE_NOT_FOUND = 0x00000002,

        /// <summary>
        /// The system cannot find the path specified.
        /// </summary>
        ERROR_PATH_NOT_FOUND = 0x00000003,

        /// <summary>
        /// The filename, directory name, or volume label syntax is incorrect.
        /// </summary>
        ERROR_INVALID_NAME = 0x0000007B,

        /// <summary>
        /// File already exists.
        /// </summary>
        ERROR_ALREADY_EXISTS = 0x000000B7,

        /// <summary>
        /// No more data is available.
        /// </summary>
        ERROR_NO_MORE_ITEMS = 0x00000103,

        /// <summary>
        /// Class not registered
        /// </summary>
        REGDB_E_CLASSNOTREG = unchecked((int)0x80040154),

        /// <summary>
        /// Interface not registered
        /// </summary>
        REGDB_E_IIDNOTREG = unchecked((int)0x80040155),

        /// <summary>
        /// Access is denied.
        /// </summary>
        ERROR_ACCESS_DENIED = 0x00000005,

        /// <summary>
        /// Catastrophic failure.
        /// </summary>
        E_UNEXPECTED = unchecked((int)0x8000FFFF),

        /// <summary>
        /// Not implemented.
        /// </summary>
        E_NOTIMPL = unchecked((int)0x80004001),

        /// <summary>
        /// Not enough storage is available to complete this operation.
        /// </summary>
        E_OUTOFMEMORY = unchecked((int)0x8007000E),


        /// <summary>
        /// The parameter is incorrect.
        /// </summary>
        E_INVALIDARG = unchecked((int)0x80070057),

        /// <summary>
        /// The RPC server is unavailable.
        /// </summary>
        RPC_SERVER_UNAVAILABLE = unchecked((int)0x800706BA),

        /// <summary>
        /// No such interface supported.
        /// </summary>
        E_NOINTERFACE = unchecked((int)0x80004002),

        /// <summary>
        /// Access is denied.
        /// </summary>
        E_ACCESSDENIED = unchecked((int)0x80070005),

        /// <summary>
        /// A Remote activation was necessary but the server name provided was invalid.
        /// </summary>
        CO_E_BAD_SERVER_NAME = unchecked((int)0x80004014),

        /// <summary>
        /// The server process could not be started.  The pathname may be incorrect.
        /// </summary>
        CO_E_CREATEPROCESS_FAILURE = unchecked((int)0x80004018),

        /// <summary>
        /// The server process could not be started as the configured identity.  The pathname may be incorrect or unavailable.
        /// </summary>
        CO_E_RUNAS_CREATEPROCESS_FAILURE = unchecked((int)0x80004019),

        /// <summary>
        /// The server process could not be started because the configured identity is incorrect.  Check the username and password.
        /// </summary>
        CO_E_RUNAS_LOGON_FAILURE = unchecked((int)0x8000401A),

        /// <summary>
        /// The client is not allowed to launch this server.
        /// </summary>
        CO_E_LAUNCH_PERMSSION_DENIED = unchecked((int)0x8000401B),

        /// <summary>
        /// Server execution failed.
        /// </summary>
        CO_E_SERVER_EXEC_FAILURE = unchecked((int)0x80080005),

        /// <summary>
        /// System call failed. You might need to restart the server machine.
        /// </summary>
        RPC_E_SYS_CALL_FAILED = unchecked((int)0x80010100),

        /// <summary>
        /// Unknown interface.
        /// </summary>
        DISP_E_UNKNOWNINTERFACE = unchecked((int)0x80020001),

        /// <summary>
        /// Member not found.
        /// </summary>
        DISP_E_MEMBERNOTFOUND = unchecked((int)0x80020003),

        /// <summary>
        /// Parameter not found.
        /// </summary>
        DISP_E_PARAMNOTFOUND = unchecked((int)0x80020004),

        /// <summary>
        /// Type mismatch.
        /// </summary>
        DISP_E_TYPEMISMATCH = unchecked((int)0x80020005),

        /// <summary>
        /// No named arguments.
        /// </summary>
        DISP_E_NONAMEDARGS = unchecked((int)0x80020007),

        /// <summary>
        /// Bad variable type.
        /// </summary>
        DISP_E_BADVARTYPE = unchecked((int)0x80020008),

        /// <summary>
        /// Exception occurred.
        /// </summary>
        DISP_E_EXCEPTION = unchecked((int)0x80020009),

        /// <summary>
        /// Invalid index.
        /// </summary>
        DISP_E_BADINDEX = unchecked((int)0x8002000B),

        /// <summary>
        /// Invalid number of parameters.
        /// </summary>
        DISP_E_BADPARAMCOUNT = unchecked((int)0x8002000E),

        /// <summary>
        /// Parameter not optional.
        /// </summary>
        DISP_E_PARAMNOTOPTIONAL = unchecked((int)0x8002000F),

        /// <summary>
        /// The requested object or interface does not exist.
        /// </summary>
        RPC_E_INVALID_IPID = unchecked((int)0x80010113),

        /// <summary>
        /// The requested object does not exist.
        /// </summary>
        RPC_E_INVALID_OBJECT = unchecked((int)0x80010114),

        /// <summary>
        /// The marshaled interface data packet (OBJREF) has an invalid or unknown format.
        /// </summary>
        RPC_E_INVALID_OBJREF = unchecked((int)0x8001011D),

        /// <summary>
        /// An internal error occurred.
        /// </summary>
        RPC_E_UNEXPECTED = unchecked((int)0x8001FFFF),

        /// <summary>
        /// Call was rejected by callee.
        /// </summary>
        RPC_E_CALL_REJECTED = unchecked((int)0x80010001),

        /// <summary>
        /// Unknown name.
        /// </summary>
        DISP_E_UNKNOWNNAME = unchecked((int)0x80020006),

        /// <summary>
        /// Wrong module kind for the operation.
        /// </summary>
        TYPE_E_BADMODULEKIND = unchecked((int)0x800288BD),

        /// <summary>
        /// Element not found.
        /// </summary>
        TYPE_E_ELEMENTNOTFOUND = unchecked((int)0x8002802B),

        /// <summary>
        /// COM server could not establish call back connection.
        /// </summary>
        E_NOINTERFACE_CALLBACK = unchecked((int)0x80040202),

        /// <summary>
        /// The object exporter was not found.
        /// </summary>
        RPC_E_INVALID_OXID = unchecked((int)0x80070776),

        /// <summary>
        /// The stub recieved bad data. . Please check whether the API has been called in the right way, with correct parameter formation.
        /// </summary>
        RPC_E_INVALID_DATA = unchecked((int)0x800706F7),

        /// <summary>
        /// The procedure number is out of range.
        /// </summary>
        RPC_S_PROCNUM_OUT_OF_RANGE2 = unchecked((int)0x800706D1),

        /// <summary>
        /// The procedure number is out of range.
        /// </summary>
        RPC_S_PROCNUM_OUT_OF_RANGE = unchecked((int)0xC002002E),

        /// <summary>
        /// Access Violation.
        /// </summary>
        RPC_S_ACCESS_VIOLATION = unchecked((int)0xC0000005),

        /// <summary>
        /// The server threw an exception.
        /// </summary>
        RPC_E_SERVERFAULT = unchecked((int)0x80010105),

        /// <summary>
        /// Invalid Callee.
        /// </summary>
        DISP_E_BADCALLEE = unchecked((int)0x80020010),

        /// <summary>
        ///  The object invoked has disconnected from its clients.
        /// </summary>
        RPC_E_DISCONNECTED = unchecked((int)0x80010108),

        /// <summary>
        /// The version of OLE on the client and server machines does not match.
        /// </summary>
        RPC_E_VERSION_MISMATCH = unchecked((int)0x80010110),

        /// <summary>
        /// Space for tools is not available.
        /// </summary>
        INPLACE_E_NOTOOLSPACE = unchecked((int)0x800401A1),

        /// <summary>
        /// The attempted logon is invalid. This is either due to a bad username or authentication information.
        /// </summary>
        WIN_AUTH_FAILURE = unchecked((int)0xC000006D),

        /// <summary>
        /// Unspecified Error.
        /// </summary>
        E_FAIL = unchecked((int)0x80004005),


        /////System's Own ...start from 0x00001001 to 0x00002001

        /// <summary>
        /// Object is already instantiated.
        /// </summary>
        JI_OBJECT_ALREADY_INSTANTIATED = 0x00001001,

        /// <summary>
        /// This API cannot be invoked right now, further operations are required before the system is ready to
        /// give out results through this API.
        /// </summary>
        JI_API_INCORRECTLY_CALLED = 0x00001002,

        /// <summary>
        /// Session is already established, please initiate a new session for new Stub.
        /// </summary>
        JI_SESSION_ALREADY_ESTABLISHED = 0x00001003,

        /// <summary>
        /// Discriminant cannot be null
        /// </summary>
        JI_UNION_NULL_DISCRMINANT = 0x00001004,

        /// <summary>
        /// Discriminant class type mismatch, please provide object of the same class as discriminant.
        /// </summary>
        JI_UNION_DISCRMINANT_MISMATCH = 0x00001005,

        /// <summary>
        /// Only 1 discriminant allowed for serialization, please remove the rest or no discriminant has been added at all.
        /// </summary>
        JI_UNION_DISCRMINANT_SERIALIZATION_ERROR = 0x00001006,

        /// <summary>
        /// No discriminant value has been added at all.
        /// </summary>
        JI_UNION_DISCRMINANT_DESERIALIZATION_ERROR = 0x00001007,

        /// <summary>
        /// Incorrect Value of FLAG sent for this API. This FLAG is not valid here.
        /// </summary>
        JI_UTIL_FLAG_ERROR = 0x00001008,

        /// <summary>
        /// Internal Library Error. This method should not have been called. Please check the parameters which you have passed to JICallBuilder.
        /// They have been sent incorrectly.
        /// </summary>
        JI_UTIL_INCORRECT_CALL = 0x00001009,

        /// <summary>
        /// Outparams cannot have more than 1 parameter here. It should be a JIVariant class parameter.
        /// </summary>
        JI_DISP_INCORRECT_OUTPARAM = 0x0000100A,

        /// <summary>
        /// Parameters inparams and dispId\paramNames arrays should have same length.
        /// </summary>
        JI_DISP_INCORRECT_PARAM_LENGTH = 0x0000100B,

        /// <summary>
        /// This in parameter cannot have null or "" values.
        /// </summary>
        JI_DISP_INCORRECT_VALUE_FOR_GETIDNAMES = 0x0000100C,

        /// <summary>
        /// progId\clsid,address,session cannot be empty or null.
        /// </summary>
        JI_COMSTUB_ILLEGAL_ARGUMENTS = 0x0000100D,

        /// <summary>
        /// Could not retrieve JIClsid from JIProgId via Windows Remote Registry Service
        /// </summary>
        JI_COMSTUB_RR_ERROR = 0x0000100E,

        /// <summary>
        /// Internal Library Error, the serializer\deserializer was not found for {0}. Please check the parameters passed to JICallBuilder.
        /// </summary>
        JI_UTIL_SERDESER_NOT_FOUND = 0x0000100F,

        /// <summary>
        /// Authentication information was not supplied.
        /// </summary>
        JI_AUTH_NOT_SUPPLIED = 0x00001010,

        /// <summary>
        /// Incorrect or Invalid Parameter(s) specified.
        /// </summary>
        JI_COMFACTORY_ILLEGAL_ARG = 0x00001011,

        /// <summary>
        /// The template cannot be null.
        /// </summary>
        JI_ARRAY_TEMPLATE_NULL = 0x00001012,

        /// <summary>
        /// Only Arrays Accepted as parameter.
        /// </summary>
        JI_ARRAY_PARAM_ONLY = 0x00001013,

        /// <summary>
        /// Arrays of Primitive Data Types are not accepted
        /// </summary>
        JI_ARRAY_PRIMITIVE_NOTACCEPT = 0x00001014,

        /// <summary>
        /// Can only accept JIStruct, JIUnion, JIPointer and JIString as parameters for template.
        /// </summary>
        JI_ARRAY_INCORRECT_TEMPLATE_PARAM = 0x00001015,

        /// <summary>
        /// IPID cannot be null.
        /// </summary>
        JI_OBJ_NULL_IPID = 0x00001016,

        /// <summary>
        /// Discriminant can only be of the type Integer,Short,Boolean or Character.
        /// </summary>
        JI_UNION_INCORRECT_DISC = 0x00001017,

        /// <summary>
        /// Referent ID for <code>VARIANT</code> not found.
        /// </summary>
        JI_VARIANT_NO_REFERENT_ID = 0x00001018,

        /// <summary>
        /// This is a programming error, this API should not be called.
        /// </summary>
        JI_ILLEGAL_CALL = 0x00001019,

        /// <summary>
        /// The parameters cannot be null.
        /// </summary>
        JI_COM_RUNTIME_INVALID_CONTAINER_INFO = 0x0000101A,

        /// <summary>
        /// An array has already been added as member and it has to be the last member of this Struct. Please insert this member elsewhere.
        /// </summary>
        JI_STRUCT_ARRAY_AT_END = 0x0000101B,

        /// <summary>
        /// An array can be added only as a last member in a structure and not inbetween.
        /// </summary>
        JI_STRUCT_ARRAY_ONLY_AT_END = 0x0000101C,

        /// <summary>
        /// This struct already has an array and the member (which also happens to be a Struct) has an array too. This member can only be present in the second last position of this new Struct.
        /// </summary>
        JI_STRUCT_INCORRECT_NESTED_STRUCT_POS = 0x0000101D,

        /// <summary>
        /// Member(which happens to be a Struct) has an array and hence can only be added to the end of this Struct, not in between.
        /// </summary>
        JI_STRUCT_INCORRECT_NESTED_STRUCT_POS2 = 0x0000101E,

        /// <summary>
        /// Authentication failure for the credentials sent by the COM server for performing call back. The identity is checked via a call back to the source COM server using SMB.
        /// </summary>
        JI_CALLBACK_AUTH_FAILURE = 0x0000101F,

        /// <summary>
        /// SMB connection failure, please check whether SERVER service is running on Target machine (where COM server) is hosted.
        /// </summary>
        JI_CALLBACK_SMB_FAILURE = 0x00001020,

        /// <summary>
        /// Illegal here to invoke this API.
        /// </summary>
        JI_CALLBACK_COMOBJECT_STATE_FAILURE = 0x00001021,

        /// <summary>
        /// Variants can only take BSTR Strings and no other String Type.
        /// </summary>
        JI_VARIANT_BSTR_ONLY = 0x00001022,

        /// <summary>
        /// Overloaded APIs are not allowed.
        /// </summary>
        JI_CALLBACK_OVERLOADS_NOTALLOWED = 0x00001023,

        /// <summary>
        /// Variants cannot take object[] having Variants themselves as indices.
        /// </summary>
        JI_VARIANT_VARARRAYS_NOTALLOWED = 0x00001024,

        /// <summary>
        /// fractionalUnits cannot be negative.
        /// </summary>
        JI_CURRENCY_FRAC_NEGATIVE = 0x00001025,

        /// <summary>
        /// Variant is null.
        /// </summary>
        JI_VARIANT_IS_NULL = 0x00001026,

        /// <summary>
        /// Library currently accepts only upto 2 dimension for the JIVariant
        /// </summary>
        JI_VARIANT_VARARRAYS_2DIMRES = 0x00001027,

        /// <summary>
        /// The upperbounds is to be specified for all dimensions or not specified at all.
        /// </summary>
        JI_ARRAY_UPPERBNDS_DIM_NOTMATCH = 0x00001028,

        /// <summary>
        /// Please use the JIArray to pass arrays.
        /// </summary>
        JI_VARIANT_ONLY_JIARRAY_EXCEPTED = 0x00001029,

        /// <summary>
        /// Unsupported type for VARIANT.
        /// </summary>
        JI_VARIANT_UNSUPPORTED_TYPE = 0x00001030,

        /// <summary>
        /// Unable to access Windows Registry, please check whether the SERVER service is running on the Target Workstation.
        /// </summary>
        JI_WINREG_EXCEPTION = 0x00001031,

        /// <summary>
        /// Invalid Identifier, or there is no Connection Info associated with this identifer on this comObject.
        /// </summary>
        JI_CALLBACK_INVALID_ID = 0x00001032,

        /// <summary>
        /// Could not set the correct encoding for password field.
        /// </summary>
        JI_WINREG_EXCEPTION2 = 0x00001033,

        /// <summary>
        /// Unknown hostname\ip was supplied for obtaining handle to WinReg
        /// </summary>
        JI_WINREG_EXCEPTION3 = 0x00001034,

        /// <summary>
        /// Type not supported for setting\getting value in\from registry.
        /// </summary>
        JI_WINREG_EXCEPTION4 = 0x00001035,

        /// <summary>
        /// Illegal values sent as parameters, please check "data".
        /// </summary>
        JI_WINREG_EXCEPTION5 = 0x00001036,

        /// <summary>
        /// JILocalMethodDescriptor is being added to a JILocalInterfaceDefinition supporting dispInterface, but it itself does not have a
        /// dispId.
        /// </summary>
        JI_METHODDESC_DISPID_MISSING = 0x00001037,

        /// <summary>
        /// No parameters can be null or "".
        /// </summary>
        JI_CALLBACK_INVALID_PARAMS = 0x00001038,

        /// <summary>
        /// Unsupported charset supplied while encoding or decoding String.
        /// </summary>
        JI_UTIL_STRING_DECODE_CHARSET = 0x00001039,

        /// <summary>
        /// Unsigned numbers cannot be negative or null.
        /// </summary>
        JI_UNSIGNED_NEGATIVE = 0x00001040,

        /// <summary>
        /// Class not supportted for unsigned operations. Only Long,Short,Integer allowed.
        /// </summary>
        JI_UNSIGNED_INCORRECT_TYPE = 0x00001041,

        /// <summary>
        /// "Object.class" arrays are not accepted. Only properly typed arrays accepted.
        /// </summary>
        JI_ARRAY_TYPE_INCORRECT = 0x00001042,

        /// <summary>
        /// This JILocalCoClass has already been exported with one interface pointer, please use a new instance of this class with JIInterfacePointer.getInterfacePointer(...) api.
        /// </summary>
        JI_JAVACOCLASS_ALREADY_EXPORTED = 0x00001043,

        /// <summary>
        /// JIInterfacePointer is not a valid parameter, please use JIVariant(IJIComObject,...).
        /// </summary>
        JI_VARIANT_TYPE_INCORRECT = 0x00001044,

        /// <summary>
        /// Direct Marshalling, UnMarshalling of Strings are not allowed, please use JIString instead.
        /// </summary>
        JI_UTIL_STRING_INVALID = 0x00001045,

        /// <summary>
        /// createInstance() cannot be called since the JIComServer(JISession, JIInterfacePointer, String) ctor was used to create this COM server instance, please use getInstance() instead.
        /// </summary>
        JI_COMSTUB_WRONGCALLCREATEINSTANCE = 0x00001046,

        /// <summary>
        /// getInstance() cannot be called since the JIComServer(JISession, JIInterfacePointer, String) ctor was NOT used to create this COM server instance, please use createInstance() instead.
        /// </summary>
        JI_COMSTUB_WRONGCALLGETINSTANCE = 0x00001047,

        /// <summary>
        /// A session is already attached with this COM object.
        /// </summary>
        JI_SESSION_ALREADY_ATTACHED = 0x00001048,

        /// <summary>
        /// This API cannot be invoked on local references.
        /// </summary>
        JI_COMOBJ_LOCAL_REF = 0x00001049,

        /// <summary>
        /// A session is not attached with this object, use JIObjectFactory.buildObject(JISession, IJIComObject) to attach a session with this object.
        /// </summary>
        JI_SESSION_NOT_ATTACHED = 0x00001050,

        /// <summary>
        /// The associated session is being destroyed. Current call to COM server has been terminated.
        /// </summary>
        JI_SESSION_DESTROYED = 0x00001051,

        /// <summary>
        /// The associated session is being destroyed. Current call to COM server has been terminated.
        /// </summary>
        JI_WIN_ONLY = 0x00001052,

        /// <summary>
        /// S.S.O cannot be used with ProgId based ctors.
        /// </summary>
        JI_COMSTUB_ILLEGAL_ARGUMENTS2 = 0x00001053,
    }
}