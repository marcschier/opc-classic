// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Batch.Tests;

public sealed class OpcBatchPropertyIdTests
{
    [Test]
    [Arguments(nameof(OpcBatchPropertyId.Id), OpcBatchPropertyId.Id, 400, "Item name, equipment ID, batch ID, internal ID, or alias used to build the qualified item ID.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.Value), OpcBatchPropertyId.Value, 401, "Item value derived from OPC Data Access property ID 2.", "<varies>")]
    [Arguments(nameof(OpcBatchPropertyId.AccessRights), OpcBatchPropertyId.AccessRights, 402, "Item access rights derived from OPC Data Access property ID 5.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.Eu), OpcBatchPropertyId.Eu, 403, "Engineering units derived from OPC Data Access property ID 100.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.Description), OpcBatchPropertyId.Description, 404, "Item description derived from OPC Data Access property ID 101.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.HighValueLimit), OpcBatchPropertyId.HighValueLimit, 405, "Highest value this item may take on.", "<varies>")]
    [Arguments(nameof(OpcBatchPropertyId.LowValueLimit), OpcBatchPropertyId.LowValueLimit, 406, "Lowest value this item may take on.", "<varies>")]
    [Arguments(nameof(OpcBatchPropertyId.TimeZone), OpcBatchPropertyId.TimeZone, 407, "Item timezone derived from OPC Data Access property ID 108.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.ConditionStatus), OpcBatchPropertyId.ConditionStatus, 408, "Condition status derived from OPC Alarms and Events property ID 300.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.PhysicalModelLevel), OpcBatchPropertyId.PhysicalModelLevel, 409, "Physical model level associated with equipment, using OPCB_ENUM_PHYS.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.BatchModelLevel), OpcBatchPropertyId.BatchModelLevel, 410, "IEC 61512-1 procedural model level for a batch or recipe procedural element, using OPCB_ENUM_PROC.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.RelatedBatchIds), OpcBatchPropertyId.RelatedBatchIds, 411, "Batch ID or batch ID list related to a physical model item.", "VT_ARRAY | VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.Version), OpcBatchPropertyId.Version, 412, "Server-specific version identifier for the associated item.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.EquipmentClass), OpcBatchPropertyId.EquipmentClass, 413, "Class of the associated equipment, such as reactor or mixer.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.Location), OpcBatchPropertyId.Location, 414, "Building or physical location where the item exists.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.MaximumUserCount), OpcBatchPropertyId.MaximumUserCount, 415, "Maximum concurrent users of the item, with -1 meaning unlimited.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.CurrentUserCount), OpcBatchPropertyId.CurrentUserCount, 416, "Current number of users of the associated item.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.CurrentUserList), OpcBatchPropertyId.CurrentUserList, 417, "Item IDs that are using the associated item.", "VT_ARRAY | VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.AllocatedEquipmentList), OpcBatchPropertyId.AllocatedEquipmentList, 418, "Equipment item IDs allocated by this item.", "VT_ARRAY | VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.RequesterList), OpcBatchPropertyId.RequesterList, 419, "Item IDs queued to allocate the associated item, in precedence order.", "VT_ARRAY | VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.RequestedList), OpcBatchPropertyId.RequestedList, 420, "Item IDs for which this item has a pending allocation request.", "VT_ARRAY | VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.SharedByList), OpcBatchPropertyId.SharedByList, 421, "Item IDs that can share this item.", "VT_ARRAY | VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.EquipmentState), OpcBatchPropertyId.EquipmentState, 422, "Current state of the equipment.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.EquipmentMode), OpcBatchPropertyId.EquipmentMode, 423, "Current mode of the equipment.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.UpstreamEquipmentList), OpcBatchPropertyId.UpstreamEquipmentList, 424, "Equipment item IDs from which material is directly received.", "VT_ARRAY | VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.DownstreamEquipmentList), OpcBatchPropertyId.DownstreamEquipmentList, 425, "Equipment item IDs to which material is directly sent.", "VT_ARRAY | VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.EquipmentProceduralElementList), OpcBatchPropertyId.EquipmentProceduralElementList, 426, "Equipment procedural elements this equipment can perform.", "VT_ARRAY | VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.CurrentProcedureList), OpcBatchPropertyId.CurrentProcedureList, 427, "Lowest-level active recipe procedural element item IDs on this equipment item.", "VT_ARRAY | VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.TrainList), OpcBatchPropertyId.TrainList, 428, "Withdrawn two-dimensional processing train list; replaced by TrainList2.", "VT_ARRAY | VT_ARRAY | VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.DeviceDataSource), OpcBatchPropertyId.DeviceDataSource, 429, "Vendor-specific address data, such as an OPC data server item ID.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.DeviceDataServer), OpcBatchPropertyId.DeviceDataServer, 430, "Vendor-specific address data, such as an OPC data server name.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.CampaignId), OpcBatchPropertyId.CampaignId, 431, "Production group of which this batch is a member.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.LotIdList), OpcBatchPropertyId.LotIdList, 432, "Strings identifying lots related to this item.", "VT_ARRAY | VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.ControlRecipeId), OpcBatchPropertyId.ControlRecipeId, 433, "Control recipe used for this batch.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.ControlRecipeVersion), OpcBatchPropertyId.ControlRecipeVersion, 434, "Version of the control recipe used.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.MasterRecipeId), OpcBatchPropertyId.MasterRecipeId, 435, "Master recipe used for this batch.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.MasterRecipeVersion), OpcBatchPropertyId.MasterRecipeVersion, 436, "Version of the master recipe used.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.ProductId), OpcBatchPropertyId.ProductId, 437, "Product produced by executing the control or master recipe.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.Grade), OpcBatchPropertyId.Grade, 438, "Grade of material being produced.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.BatchSize), OpcBatchPropertyId.BatchSize, 439, "Application-specific batch-size reference value.", "<varies>")]
    [Arguments(nameof(OpcBatchPropertyId.Priority), OpcBatchPropertyId.Priority, 440, "Relative processing priority, where lower numbers have higher priority.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.ExecutionState), OpcBatchPropertyId.ExecutionState, 441, "Current execution state using the vendor's state names.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.Iec61512State), OpcBatchPropertyId.Iec61512State, 442, "Execution state using IEC 61512-1 example state names, using OPCB_ENUM_STATE.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.ExecutionMode), OpcBatchPropertyId.ExecutionMode, 443, "Current execution mode using the vendor's mode names.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.Iec61512Mode), OpcBatchPropertyId.Iec61512Mode, 444, "Execution mode using IEC 61512-1 example mode names, using OPCB_ENUM_MODE.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.ScheduledStartTime), OpcBatchPropertyId.ScheduledStartTime, 445, "Scheduled start time for the batch or item.", "VT_DATE")]
    [Arguments(nameof(OpcBatchPropertyId.ActualStartTime), OpcBatchPropertyId.ActualStartTime, 446, "Time when the batch or item actually started.", "VT_DATE")]
    [Arguments(nameof(OpcBatchPropertyId.EstimatedEndTime), OpcBatchPropertyId.EstimatedEndTime, 447, "Planned end time for the batch or item.", "VT_DATE")]
    [Arguments(nameof(OpcBatchPropertyId.ActualEndTime), OpcBatchPropertyId.ActualEndTime, 448, "Time when the batch or item actually ended.", "VT_DATE")]
    [Arguments(nameof(OpcBatchPropertyId.PhysicalModelReference), OpcBatchPropertyId.PhysicalModelReference, 449, "Lowest-level physical-model item ID encompassing all equipment for this batch.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.EquipmentProceduralElement), OpcBatchPropertyId.EquipmentProceduralElement, 450, "Equipment procedural element corresponding to this item.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.ParameterCount), OpcBatchPropertyId.ParameterCount, 451, "Number of parameters associated with this item.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.ParameterType), OpcBatchPropertyId.ParameterType, 452, "IEC 61512-1 formula type, using OPCB_ENUM_PARAM.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.ValidValues), OpcBatchPropertyId.ValidValues, 453, "Valid values for the item.", "VT_ARRAY | VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.ScalingRule), OpcBatchPropertyId.ScalingRule, 454, "Special scaling rules for this item.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.ExpressionRule), OpcBatchPropertyId.ExpressionRule, 455, "Indicates whether a string value is a literal or expression.", "VT_BOOL")]
    [Arguments(nameof(OpcBatchPropertyId.ResultCount), OpcBatchPropertyId.ResultCount, 456, "Number of results associated with this item.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.EnumerationSetId), OpcBatchPropertyId.EnumerationSetId, 457, "Vendor-specific enumeration set ID associated with this item's value.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.MasterRecipeModelLevel), OpcBatchPropertyId.MasterRecipeModelLevel, 458, "IEC 61512-1 procedural model level for a master-recipe element, using OPCB_ENUM_MR_PROC.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.ProcedureLogic), OpcBatchPropertyId.ProcedureLogic, 459, "XML data required to recreate the procedure function chart and procedural logic.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.ProcedureLogicSchema), OpcBatchPropertyId.ProcedureLogicSchema, 460, "XML schema URI for the procedure logic property.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.EquipmentCandidateList), OpcBatchPropertyId.EquipmentCandidateList, 461, "Individual equipment that may be used by a control recipe or RPE.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.EquipmentClassCandidateList), OpcBatchPropertyId.EquipmentClassCandidateList, 462, "Equipment classes that may be used by a control recipe or RPE.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.VersionDate), OpcBatchPropertyId.VersionDate, 463, "Date and time this version of the item was last modified.", "VT_DATE")]
    [Arguments(nameof(OpcBatchPropertyId.ApprovalDate), OpcBatchPropertyId.ApprovalDate, 464, "Date and time this version of the item was last approved.", "VT_DATE")]
    [Arguments(nameof(OpcBatchPropertyId.EffectiveDate), OpcBatchPropertyId.EffectiveDate, 465, "Date and time this version of the item is effective.", "VT_DATE")]
    [Arguments(nameof(OpcBatchPropertyId.ExpirationDate), OpcBatchPropertyId.ExpirationDate, 466, "Date and time this version of the item expires.", "VT_DATE")]
    [Arguments(nameof(OpcBatchPropertyId.Author), OpcBatchPropertyId.Author, 467, "Person or system that authored this version of the item.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.ApprovedBy), OpcBatchPropertyId.ApprovedBy, 468, "Person or system that approved this version of the item.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.UsageConstraint), OpcBatchPropertyId.UsageConstraint, 469, "Rules that determine usage constraints for the item.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.RecipeStatus), OpcBatchPropertyId.RecipeStatus, 470, "Status of an item.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.ReUse), OpcBatchPropertyId.ReUse, 471, "Relationship between a recipe element and a class or library entry, using OPCB_ENUM_RE_USE.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.DerivedRe), OpcBatchPropertyId.DerivedRe, 472, "Recipe element from which this recipe element was derived.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.DerivedVersion), OpcBatchPropertyId.DerivedVersion, 473, "Version of the recipe element from which this recipe element was derived.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.Scalable), OpcBatchPropertyId.Scalable, 474, "Identifies whether a parameter is scalable.", "VT_BOOL")]
    [Arguments(nameof(OpcBatchPropertyId.ExpectedDuration), OpcBatchPropertyId.ExpectedDuration, 475, "Expected duration of an item in seconds.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.ActualDuration), OpcBatchPropertyId.ActualDuration, 476, "Actual duration of an item in seconds.", "VT_I4")]
    [Arguments(nameof(OpcBatchPropertyId.TrainList2), OpcBatchPropertyId.TrainList2, 477, "XML document containing processing trains and equipment item IDs.", "VT_BSTR")]
    [Arguments(nameof(OpcBatchPropertyId.TrainList2Schema), OpcBatchPropertyId.TrainList2Schema, 478, "XML schema URI for the TrainList2 property.", "VT_BSTR")]
    public async Task Batch_property_metadata_matches_spec(string propertyName, int actualPropertyId, int expectedPropertyId, string expectedDescription, string expectedVarType)
    {
        await Assert.That(string.IsNullOrWhiteSpace(propertyName)).IsFalse();
        await Assert.That(actualPropertyId).IsEqualTo(expectedPropertyId);
        await Assert.That(OpcBatchPropertyId.GetDescription(actualPropertyId)).IsEqualTo(expectedDescription);
        await Assert.That(OpcBatchPropertyId.GetExpectedVarType(actualPropertyId)).IsEqualTo(expectedVarType);
    }

    [Test]
    public async Task Unknown_property_ids_return_null_metadata()
    {
        await Assert.That(OpcBatchPropertyId.GetDescription(399)).IsNull();
        await Assert.That(OpcBatchPropertyId.GetExpectedVarType(399)).IsNull();
        await Assert.That(OpcBatchPropertyId.GetDescription(479)).IsNull();
        await Assert.That(OpcBatchPropertyId.GetExpectedVarType(479)).IsNull();
    }

    [Test]
    public async Task Batch_property_ids_are_distinct()
    {
        var propertyIds = new[]
        {
            OpcBatchPropertyId.Id,
            OpcBatchPropertyId.Value,
            OpcBatchPropertyId.AccessRights,
            OpcBatchPropertyId.Eu,
            OpcBatchPropertyId.Description,
            OpcBatchPropertyId.HighValueLimit,
            OpcBatchPropertyId.LowValueLimit,
            OpcBatchPropertyId.TimeZone,
            OpcBatchPropertyId.ConditionStatus,
            OpcBatchPropertyId.PhysicalModelLevel,
            OpcBatchPropertyId.BatchModelLevel,
            OpcBatchPropertyId.RelatedBatchIds,
            OpcBatchPropertyId.Version,
            OpcBatchPropertyId.EquipmentClass,
            OpcBatchPropertyId.Location,
            OpcBatchPropertyId.MaximumUserCount,
            OpcBatchPropertyId.CurrentUserCount,
            OpcBatchPropertyId.CurrentUserList,
            OpcBatchPropertyId.AllocatedEquipmentList,
            OpcBatchPropertyId.RequesterList,
            OpcBatchPropertyId.RequestedList,
            OpcBatchPropertyId.SharedByList,
            OpcBatchPropertyId.EquipmentState,
            OpcBatchPropertyId.EquipmentMode,
            OpcBatchPropertyId.UpstreamEquipmentList,
            OpcBatchPropertyId.DownstreamEquipmentList,
            OpcBatchPropertyId.EquipmentProceduralElementList,
            OpcBatchPropertyId.CurrentProcedureList,
            OpcBatchPropertyId.TrainList,
            OpcBatchPropertyId.DeviceDataSource,
            OpcBatchPropertyId.DeviceDataServer,
            OpcBatchPropertyId.CampaignId,
            OpcBatchPropertyId.LotIdList,
            OpcBatchPropertyId.ControlRecipeId,
            OpcBatchPropertyId.ControlRecipeVersion,
            OpcBatchPropertyId.MasterRecipeId,
            OpcBatchPropertyId.MasterRecipeVersion,
            OpcBatchPropertyId.ProductId,
            OpcBatchPropertyId.Grade,
            OpcBatchPropertyId.BatchSize,
            OpcBatchPropertyId.Priority,
            OpcBatchPropertyId.ExecutionState,
            OpcBatchPropertyId.Iec61512State,
            OpcBatchPropertyId.ExecutionMode,
            OpcBatchPropertyId.Iec61512Mode,
            OpcBatchPropertyId.ScheduledStartTime,
            OpcBatchPropertyId.ActualStartTime,
            OpcBatchPropertyId.EstimatedEndTime,
            OpcBatchPropertyId.ActualEndTime,
            OpcBatchPropertyId.PhysicalModelReference,
            OpcBatchPropertyId.EquipmentProceduralElement,
            OpcBatchPropertyId.ParameterCount,
            OpcBatchPropertyId.ParameterType,
            OpcBatchPropertyId.ValidValues,
            OpcBatchPropertyId.ScalingRule,
            OpcBatchPropertyId.ExpressionRule,
            OpcBatchPropertyId.ResultCount,
            OpcBatchPropertyId.EnumerationSetId,
            OpcBatchPropertyId.MasterRecipeModelLevel,
            OpcBatchPropertyId.ProcedureLogic,
            OpcBatchPropertyId.ProcedureLogicSchema,
            OpcBatchPropertyId.EquipmentCandidateList,
            OpcBatchPropertyId.EquipmentClassCandidateList,
            OpcBatchPropertyId.VersionDate,
            OpcBatchPropertyId.ApprovalDate,
            OpcBatchPropertyId.EffectiveDate,
            OpcBatchPropertyId.ExpirationDate,
            OpcBatchPropertyId.Author,
            OpcBatchPropertyId.ApprovedBy,
            OpcBatchPropertyId.UsageConstraint,
            OpcBatchPropertyId.RecipeStatus,
            OpcBatchPropertyId.ReUse,
            OpcBatchPropertyId.DerivedRe,
            OpcBatchPropertyId.DerivedVersion,
            OpcBatchPropertyId.Scalable,
            OpcBatchPropertyId.ExpectedDuration,
            OpcBatchPropertyId.ActualDuration,
            OpcBatchPropertyId.TrainList2,
            OpcBatchPropertyId.TrainList2Schema,
        };

        await Assert.That(propertyIds.Length).IsEqualTo(79);
        await Assert.That(propertyIds.Distinct().Count()).IsEqualTo(propertyIds.Length);
    }
}
