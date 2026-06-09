// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Core;

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Represents the Windows COM <code>IDispatch</code> Interface.
///
/// Sample Usage :
/// <code>
/// // Assume comServer is the reference to <see cref="ComServer"/>, obtained earlier...
/// <see cref="IComObject"/> comObject = comServer.createInstance();
/// // This call will result into a <i>QueryInterface</i> for the IDispatch
/// <see cref="IDispatch"/> dispatch =
///    (<see cref="IDispatch"/>)<see cref="ObjectFactory"/>.NarrowObject(
///    comObject.QueryInterface(<see cref="IDispatch"/>.IID));
/// </code>
///
/// Another example :
/// <code>
/// int dispId = dispatch.getIDsOfNames("Workbooks");
/// <see cref="Variant"/> outVal = dispatch.get(dispId);
/// <see cref="IDispatch"/> dispatchOfWorkBooks =
///    (<see cref="IDispatch"/>)<see cref="ObjectFactory"/>.NarrowObject(outVal.GetObjectAsComObject());
/// <see cref="Variant"/>[] outVal2 =
///    dispatchOfWorkBooks.CallMethodA("Add",new object[]{<see cref="Variant"/>.OPTIONAL_PARAM()});
/// dispatchOfWorkBook =
///    (<see cref="IDispatch"/>)<see cref="ObjectFactory"/>.NarrowObject(outVal2[0].GetObjectAsComObject());
/// outVal = dispatchOfWorkBook.get("Worksheets");
/// dispatchOfWorkSheets =
///    (<see cref="IDispatch"/>)<see cref="ObjectFactory"/>.NarrowObject(outVal.GetObjectAsComObject());
/// </code>
///
/// Please note that all <code>[in]</code> parameters are converted to
/// <code><seealso cref="Variant"/></code>
/// before being sent to the COM server through the <code><see cref="IDispatch"/></code>
/// interface. If any <code>[in]</code> parameter is already a
/// <code><see cref="Variant"/></code>, it is left as it is.
/// for example:
/// <code>
/// // From MSADO example.
/// dispatch = (<see cref="IDispatch"/>)<see cref="ObjectFactory"/>.NarrowObject(
///    comObject.queryInterface(<see cref="IDispatch"/>.IID));
/// dispatch.callMethod("Open", new object[]{
///    new <see cref="ComString"/>("driver=Microsoft Access Driver (*.mdb);dbq=C:\\temp\\products.mdb"),
///    <see cref="Variant"/>.OPTIONAL_PARAM,<see cref="Variant"/>.OPTIONAL_PARAM,new Integer(-1)
/// });
/// <see cref="Variant"/> variant[] = dispatch.CallMethodA("Execute",new Object[]{
///    new <see cref="ComString"/>("SELECT * FROM Products"), new Integer(-1)
/// });
/// if (variant[0].isNull()) {
///        Console.WriteLine("Recordset is empty.");
/// }
/// else {
///       // Do something...
/// }
/// </code>
/// Where ever the corresponding COM interface API requires an <code>[optional]</code> 
/// parameter, the developer can use <code><see cref="Variant"/>.OPTIONAL_PARAM()</code>,
/// like in the example above.
/// </summary>
public interface IDispatch : IComObject {

    /// <summary>
    /// Returns the COM <code>EXCEPINFO</code> structure wrapped as a
    /// data object for the <b>last</b> operation. Note this will only be
    /// valid if a <seealso cref="InteropException"/> has been raised
    /// in the last call.
    /// </summary>
    ExcepInfo LastExcepInfo { get; }

    /// <summary>
    /// Determines whether there is type information available for the dual interface.
    /// </summary>
    /// <returns> 1 if the object provides type information, otherwise 0. </returns>
    /// <exception cref="InteropException"> </exception>
    int TypeInfoCount { get; }

    /// <summary>
    /// Maps a method name to its corresponding <code>DISPID</code>.
    /// The result of this call is cached
    /// for further usage and no network call is performed again for
    /// the same method name.
    /// </summary>
    /// <param name="apiName"> Method name. </param>
    /// <returns> <code>DISPID</code> of the method. </returns>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.ArgumentException"> if the <code>apiName</code>
    /// is <code>null</code> or empty. </exception>
    int GetIDsOfNames(string apiName);

