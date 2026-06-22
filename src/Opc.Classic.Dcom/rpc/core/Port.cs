// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Rpc.Core;

/// <summary>
/// Port
/// </summary>
public class Port : NdrOp
{
    /// <summary>
    /// Port specification
    /// </summary>
    public string PortSpec { get; set; }

    /// <summary>
    /// Create
    /// </summary>
    public Port() :
        this(null)
    {
    }

#pragma warning disable RECS0154 // Parameter is never used
    /// <summary>
    /// Create port
    /// </summary>
    /// <param name="portSpec">Text value used as the port spec.</param>
    public Port(string portSpec) => PortSpec = portSpec;
#pragma warning restore RECS0154 // Parameter is never used

    /// <override/>
    public override void Read(NdrCodec ndr)
    {
        var length = ndr.ReadUnsignedShort();
        if (length > 0)
        {
            var buf = ndr.Buffer;
            var portSpec = new char[length - 1];
            ndr.ReadCharacterArray(portSpec, 0, portSpec.Length);
            ndr.ReadUnsignedSmall(); // null terminator
            PortSpec = new string(portSpec);
        }
        else
        {
            PortSpec = null;
        }
    }

    /// <override/>
    public override void Write(NdrCodec ndr)
    {
        char[] spec;
        if (PortSpec != null)
        {
            spec = new char[PortSpec.Length + 1];
            PortSpec.CopyTo(0, spec, 0, PortSpec.Length - 0);
        }
        else
        {
            spec = Array.Empty<char>();
        }
        ndr.WriteUnsignedShort(spec.Length);
        if (spec.Length > 0)
        {
            ndr.WriteCharacterArray(spec, 0, spec.Length);
        }
    }

    /// <override/>
    public override bool Equals(object obj)
    {
        if (!(obj is Port other))
        {
            return false;
        }
        return string.Equals(PortSpec, other.PortSpec, StringComparison.Ordinal);
    }

    /// <override/>
    public override int GetHashCode() => PortSpec is null ? 0 : StringComparer.Ordinal.GetHashCode(PortSpec);
}
