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
    using SharpCifs.Util.Sharpen;
    using org.jinterop.dcom.common;
    using Serilog;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Internal Framework Helper class. Do not use outside of framework.
    /// </summary>
    public static class JIFrameworkHelper {

        /// <summary>
        /// link session
        /// </summary>
        /// <param name="src"> </param>
        /// <param name="target"> </param>
        internal static void Link2Sessions(JISession src, JISession target) {
            if (src == null || target == null) {
                throw new NullReferenceException();
            }
            JISession.linkTwoSessions(src, target);
        }

        /// <summary>
        /// Unlink session
        /// </summary>
        /// <param name="src"> </param>
        /// <param name="unlinkedSession"> </param>
        internal static void UnLinkSession(JISession src, JISession unlinkedSession) {
            if (src == null || unlinkedSession == null) {
                throw new NullReferenceException();
            }
            JISession.unLinkSession(src, unlinkedSession);
        }

        /// <summary>
        /// Resolve session
        /// </summary>
        /// <param name="oxid"> </param>
        /// <returns></returns>
        internal static JISession ResolveSessionForOXID(byte[] oxid) {
            return JISession.resolveSessionForOxid(new JIOxid(oxid));
        }

        /// <summary>
        /// get interface pointer
        /// </summary>
        /// <param name="session"> </param>
        /// <returns></returns>
        internal static JIInterfacePointer GetInterfacePointerOfStub(JISession session) {
            return session.Stub.ServerInterfacePointer;
        }

        /// <summary>
        /// Must be called once and only once from JICallBuilder "read" to
        /// create the right pointer in case of man in the middle scenario and
        /// add it to the session.
        /// </summary>
        /// <param name="session"> </param>
        /// <param name="ptr"></param>
        /// <exception cref="JIException"> </exception>
        /// <returns></returns>
        internal static IJIComObject InstantiateComObject(JISession session, JIInterfacePointer ptr) {
            var retval = InstantiateComObject2(session, ptr);
            AddComObjectToSession(retval.AssociatedSession, retval);
            return retval;
        }

        /// <summary>
        /// Instantiate object2
        /// </summary>
        /// <param name="session"></param>
        /// <param name="ptr"></param>
        /// <exception cref="JIException"> </exception>
        /// <returns></returns>
        internal static IJIComObject InstantiateComObject2(JISession session, JIInterfacePointer ptr) {
            if (ptr == null) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COMFACTORY_ILLEGAL_ARG));
            }

            IJIComObject retval = null;
            var stubPtr = GetInterfacePointerOfStub(session);
            if (!JIInterfacePointer.IsOxidEqual(stubPtr, ptr)) {
                Log.Logger.Warning("NEW SESSION IDENTIFIED ! for ptr " + ptr);
                //first check if a session for this OXID does not already exist and thus its stub
                var newsession = ResolveSessionForOXID(ptr.OXID);
                if (newsession == null) {
                    //new COM server pointer
                    newsession = JISession.createSession(session);
                    newsession.GlobalSocketTimeout = session.GlobalSocketTimeout;
                    newsession.useSessionSecurity(session.SessionSecurityEnabled);
                    newsession.useNTLMv2(session.NTLMv2Enabled);
                    var comServer = new JIComServer(newsession, ptr, null);
                    retval = comServer.Instance;
                    Link2Sessions(session, newsession);
                }

                //this is so that the reference gets added correctly.
                session = newsession;
            }

            if (retval == null) {
                retval = new JIComObjectImpl(session, ptr);
            }

            return retval;
        }

        /// <summary>
        /// Add to session
        /// </summary>
        /// <param name="session"></param>
        /// <param name="comObject"></param>
        internal static void AddComObjectToSession(JISession session, IJIComObject comObject) {
            session.addToSession(comObject, comObject.Internal_getInterfacePointer().OID);
        }

        /// <summary>
        /// Returns an Interface Pointer representation for the Component
        /// </summary>
        /// <param name="session"></param>
        /// <param name="javaComponent"></param>
        /// <exception cref="JIException"> </exception>
        /// <returns></returns>
        public static IJIComObject InstantiateLocalComObject(JISession session, JILocalCoClass javaComponent) {
            return new JIComObjectImpl(session, JIComOxidRuntime.GetInterfacePointer(session, javaComponent), true);
        }

        /// <summary>
        /// Release local Component
        /// </summary>
        /// <param name="session"></param>
        /// <param name="javaComponent"></param>
        /// <exception cref="JIException"> </exception>
        /// <returns></returns>
        public static void ReleaseLocalComponent(JISession session, JILocalCoClass javaComponent) {
            JIComOxidRuntime.ReleaseLocalComponent(session, javaComponent);
        }

        /// <summary>
        /// Returns an Interface Pointer representation from raw bytes.
        /// </summary>
        /// <param name="session"> </param>
        /// <param name="rawBytes">
        /// </param>
        /// <param name="ipAddress"></param>
        /// <exception cref="JIException"> </exception>
        /// <returns></returns>
        public static IJIComObject InstantiateComObject(JISession session, byte[] rawBytes, string ipAddress) {
            var ndr = new NdrCodec();
            var ndrBuffer = new NdrBuffer(rawBytes, 0);
            ndr.Buffer = ndrBuffer;
            ndrBuffer.Length = rawBytes.Length;

            //this is a brand new session.
            if (session.Stub == null) {
                var comServer = new JIComServer(session, JIInterfacePointer.Decode(ndr,
                    new List<object>(), JIFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2, new Hashtable()), ipAddress);
                return comServer.Instance;
            }
            var retval = InstantiateComObject(session, JIInterfacePointer.Decode(ndr,
                new List<object>(), JIFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2, new Hashtable()));
            //increasing the reference count.
            retval.AddRef();
            return retval;
        }

        /// <summary>
        /// Typically used in the Man-In-The-Middle scenario, where one j-Interop system interacts with another over the wire.
        /// Or the IJIComObject is deserialized from a Database and is right now drifting.
        /// </summary>
        /// <param name="session"> </param>
        /// <param name="comObject"></param>
        /// <exception cref="JIException"> </exception>
        /// <returns></returns>
        public static IJIComObject InstantiateComObject(JISession session, IJIComObject comObject) {
            if (comObject.AssociatedSession != null) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_SESSION_ALREADY_ATTACHED));
            }

            if (comObject.LocalReference) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_COMOBJ_LOCAL_REF));
            }

            return InstantiateComObject(session, comObject.Internal_getInterfacePointer());
        }

        /// <summary>
        /// Detach event handler
        /// </summary>
        /// <param name="comObject"> </param>
        /// <param name="identifier"> </param>
        /// <exception cref="JIException"> </exception>
        public static void DetachEventHandler(IJIComObject comObject, string identifier) {
            var connectionInfo = comObject.Internal_getConnectionInfo(identifier);
            if (connectionInfo == null) {
                throw new JIException(JIErrorCodes.JI_CALLBACK_INVALID_ID);
            }

            Log.Logger.Information("Detaching event handler for  comObject: " +
                comObject.InterfaceIdentifier + " , identifier: " + identifier);

            var connectionPointer = (IJIComObject)connectionInfo[0];

            //first use the cookie to detach.
            var @object = new JICallBuilder(true) {
                Opnum = 3
            };
            @object.AddInParamAsInt((int)(int?)connectionInfo[1], JIFlags.FLAG_NULL);
            connectionPointer.Call(@object);
            //now release the connectionPointer.
            connectionPointer.Release();
        }

        /// <summary>
        /// Attach event handler </summary>
        /// <param name="comObject"> </param>
        /// <param name="sourceUUID"> </param>
        /// <param name="eventListener"> </param>
        /// <exception cref="JIException"> </exception>
        /// <returns></returns>
        public static string AttachEventHandler(IJIComObject comObject, string sourceUUID,
            IJIComObject eventListener) {
            if (eventListener == null ||
                comObject == null ||
                sourceUUID == null ||
                sourceUUID.Equals("", StringComparison.CurrentCultureIgnoreCase)) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_CALLBACK_INVALID_PARAMS));
            }

            Log.Logger.Information("Attaching event handler for  comObject: " +
                comObject.InterfaceIdentifier + " , sourceUUID: " + sourceUUID +
                " , eventListener: " + eventListener.InterfaceIdentifier +
                " and eventListner IPID: " + eventListener.Ipid);
            //IID of IConnectionPointContainer :- B196B284-BAB4-101A-B69C-00AA00341D07
            var connectionPointContainer = (IJIComObject)comObject.QueryInterface("B196B284-BAB4-101A-B69C-00AA00341D07");
            var @object = new JICallBuilder(true) {
                Opnum = 1
            };
            @object.AddInParamAsUUID(sourceUUID, JIFlags.FLAG_NULL);
            @object.AddOutParamAsObject(typeof(IJIComObject), JIFlags.FLAG_NULL);
            var objects = (object[])connectionPointContainer.Call(@object); //find connection point
            var connectionPointer = (IJIComObject)objects[0];

            @object.ReInit();
            @object.Opnum = 2;
            @object.AddInParamAsComObject(eventListener, JIFlags.FLAG_NULL);
            @object.AddOutParamAsType(typeof(int?), JIFlags.FLAG_NULL);
            var obj = connectionPointer.Call(@object);

            //used to unadvise from the connectionpoint
            var dwcookie = (int?)obj[0];
            Log.Logger.Information("Event handler returned cookie " + dwcookie);
            connectionPointContainer.Release();

            return comObject.Internal_setConnectionInfo(connectionPointer, dwcookie);
        }

        /// <summary>
        /// Reverse array
        /// </summary>
        /// <param name="arrayToReverse"></param>
        /// <returns></returns>
        public static int ReverseArrayForDispatch(JIArray arrayToReverse) {
            return arrayToReverse.ReverseArrayForDispatch();
        }
    }
}