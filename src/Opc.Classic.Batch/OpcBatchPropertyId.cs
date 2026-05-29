//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Batch;

/// <summary>
/// OPC Batch 2.00 DA item property identifiers defined by Appendix A's <c>OPCBatchProps.h</c>.
/// </summary>
public static class OpcBatchPropertyId
{
    private const string VariesVarType = "<varies>";

    /// <summary><c>OPC_PROP_B_ID</c> (400) — item name, equipment ID, batch ID, internal ID, or alias used to build the qualified item ID (VT_BSTR).</summary>
    public const int Id = 400;

    /// <summary><c>OPC_PROP_B_VALUE</c> (401) — item value derived from OPC Data Access property ID 2 (<c>&lt;varies&gt;</c>).</summary>
    public const int Value = 401;

    /// <summary><c>OPC_PROP_B_RIGHTS</c> (402) — item access rights derived from OPC Data Access property ID 5 (VT_I4).</summary>
    public const int AccessRights = 402;

    /// <summary><c>OPC_PROP_B_EU</c> (403) — engineering units derived from OPC Data Access property ID 100 (VT_BSTR).</summary>
    public const int Eu = 403;

    /// <summary><c>OPC_PROP_B_DESC</c> (404) — item description derived from OPC Data Access property ID 101 (VT_BSTR).</summary>
    public const int Description = 404;

    /// <summary><c>OPC_PROP_B_HIGH_VALUE_LIMIT</c> (405) — highest value this item may take on (<c>&lt;varies&gt;</c>).</summary>
    public const int HighValueLimit = 405;

    /// <summary><c>OPC_PROP_B_LOW_VALUE_LIMIT</c> (406) — lowest value this item may take on (<c>&lt;varies&gt;</c>).</summary>
    public const int LowValueLimit = 406;

    /// <summary><c>OPC_PROP_B_TIME_ZONE</c> (407) — item timezone derived from OPC Data Access property ID 108 (VT_I4).</summary>
    public const int TimeZone = 407;

    /// <summary><c>OPC_PROP_B_CONDITION_STATUS</c> (408) — condition status derived from OPC Alarms and Events property ID 300 (VT_BSTR).</summary>
    public const int ConditionStatus = 408;

    /// <summary><c>OPC_PROP_B_PHYSICAL_MODEL_LEVEL</c> (409) — physical model level associated with equipment, using <c>OPCB_ENUM_PHYS</c> (VT_I4).</summary>
    public const int PhysicalModelLevel = 409;

    /// <summary><c>OPC_PROP_B_BATCH_MODEL_LEVEL</c> (410) — IEC 61512-1 procedural model level for a batch or recipe procedural element, using <c>OPCB_ENUM_PROC</c> (VT_I4).</summary>
    public const int BatchModelLevel = 410;

    /// <summary><c>OPC_PROP_B_RELATED_BATCH_IDS</c> (411) — batch ID or batch ID list related to a physical model item (VT_ARRAY | VT_BSTR).</summary>
    public const int RelatedBatchIds = 411;

    /// <summary><c>OPC_PROP_B_VERSION</c> (412) — server-specific version identifier for the associated item (VT_BSTR).</summary>
    public const int Version = 412;

    /// <summary><c>OPC_PROP_B_EQUIPMENT_CLASS</c> (413) — class of the associated equipment, such as reactor or mixer (VT_BSTR).</summary>
    public const int EquipmentClass = 413;

    /// <summary><c>OPC_PROP_B_LOCATION</c> (414) — building or physical location where the item exists (VT_BSTR).</summary>
    public const int Location = 414;

    /// <summary><c>OPC_PROP_B_MAXIMUM_USER_COUNT</c> (415) — maximum concurrent users of the item, with -1 meaning unlimited (VT_I4).</summary>
    public const int MaximumUserCount = 415;

    /// <summary><c>OPC_PROP_B_CURRENT_USER_COUNT</c> (416) — current number of users of the associated item (VT_I4).</summary>
    public const int CurrentUserCount = 416;

