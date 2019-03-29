// REGCAT.CPP
//
// (c) Copyright 1998 The OPC Foundation
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
//  This file contains the implementation of 
//  code which registers component catagores for OPC Servers.
//  And also registers the SAMPLE OPC server.
//
//  NOTES:
//  1. We create the catagories plus register the server in RegCat
//  2. We only unregister the server in UnRegCat (we do not remove the catagories)
//  3. We register this server for DataAccess V1 and V2
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	2.00  04/08/98 ACC   


#include <conio.h>
#include <stdio.h>
#include <windows.h>
#include <COMCAT.h>
#include "opcxxx.h"

EXTERN_C const GUID CATID_OPCDAServer10;
EXTERN_C const GUID CATID_OPCDAServer20;

// Create the Component Catagories
void CreateOPCDACat(void)
{

	HRESULT hr, hr2;
	ICatRegister *pCat;

	// Get the Catagories Interface
	//
	hr = CoCreateInstance (CLSID_StdComponentCategoriesMgr, NULL,
		CLSCTX_INPROC_SERVER, IID_ICatRegister,
		(void **)&pCat);
	if (SUCCEEDED(hr)) 
	{
		CATEGORYINFO Catlist[2];
		// Build the catagory info
		//
		Catlist[0].catid = CATID_OPCDAServer10;
		Catlist[0].lcid = 0x0409;
		wcscpy(Catlist[0].szDescription, L"OPC Data Access Servers Version 1.0");

		Catlist[1].catid = CATID_OPCDAServer20;
		Catlist[1].lcid = 0x0409;
		wcscpy(Catlist[1].szDescription, L"OPC Data Access Servers Version 2.0");

		// And register the Catagory
		//
		hr2 = pCat->RegisterCategories(
			2, Catlist);

		if (FAILED(hr2)) MessageBox(NULL, "CreateOPCDACat:RegisterCatagories Failed", NULL, MB_OK);

		pCat->Release();

    } else MessageBox(NULL, "CreateOPCDACat:CoCreateInstance Failed", NULL, MB_OK);
}


void RegOPCDACat(GUID clsid)
{

	HRESULT hr, hr2;
	ICatRegister *pCat;

	// Get the Catagories Interface
	//
	hr = CoCreateInstance (CLSID_StdComponentCategoriesMgr, NULL,
		CLSCTX_INPROC_SERVER, IID_ICatRegister,
		(void **)&pCat);
	if (SUCCEEDED(hr)) 
	{
		// Register the categories as being "implemented" by 
		// the passed clsid. 
		//
		CATID rgcatid[2] ; 
		rgcatid[0] = CATID_OPCDAServer10; 
		rgcatid[1] = CATID_OPCDAServer20; 
		hr2 = pCat->RegisterClassImplCategories(clsid, 2, rgcatid); 

		if(FAILED(hr2))MessageBox(NULL, "RegOPCDACat:RegisterClassImplCatagories Failed", NULL, MB_OK);

		pCat->Release();

    } else MessageBox(NULL, "RegOPCDACat:CoCreateInstance Failed", NULL, MB_OK);
}


void UnRegOPCDACat(GUID clsid)
{

	HRESULT hr, hr2;
	ICatRegister *pCat;

	// Get the Catagories Interface
	//
	hr = CoCreateInstance (CLSID_StdComponentCategoriesMgr, NULL,
		CLSCTX_INPROC_SERVER, IID_ICatRegister,
		(void **)&pCat);
	if (SUCCEEDED(hr)) 
	{
		// UnRegister the categories as being "implemented" by 
		// the passed clsid. 
		//
		CATID rgcatid[2] ; 
		rgcatid[0] = CATID_OPCDAServer10; 
		rgcatid[1] = CATID_OPCDAServer20; 
		hr2 = pCat->UnRegisterClassImplCategories(clsid, 2, rgcatid); 

		if(FAILED(hr2))MessageBox(NULL, "UnRegOPCDACat:RegisterClassImplCatagories Failed", NULL, MB_OK);


		pCat->Release();

    } else MessageBox(NULL, "UnRegOPCDACat:CoCreateInstance Failed", NULL, MB_OK);
}