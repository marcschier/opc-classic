// opc_prm.cpp - parameters test code
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
// 2.00  09/01/97 acc   
//       01/14/98 acc   update per final spec
//

#include <stdio.h>
#include <conio.h>
#include "opcda.h"
#include "wcsutil.h"

extern	IMalloc *pIMalloc;
extern void	DumpVariant(VARIANT *v);
void TryParams(IOPCItemProperties * pPRM);



//---------------------------------------------------------
// TryParms
// 
void TryParams(IOPCItemProperties * pPRM)
{
	HRESULT r1;
	DWORD n;
	LPWSTR *descs = 0;
	LPWSTR *newids = 0;
	VARTYPE *vts = 0;
	DWORD *ids = 0;
	VARIANT *v = 0;
	HRESULT *err1 = 0;
	HRESULT *err2 = 0;
	LPWSTR Tag;
	char buffer[82] = {80};
	char *pptr;
	unsigned int i;



	// read a tag name from user
	//
	printf("Enter Enter an ItemID\n");
	pptr = gets(buffer);
	Tag = WSTRFromSBCS(pptr, 0);


	// Test QueryAvailableParameters
	//
	r1 = pPRM->QueryAvailableProperties(Tag, &n, &ids, &descs, &vts);

	if (FAILED(r1))
	{
		printf("Error from QueryAvailableParameters:%lx\n", r1);
		return;
	}
	printf("Query returns: %ld parameters:\n", n);
	printf(" ID, DESC, TYPE\n");
	for(i=0; i<n; i++)
	{
		printf("  %ld, %ls, %4lx\n", ids[i], descs[i], vts[i]);
	}
	getch();



	// Test GetItemParameters
	//
	r1 = pPRM->GetItemProperties(Tag, n, ids, &v, &err1);
	if (FAILED(r1))
	{
		printf("Error from GetItemParameters:%8lx\n", r1);
	} else
	{
		printf("Get returns:\n");
		printf(" ID, ERR, VALUE\n");
		for(i=0; i<n; i++)
		{
			if(err1[i] == S_OK)
			{
				printf("  %ld, %8lx ", ids[i], err1[i]);
				DumpVariant(&v[i]);
				printf("\n");
			}
			else printf("  %ld, %8lx\n", ids[i], err1[i]);
		}
	}
	getch();




	// Test LookupItemIDs
	//
	r1 = pPRM->LookupItemIDs(Tag, n, ids, &newids, &err2);
	if (FAILED(r1))
	{
		printf("Error from LookupItemIDs:%lx\n", r1);
	} else
	{
		printf("Lookup returns:\n");
		printf(" ID, NEWID, ERR\n");
		for(i=0; i<n; i++)
		{
			if(err2[i] == S_OK) printf("  %ld, %ls, %8lx\n", ids[i], newids[i], err2[i]);
			else printf("  %ld, %8lx\n", ids[i], err2[i]);
		}
	}
	getch();




	// Free all of the memory - Careful! this is trickey!
	//
	for(i=0; i<n; i++)
	{
		if(descs) if(descs[i]) pIMalloc->Free(descs[i]);
		if(newids) if(newids[i]) pIMalloc->Free(newids[i]);
		if(v) VariantClear(&v[i]);
	}
	if(descs) pIMalloc->Free(descs);
	if(newids) pIMalloc->Free(newids);
	if(vts) pIMalloc->Free(vts);
	if(ids) pIMalloc->Free(ids);
	if(v) pIMalloc->Free(v);
	if(err1) pIMalloc->Free(err1);
	if(err2) pIMalloc->Free(err2);

	WSTRFree(Tag, 0);
}
