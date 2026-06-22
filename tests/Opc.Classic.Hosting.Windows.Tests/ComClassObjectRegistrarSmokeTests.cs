// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Runtime.Versioning;
using Opc.Classic.Hosting.Windows;

namespace Opc.Classic.Hosting.Tests.Windows;

public sealed class ComClassObjectRegistrarSmokeTests
{
    [Test, NotInParallel]
    public async Task Register_resume_revoke_lifecycle_succeeds_on_windows()
    {
        if (!OperatingSystem.IsWindows())
        {
            string reason = GetNonWindowsSkipReason();
            await Assert.That(reason).IsNotEqualTo(string.Empty);
            return;
        }

        RegistrarResult result = RunRegistrarLifecycleOnDedicatedMtaThread();
        Console.WriteLine($"COM class-object registration cookie: {result.Cookie}");

        await Assert.That(result.TimedOut).IsFalse();
        await Assert.That(result.Exception).IsNull();
        await Assert.That(result.Cookie).IsNotEqualTo(0U);
    }

    [SupportedOSPlatform("windows")]
    private static RegistrarResult RunRegistrarLifecycleOnDedicatedMtaThread()
    {
        uint cookie = 0;
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            bool initialized = false;
            uint registeredCookie = 0;
            try
            {
                ComClassObjectRegistrar.InitializeMultithreaded();
                initialized = true;
                registeredCookie = ComClassObjectRegistrar.RegisterClassObject(Guid.NewGuid(), suspended: true);
                cookie = registeredCookie;
                ComClassObjectRegistrar.ResumeClassObjects();
                ComClassObjectRegistrar.RevokeClassObject(registeredCookie);
                registeredCookie = 0;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                if (registeredCookie != 0)
                {
                    try
                    {
                        ComClassObjectRegistrar.RevokeClassObject(registeredCookie);
                    }
                    catch (Exception ex)
                    {
                        exception ??= ex;
                    }
                }

                if (initialized)
                {
                    ComClassObjectRegistrar.Uninitialize();
                }
            }
        })
        {
            IsBackground = true,
        };

        thread.SetApartmentState(ApartmentState.MTA);
        thread.Start();
        bool completed = thread.Join(TimeSpan.FromSeconds(15));

        return new RegistrarResult(cookie, exception, TimedOut: !completed);
    }

    private static string GetNonWindowsSkipReason() =>
        "COM class-object registrar smoke requires Windows ole32/COM.";

    private sealed record RegistrarResult(uint Cookie, Exception? Exception, bool TimedOut);
}
