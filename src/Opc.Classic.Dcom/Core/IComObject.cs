// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Automation;

namespace Opc.Classic.Dcom.Core; 
/// <summary>
/// <para> Represents a Windows COM Object. Instances of this interface can
/// be retrieved by the following ways only :
/// <ul>
/// <li>During initial handshake as expressed in the sample below.</li>
/// <li>As references passed from <i>Windows COM runtime</i> such as when using
/// <seealso cref="QueryInterface"/> or returned as <code>[out]</code> parameters
/// to calls (directly as <code><see cref="IComObject"/></code>(s) or part of
/// <code><see cref="Variant"/></code>(s)). </li>
/// <li>From raw bytes using <seealso cref="ObjectFactory.BuildObject(Session, byte[])"/></li>
/// <li>As references to local Java-COM interfaces (which are then used for event handling).
/// See <seealso cref="ObjectFactory.BuildObject(Session, LocalCoClass)"/>
/// for more details.</li>
/// </ul>
/// All references obtained by any mechanism stated above <b>must</b> be <i>narrowed</i>
/// using <seealso cref="ObjectFactory.NarrowObject(IComObject)"/>
/// before being casted to the expected type.
/// </para>
/// <para>
/// Sample usage :
/// <code>
/// var session = <see cref="Session"/>.CreateSession("DOMAIN","USERNAME","PASSWORD");
/// var comserver = new <see cref="ComServer"/>(
///     <see cref="ProgId"/>.ValueOf("Word.Application"),address,session);
/// var comObject = comserver.CreateInstance();
/// </code>
/// Also,
/// <code>
/// var handle = comObject.queryInterface("620012E2-69E3-4DC0-B553-AE252524D2F6");
/// </code>
/// </para>
/// <b>Note</b>: Methods starting with <i>internal_</i> keyword are internal 
/// to the framework and must not be called by the developer.
/// </summary>
public interface IComObject {

    /// <summary>
    /// Unique 128 bit uuid representing the interface on the COM server.
    /// This value can and should be used to map an <see cref="IUnreferenced"/>
    /// handler implementation to this COM Object.
    /// </summary>
    /// <remarks>Under <b>NO</b> circumstances should a reference to this COM
    /// object be stored any where for only purposes of "unreferenced" handling. 
    /// This would hinder the way in which objects are garbage collected by 
    /// the framework and this object would be forever "live".
    /// </remarks>
    /// <returns> string representation of ipid. </returns>
    string Ipid { get; }

    /// <summary>
    /// Returns session associated with this object.
    /// </summary>
    /// <returns> <see cref="Session"/>  </returns>
    Session AssociatedSession { get; }

    /// <summary>
    /// Returns the COM <i>IID</i> of this object
    /// </summary>
    /// <returns> String representation of 128 bit uuid. </returns>
    string InterfaceIdentifier { get; }

    /// <summary>
    /// Returns <code>true</code> if <code>IDispatch</code> interface
    /// is supported by this object.
    /// </summary>
    /// <returns> <code>true</code> if <code>IDispatch</code> is supported,
    /// <code>false</code> otherwise. </returns>
    /// <exception cref="System.InvalidOperationException"> if there is
    /// no session associated with this object or this object represents a 
    /// local reference.
    /// </exception>
    /// <seealso cref="IDispatch"></seealso>
    bool DispatchSupported { get; }

    /// <summary>
    /// Returns the <see cref="IUnreferenced"/> handler associated with 
    /// this object.
    /// </summary>
    /// <returns> null if no handler is associated with this object. 
    /// </returns>
    /// <exception cref="System.InvalidOperationException"> if there 
    /// is no session associated with this object or this object 
    /// represents a local reference. </exception>
    IUnreferenced UnreferencedHandler { get; }

    /// <summary>
    /// Returns <code>true</code> if this COM object represents a local 
    /// reference obtained by
    /// <seealso cref="ObjectFactory.BuildObject(Session, LocalCoClass)"/>.
    /// </summary>
    /// <returns> <code>true</code> if this is a local reference,
    /// <code>false</code> otherwise. </returns>
    bool LocalReference { get; }

    /// <summary>
    /// Will return the Object decoded by the CustomMarshallerUnMarshaller
    /// if one is present and this <see cref="IComObject"/> is of the type
    /// OBJREF_CUSTOM.
    /// </summary>
    ComCustomMarshallerUnMarshaller CustomObject { get; }

    /// <summary>
    /// Length of interface pointer
    /// </summary>
    int LengthOfInterfacePointer { get; }

    /// <summary>
    /// Sets a timeout for all socket level operations done on this
    /// object. Calling this overrides the global socket timeout at the
    /// <code><see cref="Session"/></code> level. To unset a previous
    /// timeout, pass 0 as a
    /// parameter.
    /// </summary>
    /// <exception cref="System.InvalidOperationException"> if there
    /// is no session associated with this object or this object represents
    /// a local reference. </exception>
    int InstanceLevelSocketTimeout { set; get; }

