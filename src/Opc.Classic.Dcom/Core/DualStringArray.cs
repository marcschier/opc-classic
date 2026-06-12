// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Represents array of network address and security bindings.
/// </summary>
[Serializable]
internal sealed class DualStringArray
{

    /// <summary>
    /// String bindings
    /// </summary>
    public StringBinding[] StringBindings { get; private set; }

    /// <summary>
    /// Security bindings
    /// </summary>
    public SecurityBinding[] SecurityBindings { get; private set; }

    /// <summary>
    /// Length
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Create array
    /// </summary>
    private DualStringArray() { }

    /// <summary>
    /// Will get called from Oxid Resolver
    /// </summary>
    /// <param name="port">Network port used by the RPC endpoint or string binding.</param>
    internal DualStringArray(int port)
    {
        // create bindings here.
        StringBindings = new StringBinding[2]; // only 1
        StringBindings[0] = new StringBinding(port, false);

        Length = StringBindings[0].Length;

        StringBindings[1] = new StringBinding(port, true);

        Length = Length + StringBindings[1].Length + 2; // null termination

        _secOffset = Length;

        SecurityBindings = new SecurityBinding[1]; // support only winnt NTLM
        SecurityBindings[0] = new SecurityBinding(0x0a, 0xffff, "");
        Length += SecurityBindings[0].Length;
        // null termination, 2 bytes for num entries and 2 bytes for sec offset.
        Length = Length + 2 + 2 + 2;
    }

    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <returns>A new <see cref="DualStringArray"/> instance built from <paramref name="ndr"/>.</returns>
    internal static DualStringArray Decode(NdrCodec ndr)
    {
        var dualStringArray = new DualStringArray();

        // first extract number of entries
        var numEntries = ndr.ReadUnsignedShort();

        // return empty
        if (numEntries == 0)
        {
            return dualStringArray;
        }

        // extract security offset
        var securityOffset = ndr.ReadUnsignedShort();

        var listOfStringBindings = new List<StringBinding>();
        var listOfSecurityBindings = new List<SecurityBinding>();

        var stringbinding = true;
        while (true)
        {
            if (stringbinding)
            {
                var s = StringBinding.Decode(ndr);
                if (s == null)
                {
                    stringbinding = false;
                    // null termination
                    dualStringArray.Length += 2;
                    dualStringArray._secOffset = dualStringArray.Length;
                    continue;
                }

                listOfStringBindings.Add(s);
                dualStringArray.Length += s.Length;
            }
            else
            {
                var s = SecurityBinding.Decode(ndr);
                if (s == null)
                {
                    // null termination
                    dualStringArray.Length += 2;
                    break;
                }

                listOfSecurityBindings.Add(s);
                dualStringArray.Length += s.Length;
            }
        }

        // 2 bytes for num entries and 2 bytes for sec offset.
        dualStringArray.Length = dualStringArray.Length + 2 + 2;

        dualStringArray.StringBindings = listOfStringBindings.ToArray();
        dualStringArray.SecurityBindings = listOfSecurityBindings.ToArray();
        return dualStringArray;
    }

    /// <summary>
    /// Encode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    public void Encode(NdrCodec ndr)
    {
        // fill num entries
        // this is total length/2. since they are all shorts
        ndr.WriteUnsignedShort((Length - 4) / 2);
        ndr.WriteUnsignedShort(_secOffset / 2);

        var i = 0;
        if (StringBindings != null)
        {
            while (i < StringBindings.Length)
            {
                StringBindings[i].Encode(ndr);
                i++;
            }
            ndr.WriteUnsignedShort(0);
        }

        i = 0;
        if (SecurityBindings != null)
        {
            while (i < SecurityBindings.Length)
            {
                SecurityBindings[i].Encode(ndr);
                i++;
            }
            ndr.WriteUnsignedShort(0);
        }
    }

    private int _secOffset;
}
