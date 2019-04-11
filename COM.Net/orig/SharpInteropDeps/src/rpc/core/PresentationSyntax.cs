/// <summary>
/// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
/// 
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
/// Vikram Roopchand  - Moving to EPL from LGPL v1.
/// 
/// </summary>

namespace rpc.core {

    using NdrBuffer = ndr.NdrBuffer;
    using NdrException = ndr.NdrException;
    using NdrObject = ndr.NdrObject;
    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

    public class PresentationSyntax : NdrObject {

        private const int UUID_INDEX = 0;

        private const int VERSION_INDEX = 1;

        internal UUID Uuid_Renamed;
        internal int Version_Renamed;

        public PresentationSyntax() {
        }

        public PresentationSyntax(string syntax) : this() {
            Parse(syntax);
        }

        public PresentationSyntax(UUID uuid, int majorVersion, int minorVersion) : this() {
            Uuid = uuid;
            SetVersion(majorVersion, minorVersion);
        }

        public virtual UUID Uuid {
            get {
                return Uuid_Renamed;
            }
            set {
                this.Uuid_Renamed = value;
            }
        }


        public virtual int Version {
            get {
                return Version_Renamed;
            }
            set {
                this.Version_Renamed = value;
            }
        }


        public virtual int MajorVersion {
            get {
                return Version_Renamed & 0xffff;
            }
        }

        public virtual int MinorVersion {
            get {
                return (Version_Renamed >> 16) & 0xffff;
            }
        }

        public virtual void SetVersion(int majorVersion, int minorVersion) {
            Version = (majorVersion & 0xffff) | (minorVersion << 16);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void encode(ndr.NetworkDataRepresentation ndr, ndr.NdrBuffer dst) throws ndr.NdrException
        public override void Encode(NetworkDataRepresentation ndr, NdrBuffer dst) {
            Uuid_Renamed.Encode(ndr, dst);
            dst.Enc_ndr_long(Version_Renamed);
        }
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void decode(ndr.NetworkDataRepresentation ndr, ndr.NdrBuffer src) throws ndr.NdrException
        public override void Decode(NetworkDataRepresentation ndr, NdrBuffer src) {
            Uuid_Renamed = new UUID();
            Uuid_Renamed.Decode(ndr, src);
            Version_Renamed = src.Dec_ndr_long();
        }

        public override string ToString() {
            return Uuid.ToString() + ":" + MajorVersion + "." + MinorVersion;
        }

        public virtual void Parse(string syntax) {
            StringTokenizer tokenizer = new StringTokenizer(syntax, ":.");
            Uuid_Renamed = new UUID();
            Uuid_Renamed.Parse(tokenizer.nextToken());
            SetVersion(int.Parse(tokenizer.nextToken()), int.Parse(tokenizer.nextToken()));
        }

    }

}