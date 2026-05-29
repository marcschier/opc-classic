//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Wire constants for [MS-SMB2]. Section references in comments target the
// vendored spec under ext/private/docs/MS-SMB2.md.
//

namespace Opc.Classic.Dcom.Smb;

/// <summary>SMB2 protocol constants. See [MS-SMB2] §2.</summary>
internal static class Smb2Constants
{
    /// <summary>SMB2 protocol identifier: <c>0xFE, 'S', 'M', 'B'</c>. See [MS-SMB2] §2.2.1.1.</summary>
    public static readonly byte[] ProtocolId = { 0xFE, (byte)'S', (byte)'M', (byte)'B' };

    /// <summary>SMB2 TRANSFORM_HEADER protocol identifier: <c>0xFD, 'S', 'M', 'B'</c>. See [MS-SMB2] §2.2.41.</summary>
    public static ReadOnlySpan<byte> TransformProtocolId => [0xFD, (byte)'S', (byte)'M', (byte)'B'];

    /// <summary>SMB2 packet header size (synchronous form). See [MS-SMB2] §2.2.1.</summary>
    public const int PacketHeaderSize = 64;

    /// <summary>SMB2 TRANSFORM_HEADER size. See [MS-SMB2] §2.2.41.</summary>
    public const int TransformHeaderSize = 52;

    /// <summary>Maximum NetBIOS-over-TCP frame size (3-byte length field). See [MS-CIFS] §2.2.1.</summary>
    public const int MaxNetBiosFrameSize = 0x1FFFF;

    /// <summary>Server-to-redirector flag. See [MS-SMB2] §2.2.1.2.</summary>
    public const uint FlagsServerToRedir = 0x00000001;

    /// <summary>SMB2 SIGNED flag. See [MS-SMB2] §2.2.1.2.</summary>
    public const uint FlagsSigned = 0x00000008;

    /// <summary>SMB2_GLOBAL_CAP_ENCRYPTION capability. See [MS-SMB2] §2.2.3 and §2.2.4.</summary>
    public const uint GlobalCapEncryption = 0x00000040;

    /// <summary>SMB2_SESSION_FLAG_ENCRYPT_DATA session flag. See [MS-SMB2] §2.2.6.</summary>
    public const ushort SessionFlagEncryptData = 0x0004;

    /// <summary>SMB2_SHAREFLAG_ENCRYPT_DATA share flag. See [MS-SMB2] §2.2.10.</summary>
    public const uint ShareFlagEncryptData = 0x00008000;

    /// <summary>SMB2 TRANSFORM_HEADER Encrypted flag / AES-128-CCM algorithm value. See [MS-SMB2] §2.2.41.</summary>
    public const ushort TransformFlagsEncrypted = 0x0001;

    /// <summary>SMB2_PREAUTH_INTEGRITY_CAPABILITIES negotiate context. See [MS-SMB2] §2.2.3.1.1.</summary>
    public const ushort NegotiateContextPreauthIntegrityCapabilities = 0x0001;

    /// <summary>SMB2_ENCRYPTION_CAPABILITIES negotiate context. See [MS-SMB2] §2.2.3.1.2.</summary>
    public const ushort NegotiateContextEncryptionCapabilities = 0x0002;

    /// <summary>SMB2_PREAUTH_INTEGRITY_SHA512 hash identifier. See [MS-SMB2] §2.2.3.1.1.</summary>
    public const ushort PreauthHashSha512 = 0x0001;

    /// <summary>SMB2_ENCRYPTION_AES128_CCM cipher identifier. See [MS-SMB2] §2.2.3.1.2.</summary>
    public const ushort EncryptionCipherAes128Ccm = 0x0001;

    /// <summary>SMB2_ENCRYPTION_AES128_GCM cipher identifier. See [MS-SMB2] §2.2.3.1.2.</summary>
    public const ushort EncryptionCipherAes128Gcm = 0x0002;

    /// <summary>SMB2_NEGOTIATE_SIGNING_ENABLED security-mode bit. See [MS-SMB2] §2.2.3 and §2.2.4.</summary>
    public const ushort SecurityModeSigningEnabled = 0x0001;

