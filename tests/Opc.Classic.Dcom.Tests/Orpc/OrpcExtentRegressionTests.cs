//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using Opc.Classic.Dcom.Orpc;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Dcom.Tests.Orpc;

public sealed class OrpcExtentRegressionTests
{
    [Test]
    public async Task OrpcExtent_Constructor_CopiesPayload()
    {
        byte[] payload = [0xAA, 0xBB, 0xCC];
        var extent = new OrpcExtent(new Guid("10203040-5060-7080-90a0-b0c0d0e0f001"), payload);

        payload[0] = 0x00;

        await Assert.That(extent.Id).IsEqualTo(new Guid("10203040-5060-7080-90a0-b0c0d0e0f001"));
        await Assert.That(Convert.ToHexString(extent.Data.ToArray())).IsEqualTo("AABBCC");
    }

    [Test]
    public async Task OrpcThis_WithSingleExtent_WritesExpectedNdrLayout()
    {
        var value = new OrpcThis
        {
            Flags = 0x00000002u,
            CausalityId = new Guid("00112233-4455-6677-8899-aabbccddeeff"),
            Extensions =
            [
                new OrpcExtent(new Guid("10203040-5060-7080-90a0-b0c0d0e0f001"), new byte[] { 0x0A, 0x0B, 0x0C }),
            ],
        };

        string hex = Convert.ToHexString(WriteOrpcThis(value));

        await Assert.That(hex).IsEqualTo(
            "05000700" +
            "02000000" +
            "00000000" +
            "33221100554477668899AABBCCDDEEFF" +
            "00000200" +
            "01000000" +
            "00000000" +
            "04000200" +
            "02000000" +
            "08000200" +
            "00000000" +
            "403020106050807090A0B0C0D0E0F001" +
            "03000000" +
            "08000000" +
            "0A0B0C0000000000");
    }

    [Test]
    public async Task OrpcThis_WithSingleExtent_RoundTripsExtentFields()
    {
        var expected = new OrpcThis
        {
            Flags = 0x00000003u,
            CausalityId = new Guid("00112233-4455-6677-8899-aabbccddeeff"),
            Extensions =
            [
                new OrpcExtent(new Guid("10203040-5060-7080-90a0-b0c0d0e0f001"), new byte[] { 1, 2, 3, 4, 5 }),
            ],
        };
        byte[] bytes = WriteOrpcThis(expected);
        (OrpcThis actual, int position) = ReadOrpcThis(bytes);

        await Assert.That(actual.Version).IsEqualTo(OrpcComVersion.Default);
        await Assert.That(actual.Flags).IsEqualTo(0x00000003u);
        await Assert.That(actual.CausalityId).IsEqualTo(expected.CausalityId);
        await Assert.That(actual.Extensions!.Count).IsEqualTo(1);
        await Assert.That(actual.Extensions[0].Id).IsEqualTo(new Guid("10203040-5060-7080-90a0-b0c0d0e0f001"));
        await Assert.That(Convert.ToHexString(actual.Extensions[0].Data.ToArray())).IsEqualTo("0102030405");
        await Assert.That(position).IsEqualTo(bytes.Length);
    }

    [Test]
    public async Task OrpcThis_NullExtensions_WritesExpectedNullPointerLayout()
    {
        var value = new OrpcThis
        {
            Flags = 0u,
            CausalityId = Guid.Empty,
            Extensions = null,
        };

        string hex = Convert.ToHexString(WriteOrpcThis(value));

        await Assert.That(hex).IsEqualTo(
            "05000700" +
            "00000000" +
            "00000000" +
            "00000000000000000000000000000000" +
            "00000000");
    }

    [Test]
    public async Task OrpcThat_WithEmptyExtentArray_WritesExpectedNullPointerArrayLayout()
    {
        var value = new OrpcThat
        {
            Flags = 0x1Fu,
            Extensions = Array.Empty<OrpcExtent>(),
        };

        string hex = Convert.ToHexString(WriteOrpcThat(value));

        await Assert.That(hex).IsEqualTo(
            "1F000000" +
            "00000200" +
            "00000000" +
            "00000000" +
            "00000000");
    }

