// BATCHSUMMUtil.cpp
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
//  Will Irwin, Fisher-Rosemount Systems Inc., based on earlier code by
//  Al Chisholm, Intellution Inc.
//
// CONTENTS:
//
//  This file contains some OPCBATCHSUMMARY utility functions
//  for the OPC Sample server.
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	0.00  12/31/96 ACC
//  1.00  10/15/99 WGI   Copied ACC's ITEMUTIL.cpp and changed names
//
// Notes:
//


#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"


///////////////////////////////////////
// Clone an OPCBATCHSUMMARY
//
void BSClone(OPCBATCHSUMMARY *newBS, const OPCBATCHSUMMARY * oldBS, IMalloc *pmem)
{

  newBS->szID = WSTRClone( oldBS->szID, pmem);
  newBS->szDescription = WSTRClone( oldBS->szDescription, pmem);
  newBS->szOPCItemID = WSTRClone (oldBS->szOPCItemID, pmem);
  newBS->szMasterRecipeID = WSTRClone (oldBS->szMasterRecipeID, pmem);
  newBS->fBatchSize = oldBS->fBatchSize;
  newBS->szEU = WSTRClone (oldBS->szEU, pmem);
  newBS->szExecutionState = WSTRClone (oldBS->szExecutionState, pmem);
  newBS->szExecutionMode = WSTRClone (oldBS->szExecutionMode, pmem);
  newBS->ftActualStartTime = oldBS->ftActualStartTime;
  newBS->ftActualEndTime = oldBS->ftActualEndTime;
}


///////////////////////////////////////
// Free the contents of an OPCBATCHSUMMARY
// (Caller must Free BS itself)
//
void BSFree(OPCBATCHSUMMARY *pBS, IMalloc* pmem)
{
	if(pBS == NULL) return;

	WSTRFree(pBS->szID, pmem);
	WSTRFree(pBS->szDescription, pmem);
	WSTRFree(pBS->szOPCItemID, pmem);
	WSTRFree(pBS->szMasterRecipeID, pmem);
	WSTRFree(pBS->szEU, pmem);
	WSTRFree(pBS->szExecutionState, pmem);
	WSTRFree(pBS->szExecutionMode, pmem);
}
