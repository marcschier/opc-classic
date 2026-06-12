// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Internal.LegacyNdr;

public abstract class NdrObject
{
    public abstract void Encode(NdrBuffer dst);
    public abstract void Decode(NdrBuffer src);
}
