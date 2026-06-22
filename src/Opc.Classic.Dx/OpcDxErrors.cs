// Copyright (c) 2026 marcschier. Licensed under the MIT License.

#pragma warning disable CA1707 // OPC DX HRESULT names intentionally preserve IDL underscore casing.
#pragma warning disable MA0048 // File name is plural per repository task; type is singular per OPC result-family convention.

namespace Opc.Classic.Dx;

/// <summary>
/// OPC DX 1.00 HRESULT constants from <c>OpcDxError.h</c> and §5.1.7.
/// </summary>
public static class OpcDxError
{
    /// <summary>
    /// <c>S_OK</c>.
    /// </summary>
    public static readonly OpcResultId S_OK = OpcResultId.Ok;

    /// <summary>
    /// <c>S_FALSE</c>.
    /// </summary>
    public static readonly OpcResultId S_FALSE = OpcResultId.False;

    /// <summary>
    /// <c>E_FAIL</c>.
    /// </summary>
    public static readonly OpcResultId E_FAIL = OpcResultId.Fail;

    /// <summary>
    /// <c>E_OUTOFMEMORY</c>.
    /// </summary>
    public static readonly OpcResultId E_OUTOFMEMORY = OpcResultId.OutOfMemory;

    /// <summary>
    /// <c>E_INVALIDARG</c>.
    /// </summary>
    public static readonly OpcResultId E_INVALIDARG = OpcResultId.InvalidArg;

    /// <summary>
    /// <c>E_NOTIMPL</c>.
    /// </summary>
    public static readonly OpcResultId E_NOTIMPL = OpcResultId.NotImplemented;

    /// <summary>
    /// <c>E_POINTER</c>.
    /// </summary>
    public static readonly OpcResultId E_POINTER = new(unchecked((int)0x80004003u), "E_POINTER");

    /// <summary>
    /// <c>E_ACCESSDENIED</c>.
    /// </summary>
    public static readonly OpcResultId E_ACCESSDENIED = new(unchecked((int)0x80070005u), "E_ACCESSDENIED");

    /// <summary>
    /// <c>OPC_E_BADTYPE</c>.
    /// </summary>
    public static readonly OpcResultId OPC_E_BADTYPE = OpcResultId.BadType;

    /// <summary>
    /// <c>OPC_E_BADRIGHTS</c>.
    /// </summary>
    public static readonly OpcResultId OPC_E_BADRIGHTS = OpcResultId.BadRights;

    /// <summary>
    /// <c>OPC_E_UNKNOWNITEMID</c>.
    /// </summary>
    public static readonly OpcResultId OPC_E_UNKNOWNITEMID = OpcResultId.UnknownItemId;

    /// <summary>
    /// <c>OPC_E_INVALIDITEMID</c>.
    /// </summary>
    public static readonly OpcResultId OPC_E_INVALIDITEMID = OpcResultId.InvalidItemId;

    /// <summary>
    /// <c>OPC_E_UNKNOWNPATH</c>.
    /// </summary>
    public static readonly OpcResultId OPC_E_UNKNOWNPATH = OpcResultId.UnknownPath;

    /// <summary>
    /// <c>OPC_E_RANGE</c>.
    /// </summary>
    public static readonly OpcResultId OPC_E_RANGE = OpcResultId.Range;

    /// <summary>
    /// <c>OPC_E_DUPLICATENAME</c>.
    /// </summary>
    public static readonly OpcResultId OPC_E_DUPLICATENAME = OpcResultId.DuplicateName;

    /// <summary>
    /// <c>OPC_S_CLAMP</c>.
    /// </summary>
    public static readonly OpcResultId OPC_S_CLAMP = OpcResultId.Clamp;

    /// <summary>
    /// <c>OPCDX_E_PERSISTING</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_PERSISTING = Result(0xC0040700u, "OPCDX_E_PERSISTING");

    /// <summary>
    /// <c>OPCDX_E_NOITEMLIST</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_NOITEMLIST = Result(0xC0040701u, "OPCDX_E_NOITEMLIST");

    /// <summary>
    /// <c>OPCDX_E_SERVER_STATE</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_SERVER_STATE = Result(0xC0040702u, "OPCDX_E_SERVER_STATE");

