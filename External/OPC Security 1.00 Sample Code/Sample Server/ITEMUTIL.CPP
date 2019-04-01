// ITEMUtil.cpp
//
// (c) Copyright 1997 The OPC Foundation
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
//  This file contains some OPCITEMATTRIBUTES utility functions
//  for the OPC Sample server.
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	0.00  12/31/96 ACC
//
//
// Notes:
// See also XXXItem::GetIA() in item.cpp
//


#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"

///////////////////////////////////////
// Clone an OPCITEMATTRIBUTES 
// This is similar to IAGet() in item.cpp
//
void IAClone(OPCITEMATTRIBUTES *newIA, const OPCITEMATTRIBUTES * oldIA, IMalloc *pmem)
{
	// Note Blob and EUInfo not supported at present

	// the easy stuff...
	//
	newIA->bActive = oldIA->bActive;
	newIA->hServer = oldIA->hServer;
	newIA->hClient = oldIA->hClient;
	newIA->dwAccessRights = oldIA->dwAccessRights;
	newIA->dwBlobSize = 0;	// not supported
	newIA->pBlob = NULL;
	newIA->vtRequestedDataType= oldIA->vtRequestedDataType;
	newIA->vtCanonicalDataType= oldIA->vtCanonicalDataType;
	newIA->dwEUType = OPC_NOENUM;		// not supported
	newIA->vEUInfo.vt = VT_EMPTY;

	// strings...
	//
	newIA->szAccessPath = WSTRClone( oldIA->szAccessPath, pmem);
	newIA->szItemID = WSTRClone(oldIA->szItemID, pmem);

}

///////////////////////////////////////
// Free the contents of an OPCITEMATTRIBUTES 
// (Caller must Free IA itself)
//
void IAFree(OPCITEMATTRIBUTES *pIA, IMalloc* pmem)
{
	if(pIA == NULL) return;

	// Note Blob and EUInfo not supported at present
	WSTRFree(pIA->szAccessPath, pmem);
	WSTRFree(pIA->szItemID, pmem);
}



