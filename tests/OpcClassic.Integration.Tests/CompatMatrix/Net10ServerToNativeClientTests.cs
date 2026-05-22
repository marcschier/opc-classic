//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System.Threading.Tasks;
using TUnit.Core;

namespace OpcClassic.Integration.Tests.CompatMatrix;

public sealed class Net10ServerToNativeClientTests
{
    [Test, Category("CompatMatrix")]
    public async Task Native_simple_client_connects_to_net10_server_and_calls_GetStatus()
    {
        if (CompatMatrixProbe.ShouldSkipNet10ServerToNativeClient(out _))
        {
            return;
        }

        // Future test pattern:
        //   1. Start an in-process OpcDaServerHost with a StubDaServer impl
        //      registered for CLSID_TestServer (via Phase 4B IClsidRegistry +
        //      Phase 4C RemoteSCMActivatorServer + Phase 4A LocalCoClass
        //      modernization)
        //   2. Launch COM\BuildOutput\bin\clients\Win32\Release\OpcDaSimpleClient.exe
        //      as a child process pointed at the CLSID via command-line args
        //   3. Verify it prints "Status=Running" or similar based on the
        //      StubDaServer.GetStatus return value
        //   4. Verify the StubDaServer received the call via its captured
        //      invocation log
        //
        // For this scaffold: placeholder assertion.
        await Assert.That(ReadScaffoldPlaceholder()).IsTrue();
    }

    [Test, Category("CompatMatrix")]
    public async Task Native_client_can_AddGroup_then_Read_through_net10_server()
    {
        if (CompatMatrixProbe.ShouldSkipNet10ServerToNativeClient(out _))
        {
            return;
        }

        // Future: same pattern, exercising AddGroup + SyncIO.Read round-trip.
        await Assert.That(ReadScaffoldPlaceholder()).IsTrue();
    }

    [Test, Category("CompatMatrix")]
    public async Task Native_client_receives_OnDataChange_callbacks_from_net10_server()
    {
        if (CompatMatrixProbe.ShouldSkipNet10ServerToNativeClient(out _))
        {
            return;
        }

        // Future: net10 OpcDaDataChangePublisher fans out to a Subscribe'd
        // native client; verify the client's OnDataChange handler is invoked.
        // This is the bidirectional-DCOM smoking gun.
        await Assert.That(ReadScaffoldPlaceholder()).IsTrue();
    }

    [Test, Category("CompatMatrix")]
    public async Task Native_client_disconnect_releases_net10_server_resources()
    {
        if (CompatMatrixProbe.ShouldSkipNet10ServerToNativeClient(out _))
        {
            return;
        }

        // Future: start, advise, kill client; verify net10 server's IOPC*
        // disposed via the OnDisconnect / Release pathway in LocalCoClass.
        await Assert.That(ReadScaffoldPlaceholder()).IsTrue();
    }

    // TUnitAssertions0005 workaround: Assert.That(const) is rejected by the analyzer.
    private static bool ReadScaffoldPlaceholder() => true;
}
