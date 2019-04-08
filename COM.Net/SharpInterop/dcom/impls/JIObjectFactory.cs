// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.impls {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls.automation;

    /// <summary>
    /// Factory class for creating COM objects. 
    /// Sample Usage:
    /// <code>
    ///  //Assume comObject is the reference to IJIComObject, obtained earlier... 
    ///  newComObject = (IJIComObject)comObject.QueryInterface(
    ///     "76A6415B-CB41-11d1-8B02-00600806D9B6");//ISWbemLocator 
    ///  //This will obtain the dispatch interface 
    ///  dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(
    ///     newComObject.queryInterface(Interfaces.IID_IDispatch)); 
    /// </code>
    /// </summary>
    public class JIObjectFactory {

        /// <summary>
        ///<para> Attaches an event handler to <code>comObject</code> for the
        ///source event interface of COM , identified by the
        /// <code>sourceUUID</code>. The event listener is itself identified by
        /// <code>eventListener</code>. An exception will be raised if
        /// <code>sourceUUID</code> is not supported by the COM Server.
        ///  </para>
        /// </summary>
        /// <param name="comObject"> object to which the listener will be attached.
        /// </param>
        /// <param name="sourceUUID"> <code>IID</code> of the call back interface.
        /// </param>
        /// <param name="eventListener"> <code>IJIComObject</code> obtained using 
        /// <seealso cref="BuildObject(JISession, JILocalCoClass)"/> </param>
        /// <returns> string identifier for this connection, please save this for 
        /// eventual release using <seealso cref="DetachEventHandler(IJIComObject, string)"/>
        /// </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if any parameter is
        /// <code>null</code> or <code>sourceUUID</code> is empty. </exception>
        public static string AttachEventHandler(IJIComObject comObject, string sourceUUID, 
            IJIComObject eventListener) => 
            JIFrameworkHelper.AttachEventHandler(comObject, sourceUUID, eventListener);

        /// <summary>
        /// Detaches the event handler identified by <code>identifier</code> and 
        /// associated with this <code>comObject</code>. This method
        /// will raise an exception if the <code>identifier</code> is invalid.
        /// </summary>
        /// <param name="comObject"> </param>
        /// <param name="identifier"> </param>
        /// <exception cref="JIException"> </exception>
        public static void DetachEventHandler(IJIComObject comObject, string identifier) =>
            JIFrameworkHelper.DetachEventHandler(comObject, identifier);

        /// <summary>
        /// <i>Narrows</i> the <code>comObject</code> into its right type based on 
        /// it's <code>IID</code>. For example, passing a
        /// <code>comObject</code> which is a COM <code>IDispatch</code> reference
        /// will return a reference which can be safely casted
        /// to <code>IJIDispatch</code> interface.
        /// </summary>
        /// <param name="comObject">
        /// </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if <code>comObject</code>
        /// is <code>null</code> or a local reference. </exception>
        public static IJIComObject NarrowObject(IJIComObject comObject) {
            if (comObject == null || comObject.LocalReference) {
                throw new System.ArgumentException(
                    JISystem.GetLocalizedMessage(JIErrorCodes.JI_COMFACTORY_ILLEGAL_ARG));
            }
            //Will later on add another way to dynamically moving to factories.
            return JIAutomationFactory.NarrowObject(comObject);
        }

        /// <summary>
        /// Returns a <b>local</b> COM Object representation for the component. 
        /// <code>IJIComObject.IsLocalReference()</code>
        /// method will return <code>true</code> for all objects built by this 
        /// method. Another important point to note is that a
        /// <code>localComponent</code> can only export one reference to itself.
        /// Reusing the same <code>localComponent</code> in another
        /// call to this method will raise an exception.
        /// </summary>
        /// <param name="session"> session to attach <code>comObject</code> to.
        /// </param>
        /// <param name="localComponent"></param>
        /// <exception cref="JIException"> </exception>
        public static IJIComObject BuildObject(JISession session, JILocalCoClass localComponent) => 
            JIFrameworkHelper.InstantiateLocalComObject(session, localComponent);

        /// <summary>
        /// To be called after one is done using the local Java CoClass. 
        /// Recommended to be called from the <code>finalize()</code> method of the 
        /// local CoClass.
        /// </summary>
        /// <param name="session"> </param>
        /// <param name="localComponent"> </param>
        /// <exception cref="JIException"> </exception>
        public static void ReleaseObject(JISession session, JILocalCoClass localComponent) => 
            JIFrameworkHelper.ReleaseLocalComponent(session, localComponent);

        /// <summary>
        /// Returns a COM Object from raw bytes. These bytes must conform to the 
        /// Marshalled Interface Pointer template as per DCOM specifications.
        /// </summary>
        /// <param name="session"> session to attach <code>comObject</code> to. If
        /// required the framework will create a new session
        /// for this <code>comObject</code> and link the <code>session</code> to 
        /// the new one. This new session will be
        /// destroyed when the parent <code>session</code> is destroyed. </param>
        /// <param name="rawBytes"> bytes representing the interface pointer.
        /// </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if <code>rawBytes</code> 
        /// is an invalid representation. </exception>
        public static IJIComObject BuildObject(JISession session, byte[] rawBytes) => 
            NarrowObject(JIFrameworkHelper.InstantiateComObject(session, rawBytes, null));

        /// <summary>
        /// Returns a COM Object from raw bytes. These bytes must conform to the 
        /// Marshalled Interface Pointer template as per DCOM specifications.
        /// </summary>
        /// <param name="session"> session to attach <code>comObject</code> to. 
        /// If required the framework will create a new session
        /// for this <code>comObject</code> and link the <code>session</code> to 
        /// the new one. This new session will be
        /// destroyed when the parent <code>session</code> is destroyed. </param>
        /// <param name="rawBytes"> bytes representing the interface pointer. 
        /// </param>
        /// <param name="ipAddress">	can be <code>null</code>. Sometimes there are many 
        /// adapters (virtual as well) on the Target machine to which this interface pointer belongs,
        /// which may get sent as part of the interface pointer and consequently this call
        /// will fail since it is a possibility that IP is not reachable via this machine.
        /// The developer can send in the valid IP and if found in the interface pointer list 
        /// will be used to talk to the target machine, overriding the other IP addresses
        /// present in the interface pointer. If this IP is not found then the "machine name"
        /// binding will be used. If this param is <code>null</code> then the first
        /// binding obtained from the interface pointer is used.
        /// </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if <code>rawBytes</code> is an
        /// invalid representation. </exception>
        public static IJIComObject BuildObject(JISession session, byte[] rawBytes, string ipAddress) => 
            NarrowObject(JIFrameworkHelper.InstantiateComObject(session, rawBytes, ipAddress));

        /// <summary>
        /// Typically used in the Man-In-The-Middle scenario.
        /// <para> Some possible use-cases :
        /// <ul>
        ///  <li>One system interacts with another over the wire.</li>
        ///  <li>The <code>IJIComObject</code> is read from a database and is not
        ///  <i>attached</i> to a session.</li>
        /// </ul>
        /// </para>
        /// </summary>
        /// <param name="session"> session to attach <code>comObject</code> to.
        /// If required the framework will create a new session
        /// for this <code>comObject</code> and link the <code>session</code> to 
        /// the new one. This new session will be
        /// destroyed when the parent <code>session</code> is destroyed.
        /// </param>
        /// <param name="comObject"> <i>drifting</i> object.
        /// </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if <code>comObject</code>
        /// is <code>null</code> or a local reference. </exception>
        /// <seealso cref="IJIComObject.LocalReference"></seealso>
        public static IJIComObject NarrowObject(JISession session, IJIComObject comObject) => 
            NarrowObject(JIFrameworkHelper.InstantiateComObject(session, comObject));
    }
}