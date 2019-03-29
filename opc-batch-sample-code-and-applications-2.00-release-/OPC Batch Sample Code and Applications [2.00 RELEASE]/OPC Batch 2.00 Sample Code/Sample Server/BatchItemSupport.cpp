// BatchItemSupport.cpp
//
// (c) Copyright 2000 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Batch Specification and may be used
//  as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// CREDITS:
//  This code was generously provided to the OPC Foundation by
//  Will Irwin, Fisher-Rosemount Systems Inc., 
//  Gregory Khrapunovich, ABB Inc.
//  Dave Emerson, Yokogawa Corporation of America
//  James Wang, Yokogawa Corporation of America: Added XML file interface.
//
// CONTENTS:
//
//  This file contains the code to define the properties and 
//  support manipulation of the simulated data for the OPC Batch 
//  Sample server.
// 
#pragma warning( disable : 4786 ) //Disable warning messages from STL map

#include "OPCxxx.h"
#include "OPCProps.h"
#include "OPCBatchProps.h"
#include "OPCBatchDef.h"
#include "OPCError.h"

#include <map>
#include <fstream>
#include <iostream>
#include <string>
#include "OPCBXMLData.h"

#include <assert.h>
#include <stdio.h>

extern void CreateSamplePhysObjects();
extern void CreateSampleBatchObjects();
extern void CreateSampleRPEObjects();
extern void CreateSampleRootObjects();

static OPCBObj* LookupObjectByItemID( LPCWSTR pszItem );

// Set up the available property tables for each
// type of batch object.  See OPC Batch Specification
// 1.0 Table 10.

PROPERTY OPCBBatchObj::M_PropsList[] = 
{
  // NOTE: Make sure that the following list of parameter
  // definitions, a default value is being set up in the 
  // OPCBBatchObj constructor
  
  // Include the OPC Data Access 'set 1' props
  // Section 4.4.6 in version 2.0 of that spec
  OPC_PROP_CDT,       VT_I2,        L"CDT",         L"Item Canonical Data Type",
  OPC_PROP_VALUE,     VT_VARIANT,   L"Value",       L"Item Value",
  OPC_PROP_QUALITY,   VT_I2,        L"QUALITY",     L"Item Quality",
  OPC_PROP_time,      VT_DATE,      L"Timestamp",   L"Timestamp",
  OPC_PROP_RIGHTS,    VT_I2,        L"AccessRights", L"Item Access Rights",
  OPC_PROP_SCANRATE,  VT_R4,        L"SCANRATE",    L"Server Scan Rate",
  
  OPC_PROP_UNIT,      VT_BSTR,      L"UNIT",        L"Engineering Units",
  OPC_PROP_DESC,      VT_BSTR,      L"DESCRIPTION", L"Item Description",
  
  // Not including the other OPC Data Access 'set 2 - recommended'
  // properties, though it could easily be done by adding
  // them here.
  
  // Include the OPC Batch 'required' props for Batch objects
  // Could easily insert some or all of the optional ones 
  // here if desired.
  
  OPC_PROP_B_ID,                VT_BSTR,      L"ID",           L"ID",
  OPC_PROP_B_VALUE,             VT_VARIANT,     L"Value",          L"Item Value",
  OPC_PROP_B_RIGHTS,            VT_I4,        L"AccessRights",     L"AccessRights",
  OPC_PROP_B_EU,                VT_BSTR,      L"EU",           L"EU",
  OPC_PROP_B_DESC,              VT_BSTR,      L"Description",      L"Description",
  OPC_PROP_B_HIGH_VALUE_LIMIT,  VT_VARIANT,     L"HighValueLimit",     L"HighValueLimit",
  OPC_PROP_B_LOW_VALUE_LIMIT, VT_VARIANT,     L"LowValueLimit",      L"LowValueLimit",
  OPC_PROP_B_TIME_ZONE,     VT_I4,        L"TimeZone",       L"TimeZone",
  OPC_PROP_B_CONDITION_STATUS,  VT_BSTR,      L"ConditionStatus",    L"ConditionStatus",
  
  OPC_PROP_B_BATCH_MODEL_LEVEL, VT_I4,        L"OPCBBatchModelLevel",  L"OPCBBatchModelLevel",
  
  OPC_PROP_B_VERSION,     VT_BSTR,      L"Version",        L"Version",
  
  OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST,   VT_ARRAY | VT_BSTR, L"AllocatedEquipmentList", L"Allocated Equipment List",
  OPC_PROP_B_REQUESTER_LIST,  VT_ARRAY | VT_BSTR, L"RequesterList",      L"RequesterList",
  OPC_PROP_B_REQUESTED_LIST,  VT_ARRAY | VT_BSTR, L"RequestedList",      L"RequestedList",
  OPC_PROP_B_SHARED_BY_LIST,  VT_ARRAY | VT_BSTR, L"SharedByList",     L"SharedByList",
  
  OPC_PROP_B_CAMPAIGN_ID,       VT_BSTR,      L"CampaignID",       L"CampaignID",
  OPC_PROP_B_LOT_ID_LIST,       VT_ARRAY | VT_BSTR, L"LotIDList",        L"LotIDList",
  OPC_PROP_B_CONTROL_RECIPE_ID, VT_BSTR,      L"ControlRecipeID",    L"ControlRecipeID",
  OPC_PROP_B_CONTROL_RECIPE_VERSION,  VT_BSTR,      L"ControlRecipeVersion", L"ControlRecipeVersion",
  OPC_PROP_B_MASTER_RECIPE_ID,  VT_BSTR,      L"MasterRecipeID",     L"MasterRecipeID",
  OPC_PROP_B_MASTER_RECIPE_VERSION,   VT_BSTR,      L"MasterRecipeVersion",  L"MasterRecipeVersion",
  OPC_PROP_B_PRODUCT_ID,        VT_BSTR,      L"ProductID",        L"ProductID",
  OPC_PROP_B_GRADE,             VT_BSTR,      L"Grade",          L"Grade",
  OPC_PROP_B_BATCH_SIZE,        VT_VARIANT,     L"BatchSize",        L"BatchSize",
  OPC_PROP_B_PRIORITY,          VT_I4,        L"Priority",       L"Priority",
  OPC_PROP_B_EXECUTION_STATE,   VT_BSTR,      L"ExecutionState",     L"ExecutionState",
  OPC_PROP_B_IEC61512_1_STATE,  VT_I4,        L"IEC61512-1State",    L"IEC61512-1State",
  OPC_PROP_B_EXECUTION_MODE,    VT_BSTR,      L"ExecutionMode",      L"ExecutionMode",
  OPC_PROP_B_IEC61512_1_MODE,   VT_I4,        L"IEC61512-1Mode",     L"IEC61512-1Mode",
  OPC_PROP_B_SCHEDULED_START_TIME,  VT_DATE,      L"ScheduledStartTime",   L"ScheduledStartTime",
  OPC_PROP_B_ACTUAL_START_TIME, VT_DATE,      L"ActualStartTime",    L"ActualStartTime",
  OPC_PROP_B_ESTIMATED_END_TIME,    VT_DATE,      L"EstimatedEndTime",   L"EstimatedEndTime",
  OPC_PROP_B_ACTUAL_END_TIME,   VT_DATE,      L"ActualEndTime",      L"ActualEndTime",
  OPC_PROP_B_PHYSICAL_MODEL_REFERENCE, VT_BSTR,     L"OPCBPhysicalModelReference", 
    L"OPCBPhysicalModelReference",
  OPC_PROP_B_EQUIPMENT_PROCEDURAL_ELEMENT,  VT_BSTR,      L"EquipmentProceduralElement", 
    L"EquipmentProceduralElement",
  OPC_PROP_B_PARAMETER_COUNT,   VT_I4,          L"ParameterCount",     L"ParameterCount",
  OPC_PROP_B_VALID_VALUES,      VT_ARRAY | VT_BSTR, L"ValidValues",      L"ValidValues",
  OPC_PROP_B_SCALING_RULE,      VT_BSTR,      L"ScalingRule",      L"ScalingRule",
  OPC_PROP_B_EXPRESSION_RULE,   VT_BOOL,    L"ExpressionRule",     L"ExpressionRule",
  OPC_PROP_B_RESULT_COUNT,      VT_I4,        L"ResultCount",      L"ResultCount",
  OPC_PROP_B_ENUMERATION_SET_ID,   VT_I4,        L"EnumerationSetID",   L"EnumerationSetID",
  OPC_PROP_B_PROCEDURE_LOGIC, VT_BSTR,    L"ProcedureLogic",   L"ProcedureLogic",
  OPC_PROP_B_PROCEDURE_LOGIC_SCHEMA, VT_BSTR, L"ProcedureLogicSchema",   L"ProcedureLogicSchema",
  OPC_PROP_B_EQUIPMENT_CANDIDATE_LIST,  VT_ARRAY | VT_BSTR, L"EquipmentCandidateList",     L"EquipmentCandidateList",
  OPC_PROP_B_EQUIPMENT_CLASS_CANDIDATE_LIST,  VT_ARRAY | VT_BSTR, L"EquipmentClassCandidateList",  
    L"EquipmentClassCandidateList",
  OPC_PROP_B_VERSION_DATE,    VT_DATE,      L"VersionDate",      L"VersionDate",
  OPC_PROP_B_APPROVAL_DATE, VT_DATE,      L"ApprovalDate",     L"ApprovalDate",
  OPC_PROP_B_AUTHOR,        VT_BSTR,      L"Author",         L"Author",
  OPC_PROP_B_APPROVED_BY,   VT_BSTR,      L"ApprovedBy",     L"ApprovedBy",
  OPC_PROP_B_USAGE_CONSTRAINT,VT_BSTR,      L"UsageConstraint",    L"UsageConstraint",
  OPC_PROP_B_RECIPE_STATUS,   VT_BSTR,      L"RecipeStatus",     L"RecipeStatus",
  OPC_PROP_B_SCALABLE,      VT_BOOL,      L"Scalable",       L"Scalable",
  OPC_PROP_B_EXPECTED_DURATION, VT_I4,    L"ExpectedDuration", L"ExpectedDuration",
  OPC_PROP_B_ACTUAL_DURATION,   VT_I4,    L"ActualDuration",   L"ActualDuration",
    0,                            0,             NULL,                 NULL
};

