// itemids.cpp
//
// (c) Copyright 1999 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Data Access Specification and may be used
//  as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// CREDITS:
//  This code was generously provided to the OPC Foundation by
//  Al Chisholm, Intellution Inc.
//
// CONTENTS:
//
//  This file contains a 'dummy' address space for the OPC Server
//  based on the TAGITEM structure defined in opcxxx.h
//
//
// Modification Log:
// Vers    Date   By    Notes
// ----  -------- ---   -----
// 2.00  08/19/99 acc   
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"

///////////////////////////////////////
// Define a sample list of available ITEMIDs for GetAddressList/Browse.
// Note that in this sample code, AddItems and ValidateItems do NOT
// check against this list. In a 'real' server they would need to do so.
//

// For reference, here is the structure (which is actually defined in opcxxx.h)
//typedef struct tagitem
//{
//	LPOLESTR tag;
//	struct tagitem *parent;
//	struct tagitem *((*child)[]);	// ptr to an array of tagitem pointers
//} TAGITEM;

// Need forward declarations of everything for linked lists to work

extern TAGITEM tag1, tag2, tag3, tag4, tag5, tag6, tag7, tag8;
extern TAGITEM tag51, tag52, tag53, tag61, tag62, tag63, tag631, tag632, tag633;
extern TAGITEM *(root_list[]), *(tag5_list[]), *(tag6_list[]), *(tag63_list[]);

// Three special TAGITEMs that lead us into the OPC batch namespace
extern TAGITEM tagOPCBBatchModel, tagOPCBPhysicalModel, tagOPCBBatchIDList;

// The Root:
//
TAGITEM root_tag = { L"", NULL, &root_list};
TAGITEM *(root_list[]) = 
{
	&tag1,
	&tag2,
	&tag3,
	&tag4,
	&tag5,
	&tag6,
	&tag7,
	&tag8,
  &tagOPCBBatchModel,
  &tagOPCBPhysicalModel,
  &tagOPCBBatchIDList,
	NULL
};

TAGITEM tag1= { L"TAG1", &root_tag, NULL};
TAGITEM tag2= { L"TAG2", &root_tag, NULL};
TAGITEM tag3= { L"TAG3", &root_tag, NULL};
TAGITEM tag4= { L"TAG4", &root_tag, NULL};
TAGITEM tag5= { L"BCH5", &root_tag, &tag5_list};
TAGITEM tag6= { L"BCH6", &root_tag, &tag6_list};
TAGITEM tag7= { L"TAG7", &root_tag, NULL};
TAGITEM tag8= { L"TAG8", &root_tag, NULL};

// The following batch branches have their 'child' set to NULL.
// In fact, in this sample server there ARE children under
// here but they are not TAGITEMs, they are special batch
// objects (class OPCBObj).
// In the code for the IBrowseServerAddressSpace interface,
// there is special handling to allow browsing down into the
// batch namespace and the OPCBObj objects that live therein.
// This is not very elegant but I want to have the minimum
// changes to the existing code, while supporting the Batch
// namespace which requires more than TAGITEM allows.
// (and of course a real server that supported both batch
// items from a batch execution system and other items from 
// a some other system would also have to deal with similar
// ugliness.)  All of this is an artefact of this sample 
// server and is in no way dictated by the OPC Batch spec,
TAGITEM tagOPCBBatchModel    = { L"OPCBBatchModel",    &root_tag, NULL };
TAGITEM tagOPCBPhysicalModel = { L"OPCBPhysicalModel", &root_tag, NULL };
TAGITEM tagOPCBBatchIDList   = { L"OPCBBatchIDList",   &root_tag, NULL };

TAGITEM *tag5_list[] = 
{
	&tag51,
	&tag52,
	&tag53,
	NULL
};
TAGITEM tag51= { L"TAG51", &tag5, NULL};
TAGITEM tag52= { L"TAG52", &tag5, NULL};
TAGITEM tag53= { L"TAG53", &tag5, NULL};

TAGITEM *tag6_list[] = 
{
	&tag61,
	&tag62,
	&tag63,
	NULL
};
TAGITEM tag61= { L"TAG61", &tag6, NULL};
TAGITEM tag62= { L"TAG62", &tag6, NULL};
TAGITEM tag63= { L"BCH63", &tag6, &tag63_list};

TAGITEM *tag63_list[] = 
{
	&tag631,
	&tag632,
	&tag633,
	NULL
};
TAGITEM tag631= { L"TAG631", &tag63, NULL};
TAGITEM tag632= { L"TAG632", &tag63, NULL};
TAGITEM tag633= { L"TAG633", &tag63, NULL};
