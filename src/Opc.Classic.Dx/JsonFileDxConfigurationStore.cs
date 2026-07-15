// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable MA0048 // Internal JSON persistence metadata is colocated with its store.
#pragma warning disable MA0051 // Explicit variant type switches mirror the finite persistence format.

namespace Opc.Classic.Dx;

/// <summary>
/// Persists DX configuration in an atomically replaced, versioned JSON file.
/// </summary>
public sealed class JsonFileDxConfigurationStore : IDxConfigurationStore, IDisposable
{
    private const int CurrentFormatVersion = 1;
    private static readonly TimeSpan LockRetryDelay = TimeSpan.FromMilliseconds(20);
    private static readonly DxConfigurationJsonContext JsonContext = CreateJsonContext();
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly string _filePath;
    private readonly string _lockFilePath;

    /// <summary>
    /// Creates a store for <paramref name="filePath"/>.
    /// </summary>
    public JsonFileDxConfigurationStore(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = Path.GetFullPath(filePath);
        _lockFilePath = _filePath + ".lock";
    }

    /// <summary>
    /// Full path of the persisted JSON document.
    /// </summary>
    public string FilePath => _filePath;

    /// <inheritdoc />
    public async ValueTask<DxConfigurationSnapshot> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await LoadCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask<DxConfigurationSnapshot> SaveAsync(
        DxConfiguration configuration,
        long expectedVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedVersion);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fileLock = await AcquireFileLockAsync(cancellationToken).ConfigureAwait(false);
            var current = await LoadCoreAsync(cancellationToken).ConfigureAwait(false);
            if (current.Version != expectedVersion)
            {
                throw new DxConfigurationVersionException(expectedVersion, current.Version);
            }

            var next = new DxConfigurationSnapshot(
                checked(expectedVersion + 1),
                configuration.Copy());
            await SaveCoreAsync(next, cancellationToken).ConfigureAwait(false);
            return InMemoryDxConfigurationStore.CloneSnapshot(next);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose() => _gate.Dispose();

    private async ValueTask<DxConfigurationSnapshot> LoadCoreAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!File.Exists(_filePath))
        {
            return DxConfigurationSnapshot.Empty;
        }

        try
        {
            using var stream = new FileStream(
                _filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var model = await JsonSerializer.DeserializeAsync(
                stream,
                JsonContext.DxConfigurationFileModel,
                cancellationToken).ConfigureAwait(false);
            if (model is null)
            {
                throw new DxConfigurationCorruptException(
                    $"DX configuration file '{_filePath}' is empty.");
            }

            if (model.FormatVersion != CurrentFormatVersion)
            {
                throw new DxConfigurationFormatVersionException(
                    model.FormatVersion,
                    CurrentFormatVersion);
            }

            if (model.Version < 0 || model.Configuration is null)
            {
                throw new DxConfigurationCorruptException(
                    $"DX configuration file '{_filePath}' contains invalid version or configuration data.");
            }

            return new(model.Version, model.Configuration.Copy());
        }
        catch (JsonException exception)
        {
            throw new DxConfigurationCorruptException(
                $"DX configuration file '{_filePath}' is not valid JSON configuration.",
                exception);
        }
    }

    private async ValueTask SaveCoreAsync(
        DxConfigurationSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        var temporaryPath = _filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            var model = new DxConfigurationFileModel
            {
                FormatVersion = CurrentFormatVersion,
                Version = snapshot.Version,
                Configuration = snapshot.Configuration,
            };

            using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    model,
                    JsonContext.DxConfigurationFileModel,
                    cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporaryPath, _filePath, overwrite: true);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }

    private async ValueTask<FileStream> AcquireFileLockAsync(
        CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                return new FileStream(
                    _lockFilePath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    bufferSize: 1,
                    FileOptions.Asynchronous);
            }
            catch (IOException)
            {
                await Task.Delay(LockRetryDelay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static DxConfigurationJsonContext CreateJsonContext()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };
        options.Converters.Add(new OpcVariantJsonConverter());
        return new DxConfigurationJsonContext(options);
    }
}

internal sealed class DxConfigurationFileModel
{
    public int FormatVersion { get; set; }

    public long Version { get; set; }

    public DxConfiguration? Configuration { get; set; }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true)]
