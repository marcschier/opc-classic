// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

using org.jinterop.dcom.common;

namespace org.jinterop.dcom.core {
    /// <summary>
    /// <para> Represents a Windows COM Object. Instances of this interface can 
    /// be retrieved by the following ways only :-
    /// <ul>
    /// <li>During initial handshake as expressed in the sample below.</li>
    /// <li>As references passed from <i>Windows COM runtime</i> such as when using 
    /// <seealso cref="queryInterface"/> or returned as <code>[out]</code> parameters 
    /// to calls (directly as <code>IJIComObject</code>(s) or part of <code>JIVariant</code>
    /// (s)). </li>
    /// <li>From raw bytes using <seealso cref="impls.JIObjectFactory.buildObject(JISession, sbyte[])"/></li>
    /// <li>As references to local Java-COM interfaces (which are then used for event handling). 
    /// See <seealso cref="impls.JIObjectFactory.buildObject(JISession, JILocalCoClass)"/>
    /// for more details.</li>
    /// </ul>
    /// All references obtained by any mechanism stated above <b>must</b> be <i>narrowed</i> 
    /// using <seealso cref="impls.JIObjectFactory.narrowObject(IJIComObject)"/>
    /// before being casted to the expected type.
    /// </para>
    /// <para>
    /// Sample usage :-
    /// <code>
    /// JISession session = JISession.createSession("DOMAIN","USERNAME","PASSWORD");
    /// JIComServer comserver = new JIComServer(JIProgId.valueOf("Word.Application"),address,session);
    /// IJIComObject comObject = comserver.createInstance();
    /// </code>
    /// Also , 
    /// <code>
    /// IJIComObject handle = comObject.queryInterface("620012E2-69E3-4DC0-B553-AE252524D2F6");
    /// </code>
    /// </para>
    /// <b>Note</b>: Methods starting with <i>internal_</i> keyword are internal to the framework 
    /// and must not be called by the developer.
    /// </summary>
    public interface IJIComObject {

        /// <summary>
        ///<para>Retrieve interface references based on <code>iid</code>. Make sure to 
        /// narrow before casting to the expected type.
        /// </para>
        /// <para>
        /// For example when expecting an <code>IJIEnumVariant</code> :-
        /// </para>
        /// <para>
        /// <code>
        /// IJIComObject object2 = variant.getObjectAsComObject();
        /// IJIEnumVariant enumVariant = (IJIEnumVariant)JIObjectFactory.narrowObject(object2.queryInterface(IJIEnumVariant.IID));
        /// </code>
        /// </para>
        /// <seealso cref="impls.JIObjectFactory.narrowObject(IJIComObject)"></seealso>
        /// </summary>
        /// <param name="iid"> string representation of the IID. </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.InvalidOperationException"> if there is no session associated 
        /// with this object or this object represents a local java reference. </exception>
        /// <returns> reference to the requested unknown. </returns>
        IJIComObject queryInterface(string iid);

        /// <summary>
        /// <P>Increases the reference count on the COM server by 5
        /// (currently hard coded). The developer should refrain from calling this API, 
        /// as referencing is maintained internally by the system though he is not 
        /// obligated to do so. If the <seealso cref="#release()"/> is not called in conjunction 
        /// with <code>addRef</code> then the COM Instance will not get garbage collected 
        /// at the server. 
        /// </P> </summary>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.InvalidOperationException"> if there is no session associated 
        /// with this object or this object represents a local java reference.
        ///  </exception>
        void addRef();

        /// <summary>
        /// Decreases the reference count on the COM server by 5 
        /// (currently hard coded). The developer should refrain from calling this API, 
        /// as referencing is maintained internally by the system though he is not
        /// obligated to do so. If the <code>release</code> is not called in conjunction 
        /// with <seealso cref="addRef()"/> then the COM Instance will not get garbage collected at 
        /// the server.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.InvalidOperationException"> if there is no session associated 
        /// with this object or this object represents a local java reference.  </exception>
        void release();

        /// <summary>
        /// Unique 128 bit uuid representing the interface on the COM server. This value can
        /// and should be used to map an <code>IJIUnreferenced</code> handler implementation to this COM Object.
        /// </summary>
        /// <remarks>Under <b>NO</b> circumstances should a reference to this COM object be stored any where
        /// for only purposes of "unreferenced" handling. This would hinder the way in which objects
        /// are garbage collected by the framework and this object would be forever "live".
        /// </remarks>
        /// <returns> string representation of ipid. </returns>
        string Ipid { get; }

        /// <summary>
        /// Executes a method call on the actual COM object represented by this interface. 
        /// All the data like parameter information, operation number etc. are prepared and 
        /// sent via the <code>JICallBuilder</code>.
        /// </summary>
        /// <para>
        /// <code>
        ///  JICallBuilder obj = new JICallBuilder(); 
        ///  obj.reInit(); 
        ///  obj.setOpnum(0); //methods are sequentially indexed from 0 in the IDL
        ///  obj.addInParamAsString(new JIString("j-Interop Rocks",JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR), JIFlags.FLAG_NULL); 
        ///  obj.addInParamAsPointer(new JIPointer(new JIString("Pretty simple ;)",JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR)), JIFlags.FLAG_NULL); 
        ///  Object[] result = comObject.call(obj);
        /// </code>
        /// If return values are expected then set up the <i>Out Params</i> also in the 
        /// <code>JICallBuilder</code>. 
        /// The call timeout used here, by default is the instance level timeout. If no 
        /// instance level timeout has been specified(or is 0) then the global timeout set in 
        /// <seealso cref="JISession"/> will be used. 
        /// </para>
        /// <param name="obj"> call builder carrying all information necessary to make the call successfully. </param>
        /// <returns> Object[] array representing the results in the order expected or set in <code>JICallBuilder</code>. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.InvalidOperationException"> if there is no session associated 
        /// with this object or this object represents a local java reference. </exception>
        object[] call(JICallBuilder obj);