// Constructors for the derived classes will set up 
// a default value for every property that they support.
OPCBBatchObj::OPCBBatchObj()
{
	
	// NOTE: Keep the following initializers in sync with ths
	// list of property definitions in OPCBBatchObj::M_PropsList
	
	SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
	SetPropertyAtID(OPC_PROP_VALUE, L"");
	SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
	
	
	FILETIME ftNow;     // this effectively means 'server startup time')
	CoFileTimeNow ( &ftNow );
	SetPropertyAtID(OPC_PROP_time, ftNow);
	
	SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
	SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
	SetPropertyAtID(OPC_PROP_UNIT, L"");
	SetPropertyAtID(OPC_PROP_DESC, L"");
	
	SetPropertyAtID(OPC_PROP_B_ID, L"");
	SetPropertyAtID(OPC_PROP_B_VALUE, L"");
	SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
	SetPropertyAtID(OPC_PROP_B_EU, L"");
	SetPropertyAtID(OPC_PROP_B_DESC, L"");
	SetPropertyAtID(OPC_PROP_B_HIGH_VALUE_LIMIT, L"");
	SetPropertyAtID(OPC_PROP_B_LOW_VALUE_LIMIT, L"");
	SetPropertyAtID(OPC_PROP_B_TIME_ZONE, L"");
	SetPropertyAtID(OPC_PROP_B_CONDITION_STATUS, L"");
	SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, (DWORD)0);  //zero as spec requires for a batch
	SetPropertyAtID(OPC_PROP_B_VERSION, L"");
	
	LPCWSTR arrString[] = { L"" }; 
	SAFEARRAY *pArr, *pArr2, *pArr3, *pArr4, *pArr5, *pArr6, *pArr7;
	
	CreateSafeArrayFromStrings(arrString, 1, pArr);
	SetPropertyAtID(OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST, pArr);
	
	CreateSafeArrayFromStrings(arrString, 1, pArr2);
	SetPropertyAtID(OPC_PROP_B_REQUESTER_LIST, pArr2);
	
	CreateSafeArrayFromStrings(arrString, 1, pArr3);
	SetPropertyAtID(OPC_PROP_B_REQUESTED_LIST, pArr3);
	
	CreateSafeArrayFromStrings(arrString, 1, pArr4);
	SetPropertyAtID(OPC_PROP_B_SHARED_BY_LIST, pArr4);
	
	SetPropertyAtID(OPC_PROP_B_CAMPAIGN_ID, L"");
	
	CreateSafeArrayFromStrings(arrString, 1, pArr5);
	SetPropertyAtID(OPC_PROP_B_LOT_ID_LIST, pArr5);
	
	SetPropertyAtID(OPC_PROP_B_CONTROL_RECIPE_ID, L"");
	SetPropertyAtID(OPC_PROP_B_CONTROL_RECIPE_VERSION, L"");
	SetPropertyAtID(OPC_PROP_B_MASTER_RECIPE_ID, L"");
	SetPropertyAtID(OPC_PROP_B_MASTER_RECIPE_VERSION, L"");
	SetPropertyAtID(OPC_PROP_B_PRODUCT_ID, L"");
	SetPropertyAtID(OPC_PROP_B_GRADE, L"");
	SetPropertyAtID(OPC_PROP_B_BATCH_SIZE, L"");
	SetPropertyAtID(OPC_PROP_B_PRIORITY, L"");
	
	SetPropertyAtID(OPC_PROP_B_EXECUTION_STATE, L"");
	SetPropertyAtID(OPC_PROP_B_IEC61512_1_STATE, (DWORD) OPCB_STATE_UNKNOWN);  // client can use enumeration..
	SetPropertyAtID(OPC_PROP_B_EXECUTION_MODE, L"");
	SetPropertyAtID(OPC_PROP_B_IEC61512_1_MODE, (DWORD) OPCB_MODE_UNKNOWN);  // client can use enumeration..
	
	FILETIME ftDef;
	ftDef.dwLowDateTime = 0;
	ftDef.dwHighDateTime = 0;
	SetPropertyAtID(OPC_PROP_B_SCHEDULED_START_TIME, ftDef);
	SetPropertyAtID(OPC_PROP_B_ACTUAL_START_TIME, ftDef);
	SetPropertyAtID(OPC_PROP_B_ESTIMATED_END_TIME, ftDef);
	SetPropertyAtID(OPC_PROP_B_ACTUAL_END_TIME, ftDef);
	
	SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_REFERENCE, L"0");
	SetPropertyAtID(OPC_PROP_B_EQUIPMENT_PROCEDURAL_ELEMENT, L"");
	SetPropertyAtID(OPC_PROP_B_PARAMETER_COUNT, L"0");
	
	CreateSafeArrayFromStrings(arrString, 1, pArr6);
	SetPropertyAtID(OPC_PROP_B_VALID_VALUES, pArr6);
	
	SetPropertyAtID(OPC_PROP_B_SCALING_RULE, L"");
	SetPropertyAtID(OPC_PROP_B_RESULT_COUNT, L"0");
	SetPropertyAtID(OPC_PROP_B_ENUMERATION_SET_ID, (DWORD) -1); // 'value' property doesn't use an enum set
	
	//V2Draft3:
	SetPropertyAtID(OPC_PROP_B_PROCEDURE_LOGIC, L"");
	SetPropertyAtID(OPC_PROP_B_PROCEDURE_LOGIC_SCHEMA, L"");
	CreateSafeArrayFromStrings(arrString, 1, pArr6);
	SetPropertyAtID(OPC_PROP_B_EQUIPMENT_CANDIDATE_LIST, pArr6);
	CreateSafeArrayFromStrings(arrString, 1, pArr7);
	SetPropertyAtID(OPC_PROP_B_EQUIPMENT_CLASS_CANDIDATE_LIST, pArr7);
	
	SetPropertyAtID(OPC_PROP_B_VERSION_DATE, ftDef);
	SetPropertyAtID(OPC_PROP_B_APPROVAL_DATE, ftDef);
	SetPropertyAtID(OPC_PROP_B_AUTHOR, L"");
	SetPropertyAtID(OPC_PROP_B_APPROVED_BY, L"");
	SetPropertyAtID(OPC_PROP_B_USAGE_CONSTRAINT, L"");
	SetPropertyAtID(OPC_PROP_B_RECIPE_STATUS, L"");
	SetPropertyAtID(OPC_PROP_B_SCALABLE,	 L"" );
	SetPropertyAtID(OPC_PROP_B_EXPECTED_DURATION,	(DWORD)0);
	SetPropertyAtID(OPC_PROP_B_ACTUAL_DURATION, (DWORD)0);
}

PROPERTY OPCBPhysObj::M_PropsList[] = 
{
	// NOTE: Make sure that the following list of parameter
	// definitions, a default value is being set up in the 
	// OPCBPhysObj constructor
	
	// Include the OPC Data Access 'set 1' props
	// Section 4.4.6 in version 2.0 of that spec
	OPC_PROP_CDT,       VT_I2,        L"CDT",         L"Item Canonical Data Type",
		OPC_PROP_VALUE,     VT_VARIANT,   L"Value",       L"Item Value",
		OPC_PROP_QUALITY,   VT_I2,        L"QUALITY",     L"Item Quality",
		OPC_PROP_RIGHTS,    VT_I2,        L"AccessRights", L"Item Access Rights",
		OPC_PROP_SCANRATE,  VT_R4,        L"SCANRATE",    L"Server Scan Rate",		
		OPC_PROP_DESC,      VT_BSTR,      L"DESCRIPTION", L"Item Description",
		
		// Not including the other OPC Data Access 'set 2 - recommended'
		// properties, though it could easily be done by adding
		// them here.
		
		// Include the OPC Batch 'required' props for Physical objects
		// Could easily insert some or all of the optional ones 
		// here if desired.
		
		OPC_PROP_B_ID,                VT_BSTR,			L"ID",						L"ID",
		OPC_PROP_B_VALUE,             VT_VARIANT,			L"Value",					L"Item Value",
		OPC_PROP_B_RIGHTS,            VT_I4,				L"AccessRights",			L"AccessRights",
		OPC_PROP_B_EU,                VT_BSTR,			L"EU",						L"EU",
		OPC_PROP_B_DESC,              VT_BSTR,			L"Description",			L"Description",
		OPC_PROP_B_HIGH_VALUE_LIMIT,	VT_VARIANT,			L"HighValueLimit",			L"HighValueLimit",
		OPC_PROP_B_LOW_VALUE_LIMIT,	VT_VARIANT,			L"LowValueLimit",			L"LowValueLimit",
		OPC_PROP_B_TIME_ZONE,			VT_I4,				L"TimeZone",				L"TimeZone",
		OPC_PROP_B_CONDITION_STATUS,	VT_BSTR,			L"ConditionStatus",		L"ConditionStatus",
		OPC_PROP_B_PHYSICAL_MODEL_LEVEL,		VT_I4,				L"OPCBPhysicalModelLevel", L"OPCBPhysicalModelLevel",
		OPC_PROP_B_RELATED_BATCH_IDS, VT_ARRAY | VT_BSTR, L"RelatedBatchIDs",		L"RelatedBatchIDs",
		OPC_PROP_B_VERSION,			VT_BSTR,			L"Version",				L"Version",
		OPC_PROP_B_EQUIPMENT_CLASS,   VT_BSTR,			L"EquipmentClass",			L"EquipmentClass",
		OPC_PROP_B_LOCATION,          VT_BSTR,			L"Location",				L"Location",
		OPC_PROP_B_MAXIMUM_USER_COUNT,VT_I4,				L"MaximumUserCount",		L"MaximumUserCount",
		OPC_PROP_B_CURRENT_USER_COUNT,VT_I4,				L"CurrentUserCount",		L"CurrentUserCount",
		OPC_PROP_B_CURRENT_USER_LIST, VT_ARRAY | VT_BSTR, L"CurrentUserList",		L"CurrentUserList",
		OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST,	VT_ARRAY | VT_BSTR,	L"AllocatedEquipmentList",	L"Allocated Equipment List",
		OPC_PROP_B_REQUESTER_LIST,	VT_ARRAY | VT_BSTR,	L"RequesterList",			L"RequesterList",
		OPC_PROP_B_REQUESTED_LIST,	VT_ARRAY | VT_BSTR, L"RequestedList",			L"RequestedList",
		OPC_PROP_B_SHARED_BY_LIST,	VT_ARRAY | VT_BSTR, L"SharedByList",			L"SharedByList",
		OPC_PROP_B_EQUIPMENT_STATE,   VT_BSTR,			L"EquipmentState",			L"EquipmentState",
		OPC_PROP_B_EQUIPMENT_MODE,    VT_BSTR,			L"EquipmentMode",			L"EquipmentMode",
		OPC_PROP_B_UPSTREAM_EQUIPMENT_LIST,		VT_ARRAY | VT_BSTR, L"UpstreamEquipmentList",	L"UpstreamEquipmentList",
		OPC_PROP_B_DOWNSTREAM_EQUIPMENT_LIST, 	VT_ARRAY | VT_BSTR,	L"DownstreamEquipmentList",L"DownstreamEquipmentList",
		OPC_PROP_B_EQUIPMENT_PROCEDURAL_ELEMENT_LIST, VT_ARRAY | VT_BSTR, L"EquipmentProceduralElementList", L"EquipmentProceduralElementList",
		OPC_PROP_B_CURRENT_PROCEDURE_LIST,  VT_ARRAY | VT_BSTR, L"CurrentProcedureList",	L"CurrentProcedureList",
		OPC_PROP_B_TRAIN_LIST,        VT_ARRAY | VT_BSTR, L"TrainList",				L"TrainList",
		OPC_PROP_B_DEVICE_DATA_SOURCE,VT_BSTR,		L"DeviceDataSource",		L"DeviceDataSource",
		OPC_PROP_B_DEVICE_DATA_SERVER,VT_BSTR,		L"DeviceDataServer",		L"DeviceDataServer",
		OPC_PROP_B_LOT_ID_LIST,       VT_ARRAY | VT_BSTR, L"LotIDList",				L"LotIDList",
		OPC_PROP_B_VALID_VALUES,      VT_ARRAY | VT_BSTR, L"ValidValues",			L"ValidValues",
		OPC_PROP_B_EXPRESSION_RULE,   VT_BOOL,		L"ExpressionRule",			L"ExpressionRule",
		OPC_PROP_B_ENUMERATION_SET_ID,VT_I4,			L"EnumerationSetID",		L"EnumerationSetID",
		OPC_PROP_B_VERSION_DATE,		VT_BSTR,		L"VersionDate",		L"VersionDate",
		OPC_PROP_B_APPROVAL_DATE,	VT_BSTR,		L"ApprovalDate",		L"ApprovalDate",
		OPC_PROP_B_AUTHOR,				VT_BSTR,		L"Author",					L"Author",
		OPC_PROP_B_APPROVED_BY,		VT_BSTR,		L"ApprovedBy",			L"ApprovedBy",
		OPC_PROP_B_TRAIN_LIST2,			VT_BSTR,		L"TrainList2",			L"TrainList2",
		OPC_PROP_B_TRAIN_LIST2_SCHEMA,	VT_BSTR,		L"TrainList2Schema",	L"TrainList2Schema",
		0,                               0,             NULL,                 NULL
};