[JsonSerializable(typeof(DxConfigurationFileModel))]
internal sealed partial class DxConfigurationJsonContext : JsonSerializerContext;

internal sealed class OpcVariantJsonConverter : JsonConverter<OpcVariant>
{
    public override OpcVariant Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        try
        {
            using var document = JsonDocument.ParseValue(ref reader);
            return ReadVariant(document.RootElement);
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is ArgumentException or
            FormatException or
            InvalidCastException or
            OverflowException)
        {
            throw new JsonException(
                "OPC variant contains invalid persisted data.",
                exception);
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        OpcVariant value,
        JsonSerializerOptions options)
        => WriteVariant(writer, value);

    private static void WriteVariant(Utf8JsonWriter writer, OpcVariant value)
    {
        writer.WriteStartObject();
        writer.WriteNumber("type", (ushort)value.Type);
        if (value.Type is not VarType.VT_EMPTY and not VarType.VT_NULL)
        {
            writer.WritePropertyName("value");
            WriteValue(writer, value);
        }
        writer.WriteEndObject();
    }

    private static OpcVariant ReadVariant(JsonElement element)
    {
        if (!element.TryGetProperty("type", out var typeProperty))
        {
            throw new JsonException("An OPC variant must contain a type.");
        }

        var type = (VarType)typeProperty.GetUInt16();
        if (type is VarType.VT_EMPTY)
        {
            return OpcVariant.Empty;
        }
        if (type is VarType.VT_NULL)
        {
            return OpcVariant.Null;
        }
        if (!element.TryGetProperty("value", out var value))
        {
            throw new JsonException($"OPC variant {type} must contain a value.");
        }

        if (VarTypeMask.IsByRef(type) || VarTypeMask.IsVector(type))
        {
            throw new JsonException(
                $"OPC variant modifier in {type} is not persistable in DX configuration.");
        }
        if (VarTypeMask.IsArray(type))
        {
            return ReadSafeArray(type, value);
        }

        return type switch
        {
            VarType.VT_I1 => OpcVariant.FromInt8(value.GetSByte()),
            VarType.VT_UI1 => OpcVariant.FromUInt8(value.GetByte()),
            VarType.VT_I2 => OpcVariant.FromInt16(value.GetInt16()),
            VarType.VT_UI2 => OpcVariant.FromUInt16(value.GetUInt16()),
            VarType.VT_I4 => OpcVariant.FromInt32(value.GetInt32()),
            VarType.VT_UI4 => OpcVariant.FromUInt32(value.GetUInt32()),
            VarType.VT_I8 => OpcVariant.FromInt64(value.GetInt64()),
            VarType.VT_UI8 => OpcVariant.FromUInt64(value.GetUInt64()),
            VarType.VT_R4 => OpcVariant.FromSingle(ReadSingle(value)),
            VarType.VT_R8 => OpcVariant.FromDouble(ReadDouble(value)),
            VarType.VT_BSTR => new OpcVariant(
                VarType.VT_BSTR,
                value.ValueKind == JsonValueKind.Null ? null : value.GetString()),
            VarType.VT_BOOL => OpcVariant.FromBoolean(value.GetBoolean()),
            VarType.VT_DATE => OpcVariant.FromDate(value.GetDateTime()),
            VarType.VT_FILETIME => OpcVariant.FromFileTime(value.GetInt64()),
            VarType.VT_ERROR => OpcVariant.FromError(value.GetInt32()),
            VarType.VT_CLSID => OpcVariant.FromClsid(value.GetGuid()),
            VarType.VT_VARIANT => OpcVariant.FromVariant(ReadVariant(value)),
            VarType.VT_RECORD => OpcVariant.FromRecord(ReadRecord(value)),
            VarType.VT_CY => new OpcVariant(type, value.GetInt64()),
            VarType.VT_DECIMAL => new OpcVariant(type, value.GetDecimal()),
            VarType.VT_INT or VarType.VT_HRESULT => new OpcVariant(type, value.GetInt32()),
            VarType.VT_UINT => new OpcVariant(type, value.GetUInt32()),
            _ => throw new JsonException(
                $"OPC variant type {type} is not supported in DX JSON configuration."),
        };
    }