    /// <summary><c>OPC_PROP_B_CURRENT_USER_LIST</c> (417) — item IDs that are using the associated item (VT_ARRAY | VT_BSTR).</summary>
    public const int CurrentUserList = 417;

    /// <summary><c>OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST</c> (418) — equipment item IDs allocated by this item (VT_ARRAY | VT_BSTR).</summary>
    public const int AllocatedEquipmentList = 418;

    /// <summary><c>OPC_PROP_B_REQUESTER_LIST</c> (419) — item IDs queued to allocate the associated item, in precedence order (VT_ARRAY | VT_BSTR).</summary>
    public const int RequesterList = 419;

    /// <summary><c>OPC_PROP_B_REQUESTED_LIST</c> (420) — item IDs for which this item has a pending allocation request (VT_ARRAY | VT_BSTR).</summary>
    public const int RequestedList = 420;

    /// <summary><c>OPC_PROP_B_SHARED_BY_LIST</c> (421) — item IDs that can share this item (VT_ARRAY | VT_BSTR).</summary>
    public const int SharedByList = 421;

    /// <summary><c>OPC_PROP_B_EQUIPMENT_STATE</c> (422) — current state of the equipment (VT_BSTR).</summary>
    public const int EquipmentState = 422;

    /// <summary><c>OPC_PROP_B_EQUIPMENT_MODE</c> (423) — current mode of the equipment (VT_BSTR).</summary>
    public const int EquipmentMode = 423;

    /// <summary><c>OPC_PROP_B_UPSTREAM_EQUIPMENT_LIST</c> (424) — equipment item IDs from which material is directly received (VT_ARRAY | VT_BSTR).</summary>
    public const int UpstreamEquipmentList = 424;

    /// <summary><c>OPC_PROP_B_DOWNSTREAM_EQUIPMENT_LIST</c> (425) — equipment item IDs to which material is directly sent (VT_ARRAY | VT_BSTR).</summary>
    public const int DownstreamEquipmentList = 425;

    /// <summary><c>OPC_PROP_B_EQUIPMENT_PROCEDURAL_ELEMENT_LIST</c> (426) — equipment procedural elements this equipment can perform (VT_ARRAY | VT_BSTR).</summary>
    public const int EquipmentProceduralElementList = 426;

    /// <summary><c>OPC_PROP_B_CURRENT_PROCEDURE_LIST</c> (427) — lowest-level active recipe procedural element item IDs on this equipment item (VT_ARRAY | VT_BSTR).</summary>
    public const int CurrentProcedureList = 427;

    /// <summary><c>OPC_PROP_B_TRAIN_LIST</c> (428) — withdrawn two-dimensional processing train list; replaced by TrainList2 (VT_ARRAY | VT_ARRAY | VT_BSTR).</summary>
    public const int TrainList = 428;

    /// <summary><c>OPC_PROP_B_DEVICE_DATA_SOURCE</c> (429) — vendor-specific address data, such as an OPC data server item ID (VT_BSTR).</summary>
    public const int DeviceDataSource = 429;

    /// <summary><c>OPC_PROP_B_DEVICE_DATA_SERVER</c> (430) — vendor-specific address data, such as an OPC data server name (VT_BSTR).</summary>
    public const int DeviceDataServer = 430;

    /// <summary><c>OPC_PROP_B_CAMPAIGN_ID</c> (431) — production group of which this batch is a member (VT_BSTR).</summary>
    public const int CampaignId = 431;

    /// <summary><c>OPC_PROP_B_LOT_ID_LIST</c> (432) — strings identifying lots related to this item (VT_ARRAY | VT_BSTR).</summary>
    public const int LotIdList = 432;

    /// <summary><c>OPC_PROP_B_CONTROL_RECIPE_ID</c> (433) — control recipe used for this batch (VT_BSTR).</summary>
    public const int ControlRecipeId = 433;

    /// <summary><c>OPC_PROP_B_CONTROL_RECIPE_VERSION</c> (434) — version of the control recipe used (VT_BSTR).</summary>
    public const int ControlRecipeVersion = 434;

    /// <summary><c>OPC_PROP_B_MASTER_RECIPE_ID</c> (435) — master recipe used for this batch (VT_BSTR).</summary>
    public const int MasterRecipeId = 435;