    /// <summary>SMB2_NEGOTIATE_SIGNING_REQUIRED security-mode bit. See [MS-SMB2] §2.2.3 and §2.2.4.</summary>
    public const ushort SecurityModeSigningRequired = 0x0002;
}

/// <summary>SMB2 command identifiers. See [MS-SMB2] §2.2.1.</summary>
public enum Smb2Command : ushort
{
    /// <summary>Negotiate dialect.</summary>
    Negotiate = 0x0000,
    /// <summary>Session setup (NTLMSSP / Kerberos).</summary>
    SessionSetup = 0x0001,
    /// <summary>Logoff.</summary>
    Logoff = 0x0002,
    /// <summary>Tree connect (open a share).</summary>
    TreeConnect = 0x0003,
    /// <summary>Tree disconnect.</summary>
    TreeDisconnect = 0x0004,
    /// <summary>Create a file or named pipe.</summary>
    Create = 0x0005,
    /// <summary>Close a file or named pipe.</summary>
    Close = 0x0006,
    /// <summary>Read from a file or named pipe.</summary>
    Read = 0x0008,
    /// <summary>Write to a file or named pipe.</summary>
    Write = 0x0009,
    /// <summary>IOCTL (file-system or named-pipe control).</summary>
    Ioctl = 0x000B,
}

/// <summary>SMB2 dialect revisions. See [MS-SMB2] §2.2.3.</summary>
public enum Smb2Dialect : ushort
{
    /// <summary>Unspecified default (before negotiation).</summary>
    None = 0,

    /// <summary>SMB 2.0.2.</summary>
    Smb202 = 0x0202,

    /// <summary>SMB 2.1.</summary>
    Smb210 = 0x0210,

    /// <summary>SMB 3.0.</summary>
    Smb300 = 0x0300,

    /// <summary>SMB 3.0.2.</summary>
    Smb302 = 0x0302,

    /// <summary>SMB 3.1.1.</summary>
    Smb311 = 0x0311,
}

/// <summary>FSCTL identifiers for SMB2 IOCTL. See [MS-FSCC] §2.3 and [MS-SMB2] §2.2.31.</summary>
internal static class FsctlCode
{
    /// <summary>FSCTL_PIPE_TRANSCEIVE — synchronous named-pipe write+read used by DCE/RPC over SMB. See [MS-RPCE] §2.1.1.2.</summary>
    public const uint PipeTransceive = 0x0011C017;
}

/// <summary>SMB2 CREATE disposition values. See [MS-SMB2] §2.2.13.</summary>
internal enum CreateDisposition : uint
{
    Supersede = 0,
    Open = 1,
    Create = 2,
    OpenIf = 3,
    Overwrite = 4,
    OverwriteIf = 5,
}

/// <summary>SMB2 CREATE option flags. See [MS-SMB2] §2.2.13.</summary>
internal static class CreateOptions
{
    public const uint NonDirectoryFile = 0x00000040;
}

/// <summary>SMB2 file-access-mask values relevant to named pipes. See [MS-SMB2] §2.2.13.1.</summary>
internal static class FileAccessMask
{
    public const uint Read_Control = 0x00020000;
    public const uint Synchronize = 0x00100000;
    public const uint Generic_Read = 0x80000000;
    public const uint Generic_Write = 0x40000000;

    /// <summary>Composite access mask used to open a named pipe for read+write transact.</summary>
    public const uint PipeReadWrite =
        Generic_Read | Generic_Write | Read_Control | Synchronize;
}

/// <summary>SMB2 share-access flags. See [MS-SMB2] §2.2.13.</summary>
internal static class ShareAccess
{
    public const uint Read = 0x00000001;
    public const uint Write = 0x00000002;
    public const uint ReadWrite = Read | Write;
}

/// <summary>SMB2 NTSTATUS values relevant to client error handling. See [MS-ERREF] §2.3.</summary>
internal static class NtStatus
{
    public const uint Success = 0x00000000;
    public const uint MoreProcessingRequired = 0xC0000016;
    public const uint AccessDenied = 0xC0000022;
    public const uint LogonFailure = 0xC000006D;
}
