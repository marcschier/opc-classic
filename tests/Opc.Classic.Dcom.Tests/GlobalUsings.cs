// SPDX-License-Identifier: MIT

// Project-wide aliases: the legacy jcifs/jcifs-port tests use SharpCifs.Util.Sharpen.Thread
// (Java thread shim) intensively. After file-scoped namespace + outside-namespace usings,
// the implicit System.Threading.Thread brought in by ImplicitUsings collides with the
// SharpCifs alias on `Thread` and `ThreadGroup` symbols. Bind these names project-wide
// to the SharpCifs port to preserve the original test semantics.
global using Thread = SharpCifs.Util.Sharpen.Thread;
global using ThreadGroup = SharpCifs.Util.Sharpen.ThreadGroup;