    /// <summary><c>OPC_PROP_B_MASTER_RECIPE_VERSION</c> (436) — version of the master recipe used (VT_BSTR).</summary>
    public const int MasterRecipeVersion = 436;

    /// <summary><c>OPC_PROP_B_PRODUCT_ID</c> (437) — product produced by executing the control or master recipe (VT_BSTR).</summary>
    public const int ProductId = 437;

    /// <summary><c>OPC_PROP_B_GRADE</c> (438) — grade of material being produced (VT_BSTR).</summary>
    public const int Grade = 438;

    /// <summary><c>OPC_PROP_B_BATCH_SIZE</c> (439) — application-specific batch-size reference value (<c>&lt;varies&gt;</c>).</summary>
    public const int BatchSize = 439;

    /// <summary><c>OPC_PROP_B_PRIORITY</c> (440) — relative processing priority, where lower numbers have higher priority (VT_I4).</summary>
    public const int Priority = 440;

    /// <summary><c>OPC_PROP_B_EXECUTION_STATE</c> (441) — current execution state using the vendor's state names (VT_BSTR).</summary>
    public const int ExecutionState = 441;

    /// <summary><c>OPC_PROP_B_IEC61512_1_STATE</c> (442) — execution state using IEC 61512-1 example state names, using <c>OPCB_ENUM_STATE</c> (VT_I4).</summary>
    public const int Iec61512State = 442;

    /// <summary><c>OPC_PROP_B_EXECUTION_MODE</c> (443) — current execution mode using the vendor's mode names (VT_BSTR).</summary>
    public const int ExecutionMode = 443;

    /// <summary><c>OPC_PROP_B_IEC61512_1_MODE</c> (444) — execution mode using IEC 61512-1 example mode names, using <c>OPCB_ENUM_MODE</c> (VT_I4).</summary>
    public const int Iec61512Mode = 444;

    /// <summary><c>OPC_PROP_B_SCHEDULED_START_TIME</c> (445) — scheduled start time for the batch or item (VT_DATE).</summary>
    public const int ScheduledStartTime = 445;

    /// <summary><c>OPC_PROP_B_ACTUAL_START_TIME</c> (446) — time when the batch or item actually started (VT_DATE).</summary>
    public const int ActualStartTime = 446;

    /// <summary><c>OPC_PROP_B_ESTIMATED_END_TIME</c> (447) — planned end time for the batch or item (VT_DATE).</summary>
    public const int EstimatedEndTime = 447;

    /// <summary><c>OPC_PROP_B_ACTUAL_END_TIME</c> (448) — time when the batch or item actually ended (VT_DATE).</summary>
    public const int ActualEndTime = 448;

    /// <summary><c>OPC_PROP_B_PHYSICAL_MODEL_REFERENCE</c> (449) — lowest-level physical-model item ID encompassing all equipment for this batch (VT_BSTR).</summary>
    public const int PhysicalModelReference = 449;

    /// <summary><c>OPC_PROP_B_EQUIPMENT_PROCEDURAL_ELEMENT</c> (450) — equipment procedural element corresponding to this item (VT_BSTR).</summary>
    public const int EquipmentProceduralElement = 450;

    /// <summary><c>OPC_PROP_B_PARAMETER_COUNT</c> (451) — number of parameters associated with this item (VT_I4).</summary>
    public const int ParameterCount = 451;

    /// <summary><c>OPC_PROP_B_PARAMETER_TYPE</c> (452) — IEC 61512-1 formula type, using <c>OPCB_ENUM_PARAM</c> (VT_I4).</summary>
    public const int ParameterType = 452;

    /// <summary><c>OPC_PROP_B_VALID_VALUES</c> (453) — valid values for the item (VT_ARRAY | VT_BSTR).</summary>
    public const int ValidValues = 453;

    /// <summary><c>OPC_PROP_B_SCALING_RULE</c> (454) — special scaling rules for this item (VT_BSTR).</summary>
    public const int ScalingRule = 454;