OPCBPhysObj::OPCBPhysObj()
{
	// NOTE: Keep the following initializers in sync with the
	// list of property definitions in OPCBPhysObj::M_PropsList
	
	SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);  // what's the canonical data type of an Area?!
	SetPropertyAtID(OPC_PROP_VALUE, L"");
	SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
	SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
	SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
	SetPropertyAtID(OPC_PROP_DESC, L"");
	
	SetPropertyAtID(OPC_PROP_B_ID, L"");
	SetPropertyAtID(OPC_PROP_B_VALUE, L"");
	SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
	SetPropertyAtID(OPC_PROP_B_EU, L"");
	SetPropertyAtID(OPC_PROP_B_DESC, L"");
	SetPropertyAtID(OPC_PROP_B_HIGH_VALUE_LIMIT, L"");
	SetPropertyAtID(OPC_PROP_B_LOW_VALUE_LIMIT, L"");
	SetPropertyAtID(OPC_PROP_B_TIME_ZONE, L"");
	SetPropertyAtID(OPC_PROP_B_CONDITION_STATUS, L"");
	
	SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_LEVEL, (DWORD) OPCB_PHYS_AREA);
	
	LPCWSTR arrString[] = { L"" }; 
	SAFEARRAY *pArr, *pArr2, *pArr3, *pArr4, *pArr5, *pArr6, *pArr7, *pArr8, *pArr9, *pArr10, *pArr11, *pArr12, *pArr13;
	
	CreateSafeArrayFromStrings(arrString, 1, pArr);
	SetPropertyAtID(OPC_PROP_B_RELATED_BATCH_IDS, pArr);
	
	SetPropertyAtID(OPC_PROP_B_VERSION, L"");
	SetPropertyAtID(OPC_PROP_B_EQUIPMENT_CLASS, L"");
	SetPropertyAtID(OPC_PROP_B_LOCATION, L"");
	SetPropertyAtID(OPC_PROP_B_MAXIMUM_USER_COUNT, L"");
	SetPropertyAtID(OPC_PROP_B_CURRENT_USER_COUNT, L"");
	
	CreateSafeArrayFromStrings(arrString, 1, pArr2);
	SetPropertyAtID(OPC_PROP_B_CURRENT_USER_LIST, pArr2);	
	
	CreateSafeArrayFromStrings(arrString, 1, pArr3);
	SetPropertyAtID(OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST, pArr3);
	
	CreateSafeArrayFromStrings(arrString, 1, pArr4);
	SetPropertyAtID(OPC_PROP_B_REQUESTER_LIST, pArr4);
	
	CreateSafeArrayFromStrings(arrString, 1, pArr5);
	SetPropertyAtID(OPC_PROP_B_REQUESTED_LIST, pArr5);
	
	CreateSafeArrayFromStrings(arrString, 1, pArr6);
	SetPropertyAtID(OPC_PROP_B_SHARED_BY_LIST, pArr6);
	
	SetPropertyAtID(OPC_PROP_B_EQUIPMENT_STATE, L"");
	SetPropertyAtID(OPC_PROP_B_EQUIPMENT_MODE, L"");
	
	CreateSafeArrayFromStrings(arrString, 1, pArr7);
	SetPropertyAtID(OPC_PROP_B_UPSTREAM_EQUIPMENT_LIST, pArr7);
	
	CreateSafeArrayFromStrings(arrString, 1, pArr8);
	SetPropertyAtID(OPC_PROP_B_DOWNSTREAM_EQUIPMENT_LIST, pArr8);
	
	CreateSafeArrayFromStrings(arrString, 1, pArr9);
	SetPropertyAtID(OPC_PROP_B_EQUIPMENT_PROCEDURAL_ELEMENT_LIST, pArr9);
	
	CreateSafeArrayFromStrings(arrString, 1, pArr10);
	SetPropertyAtID(OPC_PROP_B_CURRENT_PROCEDURE_LIST, pArr10);
	
	CreateSafeArrayFromStrings(arrString, 1, pArr11);
	SetPropertyAtID(OPC_PROP_B_TRAIN_LIST, pArr11);
	
	SetPropertyAtID(OPC_PROP_B_DEVICE_DATA_SOURCE, L"");
	SetPropertyAtID(OPC_PROP_B_DEVICE_DATA_SERVER, L"");
	
	CreateSafeArrayFromStrings(arrString, 1, pArr12);
	SetPropertyAtID(OPC_PROP_B_LOT_ID_LIST, pArr12);
	
	CreateSafeArrayFromStrings(arrString, 1, pArr13);
	SetPropertyAtID(OPC_PROP_B_VALID_VALUES, pArr13);
	
	SetPropertyAtID(OPC_PROP_B_EXPRESSION_RULE, L"");
	SetPropertyAtID(OPC_PROP_B_ENUMERATION_SET_ID, (DWORD) -1);  // 'value' property doesn't use an enum set
	SetPropertyAtID(OPC_PROP_B_VERSION_DATE, L"");
	SetPropertyAtID(OPC_PROP_B_APPROVAL_DATE, L"");
	SetPropertyAtID(OPC_PROP_B_AUTHOR, L"");
	SetPropertyAtID(OPC_PROP_B_APPROVED_BY, L"");
	SetPropertyAtID(OPC_PROP_B_TRAIN_LIST2,	L"");
	SetPropertyAtID(OPC_PROP_B_TRAIN_LIST2_SCHEMA, L"");
}

//V2Draft3
PROPERTY OPCBMasterRecipeObj::M_PropsList[] = 
{
	// NOTE: Make sure that the following list of parameter
	// definitions, a default value is being set up in the 
	// OPCBPhysObj constructor
	
	// Include the OPC Data Access 'set 1' props
	// Section 4.4.6 in version 2.0 of that spec
	OPC_PROP_CDT,       VT_I2,        L"CDT",         L"Item Canonical Data Type",
		OPC_PROP_VALUE,     VT_VARIANT,   L"Value",       L"Item Value",
		OPC_PROP_QUALITY,   VT_I2,        L"QUALITY",     L"Item Quality",
		OPC_PROP_RIGHTS,    VT_I2,        L"AccessRights", L"Item Access Rights",
		OPC_PROP_SCANRATE,  VT_R4,        L"SCANRATE",    L"Server Scan Rate",
		
		OPC_PROP_DESC,      VT_BSTR,      L"DESCRIPTION", L"Item Description",
		
		// Not including the other OPC Data Access 'set 2 - recommended'
		// properties, though it could easily be done by adding
		// them here.
		
		// Include the OPC Batch 'required' props for Physical objects
		// Could easily insert some or all of the optional ones 
		// here if desired.
		
		OPC_PROP_B_ID,                VT_BSTR,			L"ID",						L"ID",
		OPC_PROP_B_VALUE,             VT_VARIANT,		L"Value",					L"Item Value",
		OPC_PROP_B_RIGHTS,            VT_I4,				L"AccessRights",			L"AccessRights",
		OPC_PROP_B_EU,                VT_BSTR,			L"EU",						L"EU",
		OPC_PROP_B_DESC,              VT_BSTR,			L"Description",			L"Description",
		OPC_PROP_B_HIGH_VALUE_LIMIT,	VT_VARIANT,		L"HighValueLimit",			L"HighValueLimit",
		OPC_PROP_B_LOW_VALUE_LIMIT,	VT_VARIANT,			L"LowValueLimit",			L"LowValueLimit",
		OPC_PROP_B_TIME_ZONE,			VT_I4,				L"TimeZone",				L"TimeZone",
		OPC_PROP_B_CONDITION_STATUS,	VT_BSTR,			L"ConditionStatus",		L"ConditionStatus",
		OPC_PROP_B_VERSION,			VT_BSTR,			L"Version",				L"Version",
		OPC_PROP_B_PRODUCT_ID,	   VT_BSTR,			L"ProductID",			L"ProductID",
		OPC_PROP_B_GRADE,				VT_BSTR,			L"Grade",				L"Grade",
		OPC_PROP_B_BATCH_SIZE,      VT_R4,			L"BatchSize",       L"BatchSize",
		OPC_PROP_B_PHYSICAL_MODEL_REFERENCE,	VT_BSTR,		L"PhysicalModelReference",	
		L"PhysicalModelReference",
		OPC_PROP_B_EQUIPMENT_PROCEDURAL_ELEMENT, VT_BSTR,		L"EquipmentProceduralElement", 
		L"EquipmentProceduralElement",
		OPC_PROP_B_PARAMETER_COUNT,   VT_I4,			    L"ParameterCount",			L"ParameterCount",
		OPC_PROP_B_PARAMETER_TYPE,   VT_I4,			    L"ParameterType",			L"ParameterType",
		OPC_PROP_B_VALID_VALUES,      VT_ARRAY | VT_BSTR, L"ValidValues",			L"ValidValues",
		OPC_PROP_B_SCALING_RULE,      VT_BSTR,			L"ScalingRule",			L"ScalingRule",
		OPC_PROP_B_EXPRESSION_RULE,   VT_BSTR,			L"ExpressionRule",		L"ExpressionRule",
		OPC_PROP_B_RESULT_COUNT,      VT_I4,				L"ResultCount",			L"ResultCount",
		OPC_PROP_B_ENUMERATION_SET_ID,
		VT_I4,				L"EnumerationSetID",		L"EnumerationSetID", 
		//V2Draft3: New property ID 
		OPC_PROP_B_MASTER_RECIPE_MODEL_LEVEL,
		VT_I4,				L"OPCBMasterRecipeModelLevel", L"OPCBMasterRecipeModelLevel",
		OPC_PROP_B_PROCEDURE_LOGIC,	VT_BSTR,		L"ProcedureLogic",		L"ProcedureLogic",
		OPC_PROP_B_PROCEDURE_LOGIC_SCHEMA, VT_BSTR, L"ProcedureLogicSchema",		L"ProcedureLogicSchema",
		OPC_PROP_B_EQUIPMENT_CANDIDATE_LIST,	VT_ARRAY | VT_BSTR,	L"EquipmentCandidateList",			L"EquipmentCandidateList",
		OPC_PROP_B_EQUIPMENT_CLASS_CANDIDATE_LIST,	VT_ARRAY | VT_BSTR, L"EquipmentClassCandidateList",	
		L"EquipmentClassCandidateList",
		OPC_PROP_B_VERSION_DATE,		VT_DATE,			L"VersionDate",			L"VersionDate",
		OPC_PROP_B_APPROVAL_DATE,	VT_DATE,			L"ApprovalDate",			L"ApprovalDate",
		OPC_PROP_B_EFFECTIVE_DATE,	VT_DATE,			L"EffectiveDate",		L"EffectiveDate",
		OPC_PROP_B_EXPIRATION_DATE, VT_DATE,			L"ExpirationDate",		L"ExpirationDate",
		OPC_PROP_B_AUTHOR,				VT_BSTR,			L"Author",					L"Author",
		OPC_PROP_B_APPROVED_BY,		VT_BSTR,			L"ApprovedBy",			L"ApprovedBy",
		OPC_PROP_B_USAGE_CONSTRAINT,VT_BSTR,			L"UsageConstraint",		L"UsageConstraint",
		OPC_PROP_B_RECIPE_STATUS,   VT_BSTR,			L"RecipeStatus",			L"RecipeStatus",
		OPC_PROP_B_RE_USE,				VT_I4,			L"ReUse",					L"ReUse",
		OPC_PROP_B_DERIVED_RE,		VT_BSTR,			L"DerivedRe",				L"DerivedRe",
		OPC_PROP_B_DERIVED_VERSION,	VT_BSTR,			L"DerivedVersion",		L"DerivedVersion",
		OPC_PROP_B_SCALABLE,			VT_BOOL,			L"Scalable",				L"Scalable",
		OPC_PROP_B_EXPECTED_DURATION,	VT_I4,		L"ExpectedDuration",	L"ExpectedDuration",
		OPC_PROP_B_ACTUAL_DURATION,		VT_I4,		L"ActualDuration",		L"ActualDuration",
		0,                               0,             NULL,                 NULL
};

