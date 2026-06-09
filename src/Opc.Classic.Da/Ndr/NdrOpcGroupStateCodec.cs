//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Ndr;

/// <summary>
/// NDR encoder / decoder for the managed <see cref="OpcGroupState" /> return shape of
/// <c>IOPCGroupStateMgt::GetState</c>.
/// </summary>
/// <remarks>
/// Wire order follows the IDL out parameters: update rate, active flag, name,
/// time bias, deadband, LCID, client handle, then server handle.
/// </remarks>
public static class NdrOpcGroupStateCodec {
    /// <summary>Encodes a group-state response payload.</summary>
    public static void Write(ref NdrWriter writer, OpcGroupState state) {
        ArgumentNullException.ThrowIfNull(state);

        writer.WriteUInt32(unchecked((uint)state.UpdateRate));
        writer.WriteInt32(state.Active ? -1 : 0);
        writer.WriteUnicodeStringPtr(state.Name);
        writer.WriteInt32(state.TimeBias);
        writer.WriteSingle(state.PercentDeadband);
        writer.WriteUInt32(unchecked((uint)state.LocaleId));
        writer.WriteUInt32(unchecked((uint)state.ClientHandle));
        writer.WriteUInt32(unchecked((uint)state.ServerHandle));
    }

    /// <summary>Decodes a group-state response payload.</summary>
    public static OpcGroupState Read(ref NdrReader reader) {
        uint updateRate = reader.ReadUInt32();
        bool active = reader.ReadInt32() != 0;
        string? name = reader.ReadUnicodeStringPtr();
        int timeBias = reader.ReadInt32();
        float percentDeadband = reader.ReadSingle();
        uint localeId = reader.ReadUInt32();
        uint clientHandle = reader.ReadUInt32();
        uint serverHandle = reader.ReadUInt32();

        return new OpcGroupState(
            ClientHandle: unchecked((int)clientHandle),
            ServerHandle: unchecked((int)serverHandle),
            Name: name,
            Active: active,
            UpdateRate: unchecked((int)updateRate),
            TimeBias: timeBias,
            PercentDeadband: percentDeadband,
            LocaleId: unchecked((int)localeId));
    }
}
