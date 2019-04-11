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

    using IJIUnreferenced = org.jinterop.dcom.common.IJIUnreferenced;
    using JIException = org.jinterop.dcom.common.JIException;





    /// <summary>
    ///<para> Represents a Windows COM Object. Instances of this interface can 
    /// be retrieved by the following ways only :-
    /// <ul>
    /// <li>During initial handshake as expressed in the sample below.
    /// <li>As references passed from <i>Windows COM runtime</i> such as when using 
    /// <seealso cref="#queryInterface(String)"/> or returned as <code>[out]</code> parameters 
    /// to calls (directly as <code>IJIComObject</code>(s) or part of <code>JIVariant</code>
    /// (s)). </li>
    /// <li>From raw bytes using <seealso cref="org.jinterop.dcom.impls.JIObjectFactory#buildObject(JISession, byte[])"/></li>
    /// <li>As references to local Java-COM interfaces (which are then used for 
    /// event handling). See <seealso cref="org.jinterop.dcom.impls.JIObjectFactory#buildObject(JISession, JILocalCoClass)"/>
    /// for more details.</li>
    /// </ul>
    /// <br>
    /// All references obtained by any mechanism stated above <b>must</b> be <i>narrowed</i> 
    /// using <seealso cref="org.jinterop.dcom.impls.JIObjectFactory#narrowObject(IJIComObject)"/>
    /// before being casted to the expected type.
    /// <br>
    /// </para>
    /// <para>
    /// Sample usage :-
    /// <br>
    /// <code>
    /// JISession session = JISession.createSession("DOMAIN","USERNAME","PASSWORD");
    /// <br>
    /// JIComServer comserver = new JIComServer(JIProgId.valueOf("Word.Application"),address,session);
    /// <br>
    /// IJIComObject comObject = comserver.createInstance();
    /// <br>
    /// </code>
    /// <br>
    /// Also , 
    /// <code>
    /// <br>
    /// IJIComObject handle = comObject.queryInterface("620012E2-69E3-4DC0-B553-AE252524D2F6");
    /// </code>
    /// </para>
    /// 
    /// <b>Note</b>: Methods starting with <i>internal_</i> keyword are internal to the framework 
    /// and must not be called by the developer.
    /// 
    /// @since 1.0 
    /// 
    /// </summary>
    //All IIDs Interfaces will be extending this interface
    public interface IJIComObject {

        /// <summary>
        /// IID representing the <code>IUnknown</code>.
        /// </summary>

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
        /// <br>
        /// IJIEnumVariant enumVariant = (IJIEnumVariant)JIObjectFactory.narrowObject(object2.queryInterface(IJIEnumVariant.IID));
        /// </code>
        /// </para>
        /// <para> 
        /// Throws IllegalStateException if <seealso cref="#isLocalReference()"/> returns <code>true</code>.
        /// 
        /// </para>
        /// </summary>
        /// <param name="iid"> string representation of the IID. </param>
        /// <returns> reference to the requested unknown. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalStateException"> if there is no session associated 
        /// with this object or this object represents a local java reference. </exception>
        /// <seealso cref=  org.jinterop.dcom.impls.JIObjectFactory#narrowObject(IJIComObject) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJIComObject queryInterface(String iid) throws org.jinterop.dcom.common.JIException;
        IJIComObject QueryInterface(string iid);

        /// <summary>
        /// <P>Increases the reference count on the COM server by 5
        /// (currently hard coded). The developer should refrain from calling this API, 
        /// as referencing is maintained internally by the system though he is not 
        /// obligated to do so. If the <seealso cref="#release()"/> is not called in conjunction 
        /// with <code>addRef</code> then the COM Instance will not get garbage collected 
        /// at the server. 
        /// </P> </summary>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalStateException"> if there is no session associated 
        /// with this object or this object represents a local java reference.
        ///  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addRef() throws org.jinterop.dcom.common.JIException;
        void AddRef();

        /// <summary>
        ///<P> Decreases the reference count on the COM server by 5 
        /// (currently hard coded). The developer should refrain from calling this API, 
        /// as referencing is maintained internally by the system though he is not
        /// obligated to do so. If the <code>release</code> is not called in conjunction 
        /// with <seealso cref="#addRef()"/> then the COM Instance will not get garbage collected at 
        /// the server.
        /// </P> </summary>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalStateException"> if there is no session associated 
        /// with this object or this object represents a local java reference.  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void release() throws org.jinterop.dcom.common.JIException;
        void Release();

        /// <summary>
        ///Unique 128 bit uuid representing the interface on the COM server. This value can
        /// and should be used to map an <code>IJIUnreferenced</code> handler implementation to this COM Object.
        /// <para>Under <b>NO</b> circumstances should a reference to this COM object be stored any where
        /// for only purposes of "unreferenced" handling. This would hinder the way in which objects
        /// are garbage collected by the framework and this object would be forever "live".
        /// 
        /// </para>
        /// </summary>
        /// <returns> string representation of ipid. </returns>
        string Ipid { get; }

        /// <summary>
        /// <P>Executes a method call on the actual COM object represented by this interface. 
        /// All the data like parameter information, operation number etc. are prepared and 
        /// sent via the <code>JICallBuilder</code>.
        /// <para>
        /// <code>
        ///  JICallBuilder obj = new JICallBuilder(); <br>
        ///  obj.reInit(); <br>
        /// obj.setOpnum(0); //methods are sequentially indexed from 0 in the IDL
        /// <br>
        /// obj.addInParamAsString(new JIString("j-Interop Rocks",JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR), JIFlags.FLAG_NULL); <br>
        /// obj.addInParamAsPointer(new JIPointer(new JIString("Pretty simple ;)",JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR)), JIFlags.FLAG_NULL); <br>
        /// <br>
        /// Object[] result = comObject.call(obj);
        /// <br>
        /// </code>
        /// </para>
        /// <para>
        /// If return values are expected then set up the <i>Out Params</i> also in the 
        /// <code>JICallBuilder</code>. 
        /// </para>
        /// <para>
        /// 
        /// The call timeout used here , by default is the instance level timeout. If no 
        /// instance level timeout has been specified(or is 0) then the global timeout set in 
        /// <seealso cref="org.jinterop.dcom.core.JISession"/> will be used. 
        /// 
        /// </P>
        /// </para>
        /// </summary>
        /// <param name="obj"> call builder carrying all information necessary to make the call successfully. </param>
        /// <returns> Object[] array representing the results in the order expected or set in <code>JICallBuilder</code>. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalStateException"> if there is no session associated 
        /// with this object or this object represents a local java reference. </exception>
        /// <seealso cref= #setInstanceLevelSocketTimeout(int) </seealso>
        /// <seealso cref= org.jinterop.dcom.core.JISession#setGlobalSocketTimeout(int) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] call(JICallBuilder obj) throws org.jinterop.dcom.common.JIException;
        object[] Call(JICallBuilder obj);

        /// <summary>
        ///<P> Refer <seealso cref="#call(JICallBuilder)"/> for details on this method.
        /// </P> </summary>
        /// <param name="obj"> call builder carrying all information necessary to make the call successfully. </param>
        /// <param name="timeout"> timeout for this call in milliseconds, overrides the instance level 
        /// timeout. Passing 0 here will use the global socket timeout. </param>
        /// <returns> Object[] array representing the results in the order expected or set in <code>JICallBuilder</code>. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalStateException"> if there is no session associated 
        /// with this object or this object represents a local java reference. </exception>
        /// <seealso cref= org.jinterop.dcom.core.JISession#setGlobalSocketTimeout(int) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] call(JICallBuilder obj, int timeout) throws org.jinterop.dcom.common.JIException;
        object[] Call(JICallBuilder obj, int timeout);

        /// <summary>
        ///<para>Sets a timeout for all socket level operations done on this
        /// object. Calling this overrides the global socket timeout at the 
        /// <code>JISession</code> level. To unset a previous timeout, pass 0 as a 
        /// parameter.
        /// 
        /// </para>
        /// </summary>
        /// <param name="timeout"> timeout for this call in milliseconds </param>
        /// <exception cref="IllegalStateException"> if there is no session associated 
        /// with this object or this object represents a local java reference. </exception>
        /// <seealso cref= org.jinterop.dcom.core.JISession#setGlobalSocketTimeout(int) </seealso>
        int InstanceLevelSocketTimeout { set;get; }


        /// <summary>
        ///<i><u>Framework Internal</u></i>
        /// Returns self Interface pointer.
        /// </summary>
        JIInterfacePointer Internal_getInterfacePointer();

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

    //    /**
    //     * @exclude
    //     * @return
    //     */
    //    public JIComServer getAssociatedComServer();

        /// <summary>
        ///Returns <code>true</code> if <code>IDispatch</code> interface is supported 
        /// by this object.
        /// </summary>
        /// <returns> <code>true</code> if <code>IDispatch</code> is supported, <code>false</code>
        /// otherwise. </returns>
        /// <exception cref="IllegalStateException"> if there is no session associated 
        /// with this object or this object represents a local java reference.
        /// </exception>
        /// <seealso cref= org.jinterop.dcom.impls.automation.IJIDispatch </seealso>
        bool DispatchSupported { get; }

        /// <summary>
        ///Adds a connection point information and it's cookie to the connectionPointMap internally.
        /// To be called only by the framework.
        /// 
        /// @exclude </summary>
        /// <param name="connectionPoint"> </param>
        /// <param name="cookie"> </param>
        /// <returns> unique identifier for the combination. </returns>
        string Internal_setConnectionInfo(IJIComObject connectionPoint, int? cookie);

        /// <summary>
        ///<i><u>Framework Internal</u></i> Returns the ConnectionPoint (IJIComObject) and it's Cookie.
        /// 
        /// @exclude </summary>
        /// <param name="identifier">
        /// @return </param>
        object[] Internal_getConnectionInfo(string identifier);

        /// <summary>
        ///<i><u>Framework Internal</u></i> Returns and Removes the connection info from the internal map. 
        /// 
        /// @exclude </summary>
        /// <param name="identifier">
        /// @return </param>
        object[] Internal_removeConnectionInfo(string identifier);

        /// <summary>
        ///Adds a <code>IJIUnreferenced</code> handler. The handler will be invoked when this comObject goes 
        /// out of reference and is removed from it's session by the library. Only a single handler can be
        /// added for each object. If a handler for this object already exists , it would be replaced by this
        /// call.
        /// </summary>
        /// <param name="unreferenced"> handler to get notification when reference count for this object hits 0 and is
        /// garbage collected by the library's runtime. </param>
        /// <exception cref="IllegalStateException"> if there is no session associated 
        /// with this object or this object represents a local java reference. </exception>
        void RegisterUnreferencedHandler(IJIUnreferenced unreferenced);

        /// <summary>
        /// Returns the <code>IJIUnreferenced</code> handler associated with this object.
        /// </summary>
        /// <returns> null if no handler is associated with this object. </returns>
        /// <exception cref="IllegalStateException"> if there is no session associated 
        /// with this object or this object represents a local java reference. </exception>
        IJIUnreferenced UnreferencedHandler { get; }

        /// <summary>
        ///Removes the <code>IJIUnreferenced</code> handler associated with this object. No exception will
        /// be thrown if one does not exist for this object. </summary>
        /// <exception cref="IllegalStateException"> if there is no session associated 
        /// with this object or this object represents a local java reference. </exception>
        void UnregisterUnreferencedHandler();


        /// <summary>
        /// <i><u>Framework Internal</u></i> 
        /// 
        /// @exclude </summary>
        /// <param name="deffered"> </param>
        void Internal_setDeffered(bool deffered);

        /// <summary>
        /// Returns <code>true</code> if this COM object represents a local Java reference obtained by 
        /// <seealso cref="org.jinterop.dcom.impls.JIObjectFactory#buildObject(JISession, JILocalCoClass)"/>.
        /// <para>
        /// 
        /// </para>
        /// </summary>
        /// <returns> <code>true</code> if this is a local reference , <code>false</code> otherwise. </returns>
        bool LocalReference { get; }

        /// <summary>
        /// Will return the Object decoded by the CustomMarshallerUnMarshaller if one is present and this
        /// IJIComObject is of the type OBJREF_CUSTOM.
        /// 
        /// @return
        /// </summary>
        JIComCustomMarshallerUnMarshaller CustomObject { get; }

        int LengthOfInterfacePointer { get; }
    }

    public static class IJIComObject_Fields {
        public const string IID = "00000000-0000-0000-c000-000000000046";
    }

}