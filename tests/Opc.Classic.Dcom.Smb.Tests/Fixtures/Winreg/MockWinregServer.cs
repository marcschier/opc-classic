//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.Globalization;
using System.IO;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Registry.Smb;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;

namespace Opc.Classic.Dcom.Smb.Tests.Fixtures.Winreg;

public sealed class MockWinregServer {
    public const string WinregSyntax = "338cd001-2244-31f1-aaaa-900038001003:1.0";

    private readonly byte[] _bindResponse;
    private readonly byte[] _expectedRequest;
    private readonly byte[] _response;
    private int _receiveCount;
    private int _sendCount;

    private MockWinregServer(byte[] bindResponse, byte[] expectedRequest, byte[] response) {
        _bindResponse = bindResponse;
        _expectedRequest = expectedRequest;
        _response = response;
    }

    private byte[] _lastCanonicalRequest = Array.Empty<byte>();

    public byte[] GetLastCanonicalRequest() => (byte[])_lastCanonicalRequest.Clone();

    public static (RegistryStub Client, MockWinregServer Server) CreateClient(
        string expectedRequestFixtureName,
        string responseFixtureName) {
        var server = new MockWinregServer(
            ReadFixture("bind_response.bin"),
            ReadFixture(expectedRequestFixtureName),
            ReadFixture(responseFixtureName));
        var client = new RegistryStub("127.0.0.1") {
            Address = "ncacn_np:mock[\\PIPE\\winreg]",
            TransportFactory = new ReplayTransportFactory(server),
        };
        return (client, server);
    }

    public static byte[] ReadFixture(string fileName) => File.ReadAllBytes(GetFixturePath(fileName));

    public void AssertCompleted() {
        if (_sendCount != 2 || _receiveCount != 2) {
            throw new InvalidOperationException(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"Expected bind and request round-trip, sent {_sendCount}, received {_receiveCount}."));
        }
    }

    private static string GetFixturePath(string fileName) {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent) {
            var localPath = Path.Combine(directory.FullName, "Fixtures", "Winreg", fileName);
            if (File.Exists(localPath)) {
                return localPath;
            }

            var repoPath = Path.Combine(directory.FullName, "tests", "Opc.Classic.Dcom.Smb.Tests", "Fixtures", "Winreg", fileName);
            if (File.Exists(repoPath)) {
                return repoPath;
            }
        }

        throw new FileNotFoundException("Could not locate WINREG fixture.", fileName);
    }

    private byte[] DequeueResponse() {
        _receiveCount++;
        return _receiveCount switch {
            1 => _bindResponse,
            2 => _response,
            _ => throw new InvalidOperationException("No queued WINREG fixture response."),
        };
    }

    private void RecordRequest(byte[] request) {
        _sendCount++;
        if (_sendCount == 1) {
            ValidateBindRequest(request);
            return;
        }

        _lastCanonicalRequest = CanonicalizeRequest(request);
        if (!_lastCanonicalRequest.AsSpan().SequenceEqual(_expectedRequest)) {
            throw new InvalidOperationException("WINREG request bytes did not match the fixture.");
        }
    }

    private static byte[] CanonicalizeRequest(byte[] request) {
        var canonical = (byte[])request.Clone();
        canonical.AsSpan(ConnectionOrientedPdu.CALL_ID_OFFSET, sizeof(int)).Clear();
        var opnum = BinaryPrimitives.ReadUInt16LittleEndian(canonical.AsSpan(22, sizeof(ushort)));
        var stubOffset = 24;
        if (opnum == 2) {
            canonical.AsSpan(stubOffset, sizeof(int)).Clear();
        }
        else if (opnum == 9) {
            ClearReferents(canonical, stubOffset, 28, 44, 52, 68);
        }

        return canonical;
    }

    private static void ClearReferents(byte[] request, int stubOffset, params int[] offsets) {
        foreach (var offset in offsets) {
            request.AsSpan(stubOffset + offset, sizeof(int)).Clear();
        }
    }

    private static void ValidateBindRequest(byte[] request) {
        var buffer = new NdrBuffer(request, 0) { Length = request.Length };
        var pdu = new BindPdu();
        pdu.Decode(new NdrCodec(), buffer);
        if (pdu.ContextList.Length != 1 || !string.Equals(
            WinregSyntax,
            pdu.ContextList[0].AbstractSyntax.ToString(),
            StringComparison.OrdinalIgnoreCase)) {
            throw new InvalidOperationException("RegistryStub did not bind to the WINREG syntax.");
        }
    }

    private sealed class ReplayTransportFactory : TransportFactory {
        private readonly MockWinregServer _server;

        public ReplayTransportFactory(MockWinregServer server) => _server = server;

        public override ITransport CreateTransport(string address, PropertyBag properties) =>
            new ReplayTransport(_server, properties);
    }

    private sealed class ReplayTransport : ITransport {
        private readonly MockWinregServer _server;

        public ReplayTransport(MockWinregServer server, PropertyBag properties) {
            _server = server;
            Properties = properties;
        }

        public string Protocol => "mock-winreg";

        public PropertyBag Properties { get; }

        public IEndpoint Attach(PresentationSyntax syntax) {
            if (!string.Equals(WinregSyntax, syntax.ToString(), StringComparison.OrdinalIgnoreCase)) {
                throw new InvalidOperationException("Unexpected presentation syntax.");
            }

            return new ConnectionOrientedEndpoint(this, syntax);
        }

        public void Close() {
        }

        public void Receive(NdrBuffer buffer) {
            var response = _server.DequeueResponse();
            buffer.WriteOctetArray(response, 0, response.Length);
        }

        public void Send(NdrBuffer buffer) {
            var request = new byte[buffer.Length];
            System.Buffer.BlockCopy(buffer.Buf, 0, request, 0, request.Length);
            _server.RecordRequest(request);
        }
    }
}