    /// <summary>
    /// <c>OPCDX_E_VERSION_MISMATCH</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_VERSION_MISMATCH = Result(0xC0040703u, "OPCDX_E_VERSION_MISMATCH");

    /// <summary>
    /// <c>OPCDX_E_UNKNOWN_ITEM_PATH</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_UNKNOWN_ITEM_PATH = Result(0xC0040704u, "OPCDX_E_UNKNOWN_ITEM_PATH");

    /// <summary>
    /// <c>OPCDX_E_UNKNOWN_ITEM_NAME</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_UNKNOWN_ITEM_NAME = Result(0xC0040705u, "OPCDX_E_UNKNOWN_ITEM_NAME");

    /// <summary>
    /// <c>OPCDX_E_INVALID_ITEM_PATH</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_INVALID_ITEM_PATH = Result(0xC0040706u, "OPCDX_E_INVALID_ITEM_PATH");

    /// <summary>
    /// <c>OPCDX_E_INVALID_ITEM_NAME</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_INVALID_ITEM_NAME = Result(0xC0040707u, "OPCDX_E_INVALID_ITEM_NAME");

    /// <summary>
    /// <c>OPCDX_E_INVALID_NAME</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_INVALID_NAME = Result(0xC0040708u, "OPCDX_E_INVALID_NAME");

    /// <summary>
    /// <c>OPCDX_E_DUPLICATE_NAME</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_DUPLICATE_NAME = Result(0xC0040709u, "OPCDX_E_DUPLICATE_NAME");

    /// <summary>
    /// <c>OPCDX_E_INVALID_BROWSE_PATH</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_INVALID_BROWSE_PATH = Result(0xC004070Au, "OPCDX_E_INVALID_BROWSE_PATH");

    /// <summary>
    /// <c>OPCDX_E_INVALID_SERVER_URL</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_INVALID_SERVER_URL = Result(0xC004070Bu, "OPCDX_E_INVALID_SERVER_URL");

    /// <summary>
    /// <c>OPCDX_E_INVALID_SERVER_TYPE</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_INVALID_SERVER_TYPE = Result(0xC004070Cu, "OPCDX_E_INVALID_SERVER_TYPE");

    /// <summary>
    /// <c>OPCDX_E_UNSUPPORTED_SERVER_TYPE</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_UNSUPPORTED_SERVER_TYPE = Result(0xC004070Du, "OPCDX_E_UNSUPPORTED_SERVER_TYPE");

    /// <summary>
    /// <c>OPCDX_E_CONNECTIONS_EXIST</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_CONNECTIONS_EXIST = Result(0xC004070Eu, "OPCDX_E_CONNECTIONS_EXIST");

    /// <summary>
    /// <c>OPCDX_E_TOO_MANY_CONNECTIONS</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_TOO_MANY_CONNECTIONS = Result(0xC004070Fu, "OPCDX_E_TOO_MANY_CONNECTIONS");

    /// <summary>
    /// <c>OPCDX_E_OVERRIDE_BADTYPE</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_OVERRIDE_BADTYPE = Result(0xC0040710u, "OPCDX_E_OVERRIDE_BADTYPE");

    /// <summary>
    /// <c>OPCDX_E_OVERRIDE_RANGE</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_OVERRIDE_RANGE = Result(0xC0040711u, "OPCDX_E_OVERRIDE_RANGE");

    /// <summary>
    /// <c>OPCDX_E_SUBSTITUTE_BADTYPE</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_SUBSTITUTE_BADTYPE = Result(0xC0040712u, "OPCDX_E_SUBSTITUTE_BADTYPE");

    /// <summary>
    /// <c>OPCDX_E_SUBSTITUTE_RANGE</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_SUBSTITUTE_RANGE = Result(0xC0040713u, "OPCDX_E_SUBSTITUTE_RANGE");

    /// <summary>
    /// <c>OPCDX_E_INVALID_TARGET_ITEM</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_INVALID_TARGET_ITEM = Result(0xC0040714u, "OPCDX_E_INVALID_TARGET_ITEM");

    /// <summary>
    /// <c>OPCDX_E_UNKNOWN_TARGET_ITEM</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_UNKNOWN_TARGET_ITEM = Result(0xC0040715u, "OPCDX_E_UNKNOWN_TARGET_ITEM");

