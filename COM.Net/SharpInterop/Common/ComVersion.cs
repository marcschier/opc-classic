//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Common {
    using System;

    /// <summary>
    /// Framework Internal.
    /// This class represents the <code>COM</code> version of the currently
    /// supported COM protocol. Default version is 5.4.
    /// </summary>
    [Serializable]
    public sealed class ComVersion {

        /// <summary>
        /// Create version
        /// </summary>
        public ComVersion() {
        }

        /// <summary>
        /// Create version
        /// </summary>
        /// <param name="majorVersion"></param>
        /// <param name="minorVersion"></param>
        public ComVersion(int majorVersion, int minorVersion) {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
        }

        /// <summary>
        /// Major
        /// </summary>
        public int MajorVersion { set; get; } = 5;

        /// <summary>
        /// Minor
        /// </summary>
        public int MinorVersion { set; get; } = 4;
    }
}