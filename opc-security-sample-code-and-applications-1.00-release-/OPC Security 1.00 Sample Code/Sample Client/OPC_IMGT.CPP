// opc_imgt.cpp - OPCItemMgt test code
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
// 2.00   01/14/98 acc   
//

#include <stdio.h>
#include <conio.h>
#include "opcda.h"
#include "wcsutil.h"

extern	IMalloc *pIMalloc;

extern	WCHAR	*ItemIDs[2];
extern	WCHAR	*AccessPaths[2];
extern	VARTYPE	dtype[2];
extern	OPCHANDLE g_sh[2];		// Returned Server Handles for the Items
extern	BOOL add_done;

extern	void TryEnumItemAttributes(IOPCItemMgt * pOPC);

void TryAddItem(IOPCItemMgt * pIM);
void TryValidateItem(IOPCItemMgt * pIM);
void TryRemoveItem(IOPCItemMgt * pIM);
void TrySetActiveState(IOPCItemMgt * pIM);
void TrySetClientHandles(IOPCItemMgt * pIM);
void TrySetDataTypes(IOPCItemMgt * pIM);

//---------------------------------------------------------


//---------------------------------------------------------
// TryItemMgt Interface
// Use the OPCGroup to try some item level functions
void TryIM(IOPCItemMgt * pIM)
{
	int loop;
	char buffer[40];

	loop = 1;
	while(loop)
	{

		printf("\n\nTest OPCItemMgt Methods...\n");
		printf("0= AddItems...\n");
		printf("1= ValidateItems...\n");
		printf("2= RemoveItems...\n");
		printf("3= SetActiveState...\n");
		printf("4= SetClientHandles...\n");
		printf("5= SetDataTypes...\n");
		printf("6= CreateEnumerator...\n");
		printf("x= Exit...\n\n");

		switch(*gets(buffer))
		{
		case '0':
			TryAddItem(pIM);
			break;
		case '1':
			TryValidateItem(pIM);
			break;
		case '2':
			TryRemoveItem(pIM);
			break;
		case '3':
			TrySetActiveState(pIM);
			break;
		case '4':
			TrySetClientHandles(pIM);
			break;
		case '5':
			TrySetDataTypes(pIM);
			break;
		case '6':
			TryEnumItemAttributes(pIM);
			break;
		case 'x':
			loop = 0;
			break;
		}
	}
	printf("IOPCServer test complete\n\n");
}


void GetItemIDs(void)
{
	char *pptr, buffer[80];
	printf("Enter two ItemIDs and two AccessPaths\n");

	printf("ItemID(0)\n");
	pptr = gets(buffer);
	ItemIDs[0] = WSTRFromSBCS(pptr, 0);

	printf("ItemID(1)\n");
	pptr = gets(buffer);
	ItemIDs[1] = WSTRFromSBCS(pptr, 0);
		
	printf("AccessPath(0)\n");
	pptr = gets(buffer);
	AccessPaths[0] = WSTRFromSBCS(pptr, 0);

	printf("AccessPath(1)\n");
	pptr = gets(buffer);
	AccessPaths[1] = WSTRFromSBCS(pptr, 0);

try_again:
	printf("Enter Requested Datatype (F=VT_R4, R=VT_R8, I=VT_I2, L=VT_I4, S=VT_BSTR\n");
	pptr = gets(buffer);
	switch(*pptr)
	{
	case 'f':
	case 'F':
		dtype[0] = VT_R4;
		dtype[1] = VT_R4;
		break;
	case 'r':
	case 'R':
		dtype[0] = VT_R8;
		dtype[1] = VT_R8;
		break;
	case 'i':
	case 'I':
		dtype[0] = VT_I2;
		dtype[1] = VT_I2;
		break;
	case 'l':
	case 'L':
		dtype[0] = VT_I4;
		dtype[1] = VT_I4;
		break;
	case 's':
	case 'S':
		dtype[0] = VT_BSTR;
		dtype[1] = VT_BSTR;
		break;
	default:
		goto try_again;
	}
}


