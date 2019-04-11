using System;
using System.Collections;

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

namespace rpc.pdu {


    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

    public class ResponseCoPdu : ConnectionOrientedPdu, Fragmentable {

        public const int RESPONSE_TYPE = 0x02;

        private sbyte[] Stub_Renamed;

        private int AllocationHint_Renamed = 0;

        private int ContextId_Renamed = 0;

        private int CancelCount_Renamed = 0;

        private static readonly Logger Logger = Logger.getLogger("org.jinterop");

        public override int Type {
            get {
                return RESPONSE_TYPE;
            }
        }

        public virtual sbyte[] Stub {
            get {
                return Stub_Renamed;
            }
            set {
                this.Stub_Renamed = value;
            }
        }


        public virtual int AllocationHint {
            get {
                return AllocationHint_Renamed;
            }
            set {
                this.AllocationHint_Renamed = value;
            }
        }


        public virtual int ContextId {
            get {
                return ContextId_Renamed;
            }
            set {
                this.ContextId_Renamed = value;
            }
        }


        public virtual int CancelCount {
            get {
                return CancelCount_Renamed;
            }
            set {
                this.CancelCount_Renamed = value;
            }
        }


        public override void ReadPdu(NetworkDataRepresentation ndr) {
            ReadHeader(ndr);
            ReadBody(ndr);
            ReadStub(ndr);
        }

        public override void WritePdu(NetworkDataRepresentation ndr) {
            WriteHeader(ndr);
            WriteBody(ndr);
            WriteStub(ndr);
        }

        public override void ReadBody(NetworkDataRepresentation ndr) {
            AllocationHint = ndr.ReadUnsignedLong();
            ContextId = ndr.ReadUnsignedShort();
            CancelCount = ndr.ReadUnsignedSmall();
        }

        public override void WriteBody(NetworkDataRepresentation ndr) {
            ndr.WriteUnsignedLong(AllocationHint);
            ndr.WriteUnsignedShort(ContextId);
            ndr.WriteUnsignedSmall((short) CancelCount);
        }

        public virtual void ReadStub(NetworkDataRepresentation ndr) {
            ndr.Buffer.Align(8);
            sbyte[] stub = null;
            int length = FragmentLength - ndr.Buffer.Index;
            if (length > 0) {
                stub = new sbyte[length];
                ndr.ReadOctetArray(stub, 0, length);
            }
            Stub = stub;
        }

        public virtual void WriteStub(NetworkDataRepresentation ndr) {
            ndr.Buffer.Align(8, (sbyte) 0);
            sbyte[] stub = Stub;
            if (stub != null) {
                ndr.WriteOctetArray(stub, 0, stub.Length);
            }
        }

        public virtual IEnumerator Fragment(int size) {
            sbyte[] stub = Stub;
            if (stub == null) {
                return Arrays.asList(new ResponseCoPdu[] { this }).GetEnumerator();
            }

            //subtracting 8 bytes for authentication header and 16 for the authentication verifier size, someone forgot the
            //poor guys..
            int stubSize = size - 24 - 8 - 16;
            if (stub.Length <= stubSize) {
                return Arrays.asList(new ResponseCoPdu[] { this }).GetEnumerator();
            }
            return new FragmentIterator(this, stubSize);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.Fragmentable assemble(java.util.Iterator fragments) throws java.io.IOException
        public virtual Fragmentable Assemble(IEnumerator fragments) {
            if (Logger.isLoggable(Level.FINEST)) {
                Logger.finest("[DURING RECIEVE IN ASSEMBLE]\n");
            }
            if (!fragments.hasNext()) {
                throw new IOException("No fragments available.");
            }
            try {
                ResponseCoPdu pdu = (ResponseCoPdu) fragments.next();
                sbyte[] stub = pdu.Stub;
                if (stub == null) {
                    stub = new sbyte[0];
                }
                int i = 0;
                while (fragments.hasNext()) {
                    if (Logger.isLoggable(Level.FINEST)) {
                        Logger.finest("[IN ASSEMBLE] Fragment { " + i + " }\n");
                    }
                    ResponseCoPdu fragment = (ResponseCoPdu) fragments.next();
                    sbyte[] fragmentStub = fragment.Stub;
                    if (fragmentStub != null && fragmentStub.Length > 0) {
                        if (Logger.isLoggable(Level.FINEST)) {
                            Logger.finest("[FRAGMENT'S STUB (new one)] Length is = " + fragmentStub.Length);
                        }
                        sbyte[] tmp = new sbyte[stub.Length + fragmentStub.Length];
                        Array.Copy(stub, 0, tmp, 0, stub.Length);
                        Array.Copy(fragmentStub, 0, tmp, stub.Length, fragmentStub.Length);
                        stub = tmp;
                        if (Logger.isLoggable(Level.FINEST)) {
                            Logger.finest("[ADDED THIS STUB (previous stub + new one) into OLD STUB] Current Length of pieces assembled so far = " + stub.Length);
                            ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
                            jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), stub, 0, stub.Length);
                            Logger.finest("\n" + byteArrayOutputStream.ToString());
                        }
                    }
                }
                int length = stub.Length;
                if (length > 0) {
                    pdu.Stub = stub;
                    pdu.AllocationHint = length;
                    if (Logger.isLoggable(Level.FINEST)) {
                        Logger.finest("[FULL AND FINAL STUB AFTER ASSEMBLY]\n");
                        ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
                        jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), stub, 0, stub.Length);
                        Logger.finest("\n" + byteArrayOutputStream.ToString());
                    }
                }
                else {
                    pdu.Stub = null;
                    pdu.AllocationHint = 0;
                }
                pdu.SetFlag(PFC_FIRST_FRAG, true);
                pdu.SetFlag(PFC_LAST_FRAG, true);
                return pdu;
            }
            catch (Exception) {
                throw new IOException("Unable to assemble PDU fragments.");
            }
        }

        public virtual object Clone() {
            try {
                return base.Clone();
            }
            catch (Exception) {
                throw new System.InvalidOperationException();
            }
        }

        private class FragmentIterator : IEnumerator {
            private readonly ResponseCoPdu OuterInstance;


            internal int StubSize;

            internal int Index = 0;

            public FragmentIterator(ResponseCoPdu outerInstance, int stubSize) {
                this.OuterInstance = outerInstance;
                this.StubSize = stubSize;
            }

            public virtual bool HasNext() {
                return Index < outerInstance.Stub_Renamed.Length;
            }

            public virtual object Next() {
                if (Index >= outerInstance.Stub_Renamed.Length) {
                    throw new NoSuchElementException();
                }
                ResponseCoPdu fragment = (ResponseCoPdu) ResponseCoPdu.this.clone();
                int allocation = outerInstance.Stub_Renamed.Length - Index;
                fragment.AllocationHint = allocation;
                if (StubSize < allocation) {
                    allocation = StubSize;
                }
                sbyte[] fragmentStub = new sbyte[allocation];
                Array.Copy(outerInstance.Stub_Renamed, Index, fragmentStub, 0, allocation);
                fragment.Stub = fragmentStub;
                int flags = outerInstance.Flags & ~(PFC_FIRST_FRAG | PFC_LAST_FRAG);
                if (Index == 0) {
                    flags |= PFC_FIRST_FRAG;
                }
                Index += allocation;
                if (Index >= outerInstance.Stub_Renamed.Length) {
                    flags |= PFC_LAST_FRAG;
                }
                fragment.Flags = flags;
                return fragment;
            }

            public virtual void Remove() {
                throw new System.NotSupportedException();
            }

        }

    }

}