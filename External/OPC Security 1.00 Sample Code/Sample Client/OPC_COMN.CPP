// opc_comn.cpp - IOPCCommon test code
//
//
// (c) Copyright 1998 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Specifications and may be used
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
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//       01/14/98 acc   update per final spec
//

#include <stdio.h>
#include <conio.h>
#include "opccomn.h"
#include "opcda.h"
#include "wcsutil.h"

extern	IMalloc *pIMalloc;


//---------------------------------------------------------
// Try IOPCCommon
// 
void TryCOMN(IOPCCommon* pCOMN)
{
	HRESULT r1;
	DWORD dwCount, *pList;
	LCID	lcid;
	LPWSTR s = 0;


	// At this time this is a pretty simple Go/NoGo test...

	r1 = pCOMN->SetLocaleID(0);
	if (FAILED(r1)) printf("Error from SetLocaleID(%lx)\n", r1);
	else printf("SetLocaleID - OK\n");

	r1 = pCOMN->GetLocaleID(&lcid);
	if (FAILED(r1)) printf("Error from GetLocaleID(%lx)\n", r1);
	else printf("GetLocaleID - OK\n");

	r1 = pCOMN->QueryAvailableLocaleIDs(&dwCount, &pList);
	if (FAILED(r1)) printf("Error from QueryAvailableLocaleIDs(%lx)\n", r1);
	else
	{
		printf("QueryAvailableLocaleIDs returned %ld IDs\n", s);
		if(dwCount) pIMalloc->Free(pList);
	}

	r1 = pCOMN->GetErrorString(0, &s);
	if (FAILED(r1)) printf("Error from GetErrorString(%lx)\n", r1);
	else
	{
		printf("GetErrorString returned \"%ls\"\n", s);
		pIMalloc->Free(s);
	}
	
	r1 = pCOMN->SetClientName(L"OPCTestClient");
	if (FAILED(r1)) printf("Error from SetClientName(%lx)\n", r1);
	else printf("SetClientName - OK\n");
}
