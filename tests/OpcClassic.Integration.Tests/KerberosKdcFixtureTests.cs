//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Diagnostics;
using OpcClassic.Tests.Fixtures;
using TUnit.Core;

namespace OpcClassic.Integration.Tests;

public sealed class KerberosKdcFixtureTests
{
    [Test, NotInParallel]
    public async Task KerberosKdcFixture_starts_and_exposes_realm()
    {
        if (!IsDockerAvailable())
        {
            return;
        }

        await using var kdc = await KerberosKdcFixture.StartAsync();

        await Assert.That(kdc.Realm).IsEqualTo("EXAMPLE.COM");
        await Assert.That(kdc.Kdc).Contains(":");
        await Assert.That(kdc.Port).IsGreaterThan(0);
    }

    private static bool IsDockerAvailable()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "info",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            if (process is null)
            {
                return false;
            }

            if (!process.WaitForExit(2000))
            {
                process.Kill(entireProcessTree: true);
                return false;
            }

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
