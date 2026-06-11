//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Globalization;
using Opc.Classic.Dcom.Internal;

namespace Opc.Classic.Dcom.Rpc;

internal static class RpcTransportQuotas
{
    public const int DefaultMaxNdrPayloadSize = 16 * 1024 * 1024;
    public const int DefaultMaxNtlmMessageSize = 64 * 1024 - 1;
    public const int DefaultMaxSmb2MessageSize = 0x1FFFF;

    public const string MaxNdrPayloadSizeProperty = "rpc.maxNdrPayloadSize";
    public const string MaxNtlmMessageSizeProperty = "rpc.maxNtlmMessageSize";
    public const string MaxSmb2MessageSizeProperty = "rpc.maxSmb2MessageSize";

    public static int GetInt32(PropertyBag? properties, string propertyName, int defaultValue, int maximumValue)
    {
        object? value = properties?.GetProperty(propertyName);
        int parsed = value switch
        {
            null => defaultValue,
            int integer => integer,
            string text => int.Parse(text, CultureInfo.InvariantCulture),
            _ => Convert.ToInt32(value, CultureInfo.InvariantCulture),
        };

        if (parsed <= 0 || parsed > maximumValue)
        {
            throw new ArgumentOutOfRangeException(
                propertyName,
                parsed,
                $"Quota {propertyName} must be 1..{maximumValue} bytes.");
        }

        return parsed;
    }
}
