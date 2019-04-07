using System;
using System.Collections;
using System.Collections.Generic;

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


	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	using JIComVersion = org.jinterop.dcom.common.JIComVersion;
	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JIException = org.jinterop.dcom.common.JIException;
	using JIRuntimeException = org.jinterop.dcom.common.JIRuntimeException;
	using JISystem = org.jinterop.dcom.common.JISystem;

	using UUID = rpc.core.UUID;

	/// <summary>
	///<para>Class used for setting up information such as <code>[in]</code>
	/// ,<code>[out]</code> parameters and the method number for executing a call to the 
	/// COM server. 
	/// </para>
	/// <para> Sample Usage :-
	/// <code>
	///  <br>
	///  JICallBuilder obj = new JICallBuilder(); <br>
	///  obj.reInit(); <br>
	/// obj.setOpnum(0); //0 based index, can be obtained from the IDL or the Type Library of COM server.
	/// <br>
	/// obj.addInParamAsString(new JIString("j-Interop Rocks !"), JIFlags.FLAG_NULL); <br>
	/// obj.addInParamAsInt(100, JIFlags.FLAG_NULL); <br>
	/// //handle is previously obtained <seealso cref="IJIComObject"/> <br>
	/// Object[] result = comObject.call(obj); 
	/// <br>
	/// </code>
	/// <br><code>[out]</code> parameters can be added in a similar way.<br>
	/// <code>
	///  obj.addOutParamAsType(JIVariant.class,JIFlags.FLAG_NULL); <br>
	///  obj.addOutParamAsObject(new JIPointer(Short.class,true),JIFlags.FLAG_NULL); <br>
	/// </code>
	/// <br>
	/// </para>
	/// @since 2.0 (formerly <code>JICallObject</code>)
	/// </summary>
	[Serializable]
	public class JICallBuilder : NdrObject {

		internal const string CURRENTSESSION = "CURRENTSESSION";
		internal const string COMOBJECTS = "COMOBJECTS";

		private const long SerialVersionUID = -2939657500731135110L;
		private int Opnum_Renamed = -1;
		private object[] Outparams = null;
		private bool DispatchNotSupported = false;
		private string EnclosingParentsIPID = null;
		private List<object> InparamFlags_Renamed = new List<object>();
		private List<object> OutparamFlags_Renamed = new List<object>();
		private List<object> InParams_Renamed = new List<object>();
		private List<object> OutParams_Renamed = new List<object>();
		private int Hresult = 0;
		private bool Executed = false;
		private object[] ResultsOfException = null;
		private JISession Session_Renamed = null;
		internal bool FromDestroySession = false;

		/// <summary>
		/// Constructs a builder object.
		/// </summary>
		/// <param name="dispatchNotSupported"> <code>true</code> if <code>IDispatch</code> is 
		/// not supported by the <code>IJIComObject</code> on which this builder would
		/// act. Use <seealso cref="IJIComObject#isDispatchSupported()"/> to find out if 
		/// dispatch is supported on the COM Object. </param>
		public JICallBuilder(bool dispatchNotSupported) : this() {
			this.DispatchNotSupported = dispatchNotSupported;
		}

		/// <summary>
		///<para> Constructs a builder object. It is assumed that <code>IDispatch</code>
		/// interface is supported by the <code>IJIComObject</code> on which this builder
		/// would act.
		/// 
		/// </para>
		/// </summary>
		public JICallBuilder() {
	//		enclosingParentsIPID = IPIDofParent;
		}

		/// <summary>
		/// Reinitializes all members of this object. It is ready to be used again on a 
		/// fresh <code><seealso cref="IJIComObject#call"/></code> after this step. 
		/// 
		/// </summary>
		//after reinit, except parent, nothing is available.
		public virtual void ReInit() {
			Opnum_Renamed = -1;
			InParams_Renamed = new List<object>();
			InparamFlags_Renamed = new List<object>();
			OutParams_Renamed = new List<object>();
			OutparamFlags_Renamed = new List<object>();
			Hresult = -1;
			Outparams = null;
			Executed = false;
		}

		public virtual string ParentIpid {
			set {
				EnclosingParentsIPID = value;
			}
			get {
				return EnclosingParentsIPID;
			}
		}


	//	/**Add IN parameter as <code>JIInterfacePointer</code> at the end of the Parameter list.
	//	 * 
	//	 * @param value
	//	 * @param FLAGS from JIFlags (if need be)
	//	 */
	//	public void addInParamAsInterfacePointer(JIInterfacePointer interfacePointer, int FLAGS)
	//	{
	//		insertInParamAsInterfacePointerAt(inParams.size(),interfacePointer,FLAGS);
	//	}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>IJIComObject</code> at the end of the Parameter list.
		/// </summary>
		/// <param name="comObject"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be) </param>
		public virtual void AddInParamAsComObject(IJIComObject comObject, int FLAGS) {
			InsertInParamAsComObjectAt(InParams_Renamed.Count,comObject,FLAGS);
		}


		/// <summary>
		///Add <code>[in]</code> parameter as <code>int</code> at the end of the Parameter list.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be) </param>
		public virtual void AddInParamAsInt(int value, int FLAGS) {
			InsertInParamAsIntAt(InParams_Renamed.Count,value,FLAGS);
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>IJIUnsigned</code> at the end of the Parameter list.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be) </param>
		public virtual void AddInParamAsUnsigned(IJIUnsigned value, int FLAGS) {
			InsertInParamAsUnsignedAt(InParams_Renamed.Count,value,FLAGS);
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>float</code> at the end of the Parameter list.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be) </param>
		public virtual void AddInParamAsFloat(float value, int FLAGS) {
			InsertInParamAsFloatAt(InParams_Renamed.Count,value,FLAGS);
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>boolean</code> at the end of the Parameter list.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be) </param>
		public virtual void AddInParamAsBoolean(bool value, int FLAGS) {
			InsertInParamAsBooleanAt(InParams_Renamed.Count,value,FLAGS);
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>short</code> at the end of the Parameter list.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be) </param>
		public virtual void AddInParamAsShort(short value, int FLAGS) {
			InsertInParamAsShortAt(InParams_Renamed.Count,value,FLAGS);
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>double</code> at the end of the Parameter list.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be) </param>
		public virtual void AddInParamAsDouble(double value, int FLAGS) {
			InsertInParamAsDoubleAt(InParams_Renamed.Count,value,FLAGS);
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>char</code> at the end of the Parameter list.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be) </param>
		public virtual void AddInParamAsCharacter(char value, int FLAGS) {
			InsertInParamAsCharacterAt(InParams_Renamed.Count,value,FLAGS);
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>String</code> at the end of the Parameter list.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (These <i>HAVE</i> to be the <b>String</b> Flags). </param>
		//flags have to be String flags
		public virtual void AddInParamAsString(string value, int FLAGS) {
			InsertInParamAsStringAt(InParams_Renamed.Count,value,FLAGS);
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>JIVariant</code> at the end of the Parameter list.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be). </param>
		public virtual void AddInParamAsVariant(JIVariant value, int FLAGS) {
			InsertInParamAsVariantAt(InParams_Renamed.Count,value,FLAGS);
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>Object</code> at the end of the Parameter list.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be). </param>
		public virtual void AddInParamAsObject(object value, int FLAGS) {
			InsertInParamAsObjectAt(InParams_Renamed.Count,value,FLAGS);
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>String representation of UUID</code> at the end of the Parameter list.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be). </param>
		public virtual void AddInParamAsUUID(string value, int FLAGS) {
			InsertInParamAsUUIDAt(InParams_Renamed.Count,value,FLAGS);
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>JIPointer</code> at the end of the Parameter list.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be). </param>
		public virtual void AddInParamAsPointer(JIPointer value, int FLAGS) {
			InsertInParamAsPointerAt(InParams_Renamed.Count,value,FLAGS);
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>JIStruct</code> at the end of the Parameter list.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be). </param>
		public virtual void AddInParamAsStruct(JIStruct value, int FLAGS) {
			InsertInParamAsStructAt(InParams_Renamed.Count,value,FLAGS);
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>JIArray</code> at the end of the Parameter list.
		/// </summary>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be). </param>
		public virtual void AddInParamAsArray(JIArray value, int FLAGS) {
			InsertInParamAsArrayAt(InParams_Renamed.Count,value,FLAGS);
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>Object[]</code> at the end of the Parameter list.The array is iterated and
		/// all members appended to the list.
		/// </summary>
		/// <param name="values"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void SetInParams(object[] values, int FLAGS) {
			for (int i = 0;i < values.Length;i++) {
				InParams_Renamed.Add(values[i]);
				InparamFlags_Renamed.Add(new int?(FLAGS)); //quite useless but do not want to change logic elsewhere
			}

		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>IJIComObject</code> at the specified index in the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void InsertInParamAsComObjectAt(int index, IJIComObject value, int FLAGS) {
			InParams_Renamed.Insert(index,value);
			InparamFlags_Renamed.Insert(index,new int?(FLAGS));
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>int</code> at the specified index in the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void InsertInParamAsIntAt(int index, int value, int FLAGS) {
			InParams_Renamed.Insert(index,new int?(value));
			InparamFlags_Renamed.Insert(index,new int?(FLAGS));
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>IJIUnsigned</code> at the specified index in the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void InsertInParamAsUnsignedAt(int index, IJIUnsigned value, int FLAGS) {
			InParams_Renamed.Insert(index,value);
			InparamFlags_Renamed.Insert(index,new int?(FLAGS));
		}


		/// <summary>
		///Add <code>[in]</code> parameter as <code>float</code> at the specified index in the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void InsertInParamAsFloatAt(int index, float value, int FLAGS) {
			InParams_Renamed.Insert(index,new float?(value));
			InparamFlags_Renamed.Insert(index,new int?(FLAGS));
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>boolean</code> at the specified index in the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void InsertInParamAsBooleanAt(int index, bool value, int FLAGS) {
			InParams_Renamed.Insert(index,Convert.ToBoolean(value));
			InparamFlags_Renamed.Insert(index,new int?(FLAGS));
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>short</code> at the specified index in the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void InsertInParamAsShortAt(int index, short value, int FLAGS) {
			InParams_Renamed.Insert(index, new short?(value));
			InparamFlags_Renamed.Insert(index,new int?(FLAGS));
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>double</code> at the specified index in the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void InsertInParamAsDoubleAt(int index, double value, int FLAGS) {
			InParams_Renamed.Insert(index, new double?(value));
			InparamFlags_Renamed.Insert(index,new int?(FLAGS));
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>char</code> at the specified index in the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void InsertInParamAsCharacterAt(int index, char value, int FLAGS) {
			InParams_Renamed.Insert(index, new char?(value));
			InparamFlags_Renamed.Insert(index,new int?(FLAGS));
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>String</code>  at the specified index in the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (These <i>HAVE</i> to be the <b>String</b> Flags). </param>
		//flags have to be String flags
		public virtual void InsertInParamAsStringAt(int index, string value, int FLAGS) {
			InParams_Renamed.Insert(index, new JIString(value,FLAGS));
			InparamFlags_Renamed.Insert(index,new int?(JIFlags.FLAG_NULL));
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>JIVariant</code> at the specified index in the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void InsertInParamAsVariantAt(int index, JIVariant value, int FLAGS) {
			InParams_Renamed.Insert(index, value);
			InparamFlags_Renamed.Insert(index,new int?(JIFlags.FLAG_NULL));
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>Object</code> at the specified index in the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		//this is for dispatch, etc...more or less will never be used.
		public virtual void InsertInParamAsObjectAt(int index, object value, int FLAGS) {
			InParams_Renamed.Insert(index, value);
			InparamFlags_Renamed.Insert(index,new int?(FLAGS));
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>String representation of UUID</code> at the specified index in the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void InsertInParamAsUUIDAt(int index, string value, int FLAGS) {
			InParams_Renamed.Insert(index, new UUID(value));
			InparamFlags_Renamed.Insert(index,new int?(FLAGS));
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>JIPointer</code> at the specified index in the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void InsertInParamAsPointerAt(int index, JIPointer value, int FLAGS) {
			InParams_Renamed.Insert(index, value);
			InparamFlags_Renamed.Insert(index,new int?(FLAGS));
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>JIStruct</code> at the specified index in the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void InsertInParamAsStructAt(int index, JIStruct value, int FLAGS) {
			InParams_Renamed.Insert(index, value);
			InparamFlags_Renamed.Insert(index,new int?(FLAGS));
		}

		/// <summary>
		///Add <code>[in]</code> parameter as <code>JIArray</code> at the specified index in the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="value"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void InsertInParamAsArrayAt(int index, JIArray value, int FLAGS) {
			InParams_Renamed.Insert(index, value);
			InparamFlags_Renamed.Insert(index,new int?(FLAGS));
		}

		/// <summary>
		///Removes <code>[in]</code> parameter at the specified index from the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void RemoveInParamAt(int index, int FLAGS) {
			object value = InParams_Renamed.Remove(index);
			InparamFlags_Renamed.RemoveAt(index);
		}

		/// <summary>
		///Returns <code>[in]</code> parameter at the specified index from the Parameter list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <returns> Primitives are returned as there Derieved types.  </returns>
		//Will just provide 1 getter, for outParams there would be overloads like inParam setters.
		public virtual object GetInParamAt(int index) {
			return InParams_Renamed[index];
		}

		/// <summary>
		/// Add <code>[out]</code> parameter of the type <code>clazz</code> at the end of the out parameter list.
		/// </summary>
		/// <param name="clazz"> </param>
		/// <param name="FLAGS"> </param>
		public virtual void AddOutParamAsType(Type clazz, int FLAGS) {
			InsertOutParamAt(OutParams_Renamed.Count,clazz,FLAGS);
		}

		/// <summary>
		/// Add <code>[out]</code> parameter at the end of the out parameter list. Typically callers are <br> 
		/// composite in nature JIStruct, JIUnions, JIPointer and JIString . 
		/// </summary>
		/// <param name="outparam"> </param>
		/// <param name="FLAGS"> </param>
		public virtual void AddOutParamAsObject(object outparam, int FLAGS) {
			InsertOutParamAt(OutParams_Renamed.Count,outparam,FLAGS);
		}

		/// <summary>
		/// insert an <code>[out]</code> parameter at the specified index in the out parameter list. 
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="classOrInstance"> can be either a Class or an Object </param>
		/// <param name="FLAGS"> </param>
		public virtual void InsertOutParamAt(int index, object classOrInstance, int FLAGS) {
			OutParams_Renamed.Insert(index, classOrInstance);
			OutparamFlags_Renamed.Insert(index,new int?(FLAGS));
		}

		/// <summary>
		/// Retrieves the <code>[out]</code> param at the index in the out parameters list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <returns>  </returns>
		public virtual object GetOutParamAt(int index) {
			return OutParams_Renamed[index];
		}

		/// <summary>
		///Removes <code>[out]</code> parameter at the specified index from the out parameters list.
		/// </summary>
		/// <param name="index"> 0 based index </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void RemoveOutParamAt(int index, int FLAGS) {
			OutParams_Renamed.RemoveAt(index);
			OutparamFlags_Renamed.RemoveAt(index);
		}

		/// <summary>
		///Add <code>[out]</code> parameter as <code>Object[]</code> at the end of the Parameter list. The array is iterated and
		/// all members appended to the list. 
		/// </summary>
		/// <param name="values"> </param>
		/// <param name="FLAGS"> from JIFlags (if need be).  </param>
		public virtual void SetOutParams(object[] values, int FLAGS) {
			for (int i = 0;i < values.Length;i++) {
				OutParams_Renamed.Add(values[i]);
				OutparamFlags_Renamed.Add(new int?(FLAGS));
			}

		}

		//now for the results

		/// <summary>
		/// Returns the results as an <code>Object[]</code>. This array has to be iterated over to get the individual values.
		/// </summary>
		//	only valid before the interpretation of read, after that has actual values
		public virtual object[] Results {
			get {
				//checkIfCalled();
				return Outparams;
			}
		}

		/// <summary>
		/// Returns the value as <code>int</code> at the index from the result list. 
		/// </summary>
		/// <param name="index"> 0 based index
		/// @return </param>
		public virtual int GetResultAsIntAt(int index) {
			CheckIfCalled();
			return (int)((int?)Outparams[index]);
		}

		/// <summary>
		/// Returns the value as <code>float</code> at the index from the result list. 
		/// </summary>
		/// <param name="index"> 0 based index
		/// @return </param>
		public virtual float GetResultAsFloatAt(int index) {
			CheckIfCalled();
			return (float)((float?)Outparams[index]);
		}

		/// <summary>
		/// Returns the value as <code>boolean</code> at the index from the result list. 
		/// </summary>
		/// <param name="index"> 0 based index
		/// @return </param>
		public virtual bool GetResultAsBooleanAt(int index) {
			CheckIfCalled();
			return (bool)((bool?)Outparams[index]);
		}

		/// <summary>
		/// Returns the value as <code>short</code> at the index from the result list. 
		/// </summary>
		/// <param name="index"> 0 based index
		/// @return </param>
		public virtual short GetResultAsShortAt(int index) {
			CheckIfCalled();
			return (short)((short?)Outparams[index]);
		}

		/// <summary>
		/// Returns the value as <code>double</code> at the index from the result list. 
		/// </summary>
		/// <param name="index"> 0 based index
		/// @return </param>
		public virtual double GetResultAsDoubleAt(int index) {
			CheckIfCalled();
			return (double)((double?)Outparams[index]);
		}

		/// <summary>
		/// Returns the value as <code>char</code> at the index from the result list. 
		/// </summary>
		/// <param name="index"> 0 based index
		/// @return </param>
		public virtual char GetResultAsCharacterAt(int index) {
			CheckIfCalled();
			return (char)((char?)Outparams[index]);
		}

		/// <summary>
		/// Returns the value as <code>JIString</code> at the index from the result list. 
		/// </summary>
		/// <param name="index"> 0 based index
		/// @return </param>
		public virtual JIString GetResultAsStringAt(int index) {
			CheckIfCalled();
			return ((JIString)Outparams[index]);
		}

		/// <summary>
		/// Returns the value as <code>JIVariant</code> at the index from the result list. 
		/// </summary>
		/// <param name="index"> 0 based index
		/// @return </param>
		public virtual JIVariant GetResultAsVariantAt(int index) {
			CheckIfCalled();
			return ((JIVariant)Outparams[index]);
		}

		/// <summary>
		/// Returns the value as <code>String representation of the UUID</code> at the index from the result list. 
		/// </summary>
		/// <param name="index"> 0 based index
		/// @return </param>
		public virtual string GetResultAsUUIDStrAt(int index) {
			CheckIfCalled();
			return ((UUID)Outparams[index]).ToString();
		}

		/// <summary>
		/// Returns the value as <code>JIPointer</code> at the index from the result list. 
		/// </summary>
		/// <param name="index"> 0 based index
		/// @return </param>
		public virtual JIPointer GetResultAsPointerAt(int index) {
			CheckIfCalled();
			return ((JIPointer)Outparams[index]);
		}

		/// <summary>
		/// Returns the value as <code>JIStruct</code> at the index from the result list. 
		/// </summary>
		/// <param name="index"> 0 based index
		/// @return </param>
		public virtual JIStruct GetResultAsStructAt(int index) {
			CheckIfCalled();
			return ((JIStruct)Outparams[index]);
		}

		/// <summary>
		/// Returns the value as <code>JIArray</code> at the index from the result list. 
		/// </summary>
		/// <param name="index"> 0 based index
		/// @return </param>
		public virtual JIArray GetResultAsArrayAt(int index) {
			CheckIfCalled();
			return ((JIArray)Outparams[index]);
		}

		/// <summary>
		/// Returns the results incase an exception occured. 
		/// 
		/// @return
		/// </summary>
		public virtual object[] ResultsInCaseOfException {
			get {
				//checkIfCalled();
				return ResultsOfException;
			}
		}

		/// <summary>
		/// Returns the <code>HRESULT</code> of this operation. This should be zero for successful calls and
		/// non-zero for failures.
		/// 
		/// @return
		/// </summary>
		public virtual int HRESULT {
			get {
				return Hresult;
			}
		}

		private void CheckIfCalled() {
			if (!Executed) {
				throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_API_INCORRECTLY_CALLED));
			}
		}

		/// <summary>
		/// Returns the entire <code>[in]</code> parameters list. 
		/// 
		/// @return
		/// </summary>
		public virtual object[] InParams {
			get {
				return InParams_Renamed.ToArray();
			}
		}

		/// <summary>
		/// Returns the entire <code>[out]</code> parameters list. 
		/// 
		/// @return
		/// </summary>
		public virtual object[] OutParams {
			get {
				return OutParams_Renamed.ToArray();
			}
		}

		/// <summary>
		/// Returns the In Param flag.
		/// 
		/// @return
		/// </summary>
		public virtual int?[] InparamFlags {
			get {
				return (int?[])InparamFlags_Renamed.ToArray(typeof(int?));
			}
		}

		/// <summary>
		/// Returns the Out Param flag.
		/// 
		/// @return
		/// </summary>
		public virtual int?[] OutparamFlags {
			get {
				return (int?[])OutparamFlags_Renamed.ToArray(typeof(int?));
			}
		}

		/// <summary>
		/// Returns the opnum of the API which will be invoked at the <code>COM</code> server. 
		/// 
		/// </summary>
		public virtual int Opnum {
			get {
				//opnum is 3 as this is a COM interface and 0,1,2 are occupied by IUnknown
				//TODO remember this for extending com components also.
				return Opnum_Renamed;
			}
			set {
				int dispatch = 0;
				if (!DispatchNotSupported) {
					dispatch = 4; //4 apis.
				}
				Opnum_Renamed = dispatch + value + 3; //0,1,2, Q.I
			}
		}

		//All Methods are 0 index based


		public virtual void Write2(NetworkDataRepresentation ndr) {
			//reset buffer size here...
			//calculate rough length required length + 16 for the last bytes
			//plus adding 30 more for the verifier etc. 
			ndr.Buffer.buf = new sbyte[BufferLength() + 16 + 30];
			JIOrpcThat.Encode(ndr);
			WritePacket(ndr);
		}

		/// <summary>
		/// @exclude
		/// </summary>
		public virtual void Write(NetworkDataRepresentation ndr) {

			//reset buffer size here...
			//calculate rough length required length + 16 for the last bytes
			//plus adding 30 more for the verifier etc. 
			ndr.Buffer.buf = new sbyte[BufferLength() + 16];

			JIOrpcThis orpcthis = new JIOrpcThis();
			orpcthis.Encode(ndr);

			WritePacket(ndr);

			//when it ends add 16 zeros.
			ndr.writeUnsignedLong(0);
			ndr.writeUnsignedLong(0);
			ndr.writeUnsignedLong(0);
			ndr.writeUnsignedLong(0);

		}

		private void WritePacket(NetworkDataRepresentation ndr) {
			if (Session_Renamed == null) {
				throw new System.InvalidOperationException("Programming Error ! Session not attached with this call ! ... Please rectify ! ");
			}

			object[] inparams = InParams_Renamed.ToArray();

			int index = 0;
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
					IList listOfDefferedPointers = new List<object>();
					if (inparams[index] == null) {
						JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?(0),listOfDefferedPointers,JIFlags.FLAG_NULL);
					}
					else {
						JIMarshalUnMarshalHelper.Serialize(ndr,inparams[index].GetType(),inparams[index],listOfDefferedPointers,(int)((int?)InparamFlags_Renamed[index]));
					}

					int x = 0;

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
						List<object> newList = new List<object>();
						JIMarshalUnMarshalHelper.Serialize(ndr,typeof(JIPointer),(JIPointer)listOfDefferedPointers[x],newList,(int)((int?)InparamFlags_Renamed[index]));
						x++; //incrementing index
						listOfDefferedPointers.AddRange(x,newList);
					}
					index++;
				}


			}
		}

		/// <summary>
		/// @exclude
		/// </summary>
		public virtual void Read(NetworkDataRepresentation ndr) {
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
			if (!ReadOnlyHRESULT) {
				if (SplCOMVersion) {
					//during handshake and no other time. Kept for OxidResolver methods.
					ServerAlive2 = new JIComVersion(ndr.readUnsignedShort(), ndr.readUnsignedShort());
					(new JIPointer(new JIPointer(typeof(JIDualStringArray)))).Decode(ndr, new List<object>(), JIFlags.FLAG_NULL, new Hashtable());
					ndr.readUnsignedLong();
				}
				else {
					JIOrpcThat orpcThat = JIOrpcThat.Decode(ndr);
					ReadPacket(ndr,false);
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
		public virtual void Read2(NetworkDataRepresentation ndr) {
			JIOrpcThis.Decode(ndr);
			ReadPacket(ndr,true);
			//readResult(ndr);
			//hresult = 0;
		}

		private void ReadPacket(NetworkDataRepresentation ndr, bool fromCallback) {

			if (Session_Renamed == null) {
				throw new System.InvalidOperationException("Programming Error ! Session not attached with this call ! ... Please rectify ! ");
			}

			int index = 0;

			Outparams = OutParams_Renamed.ToArray();

			if (JISystem.Logger.isLoggable(Level.FINEST)) {
				string str = "";
				for (int i = 0;i < Outparams.Length;i++) {
					str = str + "Out Param:[" + i + "]" + Outparams[i] + "\n";
				}

				JISystem.Logger.finest(str);
			}

			List<object> comObjects = new List<object>();
			IDictionary additionalData = new Hashtable();
			additionalData[CURRENTSESSION] = Session_Renamed;
			additionalData[COMOBJECTS] = comObjects;
			List<object> results = new List<object>();
			//user has nothing to return.
			if (Outparams != null && Outparams.Length > 0) {
				while (index < Outparams.Length) {
					IList listOfDefferedPointers = new List<object>();
					results.Add(JIMarshalUnMarshalHelper.DeSerialize(ndr,Outparams[index],listOfDefferedPointers,(int)((int?)OutparamFlags_Renamed[index]),additionalData));
					int x = 0;

					while (x < listOfDefferedPointers.Count) {

						List<object> newList = new List<object>();
						JIPointer replacement = (JIPointer)JIMarshalUnMarshalHelper.DeSerialize(ndr,(JIPointer)listOfDefferedPointers[x],newList,(int)((int?)OutparamFlags_Renamed[index]),additionalData);
						((JIPointer)listOfDefferedPointers[x]).ReplaceSelfWithNewPointer(replacement); //this should replace the value in the original place.
						x++;
						listOfDefferedPointers.AddRange(x,newList);
					}
					index++;
				}


				//now create the right COM Objects, it is required here only and no place else. 
				for (int i = 0; i < comObjects.Count; i++) {
					JIComObjectImpl comObjectImpl = (JIComObjectImpl)comObjects[i];
					try {
						IJIComObject comObject = null;
						if (fromCallback) {
							//this is a new IP , so make a new JIComServer for this.
							JISession newsession = JISession.CreateSession(Session_Renamed);
							newsession.GlobalSocketTimeout = Session_Renamed.GlobalSocketTimeout;
							newsession.UseSessionSecurity(Session_Renamed.SessionSecurityEnabled);
							newsession.UseNTLMv2(Session_Renamed.NTLMv2Enabled);
							JIComServer comServer = new JIComServer(newsession,comObjectImpl.Internal_getInterfacePointer(),null);
							comObject = comServer.Instance;
							JIFrameworkHelper.Link2Sessions(Session_Renamed, newsession);
						}
						else {
							if (comObjectImpl.Internal_getInterfacePointer().CustomObjRef) {
								continue;
							}
							comObject = JIFrameworkHelper.InstantiateComObject2(Session_Renamed, comObjectImpl.Internal_getInterfacePointer());
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
						JISystem.Logger.throwing("JICallBuilder", "readPacket", e);
						throw new JIRuntimeException(e.ErrorCode);
					}
					//replace the members of the original com objects by the completed ones.
				}

				comObjects.Clear();
			}

			Outparams = results.ToArray();
			Executed = true;
		}

		private void ReadResult(NetworkDataRepresentation ndr) {
			//last has to be the result.
			Hresult = ndr.readUnsignedLong();

			if (Hresult != 0) {
				//something exception occured at server, set up results
				ResultsOfException = Outparams;
				Outparams = null;
				throw new JIRuntimeException(Hresult);
			}
		}

		private int BufferLength() {
			int length = 0;
			object[] inparams = InParams_Renamed.ToArray();
			for (int i = 0; i < inparams.Length;i++) {
				if (inparams[i] == null) {
					length = length + 4;
					continue;
				}
				int length2 = JIMarshalUnMarshalHelper.GetLengthInBytes(inparams[i].GetType(),inparams[i],JIFlags.FLAG_NULL);
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
				CheckIfCalled();
				return Hresult != 0;
			}
		}

		public virtual void AttachSession(JISession session) {
			this.Session_Renamed = session;
		}

		public virtual JISession Session {
			get {
				return Session_Renamed;
			}
		}

		private bool ReadOnlyHRESULT = false;
		public virtual void SetReadOnlyHRESULT() {
			ReadOnlyHRESULT = true;
		}

		private bool SplCOMVersion = false;
		private JIComVersion ServerAlive2 = null;
		public virtual void Internal_COMVersion() {
			SplCOMVersion = true;
		}

		public virtual JIComVersion Internal_getComVersion() {
			return ServerAlive2;
		}
	}

}