        /// <summary>
        ///<P> Refer <seealso cref="call(JICallBuilder)"/> for details on this method.
        /// </P> </summary>
        /// <param name="obj"> call builder carrying all information necessary to make the call successfully. </param>
        /// <param name="timeout"> timeout for this call in milliseconds, overrides the instance level 
        /// timeout. Passing 0 here will use the global socket timeout. </param>
        /// <returns> Object[] array representing the results in the order expected or set in <code>JICallBuilder</code>. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.InvalidOperationException"> if there is no session associated 
        /// with this object or this object represents a local java reference. </exception>
        object[] call(JICallBuilder obj, int timeout);

        /// <summary>
        /// Sets a timeout for all socket level operations done on this
        /// object. Calling this overrides the global socket timeout at the 
        /// <code>JISession</code> level. To unset a previous timeout, pass 0 as a 
        /// parameter.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"> if there is no session associated 
        /// with this object or this object represents a local java reference. </exception>
        int InstanceLevelSocketTimeout { set; get; }

        /// <summary>
        /// Framework Internal
        /// returns self Interface pointer.
        /// </summary>
        JIInterfacePointer internal_getInterfacePointer();

        /// <summary>
        /// Returns session associated with this object.   
        /// </summary>
        /// <returns> JISession  </returns>
        JISession AssociatedSession { get; }

        /// <summary>
        /// Returns the COM <i>IID</i> of this object
        /// </summary>
        /// <returns> String representation of 128 bit uuid. </returns>
        string InterfaceIdentifier { get; }

        //	public JIComServer getAssociatedComServer();

        /// <summary>
        /// Returns <code>true</code> if <code>IDispatch</code> interface is supported 
        /// by this object.
        /// </summary>
        /// <returns> <code>true</code> if <code>IDispatch</code> is supported, <code>false</code>
        /// otherwise. </returns>
        /// <exception cref="System.InvalidOperationException"> if there is no session associated 
        /// with this object or this object represents a local java reference.
        /// </exception>
        /// <seealso cref="impls.automation.IJIDispatch"></seealso>
        bool DispatchSupported { get; }

        /// <summary>
        /// Adds a connection point information and it's cookie to the connectionPointMap internally.
        /// To be called only by the framework.
        /// </summary>
        /// <param name="connectionPoint"> </param>
        /// <param name="cookie"> </param>
        /// <returns> unique identifier for the combination. </returns>
        string internal_setConnectionInfo(IJIComObject connectionPoint, int? cookie);

        /// <summary>
        /// Framework Internal.
        /// Returns the ConnectionPoint (IJIComObject) and it's Cookie.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        object[] internal_getConnectionInfo(string identifier);

        /// <summary>
        /// Framework Internal.
        /// Returns and Removes the connection info from the internal map. 
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        object[] internal_removeConnectionInfo(string identifier);

        /// <summary>
        ///Adds a <code>IJIUnreferenced</code> handler. The handler will be invoked when this comObject goes 
        /// out of reference and is removed from it's session by the library. Only a single handler can be
        /// added for each object. If a handler for this object already exists , it would be replaced by this
        /// call.
        /// </summary>
        /// <param name="unreferenced"> handler to get notification when reference count for this object hits 0 and is
        /// garbage collected by the library's runtime. </param>
        /// <exception cref="System.InvalidOperationException"> if there is no session associated 
        /// with this object or this object represents a local java reference. </exception>
        void registerUnreferencedHandler(IJIUnreferenced unreferenced);

        /// <summary>
        /// Returns the <code>IJIUnreferenced</code> handler associated with this object.
        /// </summary>
        /// <returns> null if no handler is associated with this object. </returns>
        /// <exception cref="System.InvalidOperationException"> if there is no session associated 
        /// with this object or this object represents a local java reference. </exception>
        IJIUnreferenced UnreferencedHandler { get; }

        /// <summary>
        ///Removes the <code>IJIUnreferenced</code> handler associated with this object. No exception will
        /// be thrown if one does not exist for this object. </summary>
        /// <exception cref="System.InvalidOperationException"> if there is no session associated 
        /// with this object or this object represents a local java reference. </exception>
        void unregisterUnreferencedHandler();


        /// <summary>
        /// <i><u>Framework Internal</u></i> 
        /// 
        /// @exclude </summary>
        /// <param name="deffered"> </param>
        void internal_setDeffered(bool deffered);

        /// <summary>
        /// Returns <code>true</code> if this COM object represents a local Java reference obtained by 
        /// <seealso cref="impls.JIObjectFactory.buildObject(JISession, JILocalCoClass)"/>.
        /// </summary>
        /// <returns> <code>true</code> if this is a local reference, 
        /// <code>false</code> otherwise. </returns>
        bool LocalReference { get; }

        /// <summary>
        /// Will return the Object decoded by the CustomMarshallerUnMarshaller
        /// if one is present and this IJIComObject is of the type OBJREF_CUSTOM.
        /// </summary>
        JIComCustomMarshallerUnMarshaller CustomObject { get; }

        /// <summary>
        /// Length of interface pointer
        /// </summary>
        int LengthOfInterfacePointer { get; }
    }


    /// <summary>
    /// IID representing the <code>IUnknown</code>.
    /// </summary>
    public static class JiIUnknown {

        /// <summary>
        /// Iunknown
        /// </summary>
        public const string IID = "00000000-0000-0000-c000-000000000046";
    }
}