    /// <summary>
    /// <c>OPCDX_E_TARGET_ALREADY_CONNECTED</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_TARGET_ALREADY_CONNECTED = Result(0xC0040716u, "OPCDX_E_TARGET_ALREADY_CONNECTED");

    /// <summary>
    /// <c>OPCDX_E_UNKNOWN_SERVER_NAME</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_UNKNOWN_SERVER_NAME = Result(0xC0040717u, "OPCDX_E_UNKNOWN_SERVER_NAME");

    /// <summary>
    /// <c>OPCDX_E_UNKNOWN_SOURCE_ITEM</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_UNKNOWN_SOURCE_ITEM = Result(0xC0040718u, "OPCDX_E_UNKNOWN_SOURCE_ITEM");

    /// <summary>
    /// <c>OPCDX_E_INVALID_SOURCE_ITEM</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_INVALID_SOURCE_ITEM = Result(0xC0040719u, "OPCDX_E_INVALID_SOURCE_ITEM");

    /// <summary>
    /// <c>OPCDX_E_INVALID_QUEUE_SIZE</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_INVALID_QUEUE_SIZE = Result(0xC004071Au, "OPCDX_E_INVALID_QUEUE_SIZE");

    /// <summary>
    /// <c>OPCDX_E_INVALID_DEADBAND</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_INVALID_DEADBAND = Result(0xC004071Bu, "OPCDX_E_INVALID_DEADBAND");

    /// <summary>
    /// <c>OPCDX_E_INVALID_CONFIG_FILE</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_INVALID_CONFIG_FILE = Result(0xC004071Cu, "OPCDX_E_INVALID_CONFIG_FILE");

    /// <summary>
    /// <c>OPCDX_E_PERSIST_FAILED</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_PERSIST_FAILED = Result(0xC004071Du, "OPCDX_E_PERSIST_FAILED");

    /// <summary>
    /// <c>OPCDX_E_TARGET_FAULT</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_TARGET_FAULT = Result(0xC004071Eu, "OPCDX_E_TARGET_FAULT");

    /// <summary>
    /// <c>OPCDX_E_TARGET_NO_ACCESS</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_TARGET_NO_ACCESS = Result(0xC004071Fu, "OPCDX_E_TARGET_NO_ACCESS");

    /// <summary>
    /// <c>OPCDX_E_SOURCE_SERVER_FAULT</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_SOURCE_SERVER_FAULT = Result(0xC0040720u, "OPCDX_E_SOURCE_SERVER_FAULT");

    /// <summary>
    /// <c>OPCDX_E_SOURCE_SERVER_NO_ACCESS</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_SOURCE_SERVER_NO_ACCESS = Result(0xC0040721u, "OPCDX_E_SOURCE_SERVER_NO_ACCESS");

    /// <summary>
    /// <c>OPCDX_E_SUBSCRIPTION_FAULT</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_SUBSCRIPTION_FAULT = Result(0xC0040722u, "OPCDX_E_SUBSCRIPTION_FAULT");

    /// <summary>
    /// <c>OPCDX_E_SOURCE_ITEM_BADRIGHTS</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_SOURCE_ITEM_BADRIGHTS = Result(0xC0040723u, "OPCDX_E_SOURCE_ITEM_BADRIGHTS");

    /// <summary>
    /// <c>OPCDX_E_SOURCE_ITEM_BAD_QUALITY</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_SOURCE_ITEM_BAD_QUALITY = Result(0xC0040724u, "OPCDX_E_SOURCE_ITEM_BAD_QUALITY");

    /// <summary>
    /// <c>OPCDX_E_SOURCE_ITEM_BADTYPE</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_SOURCE_ITEM_BADTYPE = Result(0xC0040725u, "OPCDX_E_SOURCE_ITEM_BADTYPE");

    /// <summary>
    /// <c>OPCDX_E_SOURCE_ITEM_RANGE</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_SOURCE_ITEM_RANGE = Result(0xC0040726u, "OPCDX_E_SOURCE_ITEM_RANGE");

    /// <summary>
    /// <c>OPCDX_E_SOURCE_SERVER_NOT_CONNECTED</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_SOURCE_SERVER_NOT_CONNECTED = Result(0xC0040727u, "OPCDX_E_SOURCE_SERVER_NOT_CONNECTED");

    /// <summary>
    /// <c>OPCDX_E_SOURCE_SERVER_TIMEOUT</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_SOURCE_SERVER_TIMEOUT = Result(0xC0040728u, "OPCDX_E_SOURCE_SERVER_TIMEOUT");

