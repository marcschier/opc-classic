//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Kerberos.NET;
using Kerberos.NET.Client;
using Kerberos.NET.Crypto;
using Kerberos.NET.Entities;
using Kerberos.NET.Transport;
using Opc.Classic.Dcom.Kerberos;
using TUnit.Core;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class KerberosConnectionContextIntegrationTests {
    private const string Realm = "EXAMPLE.COM";
    private const string Spn = "RPCSS/server.example.com";
    private const string Username = "alice";
    private const string Password = "correct horse battery staple";

    [Test]
    public async Task AcquireApRequestAsync_observes_precanceled_token_before_kdc_io() {
        var context = CreatePasswordContext();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var thrown = await CaptureExceptionAsync(() => context.AcquireApRequestAsync(cts.Token));

        await Assert.That(thrown is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task AcquireApRequestAsync_enters_KerberosNet_path_and_fails_without_reachable_kdc() {
        var configPath = Path.Combine(AppContext.BaseDirectory, "krb5-p3d-localhost.conf");
        await File.WriteAllTextAsync(
            configPath,
            "[libdefaults]\n" +
            "    default_realm = EXAMPLE.COM\n" +
            "    dns_lookup_kdc = false\n" +
            "[realms]\n" +
            "    EXAMPLE.COM = {\n" +
            "        kdc = 127.0.0.1:9\n" +
            "    }\n");

        var previousConfig = Environment.GetEnvironmentVariable("KRB5_CONFIG");
        Environment.SetEnvironmentVariable("KRB5_CONFIG", configPath);

        try {
            var context = CreatePasswordContext();
            byte[]? token = null;
            var thrown = await CaptureExceptionAsync(async () => {
                token = await context.AcquireApRequestAsync();
            });

            if (thrown is null) {
                await Assert.That(token).IsNotNull();
                await Assert.That(token!.Length > 0).IsTrue();
                await Assert.That(token[0]).IsEqualTo((byte)0x60);
            }
            else {
                // No real KDC is available in unit tests; Phase 14A Windows CI will exercise success.
                await Assert.That(thrown is not NotImplementedException).IsTrue();
                await Assert.That(IsExpectedKdcFailure(thrown)).IsTrue();
            }
        }
        finally {
            Environment.SetEnvironmentVariable("KRB5_CONFIG", previousConfig);
        }
    }

    [Test]
    public async Task ProcessApResponseAsync_round_trips_raw_ap_rep_and_returns_subsession_key() {
        var context = CreatePasswordContext();
        var sessionKey = KrbEncryptionKey.Generate(EncryptionType.AES256_CTS_HMAC_SHA1_96);
        var subSessionKey = KrbEncryptionKey.Generate(EncryptionType.AES256_CTS_HMAC_SHA1_96);
        var cTime = DateTimeOffset.UtcNow;
        const int cuSec = 123456;
        const int sequenceNumber = 789;

        SetSessionContext(context, new ApplicationSessionContext {
            SessionKey = sessionKey,
            CTime = cTime,
            CuSec = cuSec,
            SequenceNumber = sequenceNumber,
        });

        var apRep = CreateApRep(sessionKey, subSessionKey, cTime, cuSec, sequenceNumber);

        var actual = await context.ProcessApResponseAsync(apRep);

        await Assert.That(actual.SequenceEqual(subSessionKey.KeyValue.ToArray())).IsTrue();
    }

    [Test]
    public async Task ProcessApResponseAsync_accepts_gss_api_ap_rep_token_id_frame() {
        var context = CreatePasswordContext();
        var sessionKey = KrbEncryptionKey.Generate(EncryptionType.AES256_CTS_HMAC_SHA1_96);
        var subSessionKey = KrbEncryptionKey.Generate(EncryptionType.AES256_CTS_HMAC_SHA1_96);
        var cTime = DateTimeOffset.UtcNow;
        const int cuSec = 654321;
        const int sequenceNumber = 987;

        SetSessionContext(context, new ApplicationSessionContext {
            SessionKey = sessionKey,
            CTime = cTime,
            CuSec = cuSec,
            SequenceNumber = sequenceNumber,
        });

        var apRep = CreateApRep(sessionKey, subSessionKey, cTime, cuSec, sequenceNumber);
        var framedApRep = new byte[apRep.Length + 2];
        framedApRep[0] = 0x02;
        framedApRep[1] = 0x00;
        apRep.CopyTo(framedApRep.AsMemory(2));

        var actual = await context.ProcessApResponseAsync(framedApRep);

        await Assert.That(actual.SequenceEqual(subSessionKey.KeyValue.ToArray())).IsTrue();
    }

    [Test]
    public async Task ProcessApResponseAsync_with_malformed_input_throws_kerberos_protocol_exception() {
        var context = CreatePasswordContext();

        var thrown = await CaptureExceptionAsync(() => context.ProcessApResponseAsync(new byte[] { 0x01, 0x02 }));

        await Assert.That(thrown is KerberosProtocolException).IsTrue();
    }

    private static KerberosConnectionContext CreatePasswordContext() {
        return new KerberosConnectionContext(new KerberosAuthInfo(Realm, Spn, Username, null, Password, null));
    }

    private static byte[] CreateApRep(
        KrbEncryptionKey sessionKey,
        KrbEncryptionKey subSessionKey,
        DateTimeOffset cTime,
        int cuSec,
        int sequenceNumber) {
        var response = new KrbEncApRepPart {
            CTime = cTime,
            CuSec = cuSec,
            SequenceNumber = sequenceNumber,
            SubSessionKey = subSessionKey,
        };

        var encryptedPart = KrbEncryptedData.Encrypt(
            response.EncodeApplication(),
            sessionKey.AsKey(KeyUsage.EncApRepPart),
            KeyUsage.EncApRepPart);

        return new KrbApRep { EncryptedPart = encryptedPart }.EncodeApplication().ToArray();
    }

    private static void SetSessionContext(KerberosConnectionContext context, ApplicationSessionContext sessionContext) {
        var field = typeof(KerberosConnectionContext).GetField("_sessionContext", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("KerberosConnectionContext session field was not found.");
        field.SetValue(context, sessionContext);
    }

    private static async Task<Exception?> CaptureExceptionAsync(Func<Task> action) {
        try {
            await action();
            return null;
        }
        catch (Exception ex) {
            return ex;
        }
    }

    private static bool IsExpectedKdcFailure(Exception exception) {
        return exception is KerberosProtocolException or KerberosTransportException or InvalidOperationException or OperationCanceledException ||
            exception.GetType().FullName?.Contains("SocketException", StringComparison.Ordinal) == true ||
            exception.GetType().FullName?.Contains("IOException", StringComparison.Ordinal) == true ||
            (exception.InnerException is not null && IsExpectedKdcFailure(exception.InnerException));
    }
}
