// SPDX-License-Identifier: MIT

using System;
using System.IO;

namespace SharpCifs.Util.Sharpen;

public sealed class PrintWriter : IDisposable
{
    private readonly TextWriter _writer;

    public PrintWriter(TextWriter writer) => _writer = writer;

    public void Write(string value) => _writer.Write(value);

    public void Flush() => _writer.Flush();

    public void Close() => _writer.Close();

    public void Dispose() => _writer.Dispose();
}