    /// <summary><c>OPC_PROP_B_EXPRESSION_RULE</c> (455) — indicates whether a string value is a literal or expression (VT_BOOL).</summary>
    public const int ExpressionRule = 455;

    /// <summary><c>OPC_PROP_B_RESULT_COUNT</c> (456) — number of results associated with this item (VT_I4).</summary>
    public const int ResultCount = 456;

    /// <summary><c>OPC_PROP_B_ENUMERATION_SET_ID</c> (457) — vendor-specific enumeration set ID associated with this item's value (VT_I4).</summary>
    public const int EnumerationSetId = 457;

    /// <summary><c>OPC_PROP_B_MASTER_RECIPE_MODEL_LEVEL</c> (458) — IEC 61512-1 procedural model level for a master-recipe element, using <c>OPCB_ENUM_MR_PROC</c> (VT_I4).</summary>
    public const int MasterRecipeModelLevel = 458;

    /// <summary><c>OPC_PROP_B_PROCEDURE_LOGIC</c> (459) — XML data required to recreate the procedure function chart and procedural logic (VT_BSTR).</summary>
    public const int ProcedureLogic = 459;

    /// <summary><c>OPC_PROP_B_PROCEDURE_LOGIC_SCHEMA</c> (460) — XML schema URI for the procedure logic property (VT_BSTR).</summary>
    public const int ProcedureLogicSchema = 460;

    /// <summary><c>OPC_PROP_B_EQUIPMENT_CANDIDATE_LIST</c> (461) — individual equipment that may be used by a control recipe or RPE (VT_BSTR).</summary>
    public const int EquipmentCandidateList = 461;

    /// <summary><c>OPC_PROP_B_EQUIPMENT_CLASS_CANDIDATE_LIST</c> (462) — equipment classes that may be used by a control recipe or RPE (VT_BSTR).</summary>
    public const int EquipmentClassCandidateList = 462;

    /// <summary><c>OPC_PROP_B_VERSION_DATE</c> (463) — date and time this version of the item was last modified (VT_DATE).</summary>
    public const int VersionDate = 463;

    /// <summary><c>OPC_PROP_B_APPROVAL_DATE</c> (464) — date and time this version of the item was last approved (VT_DATE).</summary>
    public const int ApprovalDate = 464;

    /// <summary><c>OPC_PROP_B_EFFECTIVE_DATE</c> (465) — date and time this version of the item is effective (VT_DATE).</summary>
    public const int EffectiveDate = 465;

    /// <summary><c>OPC_PROP_B_EXPIRATION_DATE</c> (466) — date and time this version of the item expires (VT_DATE).</summary>
    public const int ExpirationDate = 466;

    /// <summary><c>OPC_PROP_B_AUTHOR</c> (467) — person or system that authored this version of the item (VT_BSTR).</summary>
    public const int Author = 467;

    /// <summary><c>OPC_PROP_B_APPROVED_BY</c> (468) — person or system that approved this version of the item (VT_BSTR).</summary>
    public const int ApprovedBy = 468;

    /// <summary><c>OPC_PROP_B_USAGE_CONSTRAINT</c> (469) — rules that determine usage constraints for the item (VT_BSTR).</summary>
    public const int UsageConstraint = 469;

    /// <summary><c>OPC_PROP_B_RECIPE_STATUS</c> (470) — status of an item (VT_BSTR).</summary>
    public const int RecipeStatus = 470;

    /// <summary><c>OPC_PROP_B_RE_USE</c> (471) — relationship between a recipe element and a class or library entry, using <c>OPCB_ENUM_RE_USE</c> (VT_I4).</summary>
    public const int ReUse = 471;

    /// <summary><c>OPC_PROP_B_DERIVED_RE</c> (472) — recipe element from which this recipe element was derived (VT_BSTR).</summary>
    public const int DerivedRe = 472;

    /// <summary><c>OPC_PROP_B_DERIVED_VERSION</c> (473) — version of the recipe element from which this recipe element was derived (VT_BSTR).</summary>
    public const int DerivedVersion = 473;

    /// <summary><c>OPC_PROP_B_SCALABLE</c> (474) — identifies whether a parameter is scalable (VT_BOOL).</summary>
    public const int Scalable = 474;