    /// <summary>
    /// Maps a single method name and an optional set of it's argument
    /// names to a corresponding set of <code>DISPIDs</code>.
    /// The result of this call is cached for further usage and no network
    /// call is performed again for the same method[argument] set.
    /// </summary>
    /// <param name="apiName"> String[] with first index depicting method
    /// name and the rest depicting parameters. </param>
    /// <returns> int[] <code>DISPIDs</code> in the same order as the
    /// method[argument] set. </returns>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.ArgumentException"> if the <code>apiName</code>
    /// is <code>null</code> or empty. </exception>
    int[] GetIDsOfNames(string[] apiName);

    /// <summary>
    /// Returns an implementation of COM <code>ITypeInfo</code> interface
    /// based on the <code>typeInfo</code>.
    /// </summary>
    /// <param name="typeInfo"> the type information to return. Pass 0 to
    /// retrieve type information for the <code>IDispatch</code> implementation.
    /// </param>
    /// <exception cref="InteropException"> </exception>
    ITypeInfo GetTypeInfo(int typeInfo);

    /// <summary>
    /// Performs a <code>propput</code> for the method identified by the
    /// <code>dispId</code>.
    /// </summary>
    /// <param name="dispId"> <code>DISPID</code> of the method to invoke.
    /// </param>
    /// <param name="inparam"> parameter for that method. </param>
    /// <exception cref="InteropException"> </exception>
    void Put(int dispId, Variant inparam);

    /// <summary>
    /// Performs a <code>propput</code> for the method identified by the
    /// <code>name</code> parameter.
    /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/>
    /// and then delegates the call to <seealso cref="Put(int, Variant)"/>.
    /// </summary>
    /// <param name="name"> name of the method to invoke. </param>
    /// <param name="inparam"> parameter for that method. </param>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.ArgumentException"> if the <code>name</code>
    /// is <code>null</code> or empty. </exception>
    void Put(string name, Variant inparam);

    /// <summary>
    /// Performs a <code>propputref</code> for the method identified by the
    /// <code>dispId</code>.
    /// </summary>
    /// <param name="dispId"> <code>DISPID</code> of the method to invoke.
    /// </param>
    /// <param name="inparam"> parameter for that method. </param>
    /// <exception cref="InteropException"> </exception>
    void PutRef(int dispId, Variant inparam);

    /// <summary>
    /// Performs a <code>propput</code> for the method identified by the
    /// <code>name</code> parameter.
    /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/>
    /// and then delegates the call to <seealso cref="PutRef(int, Variant)"/>.
    /// </summary>
    /// <param name="name"> name of the method to invoke. </param>
    /// <param name="inparam"> parameter for that method. </param>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.ArgumentException"> if the <code>name</code> is
    /// <code>null</code> or empty. </exception>
    void PutRef(string name, Variant inparam);

    /// <summary>
    /// Performs a <code>propget</code> for the method identified by the
    /// <code>dispId</code>.
    /// </summary>
    /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
    /// <returns> <see cref="Variant"/> result of the call </returns>
    /// <exception cref="InteropException"> </exception>
    Variant Get(int dispId);

    /// <summary>
    /// Performs a <code>propget</code> for the method identified by the
    /// <code>dispId</code> parameter.
    /// <code>inparams</code> defines the parameters for the <code>get</code>
    /// operation.
    /// </summary>
    /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
    /// <param name="inparams"> members of this array are implicitly converted
    /// to <code><see cref="Variant"/></code>s before performing the
    /// actual call to the COM server, via the <code><see cref="IDispatch"/></code> interface.
    /// </param>
    /// <returns> array of <see cref="Variant"/>s </returns>
    /// <exception cref="InteropException"> </exception>
    Variant[] Get(int dispId, params object[] inparams);

    /// <summary>
    /// Performs a <code>propget</code> for the method identified by the
    /// <code>name</code> parameter.
    /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/>
    /// and then delegates the call to <seealso cref="Get(int, object[])"/>.
    /// </summary>
    /// <param name="name"> name of the method to invoke. </param>
    /// <param name="inparams"> members of this array are implicitly converted
    /// to <code><see cref="Variant"/></code>s
    /// before performing the actual call to the COM server, via the
    /// <code><see cref="IDispatch"/></code> interface. </param>
    /// <returns> array of <see cref="Variant"/>s </returns>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.ArgumentException"> if the <code>name</code> is
    /// <code>null</code> or empty. </exception>
    Variant[] Get(string name, params object[] inparams);

    /// <summary>
    /// Performs a <code>propget</code> for the method identified by the
    /// <code>name</code> parameter.
    /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/>
    /// and then delegates the call to <seealso cref="Get(int)"/>
    /// </summary>
    /// <param name="name"> name of the method to invoke. </param>
    /// <returns> <see cref="Variant"/> result of the call. </returns>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.ArgumentException"> if the <code>name</code> is
    /// <code>null</code> or empty. </exception>
    Variant Get(string name);

