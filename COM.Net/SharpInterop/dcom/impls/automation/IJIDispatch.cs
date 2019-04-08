// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.impls.automation {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.core;

    /// <summary>
    /// Represents the Windows COM <code>IDispatch</code> Interface.
    /// 
    /// Sample Usage :-
    /// <code>
    ///  //Assume comServer is the reference to JIComServer, obtained earlier... 
    ///  IJIComObject comObject = comServer.createInstance(); 
    ///  // This call will result into a <i>QueryInterface</i> for the IDispatch 
    ///  IJIDispatch dispatch = 
    ///     (IJIDispatch)JIObjectFactory.NarrowObject(comObject.queryInterface(IJIDispatch.IID)); 
    /// </code>
    /// 
    /// Another example :-
    /// <code>
    ///  int dispId = dispatch.getIDsOfNames("Workbooks");
    ///  JIVariant outVal = dispatch.get(dispId);
    ///  IJIDispatch dispatchOfWorkBooks =
    ///     (IJIDispatch)JIObjectFactory.NarrowObject(outVal.getObjectAsComObject());
    ///  JIVariant[] outVal2 = 
    ///     dispatchOfWorkBooks.CallMethodA("Add",new object[]{JIVariant.OPTIONAL_PARAM()});
    ///  dispatchOfWorkBook =
    ///     (IJIDispatch)JIObjectFactory.narrowObject(outVal2[0].getObjectAsComObject());
    ///  outVal = dispatchOfWorkBook.get("Worksheets");
    ///  dispatchOfWorkSheets =
    ///     (IJIDispatch)JIObjectFactory.NarrowObject(outVal.getObjectAsComObject());
    /// </code>
    /// 
    /// Please note that all <code>[in]</code> parameters are converted to 
    /// <code><seealso cref="JIVariant"/></code>
    /// before being sent to the COM server through the <code>IJIDispatch</code>
    /// interface. If any <code>[in]</code> parameter is already a
    /// <code>JIVariant</code>, it is left as it is.
    /// for example:- 
    ///  <code>
    ///  //From MSADO example. 
    ///  dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(
    ///     comObject.queryInterface(IJIDispatch.IID));
    ///  dispatch.callMethod("Open", new object[]{
    ///     new JIString("driver=Microsoft Access Driver (*.mdb);dbq=C:\\temp\\products.mdb"), 
    ///     JIVariant.OPTIONAL_PARAM,JIVariant.OPTIONAL_PARAM,new Integer(-1)
    ///  }); 
    ///  JIVariant variant[] = dispatch.CallMethodA("Execute",new Object[]{
    ///     new JIString("SELECT * FROM Products"), new Integer(-1)
    ///  }); 
    ///  if (variant[0].isNull()) { 
    ///		 Console.WriteLine("Recordset is empty."); 
    ///  }
    ///  else {
    ///		//Do something...
    ///  }
    ///  </code>
    ///  
    /// Where ever the corresponding COM interface API requires an <code>[optional]</code> parameter,
    /// the developer can use <code>JIVariant.OPTIONAL_PARAM()</code>, like in the example above.
    /// </summary>
    public interface IJIDispatch : IJIComObject {

        /// <summary>
        /// Returns the COM <code>EXCEPINFO</code> structure wrapped as a
        /// data object for the <b>last</b> operation. Note this will only be 
        /// valid if a <seealso cref="JIException"/> has been raised
        /// in the last call.
        /// </summary>
        JIExcepInfo LastExcepInfo { get; }

        /// <summary>
        /// Determines whether there is type information available for the dual interface.
        /// </summary>
        /// <returns> 1 if the object provides type information, otherwise 0. </returns>
        /// <exception cref="JIException"> </exception>
        int TypeInfoCount { get; }

        /// <summary>
        /// Maps a method name to its corresponding <code>DISPID</code>. 
        /// The result of this call is cached
        /// for further usage and no network call is performed again for 
        /// the same method name. 
        /// </summary>
        /// <param name="apiName"> Method name. </param>
        /// <returns> <code>DISPID</code> of the method. </returns>
        /// <exception cref="JIException"> </exception>
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
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if the <code>apiName</code> 
        /// is <code>null</code> or empty. </exception>
        int[] GetIDsOfNames(string[] apiName);

        /// <summary>
        /// Returns an implementation of COM <code>ITypeInfo</code> interface 
        /// based on the <code>typeInfo</code>.
        ///  </summary>
        /// <param name="typeInfo"> the type information to return. Pass 0 to
        /// retrieve type information for the <code>IDispatch</code> implementation.
        /// </param>
        /// <exception cref="JIException"> </exception>
        IJITypeInfo GetTypeInfo(int typeInfo);

        /// <summary>
        /// Performs a <code>propput</code> for the method identified by the
        /// <code>dispId</code>. 
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. 
        /// </param>
        /// <param name="inparam"> parameter for that method. </param>
        /// <exception cref="JIException"> </exception>
        void Put(int dispId, JIVariant inparam);

        /// <summary>
        /// Performs a <code>propput</code> for the method identified by the 
        /// <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/>
        /// and then delegates the call to <seealso cref="Put(int, JIVariant)"/>.
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <param name="inparam"> parameter for that method. </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if the <code>name</code> 
        /// is <code>null</code> or empty. </exception>
        void Put(string name, JIVariant inparam);

        /// <summary>
        /// Performs a <code>propputref</code> for the method identified by the
        /// <code>dispId</code>. 
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke.
        /// </param>
        /// <param name="inparam"> parameter for that method. </param>
        /// <exception cref="JIException"> </exception>
        void PutRef(int dispId, JIVariant inparam);

        /// <summary>
        /// Performs a <code>propput</code> for the method identified by the 
        /// <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/> 
        /// and then delegates the call to <seealso cref="PutRef(int, JIVariant)"/>.
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <param name="inparam"> parameter for that method. </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if the <code>name</code> is 
        /// <code>null</code> or empty. </exception>
        void PutRef(string name, JIVariant inparam);

        /// <summary>
        /// Performs a <code>propget</code> for the method identified by the
        /// <code>dispId</code>. 
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <returns> JIVariant result of the call </returns>
        /// <exception cref="JIException"> </exception>
        JIVariant Get(int dispId);

        /// <summary>
        /// Performs a <code>propget</code> for the method identified by the 
        /// <code>dispId</code> parameter.
        /// <code>inparams</code> defines the parameters for the <code>get</code> 
        /// operation.
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <param name="inparams"> members of this array are implicitly converted 
        /// to <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface. 
        /// </param>
        /// <returns> array of JIVariants </returns>
        /// <exception cref="JIException"> </exception>
        JIVariant[] Get(int dispId, object[] inparams);

        /// <summary>
        /// Performs a <code>propget</code> for the method identified by the 
        /// <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/>
        /// and then delegates the call to <seealso cref="Get(int, object[])"/>.
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <param name="inparams"> members of this array are implicitly converted
        /// to <code>JIVariant</code>s
        /// before performing the actual call to the COM server, via the 
        /// <code>IJIDispatch</code> interface. </param>
        /// <returns> array of JIVariants </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if the <code>name</code> is 
        /// <code>null</code> or empty. </exception>
        JIVariant[] Get(string name, object[] inparams);

        /// <summary>
        /// Performs a <code>propget</code> for the method identified by the 
        /// <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/> 
        /// and then delegates the call to <seealso cref="Get(int)"/>
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <returns> JIVariant result of the call. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if the <code>name</code> is
        /// <code>null</code> or empty. </exception>
        JIVariant Get(string name);

        /// <summary>
        /// Performs a <code>method</code> call for the method identified by the 
        /// <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/> 
        /// and then delegates the call to <seealso cref="CallMethod(int)"/>.
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if the <code>name</code> is 
        /// <code>null</code> or empty. </exception>
        void CallMethod(string name);

        /// <summary>
        /// Performs a <code>method</code> call for the method identified by the 
        /// <code>dispId</code> parameter. 
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke.</param>
        /// <exception cref="JIException"> </exception>
        void CallMethod(int dispId);

        /// <summary>
        /// Performs a <code>method</code> call for the method identified by the 
        /// <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/> and
        /// then delegates the call to <seealso cref="CallMethodA(int)"/>.
        ///  </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <returns> JIVariant result. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if the <code>name</code> 
        /// is <code>null</code> or empty. </exception>
        JIVariant CallMethodA(string name);

        /// <summary>
        /// Performs a <code>method</code> call for the method identified by the
        /// <code>dispId</code> parameter. 
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. 
        /// </param>
        /// <returns> JIVariant result. </returns>
        /// <exception cref="JIException"> </exception>
        JIVariant CallMethodA(int dispId);

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
        /// converted to <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> 
        /// interface. </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if the <code>name</code> 
        /// is <code>null</code> or empty. </exception>
        //
        void CallMethod(string name, object[] inparams);

        /// <summary>
        /// Performs a <code>method</code> call for the method identified by
        /// the <code>dispId</code> parameter.
        /// For the <code>inparams</code> array, sequential <code>DISPID</code>s
        /// (zero based index) will be used.
        /// For <code>inparam[0]</code> , <code>DISPID</code> will be
        /// <code>0</code>, for <code>inparam[1]</code>
        /// it will be <code>1</code> and so on. 
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <param name="inparams"> members of this array are implicitly converted to 
        /// <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface. 
        /// </param>
        /// <exception cref="JIException"> </exception>
        void CallMethod(int dispId, object[] inparams);

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
        /// <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface.
        /// </param>
        /// <returns> JIVariant[] result. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if the <code>name</code> is 
        /// <code>null</code> or empty. </exception>
        JIVariant[] CallMethodA(string name, object[] inparams);

        /// <summary>
        /// Performs a <code>method</code> call for the method identified by the 
        /// <code>dispId</code> parameter.
        /// For the <code>inparams</code> array, sequential <code>DISPID</code>s 
        /// (zero based index) will be used.
        /// For <code>inparam[0]</code> , <code>DISPID</code> will be <code>0</code>,
        /// for <code>inparam[1]</code>
        /// it will be <code>1</code> and so on.
        /// sequential dispIds for params are used 0,1,2,3...
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <param name="inparams"> members of this array are implicitly converted to 
        /// <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface.
        /// </param>
        /// <returns> JIVariant[] result. </returns>
        /// <exception cref="JIException"> </exception>
        JIVariant[] CallMethodA(int dispId, object[] inparams);

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
        /// <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface.
        /// </param>
        /// <param name="dispIds"> array of <code>DISPID</code>s, matching by index 
        /// to those in <code>inparams</code> array. </param>
        /// <exception cref="JIException"> </exception>
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
        /// <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface.
        /// </param>
        /// <param name="dispIds"> array of <code>DISPID</code>s , matching by index to
        /// those in <code>inparams</code> array. </param>
        /// <exception cref="JIException"> </exception>
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
        /// <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface.
        /// </param>
        /// <param name="dispIds"> array of <code>DISPID</code>s , matching by index to 
        /// those in <code>inparams</code> array. </param>
        /// <returns> JIVariant[] result. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if the <code>name</code> is 
        /// <code>null</code> or empty. </exception>
        JIVariant[] CallMethodA(string name, object[] inparams, int[] dispIds);

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
        /// <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface.
        /// </param>
        /// <param name="dispIds"> array of <code>DISPID</code>s , matching by index to
        /// those in <code>inparams</code> array. </param>
        /// <returns> JIVariant[] result. </returns>
        /// <exception cref="JIException"> </exception>
        JIVariant[] CallMethodA(int dispId, object[] inparams, int[] dispIds);

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
        /// <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface. 
        /// </param>
        /// <param name="paramNames"> Array of parameter names, matching by index to 
        /// those in <code>inparams</code> array. </param>
        /// <exception cref="JIException"> </exception>
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
        /// to <code>JIVariant</code>s before performing the
        /// actual call to the COM server, via the <code>IJIDispatch</code> interface.
        /// </param>
        /// <param name="paramNames"> Array of parameter names, matching by index to 
        /// those in <code>inparams</code> array. </param>
        /// <returns> JIVariant result. </returns>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if the <code>name</code> is 
        /// <code>null</code> or empty. </exception>
        JIVariant[] CallMethodA(string name, object[] inparams, string[] paramNames);

        /// <summary>
        /// Performs a <code>propput</code> for the method identified by the <code>dispId</code> 
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <param name="params"> parameters for that method. </param>
        /// <exception cref="JIException"> </exception>
        void Put(int dispId, object[] @params);

        /// <summary>
        /// Performs a <code>propput</code> for the method identified by the 
        /// <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/> and 
        /// then delegates the call to <seealso cref="Put(int, object[])"/>.
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <param name="params"> parameters for that method. </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if the <code>name</code> is 
        /// <code>null</code> or empty. </exception>
        void Put(string name, object[] @params);

        /// <summary>
        /// Performs a <code>propputref</code> for the method identified by the 
        /// <code>dispId</code>. 
        /// </summary>
        /// <param name="dispId"> <code>DISPID</code> of the method to invoke. </param>
        /// <param name="params"> parameters for that method. </param>
        /// <exception cref="JIException"> </exception>
        void PutRef(int dispId, object[] @params);

        /// <summary>
        /// Performs a <code>propput</code> for the method identified by the 
        /// <code>name</code> parameter.
        /// Internally it will first do a <seealso cref="GetIDsOfNames(string)"/> 
        /// and then delegates the call to <seealso cref="PutRef(int, object[])"/>.
        /// </summary>
        /// <param name="name"> name of the method to invoke. </param>
        /// <param name="params"> parameters for that method. </param>
        /// <exception cref="JIException"> </exception>
        /// <exception cref="System.ArgumentException"> if the <code>name</code> is 
        /// <code>null</code> or empty. </exception>
        void PutRef(string name, object[] @params);
    }
}