//V2Draft3
OPCBMasterRecipeObj::OPCBMasterRecipeObj()
{
	// NOTE: Keep the following initializers in sync with the
	// list of property definitions in OPCBPhysObj::M_PropsList
	
	SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);  // what's the canonical data type of an Area?!
	SetPropertyAtID(OPC_PROP_VALUE, L"");
	SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
	SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
	SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
	SetPropertyAtID(OPC_PROP_DESC, L"");
	
	SetPropertyAtID(OPC_PROP_B_ID, L"");
	SetPropertyAtID(OPC_PROP_B_VALUE, L"");
	SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
	SetPropertyAtID(OPC_PROP_B_EU, L"");
	SetPropertyAtID(OPC_PROP_B_DESC, L"Default description");
	SetPropertyAtID(OPC_PROP_B_HIGH_VALUE_LIMIT, L"");
	SetPropertyAtID(OPC_PROP_B_LOW_VALUE_LIMIT, L"");
	SetPropertyAtID(OPC_PROP_B_TIME_ZONE, L"CST");
	SetPropertyAtID(OPC_PROP_B_CONDITION_STATUS, L"");
	SetPropertyAtID(OPC_PROP_B_VERSION,	L"0"); 
	
	SetPropertyAtID(OPC_PROP_B_PRODUCT_ID,	L"");
	SetPropertyAtID(OPC_PROP_B_GRADE,	L""); 
	SetPropertyAtID(OPC_PROP_B_BATCH_SIZE,	L"");
	SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_REFERENCE,	L"");
	SetPropertyAtID(OPC_PROP_B_EQUIPMENT_PROCEDURAL_ELEMENT,	L"");
	SetPropertyAtID(OPC_PROP_B_PARAMETER_COUNT,	(DWORD) 0);
	SetPropertyAtID(OPC_PROP_B_PARAMETER_TYPE,	(DWORD) 0);
	SetPropertyAtID(OPC_PROP_B_VALID_VALUES,	L"");
	SetPropertyAtID(OPC_PROP_B_SCALING_RULE,	L"");
	SetPropertyAtID(OPC_PROP_B_EXPRESSION_RULE,	L"");
	SetPropertyAtID(OPC_PROP_B_RESULT_COUNT,	L"");
	SetPropertyAtID(OPC_PROP_B_ENUMERATION_SET_ID, (DWORD) -1); 
	
	SetPropertyAtID(OPC_PROP_B_MASTER_RECIPE_MODEL_LEVEL,	(DWORD) 0);
	SetPropertyAtID(OPC_PROP_B_PROCEDURE_LOGIC,	L"");
	SetPropertyAtID(OPC_PROP_B_PROCEDURE_LOGIC_SCHEMA,	L"");
	SetPropertyAtID(OPC_PROP_B_EQUIPMENT_CANDIDATE_LIST,	L"");
	SetPropertyAtID(OPC_PROP_B_EQUIPMENT_CLASS_CANDIDATE_LIST,	L"");
	SetPropertyAtID(OPC_PROP_B_VERSION_DATE,	L"");
	
	SetPropertyAtID(OPC_PROP_B_APPROVAL_DATE,	L"");
	SetPropertyAtID(OPC_PROP_B_EFFECTIVE_DATE,	L"");
	SetPropertyAtID(OPC_PROP_B_EXPIRATION_DATE,	L"");
	SetPropertyAtID(OPC_PROP_B_AUTHOR,	L"");
	SetPropertyAtID(OPC_PROP_B_APPROVED_BY,	L"");
	SetPropertyAtID(OPC_PROP_B_USAGE_CONSTRAINT,	L"");
	SetPropertyAtID(OPC_PROP_B_RECIPE_STATUS,	L"");
	SetPropertyAtID(OPC_PROP_B_RE_USE,	L"");
	SetPropertyAtID(OPC_PROP_B_DERIVED_RE,	L"");
	SetPropertyAtID(OPC_PROP_B_DERIVED_VERSION,	L"");
	SetPropertyAtID(OPC_PROP_B_SCALABLE,	L"");
	SetPropertyAtID(OPC_PROP_B_EXPECTED_DURATION,	L"");
	SetPropertyAtID(OPC_PROP_B_ACTUAL_DURATION,	L"");
}

PROPERTY OPCBRPEObj::M_PropsList[] = 
{
	// NOTE: Make sure that the following list of parameter
	// definitions, a default value is being set up in the 
	// OPCBRPEObj constructor
	
	// Include the OPC Data Access 'set 1' props
	// Section 4.4.6 in version 2.0 of that spec
	OPC_PROP_CDT,       VT_I2,        L"CDT",          L"Item Canonical Data Type",
		OPC_PROP_VALUE,     VT_VARIANT,   L"Value",       L"Item Value",
		OPC_PROP_QUALITY,   VT_I2,        L"QUALITY",     L"Item Quality",
		OPC_PROP_RIGHTS,    VT_I2,        L"AccessRights",  L"Item Access Rights",
		OPC_PROP_SCANRATE,  VT_R4,        L"SCANRATE",    L"Server Scan Rate",
		
		OPC_PROP_DESC,      VT_BSTR,      L"DESCRIPTION", L"Item Description",
		
		// Not including the other OPC Data Access 'set 2 - recommended'
		// properties, though it could easily be done by adding
		// them here.
		
		// Include the OPC Batch 'required' props for RPE objects
		// Could easily insert some or all of the optional ones 
		// here if desired.
		
		OPC_PROP_B_ID,                VT_BSTR,			L"ID",						L"ID",
		OPC_PROP_B_VALUE,             VT_VARIANT,			L"Value",					L"Item Value",
		OPC_PROP_B_RIGHTS,            VT_I4,				L"AccessRights",			L"AccessRights",
		OPC_PROP_B_EU,                VT_BSTR,			L"EU",						L"EU",
		OPC_PROP_B_DESC,              VT_BSTR,			L"Description",			L"Description",
		OPC_PROP_B_HIGH_VALUE_LIMIT,	VT_VARIANT,			L"HighValueLimit",			L"HighValueLimit",
		OPC_PROP_B_LOW_VALUE_LIMIT,	VT_VARIANT,			L"LowValueLimit",			L"LowValueLimit",
		OPC_PROP_B_TIME_ZONE,			VT_I4,				L"TimeZone",				L"TimeZone",
		OPC_PROP_B_CONDITION_STATUS,	VT_BSTR,			L"ConditionStatus",		L"ConditionStatus",
		
		OPC_PROP_B_BATCH_MODEL_LEVEL,	VT_I4,				L"OPCBBatchModelLevel",	L"OPCBBatchModelLevel",
		
		OPC_PROP_B_VERSION,			VT_BSTR,			L"Version",				L"Version",
		
		OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST,		
		VT_ARRAY | VT_BSTR,	L"AllocatedEquipmentList",	L"Allocated Equipment List",
		OPC_PROP_B_REQUESTER_LIST,	VT_ARRAY | VT_BSTR,	L"RequesterList",			L"RequesterList",
		OPC_PROP_B_REQUESTED_LIST,	VT_ARRAY | VT_BSTR, L"RequestedList",			L"RequestedList",
		OPC_PROP_B_SHARED_BY_LIST,	VT_ARRAY | VT_BSTR, L"SharedByList",			L"SharedByList",
		
		OPC_PROP_B_LOT_ID_LIST,       VT_ARRAY | VT_BSTR, L"LotIDList",				L"LotIDList",
		
		OPC_PROP_B_EXECUTION_STATE,   VT_BSTR,			L"ExecutionState",			L"ExecutionState",
		OPC_PROP_B_IEC61512_1_STATE,  VT_I4,				L"IEC61512-1State",		L"IEC61512-1State",
		OPC_PROP_B_EXECUTION_MODE,    VT_BSTR,			L"ExecutionMode",			L"ExecutionMode",
		OPC_PROP_B_IEC61512_1_MODE,   VT_I4,			  L"IEC61512-1Mode",			L"IEC61512-1Mode",
		OPC_PROP_B_SCHEDULED_START_TIME,  
		VT_DATE,			L"ScheduledStartTime",		L"ScheduledStartTime",
		OPC_PROP_B_ACTUAL_START_TIME, VT_DATE,			L"ActualStartTime",		L"ActualStartTime",
		OPC_PROP_B_ESTIMATED_END_TIME,
		VT_DATE,			L"EstimatedEndTime",		L"EstimatedEndTime",
		OPC_PROP_B_ACTUAL_END_TIME,   VT_DATE,			L"ActualEndTime",			L"ActualEndTime",
		
		OPC_PROP_B_EQUIPMENT_PROCEDURAL_ELEMENT,	VT_BSTR,			L"EquipmentProceduralElement", 
		L"EquipmentProceduralElement",

		OPC_PROP_B_PARAMETER_COUNT,   VT_I4,			    L"ParameterCount",			L"ParameterCount",
		OPC_PROP_B_PARAMETER_TYPE,   VT_I4,			    L"ParameterType",			L"ParameterType",
		OPC_PROP_B_VALID_VALUES,      VT_ARRAY | VT_BSTR, L"ValidValues",			L"ValidValues",
		OPC_PROP_B_SCALING_RULE,      VT_BSTR,			L"ScalingRule",			L"ScalingRule",
		OPC_PROP_B_EXPRESSION_RULE,   VT_BSTR,			L"ExpressionRule",		L"ExpressionRule",
		OPC_PROP_B_RESULT_COUNT,      VT_I4,				L"ResultCount",			L"ResultCount",
		OPC_PROP_B_ENUMERATION_SET_ID,	VT_I4,				L"EnumerationSetID",		L"EnumerationSetID",

		OPC_PROP_B_PROCEDURE_LOGIC,	VT_BSTR,		L"ProcedureLogic",		L"ProcedureLogic",
		OPC_PROP_B_PROCEDURE_LOGIC_SCHEMA, VT_BSTR, L"ProcedureLogicSchema",		L"ProcedureLogicSchema",
		OPC_PROP_B_EQUIPMENT_CANDIDATE_LIST,	VT_ARRAY | VT_BSTR,	L"EquipmentCandidateList",			L"EquipmentCandidateList",
		OPC_PROP_B_EQUIPMENT_CLASS_CANDIDATE_LIST,	VT_ARRAY | VT_BSTR, L"EquipmentClassCandidateList",	
		L"EquipmentClassCandidateList",
		OPC_PROP_B_VERSION_DATE,		VT_DATE,			L"VersionDate",			L"VersionDate",
		OPC_PROP_B_APPROVAL_DATE,	VT_DATE,			L"ApprovalDate",			L"ApprovalDate",
		OPC_PROP_B_AUTHOR,				VT_BSTR,			L"Author",					L"Author",
		OPC_PROP_B_APPROVED_BY,		VT_BSTR,			L"ApprovedBy",			L"ApprovedBy",
		OPC_PROP_B_USAGE_CONSTRAINT,VT_BSTR,			L"UsageConstraint",		L"UsageConstraint",
		OPC_PROP_B_RECIPE_STATUS,   VT_BSTR,			L"RecipeStatus",			L"RecipeStatus",
		OPC_PROP_B_SCALABLE,			VT_BOOL,			L"Scalable",				L"Scalable",
		OPC_PROP_B_EXPECTED_DURATION,	VT_I4,		L"ExpectedDuration",	L"ExpectedDuration",
		OPC_PROP_B_ACTUAL_DURATION,		VT_I4,		L"ActualDuration",		L"ActualDuration",		0,                            0,             NULL,               NULL
};

