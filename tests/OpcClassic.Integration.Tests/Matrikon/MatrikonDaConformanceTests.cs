//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using TUnit.Core;

namespace OpcClassic.Tests.Integration.Matrikon;

public sealed class MatrikonDaConformanceTests
{
    [Test, Category("MatrikonConformance")]
    public async Task GetStatus_returns_running()
    {
        if (MatrikonServerProbe.ShouldSkip(out _))
        {
            return;
        }

        // Scaffold: future test uses real DCOM CallChannel (Phase 4 prerequisite)
        await Task.CompletedTask;
    }

    [Test, Category("MatrikonConformance")]
    public async Task Read_Random_Int4_returns_value_with_Good_quality()
    {
        if (MatrikonServerProbe.ShouldSkip(out _))
        {
            return;
        }

        // Future: read "Random.Int4", assert Quality.Master == Good
        await Task.CompletedTask;
    }

    [Test, Category("MatrikonConformance")]
    public async Task BucketBrigade_Boolean_can_be_written_then_read_back()
    {
        if (MatrikonServerProbe.ShouldSkip(out _))
        {
            return;
        }

        // Future: Write(true), then Read returns true; quality Good
        await Task.CompletedTask;
    }

    [Test, Category("MatrikonConformance")]
    public async Task ReadError_Int1_returns_OPC_E_BADRIGHTS()
    {
        if (MatrikonServerProbe.ShouldSkip(out _))
        {
            return;
        }

        // Future: read "Read Error.Int1", assert HRESULT == OPC_E_BADRIGHTS
        // The point of Matrikon's "Error" tags is to validate client error-handling paths.
        await Task.CompletedTask;
    }

    [Test, Category("MatrikonConformance")]
    public async Task Subscription_on_Random_Real4_delivers_OnDataChange()
    {
        if (MatrikonServerProbe.ShouldSkip(out _))
        {
            return;
        }

        // Future: subscribe to Random.Real4 with 100ms update rate;
        // assert at least one OnDataChange callback fires within 1s
        await Task.CompletedTask;
    }
}
