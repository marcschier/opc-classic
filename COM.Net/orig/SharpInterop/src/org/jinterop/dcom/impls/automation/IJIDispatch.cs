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

namespace org.jinterop.dcom.impls.automation {

    using JIException = org.jinterop.dcom.common.JIException;
    using IJIComObject = org.jinterop.dcom.core.IJIComObject;
    using JIVariant = org.jinterop.dcom.core.JIVariant;




    /// <summary>
    ///<para> Represents the Windows COM <code>IDispatch</code> Interface.
    /// 
    /// </para>
    /// <para>
    /// Sample Usage :-
    /// <br>
    /// 
    /// <code>
    ///  //Assume comServer is the reference to JIComServer, obtained earlier... <br>
    /// IJIComObject comObject = comServer.createInstance(); <br>
    ///  // This call will result into a <i>QueryInterface</i> for the IDispatch <br>
    /// IJIDispatch dispatch = (IJIDispatch)JIObjectFactory.narrowObject(comObject.queryInterface(IJIDispatch.IID)); <br>
    /// </code>
    /// </para>
    /// <para>
    /// Another example :-
    /// <br>
    /// <code>
    ///  int dispId = dispatch.getIDsOfNames("Workbooks");<br>
    ///  JIVariant outVal = dispatch.get(dispId);<br>
    ///  IJIDispatch dispatchOfWorkBooks =(IJIDispatch)JIObjectFactory.narrowObject(outVal.getObjectAsComObject());<br>
    ///  JIVariant[] outVal2 = dispatchOfWorkBooks.callMethodA("Add",new Object[]{JIVariant.OPTIONAL_PARAM()});<br>
    ///  dispatchOfWorkBook =(IJIDispatch)JIObjectFactory.narrowObject(outVal2[0].getObjectAsComObject());<br>
    ///  outVal = dispatchOfWorkBook.get("Worksheets");<br>
    ///  dispatchOfWorkSheets = (IJIDispatch)JIObjectFactory.narrowObject(outVal.getObjectAsComObject());<br>
    /// </code>
    /// </para>
    /// <para>
    /// 
    ///  Please note that all <code>[in]</code> parameters are converted to <code><seealso cref="JIVariant"/></code>
    ///  before being sent to the COM server through the <code>IJIDispatch</code>
    ///  interface. If any <code>[in]</code> parameter is already a <code>JIVariant</code> , it is left as it is.
    /// </para>
    ///  <para>
    ///  for example:- <br>
    ///  <code>
    ///  //From MSADO example. <br>
    ///  dispatch = (IJIDispatch)JIObjectFactory.narrowObject(comObject.queryInterface(IJIDispatch.IID));<br>
    /// dispatch.callMethod("Open",new Object[]{new JIString("driver=Microsoft Access Driver (*.mdb);dbq=C:\\temp\\products.mdb"), <br>
    ///  JIVariant.OPTIONAL_PARAM,JIVariant.OPTIONAL_PARAM,new Integer(-1)}); <br>
    /// JIVariant variant[] = dispatch.callMethodA("Execute",new Object[]{new JIString("SELECT * FROM Products"),new Integer(-1)}); <br>
    /// if (variant[0].isNull()) <br>
    /// { <br>
    ///        System.out.println("Recordset is empty."); <br>
    /// }<br>
    /// else<br>
    ///     {<br>
    ///        //Do something...<br>
    ///  }<br>
    ///   </code>
    /// </para>
    ///  <para>
    /// 
    ///  Where ever the corresponding COM interface API requires an <code>[optional]</code> parameter,
    ///  the developer can use <code>JIVariant.OPTIONAL_PARAM()</code> , like in the example above.
    ///  </para>
    /// @since 1.0
    /// </summary>
    public interface IJIDispatch : IJIComObject {

        /// <summary>
        /// Flag for selecting a <code>method</code>.
        /// </summary>

        /// <summary>
        /// Flag for selecting a Property <code>propget</code>.
        /// </summary>

        /// <summary>
        /// Flag for selecting a Property <code>propput</code>.
        /// </summary>

        /// <summary>
        /// COM <code>DISPID</code> for property "put" or "putRef".
        /// </summary>

        /// <summary>
        /// Flag for selecting a Property <code>propputref</code>.
        /// </summary>

        /// <summary>
        /// IID representing the COM <code>IDispatch</code>.
        /// </summary>

