// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace SharpCifs.Util.Sharpen;

public sealed class ThreadGroup
{
    private readonly List<Thread> _threads = new();

    public ThreadGroup(string name) => Name = name;

    public string Name { get; }

    internal void Add(Thread thread)
    {
        lock (_threads)
        {
            _threads.Add(thread);
        }
    }

    public void Interrupt()
    {
        lock (_threads)
        {
            foreach (var thread in _threads)
            {
                thread.Interrupt();
            }
        }
    }
}