void TryAddItem(IOPCItemMgt * pIM)
{
	HRESULT	r1;
	OPCITEMDEF id[2];
	HRESULT *ih;
	OPCITEMRESULT *ir;


	if(add_done)
	{
		printf("add already done\n");
		return;
	}

	GetItemIDs();


	// Define two items
	//
	id[0].szItemID = ItemIDs[0];
	id[0].szAccessPath = AccessPaths[0];
	id[0].bActive = TRUE;
	id[0].hClient = 100;
	id[0].dwBlobSize = 0;
	id[0].pBlob = NULL;
	id[0].vtRequestedDataType = dtype[0];

	id[1].szItemID = ItemIDs[1];
	id[1].szAccessPath = AccessPaths[1];
	id[1].bActive = TRUE;
	id[1].hClient = 101;
	id[1].dwBlobSize = 0;
	id[1].pBlob = NULL;
	id[1].vtRequestedDataType = dtype[1];

	// Add then items and check the hresults
	//
	r1 = pIM->AddItems(2, id, &ir, &ih);
	if (FAILED(r1))
	{
		printf("Error from AddItems(%lx)\n", r1);
		return;
	} 
	else
	{
		add_done = 1;

		// Check the results of each item
		int i;

		for(i=0; i<2; i++)
		{
			if (FAILED(ih[i]))
			{
				printf("Error from AddItems/hr[%d](%lx)\n", i, ih[i]);
				// zzz more robust error here would be nice 
				// (to prevent use of bad items later)
			} else
 			{
				printf("Item[%d] serverhandle = %d, Canonical=%d, Access=%d, BlobSize=%d\n", i, 
					ir[i].hServer,
					(DWORD)ir[i].vtCanonicalDataType,
					ir[i].dwAccessRights,
					ir[i].dwBlobSize
					);

				// Store handle into global
				g_sh[i] = ir[i].hServer;

				//zzz later - show VARTYPE as a "VT_xx" string

				// also Important to free blob if it is returned!
				//
				if(ir[i].pBlob) pIMalloc->Free(ir[i].pBlob);
			}
		}

		// Free the returned HRESULTs and ITEMRESULTS
		//
		pIMalloc->Free(ir);
		pIMalloc->Free(ih);
	}
	printf("AddItems Test Complete...\n");
}

void TryValidateItem(IOPCItemMgt * pIM)
{
	HRESULT	r1;
	OPCITEMDEF id[2];
	HRESULT *ih;
	OPCITEMRESULT *ir;

	// Ask user for info
	//
	GetItemIDs();

	// NOTE we use a local ITEMRESULT here so as not to clobber the
	// one that ADD/REMOVE etc use!
	//

	// Define two items
	//
	id[0].szItemID = ItemIDs[0];
	id[0].szAccessPath = AccessPaths[0];
	id[0].bActive = TRUE;
	id[0].hClient = 100;
	id[0].dwBlobSize = 0;
	id[0].pBlob = NULL;
	id[0].vtRequestedDataType = dtype[0];	//VT_R4;

	id[1].szItemID = ItemIDs[1];
	id[1].szAccessPath = AccessPaths[1];
	id[1].bActive = TRUE;
	id[1].hClient = 101;
	id[1].dwBlobSize = 0;
	id[1].pBlob = NULL;
	id[1].vtRequestedDataType = dtype[1];

	// Validate items and check the hresults
	//
	r1 = pIM->ValidateItems(2, id, 1, &ir, &ih);
	if (FAILED(r1))
	{
		printf("Error from ValidateItems(%lx)\n", r1);
		return;
	} 
	else
	{
		// Check the results of each item
		int i;

		for(i=0; i<2; i++)
		{
			if (FAILED(ih[i]))
			{
				printf("Error from AddItems/hr[%d](%lx)\n", i, ih[i]);
			} else
 			{
				printf("Item[%d] serverhandle = %d, Canonical=%d, Access=%d, BlobSize=%d\n", i, 
					ir[i].hServer,
					(DWORD)ir[i].vtCanonicalDataType,
					ir[i].dwAccessRights,
					ir[i].dwBlobSize
					);

				// also Important to free blob if it is returned!
				//
				if(ir[i].pBlob) pIMalloc->Free(ir[i].pBlob);
			}
		}

		// Free the returned HRESULTs and OPCITEMRESULTS
		//
		pIMalloc->Free(ih);
		pIMalloc->Free(ir);
	}
	printf("ValidateItems Test Complete...\n");
}

