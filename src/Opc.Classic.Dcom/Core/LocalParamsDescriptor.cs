//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using Opc.Classic.Dcom.Internal.LegacyNdr;
using System;

namespace SharpInterop.Core; 
/// <summary>
/// Provides a way to express parameters for a particular method.
/// These are only <code>[in]</code> parameters, the <code>[out]</code>
/// parameters are decided at the implementation level. If the <code>IDL</code>
/// method being described by this class is returning multiple
/// objects then use the return type of the implementation
/// as an <code>Object[]</code>
/// For example:
/// IDL from Microsoft Internet Explorer is:
/// <code>
/// [id(0x000000fb),
/// helpstring("A new, hidden, non-navigated WebBrowser window is needed.")]
///   void NewWindow2([in, out] IDispatch** ppDisp,
///                   [in, out] VARIANT_BOOL* Cancel);
/// </code>
/// Corresponding <see cref="LocalParamsDescriptor"/> would be :
/// <code>
/// var paramObject = new <see cref="LocalParamsDescriptor"/>();
/// paramObject.AddInParamAsObject(
///     new <see cref="ComPointer"/>(typeof(<see cref="IComObject"/>),false));
/// paramObject.AddInParamAsType(typeof(<see cref="Variant"/>));
/// </code>
/// and the local implementation must return an <code>object[]</code>
/// in this case, for returning the 2 parameters back.
/// Please refer to MSInternetExplorer, Test_ITestServer2_Impl, SampleTestServer
/// and MSShell examples for more details on how to use this class.
/// </summary>
[Serializable]
public sealed class LocalParamsDescriptor {

    /// <summary>
    /// Parameters
    /// </summary>
    internal object[] InParams => _callObject.OutParams;

    /// <summary>
    /// Set current session
    /// </summary>
    /// <param name="value"></param>
    internal void SetSession(Session value) =>
        _callObject.AttachSession(value);

    /// <summary>
    /// Read
    /// </summary>
    /// <param name="ndr"></param>
    /// <returns></returns>
    internal object[] Read(NdrCodec ndr) {
        _callObject.Read2(ndr);
        return _callObject.Results;
    }

    /// <summary>
    /// Add <code>[in]</code> parameter of the type
    /// <code>clazz</code> at the end of the out parameter list.
    /// </summary>
    /// <param name="type"> </param>
    /// <param name="flags"> </param>
    public void AddInParamAsType(Type type, int flags = InteropFlags.FLAG_NULL) =>
        _callObject.AddOutParamAsType(type, flags);

    /// <summary>
    /// Add <code>[in]</code> parameter at the end of the
    /// out parameter list. Typically callers are
    /// composite in nature <code><see cref="Struct"/></code>,
    /// <code><see cref="Union"/>s</code>, <code><see cref="ComPointer"/></code>
    /// and <code><see cref="ComString"/></code> .
    /// </summary>
    /// <param name="inparam"> </param>
    /// <param name="flags"> </param>
    public void AddInParamAsObject(object inparam, int flags = InteropFlags.FLAG_NULL) =>
        _callObject.AddOutParamAsObject(inparam, flags);

    /// <summary>
    /// set params
    /// </summary>
    /// <param name="inparams"> </param>
    /// <param name="flags"> </param>
    internal void SetInParams(object[] inparams, int flags = InteropFlags.FLAG_NULL) =>
        _callObject.SetOutParams(inparams, flags);

    /// <summary>
    /// Removes <code>[in]</code> parameter at the specified index
    /// from the parameter list.
    /// </summary>
    /// <param name="index"> 0 based index </param>
    /// <param name="flags"> from <see cref="InteropFlags"/> (if need be). 
    /// </param>
    public void RemoveInParamAt(int index, int flags = InteropFlags.FLAG_NULL) =>
        _callObject.RemoveOutParamAt(index, flags);

    private readonly CallBuilder _callObject = new CallBuilder();
}