    [Test]
    public async Task OrpcThat_WithTwoExtents_RoundTripsPayloadsAndConsumesBuffer()
    {
        var expected = new OrpcThat
        {
            Flags = 0x05u,
            Extensions =
            [
                new OrpcExtent(new Guid("aaaaaaaa-0000-0000-0000-000000000001"), new byte[] { 0x10 }),
                new OrpcExtent(new Guid("bbbbbbbb-0000-0000-0000-000000000002"), new byte[] { 0x20, 0x21, 0x22, 0x23 }),
            ],
        };
        byte[] bytes = WriteOrpcThat(expected);
        (OrpcThat actual, int position) = ReadOrpcThat(bytes);

        await Assert.That(actual.Flags).IsEqualTo(0x05u);
        await Assert.That(actual.Extensions!.Count).IsEqualTo(2);
        await Assert.That(actual.Extensions[0].Id).IsEqualTo(new Guid("aaaaaaaa-0000-0000-0000-000000000001"));
        await Assert.That(Convert.ToHexString(actual.Extensions[0].Data.ToArray())).IsEqualTo("10");
        await Assert.That(actual.Extensions[1].Id).IsEqualTo(new Guid("bbbbbbbb-0000-0000-0000-000000000002"));
        await Assert.That(Convert.ToHexString(actual.Extensions[1].Data.ToArray())).IsEqualTo("20212223");
        await Assert.That(position).IsEqualTo(bytes.Length);
    }

    [Test]
    public async Task OrpcThis_ReadRejectsNonZeroReservedField()
    {
        byte[] bytes = Convert.FromHexString("050007000000000001000000");

        InvalidOperationException exception = Capture<InvalidOperationException>(() =>
        {
            var reader = new NdrReader(bytes);
            _ = OrpcThis.Read(ref reader);
        });

        await Assert.That(exception.Message).IsEqualTo("ORPC_THIS reserved1 must be zero but was 1.");
    }

    [Test]
    public async Task OrpcThat_ReadRejectsReservedFlagBits()
    {
        byte[] bytes = Convert.FromHexString("20000000");

        InvalidOperationException exception = Capture<InvalidOperationException>(() =>
        {
            var reader = new NdrReader(bytes);
            _ = OrpcThat.Read(ref reader);
        });

        await Assert.That(exception.Message).IsEqualTo("ORPC_THAT flags contain reserved bits: 0x00000020.");
    }

    [Test]
    public async Task OrpcThat_ReadRejectsNonZeroExtentArrayReservedField()
    {
        byte[] bytes = Convert.FromHexString(
            "00000000" +
            "00000200" +
            "00000000" +
            "01000000");

        InvalidOperationException exception = Capture<InvalidOperationException>(() =>
        {
            var reader = new NdrReader(bytes);
            _ = OrpcThat.Read(ref reader);
        });

        await Assert.That(exception.Message).IsEqualTo("ORPC_EXTENT_ARRAY reserved must be zero but was 1.");
    }

    [Test]
    public async Task OrpcThat_ReadRejectsNullExtentPointerForNonEmptyArray()
    {
        byte[] bytes = Convert.FromHexString(
            "00000000" +
            "00000200" +
            "01000000" +
            "00000000" +
            "00000000");

        InvalidOperationException exception = Capture<InvalidOperationException>(() =>
        {
            var reader = new NdrReader(bytes);
            _ = OrpcThat.Read(ref reader);
        });

        await Assert.That(exception.Message).IsEqualTo("ORPC_EXTENT_ARRAY extent pointer is null for a non-empty array.");
    }

    private static byte[] WriteOrpcThis(OrpcThis value)
    {
        byte[] buffer = new byte[256];
        var writer = new NdrWriter(buffer);
        value.Write(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static byte[] WriteOrpcThat(OrpcThat value)
    {
        byte[] buffer = new byte[256];
        var writer = new NdrWriter(buffer);
        value.Write(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static (OrpcThis Value, int Position) ReadOrpcThis(byte[] bytes)
    {
        var reader = new NdrReader(bytes);
        OrpcThis value = OrpcThis.Read(ref reader);
        return (value, reader.Position);
    }

    private static (OrpcThat Value, int Position) ReadOrpcThat(byte[] bytes)
    {
        var reader = new NdrReader(bytes);
        OrpcThat value = OrpcThat.Read(ref reader);
        return (value, reader.Position);
    }

    private static TException Capture<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }

        throw new InvalidOperationException($"Expected exception of type {typeof(TException).Name}.");
    }
}
