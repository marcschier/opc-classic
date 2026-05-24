// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Crypto;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using System;
using System.IO;

namespace Opc.Classic.Dcom.Rpc.Auth.ntlm; 
/// <summary>
/// Ntlm1 implementation
/// </summary>
[System.Obsolete(
    "NTLMv1 (Ntlm1) is cryptographically broken and disabled by default in Phase 3C. " +
    "Use NtlmV2 (default). To re-enable NTLMv1 for compatibility with very old legacy " +
    "servers, set OpcConnectData.AllowNtlmV1 = true or properties.SetProperty(\"rpc.ntlm.allowV1\", \"true\").")]
public class Ntlm1 : ISecurity {

    private const int kNTLM1_VERIFIER_LENGTH = 16;

    /// <summary>
    /// Verifier length
    /// </summary>
    public int VerifierLength => kNTLM1_VERIFIER_LENGTH;

    /// <summary>
    /// Auth service
    /// </summary>
    public int AuthenticationService => NtlmAuthentication.AUTHENTICATIONSERVICENTLM;

    /// <summary>
    /// Protection level
    /// </summary>
    public ProtectionLevel Protection { get; }

    /// <summary>
    /// Create
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sessionKey"></param>
    /// <param name="isServer"></param>
    public Ntlm1(NtlmFlags flags, byte[] sessionKey, bool isServer) {

        Protection = ((flags & NtlmFlags.NtlmsspNegotiateSeal) != NtlmFlags.None) ?
            ProtectionLevel.PROTECTION_LEVEL_PRIVACY : ProtectionLevel.PROTECTION_LEVEL_INTEGRITY;

        _isServer = isServer;
        _keyFactory = new NTLMKeyFactory();
        _clientSigningKey = _keyFactory.
            GenerateClientSigningKeyUsingNegotiatedSecondarySessionKey(sessionKey);
        var clientSealingKey = _keyFactory.
            GenerateClientSealingKeyUsingNegotiatedSecondarySessionKey(sessionKey);

        _serverSigningKey = _keyFactory.
            GenerateServerSigningKeyUsingNegotiatedSecondarySessionKey(sessionKey);
        var serverSealingKey = _keyFactory.
            GenerateServerSealingKeyUsingNegotiatedSecondarySessionKey(sessionKey);


        // Used by the server to decrypt client messages
        _clientCipher = _keyFactory.GetARCFOUR(clientSealingKey);
        // Used by the client to decrypt server messages
        _serverCipher = _keyFactory.GetARCFOUR(serverSealingKey);
    }


    /// <summary>
    /// Process incoming
    /// </summary>
    /// <param name="ndr"></param>
    /// <param name="index"></param>
    /// <param name="length"></param>
    /// <param name="verifierIndex"></param>
    /// <param name="isFragmented"></param>
    /// <exception cref="IOException"></exception>
    public void ProcessIncoming(NdrCodec ndr, int index, int length,
        int verifierIndex, bool isFragmented) {
        try {
            var buffer = ndr.Buffer;
            byte[] signingKey = null;
            IStreamCipher cipher = null;

            // reverse of what it is
            if (!_isServer) {
                signingKey = _serverSigningKey;
                cipher = _serverCipher;
            }
            else {
                signingKey = _clientSigningKey;
                cipher = _clientCipher;
            }

            var data = new byte[length];
            Array.Copy(ndr.Buffer.Buf, index, data, 0, data.Length);

            if (Protection == ProtectionLevel.PROTECTION_LEVEL_PRIVACY) {
                data = _keyFactory.ApplyARCFOUR(cipher, data);
                Array.Copy(data, 0, ndr.Buffer.Buf, index, data.Length);
            }
            Log.Logger.Verbose("\n AFTER Decryption\n" +
                Utils.HexString(data, 0, data.Length) + "\nLength is: " + data.Length);

            var verifier = _keyFactory.SigningPt1(_responseCounter, signingKey,
                buffer.Buf, verifierIndex);
            _keyFactory.SigningPt2(verifier, cipher);

            buffer.Index = verifierIndex;
            // now read the next 16 bytes and pass compare them
            var signing = new byte[16];
            ndr.ReadOctetArray(signing, 0, signing.Length);

            // this should result in an access denied fault
            if (!_keyFactory.CompareSignature(verifier, signing)) {
                throw new IntegrityException("Message out of sequence. " +
                    "Perhaps the user being used to run this application is different " +
                    "from the one under which the COM server is running !.");
            }
            _responseCounter++;
        }
        catch (IOException ex) {
            Log.Logger.Error(ex, "");
            throw;
        }
        catch (Exception ex) {
            Log.Logger.Error(ex, "");
            throw new IntegrityException("General error: " + ex.Message);
        }
    }

    /// <summary>
    /// Process outgoing
    /// </summary>
    /// <param name="ndr"></param>
    /// <param name="index"></param>
    /// <param name="length"></param>
    /// <param name="verifierIndex"></param>
    /// <param name="isFragmented"></param>
    public void ProcessOutgoing(NdrCodec ndr, int index, int length,
        int verifierIndex, bool isFragmented) {
        try {
            var buffer = ndr.Buffer;
            byte[] signingKey = null;
            IStreamCipher cipher = null;

            if (_isServer) {
                signingKey = _serverSigningKey;
                cipher = _serverCipher;
            }
            else {
                signingKey = _clientSigningKey;
                cipher = _clientCipher;
            }

            var verifier = _keyFactory.SigningPt1(_requestCounter,
                signingKey, buffer.Buf, verifierIndex);
            var data = new byte[length];
            Array.Copy(ndr.Buffer.Buf, index, data, 0, data.Length);
            Log.Logger.Verbose("\n BEFORE Encryption\n" +
                Utils.HexString(data, 0, data.Length) + "\n Length is: " + data.Length);

            if (Protection == ProtectionLevel.PROTECTION_LEVEL_PRIVACY) {
                var data2 = _keyFactory.ApplyARCFOUR(cipher, data);
                Array.Copy(data2, 0, ndr.Buffer.Buf, index, data2.Length);
            }
            _keyFactory.SigningPt2(verifier, cipher);
            buffer.Index = verifierIndex;
            buffer.WriteOctetArray(verifier, 0, verifier.Length);
            _requestCounter++;
        }
        catch (Exception ex) {
            throw new IntegrityException("General error: " + ex.Message);
        }
    }

    private readonly IStreamCipher _clientCipher;
    private readonly IStreamCipher _serverCipher;
    private readonly byte[] _clientSigningKey;
    private readonly byte[] _serverSigningKey;
    private readonly NTLMKeyFactory _keyFactory;
    private readonly bool _isServer;
    private int _requestCounter;
    private int _responseCounter;
}
