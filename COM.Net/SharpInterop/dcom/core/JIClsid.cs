//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {
    using rpc.core;

    /// <summary>
    /// Wrapper for class identifier to a COM Object.
    /// </summary>
    /// <remarks>
    /// Definition from MSDN: <i> A universally unique identifier (UUID) that
    /// identifies a type of Component Object Model (COM) object. Each type of
    /// COM object item has its CLSID in the registry so that it can be loaded
    /// and used by other applications. For example, a spreadsheet may create
    /// worksheet items, chart items, and macrosheet items. Each of these item
    /// types has its own CLSID that uniquely identifies it to the system. </i>
    /// For example Microsoft Office Excel Application has clsid of
    /// "00024500-0000-0000-C000-000000000046".
    /// </remarks>
    public class JIClsid {

        /// <summary>
        /// String representation of the wrapped class identifier.
        /// </summary>
        /// <returns> string of the form "00000000-0000-0000-0000-000000000000" </returns>
        public string CLSID => _nestedUUID.ToString();

        /// <summary>
        /// Indicates to the framework, if Windows Registry settings for DLL\OCX
        /// component identified by this object should be modified to add a <code>Surrogate</code>
        /// automatically. A <code>Surrogate</code> is a process which provides resources
        /// such as memory and cpu for a DLL\OCX to execute.
        /// </summary>
        public bool UseAutoRegistration { set; get; }

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="uuid"></param>
        private JIClsid(string uuid) => _nestedUUID.Parse(uuid);

        /// <summary>
        /// Factory method returning an instance of this class.
        /// </summary>
        /// <param name="uuid"> - clsid of the form
        /// "00000000-0000-0000-0000-000000000000" </param>
        /// <returns> - instance of JIClsid  </returns>
        public static JIClsid ValueOf(string uuid) {
            if (uuid == null) {
                return null;
            }
            return new JIClsid(uuid);
        }

        private readonly UUID _nestedUUID = new UUID();
    }
}