    /// <summary>
    /// <c>OPCDX_E_TARGET_ITEM_DISCONNECTED</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_TARGET_ITEM_DISCONNECTED = Result(0xC0040729u, "OPCDX_E_TARGET_ITEM_DISCONNECTED");

    /// <summary>
    /// <c>OPCDX_E_TARGET_NO_WRITES_ATTEMPTED</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_TARGET_NO_WRITES_ATTEMPTED = Result(0xC004072Au, "OPCDX_E_TARGET_NO_WRITES_ATTEMPTED");

    /// <summary>
    /// <c>OPCDX_E_TARGET_ITEM_BADTYPE</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_TARGET_ITEM_BADTYPE = Result(0xC004072Bu, "OPCDX_E_TARGET_ITEM_BADTYPE");

    /// <summary>
    /// <c>OPCDX_E_TARGET_ITEM_RANGE</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_E_TARGET_ITEM_RANGE = Result(0xC004072Cu, "OPCDX_E_TARGET_ITEM_RANGE");

    /// <summary>
    /// <c>OPCDX_S_TARGET_SUBSTITUTED</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_S_TARGET_SUBSTITUTED = Result(0x00040780u, "OPCDX_S_TARGET_SUBSTITUTED");

    /// <summary>
    /// <c>OPCDX_S_TARGET_OVERRIDEN</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_S_TARGET_OVERRIDEN = Result(0x00040781u, "OPCDX_S_TARGET_OVERRIDEN");

