//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc.Core {
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Util.Sharpen;

    /// <summary>
    /// Presentation syntax
    /// </summary>
    public class PresentationSyntax : NdrOp {

        /// <summary>
        /// Uuuid
        /// </summary>
        public UUID Uuid { get; set; }

        /// <summary>
        /// Version
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Major
        /// </summary>
        public int MajorVersion => Version & 0xffff;

        /// <summary>
        /// Minor
        /// </summary>
        public int MinorVersion => (Version >> 16) & 0xffff;

        /// <summary>
        /// Create syntax
        /// </summary>
        public PresentationSyntax() {
        }

        /// <summary>
        /// Create presentation syntax
        /// </summary>
        /// <param name="syntax"></param>
        public PresentationSyntax(string syntax) : this() {
            var tokenizer = new StringTokenizer(syntax, ":.");
            Uuid = new UUID();
            Uuid.Parse(tokenizer.NextToken());
            Version = (int.Parse(tokenizer.NextToken()) & 0xffff) |
                (int.Parse(tokenizer.NextToken()) << 16);
        }

        /// <summary>
        /// Create syntax
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="majorVersion"></param>
        /// <param name="minorVersion"></param>
        public PresentationSyntax(UUID uuid, int majorVersion, int minorVersion) : this() {
            Uuid = uuid;
            Version = (majorVersion & 0xffff) | (minorVersion << 16);
        }

        /// <inheritdoc/>
        public override void Encode(NdrCodec ndr, NdrBuffer dst) {
            Uuid.Encode(ndr, dst);
            dst.Enc_ndr_long(Version);
        }

        /// <inheritdoc/>
        public override void Decode(NdrCodec ndr, NdrBuffer src) {
            Uuid = new UUID();
            Uuid.Decode(ndr, src);
            Version = src.Dec_ndr_long();
        }

        /// <inheritdoc/>
        public override string ToString() => Uuid + ":" + MajorVersion + "." + MinorVersion;
    }

}