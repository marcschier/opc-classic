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

namespace org.jinterop.winreg {

    /// <summary>
    /// Policy handle for each key.
    /// 
    /// @since 1.0
    /// 
    /// </summary>
    public class JIPolicyHandle {
        /// <summary>
        /// Handle to the Key
        /// </summary>
        public readonly sbyte[] Handle = new sbyte[20];
        /// <summary>
        /// True, if the key was newly created.
        /// </summary>
        public readonly bool NewlyCreated;
        /// <summary>
        /// @exclude </summary>
        /// <param name="newlyCreated"> </param>
        public JIPolicyHandle(bool newlyCreated) {
            this.NewlyCreated = newlyCreated;
        }
    }

}