// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using BenchmarkDotNet.Running;

namespace Opc.Classic.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