OPCBRPEObj::OPCBRPEObj()
{
	// NOTE: Keep the following initializers in sync with ths
	// list of property definitions in OPCBRPEObj::M_PropsList
	
	SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
	SetPropertyAtID(OPC_PROP_VALUE, L"");
	SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
	SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
	SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
	SetPropertyAtID(OPC_PROP_DESC, L"");
	
	SetPropertyAtID(OPC_PROP_B_ID, L"");
	SetPropertyAtID(OPC_PROP_B_VALUE, L"");
	SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
	SetPropertyAtID(OPC_PROP_B_EU, L"");
	SetPropertyAtID(OPC_PROP_B_DESC, L"");
	SetPropertyAtID(OPC_PROP_B_HIGH_VALUE_LIMIT, L"");
	SetPropertyAtID(OPC_PROP_B_LOW_VALUE_LIMIT, L"");
	SetPropertyAtID(OPC_PROP_B_TIME_ZONE, L"");
	SetPropertyAtID(OPC_PROP_B_CONDITION_STATUS, L"");
	SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, L"");
	SetPropertyAtID(OPC_PROP_B_VERSION, L"");
	
	
	LPCWSTR arrString[] = { L"" }; 
	SAFEARRAY *pArr, *pArr2, *pArr3, *pArr4, *pArr5, *pArr6;
	
	CreateSafeArrayFromStrings(arrString, 1, pArr);
	SetPropertyAtID(OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST, pArr);
	
	CreateSafeArrayFromStrings(arrString, 1, pArr2);
	SetPropertyAtID(OPC_PROP_B_REQUESTER_LIST, pArr2);
	
	CreateSafeArrayFromStrings(arrString, 1, pArr3);
	SetPropertyAtID(OPC_PROP_B_REQUESTED_LIST, pArr3);
	
	CreateSafeArrayFromStrings(arrString, 1, pArr4);
	SetPropertyAtID(OPC_PROP_B_SHARED_BY_LIST, pArr4);
	
	CreateSafeArrayFromStrings(arrString, 1, pArr5);
	SetPropertyAtID(OPC_PROP_B_LOT_ID_LIST, pArr5);
	
	SetPropertyAtID(OPC_PROP_B_EXECUTION_STATE, L"");
	SetPropertyAtID(OPC_PROP_B_IEC61512_1_STATE, (DWORD) OPCB_STATE_UNKNOWN);
	SetPropertyAtID(OPC_PROP_B_EXECUTION_MODE, L"");
	SetPropertyAtID(OPC_PROP_B_IEC61512_1_MODE, (DWORD) OPCB_MODE_UNKNOWN);
	
	// Default the time values to a special value which 
	// will cause OPCB_NOT_MEANINGFUL to be returned
	FILETIME ftDef;
	ftDef.dwLowDateTime = 0;
	ftDef.dwHighDateTime = 0;
	SetPropertyAtID(OPC_PROP_B_SCHEDULED_START_TIME, ftDef);
	SetPropertyAtID(OPC_PROP_B_ACTUAL_START_TIME, ftDef);
	SetPropertyAtID(OPC_PROP_B_ESTIMATED_END_TIME, ftDef);
	SetPropertyAtID(OPC_PROP_B_ACTUAL_END_TIME, ftDef);
	
	SetPropertyAtID(OPC_PROP_B_EQUIPMENT_PROCEDURAL_ELEMENT, L"");
	SetPropertyAtID(OPC_PROP_B_PARAMETER_COUNT, L"");
	
	CreateSafeArrayFromStrings(arrString, 1, pArr6);
	SetPropertyAtID(OPC_PROP_B_VALID_VALUES, pArr6);
	
	SetPropertyAtID(OPC_PROP_B_SCALING_RULE, L"");
	SetPropertyAtID(OPC_PROP_B_RESULT_COUNT, L"");
	SetPropertyAtID(OPC_PROP_B_ENUMERATION_SET_ID, (DWORD) -1);
}



// Define the properties for parameters and reports.  Note that
// real server may choose to have different properties than the
// sample server does.
PROPERTY OPCBParamObj::M_PropsList[] = 
{
	// NOTE: Make sure that the following list of parameter
	// definitions, a default value is being set up in the 
	// OPCBParamObj constructor
	
	OPC_PROP_CDT,       VT_I2,        L"CDT",         L"Item Canonical Data Type",
		OPC_PROP_VALUE,     VT_VARIANT,   L"Value",       L"Item Value",
		OPC_PROP_UNIT,      VT_BSTR,      L"EU",          L"EU",
		
		OPC_PROP_DESC,      VT_BSTR,      L"DESCRIPTION", L"Item Description",
		
		
		OPC_PROP_B_ID,                VT_BSTR,			L"ID",						L"ID",
		OPC_PROP_B_VALUE,             VT_VARIANT,			L"Value",					L"Item Value",
		OPC_PROP_B_RIGHTS,            VT_I4,				L"AccessRights",			L"AccessRights",
		OPC_PROP_B_EU,                VT_BSTR,			L"EU",						L"EU",
		OPC_PROP_B_DESC,              VT_BSTR,			L"Description",			L"Description",
		OPC_PROP_B_HIGH_VALUE_LIMIT,	VT_VARIANT,			L"HighValueLimit",			L"HighValueLimit",
		OPC_PROP_B_LOW_VALUE_LIMIT,	VT_VARIANT,			L"LowValueLimit",			L"LowValueLimit",
		OPC_PROP_B_BATCH_MODEL_LEVEL,	VT_I4,				L"OPCBBatchModelLevel",	L"OPCBBatchModelLevel",
		OPC_PROP_B_VALID_VALUES,      VT_ARRAY | VT_BSTR, L"ValidValues",			L"ValidValues",
		OPC_PROP_B_SCALING_RULE,      VT_BSTR,			L"ScalingRule",			L"ScalingRule",
		OPC_PROP_B_ENUMERATION_SET_ID,
		VT_I4,				L"EnumerationSetID",		L"EnumerationSetID",
		
		0,                            0,             NULL,               NULL
};


OPCBParamObj::OPCBParamObj()
{
	// NOTE: Keep the following initializers in sync with ths
	// list of property definitions in OPCBParamObj::M_PropsList
	
	SetPropertyAtID(OPC_PROP_CDT, (short)VT_R4);
	SetPropertyAtID(OPC_PROP_VALUE, (float)0.0);
	SetPropertyAtID(OPC_PROP_UNIT, L"");
	SetPropertyAtID(OPC_PROP_DESC, L"");
	
	SetPropertyAtID(OPC_PROP_B_ID, L"");
	SetPropertyAtID(OPC_PROP_B_VALUE, L"");
	SetPropertyAtID(OPC_PROP_B_RIGHTS, L"");
	SetPropertyAtID(OPC_PROP_B_EU, L"");
	SetPropertyAtID(OPC_PROP_B_DESC, L"");
	SetPropertyAtID(OPC_PROP_B_HIGH_VALUE_LIMIT, L"");
	SetPropertyAtID(OPC_PROP_B_LOW_VALUE_LIMIT, L"");
	SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, L"");
	
	LPCWSTR arrString[] = { L"" }; 
	SAFEARRAY* pArr;
	CreateSafeArrayFromStrings(arrString, 1, pArr);
	SetPropertyAtID(OPC_PROP_B_VALID_VALUES, pArr);
	
	SetPropertyAtID(OPC_PROP_B_SCALING_RULE, L"");
	SetPropertyAtID(OPC_PROP_B_ENUMERATION_SET_ID, (DWORD)0);
}


PROPERTY OPCBResultObj::M_PropsList[] = 
{
	// NOTE: Make sure that the following list of parameter
	// definitions, a default value is being set up in the 
	// OPCBResultObj constructor
	
	OPC_PROP_CDT,                 VT_I2,        L"CDT",              L"Item Canonical Data Type",
		OPC_PROP_VALUE,               VT_VARIANT,   L"Value",            L"Item Value",
		OPC_PROP_UNIT,                VT_BSTR,      L"EU",               L"EU",
		
		OPC_PROP_DESC,                VT_BSTR,      L"Description",      L"Item Description",
		
		OPC_PROP_B_ID,                VT_BSTR,			L"ID",						L"ID",
		OPC_PROP_B_VALUE,             VT_VARIANT,			L"Value",					L"Item Value",
		OPC_PROP_B_RIGHTS,            VT_I4,				L"AccessRights",			L"AccessRights",
		OPC_PROP_B_EU,                VT_BSTR,			L"EU",						L"EU",
		OPC_PROP_B_DESC,              VT_BSTR,			L"Description",			L"Description",
		OPC_PROP_B_HIGH_VALUE_LIMIT,	VT_VARIANT,			L"HighValueLimit",			L"HighValueLimit",
		OPC_PROP_B_LOW_VALUE_LIMIT,	VT_VARIANT,			L"LowValueLimit",			L"LowValueLimit",
		OPC_PROP_B_BATCH_MODEL_LEVEL,	VT_I4,				L"OPCBBatchModelLevel",	L"OPCBBatchModelLevel",
		OPC_PROP_B_VALID_VALUES,      VT_ARRAY | VT_BSTR, L"ValidValues",			L"ValidValues",
		OPC_PROP_B_SCALING_RULE,      VT_BSTR,			L"ScalingRule",			L"ScalingRule",
		OPC_PROP_B_ENUMERATION_SET_ID,
		VT_I4,				L"EnumerationSetID",		L"EnumerationSetID",
		
		0,                            0,            NULL,                 NULL
};