    /// <summary>
    /// Performs a <code>method</code> call for the method identified by the
    /// <code>name</code> parameter.
    /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/>
    /// and then delegates the call to <seealso cref="CallMethod(int)"/>.
    /// </summary>
    /// <param name="name"> name of the method to invoke. </param>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.ArgumentException"> if the <code>name</code> is
    /// <code>null</code> or empty. </exception>
    void CallMethod(string name);

    /// <summary>
    /// Performs a <code>method</code> call for the method identified by the
    /// <code>dispId</code> parameter.
    /// </summary>
    /// <param name="dispId"> <code>DISPID</code> of the method to invoke.</param>
    /// <exception cref="InteropException"> </exception>
    void CallMethod(int dispId);

    /// <summary>
    /// Performs a <code>method</code> call for the method identified by the
    /// <code>name</code> parameter.
    /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/> and
    /// then delegates the call to <seealso cref="CallMethodA(int)"/>.
    /// </summary>
    /// <param name="name"> name of the method to invoke. </param>
    /// <returns> <see cref="Variant"/> result. </returns>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.ArgumentException"> if the <code>name</code>
    /// is <code>null</code> or empty. </exception>
    Variant CallMethodA(string name);

    /// <summary>
    /// Performs a <code>method</code> call for the method identified by the
    /// <code>dispId</code> parameter.
    /// </summary>
    /// <param name="dispId"> <code>DISPID</code> of the method to invoke.
    /// </param>
    /// <returns> <see cref="Variant"/> result. </returns>
    /// <exception cref="InteropException"> </exception>
    Variant CallMethodA(int dispId);

    /// <summary>
    /// Performs a <code>method</code> call for the method identified
    /// by the <code>name</code> parameter.
    /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/>
    /// and then delegates the call to <seealso cref="CallMethod(int, object[])"/>.
    /// For the <code>inparams</code> array, sequential <code>DISPID</code>s
    /// (zero based index) will be used. For <code>inparam[0]</code>,
    /// <code>DISPID</code> will be <code>0</code>,
    /// for <code>inparam[1]</code> it will be <code>1</code> and so on.
    /// sequential dispIds for params are used 0,1,2,3...
    /// </summary>
    /// <param name="name"> name of the method to invoke. </param>
    /// <param name="inparams"> members of this array are implicitly
    /// converted to <code><see cref="Variant"/></code>s before performing the
    /// actual call to the COM server, via the <code><see cref="IDispatch"/></code>
    /// interface. </param>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.ArgumentException"> if the <code>name</code>
    /// is <code>null</code> or empty. </exception>
    //
    void CallMethod(string name, params object[] inparams);

    /// <summary>
    /// Performs a <code>method</code> call for the method identified by
    /// the <code>dispId</code> parameter.
    /// For the <code>inparams</code> array, sequential <code>DISPID</code>s
    /// (zero based index) will be used.
    /// For <code>inparam[0]</code>, <code>DISPID</code> will be
    /// <code>0</code>, for <code>inparam[1]</code>
    /// it will be <code>1</code> and so on.
    /// </summary>
    /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
    /// <param name="inparams"> members of this array are implicitly converted to
    /// <code><see cref="Variant"/></code>s before performing the
    /// actual call to the COM server, via the <code><see cref="IDispatch"/></code> interface.
    /// </param>
    /// <exception cref="InteropException"> </exception>
    void CallMethod(int dispId, params object[] inparams);

    /// <summary>
    /// Performs a <code>method</code> call for the method identified by the
    /// <code>name</code> parameter.
    /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/>
    /// and then delegates the call to <seealso cref="CallMethodA(int, object[])"/>.
    /// For the <code>inparams</code> array, sequential <code>DISPID</code>s
    /// (zero based index) will be used. For <code>inparam[0]</code>,
    /// <code>DISPID</code> will be <code>0</code>,
    /// for <code>inparam[1]</code> it will be <code>1</code> and so on.
    /// sequential dispIds for params are used 0,1,2,3...
    /// </summary>
    /// <param name="name"> name of the method to invoke. </param>
    /// <param name="inparams"> members of this array are implicitly converted to
    /// <code><see cref="Variant"/></code>s before performing the
    /// actual call to the COM server, via the <code><see cref="IDispatch"/></code> interface.
    /// </param>
    /// <returns> <see cref="Variant"/>[] result. </returns>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.ArgumentException"> if the <code>name</code> is
    /// <code>null</code> or empty. </exception>
    Variant[] CallMethodA(string name, params object[] inparams);

