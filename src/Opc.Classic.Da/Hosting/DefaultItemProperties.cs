// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Da.Dcom;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Default managed implementation of <see cref="IOPCItemProperties"/>
/// (DA 2.x) publishing the OPC-standard property set (IDs 1-8) defined in
/// <see cref="OpcStandardProperties"/>. Property values are sourced from
/// an injected <see cref="IOpcItemPropertyProvider"/> so server authors can
/// customize per-item behaviour; the default provider returns
/// <c>OPC_E_INVALID_PID</c> for every requested property value.
/// </summary>
public sealed class DefaultItemProperties : IOPCItemProperties
{
    private readonly IOpcItemPropertyProvider _provider;

    /// <summary>
    /// Initializes with the no-op property provider.
    /// </summary>
    public DefaultItemProperties()
        : this(NullItemPropertyProvider.Instance)
    {
    }

    /// <summary>
    /// Initializes with the supplied provider.
    /// </summary>
    public DefaultItemProperties(IOpcItemPropertyProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <inheritdoc />
    public Task QueryAvailablePropertiesAsync(
        string itemId,
        out int[] propertyIds,
        out string[] descriptions,
        out ushort[] dataTypes,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(itemId);
        cancellationToken.ThrowIfCancellationRequested();
        var properties = new List<OpcStandardProperty>(OpcStandardProperties.All);
        if (_provider is IOpcItemPropertyMetadataProvider metadataProvider)
        {
            var seen = new HashSet<int>();
            foreach (var property in properties)
            {
                seen.Add(property.Id);
            }

            foreach (var property in metadataProvider.GetAvailableProperties(itemId))
            {
                if (seen.Add(property.Id))
                {
                    properties.Add(property);
                }
            }
        }

        propertyIds = new int[properties.Count];
        descriptions = new string[properties.Count];
        dataTypes = new ushort[properties.Count];
        for (int i = 0; i < properties.Count; i++)
        {
            propertyIds[i] = properties[i].Id;
            descriptions[i] = properties[i].Description;
            dataTypes[i] = (ushort)properties[i].DataType;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task GetItemPropertiesAsync(
        string itemId,
        int[] propertyIds,
        out OpcVariant[] data,
        out int[] errors,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(itemId);
        ArgumentNullException.ThrowIfNull(propertyIds);
        cancellationToken.ThrowIfCancellationRequested();
        data = new OpcVariant[propertyIds.Length];
        errors = new int[propertyIds.Length];
        for (int i = 0; i < propertyIds.Length; i++)
        {
            (OpcVariant value, int error) = _provider.TryGetPropertyValue(itemId, propertyIds[i]);
            data[i] = value;
            errors[i] = error;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task LookupItemIdsAsync(
        string itemId,
        int[] propertyIds,
        out string[] newItemIds,
        out int[] errors,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(itemId);
        ArgumentNullException.ThrowIfNull(propertyIds);
        cancellationToken.ThrowIfCancellationRequested();
        newItemIds = new string[propertyIds.Length];
        errors = new int[propertyIds.Length];
        var metadataProvider = _provider as IOpcItemPropertyMetadataProvider;
        for (int i = 0; i < propertyIds.Length; i++)
        {
            if (metadataProvider is not null)
            {
                (string resolvedItemId, int error) = metadataProvider.TryGetPropertyItemId(itemId, propertyIds[i]);
                newItemIds[i] = resolvedItemId;
                errors[i] = error;
                continue;
            }

            newItemIds[i] = string.Empty;
            errors[i] = OpcResultId.InvalidPid.Code;
        }
        return Task.CompletedTask;
    }
}
