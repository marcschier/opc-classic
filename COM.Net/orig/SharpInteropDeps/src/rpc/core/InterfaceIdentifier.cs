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

    using NdrObject = ndr.NdrObject;

    public class InterfaceIdentifier : NdrObject {

        internal UUID Uuid_Renamed;
        internal int MajorVersion_Renamed, MinorVersion_Renamed;

        public InterfaceIdentifier(string syntax) {
            Parse(syntax);
        }

        public InterfaceIdentifier(UUID uuid, int majorVersion, int minorVersion) {
            Uuid = uuid;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
        }

        public virtual UUID Uuid {
            get {
                return Uuid_Renamed;
            }
            set {
                this.Uuid_Renamed = value;
            }
        }


        public virtual int MajorVersion {
            get {
                return MajorVersion_Renamed;
            }
            set {
                this.MajorVersion_Renamed = value;
            }
        }


        public virtual int MinorVersion {
            get {
                return MinorVersion_Renamed;
            }
            set {
                this.MinorVersion_Renamed = value;
            }
        }


        public override string ToString() {
            return Uuid.ToString() + ":" + MajorVersion + "." + MinorVersion;
        }

        public virtual void Parse(string syntax) {
            StringTokenizer tokenizer = new StringTokenizer(syntax, ":.");
            Uuid.Parse(tokenizer.nextToken());
            MajorVersion = int.Parse(tokenizer.nextToken());
            MinorVersion = int.Parse(tokenizer.nextToken());
        }

    }

}