// SPDX-License-Identifier: MIT

using System;
using System.Threading;

namespace SharpCifs.Util.Sharpen;

public class Thread {
    private readonly ThreadGroup? _group;
    private readonly string? _name;
    private bool _daemon;
    private System.Threading.Thread? _thread;

    public Thread() {
    }

    public Thread(string name) => _name = name;

    public Thread(ThreadGroup group, string name) {
        _group = group;
        _name = name;
    }

    protected CancellationTokenSource Canceller { get; } = new();

    protected bool IsCanceled => Canceller.IsCancellationRequested;

    public virtual void Run() {
    }

    public string GetName() => _name ?? _thread?.Name ?? string.Empty;

    public void SetDaemon(bool daemon) => _daemon = daemon;

    public void Start() {
        _thread = new System.Threading.Thread(Run) {
            IsBackground = _daemon
        };
        if (!string.IsNullOrEmpty(_name)) {
            _thread.Name = _name;
        }
        _group?.Add(this);
        _thread.Start();
    }

    public void Interrupt() => Canceller.Cancel();

    public void Join() => _thread?.Join(TimeSpan.FromSeconds(5));

    public static void Sleep(int millisecondsTimeout) => System.Threading.Thread.Sleep(millisecondsTimeout);
}
