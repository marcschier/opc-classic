// opc_bas.cpp - browse address space test code
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
// 1.0a  08/01/97 acc   
//       01/14/98 acc   update per final spec
//

#include <stdio.h>
#include <conio.h>
#include "opcda.h"
#include "OLECTL.h"

extern	IMalloc *pIMalloc;
void TryBrowse(IOPCBrowseServerAddressSpace * pBAS);



//---------------------------------------------------------
// TryBrowse
// zzz only works for FLAT, does not test ChangeBrowsePosition or AccessPaths
// 
void TryBrowse(IOPCBrowseServerAddressSpace * pBAS)
{
	HRESULT r1;
	OPCNAMESPACETYPE org;
	IEnumString * pEnum;

	r1 = pBAS->QueryOrganization(&org);

	if (FAILED(r1))
	{
		printf("Error from QueryOrganization:%lx\n", r1);
		return;
	}
	printf("QueryOrganization returns: %d\n", org);


	r1 = pBAS->BrowseOPCItemIDs(OPC_FLAT, L"*", VT_EMPTY, 
		OPC_READABLE|OPC_WRITEABLE, &pEnum);
	if (FAILED(r1))
	{
		printf("Error from BrowseItemIDs:%lx\n", r1);
		return;
	}

	HRESULT r2 = S_OK;
	LPOLESTR pStr;
	LPOLESTR pStr2;
	ULONG actual;

	printf("Got an IEnumString...\n");

	// It would be nice to test with more than '1' later
	//
	while((r2 = pEnum->Next(1, &pStr, &actual)) == S_OK)
	{
		HRESULT r3;

		printf("EnumString returns: %ls\n", pStr);
		r3 = pBAS->GetItemID(pStr, &pStr2);
		printf("GetITEMID returns: %ls\n", pStr2);

		// Free the string(s) that Next Returned
		//
		pIMalloc->Free(pStr);
		if (pStr2) pIMalloc->Free(pStr2);
	}
	if (FAILED(r2))
	{
		printf("Error from Next(%lx)\n", r2);
	}
		
	// Release the interface
	//
	pEnum->Release();

}

