// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Classification and wire conversion helpers for <see cref="OpcDaGroupEnumerationScope"/>.
/// </summary>
public static class OpcDaGroupEnumerationScopeExtensions
{
    /// <summary>Converts an OPC DA wire value to a validated scope.</summary>
    public static OpcDaGroupEnumerationScope FromWireValue(int value)
    {
        var scope = (OpcDaGroupEnumerationScope)value;
        scope.Validate();
        return scope;
    }

    /// <summary>Returns whether the scope produces group connections.</summary>
    public static bool IsConnectionScope(this OpcDaGroupEnumerationScope scope) =>
        scope switch
        {
            OpcDaGroupEnumerationScope.PrivateConnections or
            OpcDaGroupEnumerationScope.PublicConnections or
            OpcDaGroupEnumerationScope.AllConnections => true,
            OpcDaGroupEnumerationScope.Private or
            OpcDaGroupEnumerationScope.Public or
            OpcDaGroupEnumerationScope.All => false,
            _ => throw InvalidScope(scope),
        };

    /// <summary>Returns whether the scope produces group names.</summary>
    public static bool IsNameScope(this OpcDaGroupEnumerationScope scope) =>
        scope switch
        {
            OpcDaGroupEnumerationScope.PrivateConnections or
            OpcDaGroupEnumerationScope.PublicConnections or
            OpcDaGroupEnumerationScope.AllConnections => false,
            OpcDaGroupEnumerationScope.Private or
            OpcDaGroupEnumerationScope.Public or
            OpcDaGroupEnumerationScope.All => true,
            _ => throw InvalidScope(scope),
        };

    /// <summary>Returns whether private groups are included.</summary>
    public static bool IncludesPrivateGroups(this OpcDaGroupEnumerationScope scope) =>
        scope switch
        {
            OpcDaGroupEnumerationScope.PrivateConnections or
            OpcDaGroupEnumerationScope.AllConnections or
            OpcDaGroupEnumerationScope.Private or
            OpcDaGroupEnumerationScope.All => true,
            OpcDaGroupEnumerationScope.PublicConnections or
            OpcDaGroupEnumerationScope.Public => false,
            _ => throw InvalidScope(scope),
        };

    /// <summary>Returns whether public groups are included.</summary>
    public static bool IncludesPublicGroups(this OpcDaGroupEnumerationScope scope) =>
        scope switch
        {
            OpcDaGroupEnumerationScope.PublicConnections or
            OpcDaGroupEnumerationScope.AllConnections or
            OpcDaGroupEnumerationScope.Public or
            OpcDaGroupEnumerationScope.All => true,
            OpcDaGroupEnumerationScope.PrivateConnections or
            OpcDaGroupEnumerationScope.Private => false,
            _ => throw InvalidScope(scope),
        };

    /// <summary>Throws when the value is not one of the six OPC DA scopes.</summary>
    public static void Validate(this OpcDaGroupEnumerationScope scope)
    {
        if (scope is < OpcDaGroupEnumerationScope.PrivateConnections or > OpcDaGroupEnumerationScope.All)
        {
            throw InvalidScope(scope);
        }
    }

    private static ArgumentOutOfRangeException InvalidScope(OpcDaGroupEnumerationScope scope) =>
        new(nameof(scope), scope, "The OPC DA group enumeration scope must be a value from 1 through 6.");
}