    private static void WriteValue(Utf8JsonWriter writer, OpcVariant value)
    {
        if (VarTypeMask.IsByRef(value.Type) || VarTypeMask.IsVector(value.Type))
        {
            throw new JsonException(
                $"OPC variant modifier in {value.Type} is not persistable in DX configuration.");
        }
        if (VarTypeMask.IsArray(value.Type))
        {
            WriteSafeArray(writer, value);
            return;
        }

        switch (value.Type)
        {
            case VarType.VT_I1:
            case VarType.VT_UI1:
            case VarType.VT_I2:
            case VarType.VT_UI2:
            case VarType.VT_I4:
            case VarType.VT_ERROR:
            case VarType.VT_INT:
            case VarType.VT_HRESULT:
            case VarType.VT_UI4:
            case VarType.VT_UINT:
            case VarType.VT_I8:
            case VarType.VT_CY:
            case VarType.VT_FILETIME:
            case VarType.VT_UI8:
                WriteIntegerValue(writer, value);
                break;
            case VarType.VT_R4:
                WriteSingle(writer, (float)value.Boxed!);
                break;
            case VarType.VT_R8:
                WriteDouble(writer, (double)value.Boxed!);
                break;
            case VarType.VT_DECIMAL:
                writer.WriteNumberValue((decimal)value.Boxed!);
                break;
            case VarType.VT_BSTR:
                writer.WriteStringValue((string?)value.Boxed);
                break;
            case VarType.VT_BOOL:
                writer.WriteBooleanValue((bool)value.Boxed!);
                break;
            case VarType.VT_DATE:
                writer.WriteStringValue((DateTime)value.Boxed!);
                break;
            case VarType.VT_CLSID:
                writer.WriteStringValue((Guid)value.Boxed!);
                break;
            case VarType.VT_VARIANT:
                WriteNestedVariant(writer, value);
                break;
            case VarType.VT_RECORD:
                WriteRecord(writer, value);
                break;
            default:
                throw new JsonException(
                    $"OPC variant type {value.Type} is not supported in DX JSON configuration.");
        }
    }

    private static void WriteIntegerValue(Utf8JsonWriter writer, OpcVariant value)
    {
        switch (value.Type)
        {
            case VarType.VT_I1:
                writer.WriteNumberValue((sbyte)value.Boxed!);
                break;
            case VarType.VT_UI1:
                writer.WriteNumberValue((byte)value.Boxed!);
                break;
            case VarType.VT_I2:
                writer.WriteNumberValue((short)value.Boxed!);
                break;
            case VarType.VT_UI2:
                writer.WriteNumberValue((ushort)value.Boxed!);
                break;
            case VarType.VT_I4:
            case VarType.VT_ERROR:
            case VarType.VT_INT:
            case VarType.VT_HRESULT:
                writer.WriteNumberValue((int)value.Boxed!);
                break;
            case VarType.VT_UI4:
            case VarType.VT_UINT:
                writer.WriteNumberValue((uint)value.Boxed!);
                break;
            case VarType.VT_I8:
            case VarType.VT_CY:
            case VarType.VT_FILETIME:
                writer.WriteNumberValue((long)value.Boxed!);
                break;
            case VarType.VT_UI8:
                writer.WriteNumberValue((ulong)value.Boxed!);
                break;
        }
    }

    private static void WriteNestedVariant(Utf8JsonWriter writer, OpcVariant value)
    {
        if (value.Boxed is not OpcVariant nested)
        {
            throw new JsonException("VT_VARIANT must contain an OPC variant value.");
        }

        writer.WriteStartObject();
        writer.WriteNumber("type", (ushort)nested.Type);
        if (nested.Type is not VarType.VT_EMPTY and not VarType.VT_NULL)
        {
            writer.WritePropertyName("value");
            WriteValue(writer, nested);
        }
        writer.WriteEndObject();
    }