    /// <summary><c>OPC_PROP_B_EXPECTED_DURATION</c> (475) — expected duration of an item in seconds (VT_I4).</summary>
    public const int ExpectedDuration = 475;

    /// <summary><c>OPC_PROP_B_ACTUAL_DURATION</c> (476) — actual duration of an item in seconds (VT_I4).</summary>
    public const int ActualDuration = 476;

    /// <summary><c>OPC_PROP_B_TRAIN_LIST2</c> (477) — XML document containing processing trains and equipment item IDs (VT_BSTR).</summary>
    public const int TrainList2 = 477;

    /// <summary><c>OPC_PROP_B_TRAIN_LIST2_SCHEMA</c> (478) — XML schema URI for the TrainList2 property (VT_BSTR).</summary>
    public const int TrainList2Schema = 478;

    /// <summary>
    /// Gets the OPC Batch 2.00 standard description for a DA item property ID.
    /// </summary>
    /// <param name="propertyId">The Batch property ID.</param>
    /// <returns>The standard description, or <see langword="null"/> when <paramref name="propertyId"/> is not a Batch-defined property ID.</returns>
    public static string? GetDescription(int propertyId) => propertyId switch
    {
        >= Id and <= BatchSize => GetDescription400To439(propertyId),
        >= Priority and <= TrainList2Schema => GetDescription440To478(propertyId),
        _ => null,
    };

    private static string? GetDescription400To439(int propertyId) => propertyId switch
    {
        Id => "Item name, equipment ID, batch ID, internal ID, or alias used to build the qualified item ID.",
        Value => "Item value derived from OPC Data Access property ID 2.",
        AccessRights => "Item access rights derived from OPC Data Access property ID 5.",
        Eu => "Engineering units derived from OPC Data Access property ID 100.",
        Description => "Item description derived from OPC Data Access property ID 101.",
        HighValueLimit => "Highest value this item may take on.",
        LowValueLimit => "Lowest value this item may take on.",
        TimeZone => "Item timezone derived from OPC Data Access property ID 108.",
        ConditionStatus => "Condition status derived from OPC Alarms and Events property ID 300.",
        PhysicalModelLevel => "Physical model level associated with equipment, using OPCB_ENUM_PHYS.",
        BatchModelLevel => "IEC 61512-1 procedural model level for a batch or recipe procedural element, using OPCB_ENUM_PROC.",
        RelatedBatchIds => "Batch ID or batch ID list related to a physical model item.",
        Version => "Server-specific version identifier for the associated item.",
        EquipmentClass => "Class of the associated equipment, such as reactor or mixer.",
        Location => "Building or physical location where the item exists.",
        MaximumUserCount => "Maximum concurrent users of the item, with -1 meaning unlimited.",
        CurrentUserCount => "Current number of users of the associated item.",
        CurrentUserList => "Item IDs that are using the associated item.",
        AllocatedEquipmentList => "Equipment item IDs allocated by this item.",
        RequesterList => "Item IDs queued to allocate the associated item, in precedence order.",
        RequestedList => "Item IDs for which this item has a pending allocation request.",
        SharedByList => "Item IDs that can share this item.",
        EquipmentState => "Current state of the equipment.",
        EquipmentMode => "Current mode of the equipment.",
        UpstreamEquipmentList => "Equipment item IDs from which material is directly received.",
        DownstreamEquipmentList => "Equipment item IDs to which material is directly sent.",
        EquipmentProceduralElementList => "Equipment procedural elements this equipment can perform.",
        CurrentProcedureList => "Lowest-level active recipe procedural element item IDs on this equipment item.",
        TrainList => "Withdrawn two-dimensional processing train list; replaced by TrainList2.",
        DeviceDataSource => "Vendor-specific address data, such as an OPC data server item ID.",
        DeviceDataServer => "Vendor-specific address data, such as an OPC data server name.",
        CampaignId => "Production group of which this batch is a member.",
        LotIdList => "Strings identifying lots related to this item.",
        ControlRecipeId => "Control recipe used for this batch.",
        ControlRecipeVersion => "Version of the control recipe used.",
        MasterRecipeId => "Master recipe used for this batch.",
        MasterRecipeVersion => "Version of the master recipe used.",
        ProductId => "Product produced by executing the control or master recipe.",
        Grade => "Grade of material being produced.",
        BatchSize => "Application-specific batch-size reference value.",
        _ => null,
    };