    /// <summary>
    /// <c>OPCDX_S_CLAMP</c>.
    /// </summary>
    public static readonly OpcResultId OPCDX_S_CLAMP = Result(0x00040782u, "OPCDX_S_CLAMP");

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_PERSISTING" />.
    /// </summary>
    public static readonly OpcResultId E_PERSISTING = OPCDX_E_PERSISTING;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_NOITEMLIST" />.
    /// </summary>
    public static readonly OpcResultId E_NOITEMLIST = OPCDX_E_NOITEMLIST;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_SERVER_STATE" />.
    /// </summary>
    public static readonly OpcResultId E_SERVER_STATE = OPCDX_E_SERVER_STATE;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_VERSION_MISMATCH" />.
    /// </summary>
    public static readonly OpcResultId E_VERSION_MISMATCH = OPCDX_E_VERSION_MISMATCH;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_UNKNOWN_ITEM_PATH" />.
    /// </summary>
    public static readonly OpcResultId E_UNKNOWN_ITEM_PATH = OPCDX_E_UNKNOWN_ITEM_PATH;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_UNKNOWN_ITEM_NAME" />.
    /// </summary>
    public static readonly OpcResultId E_UNKNOWN_ITEM_NAME = OPCDX_E_UNKNOWN_ITEM_NAME;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_INVALID_ITEM_PATH" />.
    /// </summary>
    public static readonly OpcResultId E_INVALID_ITEM_PATH = OPCDX_E_INVALID_ITEM_PATH;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_INVALID_ITEM_NAME" />.
    /// </summary>
    public static readonly OpcResultId E_INVALID_ITEM_NAME = OPCDX_E_INVALID_ITEM_NAME;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_INVALID_NAME" />.
    /// </summary>
    public static readonly OpcResultId E_INVALID_NAME = OPCDX_E_INVALID_NAME;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_DUPLICATE_NAME" />.
    /// </summary>
    public static readonly OpcResultId E_DUPLICATE_NAME = OPCDX_E_DUPLICATE_NAME;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_INVALID_BROWSE_PATH" />.
    /// </summary>
    public static readonly OpcResultId E_INVALID_BROWSE_PATH = OPCDX_E_INVALID_BROWSE_PATH;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_INVALID_SERVER_URL" />.
    /// </summary>
    public static readonly OpcResultId E_INVALID_SERVER_URL = OPCDX_E_INVALID_SERVER_URL;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_INVALID_SERVER_TYPE" />.
    /// </summary>
    public static readonly OpcResultId E_INVALID_SERVER_TYPE = OPCDX_E_INVALID_SERVER_TYPE;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_UNSUPPORTED_SERVER_TYPE" />.
    /// </summary>
    public static readonly OpcResultId E_UNSUPPORTED_SERVER_TYPE = OPCDX_E_UNSUPPORTED_SERVER_TYPE;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_CONNECTIONS_EXIST" />.
    /// </summary>
    public static readonly OpcResultId E_CONNECTIONS_EXIST = OPCDX_E_CONNECTIONS_EXIST;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_TOO_MANY_CONNECTIONS" />.
    /// </summary>
    public static readonly OpcResultId E_TOO_MANY_CONNECTIONS = OPCDX_E_TOO_MANY_CONNECTIONS;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_OVERRIDE_BADTYPE" />.
    /// </summary>
    public static readonly OpcResultId E_OVERRIDE_BADTYPE = OPCDX_E_OVERRIDE_BADTYPE;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_OVERRIDE_RANGE" />.
    /// </summary>
    public static readonly OpcResultId E_OVERRIDE_RANGE = OPCDX_E_OVERRIDE_RANGE;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_SUBSTITUTE_BADTYPE" />.
    /// </summary>
    public static readonly OpcResultId E_SUBSTITUTE_BADTYPE = OPCDX_E_SUBSTITUTE_BADTYPE;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_SUBSTITUTE_RANGE" />.
    /// </summary>
    public static readonly OpcResultId E_SUBSTITUTE_RANGE = OPCDX_E_SUBSTITUTE_RANGE;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_INVALID_TARGET_ITEM" />.
    /// </summary>
    public static readonly OpcResultId E_INVALID_TARGET_ITEM = OPCDX_E_INVALID_TARGET_ITEM;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_UNKNOWN_TARGET_ITEM" />.
    /// </summary>
    public static readonly OpcResultId E_UNKNOWN_TARGET_ITEM = OPCDX_E_UNKNOWN_TARGET_ITEM;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_TARGET_ALREADY_CONNECTED" />.
    /// </summary>
    public static readonly OpcResultId E_TARGET_ALREADY_CONNECTED = OPCDX_E_TARGET_ALREADY_CONNECTED;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_UNKNOWN_SERVER_NAME" />.
    /// </summary>
    public static readonly OpcResultId E_UNKNOWN_SERVER_NAME = OPCDX_E_UNKNOWN_SERVER_NAME;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_UNKNOWN_SOURCE_ITEM" />.
    /// </summary>
    public static readonly OpcResultId E_UNKNOWN_SOURCE_ITEM = OPCDX_E_UNKNOWN_SOURCE_ITEM;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_INVALID_SOURCE_ITEM" />.
    /// </summary>
    public static readonly OpcResultId E_INVALID_SOURCE_ITEM = OPCDX_E_INVALID_SOURCE_ITEM;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_INVALID_QUEUE_SIZE" />.
    /// </summary>
    public static readonly OpcResultId E_INVALID_QUEUE_SIZE = OPCDX_E_INVALID_QUEUE_SIZE;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_INVALID_DEADBAND" />.
    /// </summary>
    public static readonly OpcResultId E_INVALID_DEADBAND = OPCDX_E_INVALID_DEADBAND;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_INVALID_CONFIG_FILE" />.
    /// </summary>
    public static readonly OpcResultId E_INVALID_CONFIG_FILE = OPCDX_E_INVALID_CONFIG_FILE;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_PERSIST_FAILED" />.
    /// </summary>
    public static readonly OpcResultId E_PERSIST_FAILED = OPCDX_E_PERSIST_FAILED;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_TARGET_FAULT" />.
    /// </summary>
    public static readonly OpcResultId E_TARGET_FAULT = OPCDX_E_TARGET_FAULT;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_TARGET_NO_ACCESS" />.
    /// </summary>
    public static readonly OpcResultId E_TARGET_NO_ACCESS = OPCDX_E_TARGET_NO_ACCESS;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_SOURCE_SERVER_FAULT" />.
    /// </summary>
    public static readonly OpcResultId E_SOURCE_SERVER_FAULT = OPCDX_E_SOURCE_SERVER_FAULT;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_SOURCE_SERVER_NO_ACCESS" />.
    /// </summary>
    public static readonly OpcResultId E_SOURCE_SERVER_NO_ACCESS = OPCDX_E_SOURCE_SERVER_NO_ACCESS;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_SUBSCRIPTION_FAULT" />.
    /// </summary>
    public static readonly OpcResultId E_SUBSCRIPTION_FAULT = OPCDX_E_SUBSCRIPTION_FAULT;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_SOURCE_ITEM_BADRIGHTS" />.
    /// </summary>
    public static readonly OpcResultId E_SOURCE_ITEM_BADRIGHTS = OPCDX_E_SOURCE_ITEM_BADRIGHTS;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_SOURCE_ITEM_BAD_QUALITY" />.
    /// </summary>
    public static readonly OpcResultId E_SOURCE_ITEM_BAD_QUALITY = OPCDX_E_SOURCE_ITEM_BAD_QUALITY;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_SOURCE_ITEM_BADTYPE" />.
    /// </summary>
    public static readonly OpcResultId E_SOURCE_ITEM_BADTYPE = OPCDX_E_SOURCE_ITEM_BADTYPE;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_SOURCE_ITEM_RANGE" />.
    /// </summary>
    public static readonly OpcResultId E_SOURCE_ITEM_RANGE = OPCDX_E_SOURCE_ITEM_RANGE;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_SOURCE_SERVER_NOT_CONNECTED" />.
    /// </summary>
    public static readonly OpcResultId E_SOURCE_SERVER_NOT_CONNECTED = OPCDX_E_SOURCE_SERVER_NOT_CONNECTED;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_SOURCE_SERVER_TIMEOUT" />.
    /// </summary>
    public static readonly OpcResultId E_SOURCE_SERVER_TIMEOUT = OPCDX_E_SOURCE_SERVER_TIMEOUT;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_TARGET_ITEM_DISCONNECTED" />.
    /// </summary>
    public static readonly OpcResultId E_TARGET_ITEM_DISCONNECTED = OPCDX_E_TARGET_ITEM_DISCONNECTED;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_TARGET_NO_WRITES_ATTEMPTED" />.
    /// </summary>
    public static readonly OpcResultId E_TARGET_NO_WRITES_ATTEMPTED = OPCDX_E_TARGET_NO_WRITES_ATTEMPTED;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_TARGET_ITEM_BADTYPE" />.
    /// </summary>
    public static readonly OpcResultId E_TARGET_ITEM_BADTYPE = OPCDX_E_TARGET_ITEM_BADTYPE;

