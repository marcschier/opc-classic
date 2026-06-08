//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Mcp.Capture;
using TUnit.Core;

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class CaptureExceptionTests
{
    [Test]
    public async Task Constructor_Parameterless_SetsDefaultMessageAndNoInnerException()
    {
        var exception = new CaptureException();

        await Assert.That(exception.Message).Contains(nameof(CaptureException));
        await Assert.That(exception.InnerException).IsNull();
    }

    [Test]
    public async Task Constructor_Message_SetsMessageAndNoInnerException()
    {
        var exception = new CaptureException("missing capture privilege");

        await Assert.That(exception.Message).IsEqualTo("missing capture privilege");
        await Assert.That(exception.InnerException).IsNull();
    }

    [Test]
    public async Task Constructor_MessageAndInner_SetsBothProperties()
    {
        var inner = new InvalidOperationException("inner failure");
        var exception = new CaptureException("outer failure", inner);

        await Assert.That(exception.Message).IsEqualTo("outer failure");
        await Assert.That(exception.InnerException).IsEqualTo(inner);
    }
}
