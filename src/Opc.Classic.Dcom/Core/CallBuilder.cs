//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Core {
    using SharpInterop.Common;
    using SharpInterop.Rpc.Core;
    using Opc.Classic.Dcom.Internal;
    using Opc.Classic.Dcom.Internal.LegacyNdr;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class used for setting up information such as <code>[in]</code>,
    /// <code>[out]</code> parameters and the method number for executing
    /// a call to the COM server.
    /// Sample Usage :
    /// <code>
    /// <see cref="CallBuilder"/> obj = new <see cref="CallBuilder"/>();
    /// obj.ReInit();
    /// objOpnum = 0; // 0 based index, can be obtained from the IDL or the Type Library of COM server.
    ///
    /// obj.AddInParamAsString(new <see cref="ComString"/>("Go Vikings!"));
    /// obj.AddInParamAsInt(100);
    /// // handle is previously obtained <seealso cref="IComObject"/>
    /// object[] result = comObject.Call(obj);
    /// </code>
    /// [out] parameters can be added in a similar way.
    /// <code>
    /// obj.AddOutParamAsType(typeof(<see cref="Variant"/>));
    /// obj.AddOutParamAsObject(new <see cref="ComPointer"/>(typeof(short),true));
    /// </code>
    /// </summary>
    [Serializable]
    public class CallBuilder : NdrOp {

        /// <summary>
        /// From destroy
        /// </summary>
        internal bool FromDestroySession { get; set; }

#pragma warning disable RECS0154 // Parameter is never used
        /// <summary>
        /// Constructs a builder object.
        /// </summary>
        /// <param name="dispatchNotSupported"> <code>true</code> if <code>IDispatch</code> is
        /// not supported by the <code><see cref="IComObject"/></code> on which this builder would
        /// act. Use <seealso cref="IComObject.DispatchSupported"/> to find out if
        /// dispatch is supported on the COM Object. </param>
        public CallBuilder(bool dispatchNotSupported) : this() =>
#pragma warning restore RECS0154 // Parameter is never used
            _dispatchNotSupported = dispatchNotSupported;

        /// <summary>
        /// Constructs a builder object. It is assumed that <code>IDispatch</code>
        /// interface is supported by the <code><see cref="IComObject"/></code> on which this builder
        /// would act.
        /// </summary>
        public CallBuilder() {
            //        enclosingParentsIPID = IPIDofParent;
        }

        /// <summary>
        /// Reinitializes all members of this object. It is ready to be used again on a
        /// fresh <code><seealso cref="IComObject.Call(CallBuilder)"/></code> after this step.
        ///
        /// </summary>
        // after reinit, except parent, nothing is available.
        public void ReInit() {
            _opnum = -1;
            _inParams = new List<object>();
            _inparamFlags = new List<int>();
            _outParams = new List<object>();
            _outparamFlags = new List<int>();
            HRESULT = -1;
            _outparams = null;
            _executed = false;
        }

        internal string ParentIpid { set; get; }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code><see cref="IComObject"/></code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="comObject"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be) </param>
        public void AddInParamAsComObject(IComObject comObject, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsComObjectAt(_inParams.Count, comObject, flags);


        /// <summary>
        /// Add <code>[in]</code> parameter as <code>int</code> at the
        /// end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be) </param>
        public void AddInParamAsInt(int value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsIntAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>uint</code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be) </param>
        public void AddInParamAsUnsigned(uint value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsUnsignedAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>ushort</code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be) </param>
        public void AddInParamAsUnsigned(ushort value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsUnsignedAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>ulong</code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be) </param>
        public void AddInParamAsUnsigned(ulong value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsUnsignedAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>byte</code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be) </param>
        public void AddInParamAsUnsigned(byte value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsUnsignedAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>float</code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be) </param>
        public void AddInParamAsFloat(float value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsFloatAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>bool</code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be) </param>
        public void AddInParamAsBoolean(bool value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsBooleanAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>short</code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be) </param>
        public void AddInParamAsShort(short value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsShortAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>double</code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be) </param>
        public void AddInParamAsDouble(double value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsDoubleAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>char</code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be) </param>
        public void AddInParamAsCharacter(char value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsCharacterAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>char</code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be) </param>
        public void AddInParamAsSByte(sbyte value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsSByteAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>String</code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (These <i>HAVE</i> to be
        /// the <b>String</b> Flags).</param>
        public void AddInParamAsString(string value, int flags) =>
            InsertInParamAsStringAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code><see cref="Variant"/></code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be). </param>
        public void AddInParamAsVariant(Variant value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsVariantAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>Object</code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be). </param>
        public void AddInParamAsObject(object value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsObjectAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>String representation
        /// of UUID</code> at the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be). </param>
        public void AddInParamAsUUID(string value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsUUIDAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code><see cref="ComPointer"/></code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be). </param>
        public void AddInParamAsPointer(ComPointer value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsPointerAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code><see cref="Struct"/></code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be). </param>
        public void AddInParamAsStruct(Struct value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsStructAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>ComArray</code> at
        /// the end of the Parameter list.
        /// </summary>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be). </param>
        public void AddInParamAsArray(ComArray value, int flags = InteropFlags.FLAG_NULL) =>
            InsertInParamAsArrayAt(_inParams.Count, value, flags);

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>Object[]</code> at the
        /// end of the Parameter list.The array is iterated and
        /// all members appended to the list.
        /// </summary>
        /// <param name="values"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void SetInParams(object[] values, int flags = InteropFlags.FLAG_NULL) {
            for (var i = 0; i < values.Length; i++) {
                _inParams.Add(values[i]);
                // quite useless but do not want to change logic elsewhere
                _inparamFlags.Add(flags);
            }
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code><see cref="IComObject"/></code> at
        /// the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsComObjectAt(int index, IComObject value,
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>int</code> at the specified
        /// index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsIntAt(int index, int value, 
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>byte</code> at the
        /// specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsUnsignedAt(int index, byte value, 
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>ushort</code> at the
        /// specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsUnsignedAt(int index, ushort value,
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>uint</code> at the
        /// specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsUnsignedAt(int index, ulong value, 
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>float</code> at the
        /// specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsFloatAt(int index, float value, 
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>bool</code> at the
        /// specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsBooleanAt(int index, bool value, 
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, Convert.ToBoolean(value));
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>sbyte</code> at the
        /// specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsSByteAt(int index, sbyte value,
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>short</code> at the
        /// specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsShortAt(int index, short value, 
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>double</code> at the
        /// specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsDoubleAt(int index, double value,
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>char</code> at the
        /// specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsCharacterAt(int index, char value,
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>String</code>
        /// at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (These <i>HAVE</i> to be
        /// the <b>String</b> Flags). </param>
        public void InsertInParamAsStringAt(int index, string value, int flags) {
            _inParams.Insert(index, new ComString(value, flags));
            _inparamFlags.Insert(index, InteropFlags.FLAG_NULL);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code><see cref="Variant"/></code> at
        /// the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsVariantAt(int index, Variant value, 
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>Object</code> at
        /// the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsObjectAt(int index, object value,
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>String representation
        /// of UUID</code> at the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsUUIDAt(int index, string value, 
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, new UUID(value));
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code><see cref="ComPointer"/></code> at
        /// the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsPointerAt(int index, ComPointer value, 
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code><see cref="Struct"/></code> at
        /// the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsStructAt(int index, Struct value, 
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Add <code>[in]</code> parameter as <code>ComArray</code> at
        /// the specified index in the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="value"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void InsertInParamAsArrayAt(int index, ComArray value, 
            int flags = InteropFlags.FLAG_NULL) {
            _inParams.Insert(index, value);
            _inparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Removes <code>[in]</code> parameter at the specified index
        /// from the Parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RECS0154 // Parameter is never used
        public void RemoveInParamAt(int index, int flags = InteropFlags.FLAG_NULL) {
#pragma warning restore RECS0154 // Parameter is never used
#pragma warning restore IDE0060 // Remove unused parameter
            _inParams.RemoveAt(index);
            _inparamFlags.RemoveAt(index);
        }

        /// <summary>
        /// Returns <code>[in]</code> parameter at the specified index
        /// from the Parameter list.
        /// Will just provide 1 getter, for outParams there would be
        /// overloads like inParam setters.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <returns> Primitives are returned as there Derieved types.
        /// </returns>
        public object GetInParamAt(int index) => _inParams[index];

        /// <summary>
        /// Add <code>[out]</code> parameter of the type <code>clazz</code>
        /// at the end of the out parameter list.
        /// </summary>
        /// <param name="clazz"> </param>
        /// <param name="flags"> </param>
        public void AddOutParamAsType(Type clazz, int flags = InteropFlags.FLAG_NULL) =>
            InsertOutParamAt(_outParams.Count, clazz, flags);

        /// <summary>
        /// Add <code>[out]</code> parameter at the end of the out
        /// parameter list. Typically callers are
        /// composite in nature <see cref="Struct"/>, <see cref="Union"/>s, 
        /// <see cref="ComPointer"/> and <see cref="ComString"/> .
        /// </summary>
        /// <param name="outparam"> </param>
        /// <param name="flags"> </param>
        public void AddOutParamAsObject(object outparam, int flags = InteropFlags.FLAG_NULL) =>
            InsertOutParamAt(_outParams.Count, outparam, flags);

        /// <summary>
        /// insert an <code>[out]</code> parameter at the specified
        /// index in the out parameter list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="classOrInstance"> can be either a Class or
        /// an Object </param>
        /// <param name="flags"> </param>
        public void InsertOutParamAt(int index, object classOrInstance, 
            int flags = InteropFlags.FLAG_NULL) {
            _outParams.Insert(index, classOrInstance);
            _outparamFlags.Insert(index, flags);
        }

        /// <summary>
        /// Retrieves the <code>[out]</code> param at the index in
        /// the out parameters list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <returns>  </returns>
        public object GetOutParamAt(int index) => _outParams[index];

        /// <summary>
        ///Removes <code>[out]</code> parameter at the specified index
        ///from the out parameters list.
        /// </summary>
        /// <param name="index"> 0 based index </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RECS0154 // Parameter is never used
        public void RemoveOutParamAt(int index, int flags = InteropFlags.FLAG_NULL) {
#pragma warning restore RECS0154 // Parameter is never used
#pragma warning restore IDE0060 // Remove unused parameter
            _outParams.RemoveAt(index);
            _outparamFlags.RemoveAt(index);
        }

        /// <summary>
        /// Add <code>[out]</code> parameter as <code>Object[]</code>
        /// at the end of the Parameter list.
        /// The array is iterated and all members appended to the list.
        /// </summary>
        /// <param name="values"> </param>
        /// <param name="flags"> from <see cref="InteropFlags"/> (if need be).</param>
        public void SetOutParams(object[] values, int flags = InteropFlags.FLAG_NULL) {
            for (var i = 0; i < values.Length; i++) {
                _outParams.Add(values[i]);
                _outparamFlags.Add(flags);
            }

        }

        /// <summary>
        /// Returns the results as an <code>Object[]</code>.
        /// This array has to be iterated over to get the individual values.
        /// only valid before the interpretation of read, after that
        /// has actual values
        /// </summary>
        public object[] Results {
            get {
                CheckIfCalled();
                return _outparams;
            }
        }

        /// <summary>
        /// Returns the value as <code>int</code> at the index from
        /// the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public int GetResultAsIntAt(int index) {
            CheckIfCalled();
            return (int)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>float</code> at the index from
        /// the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public float GetResultAsFloatAt(int index) {
            CheckIfCalled();
            return (float)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>bool</code> at the index from
        /// the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public bool GetResultAsBooleanAt(int index) {
            CheckIfCalled();
            return (bool)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>short</code> at the index from
        /// the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public short GetResultAsShortAt(int index) {
            CheckIfCalled();
            return (short)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>double</code> at the index from
        /// the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public double GetResultAsDoubleAt(int index) {
            CheckIfCalled();
            return (double)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>char</code> at the index from
        /// the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public char GetResultAsCharacterAt(int index) {
            CheckIfCalled();
            return (char)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code><see cref="ComString"/></code> at the index
        /// from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public ComString GetResultAsStringAt(int index) {
            CheckIfCalled();
            return (ComString)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code><see cref="Variant"/></code> at the index
        /// from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public Variant GetResultAsVariantAt(int index) {
            CheckIfCalled();
            return (Variant)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>String representation of the
        /// UUID</code> at the index from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public string GetResultAsUUIDStrAt(int index) {
            CheckIfCalled();
            return ((UUID)_outparams[index]).ToString();
        }

        /// <summary>
        /// Returns the value as <code><see cref="ComPointer"/></code> at the index
        /// from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public ComPointer GetResultAsPointerAt(int index) {
            CheckIfCalled();
            return (ComPointer)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code><see cref="Struct"/></code> at the index
        /// from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public Struct GetResultAsStructAt(int index) {
            CheckIfCalled();
            return (Struct)_outparams[index];
        }

        /// <summary>
        /// Returns the value as <code>ComArray</code> at the index
        /// from the result list.
        /// </summary>
        /// <param name="index"> 0 based index</param>
        public ComArray GetResultAsArrayAt(int index) {
            CheckIfCalled();
            return (ComArray)_outparams[index];
        }

        /// <summary>
        /// Returns the results incase an exception occured.
        /// </summary>
        public object[] ResultsInCaseOfException {
            get {
                CheckIfCalled();
                return _resultsOfException;
            }
        }

        /// <summary>
        /// Returns the <code>HRESULT</code> of this operation. This
        /// should be zero for successful calls and non-zero for failures.
        /// </summary>
        public int HRESULT { get; private set; }

        /// <summary>
        /// Helper
        /// </summary>
        private void CheckIfCalled() {
            if (!_executed) {
                throw new InvalidOperationException(
                    Interop.GetLocalizedMessage(ErrorCode.INTEROP_API_INCORRECTLY_CALLED));
            }
        }

        /// <summary>
        /// Returns the entire <code>[in]</code> parameters list.
        /// </summary>
        public object[] InParams => _inParams.ToArray();

        /// <summary>
        /// Returns the entire <code>[out]</code> parameters list.
        /// </summary>
        public object[] OutParams => _outParams.ToArray();

        /// <summary>
        /// Returns the In Param flag.
        /// </summary>
        public int[] InparamFlags => _inparamFlags.Cast<int>().ToArray();

        /// <summary>
        /// Returns the Out Param flag.
        /// </summary>
        public int[] OutparamFlags => _outparamFlags.Cast<int>().ToArray();

        /// <summary>
        /// Returns the opnum of the API which will be invoked at the <code>COM</code> server.
        /// </summary>
        public override int Opnum {
            get =>
                // opnum is 3 as this is a COM interface and 0,1,2 are occupied by IUnknown
                // TODO remember this for extending com components also.
                _opnum;
            set {
                var dispatch = 0;
                if (!_dispatchNotSupported) {
                    dispatch = 4; // 4 apis.
                }
                _opnum = dispatch + value + 3; // 0,1,2, Q.I
            }
        }

        /// <inheritdoc/>
        public override void Write(NdrCodec ndr) {

            // reset buffer size here...
            // calculate rough length required length + 16 for the last bytes
            // plus adding 30 more for the verifier etc.
            ndr.Buffer.Buf = new byte[BufferLength() + 16];

            var orpcthis = new OrpcThis();
            orpcthis.Encode(ndr);

            WritePacket(ndr);

            // when it ends add 16 zeros.
            ndr.WriteUnsignedLong(0);
            ndr.WriteUnsignedLong(0);
            ndr.WriteUnsignedLong(0);
            ndr.WriteUnsignedLong(0);
        }

        // All Methods are 0 index based
        internal void Write2(NdrCodec ndr) {
            // reset buffer size here...
            // calculate rough length required length + 16 for the last bytes
            // plus adding 30 more for the verifier etc.
            ndr.Buffer.Buf = new byte[BufferLength() + 16 + 30];
            OrpcThat.Encode(ndr);
            WritePacket(ndr);
        }


        private void WritePacket(NdrCodec ndr) {
            if (Session == null) {
                throw new InvalidOperationException(
                    "Programming Error ! Session not attached with this call ! ... Please rectify ! ");
            }

            var inparams = _inParams.ToArray();

            var index = 0;
            if (inparams != null) {
                while (index < inparams.Length) {
                    var context = new CodecContext();
                    if (inparams[index] == null) {
                        MarshalUnMarshalHelper.Serialize(ndr, typeof(int), 0, context);
                    }
                    else {
                        context.Flag = _inparamFlags[index];
                        MarshalUnMarshalHelper.Serialize(ndr, inparams[index].GetType(), inparams[index], context);
                    }
                    context.EncodeDeferredPointers(ndr, false);
                    index++;
                }
            }
        }

        /// <inheritdoc/>
        public override void Read(NdrCodec ndr) {
            // interpret based on the out params flags
            if (!_readOnlyHRESULT) {
                if (_splCOMVersion) {
                    // during handshake and no other time. Kept for OxidResolver methods.
                    _serverAlive2 = new ComVersion(ndr.ReadUnsignedShort(), ndr.ReadUnsignedShort());
                    new ComPointer(new ComPointer(typeof(DualStringArray))).Decode(ndr, new CodecContext());
                    ndr.ReadUnsignedLong();
                }
                else {
                    _ = OrpcThat.Decode(ndr);
                    ReadPacket(ndr, false);
                }
            }
            ReadResult(ndr);
        }

        /// <summary>
        /// called by only COMRuntime and NO ONE ELSE.
        /// </summary>
        /// <param name="ndr"> </param>
        internal void Read2(NdrCodec ndr) {
            OrpcThis.Decode(ndr);
            ReadPacket(ndr, true);
            // readResult(ndr);
            // hresult = 0;
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="fromCallback"></param>
        private void ReadPacket(NdrCodec ndr, bool fromCallback) {

            if (Session == null) {
                throw new InvalidOperationException(
                    "Programming Error ! Session not attached with this call ! ... Please rectify ! ");
            }

            var index = 0;
            _outparams = _outParams.ToArray();

            if (Log.Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace)) {
                var str = "";
                for (var i = 0; i < _outparams.Length; i++) {
                    str = str + "Out Param:[" + i + "]" + _outparams[i] + "\n";
                }
                Log.Logger.Verbose(str);
            }

            var context = new CodecContext {
                CurrentSession = Session
            };
            var results = new List<object>();
            // user has nothing to return.
            if (_outparams != null && _outparams.Length > 0) {
                while (index < _outparams.Length) {
                    context.DefferedPointers = new List<ComPointer>();
                    context.Flag = _outparamFlags[index];
                    results.Add(MarshalUnMarshalHelper.Deserialize(ndr, _outparams[index], context));
                    System.Diagnostics.Debug.Assert(context.Flag == _outparamFlags[index]);
                    context.DecodeDeferredPointers(ndr);
                    index++;
                }

                // now create the right COM Objects, it is required here only and no place else.
                for (var i = 0; i < context.ComObjects.Count; i++) {
                    var comObjectImpl = (ComObjectImpl)context.ComObjects[i];
                    try {
                        IComObject comObject = null;
                        if (fromCallback) {
                            // this is a new IP, so make a new <see cref="ComServer"/> for this.
                            var newsession = Session.CreateSession(Session);
                            newsession.GlobalSocketTimeout = Session.GlobalSocketTimeout;
                            newsession.UseSessionSecurity(Session.SessionSecurityEnabled);
                            newsession.UseNTLMv2(Session.NTLMv2Enabled);
                            var comServer = new ComServer(newsession,
                                comObjectImpl.GetInterfacePointer(), null);
                            comObject = comServer.Instance;
                            FrameworkHelper.Link2Sessions(Session, newsession);
                        }
                        else {
                            if (comObjectImpl.GetInterfacePointer().IsCustomObjRef) {
                                continue;
                            }
                            comObject = FrameworkHelper.InstantiateComObject2(
                                Session, comObjectImpl.GetInterfacePointer());
                        }

                        comObjectImpl.ReplaceMembers(comObject);
                        FrameworkHelper.AddComObjectToSession(
                            comObjectImpl.AssociatedSession, comObjectImpl);
                        // Why did I put this here. We should do an addRef regardless of whether we give a pointer to COM or it gives us one.
                        //                    if (!fromCallback)
                        {
                            comObjectImpl.AddRef();
                        }

                    }
                    catch (InteropException e) {
                        Log.Logger.Error(e, "CallBuilder readPacket");
                        throw new InteropRuntimeException(e.ErrorCode);
                    }
                    // replace the members of the original com objects by the completed ones.
                }
                context.ComObjects.Clear();
            }

            _outparams = results.ToArray();
            _executed = true;
        }

        /// <summary>
        /// Read result
        /// </summary>
        /// <param name="ndr"></param>
        private void ReadResult(NdrCodec ndr) {
            // last has to be the result.
            HRESULT = ndr.ReadUnsignedLong();

            if (HRESULT != 0) {
                // something exception occured at server, set up results
                _resultsOfException = _outparams;
                _outparams = null;
                throw new InteropRuntimeException(HRESULT);
            }
        }

        /// <summary>
        /// Get buffer length
        /// </summary>
        /// <returns></returns>
        private int BufferLength() {
            var length = 0;
            var inparams = _inParams.ToArray();
            for (var i = 0; i < inparams.Length; i++) {
                if (inparams[i] == null) {
                    length += 4;
                    continue;
                }
                var length2 = MarshalUnMarshalHelper.GetLengthInBytes(
                    inparams[i].GetType(), inparams[i], InteropFlags.FLAG_NULL);
                length += length2;
            }

            return length + 2048; // 2K extra for alignments, if any.
        }

        /// <summary>
        /// Returns true incase the Call resulted in an exception, use getHRESULT to get the error code.
        /// </summary>
        public bool Error {
            get {
                CheckIfCalled();
                return HRESULT != 0;
            }
        }

        /// <summary>
        /// Attach
        /// </summary>
        /// <param name="session"></param>
        internal void AttachSession(Session session) =>
            Session = session;

        internal Session Session { get; private set; }

        internal void SetReadOnlyHRESULT() => _readOnlyHRESULT = true;

        internal void Internal_COMVersion() => _splCOMVersion = true;

        internal ComVersion Internal_getComVersion() => _serverAlive2;

        internal const string CURRENTSESSION = "CURRENTSESSION";
        internal const string COMOBJECTS = "COMOBJECTS";

        private bool _splCOMVersion;
        private ComVersion _serverAlive2;
        private bool _readOnlyHRESULT;
        private int _opnum = -1;
        private object[] _outparams;
        private readonly bool _dispatchNotSupported;
        private List<int> _inparamFlags = new List<int>();
        private List<int> _outparamFlags = new List<int>();
        private List<object> _inParams = new List<object>();
        private List<object> _outParams = new List<object>();
        private bool _executed;
        private object[] _resultsOfException;
    }
}