    private static string? GetDescription440To478(int propertyId) => propertyId switch
    {
        Priority => "Relative processing priority, where lower numbers have higher priority.",
        ExecutionState => "Current execution state using the vendor's state names.",
        Iec61512State => "Execution state using IEC 61512-1 example state names, using OPCB_ENUM_STATE.",
        ExecutionMode => "Current execution mode using the vendor's mode names.",
        Iec61512Mode => "Execution mode using IEC 61512-1 example mode names, using OPCB_ENUM_MODE.",
        ScheduledStartTime => "Scheduled start time for the batch or item.",
        ActualStartTime => "Time when the batch or item actually started.",
        EstimatedEndTime => "Planned end time for the batch or item.",
        ActualEndTime => "Time when the batch or item actually ended.",
        PhysicalModelReference => "Lowest-level physical-model item ID encompassing all equipment for this batch.",
        EquipmentProceduralElement => "Equipment procedural element corresponding to this item.",
        ParameterCount => "Number of parameters associated with this item.",
        ParameterType => "IEC 61512-1 formula type, using OPCB_ENUM_PARAM.",
        ValidValues => "Valid values for the item.",
        ScalingRule => "Special scaling rules for this item.",
        ExpressionRule => "Indicates whether a string value is a literal or expression.",
        ResultCount => "Number of results associated with this item.",
        EnumerationSetId => "Vendor-specific enumeration set ID associated with this item's value.",
        MasterRecipeModelLevel => "IEC 61512-1 procedural model level for a master-recipe element, using OPCB_ENUM_MR_PROC.",
        ProcedureLogic => "XML data required to recreate the procedure function chart and procedural logic.",
        ProcedureLogicSchema => "XML schema URI for the procedure logic property.",
        EquipmentCandidateList => "Individual equipment that may be used by a control recipe or RPE.",
        EquipmentClassCandidateList => "Equipment classes that may be used by a control recipe or RPE.",
        VersionDate => "Date and time this version of the item was last modified.",
        ApprovalDate => "Date and time this version of the item was last approved.",
        EffectiveDate => "Date and time this version of the item is effective.",
        ExpirationDate => "Date and time this version of the item expires.",
        Author => "Person or system that authored this version of the item.",
        ApprovedBy => "Person or system that approved this version of the item.",
        UsageConstraint => "Rules that determine usage constraints for the item.",
        RecipeStatus => "Status of an item.",
        ReUse => "Relationship between a recipe element and a class or library entry, using OPCB_ENUM_RE_USE.",
        DerivedRe => "Recipe element from which this recipe element was derived.",
        DerivedVersion => "Version of the recipe element from which this recipe element was derived.",
        Scalable => "Identifies whether a parameter is scalable.",
        ExpectedDuration => "Expected duration of an item in seconds.",
        ActualDuration => "Actual duration of an item in seconds.",
        TrainList2 => "XML document containing processing trains and equipment item IDs.",
        TrainList2Schema => "XML schema URI for the TrainList2 property.",
        _ => null,
    };

    /// <summary>
    /// Gets the OPC Batch 2.00 expected VARTYPE for a DA item property ID.
    /// </summary>
    /// <param name="propertyId">The Batch property ID.</param>
    /// <returns>The VARTYPE expression from the specification, or <see langword="null"/> when <paramref name="propertyId"/> is not a Batch-defined property ID.</returns>
    public static string? GetExpectedVarType(int propertyId) => propertyId switch
    {
        >= Id and <= BatchSize => GetExpectedVarType400To439(propertyId),
        >= Priority and <= TrainList2Schema => GetExpectedVarType440To478(propertyId),
        _ => null,
    };

