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

namespace rpc.core {
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Util.Sharpen;

    /// <summary>
    /// Interface id
    /// </summary>
    public class InterfaceIdentifier : NdrOp {

        /// <summary>
        /// Id
        /// </summary>
        public UUID Uuid { get; set; }

        /// <summary>
        /// Major
        /// </summary>
        public int MajorVersion { get; set; }

        /// <summary>
        /// Minor
        /// </summary>
        public int MinorVersion { get; set; }

        /// <summary>
        /// Create id
        /// </summary>
        /// <param name="syntax"></param>
        public InterfaceIdentifier(string syntax) {
            var tokenizer = new StringTokenizer(syntax, ":.");
            Uuid.Parse(tokenizer.NextToken());
            MajorVersion = int.Parse(tokenizer.NextToken());
            MinorVersion = int.Parse(tokenizer.NextToken());
        }

        /// <summary>
        /// Create id
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="majorVersion"></param>
        /// <param name="minorVersion"></param>
        public InterfaceIdentifier(UUID uuid, int majorVersion,
            int minorVersion) {
            Uuid = uuid;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
        }

        /// <override/>
        public override string ToString() => Uuid + ":" + MajorVersion + "." + MinorVersion;
    }
}