OPCBResultObj::OPCBResultObj()
{
	// NOTE: Keep the following initializers in sync with ths
	// list of property definitions in OPCBResultObj::M_PropsList
	
	SetPropertyAtID(OPC_PROP_CDT, (short)VT_R4);
	SetPropertyAtID(OPC_PROP_VALUE, (float)0.0);
	SetPropertyAtID(OPC_PROP_UNIT, L"");
	SetPropertyAtID(OPC_PROP_DESC, L"");
	
	SetPropertyAtID(OPC_PROP_B_ID, L"");
	SetPropertyAtID(OPC_PROP_B_VALUE, L"");
	SetPropertyAtID(OPC_PROP_B_RIGHTS, L"");
	SetPropertyAtID(OPC_PROP_B_EU, L"");
	SetPropertyAtID(OPC_PROP_B_DESC, L"");
	SetPropertyAtID(OPC_PROP_B_HIGH_VALUE_LIMIT, L"");
	SetPropertyAtID(OPC_PROP_B_LOW_VALUE_LIMIT, L"");
	SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, L"");
	
	LPCWSTR arrString[] = { L"" }; 
	SAFEARRAY* pArr;
	CreateSafeArrayFromStrings(arrString, 1, pArr);
	SetPropertyAtID(OPC_PROP_B_VALID_VALUES, pArr);
	
	SetPropertyAtID(OPC_PROP_B_SCALING_RULE, L"");
	SetPropertyAtID(OPC_PROP_B_ENUMERATION_SET_ID, (DWORD)0);
}


PROPERTY OPCBBatchIDListObj::M_PropsList[] = 
{
	// NOTE: Make sure that the following list of parameter
	// definitions, a default value is being set up in the 
	// OPCBBatchIDListObj constructor
	
	OPC_PROP_CDT,                 VT_I2,        L"CDT",    L"Item Canonical Data Type",
		OPC_PROP_VALUE,               VT_VARIANT,   L"Value",  L"Item Value",
		
		0,                            0,            NULL,                 NULL
};

OPCBBatchIDListObj::OPCBBatchIDListObj()
{
	SetPropertyAtID(OPC_PROP_CDT, (short)(VT_ARRAY | VT_BSTR));
	
	LPCWSTR arrString[] = { L"" }; 
	SAFEARRAY* pArr;
	CreateSafeArrayFromStrings(arrString, 1, pArr);
	SetPropertyAtID(OPC_PROP_VALUE, pArr);
}  


OPCBObj::OPCBObj( ) : 
m_parent(NULL), m_childList(NULL)
{
	// Initialize the array of property values
	for (DWORD i = 0; i < MAX_PROPERTIES; i++)
	{
		m_dwPropIDs[i] = 0;             // 0 indicates 'not used'
		VariantInit(&m_vPropValues[i]);
	}
	
}

OPCBObj::~OPCBObj()
{
	for (DWORD i = 0; i < MAX_PROPERTIES; i++)
	{
		if (m_dwPropIDs[i] != 0)             // 0 indicates 'not used'
		{
			VariantClear(&m_vPropValues[i]);
		}
	}
} 


OPCBObj* OPCBObj::findChildNamed ( LPCWSTR pName )
{
	OPCBObj* result = NULL;   // default if not found
	OPCBObj** ppObj = getChildList();
	while (ppObj != NULL && *ppObj != NULL)
	{
		if (wcscmp((*ppObj)->GetName(), pName) == 0)
		{
			result = *ppObj;
			break;  // found it
		}
		ppObj++;
	}
	return result;
}

// Finds the nth child of the object.  This is intended 
// only for the xxxx.OPCBParameters and xxxx.OPCBResults
// items whose children are the parameters of the RPE
// known as xxxx.  The index is 1-based.
OPCBObj* OPCBObj::findChildNumbered ( int index )
{
	int counter = 1;
	OPCBObj* result = NULL;   // default if not found
	OPCBObj** ppObj = getChildList();
	while (ppObj != NULL && *ppObj != NULL)
	{
		if (counter == index)
		{
			result = *ppObj;
			break;  // found it
		}
		counter++;
		ppObj++;
	}
	return result;
}

// Function counts number of items underneath current browser
// position. This function is called recursively, therefore piCount
// must be initialized outside the function
void OPCBObj::GetItemCount(
									OPCBROWSETYPE dwBrowseFilterType,
									int *piCount)
{
	
	int	count;
	OPCBObj  **childlist;	// ptr to array of tagitem ptrs
	OPCBObj  **child;   
	
	childlist = getChildList();
	if(childlist == NULL) return ;	// if no children, return
	
	// Starting position
	
	// Fist pass: Count the available tags
	//
	count = 0;
	while(*childlist != NULL)
	{
		child = (*childlist)->getChildList();
		switch(dwBrowseFilterType)
		{
		case OPC_BRANCH:
			if(child != NULL) count++;
			break;
		case OPC_LEAF:
			if(child == NULL) count++;
			break;
		case OPC_FLAT:
			count++;
			(*childlist)->GetItemCount(OPC_FLAT, &count);
			break;
		default:
			break;
		}
		childlist++;
	}
	*piCount += count;

  // Treating the properties as leaves, so we need to 
  // count them also.  This is a change in version 2;
  // version 1 did not treat properties as leaves but
  // it was found that most clients expected it to do
  // that so the spec was changed in version 2.
  DWORD cProps = 0;
  if (dwBrowseFilterType == OPC_LEAF || dwBrowseFilterType == OPC_FLAT)
  {
    for (DWORD i = 0; i < MAX_PROPERTIES; i++)
    {
      if (m_dwPropIDs[i] != 0)
      {
        if (findProperty(m_dwPropIDs[i]) != NULL)
          cProps++;
      }
    }
  }
  *piCount += cProps;

}

// Adds item IDs to pre-created array
// Function is called recursively, therefore 
// *piCurPosition and *AddressList
// must be initialized outside
void OPCBObj::GetIDList(
								OPCBROWSETYPE dwBrowseFilterType,
								LPOLESTR* ItemIDList,
								int* piCurPos, 
								int iCount,
								LPOLESTR szParentName) // used only when dwBrowseFilterType==OPC_FLAT)
{
	OPCBObj  **childlist;	// ptr to array of tagitem ptrs
	OPCBObj  **child;
	
	childlist = getChildList();
	if(childlist == NULL) return ;	// if no children, return
	
	while(*childlist != NULL)
	{
		// Note that the string is cloned to keep the enumerator 'stable'
		// Also, we return the 'name' rather than the complete ItemID.  Per
		// the Data Access spec on this method, we should not return any
		// delimiters or parent names.
		
		// Make sure that we are within bounds of allocated memory
		if (*piCurPos >= iCount) // should never happen
			return;
		
		LPOLESTR szChildName, szTemp;
		
		child = (*childlist)->getChildList();
		szChildName = (*childlist)->GetName();
		
		switch(dwBrowseFilterType)
		{
		case OPC_BRANCH:
			if(child != NULL) 
				ItemIDList[(*piCurPos)++] = WSTRClone(szChildName, NULL);
			break;
		case OPC_LEAF:
			if(child == NULL) 
				ItemIDList[(*piCurPos)++] = WSTRClone(szChildName, NULL);
			break;
		case OPC_FLAT:
			szTemp = BuildChildID(szParentName, DELIMITER, szChildName);
			ItemIDList[(*piCurPos)++] = szTemp;
			(*childlist)->GetIDList(OPC_FLAT, ItemIDList, piCurPos, iCount, szTemp);
			break;
		default:
			break;
		}
		childlist++;
	}

  // Treating properties as leaves, so we add them to the list
  // This is a change in version 2; version 1 did not treat 
  // properties as leaves but it was found that most clients 
  // expected it to do that so the spec was changed in version 2.
  if (dwBrowseFilterType == OPC_LEAF || dwBrowseFilterType == OPC_FLAT)
  {
    for (DWORD i = 0; i < MAX_PROPERTIES; i++)
    {
      if (m_dwPropIDs[i] != 0)
      {
        PROPERTY* pProp = findProperty(m_dwPropIDs[i]);
        if (pProp == NULL)
        {
          // If no such property exists, return the
          // appropriate error ??
        }
        else if (pProp->idsuffix != NULL)
        {
          // If there is a suffix available, concatenate
          // it to the object's ItemID to form a new ItemID
          // that can be used to read the property via the
          // OPC data access interfaces.
          WCHAR* temp = new WCHAR[sizeof(WCHAR) * 
            (wcslen(GetItemID()) + wcslen(DELIMITER) + wcslen(pProp->idsuffix) + 1)];
          temp[0] = '\0';
          wcscpy(temp, GetItemID());
          wcscat(temp, DELIMITER);
          wcscat(temp, pProp->idsuffix);
          ItemIDList[(*piCurPos)++] = WSTRClone(temp, NULL);  
          delete temp;
        }
      }
    }
  }

}

// Function builds parameter ID from parent name, delimiter and child name
// It is uded in GetIDList() when OPCBROWSETYPE==OPC_FLAT
//
WCHAR* OPCBObj::BuildChildID(
									  const WCHAR* szParentName,
									  const WCHAR *szDelimiter,
									  const WCHAR *szChildName)
{
	// Check for illegal parameters
	if ((szDelimiter==NULL)||(szChildName==NULL))
		return NULL;
	
	// Check for special parameter values
	if (szParentName == NULL)
		return WSTRClone(szChildName, NULL);
	if (wcslen(szParentName) == 0)
		return WSTRClone(szChildName, NULL);
	
	// Build Item ID
	WCHAR *newstr, *newstr1;
	newstr = newstr1 = NULL;
	
	newstr = new WCHAR[wcslen(szParentName) + wcslen(szDelimiter) + wcslen(szChildName) + 1];
	if (newstr==NULL) return NULL;
	
	newstr1 = newstr;
	wcscpy(newstr1, szParentName);
	newstr1 += wcslen(szParentName);
	wcscpy(newstr1, szDelimiter);
	newstr1 += wcslen(szDelimiter);
	wcscpy(newstr1, szChildName);
	
	return newstr;
}

