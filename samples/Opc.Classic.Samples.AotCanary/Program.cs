// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Microsoft.Win32;
using Opc.Classic;
using Opc.Classic.Ae;
using Opc.Classic.Ae.Ndr;
using Opc.Classic.Batch;
using Opc.Classic.Batch.Ndr;
using Opc.Classic.Commands;
using Opc.Classic.Commands.Dcom;
using Opc.Classic.Cpx;
using Opc.Classic.Da;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Kerberos.Spnego;
using Opc.Classic.Dcom.Smb;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Discovery;
using Opc.Classic.Dx;
using Opc.Classic.Dx.Ndr;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Ndr;
using Opc.Classic.Hosting;
using Opc.Classic.Hosting.Windows;
using Opc.Classic.Ndr;
using Opc.Classic.Security;
using Opc.Classic.Xml;
using Opc.Classic.Xml.Serialization;
using Opc.Classic.Windows;

var roots = new List<string>();
var fixedTime = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);

roots.Add(
    $"Meta:{typeof(OpcClassicSdk).FullName}:{OpcClassicSdk.PackageId}:"
    + $"{typeof(OpcClassicWindows).FullName}:{OpcClassicWindows.PackageId}");

var url = OpcUrl.Parse("opcda://localhost/Matrikon.OPC.Simulation.1");
var variant = OpcVariant.FromInt32(42);
roots.Add($"Core:{url.Scheme}:{variant.AsInt32()}");

var itemState = new OpcItemState(7, fixedTime, OpcQuality.Good, OpcVariant.FromDouble(3.14));
byte[] daBuffer = new byte[256];
var daWriter = new NdrWriter(daBuffer);
NdrOpcItemStateCodec.Write(ref daWriter, itemState);
var daReader = new NdrReader(daBuffer.AsSpan(0, daWriter.Position));
roots.Add($"DA:{NdrOpcItemStateCodec.Read(ref daReader).ClientHandle}");

var aeEvent = new OpcEventNotification(
    changeMask: 1,
    newState: 2,
    source: "Canary.Source",
    time: fixedTime,
    message: "Canary event",
    eventType: 4,
    eventCategory: 5,
    severity: 6,
    conditionName: "Condition",
    subconditionName: "Subcondition",
    quality: OpcQuality.Good,
    ackRequired: true,
    activeTime: fixedTime,
    cookie: 7,
    eventAttributes: [OpcVariant.FromBoolean(true)],
    actorId: "canary");
byte[] aeBuffer = new byte[2048];
var aeWriter = new NdrWriter(aeBuffer);
NdrOpcEventNotificationCodec.Write(ref aeWriter, aeEvent);
var aeReader = new NdrReader(aeBuffer.AsSpan(0, aeWriter.Position));
roots.Add($"AE:{NdrOpcEventNotificationCodec.Read(ref aeReader).Cookie}");

var hdaTime = OpcHdaTime.FromTimestamp(fixedTime);
byte[] hdaBuffer = new byte[128];
var hdaWriter = new NdrWriter(hdaBuffer);
NdrOpcHdaTimeCodec.Write(ref hdaWriter, hdaTime);
var hdaReader = new NdrReader(hdaBuffer.AsSpan(0, hdaWriter.Position));
roots.Add($"HDA:{NdrOpcHdaTimeCodec.Read(ref hdaReader).Timestamp:O}");

var batchFilter = new OpcBatchSummaryFilter(
    null, null, null, null, 0, 100, null, null, null,
    fixedTime, fixedTime, fixedTime, fixedTime);
byte[] batchBuffer = new byte[256];
var batchWriter = new NdrWriter(batchBuffer);
NdrOpcBatchSummaryFilterCodec.Write(ref batchWriter, batchFilter);
var batchReader = new NdrReader(batchBuffer.AsSpan(0, batchWriter.Position));
roots.Add($"Batch:{NdrOpcBatchSummaryFilterCodec.Read(ref batchReader).MaxBatchSize}");

var invocation = new CommandInvocation(
    Guid.Parse("11111111-2222-3333-4444-555555555555"),
    Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
    "Canary",
    CommandState.Complete,
    0,
    fixedTime);
roots.Add($"Commands:{invocation.State}:{OpcCommandsSpecCatalog.Commands.Count}");

