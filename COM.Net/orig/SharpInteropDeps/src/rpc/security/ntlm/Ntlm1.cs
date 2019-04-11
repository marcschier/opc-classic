using System;

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

namespace rpc.security.ntlm {

    //import gnu.crypto.prng.IRandom;
    //import gnu.crypto.util.Util;


    using NtlmFlags = jcifs.ntlmssp.NtlmFlags;
    using NdrBuffer = ndr.NdrBuffer;
    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

    using StreamCipher = org.bouncycastle.crypto.StreamCipher;


    public class Ntlm1 : NtlmFlags, Security {

        private const int NTLM1_VERIFIER_LENGTH = 16;

    //    private IRandom clientCipher = null;
    //    private IRandom serverCipher = null;
        private StreamCipher ClientCipher = null;
        private StreamCipher ServerCipher = null;
        private sbyte[] ClientSigningKey = null;
        private sbyte[] ServerSigningKey = null;
        private NTLMKeyFactory KeyFactory = null;
        private bool IsServer = false;
        private int ProtectionLevel_Renamed;

        private int RequestCounter = 0;
        private int ResponseCounter = 0;

        private static readonly Logger Logger = Logger.getLogger("org.jinterop");

        public Ntlm1(int flags, sbyte[] sessionKey, bool isServer) {

            ProtectionLevel_Renamed = ((flags & NTLMSSP_NEGOTIATE_SEAL) != 0) ? rpc.Security_Fields.PROTECTION_LEVEL_PRIVACY : rpc.Security_Fields.PROTECTION_LEVEL_INTEGRITY;

            this.IsServer = isServer;
            KeyFactory = new NTLMKeyFactory();
            ClientSigningKey = KeyFactory.GenerateClientSigningKeyUsingNegotiatedSecondarySessionKey(sessionKey);
            sbyte[] clientSealingKey = KeyFactory.GenerateClientSealingKeyUsingNegotiatedSecondarySessionKey(sessionKey);

            ServerSigningKey = KeyFactory.GenerateServerSigningKeyUsingNegotiatedSecondarySessionKey(sessionKey);
            sbyte[] serverSealingKey = KeyFactory.GenerateServerSealingKeyUsingNegotiatedSecondarySessionKey(sessionKey);


            //Used by the server to decrypt client messages
             ClientCipher = KeyFactory.GetARCFOUR(clientSealingKey);

            //Used by the client to decrypt server messages
             ServerCipher = KeyFactory.GetARCFOUR(serverSealingKey);

    //         if (logger.isLoggable(Level.FINEST))
    //         {
    //             logger.finest("Client Signing Key derieved from the session key: [" + Util.dumpString(clientSigningKey) + "]");
    //             logger.finest("Client Sealing Key derieved from the session key: [" + Util.dumpString(clientSealingKey) + "]");
    //             logger.finest("Server Signing Key derieved from the session key: [" + Util.dumpString(serverSigningKey) + "]");
    //             logger.finest("Server Sealing Key derieved from the session key: [" + Util.dumpString(serverSealingKey) + "]");
    //         }
        }

        public virtual int VerifierLength {
            get {
                return NTLM1_VERIFIER_LENGTH;
            }
        }

        public virtual int AuthenticationService {
            get {
                return NtlmAuthentication.AUTHENTICATION_SERVICE_NTLM;
            }
        }