// Returns a list of itemIDs of my child objects, 
// filtered in various ways.  Currently the only
// filtering that is supported is on the 
// dwBrowseFilterType
// Copied with changes from GetAddressList() function 
// in I_BSAS.cpp
void OPCBObj::GetAddressList(
									  OPCBROWSETYPE dwBrowseFilterType,
									  LPCWSTR szFilterCriteria,
									  VARTYPE vtDataTypeFilter,
									  DWORD dwAccessRightsFilter,
									  LPOLESTR **AddressList,
									  int *AddressCount)
{
	// Defaults...
	*AddressList = NULL;
	*AddressCount = 0;
	
	// Count nummer of items
	GetItemCount(dwBrowseFilterType, AddressCount);
	
	// Reserve memory for item ID array
	*AddressList = new LPOLESTR[*AddressCount];	// Caller must free this!
	if (*AddressList==NULL) return;
	
	// Fill the array
	int iPos=0;
	GetIDList(dwBrowseFilterType, *AddressList, &iPos, *AddressCount, L"");
	
}

// Find the property at a given id; return a pointer to it
PROPERTY * OPCBObj::findProperty ( DWORD idToFind )
{
	PROPERTY * pp;
	PROPERTY * result = NULL;
	
	// Point to the head of the list
	pp = getPropertyList();
	while ((pp != NULL) && (pp->pid != 0))
	{
		if (pp->pid == idToFind)
		{
			result = pp;
			break;
		}
		pp++;
	}
	return result;
}

HRESULT OPCBObj::GetPropertyAtID (DWORD id, VARIANT& v)
{
	HRESULT hr;
	PROPERTY* pProp = findProperty(id);
	if (pProp == NULL)
	{
		// no such property is allowed on this kind of object
		VariantClear(&v);
		return OPC_E_INVALID_PID;
	}
	
	bool bFound = false;
	// Look up the value, which is held in an array for this object
	for (DWORD i = 0; i < MAX_PROPERTIES; i++)
	{
		if (m_dwPropIDs[i] == id)
		{
			bFound = true;
			// property exists for this object, so return it
			
			// Handle the VT_DATE properties with the special
			// value that indicates they don't have a valid
			// value at this time (used for ActualEndTime of a 
			// batch that has not ended yet, for example).
			if (m_vPropValues[i].vt == VT_DATE)
			{
				// Create a variant containing the special default
				// time value that represents 'no value yet'.
				FILETIME ftDefault;
				ftDefault.dwLowDateTime = 0;
				ftDefault.dwHighDateTime = 0;
				
				VARIANT vDefault;
				VariantInit(&vDefault);
				
				SYSTEMTIME st;
				DATE date;
				FileTimeToSystemTime( &ftDefault, &st);
				SystemTimeToVariantTime( &st, &date);
				
				V_VT(&vDefault) = VT_DATE;
				V_DATE(&vDefault) = date;
				
				// The above 12 lines or so have created a variant
				// with the special value of date/time that we use
				// for a default.  Now see if the value of the 
				// property we are accessing equates to this value.
				if (vDefault.date == m_vPropValues[i].date)
				{
					hr = OPCB_E_NOT_MEANINGFUL;
					VariantClear(&vDefault);
					break;
				}
				VariantClear(&vDefault);
			}
			
			VariantInit(&v);
			hr = VariantCopy(&v, &m_vPropValues[i]);
			break;
		}
	}
	if (bFound)
		return hr;
	else
	{
		// Property is allowed on this kind of object, but this
		// instance does not have any value for it.  NOTE: This
		// behavior, where we may say an property exists but then
		// don't provide a value for it, is a quirk of this sample
		// server.  This can happen because we have said that all
		// "Physical" objects (for example) support a certain set
		// of properties (defined in OPCBPhysObj::M_PropsList), 
		// but the constructor for OPCBPhysObj does not provide 
		// a value for a given property. Strictly this doesn't
		// conform to the spec (section 3.3.3 of the RC1 version),
		// so either the constructor or the property table is
		// incorrect.
		VariantClear(&v);    
		return OPC_E_NOTFOUND;
	}
}

// Store the value of a property
// TODO - make this check whether id is that of a 
// property that is valid for this kind of object
// (in the M_PropsList static member).  This would
// avoid errors in the BatchItems.cpp code where 
// the objects are created and properties are 
// added to them.  We would need to do an assert
// if we detected this case as there is no other
// way for a server setup error like this to be 
// notified.
HRESULT OPCBObj::SetPropertyAtID (DWORD id, VARIANT& v)
{
	bool bFound = false;
	bool bNew = false;
	HRESULT hr;
	
	// First see if this ID already has a value.  If so, 
	// replace it.
	for (DWORD i = 0; i < MAX_PROPERTIES; i++)
	{
		if (m_dwPropIDs[i] == id)
		{
			hr = VariantCopy(&m_vPropValues[i], &v);
			bFound = true;
			break;
		}
	}
	
	if (!bFound)  // no previous value at ID, so insert it
		// at the first free position.
	{
		for (DWORD i = 0; i < MAX_PROPERTIES; i++)
		{
			if (m_dwPropIDs[i] == 0)
			{
				m_dwPropIDs[i] = id;
				hr = VariantCopy(&m_vPropValues[i], &v);
				assert(SUCCEEDED(hr));
				bNew = true;
				break;
			}
		}
	}
	
	// if we replaced an existing value, or we inserted a new
	// one, we succeeded.
	if (bFound || bNew)
		return S_OK;
	else
		return E_FAIL;
}

HRESULT OPCBObj::SetPropertyAtID (DWORD id, short i2)
{
	HRESULT hr;
	VARIANT v;
	VariantInit(&v);
	
	V_VT(&v) = VT_I2;
	V_I2(&v) = i2;
	hr = SetPropertyAtID(id, v);
	VariantClear(&v);
	return hr;
}

HRESULT OPCBObj::SetPropertyAtID (DWORD id, DWORD i4)
{
	HRESULT hr;
	VARIANT v;
	VariantInit(&v);
	
	V_VT(&v) = VT_I4;
	V_I4(&v) = i4;
	hr = SetPropertyAtID(id, v);
	VariantClear(&v);
	return hr;
}

HRESULT OPCBObj::SetPropertyAtID (DWORD id, float r4)
{
	HRESULT hr;
	VARIANT v;
	VariantInit(&v);
	
	V_VT(&v) = VT_R4;
	V_R4(&v) = r4;
	hr = SetPropertyAtID(id, v);
	VariantClear(&v);
	return hr;
}

HRESULT OPCBObj::SetPropertyAtID (DWORD id, double r8)
{
	HRESULT hr;
	VARIANT v;
	VariantInit(&v);
	
	V_VT(&v) = VT_R8;
	V_R8(&v) = r8;
	hr = SetPropertyAtID(id, v);
	VariantClear(&v);
	return hr;
}

HRESULT OPCBObj::SetPropertyAtID (DWORD id, LPWSTR str)
{  
	HRESULT hr;
	VARIANT v;
	VariantInit(&v);
	
	V_VT(&v) = VT_BSTR;
	V_BSTR(&v) = SysAllocString(str);
	hr = SetPropertyAtID(id, v);
	VariantClear(&v);
	return hr;
}

HRESULT OPCBObj::SetPropertyAtID (DWORD id, bool b)
{
	HRESULT hr;
	VARIANT v;
	VariantInit(&v);
	
	V_VT(&v) = VT_BOOL;
	V_BOOL(&v) = b;
	hr = SetPropertyAtID(id, v);
	VariantClear(&v);
	return hr;
}

// Store a property of type VT_DATE, converting from
// a passed FILETIME value.
HRESULT OPCBObj::SetPropertyAtID (DWORD id, FILETIME ft)
{
	HRESULT hr;
	VARIANT v;
	VariantInit(&v);
	SYSTEMTIME st;
	DATE date;
	
	FileTimeToSystemTime (&ft, &st);
	SystemTimeToVariantTime (&st, &date);
	
	V_VT(&v) = VT_DATE;
	V_DATE(&v) = date;
	hr = SetPropertyAtID(id, v);
	VariantClear(&v);
	return hr;
}

HRESULT OPCBObj::SetPropertyAtID (DWORD id, SAFEARRAY* arr)
{
	HRESULT hr;
	VARIANT v;
	VariantInit(&v);
	
	VARTYPE vt;
	SafeArrayGetVartype(arr, &vt);
	
	v.vt = VT_ARRAY | vt;  
	
	v.parray = arr;
	hr = SetPropertyAtID(id, v);
	VariantClear(&v);
	return hr;
}

// A simple-minded string match.  This will return true if 
// the first n characters of subject are the same as the 
// filter, where n is the length of filter.   A real server 
// would do something more sophisticated (please.....) 
// using wildcards, regular expressions or something.
// Note that the specification does allow the server to
// define what wildcarding behavior it will support.
bool StringMatch(LPWSTR filter, LPWSTR subject)
{
  int flen = wcslen(filter);
  if (flen > wcslen(subject)) // can't match if the filter is longer
    return false;

  return (wcsncmp(filter, subject, flen) == 0);
}

// Check whether a batch object matches the criteria specified
// by a filter.  In this sample implementation it only checks
// some of the criteria; a real server is expected to check
// all of them.  See the OPCBATCHSUMMARYFILTER structure
// definition for all the fields that may be checked.
bool OPCBBatchObj::PassesFilter(OPCBATCHSUMMARYFILTER* pFilter)
{
  VARIANT v;
  HRESULT hr;  
  // If pFilter is NULL, we assume no filtering so all batches pass.
  if (pFilter == NULL)
    return true;

  // Assume it is going to match.  Each of the checks below
  // may then set result to false if it does not pass.
  bool result = true; 

  // Check batch id.  An empty string in pFilter->szID means
  // ignore this field, which means it passes.
  if (wcslen(pFilter->szID) != 0)
  {
    result = StringMatch(pFilter->szID, GetName());
  }

  if (result && (wcslen(pFilter->szDescription) != 0))
  {
    VariantInit(&v);
    hr = GetPropertyAtID(OPC_PROP_B_DESC, v);
    if (SUCCEEDED(hr))
    {
      result = StringMatch(pFilter->szDescription, V_BSTR(&v));
    }
    else
    {
      result = false; // error; failed to get description property
    }
  }

  if (result)
  {
    VariantInit(&v);
    hr = GetPropertyAtID(OPC_PROP_B_BATCH_SIZE, v);
    float batchSize = V_R8(&v);
    result =  ((pFilter->fMinBatchSize <= batchSize) &&
               (batchSize <= pFilter->fMaxBatchSize));
  }

  return result;
}



OPCBObj* allObjs[MAX_BATCH_OBJ];   // enough for the sample server

void CreateSampleObjects()
{
	for (int i = 0; i < MAX_BATCH_OBJ; i++)
	{
		allObjs[i] = NULL;
	}
	
	// call methods defined in BatchItems.cpp to load
	// some data for the sample server to work with
   
	OPCBXMLData  XMLData;
	//LoadPropIDTable( "PropertyIDTable.dat" );
	
	if( FAILED( XMLData.LoadPropIDTable( "c:\\OPCB\\PropertyIDTable.dat" ) ) ||
		FAILED( XMLData.LoadXMLFile( "c:\\OPCB\\OPCBData.xml" ) ) )
		//if( FAILED( XMLData.LoadXMLFile( "OPCBData.xml" ) ) )
	{
		CreateSamplePhysObjects();
		CreateSampleBatchObjects();
		CreateSampleRPEObjects();
		CreateSampleRootObjects();
	}
}

