// Get_Cats.cpp - browse available servers
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
//  2.0   04/09/98 acc   
//

#include <stdio.h>
#include <conio.h>
#include "opcda.h"
#include "OLECTL.h"
#include "COMCAT.h"

extern	IMalloc *pIMalloc;

// zzz temp
//const CATID CATID_OPCDAServer20 = {0x31415926,0x011e,0x11d0,{0x96,0x75,0x00,0x20,0xaf,0xd8,0xad,0xb3}};


void ShowDAList(CATID cid)
{
	HRESULT hr, hr2;
	ICatInformation *pCat;
	IEnumCLSID * pEnum;
	CATID Implist[1];
	
	Implist[0] = cid;

	hr = CoCreateInstance (CLSID_StdComponentCategoriesMgr, NULL,
		CLSCTX_INPROC_SERVER, IID_ICatInformation,
		(void **)&pCat);
	if (SUCCEEDED(hr)) 
	{
		hr2 = pCat->EnumClassesOfCategories(
			1, Implist,
			0, NULL,
			&pEnum);

		if (SUCCEEDED(hr2)) 
		{
			GUID glist;
			ULONG actual;

			// Print the results of the ENum
			//	
			while((hr2 = pEnum->Next(1, &glist, &actual)) == S_OK)
			{
				// If a GUID came back then get the associated ProgID
				//
				WCHAR *result;
				hr2 = ProgIDFromCLSID(glist, &result);
				if(FAILED(hr2)) printf("ProgIDFromCLSID failed\n");
				else 
				{
					printf("\t%ls\n", result);
					pIMalloc->Free(result);
				}
			}

			pEnum->Release();
		}
		pCat->Release();
	} else
	{
		printf("CoCreate Failed\n");
	}
	printf("\n");
}