    /// <summary>
    /// Performs a <code>method</code> call for the method identified by the
    /// <code>dispId</code> parameter.
    /// For the <code>inparams</code> array, sequential <code>DISPID</code>s
    /// (zero based index) will be used.
    /// For <code>inparam[0]</code>, <code>DISPID</code> will be <code>0</code>,
    /// for <code>inparam[1]</code>
    /// it will be <code>1</code> and so on.
    /// sequential dispIds for params are used 0,1,2,3...
    /// </summary>
    /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
    /// <param name="inparams"> members of this array are implicitly converted to
    /// <code><see cref="Variant"/></code>s before performing the
    /// actual call to the COM server, via the <code><see cref="IDispatch"/></code> interface.
    /// </param>
    /// <returns> <see cref="Variant"/>[] result. </returns>
    /// <exception cref="InteropException"> </exception>
    Variant[] CallMethodA(int dispId, params object[] inparams);

    /// <summary>
    /// Performs a <code>method</code> call for the method identified by the
    /// <code>name</code> parameter.
    /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/>
    /// and then delegates the call to <seealso cref="CallMethod(int, object[], int[])"/>.
    /// For the <code>inparams</code> array, the corresponding
    /// <code>DISPID</code>s are present in the <code>dispIds</code> array.
    /// The size of both arrays should match.
    /// </summary>
    /// <remarks>inparams.length == dispIds.length.</remarks>
    /// <param name="name"> name of the method to invoke. </param>
    /// <param name="inparams"> members of this array are implicitly converted to
    /// <code><see cref="Variant"/></code>s before performing the
    /// actual call to the COM server, via the <code><see cref="IDispatch"/></code> interface.
    /// </param>
    /// <param name="dispIds"> array of <code>DISPID</code>s, matching by index
    /// to those in <code>inparams</code> array. </param>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.ArgumentException"> if the <code>name</code>
    /// is <code>null</code> or empty. </exception>
    void CallMethod(string name, object[] inparams, int[] dispIds);

    /// <summary>
    /// Performs a <code>method</code> call for the method identified by the
    /// <code>dispId</code> parameter.
    /// For the <code>inparams</code> array, the corresponding <code>DISPID</code>s
    /// are present in the <code>dispIds</code> array. The size of both arrays should
    /// match.
    /// </summary>
    /// <remarks>inparams.length == dispIds.length.</remarks>
    /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
    /// <param name="inparams"> members of this array are implicitly converted to
    /// <code><see cref="Variant"/></code>s before performing the
    /// actual call to the COM server, via the <code><see cref="IDispatch"/></code> interface.
    /// </param>
    /// <param name="dispIds"> array of <code>DISPID</code>s, matching by index to
    /// those in <code>inparams</code> array. </param>
    /// <exception cref="InteropException"> </exception>
    void CallMethod(int dispId, object[] inparams, int[] dispIds);

    /// <summary>
    /// Performs a <code>method</code> call for the method identified by the
    /// <code>name</code> parameter.
    /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/>
    /// and then delegates the call to <seealso cref="CallMethodA(int, object[], int[])"/>.
    /// For the <code>inparams</code> array, the corresponding
    /// <code>DISPID</code>s are present in the <code>dispId</code> array.
    /// The size of both arrays should match.
    /// </summary>
    /// <remarks>inparams.length == dispIds.length.</remarks>
    /// <param name="name"> name of the method to invoke. </param>
    /// <param name="inparams"> members of this array are implicitly converted to
    /// <code><see cref="Variant"/></code>s before performing the
    /// actual call to the COM server, via the <code><see cref="IDispatch"/></code> interface.
    /// </param>
    /// <param name="dispIds"> array of <code>DISPID</code>s, matching by index to
    /// those in <code>inparams</code> array. </param>
    /// <returns> <see cref="Variant"/>[] result. </returns>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.ArgumentException"> if the <code>name</code> is
    /// <code>null</code> or empty. </exception>
    Variant[] CallMethodA(string name, object[] inparams, int[] dispIds);