        /// <summary>
        /// Definition from MSDN:
        /// <i>Determines whether there is type information available for the dual interface. </i>
        /// <br> </summary>
        /// <returns> 1 if the object provides type information, otherwise 0. </returns>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getTypeInfoCount() throws org.jinterop.dcom.common.JIException;
        int TypeInfoCount { get; }

        /// <summary>
        /// Maps a method name to its corresponding <code>DISPID</code>.The result of this call is cached
        /// for further usage and no network call is performed again for the same method name. <br>
        /// </summary>
        /// <param name="apiName"> Method name. </param>
        /// <returns> <code>DISPID</code> of the method. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if the <code>apiName</code> is <code>null</code> or empty. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getIDsOfNames(String apiName) throws org.jinterop.dcom.common.JIException;
        int GetIDsOfNames(string apiName);

        /// <summary>
        /// Maps a single method name and an optional set of it's argument names to a corresponding set of <code>DISPIDs</code>.
        /// The result of this call is cached for further usage and no network call is performed again for the same method[argument] set.
        /// </summary>
        /// <param name="apiName"> String[] with first index depicting method name and the rest depicting parameters. </param>
        /// <returns> int[] <code>DISPIDs</code> in the same order as the method[argument] set. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if the <code>apiName</code> is <code>null</code> or empty. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int[] getIDsOfNames(String[] apiName) throws org.jinterop.dcom.common.JIException;
        int[] GetIDsOfNames(string[] apiName);

        /// <summary>
        /// Returns an implementation of COM <code>ITypeInfo</code> interface based on the <code>typeInfo</code>.
        /// <br> </summary>
        /// <param name="typeInfo"> the type information to return. Pass 0 to retrieve type information for the <code>IDispatch</code> implementation.
        /// @return </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJITypeInfo getTypeInfo(int typeInfo) throws org.jinterop.dcom.common.JIException;
        IJITypeInfo GetTypeInfo(int typeInfo);

        /// <summary>
        /// Performs a <code>propput</code> for the method identified by the <code>dispId</code>. <br>
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <param name="inparam"> parameter for that method. </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void put(int dispId, org.jinterop.dcom.core.JIVariant inparam) throws org.jinterop.dcom.common.JIException;
        void Put(int dispId, JIVariant inparam);

        /// <summary>
        /// Performs a <code>propput</code> for the method identified by the <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="#getIDsOfNames(String)"/> and then delegates the call to <seealso cref="#put(int, JIVariant)"/>.
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <param name="inparam"> parameter for that method. </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if the <code>name</code> is <code>null</code> or empty. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void put(String name, org.jinterop.dcom.core.JIVariant inparam) throws org.jinterop.dcom.common.JIException;
        void Put(string name, JIVariant inparam);

        /// <summary>
        /// Performs a <code>propputref</code> for the method identified by the <code>dispId</code>. <br>
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <param name="inparam"> parameter for that method. </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void putRef(int dispId, org.jinterop.dcom.core.JIVariant inparam) throws org.jinterop.dcom.common.JIException;
        void PutRef(int dispId, JIVariant inparam);

        /// <summary>
        /// Performs a <code>propput</code> for the method identified by the <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="#getIDsOfNames(String)"/> and then delegates the call to <seealso cref="#putRef(int, JIVariant)"/>.
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <param name="inparam"> parameter for that method. </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if the <code>name</code> is <code>null</code> or empty. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void putRef(String name, org.jinterop.dcom.core.JIVariant inparam) throws org.jinterop.dcom.common.JIException;
        void PutRef(string name, JIVariant inparam);

        /// <summary>
        /// Performs a <code>propget</code> for the method identified by the <code>dispId</code>. <br>
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <returns> JIVariant result of the call </returns>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant get(int dispId) throws org.jinterop.dcom.common.JIException;
        JIVariant Get(int dispId);

        /// <summary>
        /// Performs a <code>propget</code> for the method identified by the <code>dispId</code> parameter.
        /// <code>inparams</code> defines the parameters for the <code>get</code> operation.<br>
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <param name="inparams"> members of this array are implicitly converted to <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface. </param>
        /// <returns> array of JIVariants </returns>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] get(int dispId, Object[] inparams) throws org.jinterop.dcom.common.JIException;
        JIVariant[] Get(int dispId, object[] inparams);


