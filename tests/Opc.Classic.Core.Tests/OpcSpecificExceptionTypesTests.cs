//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic;
using TUnit.Core;

namespace Opc.Classic.Tests;

public sealed class OpcSpecificExceptionTypesTests
{
    private const int DefaultExceptionHResult = unchecked((int)0x80131500u);

    [Test]
    public async Task SpecExceptions_DefaultConstructor_SetsFailResultAndDefaultMessage()
    {
        OpcException[] exceptions =
        [
            new OpcDaException(),
            new OpcHdaException(),
            new OpcDxException(),
            new OpcAeException(),
        ];

        foreach (OpcException exception in exceptions)
        {
            await Assert.That(exception.ResultId).IsEqualTo(OpcResultId.Fail);
            await Assert.That(exception.HResult).IsEqualTo(DefaultExceptionHResult);
            await Assert.That(exception.InnerException).IsNull();
            await Assert.That(exception.Message).Contains(exception.GetType().FullName!);
        }
    }

    [Test]
    public async Task SpecExceptions_MessageConstructor_PreservesMessageAndFailResult()
    {
        OpcException[] exceptions =
        [
            new OpcDaException("DA read failed"),
            new OpcHdaException("HDA read failed"),
            new OpcDxException("DX connect failed"),
            new OpcAeException("AE subscribe failed"),
        ];

        foreach (OpcException exception in exceptions)
        {
            await Assert.That(exception.ResultId).IsEqualTo(OpcResultId.Fail);
            await Assert.That(exception.HResult).IsEqualTo(DefaultExceptionHResult);
            await Assert.That(exception.InnerException).IsNull();
            await Assert.That(exception.Message).Contains("failed");
        }
    }

    [Test]
    public async Task SpecExceptions_MessageAndInnerConstructor_PreservesInnerException()
    {
        var inner = new InvalidOperationException("inner failure");
        OpcException[] exceptions =
        [
            new OpcDaException("DA failure", inner),
            new OpcHdaException("HDA failure", inner),
            new OpcDxException("DX failure", inner),
            new OpcAeException("AE failure", inner),
        ];

        foreach (OpcException exception in exceptions)
        {
            await Assert.That(exception.ResultId).IsEqualTo(OpcResultId.Fail);
            await Assert.That(exception.HResult).IsEqualTo(DefaultExceptionHResult);
            await Assert.That(object.ReferenceEquals(exception.InnerException, inner)).IsTrue();
            await Assert.That(exception.Message).Contains("failure");
        }
    }

    [Test]
    public async Task SpecExceptions_ResultIdConstructor_UsesResultIdStringAsMessage()
    {
        OpcException[] exceptions =
        [
            new OpcDaException(OpcResultId.UnknownItemId),
            new OpcHdaException(OpcResultId.InvalidHandle),
            new OpcDxException(OpcResultId.BadType),
            new OpcAeException(OpcResultId.Range),
        ];

        OpcResultId[] expectedResultIds =
        [
            OpcResultId.UnknownItemId,
            OpcResultId.InvalidHandle,
            OpcResultId.BadType,
            OpcResultId.Range,
        ];

        for (int i = 0; i < exceptions.Length; i++)
        {
            await Assert.That(exceptions[i].ResultId).IsEqualTo(expectedResultIds[i]);
            await Assert.That(exceptions[i].HResult).IsEqualTo(DefaultExceptionHResult);
            await Assert.That(exceptions[i].InnerException).IsNull();
            await Assert.That(exceptions[i].Message).IsEqualTo(expectedResultIds[i].ToString());
        }
    }

    [Test]
    public async Task SpecExceptions_ResultIdAndMessageConstructor_PreservesMessage()
    {
        OpcException[] exceptions =
        [
            new OpcDaException(OpcResultId.UnknownItemId, "DA item was not found"),
            new OpcHdaException(OpcResultId.InvalidHandle, "HDA handle was invalid"),
            new OpcDxException(OpcResultId.BadType, "DX data type was invalid"),
            new OpcAeException(OpcResultId.Range, "AE condition was out of range"),
        ];

        OpcResultId[] expectedResultIds =
        [
            OpcResultId.UnknownItemId,
            OpcResultId.InvalidHandle,
            OpcResultId.BadType,
            OpcResultId.Range,
        ];

        for (int i = 0; i < exceptions.Length; i++)
        {
            await Assert.That(exceptions[i].ResultId).IsEqualTo(expectedResultIds[i]);
            await Assert.That(exceptions[i].HResult).IsEqualTo(DefaultExceptionHResult);
            await Assert.That(exceptions[i].InnerException).IsNull();
            await Assert.That(exceptions[i].Message).Contains("was");
        }
    }

    [Test]
    public async Task SpecExceptions_ResultIdMessageAndInnerConstructor_PreservesAllValues()
    {
        var inner = new InvalidOperationException("transport failure");
        OpcException[] exceptions =
        [
            new OpcDaException(OpcResultId.UnknownItemId, "DA item was not found", inner),
            new OpcHdaException(OpcResultId.InvalidHandle, "HDA handle was invalid", inner),
            new OpcDxException(OpcResultId.BadType, "DX data type was invalid", inner),
            new OpcAeException(OpcResultId.Range, "AE condition was out of range", inner),
        ];

        OpcResultId[] expectedResultIds =
        [
            OpcResultId.UnknownItemId,
            OpcResultId.InvalidHandle,
            OpcResultId.BadType,
            OpcResultId.Range,
        ];

        for (int i = 0; i < exceptions.Length; i++)
        {
            await Assert.That(exceptions[i].ResultId).IsEqualTo(expectedResultIds[i]);
            await Assert.That(exceptions[i].HResult).IsEqualTo(DefaultExceptionHResult);
            await Assert.That(object.ReferenceEquals(exceptions[i].InnerException, inner)).IsTrue();
            await Assert.That(exceptions[i].Message).Contains("was");
        }
    }

    [Test]
    public async Task PlatformNotSupported_DefaultConstructor_UsesStablePlatformMessage()
    {
        var exception = new OpcPlatformNotSupportedException();

        await Assert.That(exception.ResultId).IsEqualTo(OpcResultId.Fail);
        await Assert.That(exception.HResult).IsEqualTo(DefaultExceptionHResult);
        await Assert.That(exception.InnerException).IsNull();
        await Assert.That(exception.Message).IsEqualTo("OPC feature is not supported on this platform.");
    }

    [Test]
    public async Task PlatformNotSupported_MessageConstructor_PreservesMessage()
    {
        var exception = new OpcPlatformNotSupportedException("SSO is unavailable");

        await Assert.That(exception.ResultId).IsEqualTo(OpcResultId.Fail);
        await Assert.That(exception.HResult).IsEqualTo(DefaultExceptionHResult);
        await Assert.That(exception.InnerException).IsNull();
        await Assert.That(exception.Message).IsEqualTo("SSO is unavailable");
    }

    [Test]
    public async Task PlatformNotSupported_MessageAndInnerConstructor_PreservesInnerException()
    {
        var inner = new PlatformNotSupportedException("native platform feature missing");
        var exception = new OpcPlatformNotSupportedException("OPC platform feature missing", inner);

        await Assert.That(exception.ResultId).IsEqualTo(OpcResultId.Fail);
        await Assert.That(exception.HResult).IsEqualTo(DefaultExceptionHResult);
        await Assert.That(object.ReferenceEquals(exception.InnerException, inner)).IsTrue();
        await Assert.That(exception.Message).IsEqualTo("OPC platform feature missing");
    }
}