    /// <summary>
    /// Short alias for <see cref="OPCDX_E_TARGET_ITEM_RANGE" />.
    /// </summary>
    public static readonly OpcResultId E_TARGET_ITEM_RANGE = OPCDX_E_TARGET_ITEM_RANGE;

    /// <summary>
    /// Short alias for <see cref="OPCDX_S_TARGET_SUBSTITUTED" />.
    /// </summary>
    public static readonly OpcResultId S_TARGET_SUBSTITUTED = OPCDX_S_TARGET_SUBSTITUTED;

    /// <summary>
    /// Short alias for <see cref="OPCDX_S_TARGET_OVERRIDEN" />.
    /// </summary>
    public static readonly OpcResultId S_TARGET_OVERRIDEN = OPCDX_S_TARGET_OVERRIDEN;

    /// <summary>
    /// Short alias for <see cref="OPCDX_S_CLAMP" />.
    /// </summary>
    public static readonly OpcResultId S_CLAMP = OPCDX_S_CLAMP;

    /// <summary>
    /// Friendly alias for <see cref="OPCDX_E_VERSION_MISMATCH" />.
    /// </summary>
    public static OpcResultId VersionMismatch => OPCDX_E_VERSION_MISMATCH;

    /// <summary>
    /// Friendly alias for <see cref="OPCDX_E_CONNECTIONS_EXIST" />.
    /// </summary>
    public static OpcResultId ConnectionsExist => OPCDX_E_CONNECTIONS_EXIST;

    /// <summary>
    /// Friendly alias for <see cref="OPCDX_E_INVALID_BROWSE_PATH" />.
    /// </summary>
    public static OpcResultId InvalidBrowsePath => OPCDX_E_INVALID_BROWSE_PATH;

    /// <summary>
    /// Friendly alias for <see cref="OPCDX_E_INVALID_SERVER_URL" />.
    /// </summary>
    public static OpcResultId InvalidServerUrl => OPCDX_E_INVALID_SERVER_URL;