var structType = new StructType { Name = "Counter" };
var complexValue = new ComplexValue
{
    Type = structType,
    Fields = new Dictionary<string, object?> { ["Value"] = 42 },
};
var typeDescription = new TypeDescription(
    "Counter",
    "canary:counter",
    TypeKind.StructReference,
    isComplex: true,
    [new TypeField("Value", TypeKind.Int32)]);
byte[] cpxBytes = OpcBinaryEncoder.Encode(complexValue, typeDescription);
ComplexValue decodedComplexValue = OpcBinaryDecoder.Decode(cpxBytes, typeDescription);
roots.Add($"Cpx:{decodedComplexValue.TryGet<int>("Value", out int cpxValue)}:{cpxValue}");

var dxItem = DxItemIdentifier.FromName("Canary.Item", "1");
byte[] dxBuffer = new byte[256];
var dxWriter = new NdrWriter(dxBuffer);
NdrOpcDxItemIdentifierCodec.Write(ref dxWriter, dxItem);
var dxReader = new NdrReader(dxBuffer.AsSpan(0, dxWriter.Position));
roots.Add($"DX:{NdrOpcDxItemIdentifierCodec.Read(ref dxReader).ItemName}");

var descriptor = new OpcServerDescriptor(
    Guid.Parse("01234567-89ab-cdef-0123-456789abcdef"),
    "Opc.Classic.Canary.1",
    "AOT Canary",
    "Opc.Classic.Canary",
    []);
roots.Add($"Discovery:{descriptor.ProgId}:{OpcDiscoverySpecCatalog.Discovery.Count}");

var logon = new OpcLogonRequest("canary", "");
roots.Add($"Security:{logon.UserId.Length}");

using (var xmlStream = new MemoryStream())
{
    using var envelopeWriter = new SoapEnvelopeWriter(xmlStream);
    ReadSerializer.WriteRequest(
        envelopeWriter,
        new XmlDaReadRequest(
            new XmlDaRequestHeader("en-US", "canary"),
            [new XmlDaReadItem("Canary.Item", "1")]));
    roots.Add($"XML:{xmlStream.Length}");
}

var endpoint = ListenAddressParser.Parse("127.0.0.1:0");
byte[] targetInfo = NtlmAvPairs.AddMicFlag([0, 0, 0, 0]);
roots.Add($"DCOM:{endpoint.Address}:{targetInfo.Length}");

var spnegoToken = SpnegoEncoder.EncodeNegTokenInit(
    new SpnegoNegTokenInit([SpnegoOids.KerberosV5], new byte[] { 1, 2, 3 }, null));
var decodedSpnego = SpnegoDecoder.DecodeNegTokenInit(spnegoToken);
roots.Add($"Kerberos:{decodedSpnego.MechTypes.Count}:{spnegoToken.Length}");

var smbHeader = new Smb2PacketHeader(
    CreditCharge: 1,
    Status: 0,
    Command: Smb2Command.Negotiate,
    CreditRequestResponse: 1,
    Flags: 0,
    NextCommand: 0,
    MessageId: 1,
    ProcessId: 2,
    TreeId: 3,
    SessionId: 4,
    Signature: ReadOnlyMemory<byte>.Empty);
Span<byte> smbBuffer = stackalloc byte[64];
smbHeader.Write(smbBuffer);
roots.Add($"SMB:{Smb2PacketHeader.Read(smbBuffer).Command}");

var registration = new OpcClsidRegistration(
    Guid.Parse("fedcba98-7654-3210-fedc-ba9876543210"),
    "Opc.Classic.Canary.1",
    "Opc.Classic.Samples.AotCanary",
    "CanaryServer",
    "AOT Canary");
var registry = new InMemoryClsidRegistry([registration]);
roots.Add($"Hosting:{registry.TryResolve(registration.Clsid, out var resolved)}:{resolved.ProgId}");

if (OperatingSystem.IsWindows())
{
    Action<
        OpcClsidRegistration,
        string,
        RegistryHive,
        IReadOnlyList<RegistryView>?,
        IReadOnlyList<OpcComponentCategory>?> registerLocalServer = WindowsComRegistration.RegisterLocalServer;
    GC.KeepAlive(registerLocalServer);
    roots.Add($"WindowsHosting:{nameof(WindowsComRegistration.RegisterLocalServer)}");
}

foreach (string root in roots)
{
    Console.WriteLine(root);
}
Console.WriteLine("AOT canary OK");
return 0;
