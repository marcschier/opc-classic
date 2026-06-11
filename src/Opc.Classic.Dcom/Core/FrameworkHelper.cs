// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Common.Ntlm;
using System;
using System.Collections.Generic;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Internal Framework Helper class. Do not use outside of framework.
/// </summary>
public static class FrameworkHelper
{

    /// <summary>
    /// link session
    /// </summary>
    /// <param name="src">Source NDR buffer that supplies the field data to decode.</param>
    /// <param name="target">Target object or buffer that receives the operation result.</param>
    internal static void Link2Sessions(Session src, Session target)
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(target);
        Session.LinkTwoSessions(src, target);
    }

    /// <summary>
    /// Unlink session
    /// </summary>
    /// <param name="src">Source NDR buffer that supplies the field data to decode.</param>
    /// <param name="unlinkedSession">Session that was detached from the COM object graph.</param>
    internal static void UnLinkSession(Session src, Session unlinkedSession)
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(unlinkedSession);
        Session.UnLinkSession(src, unlinkedSession);
    }

    /// <summary>
    /// Resolve session
    /// </summary>
    /// <param name="oxid">DCOM OXID identifying the object exporter process.</param>
    /// <returns>The requested resolve session for oxid value.</returns>
    internal static Session ResolveSessionForOXID(byte[] oxid) =>
        Session.ResolveSessionForOxid(new Oxid(oxid));

    /// <summary>
    /// get interface pointer
    /// </summary>
    /// <param name="session">Session that owns the COM object, transport, and authentication state.</param>
    /// <returns>The requested interface pointer of stub value.</returns>
    internal static InterfacePointer GetInterfacePointerOfStub(Session session) => session.Stub.ServerInterfacePointer;

    /// <summary>
    /// Must be called once and only once from CallBuilder "read" to
    /// create the right pointer in case of man in the middle scenario and
    /// add it to the session.
    /// </summary>
    /// <param name="session">Session that owns the COM object, transport, and authentication state.</param>
    /// <param name="ptr">Pointer referent being encoded, decoded, or dereferenced.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    /// <returns>The COM object proxy created for the requested interface.</returns>
    internal static IComObject InstantiateComObject(Session session, InterfacePointer ptr)
    {
        var retval = InstantiateComObject2(session, ptr);
        AddComObjectToSession(retval.AssociatedSession, retval);
        return retval;
    }

    /// <summary>
    /// Instantiate object2
    /// </summary>
    /// <param name="session">Session that owns the COM object, transport, and authentication state.</param>
    /// <param name="ptr">Pointer referent being encoded, decoded, or dereferenced.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    /// <returns>The COM object proxy created from the supplied interface pointer.</returns>
    internal static IComObject InstantiateComObject2(Session session, InterfacePointer ptr)
    {
        if (ptr == null)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_COMFACTORY_ILLEGAL_ARG), nameof(ptr));
        }

        IComObject retval = null;
        var stubPtr = GetInterfacePointerOfStub(session);
        if (!InterfacePointer.IsOxidEqual(stubPtr, ptr))
        {
            Log.Logger.Warning("NEW SESSION IDENTIFIED ! for ptr " + ptr);
            // first check if a session for this OXID does not already exist and thus its stub
            var newsession = ResolveSessionForOXID(ptr.OXID);
            if (newsession == null)
            {
                // new COM server pointer
                newsession = Session.CreateSession(session);
                newsession.GlobalSocketTimeout = session.GlobalSocketTimeout;
                newsession.UseSessionSecurity(session.SessionSecurityEnabled);
                newsession.UseNTLMv2(session.NTLMv2Enabled);
                var comServer = new ComServer(newsession, ptr, null);
                retval = comServer.Instance;
                Link2Sessions(session, newsession);
            }

            // this is so that the reference gets added correctly.
            session = newsession;
        }

        if (retval == null)
        {
            retval = new ComObjectImpl(session, ptr);
        }

        return retval;
    }

    /// <summary>
    /// Add to session
    /// </summary>
    /// <param name="session">Session that owns the COM object, transport, and authentication state.</param>
    /// <param name="comObject">COM object instance whose exported interfaces are being managed.</param>
    internal static void AddComObjectToSession(Session session, IComObject comObject) =>
        session.AddToSession(comObject, ((IComObjectInternal)comObject).GetInterfacePointer().OID);

    /// <summary>
    /// Returns an Interface Pointer representation for the Component
    /// </summary>
    /// <param name="session">Session that owns the COM object, transport, and authentication state.</param>
    /// <param name="localComponent">Local component of the COM version or RPC syntax identifier.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    /// <returns>The local COM object wrapper created for the supplied implementation.</returns>
    public static IComObject InstantiateLocalComObject(Session session,
        LocalCoClass localComponent) => new ComObjectImpl(session,
            ComOxidRuntime.Instance.GetInterfacePointer(session, localComponent), true);

    /// <summary>
    /// Release local Component
    /// </summary>
    /// <param name="session">Session that owns the COM object, transport, and authentication state.</param>
    /// <param name="localComponent">Local component of the COM version or RPC syntax identifier.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    /// <returns><c>true</c> when the local component was released; otherwise <c>false</c>.</returns>
    public static void ReleaseLocalComponent(Session session, LocalCoClass localComponent) =>
        ComOxidRuntime.Instance.ReleaseLocalComponent(session, localComponent);

    /// <summary>
    /// Returns an Interface Pointer representation from raw bytes.
    /// </summary>
    /// <param name="session">Session that owns the COM object, transport, and authentication state.</param>
    /// <param name="rawBytes">
    /// </param>
    /// <param name="ipAddress">IP address of the remote endpoint.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    /// <returns>The COM object proxy created for the requested interface.</returns>
    public static IComObject InstantiateComObject(Session session, byte[] rawBytes, string ipAddress)
    {
        var ndr = new NdrCodec();
        var ndrBuffer = new NdrBuffer(rawBytes, 0);
        ndr.Buffer = ndrBuffer;
        ndrBuffer.Length = rawBytes.Length;
        var context = new CodecContext
        {
            CurrentSession = session,
            Flag = InteropFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2
        };
        // this is a brand new session.
        if (session.Stub == null)
        {
            var comServer = new ComServer(session, InterfacePointer.Decode(ndr, context), ipAddress);
            return comServer.Instance;
        }
        var retval = InstantiateComObject(session, InterfacePointer.Decode(ndr, context));
        // increasing the reference count.
        retval.AddRef();
        return retval;
    }

    /// <summary>
    /// Typically used in the Man-In-The-Middle scenario, where one 
    /// Interop system interacts with another over the wire.
    /// Or the <see cref="IComObject"/> is deserialized from a
    /// Database and is right now drifting.
    /// </summary>
    /// <param name="session">Session that owns the COM object, transport, and authentication state.</param>
    /// <param name="comObject">COM object instance whose exported interfaces are being managed.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    /// <returns>The COM object proxy created for the requested interface.</returns>
    public static IComObject InstantiateComObject(Session session, IComObject comObject)
    {
        if (comObject.AssociatedSession != null)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_SESSION_ALREADY_ATTACHED), nameof(comObject));
        }

        if (comObject.LocalReference)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_COMOBJ_LOCAL_REF), nameof(comObject));
        }

        return InstantiateComObject(session,
            ((IComObjectInternal)comObject).GetInterfacePointer());
    }

    /// <summary>
    /// Detach event handler
    /// </summary>
    /// <param name="comObject">COM object instance whose exported interfaces are being managed.</param>
    /// <param name="identifier">Connection-point identifier returned when the mapping was registered.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public static void DetachEventHandler(IComObject comObject, string identifier)
    {
        var connectionInfo = ((IComObjectInternal)comObject).GetConnectionInfo(identifier);
        if (connectionInfo == null)
        {
            throw new InteropException(ErrorCode.INTEROP_CALLBACK_INVALID_ID);
        }

        Log.Logger.Information("Detaching event handler for  comObject: " +
            comObject.InterfaceIdentifier + ", identifier: " + identifier);

        var connectionPointer = (IComObject)connectionInfo[0];

        // first use the cookie to detach.
        var @object = new CallBuilder(true)
        {
            Opnum = 3
        };
        @object.AddInParamAsInt((int)connectionInfo[1]);
        connectionPointer.Call(@object);
        // now release the connectionPointer.
        connectionPointer.Release();
    }

    /// <summary>
    /// Attach event handler </summary>
    /// <param name="comObject">COM object instance whose exported interfaces are being managed.</param>
    /// <param name="sourceUUID">UUID text used as the source identifier for event hookup.</param>
    /// <param name="eventListener">Listener object that receives Automation connection-point events.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    /// <returns>The attach event handler text value.</returns>
    public static string AttachEventHandler(IComObject comObject, string sourceUUID,
        IComObject eventListener)
    {
        if (eventListener == null ||
            comObject == null ||
            sourceUUID == null ||
            sourceUUID.Equals("", StringComparison.CurrentCultureIgnoreCase))
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_CALLBACK_INVALID_PARAMS), nameof(sourceUUID));
        }

        Log.Logger.Information("Attaching event handler for  comObject: " +
            comObject.InterfaceIdentifier + ", sourceUUID: " + sourceUUID +
            ", eventListener: " + eventListener.InterfaceIdentifier +
            " and eventListner IPID: " + eventListener.Ipid);
        // IID of IConnectionPointContainer : B196B284-BAB4-101A-B69C-00AA00341D07
        var connectionPointContainer = comObject.QueryInterface("B196B284-BAB4-101A-B69C-00AA00341D07");
        var @object = new CallBuilder(true)
        {
            Opnum = 1
        };
        @object.AddInParamAsUUID(sourceUUID);
        @object.AddOutParamAsObject(typeof(IComObject));
        var objects = connectionPointContainer.Call(@object); // find connection point
        var connectionPointer = (IComObject)objects[0];

        @object.ReInit();
        @object.Opnum = 2;
        @object.AddInParamAsComObject(eventListener);
        @object.AddOutParamAsType(typeof(int));
        var obj = connectionPointer.Call(@object);

        // used to unadvise from the connectionpoint
        var dwcookie = (int)obj[0];
        Log.Logger.Information("Event handler returned cookie " + dwcookie);
        connectionPointContainer.Release();

        return ((IComObjectInternal)comObject).SetConnectionInfo(connectionPointer, dwcookie);
    }

    /// <summary>
    /// Reverse array
    /// </summary>
    /// <param name="arrayToReverse">Array whose element order should be reversed for Automation dispatch marshaling.</param>
    /// <returns>The numeric reverse array for dispatch value.</returns>
    public static int ReverseArrayForDispatch(ComArray arrayToReverse) => arrayToReverse.ReverseArrayForDispatch();
}
