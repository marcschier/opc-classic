// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Internal.LegacyNdr;

public abstract class NdrObject
{
    public abstract void Encode(NdrBuffer dst);
    public abstract void Decode(NdrBuffer src);
}
