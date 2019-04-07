using System;

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

	using IJIUnreferenced = org.jinterop.dcom.common.IJIUnreferenced;
	using JIException = org.jinterop.dcom.common.JIException;

	/// <summary>
	/// Internal Framework class.
	/// 
	/// @exclude 
	/// @since 1.0
	/// 
	/// </summary>
	[Serializable]
	public class JIComObjectImplWrapper : IJIComObject {

		/// 
		private const long SerialVersionUID = 6142976024482507753L;
		protected internal readonly IJIComObject ComObject;

		public JIComObjectImplWrapper(IJIComObject comObject) {
			this.ComObject = comObject;
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJIComObject queryInterface(String iid) throws org.jinterop.dcom.common.JIException
		public virtual IJIComObject QueryInterface(string iid) {

			return ComObject.QueryInterface(iid);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addRef() throws org.jinterop.dcom.common.JIException
		public virtual void AddRef() {
			ComObject.AddRef();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void release() throws org.jinterop.dcom.common.JIException
		public virtual void Release() {
			ComObject.Release();
		}

		public virtual string Ipid {
			get {
				return ComObject.Ipid;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] call(JICallBuilder obj) throws org.jinterop.dcom.common.JIException
		public virtual object[] Call(JICallBuilder obj) {
			return ComObject.Call(obj);
		}

		public virtual JIInterfacePointer Internal_getInterfacePointer() {
			return ComObject.Internal_getInterfacePointer();
		}

		public virtual JISession AssociatedSession {
			get {
				return ComObject.AssociatedSession;
			}
		}

		/// <summary>
		/// Returns the <i>IID</i> of this object
		/// </summary>
		/// <returns> String representation of 128 bit uuid. </returns>
		public virtual string InterfaceIdentifier {
			get {
				return ComObject.InterfaceIdentifier;
			}
		}

	//	/**
	//	 * @exclude
	//	 */
	//	public JIComServer getAssociatedComServer()
	//	{
	//		return comObject.getAssociatedComServer();
	//	}

		public virtual bool DispatchSupported {
			get {
				return ComObject.DispatchSupported;
			}
		}


		public virtual string Internal_setConnectionInfo(IJIComObject connectionPoint, int? cookie) {
			return ComObject.Internal_setConnectionInfo(connectionPoint,cookie);
		}


		public virtual object[] Internal_getConnectionInfo(string identifier) {
			return ComObject.Internal_getConnectionInfo(identifier);
		}


		public virtual object[] Internal_removeConnectionInfo(string identifier) {
			return ComObject.Internal_removeConnectionInfo(identifier);
		}


		public virtual IJIUnreferenced UnreferencedHandler {
			get {
				return ComObject.UnreferencedHandler;
			}
		}


		public virtual void RegisterUnreferencedHandler(IJIUnreferenced unreferenced) {
			ComObject.RegisterUnreferencedHandler(unreferenced);
		}


		public virtual void UnregisterUnreferencedHandler() {
			ComObject.UnregisterUnreferencedHandler();
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] call(JICallBuilder obj, int timeout) throws org.jinterop.dcom.common.JIException
		public virtual object[] Call(JICallBuilder obj, int timeout) {
			return ComObject.Call(obj, timeout);
		}


		public virtual int InstanceLevelSocketTimeout {
			get {
				return ComObject.InstanceLevelSocketTimeout;
			}
			set {
				ComObject.InstanceLevelSocketTimeout = value;
			}
		}




		public virtual void Internal_setDeffered(bool deffered) {
			ComObject.Internal_setDeffered(deffered);
		}


		public virtual bool LocalReference {
			get {
				return ComObject.LocalReference;
			}
		}

		public override string ToString() {
			return ComObject.ToString();
		}


		public virtual JIComCustomMarshallerUnMarshaller CustomObject {
			get {
				return ComObject.CustomObject;
			}
		}


		public virtual int LengthOfInterfacePointer {
			get {
				return ComObject.LengthOfInterfacePointer;
			}
		}
	}

}