        /// <summary>
        /// Performs a <code>propget</code> for the method identified by the <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="#getIDsOfNames(String)"/> and then delegates the call to <seealso cref="#get(int, Object[])"/>.
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <param name="inparams"> members of this array are implicitly converted to <code>JIVariant</code>s
        /// before performing the actual call to the COM server, via the <code>IJIDispatch</code> interface. </param>
        /// <returns> array of JIVariants </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if the <code>name</code> is <code>null</code> or empty. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] get(String name,Object[] inparams) throws org.jinterop.dcom.common.JIException;
        JIVariant[] Get(string name, object[] inparams);


        /// <summary>
        /// Performs a <code>propget</code> for the method identified by the <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="#getIDsOfNames(String)"/> and then delegates the call to <seealso cref="#get(int)"/>
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <returns> JIVariant result of the call. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if the <code>name</code> is <code>null</code> or empty. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant get(String name) throws org.jinterop.dcom.common.JIException;
        JIVariant Get(string name);

        /// <summary>
        ///Performs a <code>method</code> call for the method identified by the <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="#getIDsOfNames(String)"/> and then delegates the call to <seealso cref="#callMethod(int)"/>.
        /// <br> </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <exception cref="JIException"> </exception>
        ///  <exception cref="IllegalArgumentException"> if the <code>name</code> is <code>null</code> or empty. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(String name) throws org.jinterop.dcom.common.JIException;
        void CallMethod(string name);

        /// <summary>
        ///Performs a <code>method</code> call for the method identified by the <code>dispId</code> parameter. <br>
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(int dispId) throws org.jinterop.dcom.common.JIException;
        void CallMethod(int dispId);

        /// <summary>
        ///Performs a <code>method</code> call for the method identified by the <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="#getIDsOfNames(String)"/> and then delegates the call to <seealso cref="#callMethodA(int)"/>.
        /// <br> </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <returns> JIVariant result. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if the <code>name</code> is <code>null</code> or empty. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant callMethodA(String name) throws org.jinterop.dcom.common.JIException;
        JIVariant CallMethodA(string name);

        /// <summary>
        ///Performs a <code>method</code> call for the method identified by the <code>dispId</code> parameter. <br>
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <returns> JIVariant result. </returns>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant callMethodA(int dispId) throws org.jinterop.dcom.common.JIException;
        JIVariant CallMethodA(int dispId);

        /// <summary>
        ///Performs a <code>method</code> call for the method identified by the <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="#getIDsOfNames(String)"/> and then delegates the call to
        /// <seealso cref="#callMethod(int, Object[])"/>. For the <code>inparams</code> array, sequential <code>DISPID</code>s
        /// (zero based index) will be used. For <code>inparam[0]</code> , <code>DISPID</code> will be <code>0</code>,
        /// for <code>inparam[1]</code> it will be <code>1</code> and so on. <br>
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <param name="inparams"> members of this array are implicitly converted to <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface. </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if the <code>name</code> is <code>null</code> or empty. </exception>
        //sequential dispIds for params are used 0,1,2,3...
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(String name, Object[] inparams) throws org.jinterop.dcom.common.JIException;
        void CallMethod(string name, object[] inparams);

        //sequential dispIds for params are used 0,1,2,3...
        /// <summary>
        ///Performs a <code>method</code> call for the method identified by the <code>dispId</code> parameter.
        /// For the <code>inparams</code> array, sequential <code>DISPID</code>s (zero based index) will be used.
        /// For <code>inparam[0]</code> , <code>DISPID</code> will be <code>0</code>, for <code>inparam[1]</code>
        /// it will be <code>1</code> and so on. <br>
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <param name="inparams"> members of this array are implicitly converted to <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface. </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(int dispId, Object[] inparams) throws org.jinterop.dcom.common.JIException;
        void CallMethod(int dispId, object[] inparams);

        /// <summary>
        /// Performs a <code>method</code> call for the method identified by the <code>name</code> parameter.
        ///     Internally it will first do a <seealso cref="#getIDsOfNames(String)"/> and then delegates the call to
        ///  <seealso cref="#callMethodA(int, Object[])"/>. For the <code>inparams</code> array, sequential <code>DISPID</code>s
        ///  (zero based index) will be used. For <code>inparam[0]</code> , <code>DISPID</code> will be <code>0</code>,
        ///  for <code>inparam[1]</code> it will be <code>1</code> and so on. <br>
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <param name="inparams"> members of this array are implicitly converted to <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface. </param>
        /// <returns> JIVariant[] result. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if the <code>name</code> is <code>null</code> or empty. </exception>
        //sequential dispIds for params are used 0,1,2,3...
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] callMethodA(String name, Object[] inparams) throws org.jinterop.dcom.common.JIException;
        JIVariant[] CallMethodA(string name, object[] inparams);

