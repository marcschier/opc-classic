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

	using UUID = rpc.core.UUID;

	/// <summary>
	///<para>Wrapper for class identifier to a COM Object.
	/// </para>
	/// <para>
	/// Definition from MSDN: <i> A universally unique identifier (UUID) that 
	/// identifies a type of Component Object Model (COM) object. Each type of 
	/// COM object item has its CLSID in the registry so that it can be loaded 
	/// and used by other applications. For example, a spreadsheet may create 
	/// worksheet items, chart items, and macrosheet items. Each of these item 
	/// types has its own CLSID that uniquely identifies it to the system. </i>
	/// 
	/// </para>
	/// <para>
	/// For example Microsoft Office Excel Application has clsid of "00024500-0000-0000-C000-000000000046".
	///  </para>
	/// @since 1.0
	/// </summary>
	public class JIClsid {

		private UUID NestedUUID = new UUID();
		private bool AutoRegister = false;

		/// <summary>
		/// Indicates to the framework, if Windows Registry settings for DLL\OCX
		/// component identified by this object should be modified to add a <code>Surrogate</code> 
		/// automatically. A <code>Surrogate</code> is a process which provides resources
		/// such as memory and cpu for a DLL\OCX to execute.
		/// </summary>
		/// <param name="autoRegister"> <code>true</code> if auto registration should be done by the framework. </param>
		public virtual bool AutoRegistration {
			set {
				this.AutoRegister = value;
			}
		}

		/// <summary>
		///Returns the status of the auto registration flag for the component identified by this object.
		/// </summary>
		/// <returns> <code>true</code> if the auto registration flag is set. </returns>
		public virtual bool AutoRegistrationSet {
			get {
				return AutoRegister;
			}
		}

		/// <summary>
		/// Factory method returning an instance of this class.
		/// </summary>
		/// <param name="uuid"> - clsid of the form "00000000-0000-0000-0000-000000000000" </param>
		/// <returns> - instance of JIClsid  </returns>
		public static JIClsid ValueOf(string uuid) {
			if (uuid == null) {
				return null;
			}
			return new JIClsid(uuid);
		}

		private JIClsid(string uuid) {
			this.NestedUUID.parse(uuid);
		}

		/// <summary>
		/// String representation of the wrapped class identifier.
		/// </summary>
		/// <returns> string of the form "00000000-0000-0000-0000-000000000000" </returns>
		public virtual string CLSID {
			get {
				return NestedUUID.ToString();
			}
		}



	}

}