    /// <summary>
    ///<para>Retrieve interface references based on <code>iid</code>. Make sure to
    /// narrow before casting to the expected type.
    /// </para>
    /// <para>
    /// For example when expecting an <code><see cref="IEnumVariant"/></code> :
    /// </para>
    /// <para>
    /// <code>
    /// var object2 = variant.GetObjectAsComObject();
    /// var enumVariant = (<see cref="IEnumVariant"/>)<see cref="ObjectFactory"/>.NarrowObject(
    ///     object2.QueryInterface(<see cref="Interfaces.IID_IEnumVARIANT"/>));
    /// </code>
    /// </para>
    /// <seealso cref="ObjectFactory.NarrowObject(IComObject)"></seealso>
    /// </summary>
    /// <param name="iid"> string representation of the IID. </param>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.InvalidOperationException"> if there is no session associated
    /// with this object or this object represents a local reference. </exception>
    /// <returns> reference to the requested unknown. </returns>
    IComObject QueryInterface(string iid);

    /// <summary>
    /// <P>Increases the reference count on the COM server by 5
    /// (currently hard coded). The developer should refrain from calling this API,
    /// as referencing is maintained internally by the system though he is not
    /// obligated to do so. If the <seealso cref="Release()"/> is not called in conjunction
    /// with <code>addRef</code> then the COM Instance will not get garbage collected
    /// at the server.
    /// </P> </summary>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.InvalidOperationException"> if there is no 
    /// session associated with this object or this object represents a local 
    /// reference. </exception>
    void AddRef();

    /// <summary>
    /// Decreases the reference count on the COM server by 5
    /// (currently hard coded). The developer should refrain from calling this API,
    /// as referencing is maintained internally by the system though he is not
    /// obligated to do so. If the <code>release</code> is not called in conjunction
    /// with <seealso cref="AddRef()"/> then the COM Instance will not get garbage
    /// collected at the server.
    /// </summary>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.InvalidOperationException"> if there is no session 
    /// associated with this object or this object represents a local reference. 
    /// </exception>
    void Release();

    /// <summary>
    /// Executes a method call on the actual COM object represented by this interface.
    /// All the data like parameter information, operation number etc. are prepared and
    /// sent via the <code>CallBuilder</code>.
    /// </summary>
    /// <para>
    /// <code>
    /// var obj = new CallBuilder();
    /// obj.ReInit();
    /// obj.OpNum = 0; // methods are sequentially indexed from 0 in the IDL
    /// obj.AddInParamAsString(new <see cref="ComString"/>("Go Mariners!",
    ///     InteropFlags.FLAG_REPRESENTATION_STRING_LPCTSTR));
    /// obj.AddInParamAsPointer(new <see cref="ComPointer"/>(
    ///     new <see cref="ComString"/>("Pretty simple ;)",
    ///     InteropFlags.FLAG_REPRESENTATION_STRING_LPCTSTR)));
    /// object[] result = comObject.call(obj);
    /// </code>
    /// If return values are expected then set up the <i>Out Params</i> also in the
    /// <code>CallBuilder</code>.
    /// The call timeout used here, by default is the instance level timeout. If no
    /// instance level timeout has been specified(or is 0) then the global timeout 
    /// set in <seealso cref="Session"/> will be used.
    /// </para>
    /// <param name="obj"> call builder carrying all information necessary to make
    /// the call successfully. </param>
    /// <returns> Object[] array representing the results in the order expected or
    /// set in <code>CallBuilder</code>. </returns>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.InvalidOperationException"> if there is no 
    /// session associated
    /// with this object or this object represents a local reference. </exception>
    object[] Call(CallBuilder obj);

    /// <summary>
    ///<P> Refer <seealso cref="Call(CallBuilder)"/> for details on this method.
    /// </P> </summary>
    /// <param name="obj"> call builder carrying all information necessary to make
    /// the call successfully. </param>
    /// <param name="timeout"> timeout for this call in milliseconds, overrides 
    /// the instance level timeout. Passing 0 here will use the global socket
    /// timeout. </param>
    /// <returns> Object[] array representing the results in the order expected
    /// or set in <code>CallBuilder</code>. </returns>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.InvalidOperationException"> if there is no 
    /// session associated with this object or this object represents a local 
    /// reference. </exception>
    object[] Call(CallBuilder obj, int timeout);

    /// <summary>
    /// Adds a <code>IUnreferenced</code> handler. The handler will be invoked 
    /// when this comObject goes out of reference and is removed from it's session
    /// by the library. Only a single handler can be added for each object. If 
    /// a handler for this object already exists, it would be replaced by this
    /// call.
    /// </summary>
    /// <param name="unreferenced"> handler to get notification when reference 
    /// count for this object hits 0 and is garbage collected by the library's 
    /// runtime. </param>
    /// <exception cref="System.InvalidOperationException"> if there is no session 
    /// associated with this object or this object represents a local reference. 
    /// </exception>
    void RegisterUnreferencedHandler(IUnreferenced unreferenced);

    /// <summary>
    /// Removes the <see cref="IUnreferenced"/> handler associated with this 
    /// object. No exception will be thrown if one does not exist for this object.
    /// </summary>
    /// <exception cref="System.InvalidOperationException"> if there is no session 
    /// associated with this object or this object represents a local reference. 
    /// </exception>
    void UnregisterUnreferencedHandler();
}