    private static string? GetExpectedVarType400To439(int propertyId) => propertyId switch
    {
        Id => "VT_BSTR",
        Value => VariesVarType,
        AccessRights => "VT_I4",
        Eu => "VT_BSTR",
        Description => "VT_BSTR",
        HighValueLimit => VariesVarType,
        LowValueLimit => VariesVarType,
        TimeZone => "VT_I4",
        ConditionStatus => "VT_BSTR",
        PhysicalModelLevel => "VT_I4",
        BatchModelLevel => "VT_I4",
        RelatedBatchIds => "VT_ARRAY | VT_BSTR",
        Version => "VT_BSTR",
        EquipmentClass => "VT_BSTR",
        Location => "VT_BSTR",
        MaximumUserCount => "VT_I4",
        CurrentUserCount => "VT_I4",
        CurrentUserList => "VT_ARRAY | VT_BSTR",
        AllocatedEquipmentList => "VT_ARRAY | VT_BSTR",
        RequesterList => "VT_ARRAY | VT_BSTR",
        RequestedList => "VT_ARRAY | VT_BSTR",
        SharedByList => "VT_ARRAY | VT_BSTR",
        EquipmentState => "VT_BSTR",
        EquipmentMode => "VT_BSTR",
        UpstreamEquipmentList => "VT_ARRAY | VT_BSTR",
        DownstreamEquipmentList => "VT_ARRAY | VT_BSTR",
        EquipmentProceduralElementList => "VT_ARRAY | VT_BSTR",
        CurrentProcedureList => "VT_ARRAY | VT_BSTR",
        TrainList => "VT_ARRAY | VT_ARRAY | VT_BSTR",
        DeviceDataSource => "VT_BSTR",
        DeviceDataServer => "VT_BSTR",
        CampaignId => "VT_BSTR",
        LotIdList => "VT_ARRAY | VT_BSTR",
        ControlRecipeId => "VT_BSTR",
        ControlRecipeVersion => "VT_BSTR",
        MasterRecipeId => "VT_BSTR",
        MasterRecipeVersion => "VT_BSTR",
        ProductId => "VT_BSTR",
        Grade => "VT_BSTR",
        BatchSize => VariesVarType,
        _ => null,
    };

    private static string? GetExpectedVarType440To478(int propertyId) => propertyId switch
    {
        Priority => "VT_I4",
        ExecutionState => "VT_BSTR",
        Iec61512State => "VT_I4",
        ExecutionMode => "VT_BSTR",
        Iec61512Mode => "VT_I4",
        ScheduledStartTime => "VT_DATE",
        ActualStartTime => "VT_DATE",
        EstimatedEndTime => "VT_DATE",
        ActualEndTime => "VT_DATE",
        PhysicalModelReference => "VT_BSTR",
        EquipmentProceduralElement => "VT_BSTR",
        ParameterCount => "VT_I4",
        ParameterType => "VT_I4",
        ValidValues => "VT_ARRAY | VT_BSTR",
        ScalingRule => "VT_BSTR",
        ExpressionRule => "VT_BOOL",
        ResultCount => "VT_I4",
        EnumerationSetId => "VT_I4",
        MasterRecipeModelLevel => "VT_I4",
        ProcedureLogic => "VT_BSTR",
        ProcedureLogicSchema => "VT_BSTR",
        EquipmentCandidateList => "VT_BSTR",
        EquipmentClassCandidateList => "VT_BSTR",
        VersionDate => "VT_DATE",
        ApprovalDate => "VT_DATE",
        EffectiveDate => "VT_DATE",
        ExpirationDate => "VT_DATE",
        Author => "VT_BSTR",
        ApprovedBy => "VT_BSTR",
        UsageConstraint => "VT_BSTR",
        RecipeStatus => "VT_BSTR",
        ReUse => "VT_I4",
        DerivedRe => "VT_BSTR",
        DerivedVersion => "VT_BSTR",
        Scalable => "VT_BOOL",
        ExpectedDuration => "VT_I4",
        ActualDuration => "VT_I4",
        TrainList2 => "VT_BSTR",
        TrainList2Schema => "VT_BSTR",
        _ => null,
    };

}
