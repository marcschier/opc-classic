//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc {
    using System.IO;
    using Opc.Classic.Dcom.Internal.LegacyNdr;

    /// <summary>
    /// Security interface
    /// </summary>
    public interface ISecurity {

        /// <summary>
        /// Verifier length
        /// </summary>
        int VerifierLength { get; }

        /// <summary>
        /// Authentication service
        /// </summary>
        int AuthenticationService { get; }

        /// <summary>
        /// Protection level
        /// </summary>
        ProtectionLevel Protection { get; }

        /// <summary>
        /// Process incoming
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <param name="verifierIndex"></param>
        /// <param name="isFragmented"></param>
        /// <exception cref="IOException"></exception>
        void ProcessIncoming(NdrCodec ndr, int index, int length,
            int verifierIndex, bool isFragmented);

        /// <summary>
        /// Process outgoing
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <param name="verifierIndex"></param>
        /// <param name="isFragmented"></param>
        /// <exception cref="IOException"></exception>
        void ProcessOutgoing(NdrCodec ndr, int index, int length,
            int verifierIndex, bool isFragmented);
    }

}