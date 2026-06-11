//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Dcom.Smb;

/// <summary>Base exception for SMB2 protocol errors.</summary>
public class Smb2ProtocolException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="Smb2ProtocolException" /> class.</summary>
    public Smb2ProtocolException() { }

    /// <summary>Initializes a new instance of the <see cref="Smb2ProtocolException" /> class.</summary>
    public Smb2ProtocolException(string message) : base(message) { }

    /// <summary>Initializes a new instance of the <see cref="Smb2ProtocolException" /> class.</summary>
    public Smb2ProtocolException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>Thrown when the server returns a non-success NTSTATUS in an SMB2 response.</summary>
public sealed class Smb2StatusException : Smb2ProtocolException
{
    /// <summary>NTSTATUS code returned by the server.</summary>
    public uint Status { get; }

    /// <summary>Initializes a new instance with a default message.</summary>
    public Smb2StatusException()
    {
        Status = 0;
    }

    /// <summary>Initializes a new instance with the given message.</summary>
    public Smb2StatusException(string message) : base(message)
    {
        Status = 0;
    }

    /// <summary>Initializes a new instance with the given message and inner exception.</summary>
    public Smb2StatusException(string message, Exception innerException) : base(message, innerException)
    {
        Status = 0;
    }

    /// <summary>Initializes a new instance with the given NTSTATUS code and message.</summary>
    public Smb2StatusException(uint status, string message)
        : base(message)
    {
        Status = status;
    }
}