void TryRemoveItem(IOPCItemMgt * pIM)
{
	HRESULT	r1;
	HRESULT *ih;

	if(!add_done)
	{
		printf("Must do Add first\n");
		return;
	}

	//

	r1 = pIM->RemoveItems(2, g_sh, &ih);
	if (FAILED(r1))
	{
		printf("Error from RemoveItems(%lx)\n", r1);
		return;
	} 
	else
	{
		// Check the results of each item
		int i;

		for(i=0; i<2; i++)
		{
			if (FAILED(ih[i]))
			{
				printf("Error from RemoveItems/hr[%d] = (%lx)\n", i, ih[i]);
			} 
		}

		// Free the returned HRESULTs
		//
		pIMalloc->Free(ih);		//acc001
	}

	add_done = 0;
	printf("RemoveItems Test Complete...\n");
}

void TrySetActiveState(IOPCItemMgt * pIM)
{
	HRESULT	r1;
	HRESULT *ih;

	// setActive and check the hresults
	// For now just exercise the call...
	// zzz later ask user for Active or Inactive
	//
	r1 = pIM->SetActiveState(2, g_sh, 1, &ih);
	if (FAILED(r1))
	{
		printf("Error from SetActiveState(%lx)\n", r1);
		return;
	} 
	else
	{
		// Check the results of each item
		int i;

		for(i=0; i<2; i++)
		{
			if (FAILED(ih[i]))
			{
				printf("Error from SetActive/hr[%d] = (%lx)\n", i, ih[i]);
			} 
		}
		// Free the returned HRESULTs
		//
		pIMalloc->Free(ih);
	}
	printf("SetActiveState Test Complete...\n");
}

void TrySetClientHandles(IOPCItemMgt * pIM)
{
	HRESULT	r1;
	HRESULT *ih;
	OPCHANDLE ch[2] = {123, 456};

	// Do function and check the hresults
	// For now just exercise the call...
	// zzz later ask user for handles
	//
	r1 = pIM->SetClientHandles(2, g_sh, ch, &ih);
	if (FAILED(r1))
	{
		printf("Error from SetClientHandles(%lx)\n", r1);
		return;
	} 
	else
	{
		// Check the results of each item
		int i;

		for(i=0; i<2; i++)
		{
			if (FAILED(ih[i]))
			{
				printf("Error from SetClientHandles/hr[%d] = (%lx)\n", i, ih[i]);
			} 
		}
		// Free the returned HRESULTs
		//
		pIMalloc->Free(ih);
	}
	printf("SetClientHandles Test Complete...\n");
}

void TrySetDataTypes(IOPCItemMgt * pIM)
{
	HRESULT	r1;
	HRESULT *ih;

	// Do function and check the hresults
	// For now just exercise the call...
	// zzz later ask user for datatypes
	//
	r1 = pIM->SetDatatypes(2, g_sh, dtype, &ih); //just reset to same value for now
	if (FAILED(r1))
	{
		printf("Error from SetDatatypes(%lx)\n", r1);
		return;
	} 
	else
	{
		// Check the results of each item
		int i;

		for(i=0; i<2; i++)
		{
			if (FAILED(ih[i]))
			{
				printf("Error from SetDataTypes/hr[%d] = (%lx)\n", i, ih[i]);
			} 
		}
		// Free the returned HRESULTs
		//
		pIMalloc->Free(ih);
	}
	printf("SetDataTypes Test Complete...\n");
}


