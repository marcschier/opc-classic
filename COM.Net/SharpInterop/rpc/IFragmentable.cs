
//
// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
//
// j-Interop (Pure Java implementation of DCOM protocol)
//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace rpc {
    using SharpCifs.Util.Sharpen;
    using System.IO;

    /// <summary>
    /// Fragmantable tag
    /// </summary>
    public interface IFragmentable {

        /// <summary>
        /// Create fragments
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        Iterator<ConnectionOrientedPdu> GetFragments(int size);

        /// <summary>
        /// Reassemble
        /// </summary>
        /// <param name="fragments"></param>
        /// <exception cref="IOException"></exception>
        /// <returns></returns>
        ConnectionOrientedPdu Reassemble(
            Iterator<ConnectionOrientedPdu> fragments);

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns></returns>
        ConnectionOrientedPdu Clone();
    }
}