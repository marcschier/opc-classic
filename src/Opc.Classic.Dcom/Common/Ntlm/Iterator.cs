// SPDX-License-Identifier: MIT

using System;

namespace SharpCifs.Util.Sharpen;

public abstract class Iterator<T> {
    public abstract bool HasNext();

    public abstract T Next();

    public virtual void Remove() => throw new NotSupportedException();
}
