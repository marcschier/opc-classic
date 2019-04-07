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


	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JISystem = org.jinterop.dcom.common.JISystem;

	/// <summary>
	///<para>Represents a C++ array which can display both <i>conformant and standard</i> 
	/// behaviors. Since this class forms a wrapper on the actual array, the developer 
	/// is expected to provide complete and final arrays (of Objects) to this class. 
	/// Modifying the wrapped array afterwards <b>will</b> have unexpected results.
	///  </para> 
	/// <para>
	/// <i>Please refer to <b>MSExcel</b> examples for more details on how to use this 
	/// class.</i>
	/// </para>
	///  <para>
	/// <b>Note</b>: Wrapped Arrays can be at most two dimensional in nature. Above
	/// that is not supported by the library.
	/// 
	/// @since 1.0
	/// </para>
	/// </summary>
	[Serializable]
	public sealed class JIArray {


		private const long SerialVersionUID = -8267477025978489665L;
		private object MemberArray = null;
		private Type Clazz = null;
		private int[] UpperBounds_Renamed = null;
		private int Dimension = -1;
		private int NumElementsInAllDimensions_Renamed = 0;
		private bool IsConformant = false;
		private bool IsVarying = false;
		private bool IsConformantProxy = false;
		private bool IsVaryingProxy = false;
		private IList ConformantMaxCounts_Renamed = new List<object>(); //list of integers
		private object Template = null;
		private bool IsArrayOfCOMObjects_56DCOM = false;
		private int SizeOfNestedArrayInBytes = 0; //used in both encoding and decoding.

		private JIArray() {

		}

		/// <summary>
		///<para>Creates an array object of the type specified by <code>clazz</code>. This is used 
		/// to prepare a template for decoding an array of that type. Used only for setting as an 
		/// <code>[out]</code> parameter in a JICallBuilder. 
		/// </para>
		/// </para><para>
		/// For example:- <br>
		/// This call creates a template for a single dimension Integer array of size 10. 
		/// <code> 
		/// <br>
		/// JIArray array = new JIArray(Integer.class,new int[]{10},1,false);
		/// </code>
		/// <br>
		/// 
		/// </P> </summary>
		/// <param name="clazz"> class whose instances will be members of the deserialized array. </param>
		/// <param name="upperBounds"> highest index for each dimension. </param>
		/// <param name="dimension"> number of dimensions </param>
		/// <param name="isConformant"> declares whether the array is <i>conformant</i> or not. </param>
		/// <exception cref="IllegalArgumentException"> if <code>upperBounds</code> is supplied and its length
		/// is not equal to the <code>dimension</code> parameter. </exception>
		public JIArray(Type clazz, int[] upperBounds, int dimension, bool isConformant) {
			this.Clazz = clazz;
			Init2(upperBounds,dimension,isConformant,false);
		}

		/// <summary>
		///<P> Refer to <seealso cref="#JIArray(Class, int[], int, boolean)"/>
		/// </summary>
		/// <param name="clazz"> class whose instances will be members of the deserialized array. </param>
		/// <param name="upperBounds"> highest index for each dimension. </param>
		/// <param name="dimension"> number of dimensions </param>
		/// <param name="isConformant"> declares whether the array is <i>conformant</i> or not. </param>
		/// <param name="isVarying"> declares whether the array is <i>varying</i> or not. </param>
		/// <exception cref="IllegalArgumentException"> if <code>upperBounds</code> is supplied and its length
		/// is not equal to the <code>dimension</code> parameter.
		///  </exception>
		public JIArray(Type clazz, int[] upperBounds, int dimension, bool isConformant, bool isVarying) {
			this.Clazz = clazz;
			Init2(upperBounds,dimension,isConformant,isVarying);
		}


		/// <summary>
		///<para> Creates an array object with members of the type <code>template</code>. 
		/// This constructor is used to prepare a template for decoding an array and is
		/// exclusively for composites like <code>JIStruct</code>, <code>JIPointer</code>, 
		/// <code>JIUnion</code>, <code>JIString</code> where more information on the 
		/// structure of the composite is required before trying to deserialize it.
		/// 
		/// </para>
		/// <para>
		/// 
		///  Sample Usage:-
		///  <br>
		///  <code>
		///  JIStruct safeArrayBounds = new JIStruct(); <br>
		/// safeArrayBounds.addMember(Integer.class); <br>
		/// safeArrayBounds.addMember(Integer.class); <br><br>
		/// 
		/// //arraydesc <br>
		/// JIStruct arrayDesc = new JIStruct(); <br>
		/// //typedesc <br>
		/// JIStruct typeDesc = new JIStruct(); <br><br>
		/// 
		/// arrayDesc.addMember(typeDesc);<br>
		/// arrayDesc.addMember(Short.class);<br>
		/// arrayDesc.addMember(<b>new JIArray(safeArrayBounds,new int[]{1},1,true)</b>);<br>
		///  </code>
		///  </para> </summary>
		/// <param name="template"> can be only of the type <code>JIStruct</code>, <code>JIPointer</code>, 
		/// <code>JIUnion</code>, <code>JIString</code> </param>
		/// <param name="upperBounds"> highest index for each dimension. </param>
		/// <param name="dimension"> number of dimensions </param>
		/// <param name="isConformant"> declares whether the array is <i>conformant</i> or not. </param>
		/// <exception cref="IllegalArgumentException"> if <code>upperBounds</code> is supplied and its length
		/// is not equal to the <code>dimension</code> parameter. </exception>
		/// <exception cref="IllegalArgumentException"> if <code>template</code> is null or is not of the 
		/// specified types. </exception>
		//for structs, pointers , unions.
		public JIArray(object template, int[] upperBounds, int dimension, bool isConformant) {
			if (template == null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_ARRAY_TEMPLATE_NULL));
			}

			if (!template.GetType().Equals(typeof(JIStruct)) && !template.GetType().Equals(typeof(JIUnion)) && !template.GetType().Equals(typeof(JIPointer)) && !template.GetType().Equals(typeof(JIString))) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_ARRAY_INCORRECT_TEMPLATE_PARAM));
			}

			this.Template = template;
			this.Clazz = template.GetType();

			Init2(upperBounds,dimension,isConformant,false);
		}


		/// <summary>
		///<para> Refer to <seealso cref="#JIArray(Object, int[], int, boolean)"/> for details.
		/// 
		/// 
		/// </para>
		/// </summary>
		/// <param name="template"> can be only of the type <code>JIStruct</code>, <code>JIPointer</code>, 
		/// <code>JIUnion</code>, <code>JIString</code> </param>
		/// <param name="upperBounds"> highest index for each dimension. </param>
		/// <param name="dimension"> number of dimensions </param>
		/// <param name="isConformant"> declares whether the array is <i>conformant</i> or not. </param>
		/// <param name="isVarying"> declares whether the array is <i>varying</i> or not. </param>
		/// <exception cref="IllegalArgumentException"> if <code>upperBounds</code> is supplied and its length
		/// is not equal to the <code>dimension</code> parameter. </exception>
		/// <exception cref="IllegalArgumentException"> if <code>template</code> is null or is not of the 
		/// specified types. </exception>
		//for structs, pointers , unions.
		public JIArray(object template, int[] upperBounds, int dimension, bool isConformant, bool isVarying) {
			if (template == null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_ARRAY_TEMPLATE_NULL));
			}

			if (!template.GetType().Equals(typeof(JIStruct)) && !template.GetType().Equals(typeof(JIUnion)) && !template.GetType().Equals(typeof(JIPointer)) && !template.GetType().Equals(typeof(JIString))) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_ARRAY_INCORRECT_TEMPLATE_PARAM));
			}

			if (JISystem.COMVersion.MinorVersion == 6 && template.GetType().Equals(typeof(JIPointer))) {
				if (((JIPointer)template).GetReferent() == typeof(IJIComObject)) {
					//in this case this pointer will be a reference type pointer and not deffered one.
					//change in MS specs since DCOM 5.4
					IsArrayOfCOMObjects_56DCOM = true;
					((JIPointer)template).SetIsReferenceTypePtr();
				}
			}

			this.Template = template;
			this.Clazz = template.GetType();

			Init2(upperBounds,dimension,isConformant,isVarying);
		}

		private void Init2(int[] upperBounds, int dimension, bool isConformant, bool isVarying) {
			this.UpperBounds_Renamed = upperBounds;
			this.Dimension = dimension;
			this.IsConformant = isConformant;
			this.IsConformantProxy = isConformant;
			this.IsVarying = isVarying;
			this.IsVaryingProxy = isVarying;

			if (upperBounds != null) {
				//have to supply the upperbounds for each dimension , no gaps in between
				if (upperBounds.Length != dimension) {
					throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_ARRAY_UPPERBNDS_DIM_NOTMATCH));
				}
			}

			for (int i = 0;upperBounds != null && i < upperBounds.Length;i++) {
				NumElementsInAllDimensions_Renamed = NumElementsInAllDimensions_Renamed + upperBounds[i];
				if (isConformant) {
					ConformantMaxCounts_Renamed.Add(new int?(upperBounds[i]));
				}
			}

			//numElementsInAllDimensions = numElementsInAllDimensions * dimension;
		}

		/// <summary>
		///<para>Creates an object with <i>array</i> parameter as the nested Array. 
		/// This constructor is used when the developer wants to send an array to
		/// COM server.
		/// </para>
		/// <para>
		/// Sample Usage :- 
		/// <br>
		/// <code>
		/// JIArray array = new JIArray(new JIString[]{new JIString(name)},true); <br>
		/// </code>
		/// 
		/// </para>
		/// </summary>
		/// <param name="array"> Array of any type. Primitive arrays are not allowed. </param>
		/// <param name="isConformant"> declares whether the array is <code>conformant</code> or not. </param>
		/// <exception cref="IllegalArgumentException"> if the <code>array</code> is not an array or 
		/// is of primitive type or is an array of <code>java.lang.Object</code>. </exception>
		public JIArray(object array, bool isConformant) {
			this.IsConformant = isConformant;
			this.IsConformantProxy = isConformant;
			Init(array);
		}

		/// <summary>
		/// Refer <seealso cref="#JIArray(Object, boolean)"/> 
		/// </summary>
		/// <param name="array"> Array of any type. Primitive arrays are not allowed. </param>
		/// <param name="isConformant"> declares whether the array is <code>conformant</code> or not. </param>
		/// <param name="isVarying"> declares whether the array is <code>varying</code> or not. </param>
		/// <exception cref="IllegalArgumentException"> if the <code>array</code> is not an array or 
		/// is of primitive type or is an array of <code>java.lang.Object</code>. </exception>
		public JIArray(object array, bool isConformant, bool isVarying) {
			this.IsConformant = isConformant;
			this.IsConformantProxy = isConformant;
			this.IsVarying = isVarying;
			this.IsVaryingProxy = isVarying;
			Init(array);
		}

		/// <summary>
		///*<para>Creates an object with <i>array</i> parameter as the nested Array. 
		/// This constructor forms a <code>non-conformant</code> array and is used 
		/// when the developer wants to send an array to COM server.
		/// </para>
		/// <para>
		/// Sample Usage :- 
		/// <br>
		/// <code>
		/// JIArray array = new JIArray(new JIString[]{new JIString(name)},true); <br>
		/// </code>
		/// 
		/// </para>
		/// </summary>
		/// <param name="array"> Array of any type. Primitive arrays are not allowed. </param>
		/// <exception cref="IllegalArgumentException"> if the <code>array</code> is not an array or 
		/// is of primitive type or is an array of <code>java.lang.Object</code>. </exception>
		public JIArray(object array) {
			Init(array);
		}

		private void Init(object array) {
			if (!array.GetType().IsArray) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_ARRAY_PARAM_ONLY));
			}

			if (array.GetType().IsPrimitive) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_ARRAY_PRIMITIVE_NOTACCEPT));
			}

			//bad way...but what the heck...
			if (array.GetType().ToString().IndexOf("java.lang.Object", StringComparison.Ordinal) != -1) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_ARRAY_TYPE_INCORRECT));
			}

			this.MemberArray = array;

			List<object> upperBounds2 = new List<object>();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			string name = array.GetType().FullName;
			object subArray = array;
			NumElementsInAllDimensions_Renamed = 1;
			while (name.StartsWith("[", StringComparison.Ordinal)) {
				name = name.Substring(1);
				int x = ((object[])subArray).Length;
				upperBounds2.Add(new int?(x));
				NumElementsInAllDimensions_Renamed = NumElementsInAllDimensions_Renamed * x;
				if (IsConformant) {
					ConformantMaxCounts_Renamed.Add(new int?(x));
				}
				Clazz = subArray.GetType().GetElementType();
				if (x == 0) { //In which ever index the length is 0 , the array stops there, example Byte[0],Byte[0][10],Byte[10][0]
					break;
				}
				subArray = Array.get(subArray,0);
				Dimension++;
			}

			if (Dimension == -1) {
				NumElementsInAllDimensions_Renamed = 0;
				Dimension++;
			}

			UpperBounds_Renamed = new int[upperBounds2.Count];
			for (int i = 0;i < upperBounds2.Count; i++) {
				UpperBounds_Renamed[i] = (int)((int?)upperBounds2[i]);
			}
			Dimension++; //since it starts from -1.
			SizeOfNestedArrayInBytes = ComputeLengthArray(array);
		}

		private int ComputeLengthArray(object array) {
			int length = 0;
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			string name = array.GetType().FullName;
			object[] o = (object[])array;
			for (int i = 0;i < o.Length; i++) {
				if (name[1] != '[') {
					object[] o1 = (object[])array;
					for (int j = 0;j < o1.Length; j++) {
						length = length + JIMarshalUnMarshalHelper.GetLengthInBytes(o1.GetType().GetElementType(),o1[j],JIFlags.FLAG_NULL);
					}
					return length;
				}
				length = length + ComputeLengthArray(Array.get(array,i));
			}

			return length;
		}

		/// <summary>
		/// Returns the nested Array.
		/// </summary>
		/// <returns> array Object which can be type casted based on value returned by <seealso cref="#getArrayClass()"/>. </returns>
		public object ArrayInstance {
			get {
				return MemberArray;
			}
		}

		/// <summary>
		/// Class of the nested Array.
		/// </summary>
		/// <returns> <code>class</code>  </returns>
		public Type ArrayClass {
			get {
				return Clazz;
			}
		}

		/// <summary>
		/// Array of integers depicting highest index for each dimension.
		/// </summary>
		/// <returns> <code>int[]</code> </returns>
		public int[] UpperBounds {
			get {
				return UpperBounds_Renamed;
			}
		}

		/// <summary>
		/// Returns the dimensions of the Array.
		/// </summary>
		/// <returns> <code>int</code> </returns>
		public int Dimensions {
			get {
				return Dimension;
			}
		}

		public int SizeOfAllElementsInBytes {
			get {
		//		int length = numElementsInAllDimensions * JIMarshalUnMarshalHelper.getLengthInBytes(clazz,((Object[])memberArray)[0],JIFlags.FLAG_NULL);
    
				//this means that decode has created this array, and we need to compute the size to stay consistent.
				if (SizeOfNestedArrayInBytes == -1) {
					SizeOfNestedArrayInBytes = ComputeLengthArray(MemberArray);
				}
    
				return SizeOfNestedArrayInBytes;
			}
		}

		/// <summary>
		/// Returns array size in bytes
		/// 
		/// @return
		/// </summary>
	//	public int getArraySize()
	//	{
	//		return getSizeOfAllElementsInBytes();
	//	}


		public void Encode(NetworkDataRepresentation ndr, object array, IList defferedPointers, int FLAG) {
		//	ArrayList listofDefferedPointers = new ArrayList();

			if (IsConformantProxy) {
				//first write the max counts ...First to last dimension.
				int i = 0;
				while (i < ConformantMaxCounts_Renamed.Count) {
					JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),ConformantMaxCounts_Renamed[i],defferedPointers,FLAG);
					i++;
				}

				IsConformantProxy = false; //this is since encode is recursive.
			}

			if (IsVaryingProxy) {
				//write the offset and the actual count
				int i = 0;
				while (i < ConformantMaxCounts_Renamed.Count) {
					JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?(0),defferedPointers,FLAG); //offset
					JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),ConformantMaxCounts_Renamed[i],defferedPointers,FLAG); //actual count
					i++;
				}

				IsVaryingProxy = false; //this is since encode is recursive.
			}