    private static void WriteSafeArray(Utf8JsonWriter writer, OpcVariant value)
    {
        if (value.Boxed is not OpcSafeArray array)
        {
            throw new JsonException($"{value.Type} must contain an OPC SAFEARRAY.");
        }

        var expectedType = VarTypeMask.BaseOf(value.Type);
        if (array.ElementType != expectedType)
        {
            throw new JsonException(
                $"SAFEARRAY element type {array.ElementType} does not match variant type {value.Type}.");
        }

        writer.WriteStartObject();
        writer.WriteNumber("elementType", (ushort)array.ElementType);
        writer.WriteNumber("features", (ushort)array.Features);
        writer.WritePropertyName("lengths");
        writer.WriteStartArray();
        foreach (var length in array.Lengths)
        {
            writer.WriteNumberValue(length);
        }
        writer.WriteEndArray();
        writer.WritePropertyName("lowerBounds");
        writer.WriteStartArray();
        foreach (var lowerBound in array.LowerBounds)
        {
            writer.WriteNumberValue(lowerBound);
        }
        writer.WriteEndArray();
        writer.WritePropertyName("elements");
        writer.WriteStartArray();
        for (var i = 0; i < array.Data.Length; i++)
        {
            WriteVariant(
                writer,
                new OpcVariant(array.ElementType, array.Data.GetValue(i)));
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static OpcVariant ReadSafeArray(VarType type, JsonElement value)
    {
        var elementType = (VarType)value.GetProperty("elementType").GetUInt16();
        if (elementType != VarTypeMask.BaseOf(type))
        {
            throw new JsonException(
                $"SAFEARRAY element type {elementType} does not match variant type {type}.");
        }

        var features = (SafeArrayFeatures)value.GetProperty("features").GetUInt16();
        var lengths = ReadInt32Array(value.GetProperty("lengths"));
        var lowerBounds = ReadInt32Array(value.GetProperty("lowerBounds"));
        var elements = value.GetProperty("elements")
            .EnumerateArray()
            .Select(ReadVariant)
            .ToArray();
        foreach (var element in elements)
        {
            if (element.Type != elementType)
            {
                throw new JsonException(
                    $"SAFEARRAY element {element.Type} does not match declared type {elementType}.");
            }
        }

        var data = CreateSafeArrayData(elementType, elements);
        return OpcVariant.FromSafeArray(
            new OpcSafeArray(elementType, data, lengths, lowerBounds, features));
    }

    private static Array CreateSafeArrayData(
        VarType elementType,
        OpcVariant[] elements) =>
        elementType switch
        {
            VarType.VT_I1 => elements.Select(element => (sbyte)element.Boxed!).ToArray(),
            VarType.VT_UI1 => elements.Select(element => (byte)element.Boxed!).ToArray(),
            VarType.VT_I2 => elements.Select(element => (short)element.Boxed!).ToArray(),
            VarType.VT_UI2 => elements.Select(element => (ushort)element.Boxed!).ToArray(),
            VarType.VT_I4 or VarType.VT_INT or VarType.VT_ERROR or VarType.VT_HRESULT =>
                elements.Select(element => (int)element.Boxed!).ToArray(),
            VarType.VT_UI4 or VarType.VT_UINT =>
                elements.Select(element => (uint)element.Boxed!).ToArray(),
            VarType.VT_I8 or VarType.VT_CY or VarType.VT_FILETIME =>
                elements.Select(element => (long)element.Boxed!).ToArray(),
            VarType.VT_UI8 => elements.Select(element => (ulong)element.Boxed!).ToArray(),
            VarType.VT_R4 => elements.Select(element => (float)element.Boxed!).ToArray(),
            VarType.VT_R8 => elements.Select(element => (double)element.Boxed!).ToArray(),
            VarType.VT_DECIMAL => elements.Select(element => (decimal)element.Boxed!).ToArray(),
            VarType.VT_BSTR => elements.Select(element => (string?)element.Boxed).ToArray(),
            VarType.VT_BOOL => elements.Select(element => (bool)element.Boxed!).ToArray(),
            VarType.VT_DATE => elements.Select(element => (DateTime)element.Boxed!).ToArray(),
            VarType.VT_CLSID => elements.Select(element => (Guid)element.Boxed!).ToArray(),
            VarType.VT_VARIANT => elements
                .Select(element => (OpcVariant)element.Boxed!)
                .ToArray(),
            VarType.VT_RECORD => elements
                .Select(element => (OpcRecordValue)element.Boxed!)
                .ToArray(),
            _ => throw new JsonException(
                $"SAFEARRAY element type {elementType} is not supported in DX JSON configuration."),
        };

    private static void WriteRecord(Utf8JsonWriter writer, OpcVariant value)
    {
        if (value.Boxed is not OpcRecordValue record)
        {
            throw new JsonException("VT_RECORD must contain an OPC record value.");
        }

        writer.WriteStartObject();
        writer.WriteString("recordInfoId", record.RecordInfoId);
        writer.WritePropertyName("fields");
        writer.WriteStartArray();
        foreach (var field in record.Values)
        {
            WriteRecordField(writer, field);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static OpcRecordValue ReadRecord(JsonElement value)
    {
        var recordInfoId = value.GetProperty("recordInfoId").GetGuid();
        var fields = value.GetProperty("fields")
            .EnumerateArray()
            .Select(ReadRecordField)
            .ToArray();
        return new OpcRecordValue(recordInfoId, fields);
    }

    private static void WriteRecordField(Utf8JsonWriter writer, object? field)
    {
        if (field is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        OpcVariant variant;
        switch (field)
        {
            case OpcVariant opcVariant:
                writer.WriteString("kind", "variant");
                variant = opcVariant;
                break;
            case OpcSafeArray safeArray:
                writer.WriteString("kind", "safeArray");
                variant = OpcVariant.FromSafeArray(safeArray);
                break;
            case OpcRecordValue record:
                writer.WriteString("kind", "record");
                variant = OpcVariant.FromRecord(record);
                break;
            default:
                writer.WriteString("kind", "scalar");
                variant = CreateScalarVariant(field);
                break;
        }

        writer.WritePropertyName("value");
        WriteVariant(writer, variant);
        writer.WriteEndObject();
    }

    private static object? ReadRecordField(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var kind = element.GetProperty("kind").GetString();
        var variant = ReadVariant(element.GetProperty("value"));
        return kind switch
        {
            "scalar" => variant.Boxed,
            "variant" => variant,
            "safeArray" => variant.AsSafeArray() ??
                throw new JsonException("Record field did not contain a SAFEARRAY."),
            "record" => variant.AsRecord() ??
                throw new JsonException("Record field did not contain an OPC record."),
            _ => throw new JsonException($"Unknown OPC record field kind '{kind}'."),
        };
    }

    private static OpcVariant CreateScalarVariant(object value) =>
        value switch
        {
            sbyte typed => OpcVariant.FromInt8(typed),
            byte typed => OpcVariant.FromUInt8(typed),
            short typed => OpcVariant.FromInt16(typed),
            ushort typed => OpcVariant.FromUInt16(typed),
            int typed => OpcVariant.FromInt32(typed),
            uint typed => OpcVariant.FromUInt32(typed),
            long typed => OpcVariant.FromInt64(typed),
            ulong typed => OpcVariant.FromUInt64(typed),
            float typed => OpcVariant.FromSingle(typed),
            double typed => OpcVariant.FromDouble(typed),
            decimal typed => new OpcVariant(VarType.VT_DECIMAL, typed),
            string typed => OpcVariant.FromString(typed),
            bool typed => OpcVariant.FromBoolean(typed),
            DateTime typed => OpcVariant.FromDate(typed),
            Guid typed => OpcVariant.FromClsid(typed),
            _ => throw new JsonException(
                $"OPC record field type {value.GetType().FullName} is not supported in DX JSON configuration."),
        };

    private static int[] ReadInt32Array(JsonElement element) =>
        element.EnumerateArray().Select(value => value.GetInt32()).ToArray();

    private static void WriteSingle(Utf8JsonWriter writer, float value)
    {
        if (float.IsFinite(value))
        {
            writer.WriteNumberValue(value);
        }
        else
        {
            writer.WriteStringValue(value.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    private static float ReadSingle(JsonElement value) =>
        value.ValueKind == JsonValueKind.String
            ? float.Parse(
                value.GetString()!,
                System.Globalization.CultureInfo.InvariantCulture)
            : value.GetSingle();

    private static void WriteDouble(Utf8JsonWriter writer, double value)
    {
        if (double.IsFinite(value))
        {
            writer.WriteNumberValue(value);
        }
        else
        {
            writer.WriteStringValue(value.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    private static double ReadDouble(JsonElement value) =>
        value.ValueKind == JsonValueKind.String
            ? double.Parse(
                value.GetString()!,
                System.Globalization.CultureInfo.InvariantCulture)
            : value.GetDouble();
}
