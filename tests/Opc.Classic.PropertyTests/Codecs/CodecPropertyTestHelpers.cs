//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using CsCheck;
using Opc.Classic.Ae;
using Opc.Classic.Batch;
using Opc.Classic.Da;
using Opc.Classic.Hda;
using Opc.Classic.Ndr;

namespace Opc.Classic.PropertyTests.Codecs;

internal delegate void NdrValueWriter<T>(ref NdrWriter writer, T value);
internal delegate T NdrValueReader<T>(ref NdrReader reader);

internal static class CodecProperty
{
    internal const int SampleIterations = 100;

    private const int InitialCapacity = 4096;
    private const int MaxCapacity = 4 * 1024 * 1024;
    private static readonly long MinUtcTicks = new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero).Ticks;
    private static readonly long MaxUtcTicks = new DateTimeOffset(2200, 1, 1, 0, 0, 0, TimeSpan.Zero).Ticks;
    private static readonly DateTime OleEpoch = new(1899, 12, 30);
    private static readonly VarType[] ScalarVarTypes =
    [
        VarType.VT_EMPTY,
        VarType.VT_NULL,
        VarType.VT_I1,
        VarType.VT_UI1,
        VarType.VT_I2,
        VarType.VT_UI2,
        VarType.VT_I4,
        VarType.VT_UI4,
        VarType.VT_I8,
        VarType.VT_UI8,
        VarType.VT_R4,
        VarType.VT_R8,
        VarType.VT_BOOL,
        VarType.VT_BSTR,
        VarType.VT_DATE,
        VarType.VT_FILETIME,
        VarType.VT_ERROR,
        VarType.VT_CLSID,
    ];
    private static readonly OpcServerState[] ServerStates =
    [
        OpcServerState.Running,
        OpcServerState.Failed,
        OpcServerState.NoConfig,
        OpcServerState.Suspended,
        OpcServerState.Test,
        OpcServerState.CommFault,
    ];
    private static readonly OpcServerState[] HdaRoundTrippableStates =
    [
        OpcServerState.Running,
        OpcServerState.Failed,
        OpcServerState.NoConfig,
    ];

    static CodecProperty()
    {
        RecordInfoRegistry.Register(RecordInfo);
    }

    internal static Guid RecordInfoId { get; } = new("f22ac2ad-2e0c-4d16-9c7f-3e4b169b8f9f");

    internal static IRecordInfo RecordInfo { get; } = new OpcRecordInfo(
        RecordInfoId,
        "PropertyTestRecord",
        [
            new OpcRecordField("Number", VarType.VT_I4),
            new OpcRecordField("Name", VarType.VT_BSTR),
            new OpcRecordField("Enabled", VarType.VT_BOOL),
            new OpcRecordField("Measurement", VarType.VT_R8),
        ]);

    internal static readonly Gen<char> UnicodeCharGen = Gen.Char[char.MinValue, char.MaxValue];
    internal static readonly Gen<string> ShortStringGen = Gen.String[UnicodeCharGen, 0, 32];
    internal static readonly Gen<string> MediumStringGen = Gen.String[UnicodeCharGen, 0, 256];
    internal static readonly Gen<string?> NullableShortStringGen =
        Gen.Select(Gen.Int[0, 4], ShortStringGen, (choice, text) => choice == 0 ? null : text);
    internal static readonly Gen<DateTimeOffset> UtcDateTimeOffsetGen =
        Gen.Long[MinUtcTicks, MaxUtcTicks]
            .Select(ticks => new DateTimeOffset(ticks - ticks % TimeSpan.TicksPerMillisecond, TimeSpan.Zero));
    internal static readonly Gen<DateTime> OleDateTimeGen =
        Gen.Select(Gen.Int[0, 73000], Gen.Int[0, 86399],
            (days, seconds) => OleEpoch.AddDays(days).AddSeconds(seconds));
    internal static readonly Gen<OpcQuality> OpcQualityGen = Gen.UShort.Select(raw => new OpcQuality(raw));
    internal static readonly Gen<VarType> ScalarVarTypeGen =
        Gen.Int[0, ScalarVarTypes.Length - 1].Select(index => ScalarVarTypes[index]);
    internal static readonly Gen<OpcServerState> ServerStateGen =
        Gen.Int[0, ServerStates.Length - 1].Select(index => ServerStates[index]);
    internal static readonly Gen<OpcServerState> HdaRoundTrippableStateGen =
        Gen.Int[0, HdaRoundTrippableStates.Length - 1].Select(index => HdaRoundTrippableStates[index]);
    internal static readonly Gen<Version> VersionGen =
        Gen.Select(Gen.Int[0, 100], Gen.Int[0, 100], Gen.Int[0, 65535],
            (major, minor, build) => new Version(major, minor, build));

    internal static readonly Gen<OpcVariant> SimpleVariantGen = Gen.OneOf<OpcVariant>(
        Gen.Int.Select(OpcVariant.FromInt32),
        Gen.Double.Select(OpcVariant.FromDouble),
        Gen.Single.Select(OpcVariant.FromSingle),
        Gen.Bool.Select(OpcVariant.FromBoolean),
        ShortStringGen.Select(OpcVariant.FromString),
        OleDateTimeGen.Select(OpcVariant.FromDate),
        Gen.Long.Select(OpcVariant.FromFileTime),
        Gen.Guid.Select(OpcVariant.FromClsid),
        Gen.Int.Select(OpcVariant.FromError),
        Gen.Byte.Select(OpcVariant.FromUInt8),
        Gen.SByte.Select(OpcVariant.FromInt8),
        Gen.Short.Select(OpcVariant.FromInt16),
        Gen.UShort.Select(OpcVariant.FromUInt16),
        Gen.UInt.Select(OpcVariant.FromUInt32),
        Gen.Long.Select(OpcVariant.FromInt64),
        Gen.ULong.Select(OpcVariant.FromUInt64),
        Gen.Int.Select(_ => OpcVariant.Empty),
        Gen.Int.Select(_ => OpcVariant.Null));

    internal static readonly Gen<OpcVariant> RecursiveVariantGen = Gen.Recursive<OpcVariant>((depth, self) =>
        depth >= 4
            ? SimpleVariantGen
            : Gen.OneOf(SimpleVariantGen, self.Select(OpcVariant.FromVariant)));

    internal static readonly Gen<OpcRecordValue> RecordValueGen =
        Gen.Select(Gen.Int, NullableShortStringGen, Gen.Bool, Gen.Double,
            (number, name, enabled, measurement) => new OpcRecordValue(
                RecordInfo,
                [number, name, enabled, measurement]));

    internal static readonly Gen<OpcSafeArray> Int32SafeArrayGen =
        Gen.Int.Array[0, 32].Select(values => new OpcSafeArray(VarType.VT_I4, values));
    internal static readonly Gen<OpcSafeArray> DoubleSafeArrayGen =
        Gen.Double.Array[0, 32].Select(values => new OpcSafeArray(VarType.VT_R8, values));
    internal static readonly Gen<OpcSafeArray> BstrSafeArrayGen =
        NullableShortStringGen.Array[0, 16].Select(values => new OpcSafeArray(
            VarType.VT_BSTR,
            values,
            features: SafeArrayFeatures.HaveVartype | SafeArrayFeatures.Bstr));
    internal static readonly Gen<OpcSafeArray> VariantSafeArrayGen =
        RecursiveVariantGen.Array[0, 8].Select(values => new OpcSafeArray(
            VarType.VT_VARIANT,
            values,
            features: SafeArrayFeatures.HaveVartype | SafeArrayFeatures.Variant));
    internal static readonly Gen<OpcSafeArray> RecordSafeArrayGen =
        RecordValueGen.Array[0, 8].Select(values => new OpcSafeArray(
            VarType.VT_RECORD,
            values,
            features: SafeArrayFeatures.HaveVartype | SafeArrayFeatures.Record));

    internal static readonly Gen<OpcItemState> OpcItemStateGen =
        Gen.Select(Gen.Int, UtcDateTimeOffsetGen, OpcQualityGen, SimpleVariantGen,
            (clientHandle, timestamp, quality, value) => new OpcItemState(clientHandle, timestamp, quality, value));

    internal static readonly Gen<OpcItemResult> OpcItemResultGen =
        Gen.Select(Gen.Int, ScalarVarTypeGen, Gen.Int, Gen.Byte.Array[0, 32],
            (serverHandle, canonicalDataType, accessRights, blob) =>
                new OpcItemResult(serverHandle, canonicalDataType, accessRights, blob));

    internal static readonly Gen<OpcItemDef> OpcItemDefGen =
        Gen.Select(NullableShortStringGen, NullableShortStringGen, Gen.Bool, Gen.Int, Gen.Byte.Array[0, 32], ScalarVarTypeGen,
            (accessPath, itemId, active, clientHandle, blob, requestedDataType) =>
                new OpcItemDef(accessPath, itemId, active, clientHandle, blob, requestedDataType));

    internal static readonly Gen<OpcItemPropertyResult> OpcItemPropertyResultGen =
        from value in SimpleVariantGen
        from propertyId in Gen.Int
        from itemId in NullableShortStringGen
        from description in NullableShortStringGen
        from errorId in Gen.Int
        select new OpcItemPropertyResult(value.Type, propertyId, itemId, description, value, errorId);

    internal static readonly Gen<OpcItemProperties> OpcItemPropertiesGen =
        from count in Gen.Int[0, 4]
        from properties in OpcItemPropertyResultGen.Array[count]
        from errorId in Gen.Int
        select new OpcItemProperties(errorId, properties);

    internal static readonly Gen<OpcBrowseElementResult> OpcBrowseElementResultGen =
        Gen.Select(NullableShortStringGen, NullableShortStringGen, Gen.Int[0, 3], OpcItemPropertiesGen,
            (name, itemId, flagValue, properties) => new OpcBrowseElementResult(name, itemId, flagValue, properties));

    internal static readonly Gen<OpcItemAttributes> OpcItemAttributesGen =
        from accessPath in NullableShortStringGen
        from itemId in NullableShortStringGen
        from active in Gen.Bool
        from clientHandle in Gen.Int
        from serverHandle in Gen.Int
        from accessRights in Gen.Int
        from blob in Gen.Byte.Array[0, 32]
        from requestedDataType in ScalarVarTypeGen
        from canonicalDataType in ScalarVarTypeGen
        from euType in Gen.Int[0, 2]
        from euInfo in SimpleVariantGen
        select new OpcItemAttributes(
            accessPath,
            itemId,
            active,
            clientHandle,
            serverHandle,
            accessRights,
            blob,
            requestedDataType,
            canonicalDataType,
            euType,
            euInfo);

    internal static readonly Gen<OpcGroupState> OpcGroupStateGen =
        Gen.Select(Gen.Int, Gen.Int, NullableShortStringGen, Gen.Bool, Gen.Int, Gen.Int, Gen.Single, Gen.Int,
            (clientHandle, serverHandle, name, active, updateRate, timeBias, percentDeadband, localeId) =>
                new OpcGroupState(clientHandle, serverHandle, name, active, updateRate, timeBias, percentDeadband, localeId));

    internal static readonly Gen<OpcItemVqt> OpcItemVqtGen =
        from value in SimpleVariantGen
        from qualityChoice in Gen.Int[0, 1]
        from quality in OpcQualityGen
        from timestampChoice in Gen.Int[0, 1]
        from timestamp in UtcDateTimeOffsetGen
        select new OpcItemVqt(
            value,
            qualityChoice == 0 ? null : quality,
            timestampChoice == 0 ? null : timestamp);

    internal static readonly Gen<OpcServerStatus> DaServerStatusGen =
        Gen.Select(UtcDateTimeOffsetGen, UtcDateTimeOffsetGen, UtcDateTimeOffsetGen, ServerStateGen, VersionGen,
            Gen.Int[0, 10_000], Gen.Int[0, 10_000], ShortStringGen,
            (start, current, lastUpdate, state, version, groupCount, bandWidth, vendorInfo) => new OpcServerStatus
            {
                Spec = OpcStatusSpec.Da,
                StartTime = start,
                CurrentTime = current,
                LastUpdateTime = lastUpdate,
                State = state,
                ServerVersion = version,
                GroupCount = groupCount,
                BandWidth = (uint)bandWidth,
                VendorInfo = vendorInfo,
            });

    internal static readonly Gen<OpcServerStatus> AeServerStatusGen =
        Gen.Select(UtcDateTimeOffsetGen, UtcDateTimeOffsetGen, UtcDateTimeOffsetGen, ServerStateGen, VersionGen, ShortStringGen,
            (start, current, lastUpdate, state, version, vendorInfo) => new OpcServerStatus
            {
                Spec = OpcStatusSpec.Ae,
                StartTime = start,
                CurrentTime = current,
                LastUpdateTime = lastUpdate,
                State = state,
                ServerVersion = version,
                VendorInfo = vendorInfo,
            });

    internal static readonly Gen<OpcServerStatus> HdaServerStatusGen =
        Gen.Select(UtcDateTimeOffsetGen, UtcDateTimeOffsetGen, HdaRoundTrippableStateGen, VersionGen,
            Gen.Int[0, 100_000], ShortStringGen,
            (start, current, state, version, maxReturnValues, vendorInfo) => new OpcServerStatus
            {
                Spec = OpcStatusSpec.Hda,
                StartTime = start,
                CurrentTime = current,
                State = state,
                ServerVersion = version,
                MaxReturnValues = maxReturnValues,
                VendorInfo = vendorInfo,
            });

    internal static readonly Gen<OpcConditionState> OpcConditionStateGen =
        from subCount in Gen.Int[0, 3]
        from attrCount in Gen.Int[0, 3]
        from state in Gen.UShort
        from activeSubCondition in NullableShortStringGen
        from activeSubConditionDefinition in NullableShortStringGen
        from activeSubConditionSeverity in Gen.UInt
        from activeSubConditionDescription in NullableShortStringGen
        from quality in OpcQualityGen
        from lastAckTime in UtcDateTimeOffsetGen
        from subConditionLastActive in UtcDateTimeOffsetGen
        from conditionLastActive in UtcDateTimeOffsetGen
        from conditionLastInactive in UtcDateTimeOffsetGen
        from acknowledgerId in NullableShortStringGen
        from comment in NullableShortStringGen
        from subConditionNames in NullableShortStringGen.Array[subCount]
        from subConditionDefinitions in NullableShortStringGen.Array[subCount]
        from subConditionSeverities in Gen.UInt.Array[subCount]
        from subConditionDescriptions in NullableShortStringGen.Array[subCount]
        from eventAttributes in SimpleVariantGen.Array[attrCount]
        from errors in Gen.Int.Array[attrCount]
        select new OpcConditionState(
            state,
            activeSubCondition,
            activeSubConditionDefinition,
            activeSubConditionSeverity,
            activeSubConditionDescription,
            quality,
            lastAckTime,
            subConditionLastActive,
            conditionLastActive,
            conditionLastInactive,
            acknowledgerId,
            comment,
            subConditionNames,
            subConditionDefinitions,
            subConditionSeverities,
            subConditionDescriptions,
            eventAttributes,
            errors);

    internal static readonly Gen<OpcEventNotification> OpcEventNotificationGen =
        from attrCount in Gen.Int[0, 4]
        from changeMask in Gen.UShort
        from newState in Gen.UShort
        from source in NullableShortStringGen
        from time in UtcDateTimeOffsetGen
        from message in NullableShortStringGen
        from eventType in Gen.UInt
        from eventCategory in Gen.UInt
        from severity in Gen.UInt
        from conditionName in NullableShortStringGen
        from subconditionName in NullableShortStringGen
        from quality in OpcQualityGen
        from ackRequired in Gen.Bool
        from activeTime in UtcDateTimeOffsetGen
        from cookie in Gen.UInt
        from eventAttributes in SimpleVariantGen.Array[attrCount]
        from actorId in NullableShortStringGen
        select new OpcEventNotification(
            changeMask,
            newState,
            source,
            time,
            message,
            eventType,
            eventCategory,
            severity,
            conditionName,
            subconditionName,
            quality,
            ackRequired,
            activeTime,
            cookie,
            eventAttributes,
            actorId);

    internal static readonly Gen<OpcHdaTime> OpcHdaTimeGen =
        Gen.Select(Gen.Int[0, 1], ShortStringGen, UtcDateTimeOffsetGen,
            (isString, expression, timestamp) => isString == 0
                ? OpcHdaTime.FromTimestamp(timestamp)
                : OpcHdaTime.FromString(expression));

    internal static readonly Gen<OpcHdaItem> OpcHdaItemGen =
        from count in Gen.Int[0, 4]
        from clientHandle in Gen.Int
        from aggregateHandle in Gen.Int
        from timestamps in UtcDateTimeOffsetGen.Array[count]
        from qualities in Gen.UInt.Array[count]
        from values in SimpleVariantGen.Array[count]
        select new OpcHdaItem(clientHandle, aggregateHandle, timestamps, qualities, values);

    internal static readonly Gen<OpcHdaAttribute> OpcHdaAttributeGen =
        from count in Gen.Int[0, 4]
        from clientHandle in Gen.Int
        from attributeId in Gen.Int
        from timestamps in UtcDateTimeOffsetGen.Array[count]
        from values in SimpleVariantGen.Array[count]
        select new OpcHdaAttribute(clientHandle, attributeId, timestamps, values);

    internal static readonly Gen<OpcHdaModifiedItem> OpcHdaModifiedItemGen =
        from count in Gen.Int[0, 4]
        from clientHandle in Gen.Int
        from timestamps in UtcDateTimeOffsetGen.Array[count]
        from qualities in Gen.UInt.Array[count]
        from values in SimpleVariantGen.Array[count]
        from modificationTimes in UtcDateTimeOffsetGen.Array[count]
        from editTypes in Gen.UInt.Array[count]
        from users in NullableShortStringGen.Array[count]
        select new OpcHdaModifiedItem(clientHandle, timestamps, qualities, values, modificationTimes, editTypes, users);

    internal static readonly Gen<OpcHdaAnnotation> OpcHdaAnnotationGen =
        from count in Gen.Int[0, 4]
        from clientHandle in Gen.Int
        from timestamps in UtcDateTimeOffsetGen.Array[count]
        from annotations in NullableShortStringGen.Array[count]
        from annotationTimes in UtcDateTimeOffsetGen.Array[count]
        from users in NullableShortStringGen.Array[count]
        select new OpcHdaAnnotation(clientHandle, timestamps, annotations, annotationTimes, users);

    internal static readonly Gen<OpcBatchSummary> OpcBatchSummaryGen =
        from id in NullableShortStringGen
        from description in NullableShortStringGen
        from opcItemId in NullableShortStringGen
        from masterRecipeId in NullableShortStringGen
        from batchSize in Gen.Single
        from engineeringUnits in NullableShortStringGen
        from executionState in NullableShortStringGen
        from executionMode in NullableShortStringGen
        from start in UtcDateTimeOffsetGen
        from end in UtcDateTimeOffsetGen
        select new OpcBatchSummary(
            id,
            description,
            opcItemId,
            masterRecipeId,
            batchSize,
            engineeringUnits,
            executionState,
            executionMode,
            start,
            end);

    internal static readonly Gen<OpcBatchSummaryFilter> OpcBatchSummaryFilterGen =
        from id in NullableShortStringGen
        from description in NullableShortStringGen
        from opcItemId in NullableShortStringGen
        from masterRecipeId in NullableShortStringGen
        from minBatchSize in Gen.Single
        from maxBatchSize in Gen.Single
        from engineeringUnits in NullableShortStringGen
        from executionState in NullableShortStringGen
        from executionMode in NullableShortStringGen
        from minStart in UtcDateTimeOffsetGen
        from maxStart in UtcDateTimeOffsetGen
        from minEnd in UtcDateTimeOffsetGen
        from maxEnd in UtcDateTimeOffsetGen
        select new OpcBatchSummaryFilter(
            id,
            description,
            opcItemId,
            masterRecipeId,
            minBatchSize,
            maxBatchSize,
            engineeringUnits,
            executionState,
            executionMode,
            minStart,
            maxStart,
            minEnd,
            maxEnd);

    internal static T RoundTrip<T>(T value, NdrValueWriter<T> write, NdrValueReader<T> read, int capacity = InitialCapacity)
    {
        for (int currentCapacity = capacity; currentCapacity <= MaxCapacity; currentCapacity *= 2)
        {
            var buffer = new byte[currentCapacity];
            var writer = new NdrWriter(buffer);
            try
            {
                write(ref writer, value);
                var reader = new NdrReader(buffer.AsSpan(0, writer.Position));
                T decoded = read(ref reader);
                if (reader.Position != reader.Length)
                {
                    throw new InvalidOperationException(
                        $"NDR reader left {reader.Length - reader.Position} trailing bytes unread.");
                }
                return decoded;
            }
            catch (InvalidOperationException ex) when (
                currentCapacity < MaxCapacity &&
                ex.Message.Contains("buffer overflow", StringComparison.OrdinalIgnoreCase))
            {
                // Retry with a larger buffer for generated long strings or arrays.
            }
        }

        throw new InvalidOperationException("NDR round-trip buffer exceeded the test maximum capacity.");
    }

    internal static bool RoundTrips<T>(
        T value,
        NdrValueWriter<T> write,
        NdrValueReader<T> read,
        Func<T, T, bool> equals,
        int capacity = InitialCapacity) =>
        equals(value, RoundTrip(value, write, read, capacity));

    internal static bool RoundTripsByEquals<T>(
        T value,
        NdrValueWriter<T> write,
        NdrValueReader<T> read,
        int capacity = InitialCapacity) =>
        RoundTrips(value, write, read, static (left, right) => Equals(left, right), capacity);

    internal static bool RoundTripsConformantArray<T>(
        T[] values,
        NdrValueWriter<T> writeOne,
        NdrValueReader<T> readOne,
        Func<T, T, bool> equals,
        int capacity = InitialCapacity)
    {
        T[] decoded = RoundTrip(
            values,
            static (ref NdrWriter writer, T[] array, NdrValueWriter<T> elementWriter) =>
            {
                writer.WriteConformanceHeader(array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    elementWriter(ref writer, array[i]);
                }
            },
            static (ref NdrReader reader, NdrValueReader<T> elementReader) =>
            {
                int count = reader.ReadConformanceHeader();
                var array = new T[count];
                for (int i = 0; i < count; i++)
                {
                    array[i] = elementReader(ref reader);
                }
                return array;
            },
            writeOne,
            readOne,
            capacity);

        return SequenceEqual(values, decoded, equals);
    }

    internal static bool VariantWriteIsUnsupported(OpcVariant value)
    {
        try
        {
            _ = RoundTrip(value, static (ref NdrWriter writer, OpcVariant variant) => writer.WriteVariant(variant),
                static (ref NdrReader reader) => reader.ReadVariant());
            return false;
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not supported", StringComparison.OrdinalIgnoreCase);
        }
    }

    internal static int VariantDepth(OpcVariant value)
    {
        if (value.Type == VarType.VT_VARIANT && value.Boxed is OpcVariant nested)
        {
            return 1 + VariantDepth(nested);
        }
        if (value.Boxed is OpcSafeArray array && array.ElementType == VarType.VT_VARIANT)
        {
            var values = (OpcVariant[])array.Data;
            return values.Length == 0 ? 1 : 1 + values.Max(VariantDepth);
        }
        return 1;
    }

    internal static bool VariantEquals(OpcVariant left, OpcVariant right)
    {
        if (left.Type != right.Type)
        {
            return false;
        }

        return (left.Boxed, right.Boxed) switch
        {
            (OpcSafeArray a, OpcSafeArray b) => SafeArrayEquals(a, b),
            (OpcRecordValue a, OpcRecordValue b) => RecordValueEquals(a, b),
            (null, null) => true,
            _ => Equals(left.Boxed, right.Boxed),
        };
    }

    internal static bool SafeArrayEquals(OpcSafeArray left, OpcSafeArray right)
    {
        if (left.ElementType != right.ElementType || left.Features != right.Features)
        {
            return false;
        }
        if (!left.Lengths.SequenceEqual(right.Lengths) || !left.LowerBounds.SequenceEqual(right.LowerBounds))
        {
            return false;
        }
        if (left.Data.Length != right.Data.Length)
        {
            return false;
        }

        for (int i = 0; i < left.Data.Length; i++)
        {
            object? l = left.Data.GetValue(i);
            object? r = right.Data.GetValue(i);
            bool equal = (l, r) switch
            {
                (OpcVariant lv, OpcVariant rv) => VariantEquals(lv, rv),
                (OpcRecordValue lv, OpcRecordValue rv) => RecordValueEquals(lv, rv),
                (null, null) => true,
                _ => Equals(l, r),
            };
            if (!equal)
            {
                return false;
            }
        }
        return true;
    }

    internal static bool RecordValueEquals(OpcRecordValue? left, OpcRecordValue? right)
    {
        if (left is null || right is null)
        {
            return left is null && right is null;
        }
        if (left.RecordInfoId != right.RecordInfoId || left.Values.Count != right.Values.Count)
        {
            return false;
        }
        for (int i = 0; i < left.Values.Count; i++)
        {
            object? l = left.Values[i];
            object? r = right.Values[i];
            bool equal = (l, r) switch
            {
                (OpcVariant lv, OpcVariant rv) => VariantEquals(lv, rv),
                (OpcRecordValue lv, OpcRecordValue rv) => RecordValueEquals(lv, rv),
                (null, null) => true,
                _ => Equals(l, r),
            };
            if (!equal)
            {
                return false;
            }
        }
        return true;
    }

    internal static bool DateTimeOffsetEquals(DateTimeOffset left, DateTimeOffset right) =>
        left.UtcTicks == right.UtcTicks;

    internal static bool NullableDateTimeOffsetEquals(DateTimeOffset? left, DateTimeOffset? right) =>
        (!left.HasValue && !right.HasValue) ||
        (left.HasValue && right.HasValue && DateTimeOffsetEquals(left.Value, right.Value));

    internal static bool SequenceEqual<T>(T[] left, T[] right, Func<T, T, bool> equals)
    {
        if (left.Length != right.Length)
        {
            return false;
        }
        for (int i = 0; i < left.Length; i++)
        {
            if (!equals(left[i], right[i]))
            {
                return false;
            }
        }
        return true;
    }

    internal static bool DateTimeOffsetArrayEquals(DateTimeOffset[] left, DateTimeOffset[] right) =>
        SequenceEqual(left, right, DateTimeOffsetEquals);

    internal static bool VariantArrayEquals(OpcVariant[] left, OpcVariant[] right) =>
        SequenceEqual(left, right, VariantEquals);

    internal static bool StringArrayEquals(string?[] left, string?[] right) =>
        SequenceEqual(left, right, static (l, r) => string.Equals(l, r, StringComparison.Ordinal));

    internal static bool OpcItemStateEquals(OpcItemState left, OpcItemState right) =>
        left.ClientHandle == right.ClientHandle &&
        DateTimeOffsetEquals(left.Timestamp, right.Timestamp) &&
        left.Quality == right.Quality &&
        VariantEquals(left.Value, right.Value);

    internal static bool OpcItemResultEquals(OpcItemResult left, OpcItemResult right) =>
        left.ServerHandle == right.ServerHandle &&
        left.CanonicalDataType == right.CanonicalDataType &&
        left.AccessRights == right.AccessRights &&
        left.Blob.SequenceEqual(right.Blob);

    internal static bool OpcItemDefEquals(OpcItemDef left, OpcItemDef right) =>
        string.Equals(left.AccessPath, right.AccessPath, StringComparison.Ordinal) &&
        string.Equals(left.ItemId, right.ItemId, StringComparison.Ordinal) &&
        left.Active == right.Active &&
        left.ClientHandle == right.ClientHandle &&
        (left.Blob ?? Array.Empty<byte>()).SequenceEqual(right.Blob ?? Array.Empty<byte>()) &&
        left.RequestedDataType == right.RequestedDataType;

    internal static bool OpcItemPropertyResultEquals(OpcItemPropertyResult left, OpcItemPropertyResult right) =>
        left.DataType == right.DataType &&
        left.PropertyId == right.PropertyId &&
        string.Equals(left.ItemId, right.ItemId, StringComparison.Ordinal) &&
        string.Equals(left.Description, right.Description, StringComparison.Ordinal) &&
        VariantEquals(left.Value, right.Value) &&
        left.ErrorId == right.ErrorId;

    internal static bool OpcItemPropertiesEquals(OpcItemProperties left, OpcItemProperties right) =>
        left.ErrorId == right.ErrorId &&
        SequenceEqual(left.Properties, right.Properties, OpcItemPropertyResultEquals);

    internal static bool OpcBrowseElementResultEquals(OpcBrowseElementResult left, OpcBrowseElementResult right) =>
        string.Equals(left.Name, right.Name, StringComparison.Ordinal) &&
        string.Equals(left.ItemId, right.ItemId, StringComparison.Ordinal) &&
        left.FlagValue == right.FlagValue &&
        OpcItemPropertiesEquals(left.Properties, right.Properties);

    internal static bool OpcItemAttributesEquals(OpcItemAttributes left, OpcItemAttributes right) =>
        string.Equals(left.AccessPath, right.AccessPath, StringComparison.Ordinal) &&
        string.Equals(left.ItemId, right.ItemId, StringComparison.Ordinal) &&
        left.Active == right.Active &&
        left.ClientHandle == right.ClientHandle &&
        left.ServerHandle == right.ServerHandle &&
        left.AccessRights == right.AccessRights &&
        left.Blob.SequenceEqual(right.Blob) &&
        left.RequestedDataType == right.RequestedDataType &&
        left.CanonicalDataType == right.CanonicalDataType &&
        left.EUType == right.EUType &&
        VariantEquals(left.EUInfo, right.EUInfo);

    internal static bool OpcGroupStateEquals(OpcGroupState left, OpcGroupState right) =>
        left.ClientHandle == right.ClientHandle &&
        left.ServerHandle == right.ServerHandle &&
        string.Equals(left.Name, right.Name, StringComparison.Ordinal) &&
        left.Active == right.Active &&
        left.UpdateRate == right.UpdateRate &&
        left.TimeBias == right.TimeBias &&
        left.PercentDeadband.Equals(right.PercentDeadband) &&
        left.LocaleId == right.LocaleId;

    internal static bool OpcItemVqtEquals(OpcItemVqt left, OpcItemVqt right) =>
        VariantEquals(left.Value, right.Value) &&
        left.Quality == right.Quality &&
        NullableDateTimeOffsetEquals(left.Timestamp, right.Timestamp);

    internal static bool OpcServerStatusEquals(OpcServerStatus left, OpcServerStatus right) =>
        left.Spec == right.Spec &&
        DateTimeOffsetEquals(left.StartTime, right.StartTime) &&
        DateTimeOffsetEquals(left.CurrentTime, right.CurrentTime) &&
        DateTimeOffsetEquals(left.LastUpdateTime, right.LastUpdateTime) &&
        left.State == right.State &&
        Equals(left.ServerVersion, right.ServerVersion) &&
        string.Equals(left.VendorInfo, right.VendorInfo, StringComparison.Ordinal) &&
        left.GroupCount == right.GroupCount &&
        left.BandWidth == right.BandWidth &&
        left.MaxReturnValues == right.MaxReturnValues;

    internal static bool OpcConditionStateEquals(OpcConditionState left, OpcConditionState right) =>
        left.State == right.State &&
        string.Equals(left.ActiveSubCondition, right.ActiveSubCondition, StringComparison.Ordinal) &&
        string.Equals(left.ActiveSubConditionDefinition, right.ActiveSubConditionDefinition, StringComparison.Ordinal) &&
        left.ActiveSubConditionSeverity == right.ActiveSubConditionSeverity &&
        string.Equals(left.ActiveSubConditionDescription, right.ActiveSubConditionDescription, StringComparison.Ordinal) &&
        left.Quality == right.Quality &&
        DateTimeOffsetEquals(left.LastAckTime, right.LastAckTime) &&
        DateTimeOffsetEquals(left.SubConditionLastActive, right.SubConditionLastActive) &&
        DateTimeOffsetEquals(left.ConditionLastActive, right.ConditionLastActive) &&
        DateTimeOffsetEquals(left.ConditionLastInactive, right.ConditionLastInactive) &&
        string.Equals(left.AcknowledgerId, right.AcknowledgerId, StringComparison.Ordinal) &&
        string.Equals(left.Comment, right.Comment, StringComparison.Ordinal) &&
        StringArrayEquals(left.SubConditionNames, right.SubConditionNames) &&
        StringArrayEquals(left.SubConditionDefinitions, right.SubConditionDefinitions) &&
        left.SubConditionSeverities.SequenceEqual(right.SubConditionSeverities) &&
        StringArrayEquals(left.SubConditionDescriptions, right.SubConditionDescriptions) &&
        VariantArrayEquals(left.EventAttributes, right.EventAttributes) &&
        left.Errors.SequenceEqual(right.Errors);

    internal static bool OpcEventNotificationEquals(OpcEventNotification left, OpcEventNotification right) =>
        left.ChangeMask == right.ChangeMask &&
        left.NewState == right.NewState &&
        string.Equals(left.Source, right.Source, StringComparison.Ordinal) &&
        DateTimeOffsetEquals(left.Time, right.Time) &&
        string.Equals(left.Message, right.Message, StringComparison.Ordinal) &&
        left.EventType == right.EventType &&
        left.EventCategory == right.EventCategory &&
        left.Severity == right.Severity &&
        string.Equals(left.ConditionName, right.ConditionName, StringComparison.Ordinal) &&
        string.Equals(left.SubconditionName, right.SubconditionName, StringComparison.Ordinal) &&
        left.Quality == right.Quality &&
        left.AckRequired == right.AckRequired &&
        DateTimeOffsetEquals(left.ActiveTime, right.ActiveTime) &&
        left.Cookie == right.Cookie &&
        VariantArrayEquals(left.EventAttributes, right.EventAttributes) &&
        string.Equals(left.ActorId, right.ActorId, StringComparison.Ordinal);

    internal static bool OpcHdaTimeEquals(OpcHdaTime left, OpcHdaTime right) =>
        left.IsStringExpression == right.IsStringExpression &&
        string.Equals(left.StringExpression, right.StringExpression, StringComparison.Ordinal) &&
        DateTimeOffsetEquals(left.Timestamp, right.Timestamp);

    internal static bool OpcHdaItemEquals(OpcHdaItem left, OpcHdaItem right) =>
        left.ClientHandle == right.ClientHandle &&
        left.AggregateHandle == right.AggregateHandle &&
        DateTimeOffsetArrayEquals(left.Timestamps, right.Timestamps) &&
        left.Qualities.SequenceEqual(right.Qualities) &&
        VariantArrayEquals(left.Values, right.Values);

    internal static bool OpcHdaAttributeEquals(OpcHdaAttribute left, OpcHdaAttribute right) =>
        left.ClientHandle == right.ClientHandle &&
        left.AttributeId == right.AttributeId &&
        DateTimeOffsetArrayEquals(left.Timestamps, right.Timestamps) &&
        VariantArrayEquals(left.Values, right.Values);

    internal static bool OpcHdaModifiedItemEquals(OpcHdaModifiedItem left, OpcHdaModifiedItem right) =>
        left.ClientHandle == right.ClientHandle &&
        DateTimeOffsetArrayEquals(left.Timestamps, right.Timestamps) &&
        left.Qualities.SequenceEqual(right.Qualities) &&
        VariantArrayEquals(left.Values, right.Values) &&
        DateTimeOffsetArrayEquals(left.ModificationTimes, right.ModificationTimes) &&
        left.EditTypes.SequenceEqual(right.EditTypes) &&
        StringArrayEquals(left.Users, right.Users);

    internal static bool OpcHdaAnnotationEquals(OpcHdaAnnotation left, OpcHdaAnnotation right) =>
        left.ClientHandle == right.ClientHandle &&
        DateTimeOffsetArrayEquals(left.Timestamps, right.Timestamps) &&
        StringArrayEquals(left.Annotations, right.Annotations) &&
        DateTimeOffsetArrayEquals(left.AnnotationTimes, right.AnnotationTimes) &&
        StringArrayEquals(left.Users, right.Users);

    internal static bool OpcBatchSummaryEquals(OpcBatchSummary left, OpcBatchSummary right) =>
        string.Equals(left.Id, right.Id, StringComparison.Ordinal) &&
        string.Equals(left.Description, right.Description, StringComparison.Ordinal) &&
        string.Equals(left.OpcItemId, right.OpcItemId, StringComparison.Ordinal) &&
        string.Equals(left.MasterRecipeId, right.MasterRecipeId, StringComparison.Ordinal) &&
        left.BatchSize.Equals(right.BatchSize) &&
        string.Equals(left.EngineeringUnits, right.EngineeringUnits, StringComparison.Ordinal) &&
        string.Equals(left.ExecutionState, right.ExecutionState, StringComparison.Ordinal) &&
        string.Equals(left.ExecutionMode, right.ExecutionMode, StringComparison.Ordinal) &&
        DateTimeOffsetEquals(left.ActualStartTime, right.ActualStartTime) &&
        DateTimeOffsetEquals(left.ActualEndTime, right.ActualEndTime);

    internal static bool OpcBatchSummaryFilterEquals(OpcBatchSummaryFilter left, OpcBatchSummaryFilter right) =>
        string.Equals(left.Id, right.Id, StringComparison.Ordinal) &&
        string.Equals(left.Description, right.Description, StringComparison.Ordinal) &&
        string.Equals(left.OpcItemId, right.OpcItemId, StringComparison.Ordinal) &&
        string.Equals(left.MasterRecipeId, right.MasterRecipeId, StringComparison.Ordinal) &&
        left.MinBatchSize.Equals(right.MinBatchSize) &&
        left.MaxBatchSize.Equals(right.MaxBatchSize) &&
        string.Equals(left.EngineeringUnits, right.EngineeringUnits, StringComparison.Ordinal) &&
        string.Equals(left.ExecutionState, right.ExecutionState, StringComparison.Ordinal) &&
        string.Equals(left.ExecutionMode, right.ExecutionMode, StringComparison.Ordinal) &&
        DateTimeOffsetEquals(left.MinStartTime, right.MinStartTime) &&
        DateTimeOffsetEquals(left.MaxStartTime, right.MaxStartTime) &&
        DateTimeOffsetEquals(left.MinEndTime, right.MinEndTime) &&
        DateTimeOffsetEquals(left.MaxEndTime, right.MaxEndTime);

    private static TValue RoundTrip<TValue, TArg1, TArg2>(
        TValue value,
        WriteWithArgs<TValue, TArg1> write,
        ReadWithArg<TArg2, TValue> read,
        TArg1 writeArg,
        TArg2 readArg,
        int capacity)
    {
        return RoundTrip<TValue>(
            value,
            (ref NdrWriter writer, TValue v) => write(ref writer, v, writeArg),
            (ref NdrReader reader) => read(ref reader, readArg),
            capacity);
    }

    private delegate void WriteWithArgs<TValue, TArg>(ref NdrWriter writer, TValue value, TArg arg);
    private delegate TResult ReadWithArg<TArg, out TResult>(ref NdrReader reader, TArg arg);
}
