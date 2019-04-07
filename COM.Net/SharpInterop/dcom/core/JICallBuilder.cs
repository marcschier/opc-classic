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
    using org.jinterop.dcom.common;
    using rpc.core;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using SharpCifs.Util.Sharpen;
    using System.Linq;

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
    public class JICallBuilder : NdrOp {

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
        /// fresh <code><seealso cref="IJIComObject.Call(JICallBuilder)"/></code> after this step.
        ///
        /// </summary>
        //after reinit, except parent, nothing is available.
        public virtual void ReInit() {
            _opnum = -1;
            _inParams = new List<object>();
            _inparamFlags = new List<object>();
            _outParams = new List<object>();
            _outparamFlags = new List<object>();
            _hresult = -1;
            _outparams = null;
            _executed = false;
        }

        internal virtual string ParentIpid {
            set => _enclosingParentsIPID = value;
            get => _enclosingParentsIPID;
        }


         /// <summary>
        /// Add <code>[in]</code> parameter as <code>IJIComObject</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="comObject"> </param>
        /// <param name="flags"> from JIFlags (if need be) </param>
        public virtual void AddInParamAsComObject(IJIComObject comObject, int flags) {
            InsertInParamAsComObjectAt(_inParams.Count, comObject, flags);
        }


        /// <summary>
        /// Add <code>[in]</code> parameter as <code>int</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be) </param>
        public virtual void AddInParamAsInt(int value, int flags) {
            InsertInParamAsIntAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>IJIUnsigned</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be) </param>
        public virtual void AddInParamAsUnsigned(IJIUnsigned value, int flags) {
            InsertInParamAsUnsignedAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>float</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be) </param>
        public virtual void AddInParamAsFloat(float value, int flags) {
            InsertInParamAsFloatAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>bool</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be) </param>
        public virtual void AddInParamAsBoolean(bool value, int flags) {
            InsertInParamAsBooleanAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>short</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be) </param>
        public virtual void AddInParamAsShort(short value, int flags) {
            InsertInParamAsShortAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>double</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be) </param>
        public virtual void AddInParamAsDouble(double value, int flags) {
            InsertInParamAsDoubleAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>char</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be) </param>
        public virtual void AddInParamAsCharacter(char value, int flags) {
            InsertInParamAsCharacterAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>String</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (These <i>HAVE</i> to be the <b>String</b> Flags).</param>
        public virtual void AddInParamAsString(string value, int flags) {
            InsertInParamAsStringAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>JIVariant</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be). </param>
        public virtual void AddInParamAsVariant(JIVariant value, int flags) {
            InsertInParamAsVariantAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>Object</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be). </param>
        public virtual void AddInParamAsObject(object value, int flags) {
            InsertInParamAsObjectAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>String representation of UUID</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be). </param>
        public virtual void AddInParamAsUUID(string value, int flags) {
            InsertInParamAsUUIDAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>JIPointer</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be). </param>
        public virtual void AddInParamAsPointer(JIPointer value, int flags) {
            InsertInParamAsPointerAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>JIStruct</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be). </param>
        public virtual void AddInParamAsStruct(JIStruct value, int flags) {
            InsertInParamAsStructAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>JIArray</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be). </param>
        public virtual void AddInParamAsArray(JIArray value, int flags) {
            InsertInParamAsArrayAt(_inParams.Count, value, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>Object[]</code> at the end of the Parameter list.The array is iterated and
        /// all members appended to the list.
        /// </summary>
        /// <param name="values"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void SetInParams(object[] values, int flags) {
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
        public virtual void InsertInParamAsComObjectAt(int index, IJIComObject value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>int</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void InsertInParamAsIntAt(int index, int value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>IJIUnsigned</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void InsertInParamAsUnsignedAt(int index, IJIUnsigned value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>float</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void InsertInParamAsFloatAt(int index, float value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>bool</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void InsertInParamAsBooleanAt(int index, bool value, int flags) {
            _inParams.Insert(index, Convert.ToBoolean(value));
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>short</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void InsertInParamAsShortAt(int index, short value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>double</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void InsertInParamAsDoubleAt(int index, double value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>char</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void InsertInParamAsCharacterAt(int index, char value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>String</code>  at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (These <i>HAVE</i> to be the <b>String</b> Flags). </param>
        public virtual void InsertInParamAsStringAt(int index, string value, int flags) {
            _inParams.Insert(index, new JIString(value, flags));
            _inparamFlags.Insert(index, JIFlags.FLAG_NULL);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>JIVariant</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void InsertInParamAsVariantAt(int index, JIVariant value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, JIFlags.FLAG_NULL);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>Object</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void InsertInParamAsObjectAt(int index, object value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>String representation of UUID</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void InsertInParamAsUUIDAt(int index, string value, int flags) {
            _inParams.Insert(index, new UUID(value));
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>JIPointer</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void InsertInParamAsPointerAt(int index, JIPointer value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>JIStruct</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void InsertInParamAsStructAt(int index, JIStruct value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>JIArray</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void InsertInParamAsArrayAt(int index, JIArray value, int flags) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Removes <code>[in]</code> parameter at the specified index from the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void RemoveInParamAt(int index, int flags) {
            _inParams.RemoveAt(index);
            _inparamFlags.RemoveAt(index);
        }

        /// <summary>
        /// Returns <code>[in]</code> parameter at the specified index from the Parameter list.
        /// Will just provide 1 getter, for outParams there would be overloads like inParam setters.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <returns> Primitives are returned as there Derieved types.  </returns>
        public virtual object GetInParamAt(int index) {
            return _inParams[index];
        }

        /// <summary>
        /// Add <code>[out]</code> parameter of the type <code>clazz</code> at the end of the out parameter list.
        /// </summary>
        /// <param name="clazz"> </param>
        /// <param name="flags"> </param>
        public virtual void AddOutParamAsType(Type clazz, int flags) {
            InsertOutParamAt(_outParams.Count, clazz, flags);
        }

        /// <summary>
        /// Add <code>[out]</code> parameter at the end of the out parameter list. Typically callers are
        /// composite in nature JIStruct, JIUnions, JIPointer and JIString .
        /// </summary>
        /// <param name="outparam"> </param>
        /// <param name="flags"> </param>
        public virtual void AddOutParamAsObject(object outparam, int flags) {
            InsertOutParamAt(_outParams.Count, outparam, flags);
        }

        /// <summary>
        /// insert an <code>[out]</code> parameter at the specified index in the out parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="classOrInstance"> can be either a Class or an Object </param>
        /// <param name="flags"> </param>
        public virtual void InsertOutParamAt(int index, object classOrInstance, int flags) {
            _outParams.Insert(index, classOrInstance);
            _outparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Retrieves the <code>[out]</code> param at the index in the out parameters list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <returns>  </returns>
        public virtual object GetOutParamAt(int index) {
            return _outParams[index];
        }

        /// <summary>
        ///Removes <code>[out]</code> parameter at the specified index from the out parameters list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void RemoveOutParamAt(int index, int flags) {
            _outParams.RemoveAt(index);
            _outparamFlags.RemoveAt(index);
        }

        /// <summary>
        /// Add <code>[out]</code> parameter as <code>Object[]</code> at the end of the Parameter list.
        /// The array is iterated and all members appended to the list.
        /// </summary>
        /// <param name="values"> </param>
        /// <param name="flags"> from JIFlags (if need be).  </param>
        public virtual void SetOutParams(object[] values, int flags) {
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
                CheckIfCalled();
                return _outparams;
            }
        }

        /// <summary>
        /// Returns the value as <code>int</code> at the index from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual int GetResultAsIntAt(int index) {
            CheckIfCalled();
            return (int)(int?)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>float</code> at the index from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual float GetResultAsFloatAt(int index) {
            CheckIfCalled();
            return (float)(float?)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>bool</code> at the index from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual bool GetResultAsBooleanAt(int index) {
            CheckIfCalled();
            return (bool)(bool?)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>short</code> at the index from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual short GetResultAsShortAt(int index) {
            CheckIfCalled();
            return (short)(short?)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>double</code> at the index from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual double GetResultAsDoubleAt(int index) {
            CheckIfCalled();
            return (double)(double?)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>char</code> at the index from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual char GetResultAsCharacterAt(int index) {
            CheckIfCalled();
            return (char)(char?)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>JIString</code> at the index from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual JIString GetResultAsStringAt(int index) {
            CheckIfCalled();
            return (JIString)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>JIVariant</code> at the index from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual JIVariant GetResultAsVariantAt(int index) {
            CheckIfCalled();
            return (JIVariant)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>String representation of the UUID</code> at the index from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual string GetResultAsUUIDStrAt(int index) {
            CheckIfCalled();
            return ((UUID)_outparams[index]).ToString();
        }

        /// <summary>
        /// Returns the value as <code>JIPointer</code> at the index from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual JIPointer GetResultAsPointerAt(int index) {
            CheckIfCalled();
            return (JIPointer)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>JIStruct</code> at the index from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual JIStruct GetResultAsStructAt(int index) {
            CheckIfCalled();
            return (JIStruct)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>JIArray</code> at the index from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public virtual JIArray GetResultAsArrayAt(int index) {
            CheckIfCalled();
            return (JIArray)_outparams[index];
        }

        /// <summary>
        /// Returns the results incase an exception occured.
        /// </summary>
        public virtual object[] ResultsInCaseOfException {
            get {
                CheckIfCalled();
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
        private void CheckIfCalled() {
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
        public virtual int[] InparamFlags => _inparamFlags.Cast<int>().ToArray();

        /// <summary>
        /// Returns the Out Param flag.
        /// </summary>
        public virtual int[] OutparamFlags => _outparamFlags.Cast<int>().ToArray();

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

        internal virtual void Write2(NdrCodec ndr) {
            //reset buffer size here...
            //calculate rough length required length + 16 for the last bytes
            //plus adding 30 more for the verifier etc.
            ndr.Buffer.Buf = new byte[BufferLength() + 16 + 30];
            JIOrpcThat.Encode(ndr);
            WritePacket(ndr);
        }

        /// <summary>
        /// @exclude
        /// </summary>
        public override void Write(NdrCodec ndr) {

            //reset buffer size here...
            //calculate rough length required length + 16 for the last bytes
            //plus adding 30 more for the verifier etc.
            ndr.Buffer.Buf = new byte[BufferLength() + 16];

            var orpcthis = new JIOrpcThis();
            orpcthis.Encode(ndr);

            WritePacket(ndr);

            //when it ends add 16 zeros.
            ndr.WriteUnsignedLong(0);
            ndr.WriteUnsignedLong(0);
            ndr.WriteUnsignedLong(0);
            ndr.WriteUnsignedLong(0);
        }

        private void WritePacket(NdrCodec ndr) {
            if (_session == null) {
                throw new InvalidOperationException("Programming Error ! Session not attached with this call ! ... Please rectify ! ");
            }

            var inparams = _inParams.ToArray();

            var index = 0;
            if (inparams != null) {
        //	if (JISystem.getLogger().isLoggable(Level.FINEST))
        //	{
        //		String str = "";
        //		for (int i = 0;i < inparams.length;i++)
        //		{
        //			str = str + "In Param:[" + i + "] " + inparams[i] + "\n";
        //		}
        //		JISystem.getLogger().finest(str);
        //	}
                while (index < inparams.Length) {
                    var listOfDefferedPointers = new List<object>();
                    if (inparams[index] == null) {
                        JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), 0, listOfDefferedPointers, JIFlags.FLAG_NULL);
                    }
                    else {
                        JIMarshalUnMarshalHelper.Serialize(ndr, inparams[index].GetType(), inparams[index], listOfDefferedPointers, (int)_inparamFlags[index]);
                    }

                    var x = 0;

                    while (x < listOfDefferedPointers.Count) {
        //	thought of this today morning...change the logic here...the defeered pointers need to be
        //	completely serialized here. If they are also having nested deffered pointers then  those pointers
        //	should be "inserted" just after the current pointer itself.
        //	change the logic below to send out a new list and insert that list after the current x.
        //	consider the case when there is a Struct having a nested pointer to another struct and this struct
        //	itself having a pointer.
        //
        //	Inparams order:- for 2 params.
        //	int f,Struct{int i;
        //				 Struct *ptr;
        //				 Struct *ptr2;
        //				 int j;
        //				}
        //
        //	while serializing this struct the pointer 1 will get deffered and so will pointer 2. Now while writing
        //	the deffered pointers , we will find that the pointer 1 is pointing to a struct which has another deffered pointer (pointer to another struct maybe)
        //	in such case, the current logic will add the deffered pointer to the end of the listOfDefferedPointers list, effectively serializing it
        //	after the pointer 2 referent. But that is what is against the rules of DCERPC, in this case the referent of pointer 1 (struct with the pointer to another struct)
        //	should be serialized in place (following th rules of the struct serialization ofcourse) and should not go to the end of the list.
                        //JIMarshalUnMarshalHelper.Serialize(ndr,JIPointer.class,(JIPointer)listOfDefferedPointers.get(x),listOfDefferedPointers,inparamFlags);
                        var newList = new List<object>();
                        JIMarshalUnMarshalHelper.Serialize(ndr, typeof(JIPointer), (JIPointer)listOfDefferedPointers[x], newList, (int)_inparamFlags[index]);
                        x++; //incrementing index
                        listOfDefferedPointers.InsertRange(x, newList);
                    }
                    index++;
                }


            }
        }

        /// <summary>
        /// @exclude
        /// </summary>
        public override void Read(NdrCodec ndr) {
            //interpret based on the out params flags
            if (!readOnlyHRESULT) {
                if (splCOMVersion) {
                    //during handshake and no other time. Kept for OxidResolver methods.
                    serverAlive2 = new JIComVersion(ndr.ReadUnsignedShort(), ndr.ReadUnsignedShort());
                    new JIPointer(new JIPointer(typeof(JIDualStringArray))).Decode(ndr,
                        new List<object>(), JIFlags.FLAG_NULL, new Hashtable());
                    ndr.ReadUnsignedLong();
                }
                else {
                    _ = JIOrpcThat.Decode(ndr);
                    ReadPacket(ndr, false);
                }
            }
            ReadResult(ndr);
        }

        /// <summary>
        /// called by only COMRuntime and NO ONE ELSE.
        ///
        /// @exclude
        /// </summary>
        /// <param name="ndr"> </param>
        internal virtual void Read2(NdrCodec ndr) {
            JIOrpcThis.Decode(ndr);
            ReadPacket(ndr, true);
            //readResult(ndr);
            //hresult = 0;
        }

        private void ReadPacket(NdrCodec ndr, bool fromCallback) {

            if (_session == null) {
                throw new InvalidOperationException(
                    "Programming Error ! Session not attached with this call ! ... Please rectify ! ");
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

            var comObjects = new List<object>();
            var additionalData = new Hashtable {
                [CURRENTSESSION] = _session,
                [COMOBJECTS] = comObjects
            };
            var results = new List<object>();
            //user has nothing to return.
            if (_outparams != null && _outparams.Length > 0) {
                while (index < _outparams.Length) {
                    var listOfDefferedPointers = new List<object>();
                    results.Add(JIMarshalUnMarshalHelper.Deserialize(ndr, _outparams[index],
                        listOfDefferedPointers, (int)_outparamFlags[index], additionalData));

                    var x = 0;
                    while (x < listOfDefferedPointers.Count) {

                        var newList = new List<object>();
                        var replacement = (JIPointer)JIMarshalUnMarshalHelper.Deserialize(ndr,
                            (JIPointer)listOfDefferedPointers[x], newList, (int)_outparamFlags[index], additionalData);

                        //this should replace the value in the original place.
                        ((JIPointer)listOfDefferedPointers[x]).ReplaceSelfWithNewPointer(replacement);
                        x++;
                        listOfDefferedPointers.InsertRange(x, newList);
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
                            var comServer = new JIComServer(newsession, comObjectImpl.Internal_getInterfacePointer(), null);
                            comObject = comServer.Instance;
                            JIFrameworkHelper.Link2Sessions(_session, newsession);
                        }
                        else {
                            if (comObjectImpl.Internal_getInterfacePointer().CustomObjRef) {
                                continue;
                            }
                            comObject = JIFrameworkHelper.InstantiateComObject2(_session, comObjectImpl.Internal_getInterfacePointer());
                        }

                        comObjectImpl.ReplaceMembers(comObject);
                        JIFrameworkHelper.AddComObjectToSession(comObjectImpl.AssociatedSession, comObjectImpl);
                        //Why did I put this here. We should do an addRef regardless of whether we give a pointer to COM or it gives us one.
                        //					if (!fromCallback)
                        {
                            comObjectImpl.AddRef();
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

        private void ReadResult(NdrCodec ndr) {
            //last has to be the result.
            _hresult = ndr.ReadUnsignedLong();

            if (_hresult != 0) {
                //something exception occured at server, set up results
                _resultsOfException = _outparams;
                _outparams = null;
                throw new JIRuntimeException(_hresult);
            }
        }

        private int BufferLength() {
            var length = 0;
            var inparams = _inParams.ToArray();
            for (var i = 0; i < inparams.Length; i++) {
                if (inparams[i] == null) {
                    length += 4;
                    continue;
                }
                var length2 = JIMarshalUnMarshalHelper.GetLengthInBytes(inparams[i].GetType(), inparams[i], JIFlags.FLAG_NULL);
                length += length2;
            }

            return length + 2048; //2K extra for alignments, if any.
        }

        /// <summary>
        /// Returns true incase the Call resulted in an exception, use getHRESULT to get the error code.
        /// </summary>
        public virtual bool Error {
            get {
                CheckIfCalled();
                return _hresult != 0;
            }
        }

        internal virtual void AttachSession(JISession session) {
            _session = session;
        }

        internal virtual JISession Session => _session;

        private bool readOnlyHRESULT;
        internal virtual void SetReadOnlyHRESULT() {
            readOnlyHRESULT = true;
        }

        private bool splCOMVersion;
        private JIComVersion serverAlive2;
        internal virtual void Internal_COMVersion() {
            splCOMVersion = true;
        }

        internal virtual JIComVersion Internal_getComVersion() {
            return serverAlive2;
        }

        internal const string CURRENTSESSION = "CURRENTSESSION";
        internal const string COMOBJECTS = "COMOBJECTS";

        private int _opnum = -1;
        private object[] _outparams;
        private readonly bool _dispatchNotSupported;
        private string _enclosingParentsIPID;
        private List<object> _inparamFlags = new List<object>();
        private List<object> _outparamFlags = new List<object>();
        private List<object> _inParams = new List<object>();
        private List<object> _outParams = new List<object>();
        private int _hresult;
        private bool _executed;
        private object[] _resultsOfException;
        private JISession _session;
        internal bool _fromDestroySession;
    }
}