        /// <summary>
        /// Performs a <code>method</code> call for the method identified by the <code>dispId</code> parameter.
        /// For the <code>inparams</code> array, sequential <code>DISPID</code>s (zero based index) will be used.
        /// For <code>inparam[0]</code> , <code>DISPID</code> will be <code>0</code>, for <code>inparam[1]</code>
        /// it will be <code>1</code> and so on.
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <param name="inparams"> members of this array are implicitly converted to <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface. </param>
        /// <returns> JIVariant[] result. </returns>
        /// <exception cref="JIException"> </exception>
        //sequential dispIds for params are used 0,1,2,3...
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] callMethodA(int dispId, Object[] inparams) throws org.jinterop.dcom.common.JIException;
        JIVariant[] CallMethodA(int dispId, object[] inparams);


        /// <summary>
        /// Performs a <code>method</code> call for the method identified by the <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="#getIDsOfNames(String)"/> and then delegates the call to
        /// <seealso cref="#callMethod(int, Object[], int[])"/>. For the <code>inparams</code> array, the corresponding
        /// <code>DISPID</code>s are present in the <code>dispIds</code> array. The size of both arrays should match.
        ///  <br> </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <param name="inparams"> members of this array are implicitly converted to <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface. </param>
        /// <param name="dispIds"> array of <code>DISPID</code>s , matching by index to those in <code>inparams</code> array. </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if the <code>name</code> is <code>null</code> or empty. </exception>
        //inparams.length == dispIds.length.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(String name, Object[] inparams, int[] dispIds) throws org.jinterop.dcom.common.JIException;
        void CallMethod(string name, object[] inparams, int[] dispIds);

        /// <summary>
        /// Performs a <code>method</code> call for the method identified by the <code>dispId</code> parameter.
        ///  For the <code>inparams</code> array, the corresponding <code>DISPID</code>s are present in
        ///  the <code>dispIds</code> array. The size of both arrays should match.
        ///  <br>
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <param name="inparams"> members of this array are implicitly converted to <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface. </param>
        /// <param name="dispIds"> array of <code>DISPID</code>s , matching by index to those in <code>inparams</code> array. </param>
        /// <exception cref="JIException"> </exception>
        //inparams.length == dispIds.length.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(int dispId, Object[] inparams, int[] dispIds) throws org.jinterop.dcom.common.JIException;
        void CallMethod(int dispId, object[] inparams, int[] dispIds);

        /// <summary>
        /// Performs a <code>method</code> call for the method identified by the <code>name</code> parameter.
        ///     Internally it will first do a <seealso cref="#getIDsOfNames(String)"/> and then delegates the call to
        ///  <seealso cref="#callMethodA(int, Object[], int[])"/>.For the <code>inparams</code> array, the corresponding
        ///  <code>DISPID</code>s are present in the <code>dispId</code> array. The size of both arrays should match.
        ///  <br> </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <param name="inparams"> members of this array are implicitly converted to <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface. </param>
        /// <param name="dispIds"> array of <code>DISPID</code>s , matching by index to those in <code>inparams</code> array. </param>
        /// <returns> JIVariant[] result. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if the <code>name</code> is <code>null</code> or empty. </exception>
        //inparams.length == dispIds.length.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] callMethodA(String name, Object[] inparams, int[] dispIds) throws org.jinterop.dcom.common.JIException;
        JIVariant[] CallMethodA(string name, object[] inparams, int[] dispIds);

        /// <summary>
        /// Performs a <code>method</code> call for the method identified by the <code>dispId</code> parameter.
        ///  For the <code>inparams</code> array, the corresponding <code>DISPID</code>s are present in the
        ///  <code>dispIds</code> array. The size of both arrays should match.
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <param name="inparams"> members of this array are implicitly converted to <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface. </param>
        /// <param name="dispIds"> array of <code>DISPID</code>s , matching by index to those in <code>inparams</code> array. </param>
        /// <returns> JIVariant[] result. </returns>
        /// <exception cref="JIException"> </exception>
        //inparams.length == dispIds.length.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] callMethodA(int dispId, Object[] inparams, int[] dispIds) throws org.jinterop.dcom.common.JIException;
        JIVariant[] CallMethodA(int dispId, object[] inparams, int[] dispIds);

        /// <summary>
        /// Performs a <code>method</code> call for the method identified by the <code>name</code> parameter.
        ///     Internally it will first do a  <seealso cref="#getIDsOfNames(String[])"/> by forming <code>name + paramNames []</code>,
        ///  and then delegates the call to <seealso cref="#callMethod(int, Object[], int[])"/>. For the <code>inparams</code> array,
        ///  the corresponding parameter names are present in the <code>paramNames</code> array. The size of both
        ///  arrays should match.
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <param name="inparams"> members of this array are implicitly converted to <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface. </param>
        /// <param name="paramNames"> Array of parameter names, matching by index to those in <code>inparams</code> array. </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if the <code>name</code> is <code>null</code> or empty. </exception>
        //    inparams.length == paramNames.length.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void callMethod(String name, Object[] inparams, String[] paramNames) throws org.jinterop.dcom.common.JIException;
        void CallMethod(string name, object[] inparams, string[] paramNames);

        /// <summary>
        /// Performs a <code>method</code> call for the method identified by the <code>name</code> parameter.
        ///     Internally it will first do a <seealso cref="#getIDsOfNames(String[])"/> by forming <code>name + paramNames []</code>,
        ///  and then delegates the call to <seealso cref="#callMethodA(int, Object[], int[])"/>. For the <code>inparams</code> array,
        ///  the corresponding parameter names are present in the <code>paramNames</code> array. The size of both
        ///  arrays should match.
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <param name="inparams"> members of this array are implicitly converted to <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface. </param>
        /// <param name="paramNames"> Array of parameter names, matching by index to those in <code>inparams</code> array. </param>
        /// <returns> JIVariant result. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if the <code>name</code> is <code>null</code> or empty. </exception>
        //inparams.length == paramNames.length.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant[] callMethodA(String name, Object[] inparams, String[] paramNames) throws org.jinterop.dcom.common.JIException;
        JIVariant[] CallMethodA(string name, object[] inparams, string[] paramNames);

        /// <summary>
        /// Performs a <code>propput</code> for the method identified by the <code>dispId</code> <br>
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <param name="params"> parameters for that method. </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void put(int dispId, Object[] params) throws org.jinterop.dcom.common.JIException;
        void Put(int dispId, object[] @params);

        /// <summary>
        /// Performs a <code>propput</code> for the method identified by the <code>name</code> parameter.
        /// Internally it will first do a  <seealso cref="#getIDsOfNames(String)"/> and then delegates the call to <seealso cref="#put(int, Object[])"/>.
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <param name="params"> parameters for that method. </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if the <code>name</code> is <code>null</code> or empty. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void put(String name, Object[] params) throws org.jinterop.dcom.common.JIException;
        void Put(string name, object[] @params);

        /// <summary>
        /// Performs a <code>propputref</code> for the method identified by the <code>dispId</code>. <br>
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <param name="params"> parameters for that method. </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void putRef(int dispId, Object[] params) throws org.jinterop.dcom.common.JIException;
        void PutRef(int dispId, object[] @params);

        /// <summary>
        /// Performs a <code>propput</code> for the method identified by the <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="#getIDsOfNames(String)"/> and then delegates the call to <seealso cref="#putRef(int, Object[])"/>.
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <param name="params"> parameters for that method. </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="IllegalArgumentException"> if the <code>name</code> is <code>null</code> or empty. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void putRef(String name, Object[] params) throws org.jinterop.dcom.common.JIException;
        void PutRef(string name, object[] @params);

        /// <summary>
        /// Returns the COM <code>EXCEPINFO</code> structure wrapped as a data object for the
        /// <b>last</b> operation. Note this will only be valid if a <seealso cref="JIException"/> has been raised
        /// in the last call.
        /// 
        /// @return
        /// </summary>
        JIExcepInfo LastExcepInfo { get; }
    }

    public static class IJIDispatch_Fields {
        public const int DISPATCH_METHOD = unchecked((int)0xFFFFFFF1);
        public const int DISPATCH_PROPERTYGET = unchecked((int)0xFFFFFFF2);
        public const int DISPATCH_PROPERTYPUT = unchecked((int)0xFFFFFFF4);
        public const int DISPATCH_DISPID_PUTPUTREF = unchecked((int)0xFFFFFFFD);
        public const int DISPATCH_PROPERTYPUTREF = unchecked((int)0xFFFFFFF8);
        public const string IID = "00020400-0000-0000-c000-000000000046";
    }

}