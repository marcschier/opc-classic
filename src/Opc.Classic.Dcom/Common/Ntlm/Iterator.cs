// SPDX-License-Identifier: MIT

using System;

namespace Opc.Classic.Dcom.Common.Ntlm;

public abstract class Iterator<T>
{
    public abstract bool HasNext();

    public abstract T Next();

    public virtual void Remove() => throw new NotSupportedException();
}