    /// <summary>
    /// Friendly alias for <see cref="OPCDX_E_SOURCE_SERVER_NOT_CONNECTED" />.
    /// </summary>
    public static OpcResultId SourceServerNotConnected => OPCDX_E_SOURCE_SERVER_NOT_CONNECTED;

    /// <summary>
    /// Friendly alias for <see cref="OPCDX_E_TARGET_ITEM_DISCONNECTED" />.
    /// </summary>
    public static OpcResultId TargetItemDisconnected => OPCDX_E_TARGET_ITEM_DISCONNECTED;

    /// <summary>
    /// Unique DX-relevant result identifiers exposed by this table.
    /// </summary>
    public static IReadOnlyList<OpcResultId> All { get; } = new[]
    {
        S_OK,
        S_FALSE,
        E_FAIL,
        E_OUTOFMEMORY,
        E_INVALIDARG,
        E_NOTIMPL,
        E_POINTER,
        E_ACCESSDENIED,
        OPC_E_BADTYPE,
        OPC_E_BADRIGHTS,
        OPC_E_UNKNOWNITEMID,
        OPC_E_INVALIDITEMID,
        OPC_E_UNKNOWNPATH,
        OPC_E_RANGE,
        OPC_E_DUPLICATENAME,
        OPC_S_CLAMP,
        OPCDX_E_PERSISTING,
        OPCDX_E_NOITEMLIST,
        OPCDX_E_SERVER_STATE,
        OPCDX_E_VERSION_MISMATCH,
        OPCDX_E_UNKNOWN_ITEM_PATH,
        OPCDX_E_UNKNOWN_ITEM_NAME,
        OPCDX_E_INVALID_ITEM_PATH,
        OPCDX_E_INVALID_ITEM_NAME,
        OPCDX_E_INVALID_NAME,
        OPCDX_E_DUPLICATE_NAME,
        OPCDX_E_INVALID_BROWSE_PATH,
        OPCDX_E_INVALID_SERVER_URL,
        OPCDX_E_INVALID_SERVER_TYPE,
        OPCDX_E_UNSUPPORTED_SERVER_TYPE,
        OPCDX_E_CONNECTIONS_EXIST,
        OPCDX_E_TOO_MANY_CONNECTIONS,
        OPCDX_E_OVERRIDE_BADTYPE,
        OPCDX_E_OVERRIDE_RANGE,
        OPCDX_E_SUBSTITUTE_BADTYPE,
        OPCDX_E_SUBSTITUTE_RANGE,
        OPCDX_E_INVALID_TARGET_ITEM,
        OPCDX_E_UNKNOWN_TARGET_ITEM,
        OPCDX_E_TARGET_ALREADY_CONNECTED,
        OPCDX_E_UNKNOWN_SERVER_NAME,
        OPCDX_E_UNKNOWN_SOURCE_ITEM,
        OPCDX_E_INVALID_SOURCE_ITEM,
        OPCDX_E_INVALID_QUEUE_SIZE,
        OPCDX_E_INVALID_DEADBAND,
        OPCDX_E_INVALID_CONFIG_FILE,
        OPCDX_E_PERSIST_FAILED,
        OPCDX_E_TARGET_FAULT,
        OPCDX_E_TARGET_NO_ACCESS,
        OPCDX_E_SOURCE_SERVER_FAULT,
        OPCDX_E_SOURCE_SERVER_NO_ACCESS,
        OPCDX_E_SUBSCRIPTION_FAULT,
        OPCDX_E_SOURCE_ITEM_BADRIGHTS,
        OPCDX_E_SOURCE_ITEM_BAD_QUALITY,
        OPCDX_E_SOURCE_ITEM_BADTYPE,
        OPCDX_E_SOURCE_ITEM_RANGE,
        OPCDX_E_SOURCE_SERVER_NOT_CONNECTED,
        OPCDX_E_SOURCE_SERVER_TIMEOUT,
        OPCDX_E_TARGET_ITEM_DISCONNECTED,
        OPCDX_E_TARGET_NO_WRITES_ATTEMPTED,
        OPCDX_E_TARGET_ITEM_BADTYPE,
        OPCDX_E_TARGET_ITEM_RANGE,
        OPCDX_S_TARGET_SUBSTITUTED,
        OPCDX_S_TARGET_OVERRIDEN,
        OPCDX_S_CLAMP,
    };

    private static OpcResultId Result(uint code, string name) => new(unchecked((int)code), name);
}
