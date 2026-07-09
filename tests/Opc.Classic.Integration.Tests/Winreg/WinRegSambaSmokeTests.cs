// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Registry;
using Opc.Classic.Dcom.Winreg;

namespace Opc.Classic.Tests.Integration.Winreg;

public sealed class WinRegSambaSmokeTests
{
    private const string EnableVariable = "OPC_CLASSIC_INTEGRATION_SAMBA";

    [Test]
    [Category("Integration")]
    [Category("WinRegSambaSmoke")]
    public async Task WinRegSambaSmoke_opens_enumerates_and_closes_HKLM()
    {
        if (!IsConfigured(EnableVariable, out string reason))
        {
            SoftSkip(reason);
            return;
        }

        string host = ReadEnvironment("OPC_CLASSIC_SAMBA_HOST", "127.0.0.1");
        string user = ReadEnvironment("OPC_CLASSIC_SAMBA_USER", "opcuser");
        string password = ReadEnvironment("OPC_CLASSIC_SAMBA_PASSWORD", "opcpass");
        string domain = ReadEnvironment("OPC_CLASSIC_SAMBA_DOMAIN", "TESTDOMAIN");

        await using WinRegClient client = await WinRegClient.ConnectAsync(
            host,
            user,
            password,
            domain,
            CancellationToken.None).ConfigureAwait(false);

        PolicyHandle handle = await client.OpenHKLMAsync(CancellationToken.None).ConfigureAwait(false);
        try
        {
            await Assert.That(handle.Handle.Any(static value => value != 0)).IsTrue();

            string[] key = await client.EnumKeyAsync(handle, 0, CancellationToken.None).ConfigureAwait(false);

            await Assert.That(key.Length).IsGreaterThan(0);
            await Assert.That(key[0]).IsNotNull();
            await Assert.That(key[0].Length).IsGreaterThan(0);
        }
        finally
        {
            await client.CloseKeyAsync(handle, CancellationToken.None).ConfigureAwait(false);
        }
    }

    private static bool IsConfigured(string variableName, out string reason)
    {
        string? value = Environment.GetEnvironmentVariable(variableName);
        if (IsEnabled(value))
        {
            reason = string.Empty;
            return true;
        }

        reason = $"Soft-skipped unless {variableName}=1.";
        return false;
    }

    private static void SoftSkip(string reason) => Console.WriteLine(reason);

    private static bool IsEnabled(string? value) =>
        string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);

    private static string ReadEnvironment(string variableName, string defaultValue) =>
        Environment.GetEnvironmentVariable(variableName) is { Length: > 0 } value
            ? value
            : defaultValue;
}
