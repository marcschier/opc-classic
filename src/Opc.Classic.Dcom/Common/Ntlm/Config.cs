// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;

namespace Opc.Classic.Dcom.Common.Ntlm;

public static class Config
{
    private static readonly Dictionary<string, string> Properties = new(StringComparer.OrdinalIgnoreCase);

    public static string? GetProperty(string key) =>
        Properties.TryGetValue(key, out var value) ? value : Environment.GetEnvironmentVariable(ToEnvironmentName(key));

    public static void SetProperty(string key, string? value)
    {
        if (value is null)
        {
            Properties.Remove(key);
        }
        else
        {
            Properties[key] = value;
        }
    }

    public static bool GetBoolean(string key, bool defaultValue) =>
        bool.TryParse(GetProperty(key), out var value) ? value : defaultValue;

    private static string ToEnvironmentName(string key) => key.Replace('.', '_').Replace('-', '_').ToUpperInvariant();
}
