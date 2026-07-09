// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Reflection;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;

namespace Opc.Classic.Dcom.Crypto.Tests;

[NotInParallel]
public sealed class PasswordZeroizationTests
{
    private const string Password = "Password";
    private const string User = "User";
    private const string Domain = "Domain";

    private static readonly byte[] Challenge = Convert.FromHexString("0123456789ABCDEF");
    private static readonly byte[] ClientNonce = Convert.FromHexString("AAAAAAAAAAAAAAAA");
    private static readonly byte[] TargetInformation = Convert.FromHexString(
        "02000C0044006F006D00610069006E00" +
        "01000C00530065007200760065007200" +
        "00000000");

    [Test]
    [Arguments("lm-response")]
    [Arguments("ntlm-response")]
    [Arguments("lmv2-response")]
    [Arguments("ntlmv2-response")]
    [Arguments("ntlm2-session-response")]
    [Arguments("ntlmv2-session-key")]
    [Arguments("ntlm2-session-key")]
    public async Task PasswordDerivedPooledBuffers_AreZeroedBeforeReturn(string path)
    {
        var snapshots = new List<BufferSnapshot>();
        using (InstallSensitiveBufferObserver(snapshots))
        {
            ExecutePasswordPath(path);
        }

        await Assert.That(snapshots.Count).IsGreaterThan(0);
        await Assert.That(snapshots.Any(static snapshot => snapshot.Length > 0)).IsTrue();
        foreach (var snapshot in snapshots)
        {
            await Assert.That(snapshot.Bytes.All(static b => b == 0)).IsTrue();
        }
    }

    private static void ExecutePasswordPath(string path)
    {
        switch (path)
        {
            case "lm-response":
                _ = Responses.GetLMResponse(Password, Challenge);
                break;
            case "ntlm-response":
                _ = Responses.GetNTLMResponse(Password, Challenge);
                break;
            case "lmv2-response":
                _ = Responses.GetLMv2Response(Domain, User, Password, Challenge, ClientNonce);
                break;
            case "ntlmv2-response":
                _ = Responses.GetNTLMv2Response(Domain, User, Password, TargetInformation, Challenge, ClientNonce);
                break;
            case "ntlm2-session-response":
                _ = Responses.GetNTLM2SessionResponse(Password, Challenge, ClientNonce);
                break;
            case "ntlmv2-session-key":
                InvokeNtlmKeyFactory(
                    "GetNTLMv2UserSessionKey",
                    Domain,
                    User,
                    Password,
                    Challenge,
                    Combine(TargetInformation, ClientNonce));
                break;
            case "ntlm2-session-key":
                InvokeNtlmKeyFactory(
                    "GetNTLM2SessionResponseUserSessionKey",
                    Password,
                    Combine(Challenge, ClientNonce));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(path), path, "Unknown password path.");
        }
    }

    private static void InvokeNtlmKeyFactory(string methodName, params object[] args)
    {
        var type = typeof(Responses).Assembly.GetType("Opc.Classic.Dcom.Rpc.Auth.ntlm.NTLMKeyFactory", throwOnError: true)!;
        var instance = Activator.CreateInstance(type)!;
        var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public)!;
        _ = method.Invoke(instance, args);
    }

    private static IDisposable InstallSensitiveBufferObserver(List<BufferSnapshot> snapshots)
    {
        var type = typeof(Responses).Assembly.GetType("Opc.Classic.Dcom.Rpc.Auth.ntlm.SensitiveBufferPool", throwOnError: true)!;
        var method = type.GetMethod("SetReturnObserverForTests", BindingFlags.Static | BindingFlags.NonPublic)!;
        var callback = new Action<string, byte[], int>((_, buffer, length) =>
        {
            snapshots.Add(new BufferSnapshot(buffer.AsSpan(0, length).ToArray(), length));
        });
        return (IDisposable)method.Invoke(null, new object?[] { callback })!;
    }

    private static byte[] Combine(byte[] first, byte[] second)
    {
        var result = new byte[first.Length + second.Length];
        first.CopyTo(result, 0);
        second.CopyTo(result, first.Length);
        return result;
    }

    private sealed record BufferSnapshot(byte[] Bytes, int Length);
}
