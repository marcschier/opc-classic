//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Runtime.CompilerServices;

namespace Opc.Classic.Dcom.Smb.Tests.Pcap;

public abstract class PcapFixtureBase
{
    protected static async Task<Smb2PcapReplayer.ReplayResult> ReplayNegotiateFixtureAsync(string fixtureFileName)
    {
        var replayer = Smb2PcapReplayer.FromFile(GetFixturePath(fixtureFileName));
        Smb2PcapReplayer.ReplayResult result = await replayer.ReplayNegotiateAsync("pcap-fixture");

        await Assert.That(result.MatchedClientPackets).IsEqualTo(1);
        await Assert.That(result.FedServerPackets).IsEqualTo(1);
        return result;
    }

    protected static async Task<Smb2PcapReplayer.ReplayResult> ReplayNegotiateFixtureAsync(
        string fixtureFileName,
        Smb2Dialect expectedDialect,
        bool expectedSigningRequired,
        bool expectedEncryptionSupported)
    {
        Smb2PcapReplayer.ReplayResult result = await ReplayNegotiateFixtureAsync(fixtureFileName);
        await Assert.That(result.NegotiatedDialect).IsEqualTo(expectedDialect);
        await Assert.That(result.SigningRequired).IsEqualTo(expectedSigningRequired);
        await Assert.That(result.EncryptionSupported).IsEqualTo(expectedEncryptionSupported);
        return result;
    }

    private static string GetFixturePath(
        string fixtureFileName,
        [CallerFilePath] string sourceFilePath = "")
    {
        string? sourceDirectory = Path.GetDirectoryName(sourceFilePath);
        if (sourceDirectory is not null)
        {
            string sourcePath = Path.Combine(sourceDirectory, "Fixtures", fixtureFileName);
            if (File.Exists(sourcePath))
            {
                return sourcePath;
            }
        }

        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            string localPath = Path.Combine(directory.FullName, "Pcap", "Fixtures", fixtureFileName);
            if (File.Exists(localPath))
            {
                return localPath;
            }

            string repoPath = Path.Combine(
                directory.FullName,
                "tests",
                "Opc.Classic.Dcom.Smb.Tests",
                "Pcap",
                "Fixtures",
                fixtureFileName);
            if (File.Exists(repoPath))
            {
                return repoPath;
            }
        }

        throw new FileNotFoundException("Could not locate SMB2 PCAP fixture.", fixtureFileName);
    }
}