    /// <summary>
    /// Performs a <code>method</code> call for the method identified by the
    /// <code>dispId</code> parameter.
    /// For the <code>inparams</code> array, the corresponding <code>DISPID</code>s
    /// are present in the <code>dispIds</code> array. The size of both arrays should
    /// match.
    /// </summary>
    /// <remarks>inparams.length == dispIds.length.</remarks>
    /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
    /// <param name="inparams"> members of this array are implicitly converted to
    /// <code><see cref="Variant"/></code>s before performing the
    /// actual call to the COM server, via the <code><see cref="IDispatch"/></code> interface.
    /// </param>
    /// <param name="dispIds"> array of <code>DISPID</code>s, matching by index to
    /// those in <code>inparams</code> array. </param>
    /// <returns> <see cref="Variant"/>[] result. </returns>
    /// <exception cref="InteropException"> </exception>
    Variant[] CallMethodA(int dispId, object[] inparams, int[] dispIds);

    /// <summary>
    /// Performs a <code>method</code> call for the method identified by the
    /// <code>name</code> parameter.
    /// Internally it will first do a  <seealso cref="GetIDsOfNames(string[])"/>
    /// by forming <code>name + paramNames []</code>,
    /// and then delegates the call to <seealso cref="CallMethod(int, object[], int[])"/>.
    /// For the <code>inparams</code> array,
    /// the corresponding parameter names are present in the <code>paramNames</code> array.
    /// The size of both
    /// arrays should match.
    /// </summary>
    /// <remarks>inparams.length == paramNames.length.</remarks>
    /// <param name="name"> name of the method to invoke. </param>
    /// <param name="inparams"> members of this array are implicitly converted to
    /// <code><see cref="Variant"/></code>s before performing the
    /// actual call to the COM server, via the <code><see cref="IDispatch"/></code> interface.
    /// </param>
    /// <param name="paramNames"> Array of parameter names, matching by index to
    /// those in <code>inparams</code> array. </param>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.ArgumentException"> if the <code>name</code> is
    /// <code>null</code> or empty. </exception>
    void CallMethod(string name, object[] inparams, string[] paramNames);

    /// <summary>
    /// Performs a <code>method</code> call for the method identified by the
    /// <code>name</code> parameter.
    /// Internally it will first do a <seealso cref="GetIDsOfNames(string[])"/>
    /// by forming <code>name + paramNames []</code>,
    /// and then delegates the call to <seealso cref="CallMethodA(int, object[], int[])"/>.
    /// For the <code>inparams</code> array,
    /// the corresponding parameter names are present in the <code>paramNames</code>
    /// array. The size of both arrays should match.
    /// </summary>
    /// <remarks>inparams.length == paramNames.length.</remarks>
    /// <param name="name"> name of the method to invoke. </param>
    /// <param name="inparams"> members of this array are implicitly converted
    /// to <code><see cref="Variant"/></code>s before performing the
    /// actual call to the COM server, via the <code><see cref="IDispatch"/></code> interface.
    /// </param>
    /// <param name="paramNames"> Array of parameter names, matching by index to
    /// those in <code>inparams</code> array. </param>
    /// <returns> <see cref="Variant"/> result. </returns>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.ArgumentException"> if the <code>name</code> is
    /// <code>null</code> or empty. </exception>
    Variant[] CallMethodA(string name, object[] inparams, string[] paramNames);

    /// <summary>
    /// Performs a <code>propput</code> for the method identified by the <code>dispId</code>
    /// </summary>
    /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
    /// <param name="inparams"> parameters for that method. </param>
    /// <exception cref="InteropException"> </exception>
    void Put(int dispId, params object[] inparams);

    /// <summary>
    /// Performs a <code>propput</code> for the method identified by the
    /// <code>name</code> parameter.
    /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/> and
    /// then delegates the call to <seealso cref="Put(int, object[])"/>.
    /// </summary>
    /// <param name="name"> name of the method to invoke. </param>
    /// <param name="inparams"> parameters for that method. </param>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.ArgumentException"> if the <code>name</code> is
    /// <code>null</code> or empty. </exception>
    void Put(string name, params object[] inparams);

    /// <summary>
    /// Performs a <code>propputref</code> for the method identified by the
    /// <code>dispId</code>.
    /// </summary>
    /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
    /// <param name="inparams"> parameters for that method. </param>
    /// <exception cref="InteropException"> </exception>
    void PutRef(int dispId, params object[] inparams);

    /// <summary>
    /// Performs a <code>propput</code> for the method identified by the
    /// <code>name</code> parameter.
    /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/>
    /// and then delegates the call to <seealso cref="PutRef(int, object[])"/>.
    /// </summary>
    /// <param name="name"> name of the method to invoke. </param>
    /// <param name="inparams"> parameters for that method. </param>
    /// <exception cref="InteropException"> </exception>
    /// <exception cref="System.ArgumentException"> if the <code>name</code> is
    /// <code>null</code> or empty. </exception>
    void PutRef(string name, params object[] inparams);
}