//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			string name = array.GetType().FullName;
			object[] o = (object[])array;
			for (int i = 0;i < o.Length; i++) {
				if (name[1] != '[') {
					object[] o1 = (object[])array;
					for (int j = 0;j < o1.Length; j++) {
						JIMarshalUnMarshalHelper.Serialize(ndr,Clazz,o1[j],defferedPointers,FLAG | JIFlags.FLAG_REPRESENTATION_ARRAY);
					}
					return;
				}
				Encode(ndr,Array.get(array,i),defferedPointers,FLAG);
			}

		}

		/// <summary>
		/// Status whether the array is <code>conformant</code> or not.
		/// </summary>
		/// <returns> <code>true</code> is array is <code>conformant</code>. </returns>
		public bool Conformant {
			get {
				return IsConformant;
			}
			set {
				IsConformantProxy = value;
			}
		}

		/// <summary>
		/// Status whether the array is <code>varying</code> or not.
		/// </summary>
		/// <returns> <code>true</code> is array is <code>varying</code>. </returns>
		public bool Varying {
			get {
				return IsVarying;
			}
			set {
				IsVaryingProxy = value;
			}
		}


		public object Decode(NetworkDataRepresentation ndr, Type arrayType, int dimension, IList defferedPointers, int FLAG, IDictionary additionalData) {
			JIArray retVal = new JIArray();
			retVal.IsConformantProxy = IsConformantProxy;
			retVal.IsVaryingProxy = IsVaryingProxy;
			if (IsConformantProxy) {

				//first read the max counts ...First to last dimension.
				int i = 0;
				while (i < dimension) {
					retVal.ConformantMaxCounts_Renamed.Add(JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(int?),defferedPointers,FLAG,additionalData));
					i++;
				}

				//isConformantProxy = false; //this is since decode is recursive.

				if (UpperBounds_Renamed == null) {
					//max elements will come now.
					retVal.NumElementsInAllDimensions_Renamed = 0;
					retVal.UpperBounds_Renamed = new int[retVal.ConformantMaxCounts_Renamed.Count];
					i = 0;
					while (i < retVal.ConformantMaxCounts_Renamed.Count) {
						retVal.UpperBounds_Renamed[i] = (int)((int?)retVal.ConformantMaxCounts_Renamed[i]);
						retVal.NumElementsInAllDimensions_Renamed = retVal.NumElementsInAllDimensions_Renamed * retVal.UpperBounds_Renamed[i];
						i++;
					}
					if (i == 0) {
						NumElementsInAllDimensions_Renamed = 0;
					}
					//retVal.numElementsInAllDimensions = retVal.numElementsInAllDimensions * dimension;
				}
			}
			else
			{ //this is the case when it is non conformant or coming from struct.
				retVal.UpperBounds_Renamed = UpperBounds_Renamed;
				retVal.ConformantMaxCounts_Renamed = ConformantMaxCounts_Renamed;
				retVal.NumElementsInAllDimensions_Renamed = NumElementsInAllDimensions_Renamed;
			}

			if (IsVaryingProxy) {
				//first read the max counts ...First to last dimension.
				int i = 0;
				retVal.ConformantMaxCounts_Renamed.Clear(); //can't take the max count size now
				retVal.UpperBounds_Renamed = null;
				retVal.NumElementsInAllDimensions_Renamed = 0;

				while (i < dimension) {
					JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(int?),defferedPointers,FLAG,null); ///offset
					retVal.ConformantMaxCounts_Renamed.Add(JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(int?),defferedPointers,FLAG,additionalData)); //actual count
					i++;
				}

				//isConformantProxy = false; //this is since decode is recursive.

				if (UpperBounds_Renamed == null) {
					//max elements will come now.
					retVal.NumElementsInAllDimensions_Renamed = 1;
					retVal.UpperBounds_Renamed = new int[retVal.ConformantMaxCounts_Renamed.Count];
					i = 0;
					while (i < retVal.ConformantMaxCounts_Renamed.Count) {
						retVal.UpperBounds_Renamed[i] = (int)((int?)retVal.ConformantMaxCounts_Renamed[i]);
						retVal.NumElementsInAllDimensions_Renamed = retVal.NumElementsInAllDimensions_Renamed * retVal.UpperBounds_Renamed[i];
						i++;
					}
					if (i == 0) {
						NumElementsInAllDimensions_Renamed = 0;
					}
					//retVal.numElementsInAllDimensions = retVal.numElementsInAllDimensions * dimension;
				}

			}

			retVal.IsConformant = IsConformant;
			retVal.IsVarying = IsVarying;
			retVal.Template = Template;
			retVal.MemberArray = RecurseDecode(retVal,ndr,arrayType,dimension, defferedPointers,FLAG, additionalData);
			retVal.Clazz = Clazz;
			retVal.Dimension = this.Dimension;
			retVal.SizeOfNestedArrayInBytes = -1; // setting here so that when a call actually comes for it's lenght , the getLength will compute. This is required since while decoding many pointers are still not complete and their length cannot be decided.
			return retVal;
		}

		private object RecurseDecode(JIArray retVal, NetworkDataRepresentation ndr, Type arrayType, int dimension, IList defferedPointers, int FLAG, IDictionary additionalData) {
			object array = null;
			Type c = arrayType;
			for (int j = 0; j < dimension; j++) {
				array = Array.CreateInstance(c, retVal.UpperBounds_Renamed[retVal.UpperBounds_Renamed.Length - j - 1]);
				c = array.GetType();
			}

			for (int i = 0; i < retVal.UpperBounds_Renamed[retVal.UpperBounds_Renamed.Length - dimension] ; i++) {
				if (dimension == 1) {
					//fill value here
					//Array.set(array,i,new Float(i));
					if (Template == null) {
						((System.Array)array).SetValue(JIMarshalUnMarshalHelper.DeSerialize(ndr,c.GetElementType() == null ? c : c.GetElementType(),defferedPointers,FLAG | JIFlags.FLAG_REPRESENTATION_ARRAY,additionalData), i);
					}
					else {
						if (IsArrayOfCOMObjects_56DCOM) {
							//not setting the array flag here.
							((System.Array)array).SetValue(JIMarshalUnMarshalHelper.DeSerialize(ndr,Template,defferedPointers,FLAG,additionalData), i);
						}
						else {
							((System.Array)array).SetValue(JIMarshalUnMarshalHelper.DeSerialize(ndr,Template,defferedPointers,FLAG | JIFlags.FLAG_REPRESENTATION_ARRAY,additionalData), i);
						}
					}
				}
				else {
					((System.Array)array).SetValue(RecurseDecode(retVal,ndr,arrayType,dimension - 1,defferedPointers,FLAG,additionalData), i);
				}
			}

			return array;
		}

		/// <summary>
		///	Reverses Array elements for IJIDispatch.
		/// 
		/// @return
		/// </summary>
		public int ReverseArrayForDispatch() {
			if (MemberArray == null) {
				return 0;
			}


			int i = 0;
			Stack stack = new Stack();
			for (i = 0; i < ((object[])MemberArray).Length;i++) {
				stack.Push(((object[])MemberArray)[i]);
			}

			i = 0;
			while (stack.Count > 0) {
				((object[])MemberArray)[i++] = stack.Pop();
			}

			return i;
		}

		public IList ConformantMaxCounts {
			get {
				return ConformantMaxCounts_Renamed;
			}
		}



		public IList MaxCountAndUpperBounds {
			set {
				ConformantMaxCounts_Renamed = value;
			//	if (upperBounds == null) this will always be null since this api will get called from a decode and 
				//in that the upperBounds is always null, since one does not know the dim expected.
				if (ConformantMaxCounts_Renamed.Count > 0) {
					//max elements will come now.
					NumElementsInAllDimensions_Renamed = 1;
					UpperBounds_Renamed = new int[ConformantMaxCounts_Renamed.Count];
					int i = 0;
					while (i < ConformantMaxCounts_Renamed.Count) {
						UpperBounds_Renamed[i] = (int)((int?)ConformantMaxCounts_Renamed[i]);
						NumElementsInAllDimensions_Renamed = NumElementsInAllDimensions_Renamed * UpperBounds_Renamed[i];
						i++;
					}
					if (i == 0) {
						NumElementsInAllDimensions_Renamed = 0;
					}
				}
				else {
					UpperBounds_Renamed = null;
					NumElementsInAllDimensions_Renamed = 0;
				}
			}
		}

		public int NumElementsInAllDimensions {
			get {
				return NumElementsInAllDimensions_Renamed;
			}
		}

		/// <summary>
		///<para>Used only from the JIVariant.getDecodedValueAsArray. It is required when the real class of the array is determined after the SafeArray Struct has been 
		/// processed. SA in COM can contain these along with normal types as well :- 
		/// FADF_BSTR 0x0100 An array of BSTRs. <br>
		/// FADF_UNKNOWN 0x0200 An array of IUnknown*. <br>  
		/// FADF_DISPATCH 0x0400 An array of IDispatch*.  <br>
		/// FADF_VARIANT 0x0800 An array of VARIANTs. <br>
		/// 
		/// I have noticed that the "type" of the array doesn't always convey the right thing, so this "feature" flag of the SA shas to be looked into.
		/// As can be seen above except only BSTR require a template others do not. But the logic for the JIString(BSTR) already works fine. So I will use this
		/// flag only to set the JIVariant.class , whereever the "type" does not specify it but the "feature" does.    
		/// </para>
		/// @exclude </summary>
		/// <param name="c"> </param>
		public void UpdateClazz(Type c) {
			Clazz = c;
		}

		public override string ToString() {
			string retVal = "[Type: " + Clazz + " , ";
			if (MemberArray == null) {
				retVal += "memberArray is null , ";
			}
			else {
				retVal += MemberArray + " , ";
			}

			if (IsConformant) {
				retVal += " conformant , ";
			}
			if (IsVarying) {
				retVal += " varying , ";
			}

			return retVal + "]";
		}
	}

}