        public virtual int ProtectionLevel {
            get {
                return ProtectionLevel_Renamed;
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void processIncoming(ndr.NetworkDataRepresentation ndr, int index, int length, int verifierIndex, boolean isFragmented) throws java.io.IOException
        public virtual void ProcessIncoming(NetworkDataRepresentation ndr, int index, int length, int verifierIndex, bool isFragmented) {
            try {
                NdrBuffer buffer = ndr.Buffer;

                sbyte[] signingKey = null;
    //            IRandom cipher = null;
                StreamCipher cipher = null;

                //reverse of what it is
                if (!IsServer) {
                    signingKey = ServerSigningKey;
                    cipher = ServerCipher;
                }
                else {
                    signingKey = ClientSigningKey;
                    cipher = ClientCipher;
                }

                sbyte[] data = new sbyte[length];
                Array.Copy(ndr.Buffer.Buffer,index,data, 0, data.Length);

                if (ProtectionLevel == rpc.Security_Fields.PROTECTION_LEVEL_PRIVACY) {
                    data = KeyFactory.ApplyARCFOUR(cipher, data);
                    Array.Copy(data, 0, ndr.Buffer.Buf, index, data.Length);
                }


                if (Logger.isLoggable(Level.FINEST)) {
                    Logger.finest("\n AFTER Decryption");
                    ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
                    jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), data, 0, data.Length);
                    Logger.finest("\n" + byteArrayOutputStream.ToString());
                    Logger.finest("\nLength is: " + data.Length);
                }



                sbyte[] verifier = KeyFactory.SigningPt1(ResponseCounter, signingKey, buffer.Buffer,verifierIndex);
                KeyFactory.SigningPt2(verifier, cipher);

                buffer.Index = verifierIndex;
                //now read the next 16 bytes and pass compare them
                sbyte[] signing = new sbyte[16];
                ndr.ReadOctetArray(signing, 0, signing.Length);

                //this should result in an access denied fault
                if (!KeyFactory.CompareSignature(verifier, signing)) {
                    throw new IntegrityException("Message out of sequence. Perhaps the user being used to run this application is different from the one under which the COM server is running !.");
                }

                //only clients increment, servers just respond to the clients seq id.
    //            if (!isServer || isFragmented)
    //            {
    //                responseCounter++;
    //            }

                ResponseCounter++;


            }
            catch (IOException ex) {
                Logger.log(Level.SEVERE, "", ex);
                throw ex;
            }
            catch (Exception ex) {
                Logger.log(Level.SEVERE, "", ex);
                throw new IntegrityException("General error: " + ex.Message);
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void processOutgoing(ndr.NetworkDataRepresentation ndr, int index, int length, int verifierIndex, boolean isFragmented) throws java.io.IOException
        public virtual void ProcessOutgoing(NetworkDataRepresentation ndr, int index, int length, int verifierIndex, bool isFragmented) {
            try {
                NdrBuffer buffer = ndr.Buffer;

                sbyte[] signingKey = null;
    //            IRandom cipher = null;
                StreamCipher cipher = null;

                if (IsServer) {
                    signingKey = ServerSigningKey;
                    cipher = ServerCipher;
                }
                else {
                    signingKey = ClientSigningKey;
                    cipher = ClientCipher;
                }

                sbyte[] verifier = KeyFactory.SigningPt1(RequestCounter, signingKey, buffer.Buffer,verifierIndex);
                sbyte[] data = new sbyte[length];
                Array.Copy(ndr.Buffer.Buffer,index,data, 0, data.Length);
                if (Logger.isLoggable(Level.FINEST)) {
                    Logger.finest("\n BEFORE Encryption");
                    ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
                    jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), data, 0, data.Length);
                    Logger.finest("\n" + byteArrayOutputStream.ToString());
                    Logger.finest("\n Length is: " + data.Length);
                }


                if (ProtectionLevel == rpc.Security_Fields.PROTECTION_LEVEL_PRIVACY) {
                    sbyte[] data2 = KeyFactory.ApplyARCFOUR(cipher, data);
                    Array.Copy(data2, 0, ndr.Buffer.Buf, index, data2.Length);
                }
                KeyFactory.SigningPt2(verifier, cipher);
                buffer.Index = verifierIndex;
                buffer.WriteOctetArray(verifier, 0, verifier.Length);


    //            if (isServer && !isFragmented)
    //            {
    //                responseCounter++;
    //            }

                RequestCounter++;


            }
            catch (Exception ex) {
                throw new IntegrityException("General error: " + ex.Message);
            }
        }

    }

}