void RemoveSampleObjects()
{
	for (int i = 0; i < MAX_BATCH_OBJ; i++)
	{
		if( allObjs[i] != NULL)
			delete allObjs[i];
	}
}

void LoadSampleObjects()
{
	for (int i = 0; i < MAX_BATCH_OBJ; i++)
	{
		allObjs[i] = NULL;
	}
	
	// call methods defined in BatchItems.cpp to load
	// some data for the sample server to work with
}

void AddSampleBatchObject ( OPCBObj* pOb )
{
	for (int i = 0; i < MAX_BATCH_OBJ; i++)
	{
		// find the first free entry
		if (allObjs[i] == NULL)
		{
			allObjs[i] = pOb;
			break;
		}
	}   // note no error check if there wasn't a free slot
}

// Search for an object in the whole batch address space
OPCBObj* FindSampleBatchObjectByItemID ( LPCWSTR pszItem )
{
	WCHAR pszItem1[200];
	
	// First see if pszItem uses the special shortcut syntax
	// for anonymous parameter access.  An itemID conforming
	// to that syntax is distinguished by containing a substring
	// such as ".OPCBParameters.OPCBPx" where x is numeric.
	ConvertShortcutItemID ( pszItem1, pszItem );
	
	// just search the list
	return LookupObjectByItemID ( pszItem1 );
}

OPCBObj* LookupObjectByItemID( LPCWSTR pszItem )
{
	for (int i = 0; i < MAX_BATCH_OBJ; i++)
	{
		if (allObjs[i] == NULL)
			break;    // no more objects; so not found
		if (wcscmp((allObjs[i])->GetItemID(), pszItem) == 0)
		{
			return allObjs[i];
		}
	}
	return NULL;
}

void ConvertShortcutItemID ( LPWSTR pszDest, LPCWSTR pszSrc )
{
	bool bIsParam = FALSE;
	bool bIsResult = FALSE;
	int lenBase;
	// We're looking for a string of the format
	// OPCBBatchModel.BATCH1.PROC1.UPROC1.OPCBParameters.OPCBPn.Value
	// (where the 'PROC1.UPROC1' part could be replaced
	// by any valid path to an RPE within the batch.)
	// If we find it, we want to replace the 'OPCBPn'
	// with the name of the corresponding (nth) parameter.
	// Same applies for results, where the string to be
	// substituted would be 'OPCBRn'
	
	// (The following string manipulation code would
	// be easier if using MFC CStrings or STL strings).
	
	// Init the dest string to something that will surely fail
	wcscpy(pszDest, L"BAD ITEM ID");
	
	LPCWSTR searchStrParam  = L"OPCBParameters.OPCBP";
	LPCWSTR sParam = L"OPCBParameters.";
	int lenParam = wcslen( sParam );
	
	LPCWSTR searchStrResult = L"OPCBResults.OPCBR";
	LPCWSTR sResult = L"OPCBResults.";
	int lenResult = wcslen(sResult);
	
	WCHAR baseItem[200];
	WCHAR* pSubstLoc;
	if ((pSubstLoc = wcsstr(pszSrc, searchStrParam)) != NULL)
	{
		bIsParam = true;
		pSubstLoc += lenParam;  // point to the first char of "OPCBPx"
		lenBase = pSubstLoc - pszSrc - 1;  // don't copy the last "."
		wcsncpy(baseItem, pszSrc, lenBase);  // copy from start up to last char of "OPCBParameters"
		*(baseItem + lenBase) = L'\0'; // wcsncpy doesn't terminate the string
	}
	else if ((pSubstLoc = wcsstr(pszSrc, searchStrResult)) != NULL)
	{
		bIsResult = true;
		pSubstLoc += lenResult;  // point to the first char of "OPCBRx"
		lenBase = pSubstLoc - pszSrc - 1;  // don't copy the last "."
		wcsncpy(baseItem, pszSrc, lenBase);  // copy from start up to last char of "OPCBParameters"
		*(baseItem + lenBase) = L'\0'; // wcsncpy doesn't terminate the string
	}
	else
	{
		// didn't find the substring we were interested in, so
		// just copy the src to the dest unchanged and return
		wcscpy(pszDest, pszSrc);
		return;
	}
	
	// pSubstLoc points to the part of the string that goes 
	// something like "OPCBPn.Value".  We need to extract the
	// numeric value of n, and the "Value" (or whatever property
	// name it is) and save them.  Note; this implementation
	// using sscanf is not at all robust but should work well
	// enough for this sample server
	int paramNum;
	WCHAR propName[30] = L"";  // in case no propName was supplied
	int r = swscanf(pSubstLoc, L"%*l5s%d.%ls", &paramNum, propName);
	
	OPCBObj* pObj = LookupObjectByItemID(baseItem);
	if (pObj != NULL)
	{
		// pObj now points to the item xxxx.OPCBParameters
		// or xxxx.OPCBResults (where xxxx is the RPE that
		// the parameters or results belong to).  We now need
		// to look up the nth parameter or report of that
		// object.
		pObj = pObj->findChildNumbered ( paramNum );
		if (pObj != NULL)
		{
			// Finally, we can reassemble the ID from the pieces
			// we have extracted.
			wcscpy ( pszDest, baseItem );  // something like "OPCBBatchModel.BATCH1.PROC1.UPROC1.OPCBParameters."
			wcscat ( pszDest, L"." );             // add delimiter
			wcscat ( pszDest, pObj->GetName() );  // add the param name
			if (wcscmp(propName, L"") != 0)   // if there is a property name, append it with a delimiter
			{
				wcscat ( pszDest, L"." );
				wcscat ( pszDest, propName );
			}
		}
	}
	return;
} 



bool isBatchItemID ( LPWSTR itemID )
{
	//V2Draft3
	return (wcsncmp(itemID, L"OPCBPhysicalModel", wcslen(L"OPCBPhysicalModel")) == 0
		|| wcsncmp(itemID, L"OPCBBatchModel", wcslen(L"OPCBBatchModel")) == 0
		|| wcsncmp(itemID, L"OPCBBatchIDList", wcslen(L"OPCBBatchIDList")) == 0
		|| wcsncmp(itemID, L"OPCBMasterRecipeModel", wcslen(L"OPCBMasterRecipeModel")) == 0
		|| wcsncmp(itemID, L"OPCBBatchArchiveModel", wcslen(L"OPCBBatchArchiveModel")) == 0);
}


// See if an itemID matches any branch or leaf object
// in the namespace and return a pointer to that
// object.
//  Case 1:
//    OPCBBatchModel.BATCH1.PROC1.Version
// would return a pointer to PROC1, with
//    OPCBBatchModel.BATCH1.PROC1   in branchID, and
//    .Version                      in propertySuffix
//    OPC_PROP_B_VERSION            in dwPropID
//
//  Case 2:
//    OPCBBatchModel.BATCH1.PROC1
// would return a pointer to PROC1, with
//    ""  in branchID
//    ""  in propertySuffix
//    0   in dwPropID
// 
//  Case 3:
//    OPCBBatchModel.BATCH1.PROC1.Bogus
// would return a NULL pointer, with
//    ""  in branchID
//    ""  in propertySuffix
//    0   in dwPropID
//
//  Case 4:
//    BAD_ITEM_ID
// would return a NULL pointer, with 
//    ""  in branchID
//    ""  in propertySuffix
//    0   in dwPropID
//
//  Case 5:
//    BAD_ITEM_ID.BOGUS
// would return a NULL pointer, with 
//    ""  in branchID
//    ""  in propertySuffix
//    0   in dwPropID
OPCBObj* ParseItemID ( LPWSTR itemID, LPWSTR branchID, LPWSTR propertySuffix, DWORD& dwPropID )
{
	// Init the return values to the 'not found' values
	wcscpy ( branchID, L"" );
	wcscpy ( propertySuffix, L"" );
	dwPropID = 0;
	
	// First we try to look up itemID as it is, in case
	// it represents a branch object.
	OPCBObj* pObj = FindSampleBatchObjectByItemID ( itemID );
	if (pObj != NULL)
	{
		// This is Case 2
		return pObj;
	}
	
	// find the last delimiter in the input: this 
	// gives us a pointer to what follows it
	WCHAR* pSuffix = wcsrchr ( itemID, L'.' );
	if (pSuffix == NULL)
	{
		// No delim found.  The only way this could be 
		// valid is if itemID is one of the objects at
		// the root (OPCBBatchModel" for example), in
		// which case we should have found it above.  So 
		// this must be a bad itemID (Case 4)
		return NULL;  
	}
	
	// Form a string with the part of the itemID 
	// preceding the suffix
	WCHAR tempBranch[200];
	int lenBranch = pSuffix - itemID;  // don't copy the last "."
	wcsncpy(tempBranch, itemID, lenBranch);
	*(tempBranch + lenBranch) = L'\0'; // wcsncpy doesn't terminate the string
	
	// See if that is the ID of a valid object
	pObj = FindSampleBatchObjectByItemID ( tempBranch );
	if (pObj != NULL)
	{
		PROPERTY* pPropList = pObj->getPropertyList();
		
		// search the property list for a match on the suffix
		while ((pPropList != NULL) && (pPropList->pid != 0))
		{
			if ((pPropList->idsuffix != 0) && (wcscmp(pPropList->idsuffix, pSuffix) == 0))
			{
				// Found it! - copy to the output parameters
				// This is Case 1
				wcscpy ( propertySuffix, pSuffix );
				wcscpy ( branchID, tempBranch );
				dwPropID = pPropList->pid;
				return pObj;
			}
			pPropList++;
		}
		// If we get here, the branch was for a valid object but
		// the suffix was not a valid property.  We treat this as
		// as an invalid item ID.  (Case 3)
		return NULL;
	}
	// If we get here the branch part of the itemID did not
	// represent a valid branch object.  (Case 4 or 5).
	return NULL;
}

// Create a SAFEARRAY of VT_BSTRs from an array of strings
HRESULT CreateSafeArrayFromStrings(LPCWSTR* pStrings, long count, SAFEARRAY*& arr)
{
	HRESULT hr = S_OK;
	
	VARIANT v;
	VariantInit(&v);
	V_VT(&v) = VT_BSTR;
	
	SAFEARRAYBOUND rgsabound[1];
	rgsabound[0].lLbound = 0;
	rgsabound[0].cElements = count;
	
	// Create a SAFEARRAY.  The caller is responsible for 
	// destroying this; normally he will store it in a 
	// VARIANT and the ultimate VariantClear will destroy
	// the SAFEARRAY.
	arr = SafeArrayCreate(VT_BSTR, 1, rgsabound);
	if (arr != NULL)
	{
		BSTR *bstrArray = NULL;
		SafeArrayAccessData(arr, (void **) &bstrArray);
		
		for (long i = 0; i < count; i++)
		{
			bstrArray[i] =  SysAllocString(pStrings[i]);
			
		}
		SafeArrayUnaccessData(arr);
	}
	return hr;
}

