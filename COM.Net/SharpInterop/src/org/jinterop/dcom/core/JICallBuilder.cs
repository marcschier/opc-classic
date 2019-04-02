// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.core {
    using ndr;
    using org.jinterop.dcom.common;
    using rpc.core;
    using Serilog;
    using System;
    using System.Collections;

    /// <summary>
    /// Class used for setting up information such as <code>[in]</code>,
    /// <code>[out]</code> parameters and the method number for executing 
    /// a call to the COM server. 
    /// Sample Usage :-
    /// <code>
    ///  JICallBuilder obj = new JICallBuilder(); 
    ///  obj.reInit(); 
    ///  obj.setOpnum(0); //0 based index, can be obtained from the IDL or the Type Library of COM server.
    /// 
    ///  obj.addInParamAsString(new JIString("j-Interop Rocks !"), JIFlags.FLAG_NULL); 
    ///  obj.addInParamAsInt(100, JIFlags.FLAG_NULL); 
    ///  //handle is previously obtained <seealso cref="IJIComObject"/> 
    ///  Object[] result = comObject.call(obj); 
    /// </code>
    /// [out] parameters can be added in a similar way. 
    /// <code>
    ///  obj.addOutParamAsType(JIVariant.class,JIFlags.FLAG_NULL); 
    ///  obj.addOutParamAsObject(new JIPointer(Short.class,true),JIFlags.FLAG_NULL); 
    /// </code>
    /// </summary>
    [Serializable]
    public class JICallBuilder : NdrObject {

        internal const string CURRENTSESSION = "CURRENTSESSION";
        internal const string COMOBJECTS = "COMOBJECTS";

        private int _opnum = -1;
        private object[] _outparams;
        private readonly bool _dispatchNotSupported;
        private string _enclosingParentsIPID;
        private ArrayList _inparamFlags = new ArrayList();
        private ArrayList _outparamFlags = new ArrayList();
        private ArrayList _inParams = new ArrayList();
        private ArrayList _outParams = new ArrayList();
        private int _hresult;
        private bool _executed;
        private object[] _resultsOfException;
        private JISession _session;
        internal bool _fromDestroySession;

        /// <summary>
        /// Constructs a builder object.
        /// </summary>
        /// <param name="dispatchNotSupported"> <code>true</code> if <code>IDispatch</code> is 
        /// not supported by the <code>IJIComObject</code> on which this builder would
        /// act. Use <seealso cref="IJIComObject.DispatchSupported"/> to find out if 
        /// dispatch is supported on the COM Object. </param>
        public JICallBuilder(bool dispatchNotSupported) : this() {
            _dispatchNotSupported = dispatchNotSupported;
        }

        /// <summary>
        /// Constructs a builder object. It is assumed that <code>IDispatch</code>
        /// interface is supported by the <code>IJIComObject</code> on which this builder
        /// would act.
        /// </summary>
        public JICallBuilder() {
            //		enclosingParentsIPID = IPIDofParent;
        }

        /// <summary>
        /// Reinitializes all members of this object. It is ready to be used again on a 
        /// fresh <code><seealso cref="IJIComObject.call(JICallBuilder)"/></code> after this step. 
        /// 
        /// </summary>
        //after reinit, except parent, nothing is available.
        public virtual void reInit() {
            _opnum = -1;
            _inParams = new ArrayList();
            _inparamFlags = new ArrayList();
            _outParams = new ArrayList();
            _outparamFlags = new ArrayList();
            _hresult = -1;
            _outparams = null;
            _executed = false;
        }

        internal virtual string ParentIpid {
            set => _enclosingParentsIPID = value;
            get => _enclosingParentsIPID;
        }


        //	/**Add IN parameter as <code>JIInterfacePointer</code> at the end of the Parameter list.
        //	 * 
        //	 * @param value
        //	 * @param flags from JIFlags (if need be)
        //	 */
        //	public void addInParamAsInterfacePointer(JIInterfacePointer interfacePointer, int flags)
        //	{
        //		insertInParamAsInterfacePointerAt(inParams.size(),interfacePointer,flags);
        //	}

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>IJIComObject</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="comObject"> </param>
        /// <param name="flags"> from JIFlags (if need be) </param>
        public virtual void addInParamAsComObject(IJIComObject comObject, int flags) {
            insertInParamAsComObjectAt(_inParams.Count, comObject, flags);
        }


        /// <summary>
        /// Add <code>[in]</code> parameter as <code>int</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be) </param>
        public virtual void addInParamAsInt(int value, int flags) {
            insertInParamAsIntAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>IJIUnsigned</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be) </param>
        public virtual void addInParamAsUnsigned(IJIUnsigned value, int flags) {
            insertInParamAsUnsignedAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>float</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be) </param>
        public virtual void addInParamAsFloat(float value, int flags) {
            insertInParamAsFloatAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>bool</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be) </param>
        public virtual void addInParamAsBoolean(bool value, int flags) {
            insertInParamAsBooleanAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>short</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be) </param>
        public virtual void addInParamAsShort(short value, int flags) {
            insertInParamAsShortAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>double</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be) </param>
        public virtual void addInParamAsDouble(double value, int flags) {
            insertInParamAsDoubleAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>char</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be) </param>
        public virtual void addInParamAsCharacter(char value, int flags) {
            insertInParamAsCharacterAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>String</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (These <i>HAVE</i> to be the <b>String</b> Flags).</param>
        public virtual void addInParamAsString(string value, int flags) {
            insertInParamAsStringAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>JIVariant</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be). </param>
        public virtual void addInParamAsVariant(JIVariant value, int flags) {
            insertInParamAsVariantAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>Object</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be). </param>
        public virtual void addInParamAsObject(object value, int flags) {
            insertInParamAsObjectAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>String representation of UUID</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be). </param>
        public virtual void addInParamAsUUID(string value, int flags) {
            insertInParamAsUUIDAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>JIPointer</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be). </param>
        public virtual void addInParamAsPointer(JIPointer value, int flags) {
            insertInParamAsPointerAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>JIStruct</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be). </param>
        public virtual void addInParamAsStruct(JIStruct value, int flags) {
            insertInParamAsStructAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>JIArray</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be). </param>
        public virtual void addInParamAsArray(JIArray value, int flags) {
            insertInParamAsArrayAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>Object[]</code> at the end of the Parameter list.The array is iterated and
        /// all members appended to the list.
        /// </summary>
        /// <param name="values"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void setInParams(object[] values, int flags) {
            for (var i = 0; i < values.Length; i++) {
                _inParams.Add(values[i]);
                _inparamFlags.Add(flags); // quite useless but do not want to change logic elsewhere
            }
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>IJIComObject</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void insertInParamAsComObjectAt(int index, IJIComObject value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>int</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void insertInParamAsIntAt(int index, int value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>IJIUnsigned</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void insertInParamAsUnsignedAt(int index, IJIUnsigned value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>float</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void insertInParamAsFloatAt(int index, float value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>bool</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void insertInParamAsBooleanAt(int index, bool value, int flags) {
            _inParams.Insert(index, Convert.ToBoolean(value));
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>short</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void insertInParamAsShortAt(int index, short value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>double</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void insertInParamAsDoubleAt(int index, double value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>char</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void insertInParamAsCharacterAt(int index, char value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>String</code>  at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (These <i>HAVE</i> to be the <b>String</b> Flags). </param>
        public virtual void insertInParamAsStringAt(int index, string value, int flags) {
            _inParams.Insert(index, new JIString(value, flags));
            _inparamFlags.Insert(index, JIFlags.FLAG_NULL);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>JIVariant</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void insertInParamAsVariantAt(int index, JIVariant value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, JIFlags.FLAG_NULL);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>Object</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void insertInParamAsObjectAt(int index, object value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>String representation of UUID</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void insertInParamAsUUIDAt(int index, string value, int flags) {
            _inParams.Insert(index, new UUID(value));
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>JIPointer</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void insertInParamAsPointerAt(int index, JIPointer value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>JIStruct</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void insertInParamAsStructAt(int index, JIStruct value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>JIArray</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void insertInParamAsArrayAt(int index, JIArray value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Removes <code>[in]</code> parameter at the specified index from the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void removeInParamAt(int index, int flags) {
            _inParams.RemoveAt(index);
            _inparamFlags.RemoveAt(index);
        }

        /// <summary>
        /// Returns <code>[in]</code> parameter at the specified index from the Parameter list.
        /// Will just provide 1 getter, for outParams there would be overloads like inParam setters.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <returns> Primitives are returned as there Derieved types.  </returns>
        public virtual object getInParamAt(int index) {
            return _inParams[index];
        }

        /// <summary>
        /// Add <code>[out]</code> parameter of the type <code>clazz</code> at the end of the out parameter list.
        /// </summary>
        /// <param name="clazz"> </param>
        /// <param name="flags"> </param>
        public virtual void addOutParamAsType(Type clazz, int flags) {
            insertOutParamAt(_outParams.Count, clazz, flags);
        }

        /// <summary>
        /// Add <code>[out]</code> parameter at the end of the out parameter list. Typically callers are  
        /// composite in nature JIStruct, JIUnions, JIPointer and JIString . 
        /// </summary>
        /// <param name="outparam"> </param>
        /// <param name="flags"> </param>
        public virtual void addOutParamAsObject(object outparam, int flags) {
            insertOutParamAt(_outParams.Count, outparam, flags);
        }

        /// <summary>
        /// insert an <code>[out]</code> parameter at the specified index in the out parameter list. 
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="classOrInstance"> can be either a Class or an Object </param>
        /// <param name="flags"> </param>
        public virtual void insertOutParamAt(int index, object classOrInstance, int flags) {
            _outParams.Insert(index, classOrInstance);
            _outparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Retrieves the <code>[out]</code> param at the index in the out parameters list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <returns>  </returns>
        public virtual object getOutParamAt(int index) {
            return _outParams[index];
        }

        /// <summary>
        ///Removes <code>[out]</code> parameter at the specified index from the out parameters list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void removeOutParamAt(int index, int flags) {
            _outParams.RemoveAt(index);
            _outparamFlags.RemoveAt(index);
        }

        /// <summary>
        /// Add <code>[out]</code> parameter as <code>Object[]</code> at the end of the Parameter list.
        /// The array is iterated and all members appended to the list. 
        /// </summary>
        /// <param name="values"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void setOutParams(object[] values, int flags) {
            for (var i = 0; i < values.Length; i++) {
                _outParams.Add(values[i]);
                _outparamFlags.Add(flags);
            }

        }

        /// <summary>
        /// Returns the results as an <code>Object[]</code>. 
        /// This array has to be iterated over to get the individual values.
        /// only valid before the interpretation of read, after that has actual values
        /// </summary>
        public virtual object[] Results {
            get {
                checkIfCalled();
                return _outparams;
            }
        }

        /// <summary>
        /// Returns the value as <code>int</code> at the index from the result list. 
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual int getResultAsIntAt(int index) {
            checkIfCalled();
            return (int)(int?)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>float</code> at the index from the result list. 
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual float getResultAsFloatAt(int index) {
            checkIfCalled();
            return (float)(float?)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>bool</code> at the index from the result list. 
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual bool getResultAsBooleanAt(int index) {
            checkIfCalled();
            return (bool)(bool?)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>short</code> at the index from the result list. 
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual short getResultAsShortAt(int index) {
            checkIfCalled();
            return (short)(short?)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>double</code> at the index from the result list. 
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual double getResultAsDoubleAt(int index) {
            checkIfCalled();
            return (double)(double?)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>char</code> at the index from the result list. 
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual char getResultAsCharacterAt(int index) {
            checkIfCalled();
            return (char)(char?)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>JIString</code> at the index from the result list. 
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual JIString getResultAsStringAt(int index) {
            checkIfCalled();
            return (JIString)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>JIVariant</code> at the index from the result list. 
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual JIVariant getResultAsVariantAt(int index) {
            checkIfCalled();
            return (JIVariant)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>String representation of the UUID</code> at the index from the result list. 
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual string getResultAsUUIDStrAt(int index) {
            checkIfCalled();
            return ((UUID)_outparams[index]).ToString();
        }

        /// <summary>
        /// Returns the value as <code>JIPointer</code> at the index from the result list. 
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual JIPointer getResultAsPointerAt(int index) {
            checkIfCalled();
            return (JIPointer)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>JIStruct</code> at the index from the result list. 
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual JIStruct getResultAsStructAt(int index) {
            checkIfCalled();
            return (JIStruct)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>JIArray</code> at the index from the result list. 
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual JIArray getResultAsArrayAt(int index) {
            checkIfCalled();
            return (JIArray)_outparams[index];
        }

        /// <summary>
        /// Returns the results incase an exception occured. 
        /// </summary>
        public virtual object[] ResultsInCaseOfException {
            get {
                checkIfCalled();
                return _resultsOfException;
            }
        }

        /// <summary>
        /// Returns the <code>HRESULT</code> of this operation. This should be zero for successful calls and
        /// non-zero for failures.
        /// </summary>
        public virtual int HRESULT => _hresult;

        /// <summary>
        /// Helper
        /// </summary>
        private void checkIfCalled() {
            if (!_executed) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_API_INCORRECTLY_CALLED));
            }
        }

        /// <summary>
        /// Returns the entire <code>[in]</code> parameters list. 
        /// </summary>
        public virtual object[] InParams => _inParams.ToArray();

        /// <summary>
        /// Returns the entire <code>[out]</code> parameters list. 
        /// </summary>
        public virtual object[] OutParams => _outParams.ToArray();

        /// <summary>
        /// Returns the In Param flag.
        /// </summary>
        public virtual int?[] InparamFlags => (int?[])_inparamFlags.ToArray(typeof(int?));

        /// <summary>
        /// Returns the Out Param flag.
        /// </summary>
        public virtual int?[] OutparamFlags => (int?[])_outparamFlags.ToArray(typeof(int?));

        /// <summary>
        /// Returns the opnum of the API which will be invoked at the <code>COM</code> server. 
        /// </summary>
        public override int Opnum {
            get =>
                //opnum is 3 as this is a COM interface and 0,1,2 are occupied by IUnknown
                //TODO remember this for extending com components also.
                _opnum;
            set {
                var dispatch = 0;
                if (!_dispatchNotSupported) {
                    dispatch = 4; //4 apis.
                }
                _opnum = dispatch + value + 3; //0,1,2, Q.I
            }
        }

        //All Methods are 0 index based

        internal virtual void write2(NetworkDataRepresentation ndr) {
            //reset buffer size here...
            //calculate rough length required length + 16 for the last bytes
            //plus adding 30 more for the verifier etc. 
            ndr.Buffer.buf = new sbyte[bufferLength() + 16 + 30];
            JIOrpcThat.encode(ndr);
            writePacket(ndr);
        }

        /// <summary>
        /// @exclude
        /// </summary>
        public override void write(NetworkDataRepresentation ndr) {

            //reset buffer size here...
            //calculate rough length required length + 16 for the last bytes
            //plus adding 30 more for the verifier etc. 
            ndr.Buffer.buf = new sbyte[bufferLength() + 16];

            var orpcthis = new JIOrpcThis();
            orpcthis.encode(ndr);

            writePacket(ndr);

            //when it ends add 16 zeros.
            ndr.writeUnsignedLong(0);
            ndr.writeUnsignedLong(0);
            ndr.writeUnsignedLong(0);
            ndr.writeUnsignedLong(0);
        }

        private void writePacket(NetworkDataRepresentation ndr) {
            if (_session == null) {
                throw new InvalidOperationException("Programming Error ! Session not attached with this call ! ... Please rectify ! ");
            }

            object[] inparams = _inParams.ToArray();

            var index = 0;
            if (inparams != null) {
                //			if (JISystem.getLogger().isLoggable(Level.FINEST))
                //			{
                //				String str = "";
                //				for (int i = 0;i < inparams.length;i++)
                //				{
                //					str = str + "In Param:[" + i + "] " + inparams[i] + "\n";
                //				}
                //				JISystem.getLogger().finest(str);
                //			}
                while (index < inparams.Length) {
                    IList listOfDefferedPointers = new ArrayList();
                    if (inparams[index] == null) {
                        JIMarshalUnMarshalHelper.serialize(ndr, typeof(int?), 0, listOfDefferedPointers, JIFlags.FLAG_NULL);
                    }
                    else {
                        JIMarshalUnMarshalHelper.serialize(ndr, inparams[index].GetType(), inparams[index], listOfDefferedPointers, (int)(int?)_inparamFlags[index]);
                    }

                    var x = 0;

                    while (x < listOfDefferedPointers.Count) {
                        //					thought of this today morning...change the logic here...the defeered pointers need to be 
                        //					completely serialized here. If they are also having nested deffered pointers then  those pointers
                        //					should be "inserted" just after the current pointer itself.
                        //					change the logic below to send out a new list and insert that list after the current x.
                        //					consider the case when there is a Struct having a nested pointer to another struct and this struct
                        //					itself having a pointer.
                        //					
                        //					Inparams order:- for 2 params.
                        //					int f,Struct{int i;			 
                        //								 Struct *ptr;
                        //								 Struct *ptr2;
                        //								 int j;
                        //								}
                        //					
                        //					while serializing this struct the pointer 1 will get deffered and so will pointer 2. Now while writing
                        //					the deffered pointers , we will find that the pointer 1 is pointing to a struct which has another deffered pointer (pointer to another struct maybe)
                        //					in such case, the current logic will add the deffered pointer to the end of the listOfDefferedPointers list, effectively serializing it
                        //					after the pointer 2 referent. But that is what is against the rules of DCERPC, in this case the referent of pointer 1 (struct with the pointer to another struct)
                        //					should be serialized in place (following th rules of the struct serialization ofcourse) and should not go to the end of the list.

                        //JIMarshalUnMarshalHelper.serialize(ndr,JIPointer.class,(JIPointer)listOfDefferedPointers.get(x),listOfDefferedPointers,inparamFlags);
                        var newList = new ArrayList();
                        JIMarshalUnMarshalHelper.serialize(ndr, typeof(JIPointer), (JIPointer)listOfDefferedPointers[x], newList, (int)(int?)_inparamFlags[index]);
                        x++; //incrementing index
                        listOfDefferedPointers.AddRange(x, newList);
                    }
                    index++;
                }


            }
        }

        /// <summary>
        /// @exclude
        /// </summary>
        public override void read(NetworkDataRepresentation ndr) {
            //		if (opnum == 10) FOR TESTING ONLY
            //		{
            //			byte[] buffer = new byte[360];
            //			FileInputStream inputStream;
            //			try {
            //				inputStream = new FileInputStream("c:/temp/ONEEVENTSTRUCT");
            //				inputStream.read(buffer,0,360);
            //			} catch (Exception e) {
            //				// TODO Auto-generated catch block
            //				e.printStackTrace();
            //			}
            //			
            //			NdrBuffer ndrBuffer = new NdrBuffer(buffer,0);
            //			ndr.setBuffer(ndrBuffer);
            //			NetworkDataRepresentation ndr2 = new NetworkDataRepresentation();
            //			ndr2.setBuffer(ndrBuffer);
            //			read2(ndr2);
            //		}
            //interpret based on the out params flags
            if (!readOnlyHRESULT) {
                if (splCOMVersion) {
                    //during handshake and no other time. Kept for OxidResolver methods.
                    serverAlive2 = new JIComVersion(ndr.readUnsignedShort(), ndr.readUnsignedShort());
                    new JIPointer(new JIPointer(typeof(JIDualStringArray))).decode(ndr, new ArrayList(), JIFlags.FLAG_NULL, new Hashtable());
                    ndr.readUnsignedLong();
                }
                else {
                    var orpcThat = JIOrpcThat.decode(ndr);
                    readPacket(ndr, false);
                }
            }
            readResult(ndr);
        }

        /// <summary>
        /// called by only COMRuntime and NO ONE ELSE.
        /// 
        /// @exclude 
        /// </summary>
        /// <param name="ndr"> </param>
        internal virtual void read2(NetworkDataRepresentation ndr) {
            JIOrpcThis.decode(ndr);
            readPacket(ndr, true);
            //readResult(ndr);
            //hresult = 0;
        }

        private void readPacket(NetworkDataRepresentation ndr, bool fromCallback) {

            if (_session == null) {
                throw new InvalidOperationException("Programming Error ! Session not attached with this call ! ... Please rectify ! ");
            }

            var index = 0;

            _outparams = _outParams.ToArray();

            if (Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Verbose)) {
                var str = "";
                for (var i = 0; i < _outparams.Length; i++) {
                    str = str + "Out Param:[" + i + "]" + _outparams[i] + "\n";
                }
                Log.Logger.Verbose(str);
            }

            var comObjects = new ArrayList();
            IDictionary additionalData = new Hashtable();
            additionalData[CURRENTSESSION] = _session;
            additionalData[COMOBJECTS] = comObjects;
            var results = new ArrayList();
            //user has nothing to return.
            if (_outparams != null && _outparams.Length > 0) {
                while (index < _outparams.Length) {
                    IList listOfDefferedPointers = new ArrayList();
                    results.Add(JIMarshalUnMarshalHelper.deSerialize(ndr, _outparams[index], listOfDefferedPointers, (int)(int?)_outparamFlags[index], additionalData));
                    var x = 0;

                    while (x < listOfDefferedPointers.Count) {

                        var newList = new ArrayList();
                        var replacement = (JIPointer)JIMarshalUnMarshalHelper.deSerialize(ndr, (JIPointer)listOfDefferedPointers[x], newList, (int)(int?)_outparamFlags[index], additionalData);
                        ((JIPointer)listOfDefferedPointers[x]).replaceSelfWithNewPointer(replacement); //this should replace the value in the original place.
                        x++;
                        listOfDefferedPointers.AddRange(x, newList);
                    }
                    index++;
                }


                //now create the right COM Objects, it is required here only and no place else. 
                for (var i = 0; i < comObjects.Count; i++) {
                    var comObjectImpl = (JIComObjectImpl)comObjects[i];
                    try {
                        IJIComObject comObject = null;
                        if (fromCallback) {
                            //this is a new IP , so make a new JIComServer for this.
                            var newsession = JISession.createSession(_session);
                            newsession.GlobalSocketTimeout = _session.GlobalSocketTimeout;
                            newsession.useSessionSecurity(_session.SessionSecurityEnabled);
                            newsession.useNTLMv2(_session.NTLMv2Enabled);
                            var comServer = new JIComServer(newsession, comObjectImpl.internal_getInterfacePointer(), null);
                            comObject = comServer.Instance;
                            JIFrameworkHelper.link2Sessions(_session, newsession);
                        }
                        else {
                            if (comObjectImpl.internal_getInterfacePointer().CustomObjRef) {
                                continue;
                            }
                            comObject = JIFrameworkHelper.instantiateComObject2(_session, comObjectImpl.internal_getInterfacePointer());
                        }

                        comObjectImpl.replaceMembers(comObject);
                        JIFrameworkHelper.addComObjectToSession(comObjectImpl.AssociatedSession, comObjectImpl);
                        //Why did I put this here. We should do an addRef regardless of whether we give a pointer to COM or it gives us one.
                        //					if (!fromCallback)
                        {
                            comObjectImpl.addRef();
                        }

                    }
                    catch (JIException e) {
                        Log.Logger.Error(e, "JICallBuilder readPacket");
                        throw new JIRuntimeException(e.ErrorCode);
                    }
                    //replace the members of the original com objects by the completed ones.
                }

                comObjects.Clear();
            }

            _outparams = results.ToArray();
            _executed = true;
        }

        private void readResult(NetworkDataRepresentation ndr) {
            //last has to be the result.
            _hresult = ndr.readUnsignedLong();

            if (_hresult != 0) {
                //something exception occured at server, set up results
                _resultsOfException = _outparams;
                _outparams = null;
                throw new JIRuntimeException(_hresult);
            }
        }

        private int bufferLength() {
            var length = 0;
            object[] inparams = _inParams.ToArray();
            for (var i = 0; i < inparams.Length; i++) {
                if (inparams[i] == null) {
                    length = length + 4;
                    continue;
                }
                var length2 = JIMarshalUnMarshalHelper.getLengthInBytes(inparams[i].GetType(), inparams[i], JIFlags.FLAG_NULL);
                length = length + length2;
            }

            return length + 2048; //2K extra for alignments, if any.
        }

        /// <summary>
        ///Returns true incase the Call resulted in an exception, use getHRESULT to get the error code.
        /// 
        /// @return
        /// </summary>
        public virtual bool Error {
            get {
                checkIfCalled();
                return _hresult != 0;
            }
        }

        internal virtual void attachSession(JISession session) {
            _session = session;
        }

        internal virtual JISession Session => _session;

        private bool readOnlyHRESULT;
        internal virtual void setReadOnlyHRESULT() {
            readOnlyHRESULT = true;
        }

        private bool splCOMVersion;
        private JIComVersion serverAlive2;
        internal virtual void internal_COMVersion() {
            splCOMVersion = true;
        }

        internal virtual JIComVersion internal_getComVersion() {
            return serverAlive2;
        }
    }

}