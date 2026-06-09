//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

#pragma warning disable MA0048 // OPC DX enums are grouped by specification section.
#pragma warning disable MA0062 // OpcDxMask_All intentionally preserves the IDL value.

namespace Opc.Classic.Dx;

/// <summary>OPC DX 1.00 source-server interface type identifiers.</summary>
public enum DxServerType {
    /// <summary>The source-server type is not known.</summary>
    Unknown = 0,

    /// <summary>COM Data Access 1.0.</summary>
    ComDa10 = 1,

    /// <summary>COM Data Access 2.04.</summary>
    ComDa204 = 2,

    /// <summary>COM Data Access 2.05.</summary>
    ComDa205 = 3,

    /// <summary>COM Data Access 3.0.</summary>
    ComDa30 = 4,

    /// <summary>XML-DA 1.0.</summary>
    XmlDa10 = 5,
}

/// <summary>OPC DX 1.00 server state values from §4.2.2.</summary>
public enum DxServerState {
    /// <summary>The server state is not known.</summary>
    Unknown = 0,

    /// <summary>The server is running normally.</summary>
    Running = 1,

    /// <summary>The server has detected a fatal internal error.</summary>
    Failed = 2,

    /// <summary>The server is running without loaded configuration.</summary>
    NoConfig = 3,

    /// <summary>The server is temporarily suspended.</summary>
    Suspended = 4,

    /// <summary>The server has shut down.</summary>
    Shutdown = 5,

    /// <summary>The server is in test mode.</summary>
    Test = 6,

    /// <summary>The server has a communication fault.</summary>
    CommFault = 7,
}

/// <summary>OPC DX 1.00 connection runtime state values from §4.3.2.19.1.</summary>
public enum DxConnectionState {
    /// <summary>The connection state is not known.</summary>
    Unknown = 0,

    /// <summary>The connection is initializing.</summary>
    Initializing = 1,

    /// <summary>The connection is operational.</summary>
    Operational = 2,

    /// <summary>The connection has been deactivated.</summary>
    Deactivated = 3,

    /// <summary>The source server is not connected.</summary>
    SourceServerNotConnected = 4,

    /// <summary>The source subscription failed.</summary>
    SubscriptionFailed = 5,

    /// <summary>The target item was not found.</summary>
    TargetItemNotFound = 6,
}

/// <summary>OPC DX 1.00 source-server connection status values from §4.4.1.6.1.</summary>
public enum DxConnectStatus {
    /// <summary>The connection status is not known.</summary>
    Unknown = 0,

    /// <summary>The source server is connected.</summary>
    Connected = 1,

    /// <summary>The source server is disconnected.</summary>
    Disconnected = 2,

    /// <summary>The source server is connecting.</summary>
    Connecting = 3,

    /// <summary>The source server connection failed.</summary>
    Failed = 4,
}

/// <summary>OPC DX quality status names from §4.3.2.19.4.</summary>
public enum DxQualityStatus {
    /// <summary>The quality status is not known.</summary>
    Unknown = 0,

    /// <summary>Bad quality.</summary>
    Bad = 1,

    /// <summary>Bad configuration error.</summary>
    BadConfigurationError = 2,

    /// <summary>Bad not connected.</summary>
    BadNotConnected = 3,

    /// <summary>Bad device failure.</summary>
    BadDeviceFailure = 4,

    /// <summary>Bad sensor failure.</summary>
    BadSensorFailure = 5,

    /// <summary>Bad last known value.</summary>
    BadLastKnownValue = 6,

    /// <summary>Bad communication failure.</summary>
    BadCommFailure = 7,

    /// <summary>Bad out of service.</summary>
    BadOutOfService = 8,

    /// <summary>Uncertain quality.</summary>
    Uncertain = 9,

    /// <summary>Uncertain last usable value.</summary>
    UncertainLastUsableValue = 10,

    /// <summary>Uncertain sensor not accurate.</summary>
    UncertainSensorNotAccurate = 11,

    /// <summary>Uncertain engineering units exceeded.</summary>
    UncertainEuExceeded = 12,

    /// <summary>Uncertain sub-normal.</summary>
    UncertainSubNormal = 13,

    /// <summary>Good quality.</summary>
    Good = 14,

    /// <summary>Good local override.</summary>
    GoodLocalOverride = 15,
}

/// <summary>OPC DX quality limit bit names from §4.3.2.19.4.</summary>
public enum DxLimitStatus {
    /// <summary>No limit condition.</summary>
    None = 0,

    /// <summary>Low limit.</summary>
    Low = 1,

    /// <summary>High limit.</summary>
    High = 2,

    /// <summary>Constant limit.</summary>
    Constant = 3,
}

/// <summary>OPC DX <c>OpcDxMask</c> optional-field presence bits.</summary>
[Flags]
public enum DxMask {
    /// <summary>No optional fields are present.</summary>
    None = 0x0,

    /// <summary><c>ItemPath</c> is present.</summary>
    ItemPath = 0x1,

    /// <summary><c>ItemName</c> is present.</summary>
    ItemName = 0x2,

    /// <summary><c>Version</c> is present.</summary>
    Version = 0x4,

    /// <summary><c>BrowsePath</c> is present.</summary>
    BrowsePaths = 0x8,

    /// <summary><c>Name</c> is present.</summary>
    Name = 0x10,

    /// <summary><c>Description</c> is present.</summary>
    Description = 0x20,

    /// <summary><c>Keyword</c> is present.</summary>
    Keyword = 0x40,

    /// <summary><c>DefaultSourceItemConnected</c> is present.</summary>
    DefaultSourceItemConnected = 0x80,

    /// <summary><c>DefaultTargetItemConnected</c> is present.</summary>
    DefaultTargetItemConnected = 0x100,

    /// <summary><c>DefaultOverridden</c> is present.</summary>
    DefaultOverridden = 0x200,

    /// <summary><c>DefaultOverrideValue</c> is present.</summary>
    DefaultOverrideValue = 0x400,

    /// <summary><c>SubstituteValue</c> is present.</summary>
    SubstituteValue = 0x800,

    /// <summary><c>EnableSubstituteValue</c> is present.</summary>
    EnableSubstituteValue = 0x1000,

    /// <summary><c>TargetItemPath</c> is present.</summary>
    TargetItemPath = 0x2000,

    /// <summary><c>TargetItemName</c> is present.</summary>
    TargetItemName = 0x4000,

    /// <summary><c>SourceServerName</c> is present.</summary>
    SourceServerName = 0x8000,

    /// <summary><c>SourceItemPath</c> is present.</summary>
    SourceItemPath = 0x10000,

    /// <summary><c>SourceItemName</c> is present.</summary>
    SourceItemName = 0x20000,

    /// <summary><c>SourceItemQueueSize</c> is present.</summary>
    SourceItemQueueSize = 0x40000,

    /// <summary><c>UpdateRate</c> is present.</summary>
    UpdateRate = 0x80000,

    /// <summary><c>DeadBand</c> is present.</summary>
    DeadBand = 0x100000,

    /// <summary><c>VendorData</c> is present.</summary>
    VendorData = 0x200000,

    /// <summary><c>ServerType</c> is present.</summary>
    ServerType = 0x400000,

    /// <summary><c>ServerURL</c> is present.</summary>
    ServerUrl = 0x800000,

    /// <summary><c>DefaultSourceServerConnected</c> is present.</summary>
    DefaultSourceServerConnected = 0x1000000,

    /// <summary>All OPC DX mask bits.</summary>
    All = 0x7FFFFFFF,
}
