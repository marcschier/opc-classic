// SPDX-License-Identifier: MIT

// Project-wide aliases: the legacy jcifs/jcifs-port tests use Opc.Classic.Dcom.Common.Ntlm.Thread
// (Java thread shim) intensively. After file-scoped namespace + outside-namespace usings,
// the implicit System.Threading.Thread brought in by ImplicitUsings collides with the
// Opc.Classic.Dcom.Common.Ntlm alias on `Thread` and `ThreadGroup` symbols. Bind these names project-wide
// to the Opc.Classic.Dcom.Common.Ntlm port to preserve the original test semantics.
global using Thread = Opc.Classic.Dcom.Common.Ntlm.Thread;
global using ThreadGroup = Opc.Classic.Dcom.Common.Ntlm.ThreadGroup;
