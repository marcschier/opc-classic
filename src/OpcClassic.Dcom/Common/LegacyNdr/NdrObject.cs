// SPDX-License-Identifier: EPL-1.0

namespace OpcClassic.Dcom.Internal.LegacyNdr;

public abstract class NdrObject
{
    public abstract void Encode(NdrBuffer dst);

    public abstract void Decode(NdrBuffer src);
}
