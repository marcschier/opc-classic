//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using Opc.Classic.Dcom.Internal.LegacyNdr;
using System;
using System.Text;

namespace SharpInterop.Core; 
/// <summary>
/// Represents a security binding
/// </summary>
[Serializable]
internal sealed class SecurityBinding {

    /// <summary>
    /// Length
    /// </summary>
    public int Length { get; private set; } = -1;

    /// <summary>
    /// Create binding
    /// </summary>
    /// <param name="authnSvc"></param>
    /// <param name="authzSvc"></param>
    /// <param name="princName"></param>
    internal SecurityBinding(int authnSvc, int authzSvc, string princName) {
        _authnSvc = authnSvc;
        _authzSvc = authzSvc;
        _princName = princName;
        if (princName.Equals("")) {
            Length = 2 + 2 + 2;
        }
        else {
            Length = 2 + 2 + (princName.Length * 2) + 2;
        }
    }

    /// <summary>
    /// Private constructor
    /// </summary>
    private SecurityBinding() {
    }

    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="ndr"></param>
    /// <returns></returns>
    internal static SecurityBinding Decode(NdrCodec ndr) {
        var securityBinding = new SecurityBinding {
            _authnSvc = ndr.ReadUnsignedShort()
        };

        if (securityBinding._authnSvc == 0) {
            // security binding over.
            return null;
        }

        securityBinding._authzSvc = ndr.ReadUnsignedShort();

        // now to read the String till a null termination character.
        // a '0' will be represented as 30
        var buffer = new StringBuilder();
        int retVal;
        while ((retVal = ndr.ReadUnsignedShort()) != 0) {
            // even though this is a unicode string, but will not have anything else
            // other than ascii charset, which is supported by all encodings.
            buffer.Append(StringHelperClass.NewString(new byte[] { (byte)retVal }));
        }
        securityBinding._princName = buffer.ToString();
        // 2 bytes for authnsvc, 2 for authzsvc, each character is 2 bytes (short) and last 2 bytes for null termination
        securityBinding.Length = 2 + 2 + (securityBinding._princName.Length * 2) + 2;
        return securityBinding;
    }

    /// <summary>
    /// Encode
    /// </summary>
    /// <param name="ndr"></param>
    public void Encode(NdrCodec ndr) {
        ndr.WriteUnsignedShort(_authnSvc);
        ndr.WriteUnsignedShort(_authzSvc);

        // now to write the network address.
        var i = 0;
        while (i < _princName.Length) {
            ndr.WriteUnsignedShort(_princName[i]);
            i++;
        }
        ndr.WriteUnsignedShort(0); // null termination
    }


    public const int COM_C_AUTHZ_NONE = 0xffff;
    private int _authnSvc; // Cannot be zero.
    private int _authzSvc; // Must not be zero.
    private string _princName; // Zero terminated.
}
